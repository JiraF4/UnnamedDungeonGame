using Godot;
using System;
using Dungeon.Tools;

public partial class AIController : Node
{
	private CharacterDoll _character;
	private CharacterControllerInputs _characterControllerInputs;
	
	[Export] public CharacterDoll TempTarget;
	
	public override void _Ready()
	{
		_character = GetParent<CharacterDoll>();
		_characterControllerInputs = (CharacterControllerInputs) _character.FindChild("ControllerInputs");
	}
	
	public override void _Process(double delta)
	{
		DebugInfo.AddLine("AI - InteractMode: " + _characterControllerInputs.InteractMode);
		DebugInfo.AddLine("AI - ScreenPosition: " + _characterControllerInputs.ScreenPosition);
		
		base._Process(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveToTarget(delta);
		
		base._PhysicsProcess(delta);
	}


	void MoveToTarget(double delta)
	{
		var targetPosition = TempTarget.GlobalPosition;
		var targetVectorHorizontal = targetPosition - _character.GlobalPosition;
		targetVectorHorizontal.Y = 0.0f;

		var speed = targetVectorHorizontal.Length() - 1.55f;
		speed = Mathf.Clamp(speed, -0.75f, 0.75f);
		if (Mathf.Abs(speed) < 0.1f) speed = 0.0f;
		_characterControllerInputs.MoveInput = targetVectorHorizontal.Normalized().Rotated(Vector3.Up, -_character.GlobalRotation.Y) * speed;
		
		var angleY = Mathf.Atan2(targetVectorHorizontal.X, targetVectorHorizontal.Z);
		var deltaAngle = MathfExtensions.DeltaAngleRad(_character.GlobalRotation.Y, angleY);
		_characterControllerInputs.RotateInput = new Vector3(0.0f, deltaAngle*5.0f, 0.0f);
		
		var targetInfo = TempTarget.CharacterInfo;
		var info = _character.CharacterInfo;
		
		if (deltaAngle <= 0.5f && info.CurrentTarget != null)
		{
			_characterControllerInputs.InteractMode = true;
		}
		else _characterControllerInputs.InteractMode = false;
		
		_characterControllerInputs.ScreenPositionMove = Vector2.Zero;
	}

	void ChangeStanceDefence()
	{
		var targetInfo = TempTarget.CharacterInfo;
		var info = _character.CharacterInfo;
		
		if (targetInfo.BlockStance != info.AttackStance)
			if (GD.Randi() % 8 == 0) _characterControllerInputs.PrimaryActionJustPressed = true;
		_characterControllerInputs.ScreenPositionMove = targetInfo.AttackStance switch
		{
			CombatStance.Left => new Vector2(150.0f, 150.0f),
			CombatStance.Right => new Vector2(-150.0f, 150.0f),
			CombatStance.Up => new Vector2(0.0f, -150.0f),
			_ => Vector2.Zero
		};
	}
	
	void Attack()
	{
		var targetInfo = TempTarget.CharacterInfo;
		var info = _character.CharacterInfo;
		
		if (targetInfo.BlockStance == info.AttackStance) return;
		
		_characterControllerInputs.PrimaryActionJustPressed = true;
		_characterControllerInputs.PrimaryAction = true;
	}
}
