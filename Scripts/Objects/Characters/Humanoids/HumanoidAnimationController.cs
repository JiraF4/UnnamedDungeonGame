using Godot;

public partial class HumanoidAnimationController : Node
{
    public HumanoidController Controller { get; private set; }
    public HumanoidDoll Doll { get; protected set; }
    public CharacterControllerInputs CharacterControllerInputs { get; protected set; }
    public HumanoidItemManipulationController ItemManipulationController { get; protected set; }
    public HumanoidCombatController CombatController { get; protected set; }
    
    protected AnimationPlayer AnimationPlayer;
    protected AnimationTree AnimationTree;
    protected AnimationNodeStateMachinePlayback StateMachine;

    protected StringName _primaryAction = new("parameters/ActionsStateMachine/conditions/PrimaryAction");
    protected StringName _notPrimaryAction = new("parameters/ActionsStateMachine/conditions/NotPrimaryAction");
    
    protected StringName _secondAction = new("parameters/ActionsStateMachine/conditions/SecondAction");
    protected StringName _notSecondAction = new("parameters/ActionsStateMachine/conditions/NotSecondAction");
    
    protected StringName _combat = new("parameters/ActionsStateMachine/conditions/Combat");
    protected StringName _interaction = new("parameters/ActionsStateMachine/conditions/Interaction");
    protected StringName _idle = new("parameters/ActionsStateMachine/conditions/Idle");
    
    protected StringName _hasTargetItem = new("parameters/ActionsStateMachine/conditions/HasTargetItem");
    protected StringName _notHasTargetItem = new("parameters/ActionsStateMachine/conditions/NotHasTargetItem");
    
    protected StringName _attack = new("parameters/ActionsStateMachine/conditions/Attack");
    
    protected StringName _stanceNone = new("parameters/ActionsStateMachine/conditions/StanceNone");
    protected StringName _stanceUp = new("parameters/ActionsStateMachine/conditions/StanceUp");
    protected StringName _stanceLeft = new("parameters/ActionsStateMachine/conditions/StanceLeft");
    protected StringName _stanceRight = new("parameters/ActionsStateMachine/conditions/StanceRight");
    
    protected StringName _stunned = new("parameters/ActionsStateMachine/conditions/Stunned");
    
    protected StringName _grabItem = new("parameters/ActionsStateMachine/conditions/GrabItem");
    protected StringName _dropItem = new("parameters/ActionsStateMachine/conditions/DropItem");
    
    protected StringName _WalkSpace2D = new("parameters/WalkSpace2D/blend_position");
    
    public override void _Ready()
    {
        Controller = GetParent<HumanoidController>();
        Doll = Controller.GetParent<HumanoidDoll>();
        CharacterControllerInputs = Controller.GetNode<CharacterControllerInputs>("ControllerInputs");
        ItemManipulationController = Controller.GetNode<HumanoidItemManipulationController>("ItemManipulationController");
        CombatController = Controller.GetNode<HumanoidCombatController>("CombatController");
        AnimationPlayer = Doll.GetNode<AnimationPlayer>("AnimationPlayer");
        AnimationTree = Doll.GetNode<AnimationTree>("AnimationTree");
        StateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/ActionsStateMachine/playback");

        AnimationTree.Active = true;
        
        base._Ready();
    }

    public override void _Process(double delta)
    {
        UpdateConditions();
        DebugInfo.AddLine("Animation: " + GetAnimation());
        base._Process(delta);
    }

    public void UpdateConditions()
    {
        AnimationTree.Set(_primaryAction, CharacterControllerInputs.PrimaryAction);
        AnimationTree.Set(_notPrimaryAction, !CharacterControllerInputs.PrimaryAction);
        AnimationTree.Set(_secondAction, CharacterControllerInputs.SecondAction);
        AnimationTree.Set(_notSecondAction, !CharacterControllerInputs.SecondAction);
        
        AnimationTree.Set(_hasTargetItem, ItemManipulationController.CurrentItem != null || ItemManipulationController.CurrentStorage != null);
        AnimationTree.Set(_notHasTargetItem, ItemManipulationController.CurrentItem == null && ItemManipulationController.CurrentStorage == null);
        
        AnimationTree.Set(_interaction, !CombatController.HasTarget() && CharacterControllerInputs.InteractMode);
        AnimationTree.Set(_combat, CombatController.HasTarget() && CharacterControllerInputs.InteractMode);
        AnimationTree.Set(_idle, !CharacterControllerInputs.InteractMode);
        
        AnimationTree.Set(_stanceNone, CombatController.NextStance == CombatStance.None || !CharacterControllerInputs.InteractMode);
        AnimationTree.Set(_stanceUp, CombatController.NextStance == CombatStance.Up);
        AnimationTree.Set(_stanceLeft, CombatController.NextStance == CombatStance.Left);
        AnimationTree.Set(_stanceRight, CombatController.NextStance == CombatStance.Right);
        AnimationTree.Set(_attack, CharacterControllerInputs.PrimaryActionJustPressed);


        var moveDotX = Controller.Doll.LinearVelocity.Normalized().Dot(Controller.Doll.GlobalTransform.Basis.Z);
        var moveDotY = Controller.Doll.LinearVelocity.Normalized().Dot(Controller.Doll.GlobalTransform.Basis.X);
        var walkVector = new Vector2(
            Controller.Doll.LinearVelocity.Length() / Controller.MoveMaxSpeed * moveDotX,
            Controller.Doll.LinearVelocity.Length() / Controller.MoveMaxSpeed * moveDotY
        );
        AnimationTree.Set(_WalkSpace2D, walkVector);
        
        AnimationTree.Set(_grabItem, CharacterControllerInputs.PrimaryActionJustPressed && ItemManipulationController.CurrentItem != null && Doll.LeftArm.ItemSlot.Item == null);
        AnimationTree.Set(_dropItem, CharacterControllerInputs.PrimaryActionJustPressed && Doll.LeftArm.ItemSlot.Item != null);
    }

    public string GetAnimation()
    {
        return StateMachine.GetCurrentNode();
    }

    public void HitStun()
    {
        StateMachine.Travel("CombatStun");
    }
    
    public void Die()
    {
        StateMachine.Travel("Die");
    }
}