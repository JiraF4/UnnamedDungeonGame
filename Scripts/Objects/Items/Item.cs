using Godot;
using System.Numerics;
using Vector2 = Godot.Vector2;
using Vector3 = Godot.Vector3;

public partial class Item : RigidBody3D
{
    public TextureRect ItemRect;
    
    public bool[,] ItemInventoryMatrix;
    public Vector2I ItemInventoryMatrixSize;
    
    [Export] public Vector2I ItemInventoryPosition;
    public Storage Storage;
    
    private bool _itemVisible;
    
    public override void _Ready()
    {
        ItemRect = (TextureRect) FindChild("ItemRect");
        CreateMatrix();
        base._Ready();
    }

    void CreateMatrix()
    {
        ItemInventoryMatrixSize = new Vector2I(Mathf.CeilToInt(ItemRect.Size.X / Inventory.InventoryCellSize), Mathf.CeilToInt(ItemRect.Size.Y / Inventory.InventoryCellSize));
        ItemInventoryMatrix = new bool[ItemInventoryMatrixSize.X, ItemInventoryMatrixSize.Y];
        for (var x = 0; x < ItemInventoryMatrixSize.X; x++)
        {
            for (var y = 0; y < ItemInventoryMatrixSize.Y; y++)
            {
                ItemInventoryMatrix[x, y] = true;
            }
        }
    }

    public void InsideInventoryDraw(Vector2 inventoryRectPosition)
    {
        SetRectPosition(inventoryRectPosition + Inventory.InventoryBorderVector + ItemInventoryPosition * Inventory.InventoryCellSize);
        _itemVisible = true;
    }

    private void ExtractFromInventory()
    {
        Storage.ExtractItem(this);
    }

    public void SetRectPosition(Vector2 position)
    {
        ItemRect.Position = position;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        ItemRect.Visible = _itemVisible;
        _itemVisible = false;
    }

    public void Store(Storage storage, Vector2I inventoryPosition)
    {
        GetParent().RemoveChild(this);
        storage.AddChild(this);
        FreezeMode = FreezeModeEnum.Kinematic;
        Freeze = true;
        GlobalPosition = new Vector3(0.0f, -100.0f, 0.0f);
        CollisionLayer = 0;
    
        ItemInventoryPosition = inventoryPosition;
        Storage = storage;
    }
    
    public void Extract(Node space)
    {
        var globalPosition = GlobalPosition;
        GetParent().RemoveChild(this);
        space.AddChild(this);
        GlobalPosition = globalPosition;
        Freeze = false;
        CollisionLayer = (uint) CollisionLayers.Items | (uint) CollisionLayers.All;
        
        ItemInventoryPosition = Vector2I.Zero;
        Storage = null;
    }
}
