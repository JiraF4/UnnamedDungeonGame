using Godot;
using System;

public enum HumanoidArmState {
	Idle,
	Hold,
	PrepareGrab,
	Grabbing,
	SwingStart,
	SwingEnd,
	Count,
}

public partial class HumanoidArm : Node3D
{
	public Vector3 ShoulderPosition { get; set; }
	public Vector3 ShoulderVector { get; set; }
	public Vector3 TargetPosition { get; set; }
	public Node3D TargetNode { get; set; }

	private HumanoidArmState _state;
	public HumanoidArmState State
	{
		get => _state;
		set
		{
			if (_state != HumanoidArmState.Idle) return;
			_state = value;
		}
	}

	protected readonly Action<double>[] AnimateFunctions;
	
	public Storage TargetStorage { get; set; }
	public Vector2I TargetStoragePosition { get; set; }
	
	public Slot ItemSlot { get; protected set; }
	public bool HasWeapon => ItemSlot.Item is Weapon;

	public HumanoidArm()
	{
		AnimateFunctions = new Action<double>[]
		{
			Idle,
			Hold,
			PrepareGrab,
			Grabbing,
			SwingStart,
			SwingEnd,
		};
	}

	public override void _Ready()
	{
		ItemSlot = (Slot) FindChild("ArmSlot");
		base._Ready();
	}


	public override void _Process(double delta)
	{
		base._Process(delta);
		DebugInfo.AddLine("### " + Name + " - LastState: " + _lastState.ToString());
		DebugInfo.AddLine("### " + Name + " - ItemAngle: " + ItemSlot.Item?.RotationDegrees);
		var targetVector = ShoulderPosition + new Vector3(0.0f, 0.4f, 0.0f);
	}

	private HumanoidArmState _lastState; 
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		_lastState = State;
		AnimateFunctions[(int)State]?.Invoke(delta);
	}

	public void TransferItem()
	{
		if (ItemSlot.Item != null)
		{
			var item = ItemSlot.Item;
			ItemSlot.ExtractItem(item);
			TargetStorage?.InsertItem(TargetStoragePosition, item);
		}
	}
	
	protected void Idle(double delta)
	{
		var targetVector = ShoulderPosition + new Vector3(0.0f, 0.2f, 0.5f);
		Position = Position.Lerp(targetVector, (float) delta * 30);
		
		var rotation = RotationDegrees;
		rotation = rotation.Lerp(new Vector3(0.0f, 0.0f, 0.0f), (float) delta * 30);
		RotationDegrees = rotation;
	}
	
	protected void Hold(double delta)
	{
		var targetVector = ShoulderPosition + new Vector3(0.0f, 0.2f, 0.5f);
		Position = Position.Lerp(targetVector, (float) delta * 30);
		_state = HumanoidArmState.Idle;
	}
	
	protected void PrepareGrab(double delta)
	{
		var localTargetPosition = ((Node3D) GetParent()).ToLocal(TargetPosition);
		var targetVector = (localTargetPosition - ShoulderPosition).LimitLength(0.9f) + ShoulderPosition;
		Position = Position.Lerp(targetVector, (float) delta * 30);
		_state = HumanoidArmState.Idle;
	}
	
	protected void Grabbing(double delta)
	{
		PrepareGrab(delta);
		
		var handVector = GlobalPosition - TargetPosition;
		if (TargetNode is Item item && handVector.Length() < 0.45f)
		{
			item.Storage?.ExtractItem(item); 
			ItemSlot.InsertItem(new Vector2I(), item);
		}
		_state = HumanoidArmState.Idle;
	}

	private Vector3 _swingStartPosition;
	private float _swingRotation;
	protected void SwingStart(double delta)
	{
		if (_swingStartPosition == Vector3.Zero)
		{
			_swingRotation = GD.Randf() * Mathf.DegToRad(90.0f) * Mathf.Sign(ShoulderVector.X);
			_swingStartPosition = ShoulderPosition + new Vector3(0.0f, 0.6f, 0.0f).Rotated(Vector3.Forward, _swingRotation);
		}
		Position = Position.Slerp(_swingStartPosition, (float) delta * 15);
		
		if ((_swingStartPosition - Position).Length() < 0.1f)
			_state = HumanoidArmState.SwingEnd;
	}
	
	protected void SwingEnd(double delta)
	{
		var targetVector = new Vector3(-ShoulderPosition.X, ShoulderPosition.Y + 0.25f, 1.0f);
		
		var armDistance = Position.Length();
		var armVector = Position / armDistance;
		armDistance = Mathf.Lerp(armDistance, targetVector.Length(), (float) delta * 60);
		
		Position = armVector * armDistance;
		
		var rotateAngle = Position.AngleTo(targetVector);
		if (rotateAngle > Mathf.DegToRad(8.0f)) rotateAngle = Mathf.DegToRad(8.0f);
		Position = Position.Rotated(Position.Cross(targetVector).Normalized(), rotateAngle);

		var angle = 130.0f * (1.0f - (Position.DistanceTo(targetVector) / _swingStartPosition.DistanceTo(targetVector)));
		RotationDegrees = new Vector3(angle, _swingRotation, 0.0f);

		if ((targetVector - Position).Length() < 0.1f)
		{
			_swingStartPosition = Vector3.Zero;
			_state = HumanoidArmState.Idle;
		}
	}
}
