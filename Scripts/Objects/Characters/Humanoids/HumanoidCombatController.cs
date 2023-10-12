using System;
using Godot;

public enum HumanoidCombatStance {
    None,
    Up,
    Left,
    Right,
}

public partial class HumanoidCombatController : Node
{
    public HumanoidController Controller { get; private set; }
    public HumanoidDoll Doll { get; protected set; }
    public CharacterControllerInputs CharacterControllerInputs { get; protected set; }
    public HumanoidAnimationController AnimationController { get; protected set; }
    
    private CharacterController _characterTarget;
    
    public HumanoidCombatStance Stance { get; protected set; }
    
    public override void _Ready()
    {
        Controller = GetParent<HumanoidController>();
        Doll = Controller.GetParent<HumanoidDoll>();
        CharacterControllerInputs = Controller.GetNode<CharacterControllerInputs>("CharacterControllerInputs");
        AnimationController = Controller.GetNode<HumanoidAnimationController>("AnimationController");
        Stance = HumanoidCombatStance.Up;
        
        base._Ready();
    }

    public override void _Process(double delta)
    {
        DebugInfo.AddLine("Target: " + _characterTarget?.Name);
        DebugInfo.AddLine("Stance: " + Stance.ToString());
        var vector = CharacterControllerInputs.ScreenPositionMove.Normalized();
        var StanceAngle = Mathf.Atan2(vector.X, vector.Y);
        DebugInfo.AddLine("Angle: " + Mathf.RadToDeg(StanceAngle));
        DebugInfo.AddLine("Angle: " + CharacterControllerInputs.ScreenPositionMove.Length());
        
        base._Process(delta);
    }

    public void RotateToTarget(double delta)
    {
        if (_characterTarget == null) return;
        Controller.LookTo(delta, _characterTarget.Target.GlobalPosition, 4.5f);
    }
    
    public void ChangeStance(double delta)
    {
        if (Controller.StateController.State != HumanoidState.Combat) Stance = HumanoidCombatStance.None;
        var vector = CharacterControllerInputs.ScreenPositionMove.Normalized();
        var stanceAngle = Mathf.Atan2(vector.X, vector.Y);
        if (CharacterControllerInputs.ScreenPositionMove.Length() > 40.0f)
        {
            if (stanceAngle > Mathf.DegToRad(   0) && stanceAngle < Mathf.DegToRad( 120)) Stance = HumanoidCombatStance.Right;
            if (stanceAngle > Mathf.DegToRad(-120) && stanceAngle < Mathf.DegToRad(   0)) Stance = HumanoidCombatStance.Left;
            if (stanceAngle > Mathf.DegToRad( 120) || stanceAngle < Mathf.DegToRad(-120)) Stance = HumanoidCombatStance.Up;
        }
    }

    public void UpdateState()
    {
        _characterTarget = CharactersService.Instance.FindNearestTarget(Doll.Head.GlobalPosition, Doll.Head.GlobalTransform.Basis.Z);
    }

    public bool HasTarget()
    {
        return _characterTarget != null;
    }

    public void Hit()
    {
        if (_characterTarget == null) return;
        _characterTarget.HitReceive();
        Console.WriteLine("!!!");
    }

    public void HitReceive()
    {
        AnimationController.HitStun();
    }
}