using Godot;
using System;
using DeepDungeon.Dungeon;

public partial class MapHolder : Node
{
	[Export] public TextureRect DebugTextureRect;
	[Export] public MapGenerator MapGenerator;
	[Export] public MapMeshBaker MapMeshBaker;
	[Export] public Mesh TestMesh;
	[Export] public Material TestMaterial;
	
	public readonly Map Map;
	public int Seed = 0; 

	public MapHolder()
	{
		Map = new Map(this, 128, 128);
	}

	public override void _Ready()
	{
		MapGenerator.MapHolder = this;
		MapMeshBaker.MapHolder = this;

		if (Network.IsServer) GenerateMap(7);
	}

	public void GenerateMapRemote(long peerID)
	{
		RpcId(peerID, nameof(GenerateMap), Seed);
		Console.WriteLine("Generating");
	}
	
	[Rpc(CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void GenerateMap(int seed)
	{
		Seed = seed;
		MapGenerator.Random = new Random(Seed);
		MapGenerator.Generate();
		Map.ReBakeMeshes();
	}
}
