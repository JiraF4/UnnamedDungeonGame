using System;
using Godot;

public partial class HumanoidCombatController : Node
{
    public HumanoidController Controller { get; private set; }
    public HumanoidDoll Doll { get; protected set; }
    public CharacterControllerInputs CharacterControllerInputs { get; protected set; }
    public HumanoidAnimationController AnimationController { get; protected set; }
    public HumanoidStateController StateController { get; protected set; }

    public CharacterController CharacterTarget { get; private set; }

    public CombatStance NextStance { get; protected set; }
    
    public CombatStance AttackStance {
        get
        {
            var animation = AnimationController.GetAnimation();
            if (animation.EndsWith("Up")) return CombatStance.Up;
            if (animation.EndsWith("Left")) return CombatStance.Left;
            if (animation.EndsWith("Right")) return CombatStance.Right;
            return CombatStance.None;
        }
    }

    public CombatStance BlockStance
    {
        get
        {
            var animation = AnimationController.GetAnimation();
            return animation switch
            {
                "CombatStanceUp" => CombatStance.Up,
                "CombatStanceLeft" => CombatStance.Right,
                "CombatStanceRight" => CombatStance.Left,
                _ => CombatStance.None
            };
        }
    }

    public override void _Ready()
    {
        Controller = GetParent<HumanoidController>();
        Doll = Controller.GetParent<HumanoidDoll>();
        CharacterControllerInputs = Controller.GetNode<CharacterControllerInputs>("ControllerInputs");
        AnimationController = Controller.GetNode<HumanoidAnimationController>("AnimationController");
        StateController = Controller.GetNode<HumanoidStateController>("StateController");
        
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
        if (Controller.StateController.State != HumanoidState.Combat) NextStance = CombatStance.None;
        var vector = CharacterControllerInputs.ScreenPositionMove.Normalized();
        var stanceAngle = Mathf.Atan2(vector.X, vector.Y);
        if (CharacterControllerInputs.ScreenPositionMove.Length() > 40.0f)
        {
            if (stanceAngle > Mathf.DegToRad(   0) && stanceAngle < Mathf.DegToRad( 120)) NextStance = CombatStance.Right;
            if (stanceAngle > Mathf.DegToRad(-120) && stanceAngle < Mathf.DegToRad(   0)) NextStance = CombatStance.Left;
            if (stanceAngle > Mathf.DegToRad( 120) || stanceAngle < Mathf.DegToRad(-120)) NextStance = CombatStance.Up;
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
        if (CharacterTarget.Target.GlobalPosition.DistanceTo(Controller.Target.GlobalPosition) > 1.75f) return;
        var targetInfo = CharacterTarget.CharacterInfo;
        switch (targetInfo.BlockStance)
        {
            case CombatStance.Up when AttackStance == CombatStance.Up:
            case CombatStance.Left when AttackStance == CombatStance.Left:
            case CombatStance.Right when AttackStance == CombatStance.Right:
                AnimationController.HitStun();
                break;
            default:
                CharacterTarget?.HitReceive();
                break;
        }
    }

    public void HitReceive()
    {
        AnimationController.HitStun();
        Controller.Characteristics.Health -= 50.0f;
    }
}