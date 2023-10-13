using Godot;
using System;

public partial class StanceIndicator : TextureRect
{
	protected TextureRect StanceUpRect; 
	protected TextureRect StanceLeftRect; 
	protected TextureRect StanceRightRect;

	private bool _showIndicator;
	
	public void SetStance(CombatStance stance)
	{
		switch (stance)
        {
            case CombatStance.Up:
                StanceUpRect.Visible = true;
                StanceLeftRect.Visible = false;
                StanceRightRect.Visible = false;
                break;
            case CombatStance.Left:
                StanceUpRect.Visible = false;
                StanceLeftRect.Visible = true;
                StanceRightRect.Visible = false;
                break;
            case CombatStance.Right:
                StanceUpRect.Visible = false;
                StanceLeftRect.Visible = false;
                StanceRightRect.Visible = true;
                break;
            case CombatStance.None:
	            StanceUpRect.Visible = false;
	            StanceLeftRect.Visible = false;
	            StanceRightRect.Visible = false;
	            break;
            default:
	            throw new ArgumentOutOfRangeException(nameof(stance), stance, null);
        }
	}

	public void ShowIndicator()
	{
		_showIndicator = true;
	}

	public override void _Ready()
	{
		StanceUpRect = GetNode<TextureRect>("StanceUpRect");
        StanceLeftRect = GetNode<TextureRect>("StanceLeftRect");
        StanceRightRect = GetNode<TextureRect>("StanceRightRect");
        
		base._Ready();
	}

	public override void _Process(double delta)
	{
		Visible = _showIndicator;
		_showIndicator = false;
	}
}
