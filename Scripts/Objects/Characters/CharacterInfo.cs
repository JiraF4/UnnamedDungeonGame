using Godot;
using System;
using Godot.Collections;

public partial class CharacterInfo : Node
{
	protected CharacterController Controller;

	public override void _Ready()
	{
		Controller = GetParent<CharacterController>();
		base._Ready();
	}

	public override void _Process(double delta)
	{
		if (Multiplayer.GetUniqueId() == GetMultiplayerAuthority()) ReadInfo(delta);
		base._Process(delta);
	}

	public virtual void ReadInfo(double delta)
	{
		MaxHealth = Controller.Characteristics.MaxHealth;
		Health = Controller.Characteristics.Health;
	}
	
	
	public virtual void CollectSyncData(Dictionary syncData)
	{
		syncData["AttackStance"] = (int) AttackStance;
		syncData["BlockStance"] = (int) BlockStance;
		syncData["CurrentTarget"] = CurrentTarget;
		syncData["MaxHealth"] = MaxHealth;
		syncData["Health"] = Health;
	}

	public virtual void ApplySyncData(Dictionary syncData)
	{
		AttackStance = (CombatStance) (int) syncData["AttackStance"];
		BlockStance = (CombatStance) (int) syncData["BlockStance"];
		CurrentTarget = (NodePath) syncData["CurrentTarget"];
		MaxHealth = (float) syncData["MaxHealth"];
		Health = (float) syncData["Health"];
	}
	
	public CombatStance AttackStance { get; protected set; } = CombatStance.None;
	public CombatStance BlockStance { get; protected set; } = CombatStance.None;
	public NodePath CurrentTarget { get; protected set; } = null;
	public float MaxHealth { get; protected set; } 
	public float Health { get; protected set; }
}
