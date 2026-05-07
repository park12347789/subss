using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab = null;
    [SerializeField] private Transform[] spawnPoints = null;
    [SerializeField] private Transform enemyPoolRoot = null;
    [SerializeField] private Transform lookAtCenter = null;

    [Header("Pool")]
    [SerializeField] private int initialPoolSize = 4;
    [SerializeField] private int maxPoolSize = 8;

    [Header("Spawn")]
    [SerializeField] private float spawnIntervalSeconds = 1f;
    [SerializeField] private float enemyMoveSpeedUnitsPerSecond = 5f;
    [SerializeField] private float enemyLifeTimeSeconds = 10f;

    private readonly List<AliveEnemy> aliveEnemies = new List<AliveEnemy>(32);
    private GameObjectPool enemyPool;
    private float nextSpawnTimeSeconds;

    private void Awake()
    {
        enemyPool = new GameObjectPool(enemyPrefab, enemyPoolRoot, initialPoolSize, maxPoolSize);
    }

    private void Start()
    {
        for (int spawnIndex = 0; spawnIndex < initialPoolSize; spawnIndex++)
        {
            if (!TrySpawnOneEnemy())
            {
                break;
            }
        }

        nextSpawnTimeSeconds = Time.time + spawnIntervalSeconds;
    }

    private void Update()
    {
        ReleaseExpiredEnemies();

        if (Time.time < nextSpawnTimeSeconds || aliveEnemies.Count >= maxPoolSize)
        {
            return;
        }

        if (TrySpawnOneEnemy())
        {
            nextSpawnTimeSeconds = Time.time + spawnIntervalSeconds;
        }
    }

    private bool TrySpawnOneEnemy()
    {
        if (enemyPool == null || enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            return false;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        if (spawnPoint == null)
        {
            return false;
        }

        GameObject enemy = enemyPool.Get(spawnPoint.position, GetSpawnRotation(spawnPoint));
        if (enemy == null)
        {
            return false;
        }

        if (enemy.TryGetComponent(out EnemyLinearMover mover))
        {
            mover.Initialize(enemyMoveSpeedUnitsPerSecond, enemyLifeTimeSeconds, ReleaseEnemy);
        }

        aliveEnemies.Add(new AliveEnemy(enemy, Time.time + enemyLifeTimeSeconds));
        return true;
    }

    private Quaternion GetSpawnRotation(Transform spawnPoint)
    {
        if (lookAtCenter == null)
        {
            return spawnPoint.rotation;
        }

        Vector3 toCenter = lookAtCenter.position - spawnPoint.position;
        if (toCenter.sqrMagnitude <= 1e-8f)
        {
            return spawnPoint.rotation;
        }

        return Quaternion.LookRotation(toCenter.normalized, Vector3.up);
    }

    private void ReleaseEnemy(EnemyLinearMover mover)
    {
        if (mover == null)
        {
            return;
        }

        ReleaseEnemy(mover.gameObject);
    }

    private void ReleaseEnemy(GameObject enemy)
    {
        if (enemy == null)
        {
            return;
        }

        RemoveTrackedEnemy(enemy);
        if (enemyPool != null && enemyPool.Release(enemy))
        {
            return;
        }

        enemy.SetActive(false);
    }

    private void ReleaseExpiredEnemies()
    {
        for (int index = aliveEnemies.Count - 1; index >= 0; index--)
        {
            GameObject enemy = aliveEnemies[index].Instance;
            if (enemy == null)
            {
                aliveEnemies.RemoveAt(index);
                continue;
            }

            if (!enemy.activeSelf || Time.time >= aliveEnemies[index].ReleaseTimeSeconds)
            {
                if (enemyPool != null)
                {
                    enemyPool.Release(enemy);
                }

                aliveEnemies.RemoveAt(index);
            }
        }
    }

    private void RemoveTrackedEnemy(GameObject enemy)
    {
        for (int index = aliveEnemies.Count - 1; index >= 0; index--)
        {
            if (aliveEnemies[index].Instance == enemy)
            {
                aliveEnemies.RemoveAt(index);
                return;
            }
        }
    }

    private void OnValidate()
    {
        initialPoolSize = Mathf.Max(0, initialPoolSize);
        maxPoolSize = Mathf.Max(1, maxPoolSize);

        if (maxPoolSize < initialPoolSize)
        {
            maxPoolSize = initialPoolSize;
        }

        spawnIntervalSeconds = Mathf.Max(0.1f, spawnIntervalSeconds);
        enemyMoveSpeedUnitsPerSecond = Mathf.Max(0.1f, enemyMoveSpeedUnitsPerSecond);
        enemyLifeTimeSeconds = Mathf.Max(0.1f, enemyLifeTimeSeconds);
    }

    private readonly struct AliveEnemy
    {
        public AliveEnemy(GameObject instance, float releaseTimeSeconds)
        {
            Instance = instance;
            ReleaseTimeSeconds = releaseTimeSeconds;
        }

        public GameObject Instance { get; }

        public float ReleaseTimeSeconds { get; }
    }
}
