using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float _HorizontalMoveSpeed = 5f;
    [SerializeField] float _JumpForce = 7f;
    [SerializeField] float _HorizontalClampRange = 1.5f;

    [Header("Ground Check")]
    [SerializeField] float _FloorDetectionDistance = 0.1f;
    [SerializeField] LayerMask _FloorMask; 

    [Header("Collision Glow")]
    [SerializeField] GameObject _OrbGlowObj = null;
    [SerializeField] GameObject _ObstacleGlowObj = null; 

    Rigidbody mRigidBody;
    bool mLeftPressed, mRightPressed, mJumpPressed;
    float mHorizontalMovementDirection;

    void Awake()
    {
        mRigidBody = GetComponent<Rigidbody>();
        mRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        // horizontal movement input
        if (mLeftPressed) mHorizontalMovementDirection = -1f;
        else if (mRightPressed) mHorizontalMovementDirection = +1f;
        else mHorizontalMovementDirection = 0f;
    }

    void FixedUpdate()
    {
        //Horizontal movement by velocity
        Vector3 vel = mRigidBody.velocity;
        vel.x = mHorizontalMovementDirection * _HorizontalMoveSpeed;
        mRigidBody.velocity = vel;

        //Clamp X position
        Vector3 pos = mRigidBody.position;
        pos.x = Mathf.Clamp(pos.x, -1* _HorizontalClampRange, _HorizontalClampRange);
        mRigidBody.MovePosition(pos);

        //Ground check via Raycast down
        bool grounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            0.5f + _FloorDetectionDistance,
            _FloorMask
        );

        //Jump
        if (mJumpPressed && grounded)
        {
            mRigidBody.AddForce(Vector3.up * _JumpForce, ForceMode.Impulse);
            SyncBuffer.RecordEvent(new SyncBuffer.PlayerEvent
            {
                time = Time.time,
                type = SyncBuffer.ActionType.Jump,
                targetID = 0
            });
        }
        mJumpPressed = false;

        //Record position snapshot
        SyncBuffer.RecordSnapshot(Time.time, mRigidBody.position);
    }

    //References for Editor
    public void OnLeftButtonDown() => mLeftPressed = true;
    public void OnLeftButtonUp() => mLeftPressed = false;
    public void OnRightButtonDown() => mRightPressed = true;
    public void OnRightButtonUp() => mRightPressed = false;
    public void OnJumpButton() => mJumpPressed = true;

    public void ActivateCollisionGlow(PooledObjectType objType, float duration)
    {
        //Glow to detect collision with respective objects
        switch(objType)
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