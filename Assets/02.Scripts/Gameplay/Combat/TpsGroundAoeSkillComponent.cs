using System.Collections.Generic;
using SystemicOverload.Phase1;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// Ground Raycast로 중심점을 정하고 NonAlloc Overlap으로 광역 피해를 적용합니다.
    /// </summary>
    [RequireComponent(typeof(InputProvider))]
    public sealed class TpsGroundAoeSkillComponent : MonoBehaviour
    {
        [Header("Aim")]
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float aimRange = 60.0f;
        [SerializeField] private LayerMask groundMask = ~0;

        [Header("Damage")]
        [SerializeField] private float radius = 5.0f;
        [SerializeField] private float maxDamage = 35.0f;
        [SerializeField] private float cooldown = 1.5f;
        [SerializeField] private LayerMask enemyMask = ~0;

        [Header("NonAlloc")]
        [SerializeField] private int bufferSize = 32;

        private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
        private InputProvider inputProvider;
        private IAttackFeedback attackFeedback;
        private Collider[] overlapBuffer;
        private float nextAllowedCastTime;
        private Vector3 lastAoeCenter;
        private bool hasLastAoeCenter;

        private void Awake()
        {
            inputProvider = GetComponent<InputProvider>();
            attackFeedback = GetComponent<IAttackFeedback>();
            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }

            EnsureBuffer();
        }

        private void OnValidate()
        {
            aimRange = Mathf.Max(0.5f, aimRange);
            radius = Mathf.Max(0.1f, radius);
            maxDamage = Mathf.Max(0.0f, maxDamage);
            cooldown = Mathf.Max(0.0f, cooldown);
            bufferSize = Mathf.Max(1, bufferSize);
        }

        private void Update()
        {
            if (!inputProvider.WasAoePressedThisFrame)
            {
                return;
            }

            if (Time.time < nextAllowedCastTime)
            {
                return;
            }

            nextAllowedCastTime = Time.time + cooldown;
            CastAoe();
            attackFeedback?.PlayAttackFeedback(AttackFeedbackKind.Area);
        }

        private void CastAoe()
        {
            if (aimCamera == null)
            {
                aimCamera = Camera.main;
                if (aimCamera == null)
                {
                    return;
                }
            }

            EnsureBuffer();

            Ray centerRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            if (!Physics.Raycast(centerRay, out RaycastHit groundHit, aimRange, groundMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            Vector3 aoeCenter = groundHit.point;
            int overlapCount = Physics.OverlapSphereNonAlloc(
                aoeCenter,
                radius,
                overlapBuffer,
                enemyMask,
                QueryTriggerInteraction.Ignore);

            if (overlapCount == overlapBuffer.Length)
            {
                Debug.LogWarning("AOE 버퍼가 가득 찼습니다. 일부 대상이 누락될 수 있습니다.");
            }

            damagedTargets.Clear();
            int appliedCount = 0;
            for (int index = 0; index < overlapCount; index++)
            {
                Collider targetCollider = overlapBuffer[index];
                IDamageable targetDamageable = targetCollider.GetComponentInParent<IDamageable>();
                if (targetDamageable == null || !targetDamageable.IsAlive)
                {
                    continue;
                }

                if (!damagedTargets.Add(targetDamageable))
                {
                    continue;
                }

                float targetDistance = Vector3.Distance(aoeCenter, targetCollider.transform.position);
                float damageRatio = 1.0f - Mathf.Clamp01(targetDistance / radius);
                float finalDamage = Mathf.Max(0.0f, maxDamage * damageRatio);

                DamagePayload payload = new DamagePayload
                {
                    Amount = finalDamage,
                    Attacker = transform
                };
                targetDamageable.ApplyDamage(in payload);
                appliedCount++;
            }

            lastAoeCenter = aoeCenter;
            hasLastAoeCenter = true;
            Debug.Log($"AOE hit count: {appliedCount}");
        }

        private void EnsureBuffer()
        {
            if (overlapBuffer != null && overlapBuffer.Length == bufferSize)
            {
                return;
            }

            overlapBuffer = new Collider[bufferSize];
        }

        private void OnDrawGizmosSelected()
        {
            if (!hasLastAoeCenter)
            {
                return;
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(lastAoeCenter, radius);
        }
    }
}
