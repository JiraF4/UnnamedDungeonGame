using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public partial class HumanoidDoll : CharacterDoll
{
	public Node3D Body { get; protected set; }
	public Node3D Head { get; protected set; }
	public HumanoidArm RightArm { get; protected set; }
	public HumanoidArm LeftArm { get; protected set; }
	public Node3D RightLeg { get; protected set; }
	public Node3D LeftLeg { get; protected set; }

	public Vector3 BodyRotation;
	
	protected HumanoidSynchronizationInterpolator SynchronizationInterpolator;
	
	public override void _Ready()
	{
		Body = GetNode<Node3D>("BodyAnimation/Body");
		Head = Body.GetNode<Node3D>("Neck/HeadAnimation/Head");
		RightArm = Body.GetNode<HumanoidArm>("RightShoulder/RightArm");
		LeftArm = Body.GetNode<HumanoidArm>("LeftShoulder/LeftArm");
		RightLeg = GetNode<Node3D>("RightLeg");
		LeftLeg = GetNode<Node3D>("LeftLeg");
		SynchronizationInterpolator = new HumanoidSynchronizationInterpolator(this);
		
        base._Ready();
	}

	public override void _Process(double delta)
	{
		DebugInfo.AddLine("Rotation: " + Rotation);
		
		if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId()) SynchronizationInterpolator.Interpolate(delta);
		
		var headRotation = Head.Rotation;
		var bodyRotation = Body.Rotation;
		
		headRotation.X = BodyRotation.X * 0.6f;
		bodyRotation.X = BodyRotation.X * 0.4f;
		headRotation.Y = BodyRotation.Y * 0.5f;
		bodyRotation.Y = BodyRotation.Y * 0.5f;
			
		Head.Rotation = headRotation;
		Body.Rotation = bodyRotation;
		
		base._Process(delta);
	}
	
	public override void CollectSyncData(Dictionary syncData)
	{
		syncData["BodyRotation"] = BodyRotation;
		syncData["LinearVelocity"] = LinearVelocity;
		syncData["Rotation"] = Quaternion;
		syncData["Position"] = Position;
		Controller.CollectSyncData(syncData);
		CharacterInfo.CollectSyncData(syncData);
	}

	public override void ApplySyncData(Dictionary syncData)
	{
		SynchronizationInterpolator.Next(syncData, Multiplayer.GetRemoteSenderId());
		Controller.ApplySyncData(syncData);
		CharacterInfo.ApplySyncData(syncData);
	}
}




























