using Godot;

public partial class HumanoidUIController : Node
{
    private HumanoidController Controller;
    public HumanoidDoll Doll { get; protected set; }
    public HumanoidStateController StateController { get; protected set; }
    public HumanoidItemManipulationController ItemManipulationController { get; protected set; }
    public HumanoidCombatController CombatController { get; protected set; }
    public CharacterControllerInputs ControllerInputs { get; protected set; }
    
    public TextureRect GrabbedItemTextureRect { get; protected set; }
    
    public override void _Ready()
    {
        Controller = GetParent<HumanoidController>();
        Doll = Controller.GetParent<HumanoidDoll>();
        StateController = Controller.GetNode<HumanoidStateController>("StateController");
        ItemManipulationController = Controller.GetNode<HumanoidItemManipulationController>("ItemManipulationController");
        GrabbedItemTextureRect = Doll.GetNode<TextureRect>("GrabbedItemTextureRect");
        CombatController = Controller.GetNode<HumanoidCombatController>("CombatController");
        ControllerInputs = Controller.GetNode<CharacterControllerInputs>("ControllerInputs");
    }
    
    public void UpdateUI(double delta)
    {
        DebugInfo.AddLine("Speed: " + Doll.LinearVelocity.Length().ToString());
        DebugInfo.AddLine(Controller.StateController.State.ToString());
        Controller.StanceIndicator.ShowIndicator();
        Controller.StanceIndicator.SetStance(CombatController.AttackStance);

        CombatController.CharacterTarget?.InfoBar.Show();
        
        if (Controller.StateController.State != HumanoidState.Interact)
        {
            GrabbedItemTextureRect.Visible = false;
            return;
        }
        
        Doll.LeftArm.ItemSlot.Draw();
        Doll.RightArm.ItemSlot.Draw();
        Controller.ItemManipulationController.CurrentStorage?.Draw();
        
        var storage = ItemManipulationController.CurrentStorage;
        var item = Doll.LeftArm.ItemSlot.Item;
        if (storage != null && item != null)
        {
            GrabbedItemTextureRect.Visible = true;
            GrabbedItemTextureRect.Texture = item.ItemRect.Texture;
            GrabbedItemTextureRect.Size = item.ItemRect.Size;
            GrabbedItemTextureRect.Position = storage.AlignPositionItem(ControllerInputs.ScreenPosition, item);
            DebugInfo.AddLine(storage.GetInventoryPositionOrRandomFree(ControllerInputs.ScreenPosition, item).ToString());
        } else GrabbedItemTextureRect.Visible = false;
        
        
        /*
        var storage = ItemManipulationController.CurrentStorage;
        if (StateController.State != HumanoidState.Combat) storage?.Draw();
        Doll.LeftArm.ItemSlot.Draw();
        Doll.RightArm.ItemSlot.Draw();
		*/
		
		
        /*
        if (HoldItem != null && State != HumanoidState.Combat)
        {
            GrabbedItemTextureRect.Visible = true;
            GrabbedItemTextureRect.Texture = HoldItem.ItemRect.Texture;
            GrabbedItemTextureRect.Size = HoldItem.ItemRect.Size;
            if (storage != null) GrabbedItemTextureRect.Position = storage.AlignPositionItem(CharacterControllerInputs.ScreenPosition, HoldItem);
            else GrabbedItemTextureRect.Position = CharacterControllerInputs.ScreenPosition - new Vector2(Inventory.InventoryCellHalfSize, Inventory.InventoryCellHalfSize);
        }
        else GrabbedItemTextureRect.Visible = false;
        */
		
        /*
        DebugInfo.AddLine("CurrentTargetItem: " + CurrentTargetItemFocus?.Name);
        DebugInfo.AddLine("FocusTargetItem: " + FocusTargetItem?.Name);
        DebugInfo.AddLine("MoveInput: " + MoveInput);
        DebugInfo.AddLine("State: " + State);
        DebugInfo.AddLine("CurrentAnimationPosition: " + _animationPlayer.CurrentAnimationPosition);
        DebugInfo.AddLine("CurrentAnimationLength: " + _animationPlayer.CurrentAnimationLength);
        DebugInfo.AddLine("CurrentAnimation: " + _animationPlayer.CurrentAnimation);
        */
    }
}