using Godot;
using System;

public partial class Slot : Storage
{
	[Export] protected NinePatchRect SlotRect;
	[Export] public Item Item
	{
		get
		{
			return GetChildOrNull<Item>(0);
		}
		set {}
	}

	public const int SlotCellSize = 24;
	public const int SlotCellHalfSize = SlotCellSize/2;
	public const int SlotBorderSize = 1;
	
	private bool _slotVisible;

	public override void _PhysicsProcess(double delta)
	{
		if (Item != null)
		{
			Item.GlobalPosition = GlobalPosition;
			Item.GlobalRotation = GlobalRotation;
		}

		base._PhysicsProcess(delta);
	}

	public override void _Process(double delta)
	{
		if (Item != null)
		{
			DebugInfo.AddLine(Item?.Name);
		}
		
		SlotRect.Visible = _slotVisible;
		_slotVisible = false;
		base._Process(delta);
	}

	public override void Draw()
	{
		_slotVisible = true;
		Item?.InsideInventoryDraw(SlotRect.Position + new Vector2(SlotBorderSize, SlotBorderSize));
	}
    
	public override bool IsScreenPositionInside(Vector2 screenPosition)
	{
		if (screenPosition.X < SlotRect.Position.X) return false;
		if (screenPosition.Y < SlotRect.Position.Y) return false;
		if (screenPosition.X > SlotRect.Position.X + SlotRect.Size.X) return false;	
		if (screenPosition.Y > SlotRect.Position.Y + SlotRect.Size.Y) return false;
		return true;
	}

	public override bool InsertItemServer(Vector2I inventoryPosition, Item item)
	{
		if (item == null) return false;
		if (Item != null) return false;
		
		item.Store(this, Vector2I.Zero);
		
		return true;
	}
	
	public override void ExtractItemServer(Item item)
	{
		base.ExtractItemServer(item);
	}
}
