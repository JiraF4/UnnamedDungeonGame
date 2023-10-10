using System;
using System.Collections.Generic;
using System.Linq;
using DeepDungeon.Dungeon;
using DeepDungeon.Dungeon.Generation;
using DeepDungeon.Dungeon.Generation.TunnelGenerator;
using Dungeon;
using Godot;

public partial class TunnelGenerator : MapGenerator
{
    private readonly List<TunnelGeneratorMiner> _miners = new();

    public override void Generate()
    {
        var diagonalReplacer1 = new MapPatternReplace(new[,]{
                {'#', '#', '_', '_'},
                {'#', '#', '_', '_'},
                {'_', '_', '#', '#'},
                {'_', '_', '#', '#'},
            },
            new[,] {
                {'_', '_', '_', '_'},
                {'_', '_', '_', '_'},
                {'_', '_', '_', '_'},
                {'_', '_', '_', '_'},
            });
        var diagonalReplacer2 = new MapPatternReplace(
            new[,]{
                {'_', '_', '#', '#'},
                {'_', '_', '#', '#'},
                {'#', '#', '_', '_'},
                {'#', '#', '_', '_'},
            },
            new[,] {
                {'_', '_', '_', '_'},
                {'_', '_', '_', '_'},
                {'_', '_', '_', '_'},
                {'_', '_', '_', '_'},
            }
        );
        
        
        Random = new Random(7);

        var maxDistance = (MapHolder.Map.Size / 2).Length() + 1;
        for (var x = 0; x < MapHolder.Map.Size.X; x++)
        {
            for (var y = 0; y < MapHolder.Map.Size.Y; y++)
            {
                var position = new Vector2I(x, y);
                var distance = ((MapHolder.Map.Size / 2) - position).Length();
                if (distance > maxDistance / 3)
                {
                    var reveseDistance = (int) (maxDistance - distance);
                    var chance = reveseDistance * reveseDistance * reveseDistance / 200 + 1;
                    if ((Random.Next() % chance) == 0)
                    {
                        MapHolder.Map.MapCells[x, y].MapCellType = MapCellType.Unbreakable;
                    }
                }
            }    
        }

        for (var i = 0; i < 30; i++)
        {
            var cells = new List<MapCell>();
            for (var x = 0; x < MapHolder.Map.Size.X; x++)
            {
                for (var y = 0; y < MapHolder.Map.Size.Y; y++)
                {
                    var cell = MapHolder.Map.MapCells[x, y];
                    if (cell.Neighbours.Any(c => c != null && c.MapCellType == MapCellType.Unbreakable))
                    {
                        if ((Random.Next() % 4) == 0)
                        {
                            cells.Add(cell);
                        }
                    }
                }
            }

            foreach (var cell in cells)
            {
                cell.MapCellType = MapCellType.Unbreakable;
            }
        }

        _miners.Add(new TunnelGeneratorMiner(this, MapHolder.Map.Size/2, 3, new Direction2I(Random.Next() % 4), 30));
        _miners.Add(new TunnelGeneratorMiner(this, MapHolder.Map.Size/2, 3, new Direction2I(Random.Next() % 4), 30));
        _miners.Add(new TunnelGeneratorMiner(this, MapHolder.Map.Size/2, 3, new Direction2I(Random.Next() % 4), 30));
        _miners.Add(new TunnelGeneratorMiner(this, MapHolder.Map.Size/2, 3, new Direction2I(Random.Next() % 4), 30));
        while (_miners.Count > 0)
        {
            for (var i = 0; i < _miners.Count; i++)
            {
                if (_miners[i].Mine())
                {
                    _miners.RemoveAt(i);
                    i--;
                }
                else
                {
                    if ((_miners[i].LifeTime % 2) == 0 && _miners[i].LifeTime < 30)
                    {
                        var newRadius = _miners[i].Radius + (Random.Next() % 2) - (Random.Next() % 2);;
                        if (newRadius > 5) newRadius = 5;
                        if (newRadius < 2) newRadius = 2;
                        var side = 1 + (2 * (Random.Next() % 2));
                        var newDirection = _miners[i].Direction + side;
                        _miners.Add(new TunnelGeneratorMiner(
                                this,
                                _miners[i].Position + (newDirection * (newRadius + _miners[i].Radius - 1)),
                                newRadius,
                                newDirection,
                                30 + (Random.Next() % 10)
                            ));
                    }
                }
            }
        }
        
        diagonalReplacer1.Apply(MapHolder.Map);
        diagonalReplacer2.Apply(MapHolder.Map);
        
        for (var x = 1; x < MapHolder.Map.Size.X - 2; x++)
        {
            for (var y = 1; y < MapHolder.Map.Size.Y - 2; y++)
            {
                var position = new Vector2I(x, y);
                var rect = new Rect2I(position - new Vector2I(1, 1), new Vector2I(3, 3));
                if (MapHolder.Map.IsAllCellsOfType(rect, MapCellType.Wall))
                {
                    var roomCells = MapHolder.Map.GetEnclosedSpace(position, 7);
                    if (roomCells.Any())
                    {
                        foreach (var roomCell in roomCells)
                        {
                            var roomRect = new Rect2I(roomCell.Position, new Vector2I(1, 1)).Grow(6);
                            MapHolder.Map.SetCells(roomRect, MapCellType.Unknown);
                            //MapHolder.Map.SetCellsColor(roomRect, new Color(0.0f, 0.0f, 1.0f, 0.5f));
                        }

                        var roomSpace = MapHolder.Map.GetConnectedCellsOfSameType(position);
                        
                        var roomEdges = MapHolder.Map.GetEdgesOfRegion(roomSpace)
                            .Where(c => c.Neighbours.Any(n => n != null && n.MapCellType == MapCellType.Empty))
                            .ToList();
                        roomEdges = roomEdges
                            .Where(c => c.Neighbours.Any(c => roomEdges.Contains(c)))
                            .OrderBy(c => Random.Next())
                            .ToList();

                        foreach (var roomCell in roomSpace)
                        {
                            roomCell.CellColor = Colors.Bisque;
                            roomCell.MapCellType = MapCellType.Empty;
                            roomCell.textureOffset = 64;
                        }

                        foreach (var roomEdge in roomEdges)
                        {
                            //roomEdge.CellColor = Colors.GreenYellow;
                        }
                        
                        if (roomEdges.Any())
                        {
                            var doors = new List<MapCell>() {};
                            foreach (var roomEdge in roomEdges)
                            {
                                var checkRadius = 16;
                                var checkRect = new Rect2I(roomEdge.Position - new Vector2I(checkRadius, checkRadius),
                                    new Vector2I(checkRadius * 2, checkRadius * 2));
                                if (!MapHolder.Map.IsAnyCellsOfType(checkRect, MapCellType.Unknown))
                                {
                                    var secondCell = roomEdge
                                        .Neighbours
                                        .First(c => c != null && c != roomEdge && roomEdges.Contains(c));
                                    doors.Add(roomEdge);
                                    doors.Add(secondCell);
                                    roomEdge.MapCellType = MapCellType.Unknown; 
                                    secondCell.MapCellType = MapCellType.Unknown;
                                    roomEdge.CellColor = Colors.Red;
                                    secondCell.CellColor = Colors.Blue;
                                }
                            }

                            foreach (var doorCell in doors)
                            {
                                doorCell.MapCellType = MapCellType.Empty;
                            }
                        }
                    }
                }
            }    
        }
        
        base.Generate();
    }
}