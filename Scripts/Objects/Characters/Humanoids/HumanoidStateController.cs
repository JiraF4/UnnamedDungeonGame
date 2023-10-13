using Godot;
using System;

public enum HumanoidState
{
	Idle,
	Combat,
	Interact,
	Stunned,
}

public enum HumanoidAction
{
	Idle,
	Grabbing,
	Attack,
}

public partial class HumanoidStateController : Node
{
	public HumanoidController Controller { get; private set; }
	public HumanoidDoll Doll { get; protected set; }
	public CharacterControllerInputs CharacterControllerInputs { get; protected set; }
	public HumanoidAnimationController AnimationController { get; protected set; }
	public HumanoidCombatController CombatController { get; protected set; }
	
	public HumanoidState State { get; protected set; }
	public HumanoidAction Action { get; protected set; }
	
	public override void _Ready()
	{
		Controller = GetParent<HumanoidController>();
		Doll = Controller.GetParent<HumanoidDoll>();
		CharacterControllerInputs = Controller.GetNode<CharacterControllerInputs>("ControllerInputs");
		AnimationController = Controller.GetNode<HumanoidAnimationController>("AnimationController");
		CombatController = Controller.GetNode<HumanoidCombatController>("CombatController");
		
		base._Ready();
	}

	private void UpdateCurrentState()
	{
		var animation = AnimationController.GetAnimation();
		
		// TODO: get rid of strings
		if (animation.StartsWith("Idle"))
		{
			State = HumanoidState.Idle;
		}
		if (animation.StartsWith("Stunned"))
		{
			State = HumanoidState.Stunned;
		}
		else if (animation.StartsWith("Combat"))
		{
			State = HumanoidState.Combat;
		} 
		else if (animation.StartsWith("Interact"))
		{
			State = HumanoidState.Interact;
		}

		if (animation.EndsWith("Grabbing"))
		{
			Action = HumanoidAction.Grabbing;
		} if (animation.Contains("Attack"))
		{
			Action = HumanoidAction.Attack;
		}
		else Action = HumanoidAction.Idle;
	}
	
	public void UpdateState()
	{
		AnimationController.UpdateConditions();
		UpdateCurrentState();
	}
}
