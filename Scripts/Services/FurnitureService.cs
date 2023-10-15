using System.Collections.Generic;
using System.Linq;
using DeepDungeon.Dungeon;
using Godot;

public partial class FurnitureService : Node
{
    public static FurnitureService Instance { get; private set; }

    class FurnitureSetChance
    {
        public ulong ID;
        public Vector2I Size;
        public int Chance;

        public FurnitureSetChance(ulong id, Vector2I size, int chance)
        {
            ID = id;
            Size = size;
            Chance = chance;
        }
    }
    
    private readonly Dictionary<MapCellTypeAdd, List<FurnitureSetChance>> _furnitureSetChances = new();
    
    private void AddSet(ulong id, Vector2I size, MapCellTypeAdd type, int chance)
    {
        _furnitureSetChances[type].Add(new FurnitureSetChance(id, size, chance));
    }
    
    public override void _Ready()
    {
        Instance = this;

        _furnitureSetChances[MapCellTypeAdd.Default] = new List<FurnitureSetChance>
        {
            new FurnitureSetChance(0, new Vector2I(0, 0), 100)
        };
        _furnitureSetChances[MapCellTypeAdd.Room] = new List<FurnitureSetChance>
        {
            new FurnitureSetChance(0, new Vector2I(0, 0), 100)
        };

        AddSet(0, new Vector2I(1, 1), MapCellTypeAdd.Room, 2);
        AddSet(Registry.GetId("FurnitureSet/Table2Chairs"), new Vector2I(4, 3), MapCellTypeAdd.Room, 2);
        AddSet(Registry.GetId("FurnitureSet/ClayPots1"), new Vector2I(2, 2), MapCellTypeAdd.Room, 1);
        AddSet(Registry.GetId("FurnitureSet/ClayPots2"), new Vector2I(2, 2), MapCellTypeAdd.Room, 1);
        AddSet(Registry.GetId("FurnitureSet/ClayPots3"), new Vector2I(1, 2), MapCellTypeAdd.Room, 1);
        AddSet(Registry.GetId("FurnitureSet/ClayPots4"), new Vector2I(1, 2), MapCellTypeAdd.Room, 1);
        AddSet(Registry.GetId("FurnitureSet/ClayPots5"), new Vector2I(2, 2), MapCellTypeAdd.Room, 1);
        AddSet(Registry.GetId("FurnitureSet/WeaponRack1"), new Vector2I(2, 2), MapCellTypeAdd.Room, 1);
        AddSet(Registry.GetId("FurnitureSet/WeaponRack2"), new Vector2I(2, 2), MapCellTypeAdd.Room, 1);
        
        base._Ready();
    }

    public ulong GetValidFurniture(Map map, MapCell cell, int randI)
    {
        var type = cell.MapCellTypeAdd;
        var validFurniture = _furnitureSetChances[type]
            .Where(fs =>
            {
                var furnitureRectI = new Rect2I(cell.Position, fs.Size);
                return map.IsAllCellsOfType(furnitureRectI, MapCellType.Empty) &&  map.IsAllCellsOfTypeAdd(furnitureRectI, type) && map.IsAllCellsFurnitureFree(furnitureRectI);
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
                return furniture.ID;
            }
        }
        
        return 0;
    }
    
}