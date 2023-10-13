using Godot;
using System;

public partial class CharacterDoll : RigidBody3D
{
	public CharacterInfo CharacterInfo { get; protected set; }
	
	public override void _Ready()
	{
		CharacterInfo = GetNode<CharacterInfo>("CharacterController/CharacterInfo");
	}
}
