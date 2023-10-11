using Godot;

public partial class HumanoidAnimationController : Node
{
    public HumanoidController Controller { get; private set; }
    public HumanoidDoll Doll { get; protected set; }
    public CharacterControllerInputs CharacterControllerInputs { get; protected set; }
    public HumanoidItemManipulationController ItemManipulationController { get; protected set; }
    
    protected AnimationPlayer AnimationPlayer;
    protected AnimationTree AnimationTree;
    protected AnimationNodeStateMachinePlayback StateMachine;

    protected StringName _primaryAction = new("parameters/conditions/PrimaryAction");
    protected StringName _notPrimaryAction = new("parameters/conditions/NotPrimaryAction");
    
    protected StringName _secondAction = new("parameters/conditions/SecondAction");
    protected StringName _notSecondAction = new("parameters/conditions/NotSecondAction");
    
    protected StringName _combat = new("parameters/conditions/Combat");
    protected StringName _interaction = new("parameters/conditions/Interaction");
    
    protected StringName _hasTargetItem = new("parameters/conditions/HasTargetItem");
    protected StringName _notHasTargetItem = new("parameters/conditions/NotHasTargetItem");
    
    protected StringName _stunned = new("parameters/conditions/Stunned");
    
    protected StringName _grabItem = new("parameters/conditions/GrabItem");
    protected StringName _dropItem = new("parameters/conditions/DropItem");
    
    public override void _Ready()
    {
        Controller = GetParent<HumanoidController>();
        Doll = Controller.GetParent<HumanoidDoll>();
        CharacterControllerInputs = Controller.GetNode<CharacterControllerInputs>("CharacterControllerInputs");
        ItemManipulationController = Controller.GetNode<HumanoidItemManipulationController>("ItemManipulationController");
        AnimationPlayer = Doll.GetNode<AnimationPlayer>("AnimationPlayer");
        AnimationTree = Doll.GetNode<AnimationTree>("AnimationTree");
        StateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");

        AnimationTree.Active = true;
        
        base._Ready();
    }

    public override void _Process(double delta)
    {
        UpdateConditions();
        DebugInfo.AddLine(GetAnimation());
        DebugInfo.AddLine(AnimationTree.Get(_interaction).ToString());
        DebugInfo.AddLine(AnimationTree.Get(_combat).ToString());
        base._Process(delta);
    }

    public void UpdateConditions()
    {
        AnimationTree.Set(_primaryAction, CharacterControllerInputs.PrimaryAction);
        AnimationTree.Set(_notPrimaryAction, !CharacterControllerInputs.PrimaryAction);
        AnimationTree.Set(_secondAction, CharacterControllerInputs.SecondAction);
        AnimationTree.Set(_notSecondAction, !CharacterControllerInputs.SecondAction);
        AnimationTree.Set(_interaction, CharacterControllerInputs.InteractMode);
        AnimationTree.Set(_combat, !CharacterControllerInputs.InteractMode);
        AnimationTree.Set(_hasTargetItem, ItemManipulationController.CurrentItem != null || ItemManipulationController.CurrentStorage != null);
        AnimationTree.Set(_notHasTargetItem, ItemManipulationController.CurrentItem == null && ItemManipulationController.CurrentStorage == null);
        
        AnimationTree.Set(_grabItem, CharacterControllerInputs.PrimaryActionJustPressed && ItemManipulationController.CurrentItem != null && Doll.LeftArm.ItemSlot.Item == null);
        AnimationTree.Set(_dropItem, CharacterControllerInputs.PrimaryActionJustPressed && Doll.LeftArm.ItemSlot.Item != null);
    }

    public string GetAnimation()
    {
        return StateMachine.GetCurrentNode();
    }
}