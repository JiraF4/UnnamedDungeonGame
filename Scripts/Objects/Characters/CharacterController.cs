using Godot;
using System;
using System.Collections.Generic;
using Dungeon.Tools;

public partial class CharacterController : Node
{
	public static readonly List<CharacterController> CharacterControllers = new();
	
	[Export] public Vector2 RotationSpeed = new(1f, 1f);
	[Export] public float MoveForce = 1350.0f;
	[Export] public float MoveMaxSpeed = 5.0f;
	
	public CharacterControllerInputs ControllerInputs { get; protected set; }
	public CharacterCharacteristics Characteristics { get; protected set; }
	protected RigidBody3D CharacterBody;
	public StanceIndicator StanceIndicator;
	public InfoBar3D InfoBar { get; protected set; }
	
	protected Vector3 RotateInput;
	protected Vector3 MoveInput;
	public bool Dead { get; protected set; } = false;

	public Node3D Target;
	
	public CharacterInfo CharacterInfo { get; protected set; }
	
	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public override void _Ready()
	{
		CharacterControllers.Add(this);
		CharacterBody = GetParent<RigidBody3D>();
		Target = CharacterBody.GetNode<Node3D>("Target");
		StanceIndicator = CharacterBody.GetNode<StanceIndicator>("StanceIndicator");
		ControllerInputs = GetNode<CharacterControllerInputs>("ControllerInputs");
		CharacterInfo = GetNode<CharacterInfo>("CharacterInfo");
		Characteristics = GetNode<CharacterCharacteristics>("Characteristics");
		InfoBar = CharacterBody.GetNode<InfoBar3D>("InfoBar3D");
		
		base._Ready();
	}

	public override void _PhysicsProcess(double delta)
	{
		RotateInput = ControllerInputs.RotateInput;
		MoveInput = ControllerInputs.MoveInput;
		UpdateState(delta);
		base._PhysicsProcess(delta);
		ControllerInputs.PrimaryActionJustPressed = false;
		ControllerInputs.PrimaryActionJustReleased = false;
	}


	public virtual void UpdateState(double delta)
	{
		CharacterBody.AngularVelocity = new Vector3(0.0f, RotateInput.Y * RotationSpeed.Y, 0.0f);
		UpdateMoveSpeed(delta);
	}

	void UpdateMoveSpeed(double delta)
	{
		var horizontalMoveSpeed = MoveInput * MoveForce;
		horizontalMoveSpeed.Y = 0.0f;
		horizontalMoveSpeed = horizontalMoveSpeed.Rotated(Vector3.Up, CharacterBody.Rotation.Y);

		var horizontalLinearVelocity = new Vector3(CharacterBody.LinearVelocity.X, 0.0f, CharacterBody.LinearVelocity.Z);
		var horizontalSpeed = horizontalLinearVelocity.Length();
		var moveDot = horizontalMoveSpeed.Normalized().Dot(CharacterBody.LinearVelocity.Normalized());
		var stopForce = (CharacterBody.Mass * horizontalSpeed) * 1000.0f + 10000.0f;
		
		if ((horizontalMoveSpeed.Length() <= 0.0f || moveDot <= 0.0f) && horizontalSpeed > 0.0f)
		{
			CharacterBody.ApplyCentralForce(-horizontalLinearVelocity.Normalized() * (stopForce * (float) delta * (1.0f - moveDot)));
		}
		if (horizontalSpeed >= MoveMaxSpeed)
		{
			CharacterBody.ApplyCentralForce(-horizontalLinearVelocity.Normalized() * (stopForce * (float) delta));
		}

		CharacterBody.ApplyCentralForce(horizontalMoveSpeed);
		//_characterBody.LinearVelocity += horizontalMoveSpeed * (float) delta;
	}

	public virtual void HitReceive()
	{
		
	}
}
