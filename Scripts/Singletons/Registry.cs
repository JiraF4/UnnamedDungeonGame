using Godot;
using Godot.Collections;

public partial class Registry : Node
{
    private static Registry _instance;
    
    Dictionary<ulong, PackedScene> _packedScenes = new();
    Dictionary<string, ulong> _packedScenesIds = new();

    public static ulong GetId(string name)
    {
        return _instance._packedScenesIds[name];
    }
    
    public static PackedScene GetScene(ulong id)
    {
        return _instance._packedScenes[id];
    }
    
    private void AddScene(string path)
    {
        var name = path.Replace("res://Scenes/", "").Replace(".tscn", "");
        var id = (ulong) _packedScenes.Count;
        _packedScenesIds.Add(name, id);
        _packedScenes.Add(id, (PackedScene) ResourceLoader.Load(path));
    }
    
    public override void _Ready()
    {
        _instance = this;
        
        // Items
        AddScene("res://Scenes/Items/Usable/Potion.tscn");
        AddScene("res://Scenes/Items/Weapon/Dagger.tscn");
        AddScene("res://Scenes/Items/Shield/Shield.tscn");
        
        // Furniture
        AddScene("res://Scenes/Furniture/Chest.tscn");
        AddScene("res://Scenes/Furniture/Chair.tscn");
        AddScene("res://Scenes/Furniture/Table.tscn");
        
        // Furniture set
        AddScene("res://Scenes/FurnitureSet/Table2Chairs.tscn");
        AddScene("res://Scenes/FurnitureSet/ClayPots1.tscn");
        AddScene("res://Scenes/FurnitureSet/ClayPots2.tscn");
        AddScene("res://Scenes/FurnitureSet/ClayPots3.tscn");
        AddScene("res://Scenes/FurnitureSet/ClayPots4.tscn");
        AddScene("res://Scenes/FurnitureSet/ClayPots5.tscn");
        AddScene("res://Scenes/FurnitureSet/WeaponRack1.tscn");
        AddScene("res://Scenes/FurnitureSet/WeaponRack2.tscn");
        
        base._Ready();
    }
}