﻿using Godot;

public partial class Storage : Node3D
{
    public virtual void ExtractItem(Item item) {}
    public virtual bool InsertItem(Vector2I inventoryPosition, Item item) { return false; }
    public virtual void Draw() {}
    
    public virtual bool IsScreenPositionInside(Vector2 screenPosition)
    {
        return false;
    }
    
    public virtual Vector2 AlignPositionItem(Vector2 screenPosition, Item item)
    {
        return screenPosition;
    }
    
    public virtual Vector2I GetInventoryPosition(Vector2 screenPosition)
    {
        return Vector2I.Zero;
    }
}