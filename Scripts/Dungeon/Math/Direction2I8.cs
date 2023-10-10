using Godot;

namespace Dungeon
{
    public class Direction2I8
    {
        private static readonly Vector2I[] Directions = {
            new Vector2I( 0, 1),
            new Vector2I( 1, 1),
            new Vector2I( 1, 0),
            new Vector2I( 1,-1),
            new Vector2I( 0,-1),
            new Vector2I(-1,-1),
            new Vector2I(-1, 0),
            new Vector2I(-1, 1)
        };

        public Direction2I8(int direction)
        {
            Direction = direction;
        }

        public static Vector2I operator *(Direction2I8 direction, int length)
        {
            return direction.DirectionVec * length;
        }
    
        public static Direction2I8 operator +(Direction2I8 direction, int side)
        {
            var newDirection = new Direction2I8(direction._direction + side);
            return newDirection;
        }
    
        public static bool operator !=(Direction2I8 direction, int directionInt)
        {
            return direction != null && direction._direction != directionInt;
        }

        public static bool operator ==(Direction2I8 direction, int directionInt)
        {
            return direction != null && direction._direction == directionInt;
        }

        private int _direction;
        public int Direction
        {
            get => _direction;
            set => _direction = (((value % 8) + 8) % 8);
        }
        public Vector2I DirectionVec => Directions[Direction];
    }
}