﻿using Godot;
using Godot.Collections;

public partial class SynchronizationController : Node
{
    private Dictionary _lastSendSyncData = new();
    private Dictionary _lastSendSyncDataUnique = new();
    public NodePath InitialPath { get; private set; }
    public ulong SUID { get; protected set; }
    public SynchronizedRigidBody Body { get; private set; }
    
    
    public override void _Ready()
    {
        InitialPath = GetPath();
        
        if (Network.IsServer)
        {
            SUID = Network.GetNextSUID();
            Synchronizator.Instance.AddController(this);
        }
        else
        {
            Synchronizator.Instance.GetControllerSUID(InitialPath);
        }

        Body = GetParent<SynchronizedRigidBody>();
        base._Ready();
    }

    public void SetControllerSUID(ulong newSUID)
    {
        SUID = newSUID;
        Synchronizator.Instance.AddController(this);
    }
    
    public Dictionary GetSyncData()
    {
        var syncData = new Dictionary();
        if (Multiplayer.GetUniqueId() != GetMultiplayerAuthority()) return syncData;
        if (GetParent() is RigidBody3D {Sleeping: true}) return syncData;
        
        _lastSendSyncData = syncData;
        CollectSyncData(syncData);
        return syncData;
    }

    public Dictionary GetClearSyncData()
    {
        var syncDataUnique = new Dictionary();
        if (Multiplayer.GetUniqueId() != GetMultiplayerAuthority()) return syncDataUnique;
        
        var syncData = new Dictionary();
        CollectSyncData(syncData);
        foreach (var (key, value) in syncData)
        {
            if (!_lastSendSyncData.ContainsKey(key))
                syncDataUnique.Add(key, value);
            else if (!value.Equals(_lastSendSyncData[key]))
            {
                if ((string) key == "ParentPath")
                {
                    if ((NodePath) _lastSendSyncData[key] != (NodePath) value)
                        syncDataUnique.Add(key, value);
                }
                else syncDataUnique.Add(key, value);
            }
                
        }
        _lastSendSyncData = syncData;
        _lastSendSyncDataUnique = syncDataUnique;
        return syncDataUnique;
    }
    
    public void ApplySyncDataIfAuthority(Dictionary syncData)
    {
        if (Multiplayer.GetUniqueId() == GetMultiplayerAuthority()) return;
        ApplySyncData(syncData);
    }
    
    protected virtual void CollectSyncData(Dictionary syncData)
    {
        syncData["ParentPath"] = GetParent().GetParent().GetPath();
        Body.CollectSyncData(syncData);
    }

    void TransferNode(NodePath parentPath)
    {
        if (GetParent().GetParent().GetPath() == parentPath) return;
        var newParent = GetTree().Root.GetNode(parentPath);
        GetParent().GetParent().RemoveChild(GetParent());
        newParent.AddChild(GetParent());
    }
    
    protected virtual void ApplySyncData(Dictionary syncData)
    {
        if (syncData.ContainsKey("ParentPath")) TransferNode((NodePath) syncData["ParentPath"]);
        Body.ApplySyncData(syncData);
    }

}