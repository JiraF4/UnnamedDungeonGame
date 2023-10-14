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
		syncData["DollBodyPosition"] = Body.Position;
		syncData["DollHeadPosition"] = Head.Position;
		syncData["DollRightArmPosition"] = RightArm.Position;
		syncData["DollLeftArmPosition"] = LeftArm.Position;
		syncData["DollRightLegPosition"] = RightLeg.Position;
		syncData["DollLeftLegPosition"] = LeftLeg.Position;
		syncData["DollBodyAnimationPosition"] = BodyAnimation.Position;
		syncData["DollHeadAnimationPosition"] = HeadAnimation.Position;
		
		syncData["DollBodyRotation"] = Body.Rotation;
		syncData["DollHeadRotation"] = Head.Rotation;
		syncData["DollRightArmRotation"] = RightArm.Rotation;
		syncData["DollLeftArmRotation"] = LeftArm.Rotation;
		syncData["DollRightLegRotation"] = RightLeg.Rotation;
		syncData["DollLeftLegRotation"] = LeftLeg.Rotation;
		syncData["DollBodyAnimationRotation"] = BodyAnimation.Rotation;
		syncData["DollHeadAnimationRotation"] = HeadAnimation.Rotation;
	}

	private void ApplyLimbData(Node3D limb, float time)
	{
		SynchronizationInterpolators[limb].Next(
				limb.Position,
				limb.Quaternion,
				time
			);
	}

	public override void ApplySyncData(Dictionary syncData)
	{
		if (syncData.ContainsKey("DollBodyPosition")) Body.Position = (Vector3) syncData["DollBodyPosition"];
		if (syncData.ContainsKey("DollHeadPosition")) Head.Position = (Vector3) syncData["DollHeadPosition"];
		if (syncData.ContainsKey("DollRightArmPosition")) RightArm.Position = (Vector3) syncData["DollRightArmPosition"];
		if (syncData.ContainsKey("DollLeftArmPosition")) LeftArm.Position = (Vector3) syncData["DollLeftArmPosition"];
		if (syncData.ContainsKey("DollRightLegPosition")) RightLeg.Position = (Vector3) syncData["DollRightLegPosition"];
		if (syncData.ContainsKey("DollLeftLegPosition")) LeftLeg.Position = (Vector3) syncData["DollLeftLegPosition"];
		if (syncData.ContainsKey("DollBodyAnimationPosition")) BodyAnimation.Position = (Vector3) syncData["DollBodyAnimationPosition"];
		if (syncData.ContainsKey("DollHeadAnimationPosition")) HeadAnimation.Position = (Vector3) syncData["DollHeadAnimationPosition"];
		
		if (syncData.ContainsKey("DollBodyRotation")) Body.Rotation = (Vector3) syncData["DollBodyRotation"];
		if (syncData.ContainsKey("DollHeadRotation")) Head.Rotation = (Vector3) syncData["DollHeadRotation"];
		if (syncData.ContainsKey("DollRightArmRotation")) RightArm.Rotation = (Vector3) syncData["DollRightArmRotation"];
		if (syncData.ContainsKey("DollLeftArmRotation")) LeftArm.Rotation = (Vector3) syncData["DollLeftArmRotation"];
		if (syncData.ContainsKey("DollRightLegRotation")) RightLeg.Rotation = (Vector3) syncData["DollRightLegRotation"];
		if (syncData.ContainsKey("DollLeftLegRotation")) LeftLeg.Rotation = (Vector3) syncData["DollLeftLegRotation"];
		if (syncData.ContainsKey("DollBodyAnimationRotation")) BodyAnimation.Rotation = (Vector3) syncData["DollBodyAnimationRotation"];
		if (syncData.ContainsKey("DollHeadAnimationRotation")) HeadAnimation.Rotation = (Vector3) syncData["DollHeadAnimationRotation"];
		
		ApplyLimbData(Body, (float) syncData["SyncDelay"]);
		ApplyLimbData(Head, (float) syncData["SyncDelay"]);
		ApplyLimbData(LeftArm, (float) syncData["SyncDelay"]);
		ApplyLimbData(RightArm, (float) syncData["SyncDelay"]);
		ApplyLimbData(LeftLeg, (float) syncData["SyncDelay"]);
		ApplyLimbData(RightLeg, (float) syncData["SyncDelay"]);
		ApplyLimbData(BodyAnimation, (float) syncData["SyncDelay"]);
		ApplyLimbData(HeadAnimation, (float) syncData["SyncDelay"]);
	}
}
