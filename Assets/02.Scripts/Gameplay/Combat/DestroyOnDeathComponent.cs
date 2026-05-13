using UnityEngine;

namespace SystemicOverload.Combat
{
    [RequireComponent(typeof(HealthComponent))]
    public sealed class DestroyOnDeathComponent : MonoBehaviour
    {
        [SerializeField] private GameObject targetRoot;
        [SerializeField] private float destroyDelay;

        private HealthComponent healthComponent;

        private void Awake()
        {
            healthComponent = GetComponent<HealthComponent>();
            if (targetRoot == null)
            {
                targetRoot = gameObject;
            }
        }

        private void OnEnable()
        {
            if (healthComponent == null)
            {
                healthComponent = GetComponent<HealthComponent>();
            }

            healthComponent.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (healthComponent != null)
            {
                healthComponent.Died -= HandleDied;
            }
        }

        private void OnValidate()
        {
            destroyDelay = Mathf.Max(0.0f, destroyDelay);
        }

        private void HandleDied()
        {
            GameObject target = targetRoot != null ? targetRoot : gameObject;
            Destroy(target, destroyDelay);
        }
    }
}
