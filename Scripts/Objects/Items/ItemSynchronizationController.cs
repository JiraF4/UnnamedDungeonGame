using System.Diagnostics;
using Godot;
using Godot.Collections;

public partial class ItemSynchronizationController : SynchronizationController
{
    protected Item Item;
    
    public override void _Ready()
    {
        Item = GetParent<Item>();
        base._Ready();
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) Item.Freeze = true;
        base._PhysicsProcess(delta);
    }
    
    protected override void CollectSyncData(Dictionary syncData)
    {
        syncData["Position"] = Item.Position;
        syncData["Rotation"] = Item.Quaternion;
        syncData["ItemInventoryPosition"] = Item.ItemInventoryPosition;
        base.CollectSyncData(syncData);
    }


    protected override void ApplySyncData(Dictionary syncData)
    {
        if (syncData.ContainsKey("Position")) Item.Position = (Vector3) syncData["Position"];
        if (syncData.ContainsKey("Rotation")) Item.Quaternion = (Quaternion) syncData["Rotation"];
        if (syncData.ContainsKey("ItemInventoryPosition")) Item.ItemInventoryPosition = (Vector2I) syncData["ItemInventoryPosition"];
        base.ApplySyncData(syncData);
    }   
}