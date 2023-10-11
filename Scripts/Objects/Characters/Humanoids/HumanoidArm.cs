using Godot;
using System;

public partial class HumanoidArm : Node3D
{
	public Node3D Shoulder { get; set; }
	public Vector3 TargetPosition { get; set; }
	public Node3D TargetNode { get; set; }
	
	public Slot ItemSlot { get; protected set; }
	public bool HasWeapon => ItemSlot.Item is Weapon;

	public override void _Ready()
	{
		ItemSlot = (Slot) FindChild("ArmSlot");
		Shoulder = GetParent<Node3D>();
		base._Ready();
	}
}
