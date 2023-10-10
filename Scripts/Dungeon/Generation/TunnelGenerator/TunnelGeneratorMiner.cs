using Dungeon;
using Godot;

namespace DeepDungeon.Dungeon.Generation.TunnelGenerator
{
    public class TunnelGeneratorMiner
    {
        public global::TunnelGenerator TunnelGenerator;
        public int Radius;
        
        public Vector2I Position;
        public Rect2I MineRect => new(
            new Vector2I(Position.X - Radius, Position.Y - Radius), 
            new Vector2I(Radius * 2, Radius * 2));
        public Vector2I NextPosition => Position + (Direction * (Radius * 2));
        public Rect2I NextMineRect => new(
            new Vector2I(NextPosition.X - Radius, NextPosition.Y - Radius), 
            new Vector2I(Radius * 2, Radius * 2));


        public Direction2I Direction;
        public int LifeTime;

        public TunnelGeneratorMiner(global::TunnelGenerator tunnelGenerator, Vector2I position, int radius, Direction2I direction, int lifeTime)
        { 
            TunnelGenerator = tunnelGenerator;
            Position = position;
            Radius = radius - 1;
            Direction = direction;
            LifeTime = lifeTime;
            
            if (CanMine()) TunnelGenerator.MapHolder.Map.SetCells(MineRect, MapCellType.Empty);
        }

        public bool Mine()
        {
            LifeTime--;
            
            var i = 0;
            var side = 1 + (2 * (TunnelGenerator.Random.Next() % 2));
            while (!CanMine())
            {
                Direction += side;
                i++;
                if (i > 4) return true;
            }
            Position = NextPosition;
            TunnelGenerator.MapHolder.Map.SetCells(MineRect, MapCellType.Empty);
            return LifeTime <= 0;
        }

        public bool CanMine()
        {
            var minDistance = 15;
            var mineRect = NextMineRect;
            if (Direction == 0 || Direction == 2)
            {
                mineRect = mineRect.GrowIndividual(minDistance, 0, minDistance, 0);
            }
            if (Direction == 1 || Direction == 3)
            {
                mineRect = mineRect.GrowIndividual(0, minDistance, 0, minDistance);
            }
            
            //TunnelGenerator.MapHolder.Map.SetCellsColor(mineRect, Colors.Red);
            return TunnelGenerator.MapHolder.Map.IsAllCellsOfType(mineRect, MapCellType.Wall);
        }
    }
}