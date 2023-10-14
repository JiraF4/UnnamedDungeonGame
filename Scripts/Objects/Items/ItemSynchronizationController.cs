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

    public override void _Process(double delta)
    {
        //DebugInfo.AddLine(Item.GetPath());
        //DebugInfo.AddLine(_temp?.ToString().Replace(", \"", ",\n\""));
        base._Process(delta);
    }

    protected override void CollectSyncData(Dictionary syncData)
    {
        syncData["Position"] = Item.Position;
        syncData["Rotation"] = Item.Quaternion;
        syncData["ItemInventoryPosition"] = Item.ItemInventoryPosition;
        base.CollectSyncData(syncData);
    }


    private Dictionary _temp;
    protected override void ApplySyncData(Dictionary syncData)
    {
        _temp = syncData;
        if (syncData.ContainsKey("Position")) Item.Position = (Vector3) syncData["Position"];
        if (syncData.ContainsKey("Rotation")) Item.Quaternion = (Quaternion) syncData["Rotation"];
        if (syncData.ContainsKey("ItemInventoryPosition")) Item.ItemInventoryPosition = (Vector2I) syncData["ItemInventoryPosition"];
        base.ApplySyncData(syncData);
    }   
}