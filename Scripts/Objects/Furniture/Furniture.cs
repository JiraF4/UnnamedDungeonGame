using Godot;
using Godot.Collections;

public partial class Furniture : RigidBody3D
{
    private FurnitureSynchronizationController SynchronizationController;
    protected SynchronizationInterpolator SynchronizationInterpolator;
    
    public override void _Ready()
    {
        SynchronizationInterpolator = new SynchronizationInterpolator(this);
        SynchronizationController = GetNode<FurnitureSynchronizationController>("SynchronizationController");
        base._Ready();
    }
    
    public override void _Process(double delta)
    {
        if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) SynchronizationInterpolator.Interpolate(delta);
        base._Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Sleeping)
        {
            foreach (var body in GetCollidingBodies())
            {
                if (body.GetMultiplayerAuthority() != 1)
                    if (body is CharacterDoll || body is Furniture)
                        SynchronizationController.TemporallyAuthority(body.GetMultiplayerAuthority());
            }
        }
        base._PhysicsProcess(delta);
    }
    
    public void CollectSyncData(Dictionary syncData)
    {
        syncData["Position"] = Position;
        syncData["Rotation"] = Quaternion;
    }


    public void ApplySyncData(Dictionary syncData)
    {
        Vector3? position = null;
        if (syncData.ContainsKey("Position"))
            position = (Vector3) syncData["Position"];
        Quaternion? rotation = null;
        if (syncData.ContainsKey("Rotation"))
            rotation = (Quaternion) syncData["Rotation"];
        SynchronizationInterpolator.Next(position, rotation, Multiplayer.GetRemoteSenderId());
    }

    public void ResetInterpolator()
    {
        SynchronizationInterpolator.Reset(Position, Quaternion);
    }
}