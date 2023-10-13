public partial class HumaniodInfo : CharacterInfo
{
    public override void ReadInfo(double delta)
    {
        AttackStance = ((HumanoidController) Controller).CombatController.AttackStance;
        BlockStance = ((HumanoidController) Controller).CombatController.BlockStance;
        CurrentTarget = ((HumanoidController) Controller).CombatController.CharacterTarget?.GetPath();
        base.ReadInfo(delta);
    }
}