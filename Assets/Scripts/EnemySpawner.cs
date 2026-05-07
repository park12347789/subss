using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public sealed class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab = null;
    [SerializeField] private Transform[] spawnPoints = null;
    [SerializeField] private Transform enemyPoolRoot = null;
    [SerializeField] private Transform lookAtCenter = null;

    [Header("Pool")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 10;
    [SerializeField] private int maxAliveCount = 10;

    [Header("Spawn")]
    [SerializeField] private float enemyMoveSpeedUnitsPerSecond = 5f;
    [SerializeField] private float enemyLifeTimeSeconds = 10f;

    private readonly List<AliveEnemy> aliveEnemies = new List<AliveEnemy>(32);
    private GameObjectPool enemyPool;

    private void Awake()
    {
        enemyPool = new GameObjectPool(enemyPrefab, enemyPoolRoot, initialPoolSize, maxPoolSize);
    }

    private void Update()
    {
        ReleaseExpiredEnemies();

        if (WasSpawnRequested())
        {
            TrySpawnOneEnemy();
        }
    }

    private bool TrySpawnOneEnemy()
    {
        if (!TryGetSpawnPoint(out Transform spawnPoint))
        {
            return false;
        }

        if (aliveEnemies.Count >= maxAliveCount)
        {
            ReleaseOldestEnemy();
        }

        if (aliveEnemies.Count >= maxAliveCount)
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

    private bool TryGetSpawnPoint(out Transform spawnPoint)
    {
        spawnPoint = null;

        if (enemyPool == null || enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            return false;
        }

        int startIndex = Random.Range(0, spawnPoints.Length);
        for (int offset = 0; offset < spawnPoints.Length; offset++)
        {
            Transform candidate = spawnPoints[(startIndex + offset) % spawnPoints.Length];
            if (candidate != null)
            {
                spawnPoint = candidate;
                return true;
            }
        }

        return false;
    }

    private static bool WasSpawnRequested()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space))
        {
            return true;
        }
#endif
        return false;
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

    private bool ReleaseOldestEnemy()
    {
        while (aliveEnemies.Count > 0)
        {
            GameObject oldestEnemy = aliveEnemies[0].Instance;
            aliveEnemies.RemoveAt(0);

            if (oldestEnemy == null)
            {
                continue;
            }

            if (enemyPool != null && enemyPool.Release(oldestEnemy))
            {
                return true;
            }

            oldestEnemy.SetActive(false);
            return true;
        }

        return false;
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
        maxAliveCount = Mathf.Clamp(maxAliveCount, 1, 10);

        if (maxPoolSize < maxAliveCount)
        {
            maxPoolSize = maxAliveCount;
        }

        if (initialPoolSize < maxAliveCount)
        {
            initialPoolSize = maxAliveCount;
        }

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
