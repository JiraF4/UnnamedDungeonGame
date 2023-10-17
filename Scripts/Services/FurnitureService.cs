using System.Collections.Generic;
using System.Linq;
using DeepDungeon.Dungeon;
using Godot;

public partial class FurnitureService : Node
{
    public static FurnitureService Instance { get; private set; }

    public class FurnitureSetChance
    {
        public readonly ulong ID;
        public readonly Vector2I Size;
        public readonly int Chance;
        public readonly int Rotation;
        public Vector2I WallCheck;

        public FurnitureSetChance(ulong id, Vector2I size, int chance, int rotation, Vector2I wallCheck)
        {
            ID = id;
            Size = size;
            Chance = chance;
            Rotation = rotation;
            WallCheck = wallCheck;
        }
    }
    
    private readonly Dictionary<MapCellTypeAdd, List<FurnitureSetChance>> _furnitureSetChances = new();
    
    private void AddSet(ulong id, Vector2I size, MapCellTypeAdd type, int chance, bool addRotations, Vector2I wallCheck)
    {
        if (addRotations)
        {
            _furnitureSetChances[type].Add(new FurnitureSetChance(id, size, chance, 0, wallCheck));
            _furnitureSetChances[type].Add(new FurnitureSetChance(id, new Vector2I(size.Y, size.X), chance, 1, new Vector2I(wallCheck.Y, -wallCheck.X)));
            _furnitureSetChances[type].Add(new FurnitureSetChance(id, size, chance, 2, new Vector2I(-wallCheck.X, -wallCheck.Y)));
            _furnitureSetChances[type].Add(new FurnitureSetChance(id, new Vector2I(size.Y, size.X), chance, 3, new Vector2I(-wallCheck.Y, wallCheck.X)));
        }
        else _furnitureSetChances[type].Add(new FurnitureSetChance(id, size, chance*4, 0, wallCheck));
    }
    
    public override void _Ready()
    {
        Instance = this;

        _furnitureSetChances[MapCellTypeAdd.Default] = new List<FurnitureSetChance>
        {
            new(0, new Vector2I(0, 0), 500, 0, new Vector2I(0, 0))
        };
        _furnitureSetChances[MapCellTypeAdd.Room] = new List<FurnitureSetChance>
        {
            new(0, new Vector2I(0, 0), 500, 0, new Vector2I(0, 0))
        };

        AddSet(Registry.GetId("FurnitureSet/Table2Chairs"), new Vector2I(4, 3), MapCellTypeAdd.Room, 2, true, new Vector2I(0, 0));
        AddSet(Registry.GetId("FurnitureSet/ClayPots1"), new Vector2I(2, 2), MapCellTypeAdd.Room, 100, true, new Vector2I(-1, 0));
        AddSet(Registry.GetId("FurnitureSet/ClayPots2"), new Vector2I(2, 2), MapCellTypeAdd.Room, 100, true, new Vector2I(-1, 0));
        AddSet(Registry.GetId("FurnitureSet/ClayPots3"), new Vector2I(1, 2), MapCellTypeAdd.Room, 100, true, new Vector2I(-1, 0));
        AddSet(Registry.GetId("FurnitureSet/ClayPots4"), new Vector2I(1, 2), MapCellTypeAdd.Room, 100, true, new Vector2I(-1, 0));
        AddSet(Registry.GetId("FurnitureSet/ClayPots5"), new Vector2I(2, 2), MapCellTypeAdd.Room, 100, true, new Vector2I(-1, 0));
        AddSet(Registry.GetId("FurnitureSet/WeaponRack1"), new Vector2I(2, 2), MapCellTypeAdd.Room, 1, true, new Vector2I(0, 0));
        AddSet(Registry.GetId("FurnitureSet/WeaponRack2"), new Vector2I(2, 2), MapCellTypeAdd.Room, 1, true, new Vector2I(0, 0));
        
        base._Ready();
    }

    public FurnitureSetChance GetValidFurniture(Map map, MapCell cell, int randI)
    {
        var type = cell.MapCellTypeAdd;
        var validFurniture = _furnitureSetChances[type]
            .Where(fs =>
            {
                var furnitureRectI = new Rect2I(cell.Position, fs.Size);
                if (!map.IsAllCellsOfType(furnitureRectI, MapCellType.Empty)) return false;
                if (!map.IsAllCellsOfTypeAdd(furnitureRectI, type)) return false;
                if (!map.IsAllCellsFurnitureFree(furnitureRectI)) return false;
                if (fs.WallCheck.X != 0)
                {
                    var wallCheckRectI = new Rect2I(cell.Position + new Vector2I(fs.WallCheck.X * (fs.WallCheck.X > 0 ? fs.Size.X : 1), 0), new Vector2I(1, fs.Size.Y));
                    
                    if (!map.IsAllCellsOfType(wallCheckRectI, MapCellType.Wall)) return false;
                    map.SetCellsColor(wallCheckRectI, new Color(1.0f, 0.0f, 0.0f, 1.0f));
                }
                if (fs.WallCheck.Y != 0)
                {
                    var wallCheckRectI = new Rect2I(cell.Position + new Vector2I(0, fs.WallCheck.Y * (fs.WallCheck.Y > 0 ? fs.Size.Y : 1)), new Vector2I(fs.Size.X, 1));
                    
                    if (!map.IsAllCellsOfType(wallCheckRectI, MapCellType.Wall)) return false;
                    map.SetCellsColor(wallCheckRectI, new Color(1.0f, 0.0f, 0.0f, 1.0f));
                }
                return true;
            }).ToList();

        var maxChance = validFurniture.Sum(fs => fs.Chance);
        randI %= maxChance;
        var chance = 0;
        foreach (var furniture in validFurniture)
        {
            chance += furniture.Chance;
            if (chance > randI)
            {
                var furnitureRectI = new Rect2I(cell.Position, furniture.Size);
                map.SetCellsFurniture(furnitureRectI);
                return furniture;
            }
        }
        
        return new FurnitureSetChance(0, new Vector2I(1, 1), 0, 0, new Vector2I());
    }
    
}