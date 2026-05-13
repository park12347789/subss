using UnityEngine;

namespace SystemicOverload.Combat
{
    [DefaultExecutionOrder(100)]
    public sealed class AttackAnimationFeedback : MonoBehaviour, IAttackFeedback
    {
        private const string RangedTriggerName = "AttackRangedTrig";
        private const string MeleeTriggerName = "AttackMeleeTrig";
        private const string MagicTriggerName = "AttackMagicTrig";
        private const string AreaTriggerName = "AttackAreaTrig";

        [Header("Clip Fallback")]
        [SerializeField] private AnimationClip rangedClip;
        [SerializeField] private AnimationClip meleeClip;
        [SerializeField] private AnimationClip magicClip;
        [SerializeField] private AnimationClip areaClip;

        [Header("Animator Fallback")]
        [SerializeField] private Animator animator;

        [Header("Transform Fallback")]
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
        private AnimationClip activeClip;
        private float activeClipTime;

        private float TotalDuration => attackInTime + attackOutTime;

        private void Awake()
        {
            animator ??= GetComponent<Animator>();
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
            if (TrySetAnimatorTrigger(feedbackKind))
            {
                return;
            }

            if (TryPlayClip(feedbackKind))
            {
                return;
            }

            ResolveAnimatedRoot();
            CaptureBasePose();

            activeEulerOffset = ResolveEulerOffset(feedbackKind);
            feedbackTimer = TotalDuration;
        }

        private bool TryPlayClip(AttackFeedbackKind feedbackKind)
        {
            AnimationClip clip = ResolveClip(feedbackKind);
            if (clip == null)
            {
                return false;
            }

            activeClip = clip;
            activeClipTime = 0.0f;
            activeClip.SampleAnimation(gameObject, 0.0f);
            return true;
        }

        private bool TrySetAnimatorTrigger(AttackFeedbackKind feedbackKind)
        {
            if (animator == null)
            {
                return false;
            }

            string triggerName = ResolveTriggerName(feedbackKind);
            for (int index = 0; index < animator.parameters.Length; index++)
            {
                AnimatorControllerParameter parameter = animator.parameters[index];
                if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
                {
                    animator.SetTrigger(triggerName);
                    return true;
                }
            }

            return false;
        }

        private AnimationClip ResolveClip(AttackFeedbackKind feedbackKind)
        {
            return feedbackKind switch
            {
                AttackFeedbackKind.Melee => meleeClip,
                AttackFeedbackKind.Magic => magicClip,
                AttackFeedbackKind.Area => areaClip,
                _ => rangedClip
            };
        }

        private string ResolveTriggerName(AttackFeedbackKind feedbackKind)
        {
            return feedbackKind switch
            {
                AttackFeedbackKind.Melee => MeleeTriggerName,
                AttackFeedbackKind.Magic => MagicTriggerName,
                AttackFeedbackKind.Area => AreaTriggerName,
                _ => RangedTriggerName
            };
        }

        private void LateUpdate()
        {
            if (activeClip != null)
            {
                activeClipTime += Time.deltaTime;
                activeClip.SampleAnimation(gameObject, Mathf.Min(activeClipTime, activeClip.length));
                if (activeClipTime >= activeClip.length)
                {
                    activeClip = null;
                }

                return;
            }

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
            activeClip = null;
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
