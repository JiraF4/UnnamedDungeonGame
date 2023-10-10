using System.Collections.Generic;
using System.Linq;

namespace Dungeon.Tools
{
    public static class ArrayExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T[,] target)
        {
            return target.Cast<T>();
        }

        public static T[,] Rotate2D<T>(this T[,] target)
        {
            var width = target.GetLength(0);
            var height = target.GetLength(1);

            var rotated = new T[height, width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    rotated[x, y] = target[height - 1 - y, x];
                }
            }
            
            return rotated;
        }

        public static T[] Fill<T>(this T[] target, T value)
        {
            for (var i = 0; i < target.Length; i++)
            {
                target[i] = value;
            }

            return target;
        }

        public static T[,,] Fill3D<T>(this T[,,] target, T value)
        {
            for (var x = 0; x < target.GetLength(0); x++)
            {
                for (var y = 0; y < target.GetLength(1); y++)
                {
                    for (var z = 0; z < target.GetLength(2); z++)
                    {
                        target[x, y, z] = value;
                    }
                }
            }

            return target;
        }

        public static T[,,,] Fill4D<T>(this T[,,,] target, T value)
        {
            for (var x = 0; x < target.GetLength(0); x++)
            {
                for (var y = 0; y < target.GetLength(1); y++)
                {
                    for (var z = 0; z < target.GetLength(2); z++)
                    {
                        for (var w = 0; w < target.GetLength(3); w++)
                        {
                            target[x, y, z, w] = value;
                        }
                    }
                }
            }

            return target;
        }
    }
}