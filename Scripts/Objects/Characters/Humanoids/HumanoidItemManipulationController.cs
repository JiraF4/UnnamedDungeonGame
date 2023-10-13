using Godot;

public partial class HumanoidItemManipulationController : Node
{
    public HumanoidController Controller { get; protected set; }
    public HumanoidDoll Doll { get; protected set; }
    public CharacterControllerInputs CharacterControllerInputs { get; protected set; }
    
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
        CharacterControllerInputs = Controller.GetNode<CharacterControllerInputs>("ControllerInputs");
        Doll.GetNode<TextureRect>("GrabbedItemTextureRect");
        GrabbingArm = Doll.LeftArm;
        
        base._Ready();
    }
    
    Storage GetTargetStorage() {
        if (Doll.LeftArm.ItemSlot.IsScreenPositionInside(CharacterControllerInputs.ScreenPosition)) return Doll.LeftArm.ItemSlot;
        if (Doll.RightArm.ItemSlot.IsScreenPositionInside(CharacterControllerInputs.ScreenPosition)) return Doll.RightArm.ItemSlot;
        var targetNode = CharacterControllerInputs.TargetNode;
        var inventory = (Inventory) targetNode?.FindChild("Inventory");
        return inventory;
    }
    
    Item GetTargetItem() {
        if (CurrentStorage is Inventory inventory) return inventory.GetItemScreenPosition(CharacterControllerInputs.ScreenPosition);
        if (CurrentStorage is Slot slot) return slot.Item;
        var targetNode = CharacterControllerInputs.TargetNode;
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
            if (item != null)
            {
                Doll.LeftArm.ItemSlot.DropItem();
                if (item.Storage == null) item.GlobalPosition = CurrentItem.GlobalPosition;
            }
            CurrentItem.Storage?.ExtractItem(CurrentItem);
            Doll.LeftArm.ItemSlot.InsertItem(new Vector2I(), CurrentItem);
        }
    }
    
    public void DropItem()
    {
        var item = Doll.LeftArm.ItemSlot.Item;
        Doll.LeftArm.ItemSlot.DropItem();
        CurrentStorage?.InsertItem(new Vector2I(), item);
        if (CurrentItem != null) Doll.LeftArm.ItemSlot.InsertItem(new Vector2I(), CurrentItem);
    }

    public void DropItems()
    {
        Doll.LeftArm.ItemSlot.DropItem();
        Doll.RightArm.ItemSlot.DropItem();
    }
}