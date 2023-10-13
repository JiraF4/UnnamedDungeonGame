using Godot;
using System;

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
}
