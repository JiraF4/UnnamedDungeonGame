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
		syncData["DollBodyPosition"] = Body.GlobalPosition;
		syncData["DollHeadPosition"] = Head.GlobalPosition;
		syncData["DollRightArmPosition"] = RightArm.GlobalPosition;
		syncData["DollLeftArmPosition"] = LeftArm.GlobalPosition;
		syncData["DollRightLegPosition"] = RightLeg.GlobalPosition;
		syncData["DollLeftLegPosition"] = LeftLeg.GlobalPosition;
		
		syncData["DollBodyRotation"] = Body.GlobalRotation;
		syncData["DollHeadRotation"] = Head.GlobalRotation;
		syncData["DollRightArmRotation"] = RightArm.GlobalRotation;
		syncData["DollLeftArmRotation"] = LeftArm.GlobalRotation;
		syncData["DollRightLegRotation"] = RightLeg.GlobalRotation;
		syncData["DollLeftLegRotation"] = LeftLeg.GlobalRotation;
	}

	public override void ApplySyncData(Dictionary syncData)
	{
		Body.GlobalPosition = (Vector3) syncData["DollBodyPosition"];
		Head.GlobalPosition = (Vector3) syncData["DollHeadPosition"];
		RightArm.GlobalPosition = (Vector3) syncData["DollRightArmPosition"];
		LeftArm.GlobalPosition = (Vector3) syncData["DollLeftArmPosition"];
		RightLeg.GlobalPosition = (Vector3) syncData["DollRightLegPosition"];
		LeftLeg.GlobalPosition = (Vector3) syncData["DollLeftLegPosition"];
		
		Body.GlobalRotation = (Vector3) syncData["DollBodyRotation"];
		Head.GlobalRotation = (Vector3) syncData["DollHeadRotation"];
		RightArm.GlobalRotation = (Vector3) syncData["DollRightArmRotation"];
		LeftArm.GlobalRotation = (Vector3) syncData["DollLeftArmRotation"];
		RightLeg.GlobalRotation = (Vector3) syncData["DollRightLegRotation"];
		LeftLeg.GlobalRotation = (Vector3) syncData["DollLeftLegRotation"];
	}
}
