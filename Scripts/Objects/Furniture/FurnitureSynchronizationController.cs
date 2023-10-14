using Godot;
using Godot.Collections;

public partial class FurnitureSynchronizationController : SynchronizationController
{
    protected RigidBody3D Furniture;

    public override void _PhysicsProcess(double delta)
    {
        if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) Furniture.Freeze = true;
        base._PhysicsProcess(delta);
    }

    public override void _Ready()
    {
        Furniture = GetParent<RigidBody3D>();
        base._Ready();
    }
    
    protected override void CollectSyncData(Dictionary syncData)
    {
        syncData["Position"] = Furniture.Position;
        syncData["Rotation"] = Furniture.Quaternion;
        base.CollectSyncData(syncData);
    }


    protected override void ApplySyncData(Dictionary syncData)
    {
        if (syncData.ContainsKey("Position")) Furniture.Position = (Vector3) syncData["Position"];
        if (syncData.ContainsKey("Rotation")) Furniture.Quaternion = (Quaternion) syncData["Rotation"];
        base.ApplySyncData(syncData);
    }  
}