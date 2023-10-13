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

	public virtual CombatStance AttackStance => CombatStance.None;
	public virtual CombatStance BlockStance => CombatStance.None;
	public virtual CharacterController CurrentTarget => null;
	public virtual float MaxHealth => Controller.Characteristics.MaxHealth;
	public virtual float Health => Controller.Characteristics.Health;
}
