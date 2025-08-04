using System.Collections;
using UnityEngine;

public enum PooledObjectType
{
    Obstacle,
    Orb
}

public class PooledObject : MonoBehaviour
{
    [SerializeField] PooledObjectType _ObjectType;
    [SerializeField] int _ObjectCollisionPoints = 5;

    // assigned at spawn time
    [HideInInspector] public int pSpawnedObjectID;
    [HideInInspector] public ObjectPool pPool;   
    [HideInInspector] public float pSpeed;

    void Update()
    {
        transform.Translate(Vector3.back * pSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerWorld"))
        {
            other.GetComponent<PlayerController>().ActivateCollisionGlow(_ObjectType, 0.15f);
            switch (_ObjectType)
            {
                case PooledObjectType.Orb:
                    // Record the collect‐orb event
                    SyncBuffer.RecordEvent(new SyncBuffer.PlayerEvent
                    {
                        time = Time.time,
                        type = SyncBuffer.ActionType.CollectOrb,
                        targetID = pSpawnedObjectID
                    });
                    GameSceneManager.Instance.UpdateScore(_ObjectCollisionPoints);
                    Release();
                    break;

                case PooledObjectType.Obstacle:
                    // Record the obstacle‐hit event
                    SyncBuffer.RecordEvent(new SyncBuffer.PlayerEvent
                    {
                        time = Time.time,
                        type = SyncBuffer.ActionType.HitObstacle,
                        targetID = pSpawnedObjectID
                    });
                    StartCoroutine(PlayHitEffectThenGameOver());
                    break;
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Default")) //if hits boundary
            Release();
    }

    IEnumerator PlayHitEffectThenGameOver()
    {
        var effects = GetComponentsInChildren<ParticleSystem>();
        foreach(var effect in effects)
            effect.Play();
        yield return new WaitForSeconds(0.2f);
        Release();
        GameSceneManager.Instance.GameOver();
    }


    /// <summary>
    /// Return this object to its pool.
    /// </summary>
    public void Release()
    {
        // You could add common reset logic here (e.g. reset shaders/particles).
        pPool.Release(gameObject);
    }
}