using Godot;

namespace Dungeon
{
    public struct Direction2I
    {
        private static readonly Vector2I[] Directions = {
            new Vector2I( 0, 1),
            new Vector2I( 1, 0),
            new Vector2I( 0,-1),
            new Vector2I(-1, 0)
        };
        
        private static readonly Vector3[] DirectionsWorld = {
            new Vector3( 0, 0,1),
            new Vector3( 1, 0, 0),
            new Vector3( 0, 0,-1),
            new Vector3(-1, 0, 0)
        };
        
        public Direction2I(int direction)
        {
            _directionNum = (((direction % 4) + 4) % 4);;
        }

        public static Vector2I operator *(Direction2I direction, int length)
        {
            return direction.DirectionVec * length;
        }
        
        public static Direction2I operator +(Direction2I direction, int side)
        {
            var newDirection = new Direction2I(direction._directionNum + side);
            return newDirection;
        }
        
        public static bool operator !=(Direction2I direction, int directionInt)
        {
            return direction._directionNum != directionInt;
        }

        public static bool operator ==(Direction2I direction, int directionInt)
        {
            return direction._directionNum == directionInt;
        }

        private int _directionNum;

        public int DirectionNum
        {
            get => _directionNum;
            set => _directionNum = (((value % 4) + 4) % 4);
        }
        public Vector2I DirectionVec => Directions[DirectionNum];
        public Vector3 DirectionVecWorld => DirectionsWorld[DirectionNum];
    }
}