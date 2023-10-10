using Godot;
using System;
using System.Diagnostics;

public enum HumanoidState {
	Combat,
	Interact,
	Grabbing,
}

public partial class HumanoidController : CharacterController
{
	protected Node3D _body;
	protected Node3D _head;
	protected HumanoidArm _rightArm;
	protected HumanoidArm _leftArm;
	protected Node3D _RightLeg;
	protected Node3D _LeftLeg;
	protected AnimationPlayer _animationPlayer;

	protected Vector3 _bodyBasePosition;
	protected ulong AnimationOffset = 0;
	
	protected HumanoidState State = HumanoidState.Interact;
	
	public Item HoldItem {
		get
		{
			return GrabbingArm.ItemSlot.Item;
		}
	}

	protected HumanoidArm GrabbingArm;

	protected Storage FocusTargetStorage;
	protected Item FocusTargetItem;
	
	protected float XRotation = 0.0f;
	protected TextureRect GrabbedItemTextureRect;
	
	public Storage CurrentTargetStorageFocus
	{
		get
		{
			if (FocusTargetStorage != null) return FocusTargetStorage;
			return CurrentTargetStorage;
		}
	}
	public Storage CurrentTargetStorage {
		get
		{
			if (_leftArm.ItemSlot.IsScreenPositionInside(_characterControllerInputs.ScreenPosition)) return _leftArm.ItemSlot;
			if (_rightArm.ItemSlot.IsScreenPositionInside(_characterControllerInputs.ScreenPosition)) return _rightArm.ItemSlot;
			var targetNode = _characterControllerInputs.TargetNode;
			var inventory = (Inventory) targetNode?.FindChild("Inventory");
			return inventory;
		}
	}

	public Item CurrentTargetItemFocus
	{
		get
		{
			if (FocusTargetItem != null) return FocusTargetItem;
			return CurrentTargetItem;
		}
	}
	public Item CurrentTargetItem
	{
		get
		{
			var targetNode = _characterControllerInputs.TargetNode;
			if (targetNode is Item item) return item;
			var storage = CurrentTargetStorageFocus;
			if (CurrentTargetStorageFocus is Inventory inventory) return inventory.GetItemScreenPosition(_characterControllerInputs.ScreenPosition);
			if (CurrentTargetStorageFocus is Slot slot) return slot.Item;
			return null;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		
		_body = GetParent().GetNode<Node3D>("Body");
        _head = _body.GetNode<Node3D>("Head");
        _rightArm = _body.GetNode<HumanoidArm>("RightArm");
        _leftArm = _body.GetNode<HumanoidArm>("LeftArm");
        _RightLeg = GetParent().GetNode<Node3D>("RightLeg");
        _LeftLeg = GetParent().GetNode<Node3D>("LeftLeg");
        GrabbingArm = _leftArm;
        
        _animationPlayer = GetParent().GetNode<AnimationPlayer>("AnimationPlayer");
        
        GrabbedItemTextureRect = GetParent().GetNode<TextureRect>("GrabbedItemTextureRect");
		
        _leftArm.ShoulderPosition = new Vector3(0.3f, 0.2f, 0.0f);
        _leftArm.ShoulderVector = new Vector3(1.0f, 0.0f, 0.0f);
        _rightArm.ShoulderPosition = new Vector3(-0.3f, 0.2f, 0.0f);
        _rightArm.ShoulderVector = new Vector3(-1.0f, 0.0f, 0.0f);
        
        _bodyBasePosition = _body.Position;
        AnimationOffset = GD.Randi() % 1000;
		
        CallDeferred(nameof(ReadyItemsGrab));
	}

	private void ReadyItemsGrab()
	{
		var leftItem = (Item) CharacterBody.FindChild("LeftArmItem")?.GetChild(0);
		if (leftItem != null) _leftArm.ItemSlot.InsertItem(Vector2I.Zero, leftItem);
		var rightItem = (Item) CharacterBody.FindChild("RightArmItem")?.GetChild(0);
		if (rightItem != null) _rightArm.ItemSlot.InsertItem(Vector2I.Zero, rightItem);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_characterControllerInputs.UIControl) UpdateUI(delta);

		var bodyPosition = _bodyBasePosition;
		//bodyPosition.Y += Mathf.Sin((Time.GetTicksMsec() + AnimationOffset) / 1000.0f) * 0.01f;
		bodyPosition.Y += Mathf.Sin((Time.GetTicksMsec() + AnimationOffset) / 100.0f) * CharacterBody.LinearVelocity.Length() * 0.01f;
		_body.Position = bodyPosition;
        
	}

	public override void UpdateState(double delta)
	{
		State = HumanoidState.Combat;
		if (_characterControllerInputs.InteractMode) State = HumanoidState.Interact;
		ArmsStateChange(delta);
		
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
		
		XRotation -= RotateInput.X * RotationSpeed.X * (float) delta;
		XRotation = Mathf.Clamp(XRotation, -90.0f, 90.0f);
		var headRotation = _head.RotationDegrees;
		var bodyRotation = _body.RotationDegrees;
		headRotation.X = XRotation * 0.3f;
		bodyRotation.X = XRotation * 0.7f;
		_head.RotationDegrees = headRotation;
		_body.RotationDegrees = bodyRotation;

		base.UpdateState(delta);
	}
	
	

