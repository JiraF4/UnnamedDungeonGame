using Godot;
using System;

public partial class CharacterInfo : Node
{
	protected CharacterController Controller;

	public override void _Ready()
	{
		Controller = GetParent<CharacterController>();
		base._Ready();
	}

	public virtual CombatStance CombatStance => CombatStance.None;
	public virtual CharacterController CurrentTarget => null;
}
