using Godot;
using System;

public partial class CharacterController : Node
{
	[Export] public Vector2 RotationSpeed = new(1f, 0.03f);
	[Export] public float MoveForce = 1350.0f;
	[Export] public float MoveMaxSpeed = 5.0f;

	[Export] public CharacterControllerInputs CharacterControllerInputs { get; protected set; }
	protected RigidBody3D CharacterBody;

	protected Vector3 RotateInput;
	protected Vector3 MoveInput;

	public override void _Process(double delta)
	{
		DebugInfo.AddLine("MoveSpeed: " + CharacterBody.LinearVelocity.Length().ToString());
		base._Process(delta);
	}

	public override void _Ready()
	{
		CharacterBody = GetParent<RigidBody3D>();
		CharacterControllerInputs = GetNode<CharacterControllerInputs>("CharacterControllerInputs");
		base._Ready();
	}

	public override void _PhysicsProcess(double delta)
	{
		RotateInput = CharacterControllerInputs.RotateInput;
		MoveInput = CharacterControllerInputs.MoveInput;
		UpdateState(delta);
		base._PhysicsProcess(delta);
		CharacterControllerInputs.PrimaryActionJustPressed = false;
		CharacterControllerInputs.PrimaryActionJustReleased = false;
	}

	public virtual void UpdateState(double delta)
	{
		CharacterBody.AngularVelocity = new Vector3(0.0f, CharacterControllerInputs.RotateInput.Y * RotationSpeed.Y, 0.0f);
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
}
