using System.Collections.Generic;
using Godot;

namespace DeepDungeon.Dungeon
{
    public class MapCell
    {
        public Vector2I Position;
        public MapCellType MapCellType;
        public Color CellColor;
        public ulong Flag;
        public MapCell[] Neighbours;
        public int TextureOffset;
        public MapCellTypeAdd MapCellTypeAdd = MapCellTypeAdd.Default;
        public bool HasFurniture;
    }
}