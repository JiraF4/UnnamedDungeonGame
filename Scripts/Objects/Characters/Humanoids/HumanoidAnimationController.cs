using Dungeon.Tools;
using Godot;
using Godot.Collections;

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

    protected readonly StringName PrimaryAction = new("parameters/ActionsStateMachine/conditions/PrimaryAction");
    protected readonly StringName NotPrimaryAction = new("parameters/ActionsStateMachine/conditions/NotPrimaryAction");
    
    protected readonly StringName SecondAction = new("parameters/ActionsStateMachine/conditions/SecondAction");
    protected readonly StringName NotSecondAction = new("parameters/ActionsStateMachine/conditions/NotSecondAction");
    
    protected readonly StringName Combat = new("parameters/ActionsStateMachine/conditions/Combat");
    protected readonly StringName Interaction = new("parameters/ActionsStateMachine/conditions/Interaction");
    protected readonly StringName Idle = new("parameters/ActionsStateMachine/conditions/Idle");
    
    protected readonly StringName HasTargetItem = new("parameters/ActionsStateMachine/conditions/HasTargetItem");
    protected readonly StringName NotHasTargetItem = new("parameters/ActionsStateMachine/conditions/NotHasTargetItem");
    
    protected readonly StringName Attack = new("parameters/ActionsStateMachine/conditions/Attack");
    
    protected readonly StringName StanceNone = new("parameters/ActionsStateMachine/conditions/StanceNone");
    protected readonly StringName StanceUp = new("parameters/ActionsStateMachine/conditions/StanceUp");
    protected readonly StringName StanceLeft = new("parameters/ActionsStateMachine/conditions/StanceLeft");
    protected readonly StringName StanceRight = new("parameters/ActionsStateMachine/conditions/StanceRight");
    
    protected readonly StringName Stunned = new("parameters/ActionsStateMachine/conditions/Stunned");
    
    protected readonly StringName GrabItem = new("parameters/ActionsStateMachine/conditions/GrabItem");
    protected readonly StringName DropItem = new("parameters/ActionsStateMachine/conditions/DropItem");
    
    protected readonly StringName WalkSpace2D = new("parameters/WalkSpace2D/blend_position");
    protected readonly StringName RotationDirectionSpace1D = new("parameters/RotationDirectionSpace1D/blend_position");
    protected readonly StringName LegsBlend = new("parameters/LegsBlend/blend_amount");

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
        DebugInfo.AddLine(Controller.Doll.LinearVelocity.ToString());
        UpdateConditions();
        base._Process(delta);
    }


    public void UpdateConditions()
    {
        var forwardVelocity = Controller.Doll.LinearVelocity.Normalized().Rotated(Vector3.Up, -Controller.Doll.Rotation.Y);
        var walkVector = new Vector2(
            Controller.Doll.LinearVelocity.Length() / Controller.MoveMaxSpeed * forwardVelocity.Z,
            Controller.Doll.LinearVelocity.Length() / Controller.MoveMaxSpeed * forwardVelocity.X
        );
        if (Controller.Doll.LinearVelocity.Length() < 0.5f)
        {
            AnimationTree.Set(LegsBlend, 1);
            AnimationTree.Set(WalkSpace2D, new Vector2());
            AnimationTree.Set(RotationDirectionSpace1D, Doll.BodyRotation.Y / 1.0f);
        }
        else
        {
            AnimationTree.Set(WalkSpace2D, walkVector);
            AnimationTree.Set(LegsBlend, 0);
        }

        if (Multiplayer.GetUniqueId() != GetMultiplayerAuthority()) return;
        
        AnimationTree.Set(PrimaryAction, CharacterControllerInputs.PrimaryAction);
        AnimationTree.Set(NotPrimaryAction, !CharacterControllerInputs.PrimaryAction);
        AnimationTree.Set(SecondAction, CharacterControllerInputs.SecondAction);
        AnimationTree.Set(NotSecondAction, !CharacterControllerInputs.SecondAction);
        
        AnimationTree.Set(HasTargetItem, ItemManipulationController.CurrentItem != null || ItemManipulationController.CurrentStorage != null);
        AnimationTree.Set(NotHasTargetItem, ItemManipulationController.CurrentItem == null && ItemManipulationController.CurrentStorage == null);
        
        AnimationTree.Set(Interaction, !CombatController.HasTarget() && CharacterControllerInputs.InteractMode);
        AnimationTree.Set(Combat, CombatController.HasTarget() && CharacterControllerInputs.InteractMode);
        AnimationTree.Set(Idle, !CharacterControllerInputs.InteractMode);
        
        AnimationTree.Set(StanceNone, CombatController.NextStance == CombatStance.None || !CharacterControllerInputs.InteractMode);
        AnimationTree.Set(StanceUp, CombatController.NextStance == CombatStance.Up);
        AnimationTree.Set(StanceLeft, CombatController.NextStance == CombatStance.Left);
        AnimationTree.Set(StanceRight, CombatController.NextStance == CombatStance.Right);
        AnimationTree.Set(Attack, CharacterControllerInputs.PrimaryActionJustPressed);

        
        AnimationTree.Set(GrabItem, CharacterControllerInputs.PrimaryActionJustPressed && ItemManipulationController.CurrentItem != null && Doll.LeftArm.ItemSlot.Item == null);
        AnimationTree.Set(DropItem, CharacterControllerInputs.PrimaryActionJustPressed && Doll.LeftArm.ItemSlot.Item != null);
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
    
    public virtual void CollectSyncData(Dictionary syncData)
    {
        syncData["ActionAnimation"] = StateMachine.GetCurrentNode();
    }

    public virtual void ApplySyncData(Dictionary syncData)
    {
        StateMachine.Travel((StringName) syncData["ActionAnimation"]);
    }
}