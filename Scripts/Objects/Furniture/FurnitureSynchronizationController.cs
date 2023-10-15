using System;
using Godot;
using Godot.Collections;

public partial class FurnitureSynchronizationController : SynchronizationController
{
    protected Furniture Furniture;
    protected ulong temporallyAuthorityTime;

    public override void _Process(double delta)
    {
        if (temporallyAuthorityTime + 350 < Time.GetTicksMsec())
        {
            Furniture.SetMultiplayerAuthority(1);
            Furniture.ResetInterpolator();
        }
            
        base._Process(delta);
    }

    public override void _Ready()
    {
        Furniture = GetParent<Furniture>();
        base._Ready();
    }
    
    protected override void CollectSyncData(Dictionary syncData)
    {
        Furniture.CollectSyncData(syncData);
        base.CollectSyncData(syncData);
    }


    protected override void ApplySyncData(Dictionary syncData)
    {
        Furniture.ApplySyncData(syncData);
        base.ApplySyncData(syncData);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable, CallLocal = true)]
    public void TemporallyAuthorityRemote(int peerId)
    {
        temporallyAuthorityTime = Time.GetTicksMsec();
        Furniture.SetMultiplayerAuthority(peerId);
    }
    
    public void TemporallyAuthority(int peerId)
    {
        if (temporallyAuthorityTime + 100 < Time.GetTicksMsec())
            Rpc(nameof(TemporallyAuthorityRemote), peerId);
    }
}