using System.Diagnostics;
using Godot;
using Godot.Collections;

public partial class SynchronizationController : Node
{
    private Dictionary _lastSendSyncData = new();
    private Dictionary _lastSendSyncDataUnique = new();
    
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

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    void SyncRemote(Dictionary syncData)
    {
        ApplySyncData(syncData);
    }

    
    protected virtual void CollectSyncData(Dictionary syncData)
    {
        
    }
    
    protected virtual void ApplySyncData(Dictionary syncData)
    {
        
    }

}