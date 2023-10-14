using System.Diagnostics;
using Godot;
using Godot.Collections;

public partial class SynchronizationController : Node
{
    private Dictionary _lastSendSyncData = new();
    private Dictionary _lastSendSyncDataUnique = new();
    private ulong _lastPackedTime = 0;
    private ulong _lastSendPackedTime = 0;
    private ulong _minimumSendPackedTime = 25; 
    
    public override void _Ready()
    {
        base._Ready();
    }
    
    public override void _Process(double delta)
    {
        if (GetMultiplayerAuthority() == Multiplayer.GetUniqueId()) Sync();
        base._Process(delta);
    }

    void Sync()
    {
        //DebugInfo.AddLine(_lastSendSyncDataUnique?.ToString().Replace(", \"", ",\n\""));
        if (Time.GetTicksMsec() - _lastSendPackedTime < _minimumSendPackedTime) return;
        _lastSendPackedTime = Time.GetTicksMsec();
        
        var syncData = new Dictionary();
        CollectSyncData(syncData);
        
        var syncDataUnique = new Dictionary();
        foreach (var (key, value) in syncData)
        {
            if (!_lastSendSyncData.ContainsKey(key) || !value.Equals(_lastSendSyncData[key]))
                syncDataUnique[key] = value;
        }
        _lastSendSyncData = syncData;
        _lastSendSyncDataUnique = syncDataUnique;
        
        Rpc(nameof(SyncRemote), syncDataUnique);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    void SyncRemote(Dictionary syncData)
    {
        var syncTime = (ulong) syncData["SyncTime"];
        if (_lastPackedTime > syncTime) return;
        syncData["SyncDelay"] = (syncTime - _lastPackedTime) / 1000.0f;
        _lastPackedTime = syncTime;
        ApplySyncData(syncData);
    }

    
    protected virtual void CollectSyncData(Dictionary syncData)
    {
        syncData["SyncTime"] = Time.GetTicksMsec();
    }
    
    protected virtual void ApplySyncData(Dictionary syncData)
    {
        
    }

}