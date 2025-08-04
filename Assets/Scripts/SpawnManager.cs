using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Pools")]
    [SerializeField] ObjectPool _ObstaclePool;
    [SerializeField] ObjectPool _OrbPool;
    [SerializeField] float _ObstacleSpawnInterval = 1.2f;
    [SerializeField] float _OrbSpawnInterval = 1.7f;
    [SerializeField] float _PooledObjSpawnRangeX = 1.5f;
    [SerializeField] float _PooledObjSpawnPosZ = 20f;
    [SerializeField] float _PooledObjStartSpeed = 1f;
    

    [Header("Separation")]
    [SerializeField] float _MinSeparation = 0.5f;

    float mLastObstacleX = float.NaN, mLastOrbX = float.NaN;
    float mObstacleTimer, mOrbTimer;

    void Update()
    {
        mObstacleTimer += Time.deltaTime;
        mOrbTimer += Time.deltaTime;
        if (mObstacleTimer >= _ObstacleSpawnInterval)
        {
            mObstacleTimer = 0;
            SpawnPair(PooledObjectType.Obstacle);
        }

        if (mOrbTimer >= _OrbSpawnInterval)
        {
            mOrbTimer = 0;
            SpawnPair(PooledObjectType.Orb);
        }
    }

    void SpawnPair(PooledObjectType objectType)
    {
        float x;
        if (objectType == PooledObjectType.Obstacle)
        {
            x = GetSafeX(mLastOrbX);
            mLastObstacleX = x;
        }
        else // Orb
        {
            x = GetSafeX(mLastObstacleX);
            mLastOrbX = x;
        }

        int id = IdGenerator.Next();

        // pick the right pool
        ObjectPool pool = objectType == PooledObjectType.Obstacle
            ? _ObstaclePool
            : _OrbPool;

        // PLAYER WORLD
        var pObj = pool.Get();
        InitializePooledObject(pObj, id, "PlayerWorld", pool, _PooledObjStartSpeed);
        pObj.transform.position = new Vector3(x, 0f, _PooledObjSpawnPosZ);

        // GHOST WORLD
        var gObj = pool.Get();
        InitializePooledObject(gObj, id, "GhostWorld", pool, _PooledObjStartSpeed);
        gObj.transform.position = new Vector3(x, 0f, _PooledObjSpawnPosZ);
    }

    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    void InitializePooledObject(GameObject obj, int id, string layerName, ObjectPool pool, float speed)
    {
        int layer = LayerMask.NameToLayer(layerName);
        SetLayerRecursively(obj, layer);

        var info = obj.GetComponent<PooledObject>();
        info.pSpawnedObjectID = id;
        info.pPool = pool;
        var currentSpeed = GameSceneManager.Instance.GetObjectSpeed();
        if (currentSpeed < speed)
        {
            info.pSpeed = speed;
            GameSceneManager.Instance.SetObjectSpeed(speed);
        }
        else
            info.pSpeed = currentSpeed;
    }

    float GetSafeX(float forbiddenX)
    {
        const int MAX_ATTEMPTS = 10;
        for (int i = 0; i < MAX_ATTEMPTS; i++)
        {
            float candidate = Random.Range(-1 * _PooledObjSpawnRangeX, _PooledObjSpawnRangeX);
            if (float.IsNaN(forbiddenX) || Mathf.Abs(candidate - forbiddenX) >= _MinSeparation)
                return candidate;
        }
        // fallback if we failed to find a good one:
        return Mathf.Clamp(forbiddenX + _MinSeparation, -1 * _PooledObjSpawnRangeX, _PooledObjSpawnRangeX);
    }
}