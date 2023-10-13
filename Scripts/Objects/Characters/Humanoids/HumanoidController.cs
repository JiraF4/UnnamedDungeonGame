using Godot;
using System;
using System.Diagnostics;
using Dungeon.Tools;


public partial class HumanoidController : CharacterController
{
	public HumanoidDoll Doll { get; protected set; }
	public HumanoidStateController StateController { get; protected set; }
	public HumanoidItemManipulationController ItemManipulationController { get; protected set; }
	public HumanoidCombatController CombatController { get; protected set; }
	public HumanoidUIController UIController { get; protected set; }
	public HumanoidAnimationController AnimationController { get; protected set; }
	
	protected float XRotation;
	
	
	public void LookTo(double delta, Vector3 position, float speed)
	{
		var vectorTo = (position - Doll.Head.GlobalPosition).Normalized();
		RotateTo(delta, vectorTo, speed);
	}

	public void RotateTo(double delta, Vector3 vector, float speed)
	{
		if (vector == Vector3.Zero) return;
		var angleY = Mathf.Atan2(vector.X, vector.Z);
		var angleX = XRotation;
		var angleDeltaY = MathfExtensions.DeltaAngleRad(CharacterBody.GlobalRotation.Y, angleY);
		var angleDeltaX = MathfExtensions.DeltaAngleRad(CharacterBody.GlobalRotation.X, angleX);
		var rotationYMaxSpeed = RotationSpeed.X * (float) delta * speed;
		var rotationXMaxSpeed = RotationSpeed.Y * (float) delta * speed;
		angleDeltaY = Math.Clamp(angleDeltaY, -rotationYMaxSpeed, rotationYMaxSpeed);
		angleDeltaX = Math.Clamp(angleDeltaX, -rotationXMaxSpeed, rotationXMaxSpeed);
		RotateInput = RotateInput.Lerp(new Vector3(angleDeltaX/rotationXMaxSpeed * 10.0f, angleDeltaY/rotationYMaxSpeed * 3.0f, 0), 0.9f);
	}
	
	public override void _Ready()
	{
		base._Ready();
		Doll = GetParent<HumanoidDoll>();
		StateController = (HumanoidStateController) FindChild("StateController");
		ItemManipulationController = GetNode<HumanoidItemManipulationController>("ItemManipulationController");
		CombatController = GetNode<HumanoidCombatController>("CombatController");
		UIController = GetNode<HumanoidUIController>("UIController");
		AnimationController = GetNode<HumanoidAnimationController>("AnimationController");
        CallDeferred(nameof(ReadyItemsGrab)); // TODO: This should be done in a better way
	}

	private void ReadyItemsGrab()
	{
		var leftItem = (Item) CharacterBody.FindChild("LeftArmItem")?.GetChild(0);
		if (leftItem != null) Doll.LeftArm.ItemSlot.InsertItem(Vector2I.Zero, leftItem);
		var rightItem = (Item) CharacterBody.FindChild("RightArmItem")?.GetChild(0);
		if (rightItem != null) Doll.RightArm.ItemSlot.InsertItem(Vector2I.Zero, rightItem);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (ControllerInputs.UIControl) UIController.UpdateUI(delta);
		
		// TODO: Transfer to animation
		//var bodyPosition = _bodyBasePosition;
		//bodyPosition.Y += Mathf.Sin((Time.GetTicksMsec() + AnimationOffset) / 1000.0f) * 0.01f;
		//bodyPosition.Y += Mathf.Sin((Time.GetTicksMsec() + AnimationOffset) / 100.0f) * CharacterBody.LinearVelocity.Length() * 0.01f;
		//_body.Position = bodyPosition;
        
	}

	void Die()
	{
		AnimationController.Die();
		CharacterBody.Freeze = true;
		CharacterBody.CollisionLayer = 0;
		Dead = true;
	}

	public override void UpdateState(double delta)
	{
		if (Characteristics.Health <= 0)
		{
			if (!Dead) Die();
			return;
		}
		
		StateController.UpdateState();
		
		ItemManipulationController.UpdateTarget();
		if (ControllerInputs.PrimaryActionJustPressed) ItemManipulationController.UpdateTargetFocus(); 
		if (ControllerInputs.PrimaryActionJustReleased) ItemManipulationController.ClearTargetFocus(); 
		
		if (!ControllerInputs.InteractMode) CombatController.UpdateState();
		CombatController.ChangeStance(delta);
		if (StateController.State == HumanoidState.Combat) RotateInput *= 0.1f;
		if (StateController.State == HumanoidState.Combat) CombatController.RotateToTarget(delta);

		if (StateController.Action == HumanoidAction.Attack)
		{
			var target = CombatController.CharacterTarget;
			if (target != null)
			{
				var targetVectorHorizontal = target.Target.GlobalPosition - Target.GlobalPosition;
				targetVectorHorizontal.Y = 0.0f;
				
				var speed = targetVectorHorizontal.Length() - 1.55f;
				speed = Mathf.Clamp(speed*4.0f, -1.00f, 1.00f);
				MoveInput = targetVectorHorizontal.Normalized().Rotated(Vector3.Up, -Doll.GlobalRotation.Y) * speed;
			}
		}
		
		/*
		if (State == HumanoidState.Grabbing)
		{
			var vectorToItem = (CurrentTargetStorage?.GlobalPosition ?? CurrentTargetItemFocus.GlobalPosition) - CharacterBody.GlobalPosition;
			vectorToItem.Y = 0;
			if (vectorToItem.Length() > 1.05f)
			{
				vectorToItem = vectorToItem.Normalized().Rotated(Vector3.Up, -CharacterBody.Rotation.Y);
				vectorToItem.X += MoveInput.X;
				vectorToItem.Y += MoveInput.Y;
				vectorToItem = vectorToItem.Normalized();
				MoveInput = new Vector3(vectorToItem.X, MoveInput.Y, vectorToItem.Z);
			}
		}
		*/
		
		XRotation -= RotateInput.X * RotationSpeed.X * (float) delta;
		XRotation = Mathf.Clamp(XRotation, -90.0f, 90.0f);
		var headRotation = Doll.Head.RotationDegrees;
		var bodyRotation = Doll.Body.RotationDegrees;
		headRotation.X = XRotation * 0.3f;
		bodyRotation.X = XRotation * 0.7f;
		Doll.Head.RotationDegrees = headRotation;
		Doll.Body.RotationDegrees = bodyRotation;

		base.UpdateState(delta);
	}
	
	public override void HitReceive()
	{
		CombatController.HitReceive();
	}

	
	
}
