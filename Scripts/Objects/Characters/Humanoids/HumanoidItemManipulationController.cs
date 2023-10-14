using Godot;

public partial class HumanoidItemManipulationController : Node
{
    public HumanoidController Controller { get; protected set; }
    public HumanoidDoll Doll { get; protected set; }
    public CharacterControllerInputs ControllerInputs { get; protected set; }
    
    public Storage FocusTargetStorage { get; protected set; }
    public Item FocusTargetItem { get; protected set; }
    public Storage TargetStorage { get; protected set; }
    public Item TargetItem { get; protected set; }
    
    public Storage CurrentStorage => FocusTargetStorage ?? TargetStorage;
    public Item CurrentItem => FocusTargetItem ?? TargetItem;

    protected TextureRect GrabbedItemTextureRect;
    protected Node3D GrabbingArm;
    
    public override void _Ready()
    {
        Controller = GetParent<HumanoidController>();
        Doll = Controller.GetParent<HumanoidDoll>();
        ControllerInputs = Controller.GetNode<CharacterControllerInputs>("ControllerInputs");
        Doll.GetNode<TextureRect>("GrabbedItemTextureRect");
        GrabbingArm = Doll.LeftArm;
        
        base._Ready();
    }
    
    Storage GetTargetStorage() {
        if (Doll.LeftArm.ItemSlot.IsScreenPositionInside(ControllerInputs.ScreenPosition)) return Doll.LeftArm.ItemSlot;
        if (Doll.RightArm.ItemSlot.IsScreenPositionInside(ControllerInputs.ScreenPosition)) return Doll.RightArm.ItemSlot;
        var targetNode = ControllerInputs.TargetNode;
        var inventory = (Inventory) targetNode?.FindChild("Inventory");
        return inventory;
    }
    
    Item GetTargetItem() {
        if (CurrentStorage is Inventory inventory) return inventory.GetItemScreenPosition(ControllerInputs.ScreenPosition);
        if (CurrentStorage is Slot slot) return slot.Item;
        var targetNode = ControllerInputs.TargetNode;
        if (targetNode is Item item) return item;
        return null;
    }

    public void UpdateTarget()
    {
        TargetStorage = GetTargetStorage();
        TargetItem = GetTargetItem();
    }

    public void UpdateTargetFocus()
    {
        FocusTargetStorage = TargetStorage;
        FocusTargetItem = TargetItem;
    }

    public void ClearTargetFocus()
    {
        FocusTargetStorage = null;
        FocusTargetItem = null;
    }

    public void GrabItem()
    {
        if (CurrentItem != null)
        {
            var item = Doll.LeftArm.ItemSlot.Item;
            item?.TransferClient(null, new Vector2I());
            CurrentItem.TransferClient(Doll.LeftArm.ItemSlot, new Vector2I());
        }
    }
    
    public void DropItem()
    {
        var item = Doll.LeftArm.ItemSlot.Item;
        item?.TransferClient(CurrentStorage, CurrentStorage?.GetInventoryPositionOrRandomFree(ControllerInputs.ScreenPosition, item) ?? new Vector2I());
    }

    public void DropItems()
    {
        Doll.LeftArm.ItemSlot.Item?.TransferClient(null, new Vector2I());
        Doll.RightArm.ItemSlot.Item?.TransferClient(null, new Vector2I());
    }
}