using Godot;
using System;

public partial class MainMenu : Control
{
	TextEdit _ipTextEdit;

	public override void _Ready()
	{
		_ipTextEdit = GetNode<TextEdit>("IpTextEdit");
		base._Ready();
	}

	public void Connect()
	{
		Network.Instance.Start(_ipTextEdit.Text);
	}
}
