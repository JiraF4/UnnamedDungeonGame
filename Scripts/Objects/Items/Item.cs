using Godot;
using System.Numerics;
using Vector2 = Godot.Vector2;
using Vector3 = Godot.Vector3;

public partial class Item : InterpolatedRigidBody
{
    public TextureRect ItemRect;
    
    public bool[,] ItemInventoryMatrix;
    public Vector2I ItemInventoryMatrixSize;
    
    [Export] public Vector2I ItemInventoryPosition;
    private bool _itemVisible;
    
    public Storage Storage
    {
        get
        {
            var parent = GetParent();
            if (parent is Storage storage) return storage;
            return null;
        }
    }

    
    public override void _Ready()
    {
        ItemRect = (TextureRect) FindChild("ItemRect");
        CreateMatrix();
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ItemRect.Visible = _itemVisible;
        _itemVisible = false;
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

    public void SetRectPosition(Vector2 position)
    {
        ItemRect.Position = position;
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
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void TransferServer(NodePath storagePath, Vector2I inventoryPosition, bool replaceCurrent)
    {
        var storage = GetTree().Root.GetNodeOrNull<Storage>(storagePath);
        Storage?.ExtractItemServer(this);
        storage?.InsertItemServer(inventoryPosition, this);
    }
    
    public void TransferClient(Storage storage, Vector2I inventoryPosition, bool replaceCurrent = true)
    {
        RpcId(1, nameof(TransferServer), storage?.GetPath(), inventoryPosition, replaceCurrent);
    }
}
