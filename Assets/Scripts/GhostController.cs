using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GhostController : MonoBehaviour
{
    [Header("Sync Settings")]
    [SerializeField] float _LagSeconds = 0.2f;
    [SerializeField] float _Smoothing = 10f;

    [Header("Jump Settings")]
    [SerializeField] float _JumpForce = 7f;

    [Header("Collision Glow")]
    [SerializeField] GameObject _OrbGlowObj = null;
    [SerializeField] GameObject _ObstacleGlowObj = null;

    Rigidbody mRigidBody;

    void Awake()
    {
        mRigidBody = GetComponent<Rigidbody>();
        mRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        //Horizontal replay
        float targetTime = Time.time - _LagSeconds;
        var snapOpt = SyncBuffer.GetInterpolatedSnapshot(targetTime);
        if (snapOpt.HasValue)
        {
            Vector3 desired = snapOpt.Value.position;
            Vector3 current = mRigidBody.position;
            float step = _Smoothing * Time.fixedDeltaTime;
            // only move X 
            float newX = Mathf.MoveTowards(current.x, desired.x, step);
            mRigidBody.MovePosition(new Vector3(newX, current.y, current.z));
        }

        //Replay events
        while (SyncBuffer.PeekEvent(out var ev) && ev.time + _LagSeconds <= Time.time)
        {
            SyncBuffer.DequeueEvent();
            switch (ev.type)
            {
                case SyncBuffer.ActionType.Jump:
                    mRigidBody.AddForce(Vector3.up * _JumpForce, ForceMode.Impulse);
                    break;
                case SyncBuffer.ActionType.CollectOrb:
                    var orb = FindPooledObject(ev.targetID);
                    if (orb != null) orb.Release();
                    ActivateCollisionGlow(PooledObjectType.Orb, 0.15f);
                    break;
                case SyncBuffer.ActionType.HitObstacle:
                    var obs = FindPooledObject(ev.targetID);
                    if (obs != null) obs.Release();
                    ActivateCollisionGlow(PooledObjectType.Obstacle, 0.15f);
                    break;
            }
        }
    }

    PooledObject FindPooledObject(int id)
    {
        foreach (var po in FindObjectsOfType<PooledObject>())
            if (po.pSpawnedObjectID == id && po.gameObject.layer == LayerMask.NameToLayer("GhostWorld"))
                return po;
        return null;
    }

    public void ActivateCollisionGlow(PooledObjectType objType, float duration)
    {
        //Glow to detect collision with respective objects
        switch (objType)
        {
            case PooledObjectType.Obstacle:
                _ObstacleGlowObj.SetActive(true);
                Invoke(nameof(DeactivateCollisionGlow), duration);
                break;
            case PooledObjectType.Orb:
                _OrbGlowObj.SetActive(true);
                Invoke(nameof(DeactivateCollisionGlow), duration);
                break;
        }
    }

    private void DeactivateCollisionGlow()
    {
        if (_ObstacleGlowObj.activeInHierarchy)
            _ObstacleGlowObj.SetActive(false);
        if (_OrbGlowObj.activeInHierarchy)
            _OrbGlowObj.SetActive(false);
    }
}