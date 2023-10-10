using Godot;
using System;
using DeepDungeon.Dungeon;

public partial class MapGenerator : Node
{
	[Export] public TextureRect DebugTextureRect;
	public MapHolder MapHolder;
	public Random Random;
	
	public virtual void Generate()
	{
		var debugImage = Image.Create(MapHolder.Map.Size.X, MapHolder.Map.Size.Y, false, Image.Format.Rgb8);
		for (var x = 0; x < MapHolder.Map.Size.X; x++)
		{
			for (var y = 0; y < MapHolder.Map.Size.Y; y++)
			{
				var color = Map.MapCellColors[MapHolder.Map.MapCells[x, y].MapCellType];
				var customColor = MapHolder.Map.MapCells[x, y].CellColor;
				customColor.A = 0.5f;
				if (customColor != Colors.Transparent) 
					color = color.Blend(customColor);
				if (x % Map.ChunkSize == 0 || y % Map.ChunkSize == 0)
					color = color.Blend(new Color(0.0f, 0.0f, 0.0f, 0.3f));
				debugImage.SetPixel(x, y, color);
			}
		}
		
		var debugTexture = ImageTexture.CreateFromImage(debugImage);
		DebugTextureRect.Texture = debugTexture;
		DebugTextureRect.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
	}
}
