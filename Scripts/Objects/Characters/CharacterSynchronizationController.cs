using System;
using Godot;
using Godot.Collections;

public partial class CharacterSynchronizationController : SynchronizationController
{
    protected CharacterController CharacterController;

    public override void _Ready()
    {
        CharacterController = GetParent<CharacterDoll>().GetNode<CharacterController>("CharacterController");
        base._Ready();
    }

    protected override void CollectSyncData(Dictionary syncData)
    {
        CharacterController.CollectSyncData(syncData);
        base.CollectSyncData(syncData);
    }
    
    protected override void ApplySyncData(Dictionary syncData)
    {
        CharacterController.ApplySyncData(syncData);
        base.ApplySyncData(syncData);
    }
}