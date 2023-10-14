using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public partial class CharacterDoll : RigidBody3D
{
	protected readonly System.Collections.Generic.Dictionary<Node3D, SynchronizationInterpolator> SynchronizationInterpolators = new();
	
	public CharacterInfo CharacterInfo { get; protected set; }
	
	protected AnimationPlayer AnimationPlayer;
	protected AnimationTree AnimationTree;
	public bool AnimationActive { get; protected set; }
	
	public void SetAnimationActive(bool active)
	{
		CallDeferred(nameof(SetAnimationActiveDeferred), active);
	}
	
	void SetAnimationActiveDeferred(bool active)
	{
		AnimationActive = active;
		AnimationTree.Active = active;
		if (!active)
		{
			RemoveChild(AnimationPlayer);
			RemoveChild(AnimationTree);
		}
	}

	public override void _Process(double delta)
	{
		if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId())
		{
			foreach (var synchronizationInterpolator in SynchronizationInterpolators)
			{ 
				synchronizationInterpolator.Value.Interpolate(delta);
			}
		}
		base._Process(delta);
	}

	public override void _Ready()
	{
		CharacterInfo = GetNode<CharacterInfo>("CharacterController/CharacterInfo");
		AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		AnimationTree = GetNode<AnimationTree>("AnimationTree");
	}

	public virtual void CollectSyncData(Dictionary syncData)
	{
		
	}

	public virtual void ApplySyncData(Dictionary syncData)
	{
		
	}
}
