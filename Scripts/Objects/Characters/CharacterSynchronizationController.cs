using System;
using Godot;
using Godot.Collections;

public partial class CharacterSynchronizationController : SynchronizationController
{
    private CharacterDoll _characterDoll;

    public override void _Ready()
    {
        _characterDoll = GetParent<CharacterDoll>();
        base._Ready();
    }

    protected override void CollectSyncData(Dictionary syncData)
    {
        syncData["GlobalPosition"] = _characterDoll.GlobalPosition;
        syncData["GlobalRotation"] = _characterDoll.GlobalRotation;
    }
    
    protected override void ApplySyncData(Dictionary syncData)
    {
        _characterDoll.GlobalPosition = (Vector3) syncData["GlobalPosition"];
        _characterDoll.GlobalRotation = (Vector3) syncData["GlobalRotation"];
    }
}