using Godot;
using System;
using DeepDungeon.Dungeon;

public partial class MapGenerator : Node
{
	public MapHolder MapHolder;
	public Random Random;
	
	public virtual void Generate()
	{
		var debugImage = Image.Create(MapHolder.Map.Size.X, MapHolder.Map.Size.Y, false, Image.Format.Rgb8);
		for (var x = 0; x < MapHolder.Map.Size.X; x++)
		{
			for (var y = 0; y < MapHolder.Map.Size.Y; y++)
			{
				var cell = MapHolder.Map.MapCells[x, y];
				var color = Map.MapCellColors[cell.MapCellType];
				var customColor = cell.CellColor;
				customColor.A = 0.5f;
				if (customColor != Colors.Transparent) 
					color = color.Blend(customColor);
				if (x % Map.ChunkSize == 0 || y % Map.ChunkSize == 0)
					color = color.Blend(new Color(0.0f, 0.0f, 0.0f, 0.3f));
				if (cell.HasFurniture)
					color = color.Blend(new Color(0.0f, 0.5f, 0.0f, 0.3f));
				debugImage.SetPixel(x, y, color);
			}
		}
		
		var debugTexture = ImageTexture.CreateFromImage(debugImage);
		MapHolder.DebugTextureRect.Texture = debugTexture;
		MapHolder.DebugTextureRect.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
	}
}
