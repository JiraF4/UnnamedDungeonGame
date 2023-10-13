using Godot;
using System;

public partial class InfoBar3D : Node3D
{
	private Control _infoBar;
	private ProgressBar _healthBar;
	private bool _showInfoBar;
	public CharacterController Controller { get; protected set; }
	
	public override void _Ready()
	{
		_infoBar = GetNode<Control>("InfoBar");
		_healthBar = _infoBar.GetNode<ProgressBar>("HealthBar");
		Controller = GetParent().GetNode<CharacterController>("CharacterController");
		base._Ready();
	}

	public void Show()
	{
		_showInfoBar = true;
	}
	
	public override void _Process(double delta)
	{
		_infoBar.Visible = _showInfoBar;
		if (_showInfoBar)
		{
			var camera3D = GetViewport().GetCamera3D();
			var inventoryScreenPosition = camera3D.UnprojectPosition(GlobalPosition);
			if (!camera3D.IsPositionBehind(GlobalPosition))
			{
				_infoBar.Position = inventoryScreenPosition - _infoBar.Size / 2;
				UpdateInfo();
			}
			else _infoBar.Visible = false;
		}
		_showInfoBar = false;
	}

	void UpdateInfo()
	{
		var info = Controller.CharacterInfo;
		
		_healthBar.MaxValue = info.MaxHealth;
		_healthBar.Value = info.Health;
	}
}
