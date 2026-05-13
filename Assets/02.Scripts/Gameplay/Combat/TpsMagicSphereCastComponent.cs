using SystemicOverload.Phase1;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// Fires a visible round magic projectile toward the camera-center aim point.
    /// </summary>
    [RequireComponent(typeof(InputProvider))]
    public sealed class TpsMagicSphereCastComponent : MonoBehaviour
    {
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private float projectileSpeed = 18.0f;
        [SerializeField] private float projectileLifetime = 2.0f;

        [Header("Aim")]
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float range = 30.0f;
        [SerializeField] private float radius = 0.35f;

        [Header("Damage")]
        [SerializeField] private float damage = 20.0f;
        [SerializeField] private float cooldown = 0.25f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Feedback")]
        [SerializeField] private GameObject hitFxPrefab;
        [SerializeField] private bool drawGizmo = true;

        private InputProvider inputProvider;
        private IAttackFeedback attackFeedback;
        private float nextAllowedCastTime;
        private Vector3 lastCastOrigin;
        private Vector3 lastCastDirection = Vector3.forward;
        private float lastCastDistance;
        private bool hasLastCast;

        private void Awake()
        {
            inputProvider = GetComponent<InputProvider>();
            attackFeedback = GetComponent<IAttackFeedback>();
            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }
        }

        private void OnValidate()
        {
            range = Mathf.Max(0.5f, range);
            radius = Mathf.Max(0.01f, radius);
            damage = Mathf.Max(0.0f, damage);
            cooldown = Mathf.Max(0.0f, cooldown);
            projectileSpeed = Mathf.Max(0.1f, projectileSpeed);
            projectileLifetime = Mathf.Max(0.05f, projectileLifetime);
        }

        private void Update()
        {
            if (!inputProvider.WasMagicPressedThisFrame)
            {
                return;
            }

            if (Time.time < nextAllowedCastTime)
            {
                return;
            }

            nextAllowedCastTime = Time.time + cooldown;
            CastMagic();
            attackFeedback?.PlayAttackFeedback(AttackFeedbackKind.Magic);
        }

        private void CastMagic()
        {
            if (aimCamera == null)
            {
                aimCamera = Camera.main;
                if (aimCamera == null)
                {
                    return;
                }
            }

            Ray centerRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            Vector3 aimPoint = ResolveAimPoint(centerRay, out float aimDistance);
            Vector3 spawnPosition = ResolveProjectileOrigin();
            Vector3 launchDirection = aimPoint - spawnPosition;
            if (launchDirection.sqrMagnitude < 0.0001f)
            {
                launchDirection = centerRay.direction;
            }

            lastCastOrigin = spawnPosition;
            lastCastDirection = launchDirection.normalized;
            lastCastDistance = Mathf.Min(range, Mathf.Max(aimDistance, launchDirection.magnitude));
            hasLastCast = true;

            if (projectilePrefab == null)
            {
                ApplyInstantSphereCast(centerRay);
                return;
            }

            GameObject projectileObject = Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(launchDirection.normalized, Vector3.up));
            MagicProjectileComponent projectile = projectileObject.GetComponent<MagicProjectileComponent>();
            if (projectile == null)
            {
                Debug.LogWarning("Magic projectile prefab is missing MagicProjectileComponent.", projectileObject);
                Destroy(projectileObject);
                ApplyInstantSphereCast(centerRay);
                return;
            }

            projectile.Launch(
                transform,
                spawnPosition,
                launchDirection,
                damage,
                projectileSpeed,
                radius,
                projectileLifetime,
                hitMask,
                hitFxPrefab);

            if (drawGizmo)
            {
                Debug.DrawRay(spawnPosition, launchDirection.normalized * Mathf.Min(range, launchDirection.magnitude), Color.cyan, 0.5f);
            }
        }

        private Vector3 ResolveAimPoint(Ray centerRay, out float aimDistance)
        {
            if (TrySphereCastExcludeSelf(centerRay, out RaycastHit hitInfo))
            {
                aimDistance = hitInfo.distance;
                return hitInfo.point;
            }

            aimDistance = range;
            return centerRay.origin + centerRay.direction * range;
        }

        private Vector3 ResolveProjectileOrigin()
        {
            if (projectileSpawnPoint != null)
            {
                return projectileSpawnPoint.position;
            }

            return transform.position + Vector3.up * 1.0f + transform.forward * 1.2f;
        }

        private void ApplyInstantSphereCast(Ray centerRay)
        {
            if (!TrySphereCastExcludeSelf(centerRay, out RaycastHit hitInfo))
            {
                return;
            }

            IDamageable targetDamageable = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (targetDamageable != null && targetDamageable.IsAlive)
            {
                DamagePayload payload = new DamagePayload
                {
                    Amount = damage,
                    Attacker = transform
                };
                targetDamageable.ApplyDamage(in payload);
            }

            if (hitFxPrefab != null)
            {
                Quaternion rotation = hitInfo.normal.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(hitInfo.normal)
                    : Quaternion.identity;
                Instantiate(hitFxPrefab, hitInfo.point, rotation);
            }

            Debug.Log($"Magic hit: {hitInfo.collider.name} / {hitInfo.distance:F1}m");
        }

        private bool TrySphereCastExcludeSelf(Ray ray, out RaycastHit bestHit)
        {
            RaycastHit[] allHits = Physics.SphereCastAll(ray, radius, range, hitMask, QueryTriggerInteraction.Ignore);
            bestHit = default;
            float bestDistance = float.MaxValue;
            bool found = false;
            for (int index = 0; index < allHits.Length; index++)
            {
                RaycastHit candidateHit = allHits[index];
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

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmo || !hasLastCast)
            {
                return;
            }

            Vector3 castEnd = lastCastOrigin + lastCastDirection * lastCastDistance;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(lastCastOrigin, castEnd);
            Gizmos.DrawWireSphere(lastCastOrigin, radius);
            Gizmos.DrawWireSphere(castEnd, radius);
        }
    }
}
