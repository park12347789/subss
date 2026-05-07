using System;
using UnityEngine;

public sealed class ProjectileMover : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 3f;

    private float age;
    private Action<ProjectileMover> expiredHandler;

    public void Initialize(float projectileSpeed, float projectileLifeTime)
    {
        Initialize(projectileSpeed, projectileLifeTime, null);
    }

    public void Initialize(float projectileSpeed, float projectileLifeTime, Action<ProjectileMover> onExpired)
    {
        speed = projectileSpeed;
        lifeTime = projectileLifeTime;
        age = 0f;
        expiredHandler = onExpired;
    }

    private void OnEnable()
    {
        age = 0f;
    }

    private void OnDisable()
    {
        expiredHandler = null;
    }

    private void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
        age += Time.deltaTime;

        if (age >= lifeTime)
        {
            Expire();
        }
    }

    private void Expire()
    {
        Action<ProjectileMover> handler = expiredHandler;
        if (handler != null)
        {
            handler(this);
            return;
        }

        gameObject.SetActive(false);
    }
}