	private void UpdateUI(double delta)
	{
		var storage = CurrentTargetStorage;
		if (State != HumanoidState.Combat) storage?.Draw();
		_leftArm.ItemSlot.Draw();
		_rightArm.ItemSlot.Draw();

		if (HoldItem != null && State != HumanoidState.Combat)
		{
			GrabbedItemTextureRect.Visible = true;
			GrabbedItemTextureRect.Texture = HoldItem.ItemRect.Texture;
			GrabbedItemTextureRect.Size = HoldItem.ItemRect.Size;
			if (storage != null) GrabbedItemTextureRect.Position = storage.AlignPositionItem(_characterControllerInputs.ScreenPosition, HoldItem);
			else GrabbedItemTextureRect.Position = _characterControllerInputs.ScreenPosition - new Vector2(Inventory.InventoryCellHalfSize, Inventory.InventoryCellHalfSize);
		}
		else GrabbedItemTextureRect.Visible = false;
		
		DebugInfo.AddLine("CurrentTargetItem: " + CurrentTargetItemFocus?.Name);
		DebugInfo.AddLine("FocusTargetItem: " + FocusTargetItem?.Name);
		DebugInfo.AddLine("MoveInput: " + MoveInput);
		DebugInfo.AddLine("State: " + State);
		DebugInfo.AddLine("CurrentAnimationPosition: " + _animationPlayer.CurrentAnimationPosition);
		DebugInfo.AddLine("CurrentAnimationLength: " + _animationPlayer.CurrentAnimationLength);
		DebugInfo.AddLine("CurrentAnimation: " + _animationPlayer.CurrentAnimation);
	}

	private void ArmsStateChange(double delta)
	{
		var targetNode = _characterControllerInputs.TargetNode;
		var mainArm = _leftArm;
		var sideArm = _rightArm;
		
		if (State != HumanoidState.Combat) ArmsStateChangeInteract(delta, mainArm, sideArm);
		else if (State == HumanoidState.Combat) ArmsStateChangeCombat(delta, mainArm, sideArm);
	}

	private void ArmsStateChangeInteract(double delta, HumanoidArm mainArm, HumanoidArm sideArm)
	{
		_animationPlayer.PlaybackActive = false;
		_animationPlayer.AssignedAnimation = "CombatIdle";
		
		if (!_characterControllerInputs.PrimaryAction)
		{
			FocusTargetStorage = null;
			FocusTargetItem = null;
		}
		else if (_characterControllerInputs.PrimaryActionJustPressed)
		{
			FocusTargetStorage = CurrentTargetStorageFocus;
			FocusTargetItem = CurrentTargetItemFocus;
		}
		if (_characterControllerInputs.PrimaryActionJustReleased)
			GrabbingArm.TransferItem();
		
		mainArm.TargetStorage = null;
		sideArm.TargetStorage = null;
		
		var storage = CurrentTargetStorage;
		if (storage != null)
		{
			mainArm.TargetStorage = storage;
			mainArm.TargetStoragePosition = storage.GetInventoryPosition(_characterControllerInputs.ScreenPosition);
			sideArm.TargetStorage = storage;
			sideArm.TargetStoragePosition = storage.GetInventoryPosition(_characterControllerInputs.ScreenPosition);
		}
		if (HoldItem != null && _characterControllerInputs.PrimaryAction) mainArm.State = HumanoidArmState.Hold;
		else SetGrabbing(mainArm);
	}
	
	private void ArmsStateChangeCombat(double delta, HumanoidArm mainArm, HumanoidArm sideArm)
	{
		_animationPlayer.PlaybackActive = true;
		if (_characterControllerInputs.PrimaryActionJustPressed)
		{
			_animationPlayer.AssignedAnimation = "CombatAttack";
			_animationPlayer.Queue("CombatIdle");
		}
		
		/*
		if (_characterControllerInputs.PrimaryActionJustPressed)
		{
			if (mainArm.HasWeapon) mainArm.State = HumanoidArmState.SwingStart;
			else if (sideArm.HasWeapon) sideArm.State = HumanoidArmState.SwingStart;
		}
		*/
	}

	void SetGrabbing(HumanoidArm grabArm)
	{
		if (FocusTargetItem != null || !_characterControllerInputs.PrimaryAction)
		{
			var storage = CurrentTargetStorageFocus;
			var targetItem = CurrentTargetItemFocus;
			if (targetItem != null)
			{
				grabArm.TargetPosition = storage?.GlobalPosition ?? targetItem.GlobalPosition;
				grabArm.State = _characterControllerInputs.PrimaryAction ? HumanoidArmState.Grabbing : HumanoidArmState.PrepareGrab;
				grabArm.TargetNode = targetItem;

				if (_characterControllerInputs.PrimaryAction && State == HumanoidState.Interact) State = HumanoidState.Grabbing;
				return;
			}
		}

		grabArm.TargetPosition = Vector3.Zero;
		grabArm.State = HumanoidArmState.Idle;
		grabArm.TargetNode = null;
	}
}
