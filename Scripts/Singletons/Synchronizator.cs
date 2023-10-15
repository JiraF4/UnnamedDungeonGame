
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class Synchronizator : Node
{
    public static Synchronizator Instance;
    protected readonly Godot.Collections.Dictionary<ulong, SynchronizationController> Controllers = new();
    protected readonly Godot.Collections.Dictionary<NodePath, SynchronizationController> InitialPathControllers = new();
    
    private ulong _lastSendPackedTime = 0;
    protected readonly Godot.Collections.Dictionary<int, ulong> LastPackedTime = new();
    protected readonly Godot.Collections.Dictionary<int, float> PackedDelays = new();
    private ulong _minimumSendPackedTime = 33;
    private int _forceSyncNumber = 0;
    
    private Dictionary _lastSentPacked;
    
    public override void _Ready()
    {
        Instance = this;
    }

    public void AddController(SynchronizationController controller)
    {
        Controllers.Add(controller.SUID, controller);
        InitialPathControllers.Add(controller.InitialPath, controller);
    }
    
    public void GetControllerSUID(NodePath initialPath)
    {
        RpcId(1, nameof(GetControllerSUIDServer), initialPath);
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = false)]
    protected void GetControllerSUIDServer(NodePath initialPath)
    {
        try
        {
            var controller = InitialPathControllers[initialPath];
            RpcId(Multiplayer.GetRemoteSenderId(), nameof(GetControllerSUIDClient), initialPath, controller.SUID);
        }
        catch (KeyNotFoundException e)
        {
            GD.Print("Lost controller: " + initialPath);
            throw;
        }
        
    }
    
    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = false)]
    protected void GetControllerSUIDClient(NodePath initialPath, ulong suid)
    {
        var controller = GetTree().Root.GetNode<SynchronizationController>(initialPath);
        controller.SetControllerSUID(suid);
    }

    public float GetDelay(int peerId)
    {
        return PackedDelays[peerId];
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!Network.IsConnected && !Network.IsServer) return;
        foreach (var (key, value) in PackedDelays)
        {
            DebugInfo.AddLine("Peer" + key + "Delay: " + value);
        }
        if (Network.IsServer) DebugInfo.AddLine("LastSentPacked: " + _lastSentPacked?.ToString().Replace(", \"", ",\n\"").Left(3000));
        
        if (Time.GetTicksMsec() - _lastSendPackedTime < _minimumSendPackedTime) return;
        _lastSendPackedTime = Time.GetTicksMsec();
        
        var syncData = new Dictionary();
        CollectAll(syncData);

        var syncPacket = new Dictionary {["SendTime"] = Time.GetTicksMsec(), ["SyncData"] = syncData};
        _lastSentPacked = syncPacket;

        Rpc(nameof(ApplyAll), syncPacket);
    }
    
    public void CollectAll(Dictionary syncData)
    {
        if (_forceSyncNumber >= Controllers.Count) _forceSyncNumber = 0;

        var i = 0;
        foreach (var (key, controller) in Controllers)
        {
            var controllerSyncData = i == _forceSyncNumber ? controller.GetSyncData() : controller.GetClearSyncData();
            if (controllerSyncData.Count > 0)
                syncData[key] = controllerSyncData;
            i++;
        }

        _forceSyncNumber++;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable, CallLocal = false)]
    public void ApplyAll(Dictionary syncPacket)
    {
        var peerId = Multiplayer.GetRemoteSenderId();
        var syncTime = (ulong) syncPacket["SendTime"];
        if (!LastPackedTime.ContainsKey(peerId)) LastPackedTime[peerId] = 0;
        if (LastPackedTime[peerId] > syncTime) return;
        PackedDelays[peerId] = (syncTime - LastPackedTime[peerId]) / 1000.0f;
        LastPackedTime[peerId] = syncTime;
        
        var syncData = (Dictionary) syncPacket["SyncData"];
        foreach (var (key, controllerSyncData) in syncData)
        {
            if (Controllers.ContainsKey((ulong) key))
                Controllers[(ulong) key].ApplySyncDataIfAuthority((Dictionary) controllerSyncData);
        }
    }
}