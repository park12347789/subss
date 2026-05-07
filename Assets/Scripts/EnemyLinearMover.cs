using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyLinearMover : MonoBehaviour
{
    [SerializeField] private float moveSpeedUnitsPerSecond = 5f;
    [SerializeField] private float lifeTimeSeconds = 10f;

    private float remainingLifeSeconds;
    private Action<EnemyLinearMover> expiredHandler;

    public void Initialize(float speedUnitsPerSecond, float lifeTime, Action<EnemyLinearMover> onExpired = null)
    {
        moveSpeedUnitsPerSecond = speedUnitsPerSecond;
        lifeTimeSeconds = lifeTime;
        remainingLifeSeconds = lifeTimeSeconds;
        expiredHandler = onExpired;
    }

    private void OnEnable()
    {
        remainingLifeSeconds = lifeTimeSeconds;
    }

    private void OnDisable()
    {
        expiredHandler = null;
    }

    private void Update()
    {
        transform.position += transform.forward * (moveSpeedUnitsPerSecond * Time.deltaTime);
        remainingLifeSeconds -= Time.deltaTime;

        if (remainingLifeSeconds <= 0f)
        {
            Expire();
        }
    }

    private void Expire()
    {
        Action<EnemyLinearMover> handler = expiredHandler;
        if (handler != null)
        {
            handler(this);
            return;
        }

        gameObject.SetActive(false);
    }
}
