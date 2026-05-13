using SystemicOverload.Phase1;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 기본 원거리(레이캐스트) 공격과 발사 간격을 처리합니다. Animator가 있으면 Attack 트리거를 전달합니다.
    /// </summary>
    [RequireComponent(typeof(InputProvider))]
    public sealed class CombatComponent : MonoBehaviour
    {
        private const string AttackTriggerParameterName = "AttackTrig";

        [Header("Weapon")]
        [SerializeField] private float damage = 12.0f;
        [SerializeField] private float shotsPerSecond = 4.0f;
        [SerializeField] private float maxRange = 40.0f;
        [SerializeField] private float rayOriginHeight = 1.0f;
        [SerializeField] private float rayStartForwardOffset = 0.35f;
        [SerializeField] private LayerMask hitLayerMask = ~0;
        [SerializeField] private float cameraAimRayMaxDistance = 200.0f;
        [SerializeField] private float muzzleAimPaddingDistance = 0.1f;
        [SerializeField] private bool drawDebugRay = true;

        [Header("Visible Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 42.0f;
        [SerializeField] private float projectileRadius = 0.08f;
        [SerializeField] private float projectileLifetime = 1.2f;
        [SerializeField] private GameObject hitFxPrefab;

        [Header("References")]
        [SerializeField] private MovementComponent movementComponent;
        [SerializeField] private Animator animator;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform muzzlePoint;

        private InputProvider inputProvider;
        private IAttackFeedback attackFeedback;
        private float nextAllowedShotTime;

        private static readonly int AttackTriggerHash = Animator.StringToHash(AttackTriggerParameterName);

        private void Awake()
        {
            inputProvider = GetComponent<InputProvider>();
            movementComponent ??= GetComponent<MovementComponent>();
            animator ??= GetComponent<Animator>();
            attackFeedback = GetComponent<IAttackFeedback>();
        }

        private void OnValidate()
        {
            damage = Mathf.Max(0.0f, damage);
            shotsPerSecond = Mathf.Max(0.01f, shotsPerSecond);
            maxRange = Mathf.Max(0.1f, maxRange);
            cameraAimRayMaxDistance = Mathf.Max(0.1f, cameraAimRayMaxDistance);
            muzzleAimPaddingDistance = Mathf.Max(0.0f, muzzleAimPaddingDistance);
            projectileSpeed = Mathf.Max(0.1f, projectileSpeed);
            projectileRadius = Mathf.Max(0.01f, projectileRadius);
            projectileLifetime = Mathf.Max(0.05f, projectileLifetime);
        }

        private void Update()
        {
            if (!inputProvider.WasAttackPressedThisFrame)
            {
                return;
            }

            if (Time.time < nextAllowedShotTime)
            {
                return;
            }

            float interval = 1.0f / shotsPerSecond;
            nextAllowedShotTime = Time.time + interval;

            TryFireHitScan();
            if (attackFeedback != null)
            {
                attackFeedback.PlayAttackFeedback(AttackFeedbackKind.Ranged);
            }
            else
            {
                TrySetAttackTrigger();
            }
        }

        /// <summary>
        /// TPS 정석 2-Step Raycast를 수행합니다.
        /// 1) Camera center Ray로 조준점을 확정하고
        /// 2) 실제 발사점(Muzzle)에서 조준점 방향으로 다시 Raycast해 최종 피격을 결정합니다.
        /// </summary>
        private void TryFireHitScan()
        {
            if (!TryResolveCameraRay(out Ray cameraRay))
            {
                return;
            }

            Vector3 step1AimPoint = ResolveCameraAimPoint(cameraRay);
            Vector3 muzzleOrigin = ResolveMuzzleOrigin();
            Vector3 muzzleToAim = step1AimPoint - muzzleOrigin;
            if (muzzleToAim.sqrMagnitude < 0.0001f)
            {
                muzzleToAim = transform.forward;
            }

            Vector3 step2Direction = muzzleToAim.normalized;
            float step2Distance = Mathf.Min(maxRange, muzzleToAim.magnitude + muzzleAimPaddingDistance);
            if (step2Distance <= 0.0001f)
            {
                step2Distance = maxRange;
            }

            if (drawDebugRay)
            {
                Debug.DrawRay(cameraRay.origin, cameraRay.direction * maxRange, Color.yellow, 0.2f);
                Debug.DrawRay(muzzleOrigin, step2Direction * step2Distance, Color.cyan, 0.2f);
            }

            if (projectilePrefab != null && TryFireProjectile(muzzleOrigin, step2Direction))
            {
                return;
            }

            if (!TryRaycastExcludeSelf(muzzleOrigin, step2Direction, step2Distance, out RaycastHit hitInfo))
            {
                return;
            }

            IDamageable damageable = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            DamagePayload payload = new DamagePayload
            {
                Amount = damage,
                Attacker = transform
            };
            damageable.ApplyDamage(in payload);

            if (drawDebugRay)
            {
                Debug.DrawLine(muzzleOrigin, hitInfo.point, Color.green, 0.25f);
            }
        }

        private bool TryResolveCameraRay(out Ray cameraRay)
        {
            cameraRay = default;
            Camera targetCamera = aimCamera;
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera == null)
            {
                return false;
            }

            cameraRay = targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            return true;
        }

        private Vector3 ResolveCameraAimPoint(Ray cameraRay)
        {
            if (TryRaycastExcludeSelf(cameraRay.origin, cameraRay.direction, cameraAimRayMaxDistance, out RaycastHit cameraHit))
            {
                return cameraHit.point;
            }

            return cameraRay.origin + cameraRay.direction * maxRange;
        }

        private Vector3 ResolveMuzzleOrigin()
        {
            if (muzzlePoint != null)
            {
                return muzzlePoint.position;
            }

            return transform.position + Vector3.up * rayOriginHeight + transform.forward * rayStartForwardOffset;
        }

        private bool TryFireProjectile(Vector3 origin, Vector3 direction)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction, Vector3.up));
            MagicProjectileComponent projectile = projectileObject.GetComponent<MagicProjectileComponent>();
            if (projectile == null)
            {
                Debug.LogWarning("Ranged projectile prefab is missing MagicProjectileComponent.", projectileObject);
                Destroy(projectileObject);
                return false;
            }

            projectile.Launch(
                transform,
                origin,
                direction,
                damage,
                projectileSpeed,
                projectileRadius,
                projectileLifetime,
                hitLayerMask,
                hitFxPrefab);
            return true;
        }

        private bool TryRaycastExcludeSelf(Vector3 origin, Vector3 direction, float distance, out RaycastHit bestHit)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, hitLayerMask, QueryTriggerInteraction.Ignore);
            bestHit = default;
            float bestDistance = float.MaxValue;
            bool found = false;
            for (int index = 0; index < hits.Length; index++)
            {
                RaycastHit candidateHit = hits[index];
                if (candidateHit.collider == null)
                {
                    continue;
                }

                if (candidateHit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (candidateHit.distance < bestDistance)
                {
                    bestDistance = candidateHit.distance;
                    bestHit = candidateHit;
                    found = true;
                }
            }

            return found;
        }

        private void TrySetAttackTrigger()
        {
            if (animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == AttackTriggerParameterName && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(AttackTriggerHash);
                    return;
                }
            }
        }
    }
}
