using UnityEngine;

namespace SystemicOverload.Phase1
{
    /// <summary>
    /// Keeps the character facing the camera yaw for shoulder-aim controls.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public sealed class CameraAimFacingComponent : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float rotationSharpness = 32.0f;

        private void OnValidate()
        {
            rotationSharpness = Mathf.Max(0.0f, rotationSharpness);
        }

        private void LateUpdate()
        {
            Camera targetCamera = ResolveAimCamera();
            if (targetCamera == null)
            {
                return;
            }

            Vector3 cameraForward = Vector3.ProjectOnPlane(targetCamera.transform.forward, Vector3.up);
            if (cameraForward.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(cameraForward.normalized, Vector3.up);
            float blendFactor = 1.0f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, blendFactor);
        }

        private Camera ResolveAimCamera()
        {
            if (aimCamera != null)
            {
                return aimCamera;
            }

            aimCamera = Camera.main;
            return aimCamera;
        }
    }
}
