using Godot;
using System;
using DeepDungeon.Dungeon;

public partial class MapHolder : Node
{
	[Export] public MapGenerator MapGenerator;
	[Export] public MapMeshBaker MapMeshBaker;
	[Export] public Mesh TestMesh;
	[Export] public Material TestMaterial;
	
	public readonly Map Map;

	public MapHolder()
	{
		Map = new Map(this, 128, 128);
	}

	public override void _Ready()
	{
		MapGenerator.MapHolder = this;
		MapMeshBaker.MapHolder = this;
		MapGenerator.Generate();
		Map.ReBakeMeshes();
	}
}
