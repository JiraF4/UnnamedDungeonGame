﻿using Godot;
using Godot.Collections;

public partial class SynchronizationController : Node
{
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
        var syncData = new Dictionary();
        CollectSyncData(syncData);
        Rpc(nameof(SyncRemote), syncData);
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