using Godot;
using System;

public partial class DebugNode : Node
{
	public override void _Ready()
	{
		var controller = GetParent().GetNode<PlayerController>("PlayerController");
		controller.GetInput("TestWorld/Character");
	}
}
