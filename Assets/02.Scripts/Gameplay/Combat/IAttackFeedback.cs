namespace SystemicOverload.Combat
{
    public enum AttackFeedbackKind
    {
        Ranged,
        Melee,
        Magic,
        Area
    }

    public interface IAttackFeedback
    {
        void PlayAttackFeedback(AttackFeedbackKind feedbackKind);
    }
}
