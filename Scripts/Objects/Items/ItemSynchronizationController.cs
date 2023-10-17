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
    
    protected override void CollectSyncData(Dictionary syncData)
    {
        syncData["ItemInventoryPosition"] = Item.ItemInventoryPosition;
        base.CollectSyncData(syncData);
    }


    protected override void ApplySyncData(Dictionary syncData)
    {
        if (syncData.ContainsKey("ItemInventoryPosition")) Item.ItemInventoryPosition = (Vector2I) syncData["ItemInventoryPosition"];
        base.ApplySyncData(syncData);
    }   
}