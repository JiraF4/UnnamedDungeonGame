using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DeepDungeon.Dungeon
{
    public class Map
    {
        public MapHolder MapHolder;
        public Vector2I Size;
        public readonly MapCell[,] MapCells;
        public readonly MapChunkMesh[,] MapChunkMeshes;
        public const int ChunkSize = 16;
        public const float CellRealSize = 1.0f;
        public ulong Flag;
        public BoxShape3D BlockCollisionShape;

        public static readonly Dictionary<MapCellType, Color> MapCellColors = new Dictionary<MapCellType, Color>
        {
            {MapCellType.Wall, new Color(1.0f, 1.0f, 1.0f)},
            {MapCellType.Empty, new Color(0.2f, 0.2f, 0.2f)},
            {MapCellType.Unbreakable, new Color(0.0f, 0.0f, 0.0f)}
        };

        public Map(MapHolder mapHolder, int width, int height)
        {
            MapHolder = mapHolder;
            MapCells = new MapCell[width, height];
            MapChunkMeshes = new MapChunkMesh[width, height];
            Size = new Vector2I(width, height);
            MapCells = new MapCell[width, height];
            MapChunkMeshes = new MapChunkMesh[width / ChunkSize, height / ChunkSize];
            for (var x = 0; x < width / ChunkSize; x++)
            {
                for (var y = 0; y < height / ChunkSize; y++)
                {
                    var chunk = new MapChunkMesh(this, new Vector2I(x, y));
                    chunk.Position = new Vector3(x * ChunkSize * CellRealSize, 0, y * ChunkSize * CellRealSize); // + new Vector3(x * 0.1f, 0, y * 0.1f);
                    MapChunkMeshes[x, y] = chunk;
                    MapHolder.AddChild(chunk);
                }
            }
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    MapCells[x, y] = new MapCell()
                    {
                        Position = new Vector2I(x, y),
                        CellColor = Colors.Transparent,
                    };
                    
                }
            }
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    MapCells[x, y].Neighbours = GetNeighbours(new Vector2I(x, y));
                }
            }

            BlockCollisionShape = new BoxShape3D()
            {
                Size = new Vector3(CellRealSize, CellRealSize*3, CellRealSize)
            };
        }
        public bool IsRectInside(Rect2I rect)
        {
            if (rect.Position.X < 0) return false;
            if (rect.Position.Y < 0) return false;
            if (rect.End.X >= Size.X) return false;
            if (rect.End.Y >= Size.Y) return false;
            return true;
        }
        public bool IsAllCellsOfType(Rect2I rect, MapCellType type)
        {
            if (!IsRectInside(rect)) return false;
            for (var x = rect.Position.X; x < rect.End.X; x++)
            {
                for (var y = rect.Position.Y; y < rect.End.Y; y++)
                {
                    if (MapCells[x, y].MapCellType != type)
                        return false;
                }
            }

            return true;
        }
        public bool IsAnyCellsOfType(Rect2I rect, MapCellType type)
        {
            rect = FitRectInside(rect);
            for (var x = rect.Position.X; x < rect.End.X; x++)
            {
                for (var y = rect.Position.Y; y < rect.End.Y; y++)
                {
                    if (MapCells[x, y].MapCellType == type)
                        return true;
                }
            }

            return false;
        }
        public Rect2I FitRectInside(Rect2I rect)
        {
            var rectPosition = rect.Position;
            if (rectPosition.X < 0) rectPosition.X = 0;
            if (rectPosition.Y < 0) rectPosition.Y = 0;
            var rectEnd = rect.End;
            if (rectEnd.X >= Size.X) rectEnd.X = Size.X - 1;
            if (rectEnd.Y >= Size.Y) rectEnd.Y = Size.Y - 1;
            return new Rect2I(rectPosition, rectEnd - rectPosition);
        }
        public void SetCells(Rect2I rect, MapCellType type)
        {
            rect = FitRectInside(rect);
            for (var x = rect.Position.X; x < rect.End.X; x++)
            {
                for (var y = rect.Position.Y; y < rect.End.Y; y++)
                {
                    MapCells[x, y].MapCellType = type;
                }
            }
        }
        public void SetCellsColor(Rect2I rect, Color color)
        {
            var rectPosition = rect.Position;
            if (rectPosition.X < 0) rectPosition.X = 0;
            if (rectPosition.Y < 0) rectPosition.Y = 0;
            var rectEnd = rect.End;
            if (rectEnd.X >= Size.X) rectEnd.X = Size.X - 1;
            if (rectEnd.Y >= Size.Y) rectEnd.Y = Size.Y - 1;
            for (var x = rectPosition.X; x < rectEnd.X; x++)
            {
                for (var y = rectPosition.Y; y < rectEnd.Y; y++)
                {
                    MapCells[x, y].CellColor = color;
                }
            }
        }
        public bool CheckPattern(Vector2I position, MapCellType[,] pattern)
        {
            var size = new Vector2I(pattern.GetLength(0), pattern.GetLength(1));
            var rect = new Rect2I(position, size);
            if (!IsRectInside(rect)) return false;
            for (var x = 0; x < size.X; x++)
            {
                for (var y = 0; y < size.Y; y++)
                {
                    if (pattern[x, y] == MapCellType.Unknown)
                        continue;
                    if (MapCells[x + position.X, y + position.Y].MapCellType != pattern[x, y])
                        return false;
                }
            }

            return true;
        }
        public void SetPattern(Vector2I position, MapCellType[,] pattern)
        {
            var size = new Vector2I(pattern.GetLength(0), pattern.GetLength(1));
            var rect = new Rect2I(position, size);
            if (!IsRectInside(rect)) return;
            for (var x = 0; x < size.X; x++)
            {
                for (var y = 0; y < size.Y; y++)
                {
                    if (pattern[x, y] == MapCellType.Unknown)
                        continue;
                    MapCells[x + position.X, y + position.Y].MapCellType = pattern[x, y];
                }
            }
        }
        public MapCell[] GetNeighbours(Vector2I position)
        {
            var cells = new MapCell[4];
            if (position.Y < (Size.Y - 1)) cells[0] = MapCells[position.X, position.Y + 1];
            if (position.X < (Size.X - 1)) cells[1] = MapCells[position.X + 1, position.Y];
            if (position.Y > 0) cells[2] = MapCells[position.X, position.Y - 1];
            if (position.X > 0) cells[3] = MapCells[position.X - 1, position.Y];
            return cells;
        }
        public List<MapCell> GetEnclosedSpace(Vector2I position, int minWidth)
        {
            Flag++;
            if (GetMinDistanceForCellOfType(position, MapCellType.Wall) <= minWidth) return new List<MapCell>();
            var cells = new List<MapCell> {MapCells[position.X, position.Y]};
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var neighbours = cell.Neighbours
                    .Where(c => c != null 
                                && c.Flag != Flag 
                                && c.MapCellType == MapCellType.Wall
                                && GetMinDistanceForCellOfType(c.Position, MapCellType.Wall) > minWidth)
                    .ToList();
                foreach (var mapCell in neighbours)
                {
                    mapCell.Flag = Flag;
                }
                cells.AddRange(neighbours);
            }
            return cells;
        }
        public List<MapCell> GetConnectedCellsOfSameType(Vector2I position)
        {
            Flag++;
            var cells = new List<MapCell> {MapCells[position.X, position.Y]};
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var neighbours = cell.Neighbours
                   .Where(c => c!= null 
                                && c.Flag != Flag 
                                && c.MapCellType == cell.MapCellType)
                   .ToList();
                foreach (var mapCell in neighbours)
                {
                    mapCell.Flag = Flag;
                }
                cells.AddRange(neighbours);
            }
            return cells;
        }
        public int GetMinDistanceForCellOfType(Vector2I position, MapCellType type)
        {
            var rect = new Rect2I(position, new Vector2I(1, 1));
            var count = 0;
            while (IsAllCellsOfType(rect, type))
            {
                rect = rect.Grow(1);
                count++;
            }
            
            return count;
        }
        public List<MapCell> GetEdgesOfRegion(List<MapCell> regionCells)
        {
            var edges = new List<MapCell>();
            foreach (var regionCell in regionCells)
            {
                var neighbours = regionCell.Neighbours
                    .Where(n => n != null && !regionCells.Contains(n) && !edges.Contains(n));
                edges.AddRange(neighbours);
            }

            return edges;
        }
        public void ReBakeMeshes()
        {
            foreach (var mesh in MapChunkMeshes)
            {
                mesh.ReBake();
            }
        }
    }
}