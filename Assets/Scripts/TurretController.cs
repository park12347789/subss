using UnityEngine;

public sealed class TurretController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target = null;
    [SerializeField] private Transform yawPivot = null;
    [SerializeField] private Transform pitchPivot = null;
    [SerializeField] private Transform muzzlePoint = null;
    [SerializeField] private GameObject projectilePrefab = null;

    [Header("Rotation")]
    [SerializeField] private float yawSpeed = 120f;
    [SerializeField] private float pitchSpeed = 90f;
    [SerializeField] private float minPitch = -45f;
    [SerializeField] private float maxPitch = 20f;

    [Header("Fire")]
    [SerializeField] private float fireAngleThreshold = 5f;
    [SerializeField] private float fireInterval = 0.5f;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifeTime = 3f;

    private float fireTimer;

    private void Awake()
    {
        fireTimer = fireInterval;
    }

    private void Update()
    {
        if (!target || !yawPivot || !pitchPivot || !muzzlePoint || !projectilePrefab)
        {
            return;
        }

        RotateYaw();
        RotatePitch();
        TryFire();
    }

    private void RotateYaw()
    {
        Vector3 targetDirection = target.position - yawPivot.position;
        targetDirection.y = 0f;

        if (targetDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized, Vector3.up);
        yawPivot.rotation = Quaternion.RotateTowards(
            yawPivot.rotation,
            targetRotation,
            yawSpeed * Time.deltaTime);
    }

    private void RotatePitch()
    {
        Vector3 targetDirection = target.position - pitchPivot.position;
        Vector3 localDirection = yawPivot.InverseTransformDirection(targetDirection.normalized);

        float targetPitch = -Mathf.Atan2(localDirection.y, localDirection.z) * Mathf.Rad2Deg;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        Quaternion targetRotation = Quaternion.Euler(targetPitch, 0f, 0f);
        pitchPivot.localRotation = Quaternion.RotateTowards(
            pitchPivot.localRotation,
            targetRotation,
            pitchSpeed * Time.deltaTime);
    }

    private void TryFire()
    {
        fireTimer += Time.deltaTime;

        Vector3 targetDirection = (target.position - muzzlePoint.position).normalized;
        float aimAngle = Vector3.Angle(muzzlePoint.forward, targetDirection);

        if (aimAngle > fireAngleThreshold || fireTimer < fireInterval)
        {
            return;
        }

        FireProjectile();
        fireTimer = 0f;
    }

    private void FireProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        projectile.SetActive(true);

        if (projectile.TryGetComponent(out ProjectileMover mover))
        {
            mover.Initialize(projectileSpeed, projectileLifeTime);
        }
    }
}
