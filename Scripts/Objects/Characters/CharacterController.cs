using Godot;
using System;
using System.Collections.Generic;
using Dungeon.Tools;
using Godot.Collections;

public partial class CharacterController : Node
{
	public static readonly List<CharacterController> CharacterControllers = new();
	
	[Export] public Vector2 RotationSpeed = new(1f, 1f);
	[Export] public float MoveForce = 1350.0f;
	[Export] public float MoveMaxSpeed = 5.0f;
	
	public CharacterControllerInputs ControllerInputs { get; protected set; }
	public CharacterCharacteristics Characteristics { get; protected set; }
	protected CharacterDoll CharacterDoll;
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
		CharacterDoll = GetParent<CharacterDoll>();
		Target = CharacterDoll.GetNode<Node3D>("Target");
		StanceIndicator = CharacterDoll.GetNode<StanceIndicator>("StanceIndicator");
		ControllerInputs = GetNode<CharacterControllerInputs>("ControllerInputs");
		CharacterInfo = GetNode<CharacterInfo>("CharacterInfo");
		Characteristics = GetNode<CharacterCharacteristics>("Characteristics");
		InfoBar = CharacterDoll.GetNode<InfoBar3D>("InfoBar3D");
		
		base._Ready();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Multiplayer.GetUniqueId() != GetMultiplayerAuthority()) return;
		RotateInput = ControllerInputs.RotateInput;
		MoveInput = ControllerInputs.MoveInput;
		UpdateState(delta);
		base._PhysicsProcess(delta);
		ControllerInputs.PrimaryActionJustPressed = false;
		ControllerInputs.PrimaryActionJustReleased = false;
	}


	public virtual void UpdateState(double delta)
	{
		CharacterDoll.AngularVelocity = new Vector3(0.0f, RotateInput.Y * RotationSpeed.Y, 0.0f);
		UpdateMoveSpeed(delta);
	}

	void UpdateMoveSpeed(double delta)
	{
		var horizontalMoveSpeed = MoveInput * MoveForce;
		horizontalMoveSpeed.Y = 0.0f;
		horizontalMoveSpeed = horizontalMoveSpeed.Rotated(Vector3.Up, CharacterDoll.Rotation.Y);

		var horizontalLinearVelocity = new Vector3(CharacterDoll.LinearVelocity.X, 0.0f, CharacterDoll.LinearVelocity.Z);
		var horizontalSpeed = horizontalLinearVelocity.Length();
		var moveDot = horizontalMoveSpeed.Normalized().Dot(CharacterDoll.LinearVelocity.Normalized());
		var stopForce = (CharacterDoll.Mass * horizontalSpeed) * 1000.0f + 10000.0f;
		
		if ((horizontalMoveSpeed.Length() <= 0.0f || moveDot <= 0.0f) && horizontalSpeed > 0.0f)
		{
			CharacterDoll.ApplyCentralForce(-horizontalLinearVelocity.Normalized() * (stopForce * (float) delta * (1.0f - moveDot)));
		}
		if (horizontalSpeed >= MoveMaxSpeed)
		{
			CharacterDoll.ApplyCentralForce(-horizontalLinearVelocity.Normalized() * (stopForce * (float) delta));
		}

		CharacterDoll.ApplyCentralForce(horizontalMoveSpeed);
		//_characterBody.LinearVelocity += horizontalMoveSpeed * (float) delta;
	}

	public virtual void HitReceive()
	{
		
	}


	public virtual void CollectSyncData(Dictionary syncData)
	{
		syncData["GlobalPosition"] = CharacterDoll.GlobalPosition;
		syncData["GlobalRotation"] = CharacterDoll.GlobalRotation;
	}

	public virtual void ApplySyncData(Dictionary syncData)
	{
		CharacterDoll.GlobalPosition = (Vector3) syncData["GlobalPosition"];
		CharacterDoll.GlobalRotation = (Vector3) syncData["GlobalRotation"];
	}
}
