using System.Collections.Generic;
using Godot;

namespace DeepDungeon.Dungeon.Generation
{
    public class MapPatternReplace
    {
        public static readonly Dictionary<char, MapCellType> MapCellCharacters = new Dictionary<char, MapCellType>
        {
            {'?', MapCellType.Unknown},
            {'#', MapCellType.Wall},
            {'_', MapCellType.Empty},
            {'*', MapCellType.Unbreakable},
        };

        private readonly Vector2I _size;
        private readonly MapCellType[,] _pattern;
        private readonly MapCellType[,] _replace;

        public MapPatternReplace(char[,] pattern, char[,] replace)
        {
            _size = new Vector2I(pattern.GetLength(0), pattern.GetLength(1));
            _pattern = new MapCellType[_size.X, _size.Y];
            _replace = new MapCellType[_size.X, _size.Y];
            for (var x = 0; x < _size.X; x++)
            {
                for (var y = 0; y < _size.X; y++)
                {
                    _pattern[x, y] = MapCellCharacters[pattern[x, y]];
                    _replace[x, y] = MapCellCharacters[replace[x, y]];
                }
            }
        }

        public void Apply(Map map)
        {
            for (var x = 0; x < map.Size.X - _size.X; x++)
            {
                for (var y = 0; y < map.Size.Y - _size.Y; y++)
                {
                    var position = new Vector2I(x, y);
                    if (map.CheckPattern(position, _pattern))
                        map.SetPattern(position, _replace);
                }    
            }
        }
        
    }
}