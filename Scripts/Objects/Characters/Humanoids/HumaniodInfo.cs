public partial class HumaniodInfo : CharacterInfo
{
    public override CombatStance CombatStance => ((HumanoidController) Controller).CombatController.Stance;
    public override CharacterController CurrentTarget => ((HumanoidController) Controller).CombatController.CharacterTarget;
}