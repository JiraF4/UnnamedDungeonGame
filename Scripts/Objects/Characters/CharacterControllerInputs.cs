using Godot;
using System;

public partial class CharacterControllerInputs : Node
{
	public Vector3 MoveInput { get; set; }
	public Vector3 RotateInput { get; set; }
	public Node3D TargetNode { get; set; }
	public Vector3 TargetPosition { get; set; }
	public bool PrimaryAction { get; set; }
	public bool PrimaryActionJustPressed { get; set; }
	public bool PrimaryActionJustReleased { get; set; }
	public bool UIControl { get; set; }
	public Vector2 ScreenPosition { get; set; }
	public bool InteractMode { get; set; }
}
