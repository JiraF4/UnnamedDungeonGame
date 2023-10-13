using Godot;
using System;

public partial class MainMenu : Control
{
	public void Connect()
	{
		Network.Instance.Start("127.0.0.1");
	}
}
