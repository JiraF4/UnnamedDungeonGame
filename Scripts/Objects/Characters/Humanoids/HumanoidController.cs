using Godot;
using System;
using System.Diagnostics;
using System.Globalization;
using Dungeon.Tools;
using Godot.Collections;


public partial class HumanoidController : CharacterController
{
	public HumanoidDoll Doll { get; protected set; }
	public HumanoidStateController StateController { get; protected set; }
	public HumanoidItemManipulationController ItemManipulationController { get; protected set; }
	public HumanoidCombatController CombatController { get; protected set; }
	public HumanoidUIController UIController { get; protected set; }
	public HumanoidAnimationController AnimationController { get; protected set; }
	
	public RayCast3D LegsCast { get; protected set; }
	
	public void LookTo(double delta, Vector3 position, float speed)
	{
		var vectorTo = (position - Doll.Head.GlobalPosition).Normalized();
		RotateTo(delta, vectorTo, speed);
	}

	public void RotateTo(double delta, Vector3 vector, float speed)
	{
		if (vector == Vector3.Zero) return;
		var angleDeltaY = MathfExtensions.DeltaAngleRad(Doll.Head.GlobalRotation.Y, Mathf.Atan2(vector.X, vector.Z));
		var angleDeltaX = MathfExtensions.DeltaAngleRad(Doll.Head.GlobalRotation.X, 0.0f);
		var inputX = angleDeltaX * RotationSpeed.X / (float) delta * speed;
		var inputY = angleDeltaY * RotationSpeed.Y / (float) delta * speed;
		RotateInput = RotateInput.Lerp(new Vector3(inputX, inputY, 0.0f), 0.9f);
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
		LegsCast = CharacterDoll.GetNode<RayCast3D>("LegsCast");
        CallDeferred(nameof(ReadyItemsGrab)); // TODO: This should be done in a better way
	}
	
	private void ReadyItemsGrab()
	{
		var leftItem = (Item) CharacterDoll.FindChild("LeftArmItem")?.GetChild(0);
		//if (leftItem != null) Doll.LeftArm.ItemSlot.InsertItem(Vector2I.Zero, leftItem);
		var rightItem = (Item) CharacterDoll.FindChild("RightArmItem")?.GetChild(0);
		//if (rightItem != null) Doll.RightArm.ItemSlot.InsertItem(Vector2I.Zero, rightItem);
	}

	public override void _Process(double delta)
	{
		if (ControllerInputs.UIControl) UIController.UpdateUI(delta);
		base._Process(delta);
	}
	
	protected override void DieRemote()
	{
		AnimationController.Die();
		base.DieRemote();
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

		var legsDistance = LegsCast.GetCollisionPoint().DistanceTo(LegsCast.GlobalPosition);
		var legsLength = -LegsCast.TargetPosition.Y;
		if (!LegsCast.IsColliding()) legsDistance = legsLength;
		if (legsDistance < legsLength) CharacterDoll.LinearVelocity += (Vector3.Up * Mathf.Max(((1.0f - (legsDistance / legsLength)) * 3f - CharacterDoll.LinearVelocity.Y), 0.0f));
		else CharacterDoll.Sleeping = false;

		var dollBodyRotation = Doll.BodyRotation;
		dollBodyRotation.X += RotateInput.X * RotationSpeed.X * (float) delta;
		dollBodyRotation.X = Mathf.Clamp(dollBodyRotation.X, Mathf.DegToRad(-85.0f), Mathf.DegToRad(85.0f));
		dollBodyRotation.Y += RotateInput.Y * RotationSpeed.Y * (float) delta;
		dollBodyRotation.Y = Mathf.Clamp(dollBodyRotation.Y, Mathf.DegToRad(-85.0f), Mathf.DegToRad(85.0f));
		
		dollBodyRotation.Y -= dollBodyRotation.Y * (float) delta * 6.0f;
		CharacterDoll.Rotation += new Vector3(0.0f, dollBodyRotation.Y * (float) delta * 6.0f, 0.0f);
		Doll.BodyRotation = dollBodyRotation;
		
		base.UpdateState(delta);
	}
	
	public override void HitReceive()
	{
		CombatController.HitReceive();
	}
	
	public override void CollectSyncData(Dictionary syncData)
	{
		AnimationController.CollectSyncData(syncData);
	}

	public override void ApplySyncData(Dictionary syncData)
	{
		AnimationController.ApplySyncData(syncData);
	}
	
}
