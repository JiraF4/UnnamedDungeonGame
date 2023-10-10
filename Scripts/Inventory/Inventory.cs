using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Inventory : Storage
{
	private static readonly PackedScene InventoryRectScene = GD.Load<PackedScene>("res://Scenes/Inventory/InventoryRect.tscn");
	public const int InventoryCellSize = 24;
	public const int InventoryCellHalfSize = InventoryCellSize/2;
	public const int InventoryBorderSize = 1;
	public static readonly Vector2 InventoryBorderVector = new Vector2(InventoryBorderSize, InventoryBorderSize);
	private NinePatchRect _inventoryRect;

	private Item[,] _itemsMatrix;
	private readonly List<Item> _items = new();
	
	[Export] public Vector2I InventorySize = new(6, 6);
	private bool _inventoryVisible;

	private Vector2I _randomPosition;

	private Vector2I GetRandomFreePosition(Item item)
	{
		if (IsItemFit(_randomPosition, item)) return _randomPosition;
		var freePositions = GetAllFreePositionsInRect(item);
		if (freePositions.Count > 0)
		{
			_randomPosition = freePositions[(int) (GD.Randi() % freePositions.Count)];
            return _randomPosition;	
		}
		return new Vector2I(-1, -1);
	}
	
	private List<Vector2I> GetAllFreePositionsInRect(Item item)
	{
		List<Vector2I> freePositions = new();
		for (var x = 0; x < InventorySize.X; x++)
		{
			for (var y = 0; y < InventorySize.Y; y++)
			{
				var inventoryPosition = new Vector2I(x, y);
				if (IsItemFit(inventoryPosition, item)) freePositions.Add(inventoryPosition); 
			}	
		}
		return freePositions;
	}


	public override void Draw()
	{
		_inventoryVisible = true;
	}
	
	public override void _Ready()
	{
		base._Ready();
		
		_inventoryRect = (NinePatchRect) InventoryRectScene.Instantiate();
		_inventoryRect.Size = InventorySize*InventoryCellSize + new Vector2(InventoryBorderSize, InventoryBorderSize);
		_itemsMatrix = new Item[InventorySize.X, InventorySize.Y];
		AddChild(_inventoryRect);
	}

	public override void _Process(double delta)
	{
		_inventoryRect.Visible = _inventoryVisible;
		if (_inventoryVisible)
		{
			var camera3D = GetViewport().GetCamera3D();
			var inventoryScreenPosition = camera3D.UnprojectPosition(GlobalPosition);
			if (!camera3D.IsPositionBehind(GlobalPosition))
			{
				_inventoryRect.Position = inventoryScreenPosition - _inventoryRect.Size / 2;
				foreach (var item in _items)
				{
					item.InsideInventoryDraw(_inventoryRect.Position);
				}
			}
			else _inventoryRect.Visible = false;
		}
		DebugInfo.AddLine("InventoryVisible: " + _inventoryVisible.ToString());
		_inventoryVisible = false;
		base._Process(delta);
	}
	
	public override bool IsScreenPositionInside(Vector2 screenPosition)
	{
		if (screenPosition.X < _inventoryRect.Position.X) return false;
		if (screenPosition.Y < _inventoryRect.Position.Y) return false;
		if (screenPosition.X > _inventoryRect.Position.X + _inventoryRect.Size.X) return false;	
		if (screenPosition.Y > _inventoryRect.Position.Y + _inventoryRect.Size.Y) return false;
		return true;
	}

	public bool IsInventoryPositionInside(Vector2I inventoryPosition)
	{
		if (inventoryPosition.X < 0) return false;
		if (inventoryPosition.Y < 0) return false;
		if (inventoryPosition.X >= InventorySize.X) return false;
		if (inventoryPosition.Y >= InventorySize.Y) return false;
		return true;
	}
	
	public Item GetItemScreenPosition(Vector2 screenPosition)
	{
		return GetItem(GetInventoryPosition(screenPosition));
	}
	
	public Item GetItem(Vector2I inventoryRectPosition)
	{
        if (!IsInventoryPositionInside(inventoryRectPosition)) return null;
        return _itemsMatrix[inventoryRectPosition.X, inventoryRectPosition.Y];
	}
	
	public bool InsertItemScreenPosition(Vector2 screenPosition, Item holdItem)
	{
		return InsertItem(GetInventoryPosition(screenPosition), holdItem);
	}
	
	public override bool InsertItem(Vector2I inventoryPosition, Item item)
	{
		if (!IsItemFit(inventoryPosition, item)) inventoryPosition = _randomPosition;
		if (!IsItemFit(inventoryPosition, item)) return false;
		
		for (var x = 0; x < item.ItemInventoryMatrixSize.X; x++)
		{
			for (var y = 0; y < item.ItemInventoryMatrixSize.Y; y++)
			{
				var inventoryPositionSet = new Vector2I(inventoryPosition.X + x, inventoryPosition.Y + y);
				if (item.ItemInventoryMatrix[x, y]) _itemsMatrix[inventoryPositionSet.X, inventoryPositionSet.Y]= item;
			}	
		}
		_items.Add(item);
		
		item.Store(this, inventoryPosition);
		
		return true;
	}

	public bool IsItemFit(Vector2I inventoryPosition, Item item)
	{
		if (!IsInventoryPositionInside(inventoryPosition)) return false;
		for (var x = 0; x < item.ItemInventoryMatrixSize.X; x++)
		{
			for (var y = 0; y < item.ItemInventoryMatrixSize.Y; y++)
			{
				if (!item.ItemInventoryMatrix[x, y]) continue;
				var inventoryPositionCheck = new Vector2I(inventoryPosition.X + x, inventoryPosition.Y + y);
				if (!IsInventoryPositionInside(inventoryPositionCheck)) return false;
				if (_itemsMatrix[inventoryPositionCheck.X, inventoryPositionCheck.Y] != null) return false;
			}	
		}
		return true;
	}

	public override Vector2I GetInventoryPosition(Vector2 screenPosition)
	{
		return (Vector2I) ((screenPosition + new Vector2(InventoryCellSize, InventoryCellSize) - _inventoryRect.Position - InventoryBorderVector) / InventoryCellSize) - new Vector2I(1, 1);
	}
	
	public Vector2 GetScreenPosition(Vector2I inventoryPosition)
	{
		return _inventoryRect.Position + InventoryBorderVector + inventoryPosition * InventoryCellSize;
	}

	public Vector2 AlignPosition(Vector2 screenPosition)
	{
		return GetScreenPosition(GetInventoryPosition(screenPosition));
	}
	
	public override Vector2 AlignPositionItem(Vector2 screenPosition, Item item)
	{
		if (item != null && !IsItemFit(GetInventoryPosition(screenPosition), item))
		{
			var randomPosition = GetRandomFreePosition(item);
            if (randomPosition != new Vector2I(-1, -1))
            {
                return GetScreenPosition(randomPosition);
            }

            return screenPosition;
		}
		var inventoryPosition = GetInventoryPosition(screenPosition);
		_randomPosition = inventoryPosition;
		return GetScreenPosition(inventoryPosition);
	}

	public override void ExtractItem(Item item)
	{
		for (var x = 0; x < item.ItemInventoryMatrixSize.X; x++)
		{
			for (var y = 0; y < item.ItemInventoryMatrixSize.Y; y++)
            {
                var inventoryPositionSet = new Vector2I(item.ItemInventoryPosition.X + x, item.ItemInventoryPosition.Y + y);
                if (item.ItemInventoryMatrix[x, y]) _itemsMatrix[inventoryPositionSet.X, inventoryPositionSet.Y] = null;
            }
		}
		_items.Remove(item);
		item.Extract(GetTree().Root);
	}
}
