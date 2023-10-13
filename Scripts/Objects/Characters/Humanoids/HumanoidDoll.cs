using Godot;
using System;
using Godot.Collections;

public partial class HumanoidDoll : CharacterDoll
{
	public Node3D Body { get; protected set; }
	public Node3D Head { get; protected set; }
	public HumanoidArm RightArm { get; protected set; }
	public HumanoidArm LeftArm { get; protected set; }
	public Node3D RightLeg { get; protected set; }
	public Node3D LeftLeg { get; protected set; }
	public Vector3 LeftShoulderPosition { get; protected set; }
	public Vector3 RightShoulderPosition { get; protected set; }
	
	public override void _Ready()
	{
		Body = GetNode<Node3D>("BodyAnimation/Body");
		Head = Body.GetNode<Node3D>("Neck/HeadAnimation/Head");
		RightArm = Body.GetNode<HumanoidArm>("RightShoulder/RightArm");
		LeftArm = Body.GetNode<HumanoidArm>("LeftShoulder/LeftArm");
		RightLeg = GetNode<Node3D>("RightLeg");
		LeftLeg = GetNode<Node3D>("LeftLeg");

        LeftShoulderPosition = new Vector3(0.3f, 0.2f, 0.0f);
        RightShoulderPosition = new Vector3(-0.3f, 0.2f, 0.0f);
        
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
		
		syncData["DollBodyRotation"] = Body.Rotation;
		syncData["DollHeadRotation"] = Head.Rotation;
		syncData["DollRightArmRotation"] = RightArm.Rotation;
		syncData["DollLeftArmRotation"] = LeftArm.Rotation;
		syncData["DollRightLegRotation"] = RightLeg.Rotation;
		syncData["DollLeftLegRotation"] = LeftLeg.Rotation;
	}

	public override void ApplySyncData(Dictionary syncData)
	{
		Body.Position = (Vector3) syncData["DollBodyPosition"];
		Head.Position = (Vector3) syncData["DollHeadPosition"];
		RightArm.Position = (Vector3) syncData["DollRightArmPosition"];
		LeftArm.Position = (Vector3) syncData["DollLeftArmPosition"];
		RightLeg.Position = (Vector3) syncData["DollRightLegPosition"];
		LeftLeg.Position = (Vector3) syncData["DollLeftLegPosition"];
		
		Body.Rotation = (Vector3) syncData["DollBodyRotation"];
		Head.Rotation = (Vector3) syncData["DollHeadRotation"];
		RightArm.Rotation = (Vector3) syncData["DollRightArmRotation"];
		LeftArm.Rotation = (Vector3) syncData["DollLeftArmRotation"];
		RightLeg.Rotation = (Vector3) syncData["DollRightLegRotation"];
		LeftLeg.Rotation = (Vector3) syncData["DollLeftLegRotation"];
	}
}
