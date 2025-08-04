using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] GameObject _ObjectPrefab;
    [SerializeField] int _InitialPoolSize = 10;

    private Queue<GameObject> mPool;

    void Awake()
    {
        mPool = new Queue<GameObject>();
        for (int i = 0; i < _InitialPoolSize; i++)
        {
            var go = Instantiate(_ObjectPrefab);
            go.SetActive(false);
            mPool.Enqueue(go);
        }
    }

    /// <summary>
    /// Returns an instance (activates it).  
    /// If none are available, instantiates a new one.
    /// </summary>
    public GameObject Get()
    {
        GameObject go = mPool.Count > 0
            ? mPool.Dequeue()
            : Instantiate(_ObjectPrefab);
        go.SetActive(true);
        return go;
    }

    /// <summary>
    /// Deactivates and returns this object to the pool.
    /// </summary>
    public void Release(GameObject go)
    {
        go.SetActive(false);
        mPool.Enqueue(go);
    }
}