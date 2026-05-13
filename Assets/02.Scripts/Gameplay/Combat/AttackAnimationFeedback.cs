using UnityEngine;

namespace SystemicOverload.Combat
{
    [DefaultExecutionOrder(100)]
    public sealed class AttackAnimationFeedback : MonoBehaviour, IAttackFeedback
    {
        [SerializeField] private Transform animatedRoot;
        [SerializeField] private Vector3 rangedEulerOffset = new Vector3(-4.0f, 0.0f, 0.0f);
        [SerializeField] private Vector3 meleeEulerOffset = new Vector3(-10.0f, 0.0f, 0.0f);
        [SerializeField] private Vector3 magicEulerOffset = new Vector3(-3.0f, 8.0f, 0.0f);
        [SerializeField] private Vector3 areaEulerOffset = new Vector3(-6.0f, -8.0f, 0.0f);
        [SerializeField] private Vector3 scaleOffset = new Vector3(1.04f, 1.0f, 0.96f);
        [SerializeField] private float attackInTime = 0.06f;
        [SerializeField] private float attackOutTime = 0.14f;

        private Quaternion baseRotation;
        private Vector3 baseScale;
        private Vector3 activeEulerOffset;
        private float feedbackTimer;

        private float TotalDuration => attackInTime + attackOutTime;

        private void Awake()
        {
            ResolveAnimatedRoot();
            CaptureBasePose();
        }

        private void OnValidate()
        {
            attackInTime = Mathf.Max(0.01f, attackInTime);
            attackOutTime = Mathf.Max(0.01f, attackOutTime);
        }

        public void PlayAttackFeedback(AttackFeedbackKind feedbackKind)
        {
            ResolveAnimatedRoot();
            CaptureBasePose();

            activeEulerOffset = ResolveEulerOffset(feedbackKind);
            feedbackTimer = TotalDuration;
        }

        private void LateUpdate()
        {
            if (animatedRoot == null || feedbackTimer <= 0.0f)
            {
                return;
            }

            feedbackTimer -= Time.deltaTime;
            float elapsed = Mathf.Clamp(TotalDuration - feedbackTimer, 0.0f, TotalDuration);
            float weight = elapsed <= attackInTime
                ? elapsed / attackInTime
                : 1.0f - ((elapsed - attackInTime) / attackOutTime);
            weight = Mathf.SmoothStep(0.0f, 1.0f, Mathf.Clamp01(weight));

            Quaternion targetRotation = baseRotation * Quaternion.Euler(activeEulerOffset);
            animatedRoot.localRotation = Quaternion.Slerp(baseRotation, targetRotation, weight);
            animatedRoot.localScale = Vector3.Lerp(baseScale, Vector3.Scale(baseScale, scaleOffset), weight);

            if (feedbackTimer <= 0.0f)
            {
                ResetPose();
            }
        }

        private void OnDisable()
        {
            ResetPose();
        }

        private Vector3 ResolveEulerOffset(AttackFeedbackKind feedbackKind)
        {
            return feedbackKind switch
            {
                AttackFeedbackKind.Melee => meleeEulerOffset,
                AttackFeedbackKind.Magic => magicEulerOffset,
                AttackFeedbackKind.Area => areaEulerOffset,
                _ => rangedEulerOffset
            };
        }

        private void ResolveAnimatedRoot()
        {
            if (animatedRoot != null)
            {
                return;
            }

            animatedRoot = transform.Find("Geometry") ?? transform.Find("Skeleton") ?? transform;
        }

        private void CaptureBasePose()
        {
            if (animatedRoot == null || feedbackTimer > 0.0f)
            {
                return;
            }

            baseRotation = animatedRoot.localRotation;
            baseScale = animatedRoot.localScale;
        }

        private void ResetPose()
        {
            if (animatedRoot == null)
            {
                return;
            }

            animatedRoot.localRotation = baseRotation;
            animatedRoot.localScale = baseScale;
            feedbackTimer = 0.0f;
        }
    }
}
