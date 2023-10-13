using System;
using Godot;

public partial class HumanoidCombatController : Node
{
    public HumanoidController Controller { get; private set; }
    public HumanoidDoll Doll { get; protected set; }
    public CharacterControllerInputs CharacterControllerInputs { get; protected set; }
    public HumanoidAnimationController AnimationController { get; protected set; }

    public CharacterController CharacterTarget { get; private set; }

    public CombatStance Stance { get; protected set; }
    
    public override void _Ready()
    {
        Controller = GetParent<HumanoidController>();
        Doll = Controller.GetParent<HumanoidDoll>();
        CharacterControllerInputs = Controller.GetNode<CharacterControllerInputs>("CharacterControllerInputs");
        AnimationController = Controller.GetNode<HumanoidAnimationController>("AnimationController");
        Stance = CombatStance.Up;
        
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void RotateToTarget(double delta)
    {
        if (CharacterTarget == null) return;
        Controller.LookTo(delta, CharacterTarget.Target.GlobalPosition, 4.5f);
    }
    
    public void ChangeStance(double delta)
    {
        if (Controller.StateController.State != HumanoidState.Combat) Stance = CombatStance.None;
        var vector = CharacterControllerInputs.ScreenPositionMove.Normalized();
        var stanceAngle = Mathf.Atan2(vector.X, vector.Y);
        if (CharacterControllerInputs.ScreenPositionMove.Length() > 40.0f)
        {
            if (stanceAngle > Mathf.DegToRad(   0) && stanceAngle < Mathf.DegToRad( 120)) Stance = CombatStance.Right;
            if (stanceAngle > Mathf.DegToRad(-120) && stanceAngle < Mathf.DegToRad(   0)) Stance = CombatStance.Left;
            if (stanceAngle > Mathf.DegToRad( 120) || stanceAngle < Mathf.DegToRad(-120)) Stance = CombatStance.Up;
        }
    }

    public void UpdateState()
    {
        CharacterTarget = CharactersService.Instance.FindNearestTarget(Doll.Head.GlobalPosition, Doll.Head.GlobalTransform.Basis.Z);
    }

    public bool HasTarget()
    {
        return CharacterTarget != null;
    }

    public void Hit()
    {
        CharacterTarget?.HitReceive();
    }

    public void HitReceive()
    {
        AnimationController.HitStun();
    }
}