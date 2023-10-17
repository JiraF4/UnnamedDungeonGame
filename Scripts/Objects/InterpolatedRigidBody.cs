using Godot;
using Godot.Collections;

public partial class InterpolatedRigidBody : SynchronizedRigidBody
{
    protected SynchronizationInterpolator SynchronizationInterpolator;
    
    public override void _Ready()
    {
        SynchronizationInterpolator = new SynchronizationInterpolator(this);
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        Freeze = GetMultiplayerAuthority() != Multiplayer.GetUniqueId();
        base._PhysicsProcess(delta);
    }

    public override void _Process(double delta)
    {
        if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) SynchronizationInterpolator.Interpolate(delta);
        base._Process(delta);
    }
    
    public void ResetInterpolator()
    {
        SynchronizationInterpolator.Reset(Position, Quaternion);
    }
    
    
    public override void CollectSyncData(Dictionary syncData)
    {
        syncData["Position"] = Position;
        syncData["Rotation"] = Quaternion;
    }


    public override void ApplySyncData(Dictionary syncData)
    {
        SynchronizationInterpolator.Next(syncData, Multiplayer.GetRemoteSenderId());
    }
}