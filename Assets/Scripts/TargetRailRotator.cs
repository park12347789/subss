using UnityEngine;

public sealed class TargetRailRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 35f;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
