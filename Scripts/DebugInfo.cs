using Godot;
using System;

public partial class DebugInfo : Label
{
	static DebugInfo _instance;

	DebugInfo()
	{
		_instance = this;
	}
	
	public static void AddLine(string str)
	{
		_instance.Text += str + "\n";
	}
	
	public override void _Process(double delta)
	{
		Text = "";
	}
}
