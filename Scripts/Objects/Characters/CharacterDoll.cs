using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

public partial class CharacterDoll : SynchronizedRigidBody
{
	public CharacterInfo CharacterInfo { get; protected set; }
	public CharacterController Controller { get; protected set; }
	
	public override void _Ready()
	{
		CharacterInfo = GetNode<CharacterInfo>("CharacterController/CharacterInfo");
		Controller = GetNode<CharacterController>("CharacterController");
	}
}
