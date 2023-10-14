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
	public Node3D BodyAnimation { get; protected set; }
	public Node3D HeadAnimation { get; protected set; }
	
	protected List<Node3D> Limbs = new();
	
	public override void _Ready()
	{
		Body = GetNode<Node3D>("BodyAnimation/Body");
		Head = Body.GetNode<Node3D>("Neck/HeadAnimation/Head");
		RightArm = Body.GetNode<HumanoidArm>("RightShoulder/RightArm");
		LeftArm = Body.GetNode<HumanoidArm>("LeftShoulder/LeftArm");
		RightLeg = GetNode<Node3D>("RightLeg");
		LeftLeg = GetNode<Node3D>("LeftLeg");
		BodyAnimation = GetNode<Node3D>("BodyAnimation");
		HeadAnimation = GetNode<Node3D>("BodyAnimation/Body/Neck/HeadAnimation");
		
		Limbs.Add(Body);
		Limbs.Add(Head);
		Limbs.Add(RightArm);
		Limbs.Add(LeftArm);
		Limbs.Add(RightLeg);
		Limbs.Add(LeftLeg);
		Limbs.Add(BodyAnimation);
		Limbs.Add(HeadAnimation);
		
        SynchronizationInterpolators.Add(Body, new SynchronizationInterpolator(Body));
        SynchronizationInterpolators.Add(Head, new SynchronizationInterpolator(Head));
        SynchronizationInterpolators.Add(RightArm, new SynchronizationInterpolator(RightArm));
        SynchronizationInterpolators.Add(LeftArm, new SynchronizationInterpolator(LeftArm));
        SynchronizationInterpolators.Add(RightLeg, new SynchronizationInterpolator(RightLeg));
        SynchronizationInterpolators.Add(LeftLeg, new SynchronizationInterpolator(LeftLeg));
        SynchronizationInterpolators.Add(BodyAnimation, new SynchronizationInterpolator(BodyAnimation));
        SynchronizationInterpolators.Add(HeadAnimation, new SynchronizationInterpolator(HeadAnimation));
        
        base._Ready();
	}

	public override void CollectSyncData(Dictionary syncData)
	{
		foreach (var limb in Limbs)
		{
			syncData["Doll" + limb.Name + "Position"] = limb.Position;
			syncData["Doll" + limb.Name + "Rotation"] = limb.Quaternion;
		}
	}

	private void ApplyLimbData(Node3D limb, Vector3? position, Quaternion? rotation)
	{
		SynchronizationInterpolators[limb].Next(
				position,
				rotation,
				Multiplayer.GetRemoteSenderId()
			);
	}

	public override void ApplySyncData(Dictionary syncData)
	{
		foreach (var limb in Limbs)
		{
			Vector3? position = null;
			if (syncData.ContainsKey("Doll" + limb.Name + "Position"))
				position = (Vector3) syncData["Doll" + limb.Name + "Position"];
			Quaternion? rotation = null;
			if (syncData.ContainsKey("Doll" + limb.Name + "Rotation"))
				rotation = (Quaternion) syncData["Doll" + limb.Name + "Rotation"];
			
			ApplyLimbData(limb, position, rotation);
		}
	}
}
