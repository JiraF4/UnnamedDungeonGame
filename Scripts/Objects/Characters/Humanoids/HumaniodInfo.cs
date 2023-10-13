public partial class HumaniodInfo : CharacterInfo
{
    public override CombatStance AttackStance => ((HumanoidController) Controller).CombatController.AttackStance;
    public override CombatStance BlockStance => ((HumanoidController) Controller).CombatController.BlockStance;
    public override CharacterController CurrentTarget => ((HumanoidController) Controller).CombatController.CharacterTarget;
}