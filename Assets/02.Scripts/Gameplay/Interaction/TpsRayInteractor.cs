using SystemicOverload.Phase1;
using UnityEngine;

namespace SystemicOverload.Interaction
{
    /// <summary>
    /// 카메라 중앙 Raycast로 상호작용 대상을 탐색하고 입력 시 실행합니다.
    /// </summary>
    [RequireComponent(typeof(InputProvider))]
    public sealed class TpsRayInteractor : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float interactDistance = 4.0f;
        [SerializeField] private LayerMask interactMask = ~0;
        [SerializeField] private bool drawDebugRay = true;

        private InputProvider inputProvider;
        private IInteractable currentTarget;
        private string lastPrompt;

        public string CurrentPrompt => currentTarget?.GetPrompt() ?? string.Empty;

        private void Awake()
        {
            inputProvider = GetComponent<InputProvider>();
            if (aimCamera == null)
            {
                aimCamera = Camera.main;
            }
        }

        private void OnValidate()
        {
            interactDistance = Mathf.Max(0.2f, interactDistance);
        }

        private void Update()
        {
            ScanInteractable();
            if (currentTarget == null)
            {
                return;
            }

            if (inputProvider.WasInteractPressedThisFrame)
            {
                currentTarget.Interact(gameObject);
            }
        }

        private void ScanInteractable()
        {
            currentTarget = null;

            if (aimCamera == null)
            {
                aimCamera = Camera.main;
                if (aimCamera == null)
                {
                    return;
                }
            }

            Ray centerRay = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            if (Physics.Raycast(centerRay, out RaycastHit raycastHit, interactDistance, interactMask, QueryTriggerInteraction.Ignore))
            {
                if (drawDebugRay)
                {
                    Debug.DrawRay(centerRay.origin, centerRay.direction * raycastHit.distance, Color.yellow);
                }

                currentTarget = raycastHit.collider.GetComponentInParent<IInteractable>();
                if (currentTarget == null)
                {
                    lastPrompt = string.Empty;
                    return;
                }

                string prompt = currentTarget.GetPrompt();
                if (!string.IsNullOrEmpty(prompt) && prompt != lastPrompt)
                {
                    Debug.Log(prompt);
                }

                lastPrompt = prompt;
                return;
            }

            if (drawDebugRay)
            {
                Debug.DrawRay(centerRay.origin, centerRay.direction * interactDistance, Color.white);
            }

            lastPrompt = string.Empty;
        }
    }
}
