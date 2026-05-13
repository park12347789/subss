using UnityEngine;

namespace SystemicOverload.Combat
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class MagicProjectileComponent : MonoBehaviour
    {
        [SerializeField] private float speed = 18.0f;
        [SerializeField] private float radius = 0.35f;
        [SerializeField] private float lifetime = 2.0f;
        [SerializeField] private float damage = 20.0f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private GameObject hitFxPrefab;

        private Transform attacker;
        private Vector3 direction = Vector3.forward;
        private float aliveTime;
        private bool launched;

        private void Awake()
        {
            ConfigureCollider();
        }

        private void OnValidate()
        {
            speed = Mathf.Max(0.1f, speed);
            radius = Mathf.Max(0.01f, radius);
            lifetime = Mathf.Max(0.05f, lifetime);
            damage = Mathf.Max(0.0f, damage);
            ConfigureCollider();
        }

        public void Launch(
            Transform sourceAttacker,
            Vector3 spawnPosition,
            Vector3 launchDirection,
            float launchDamage,
            float launchSpeed,
            float launchRadius,
            float launchLifetime,
            LayerMask launchHitMask,
            GameObject launchHitFxPrefab)
        {
            attacker = sourceAttacker;
            transform.position = spawnPosition;
            direction = launchDirection.sqrMagnitude > 0.0001f
                ? launchDirection.normalized
                : transform.forward;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            damage = Mathf.Max(0.0f, launchDamage);
            speed = Mathf.Max(0.1f, launchSpeed);
            radius = Mathf.Max(0.01f, launchRadius);
            lifetime = Mathf.Max(0.05f, launchLifetime);
            hitMask = launchHitMask;
            hitFxPrefab = launchHitFxPrefab;
            aliveTime = 0.0f;
            launched = true;
            ConfigureCollider();
        }

        private void Update()
        {
            if (!launched)
            {
                launched = true;
                direction = transform.forward.sqrMagnitude > 0.0001f ? transform.forward.normalized : Vector3.forward;
            }

            float stepDistance = speed * Time.deltaTime;
            if (TryHit(stepDistance, out RaycastHit hitInfo))
            {
                ApplyHit(hitInfo);
                return;
            }

            transform.position += direction * stepDistance;
            aliveTime += Time.deltaTime;
            if (aliveTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private bool TryHit(float distance, out RaycastHit bestHit)
        {
            RaycastHit[] hits = Physics.SphereCastAll(
                transform.position,
                radius,
                direction,
                distance,
                hitMask,
                QueryTriggerInteraction.Ignore);

            bestHit = default;
            float bestDistance = float.MaxValue;
            bool found = false;
            for (int index = 0; index < hits.Length; index++)
            {
                RaycastHit candidate = hits[index];
                if (candidate.collider == null)
                {
                    continue;
                }

                Transform candidateTransform = candidate.collider.transform;
                if (candidateTransform.IsChildOf(transform))
                {
                    continue;
                }

                if (attacker != null && candidateTransform.IsChildOf(attacker))
                {
                    continue;
                }

                if (candidate.distance < bestDistance)
                {
                    bestDistance = candidate.distance;
                    bestHit = candidate;
                    found = true;
                }
            }

            return found;
        }

        private void ApplyHit(RaycastHit hitInfo)
        {
            IDamageable damageable = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                DamagePayload payload = new DamagePayload
                {
                    Amount = damage,
                    Attacker = attacker
                };
                damageable.ApplyDamage(in payload);
            }

            if (hitFxPrefab != null)
            {
                Quaternion rotation = hitInfo.normal.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(hitInfo.normal)
                    : Quaternion.identity;
                Instantiate(hitFxPrefab, hitInfo.point, rotation);
            }

            Destroy(gameObject);
        }

        private void ConfigureCollider()
        {
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider == null)
            {
                return;
            }

            sphereCollider.isTrigger = true;
            sphereCollider.radius = radius;
        }
    }
}
