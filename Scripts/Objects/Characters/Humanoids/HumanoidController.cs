using Godot;
using System;
using System.Diagnostics;



public partial class HumanoidController : CharacterController
{
	public HumanoidDoll Doll { get; protected set; }
	public HumanoidStateController StateController { get; protected set; }
	public HumanoidItemManipulationController ItemManipulationController { get; protected set; }
	public HumanoidUIController UIController { get; protected set; }
	
	protected float XRotation;
	

	public override void _Ready()
	{
		base._Ready();
		Doll = GetParent<HumanoidDoll>();
		StateController = (HumanoidStateController) FindChild("StateController");
		ItemManipulationController = GetNode<HumanoidItemManipulationController>("ItemManipulationController");
		UIController = GetNode<HumanoidUIController>("UIController");
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
		if (CharacterControllerInputs.UIControl) UIController.UpdateUI(delta);
		
		// TODO: Transfer to animation
		//var bodyPosition = _bodyBasePosition;
		//bodyPosition.Y += Mathf.Sin((Time.GetTicksMsec() + AnimationOffset) / 1000.0f) * 0.01f;
		//bodyPosition.Y += Mathf.Sin((Time.GetTicksMsec() + AnimationOffset) / 100.0f) * CharacterBody.LinearVelocity.Length() * 0.01f;
		//_body.Position = bodyPosition;
        
	}

	public override void UpdateState(double delta)
	{
		StateController.UpdateState();
		
		ItemManipulationController.UpdateTarget();
		if (CharacterControllerInputs.PrimaryActionJustPressed) ItemManipulationController.UpdateTargetFocus(); 
		if (CharacterControllerInputs.PrimaryActionJustReleased) ItemManipulationController.ClearTargetFocus(); 
		
		
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
	
	

	
	
}
