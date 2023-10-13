using System.Collections.Generic;
using Godot;

public partial class PlayersService : Node
{
    public static PlayersService Instance { get; private set; }
    [Export] public PackedScene playerScene;
    
    protected Dictionary<long, bool> ExistedPlayers = new();
    
    public override void _Ready()
    {
        Instance = this;
        base._Ready();
    }
    
    public void SpawnPlayersServer(Vector3 position)
    {
        if (!Network.IsServer) return;

        foreach (var peerId in Multiplayer.GetPeers())
        {
            Rpc(nameof(SpawnNewPlayer), peerId, position);    
        }
    }
    
    [Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SpawnNewPlayer(long peerId, Vector3 position)
    {
        if (ExistedPlayers.ContainsKey(peerId)) return;
        ExistedPlayers[peerId] = true;
        
        // TODO: More elegant
        var newPlayer = playerScene.Instantiate<CharacterDoll>();
        newPlayer.Name = "Player" + peerId;
        GD.Print("SpawnNewPlayer: " + newPlayer.Name);
        newPlayer.SetMultiplayerAuthority((int) peerId);
        GetTree().Root.GetNode("World").AddChild(newPlayer);
        newPlayer.GlobalPosition = position;
        newPlayer.SetAnimationActive(peerId == Multiplayer.MultiplayerPeer.GetUniqueId());
        if (peerId == Multiplayer.MultiplayerPeer.GetUniqueId())
        {
            GetTree().Root.GetNode<PlayerController>("World/PlayerController").GetInput(newPlayer.GetPath());
        }
            
    }
}