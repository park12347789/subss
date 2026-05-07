using System.Collections.Generic;
using UnityEngine;

public sealed class GameObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform poolRoot;
    private readonly int maxSize;
    private readonly Queue<GameObject> availableObjects = new Queue<GameObject>();
    private readonly HashSet<GameObject> activeObjects = new HashSet<GameObject>();

    private int totalCreated;

    public GameObjectPool(GameObject prefab, Transform poolRoot, int initialSize, int maxSize)
    {
        this.prefab = prefab;
        this.poolRoot = poolRoot;
        this.maxSize = Mathf.Max(1, maxSize);

        Prewarm(Mathf.Clamp(initialSize, 0, this.maxSize));
    }

    public int ActiveCount => activeObjects.Count;

    public int AvailableCount => availableObjects.Count;

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject instance = availableObjects.Count > 0 ? availableObjects.Dequeue() : CreateInstance();
        if (instance == null)
        {
            return null;
        }

        Transform instanceTransform = instance.transform;
        instanceTransform.SetParent(poolRoot, false);
        instanceTransform.SetPositionAndRotation(position, rotation);

        activeObjects.Add(instance);
        instance.SetActive(true);
        return instance;
    }

    public bool Release(GameObject instance)
    {
        if (instance == null || !activeObjects.Remove(instance))
        {
            return false;
        }

        instance.SetActive(false);

        Transform instanceTransform = instance.transform;
        instanceTransform.SetParent(poolRoot, false);
        instanceTransform.localPosition = Vector3.zero;
        instanceTransform.localRotation = Quaternion.identity;

        availableObjects.Enqueue(instance);
        return true;
    }

    private void Prewarm(int count)
    {
        for (int index = 0; index < count; index++)
        {
            GameObject instance = CreateInstance();
            if (instance == null)
            {
                return;
            }

            availableObjects.Enqueue(instance);
        }
    }

    private GameObject CreateInstance()
    {
        if (prefab == null || totalCreated >= maxSize)
        {
            return null;
        }

        totalCreated++;

        GameObject instance = Object.Instantiate(prefab, poolRoot);
        instance.name = $"{prefab.name}_Pooled_{totalCreated:00}";
        instance.SetActive(false);
        return instance;
    }
}
