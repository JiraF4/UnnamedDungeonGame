using System.Collections.Generic;
using Godot;

public partial class PlayersService : Node
{
    public static PlayersService Instance { get; private set; }
    public PackedScene playerScene;
    
    public readonly Dictionary<long, CharacterDoll> ExistedPlayers = new();
    
    public override void _Ready()
    {
        playerScene = (PackedScene) ResourceLoader.Load("res://Scenes/Characters/Humanoid.tscn");
        Instance = this;
        base._Ready();
    }
    
    public void SpawnPlayersServer(Vector3 position)
    {
        if (!Network.IsServer) return;

        foreach (var peerId in Multiplayer.GetPeers())
        {
            SpawnNewPlayer(peerId, position);
        }
    }
    
    [Rpc(CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SpawnNewPlayer(long peerId, Vector3 position)
    {
        if (ExistedPlayers.ContainsKey(peerId))
        {
            if (Network.IsServer) Rpc(nameof(SpawnNewPlayer), peerId, position);
            return;
        }
        
        // TODO: More elegant
        var newPlayer = playerScene.Instantiate<CharacterDoll>();
        newPlayer.Name = "Player" + peerId;
        GD.Print("SpawnNewPlayer: " + newPlayer.Name);
        newPlayer.SetMultiplayerAuthority((int) peerId);
        GetTree().Root.GetNode("World").AddChild(newPlayer);
        newPlayer.GlobalPosition = position;
        if (peerId == Multiplayer.MultiplayerPeer.GetUniqueId())
        {
            GetTree().Root.GetNode<PlayerController>("World/PlayerController").GetInput(newPlayer.GetPath());
        }
        ExistedPlayers[peerId] = newPlayer;
        
        if (Network.IsServer) Rpc(nameof(SpawnNewPlayer), peerId, position);
    }
}