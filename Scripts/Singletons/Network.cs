using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using Godot;

public partial class Network : Node
{ 
    public static Network Instance;
    
    // server unique ID 
    private static ulong _suid = 0;
    public static ulong GetNextSUID()
    {
        return _suid++;
    }
    
    // global settings
    public const int MaxTPS = 40;
    private const int Port = 3333;
    private const int MaxPlayers = 2;

    public static bool IsServer { get; protected set; }
    public static bool IsConnected { get; protected set; }

    public static readonly ENetMultiplayerPeer Peer = new();
    
    public override void _Ready()
    {
        Instance = this;
        IsServer = ( OS.GetCmdlineArgs().Contains("--server"));

        if (OS.GetCmdlineArgs().Contains("--host"))
        {
            IsServer = true;
            var processStartInfo = new ProcessStartInfo
            {
                FileName = OS.GetExecutablePath(),
                Arguments = "--path \"G:/GodotProject/DeepDungeon\" --autoConnect",
                WindowStyle = ProcessWindowStyle.Minimized,
            };
            var process = Process.Start(processStartInfo);
        }
        
        
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;
        Multiplayer.ServerDisconnected += ServerDisconnected;
        
        if (OS.GetCmdlineArgs().Contains("--autoConnect")) 
            Start("localhost");
        
        if (!IsServer) return;
        Start("");
    }

    private void ServerDisconnected()
    {
        GD.Print("ServerDisconnected");
    }

    private void ConnectionFailed()
    {
        GD.Print("ConnectionFailed");
    }

    private void ConnectedToServer()
    {
        GD.Print("ConnectedToServer");
    }
    
    // base network
    public void Start(string ip)
    {
        GetTree().ChangeSceneToFile("res://Scenes/World.tscn");
        if (IsServer)
        {
            //RenderingServer.RenderLoopEnabled = false;
            Engine.MaxFps = MaxTPS;
            GD.Print("Run server");
            var error = Peer.CreateServer(Port, MaxPlayers);
            GD.Print(error);
            DisplayServer.WindowSetTitle("Dungeon Server");
        } else {
            GD.Print("Run client");
            var error = Peer.CreateClient(ip, Port);
            GD.Print(error);
            GD.Print("Connecting: " + ip);
        }
        Peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = Peer;
    }
    public void PeerConnected(long peerId)
    {
        GD.Print("Peer " + peerId + " connected");
        if (IsServer)
        {
            GetTree().Root.GetNode<MapHolder>("World/MapHolder").GenerateMapRemote(peerId);
        }
        else
        {
            IsConnected = true;
        }
        PlayersService.Instance.SpawnPlayersServer(new Vector3(64.0f, 0, 64.0f));
    }
    public void PeerDisconnected(long peerId)
    {
        GD.Print("Peer " + peerId + " disconnected");
    }
    
    // players scenes
    public void SpawnExistPlayer(IEnumerable<int> players)
    {
        
    }
    public void SpawnPlayer(int id)
    {
        
    }
    public void RemovePlayer(int id)
    {
        
    }

}