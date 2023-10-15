using Godot;

public partial class SceneInstantiateService : Node
{
    public static SceneInstantiateService Instance { get; protected set;}
    private static ulong _newSceneId;
    
    public override void _Ready()
    {
        Instance = this;
        base._Ready();
    }
    
    public void SpawnByName(string name, Vector3 position, Vector3 rotation)
    {
        SpawnById(Registry.GetId(name), position, rotation);
    }
    
    public void SpawnById(ulong id, Vector3 position, Vector3 rotation)
    {
        var scene = Registry.GetScene(id).Instantiate();
        scene.Name = "Spawned" + _newSceneId++;
        if (scene is Node3D node3D)
        {
            node3D.Position = position;
            node3D.Rotation = rotation;
        }
        GetTree().Root.AddChild(scene);
    }
    
}