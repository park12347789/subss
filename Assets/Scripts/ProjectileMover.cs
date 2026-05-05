using UnityEngine;

public sealed class ProjectileMover : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 3f;

    private float age;

    public void Initialize(float projectileSpeed, float projectileLifeTime)
    {
        speed = projectileSpeed;
        lifeTime = projectileLifeTime;
        age = 0f;
    }

    private void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
        age += Time.deltaTime;

        if (age >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
