using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microgame 
{

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class KinematicMotion : MonoBehaviour
{
    public bool useGravity = true;
    public float gravityMultiplier = 1f;
    public float margin = 0.01f;

    public float groundCheckDistance = 0.02f;
    public float maxGroundAngleDegrees = 45;

    public float minSpeed = 0.01f;

    Rigidbody2D rb;

    public Vector2 velocity {get; private set;}
    public bool isGrounded  {get; private set;}

    public Vector2 groundNormal {get; private set;}
    public Vector2 groundRight {
        get {
            return new Vector2(groundNormal.y, groundNormal.x);
        }
    }

    public Vector2 groundVelocity {
        get {
            if (velocity.magnitude > minSpeed) {
                return Mathf.Abs(Vector2.Dot(velocity, groundRight)) * velocity.normalized;
            }
            else {
                return Vector2.zero;
            }
        }
    }

    public Vector2 lastGroundVelocity { get; private set; }

    public Vector2 gravity
    {
        get
        {
            return Physics2D.gravity * gravityMultiplier;
        }
    }

    public bool isFalling {
        get {
            return Vector2.Dot(gravity, velocity) > 0f;
        }
    }
    
    RaycastHit2D[] hits = new RaycastHit2D[8];
    ContactFilter2D contactFilter = new ContactFilter2D();

    public void AddImpulse(Vector2 velocityChange)
    {
        velocity += velocityChange;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        isGrounded = false;
        groundNormal = gravity.normalized * -1f;
        lastGroundVelocity = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateGroundedState()
    {
        bool oldState = isGrounded;
        int hitCount = rb.Cast(gravity.normalized, contactFilter, hits, groundCheckDistance);
        
        isGrounded = false;
        groundNormal = gravity.normalized * -1f;
        
        for (int i = 0; i <hitCount; i++) {
            if (Vector2.Angle(gravity * -1, hits[i].normal) < maxGroundAngleDegrees) {
                isGrounded = true;
                groundNormal = hits[i].normal;
            }
        }
    }

    Vector2 ApplyMotion(Vector2 motion)
    {
        // 衝突判定
        float distance = motion.magnitude;
        Vector2 direction = motion.normalized;

        int hitCount = rb.Cast(direction, contactFilter, hits, distance + margin);
        for (int i = 0; i < hitCount; i++)
        {
            distance = Mathf.Min(distance, hits[i].distance - margin);
        }

        // オブジェクトを移動させる
        Vector2 oldPosition = rb.position;
        rb.position += direction * distance;
        motion = rb.position - oldPosition;
        return motion;
    }

    void FixedUpdate()
    {
        // 重力
        if (useGravity) {
            velocity += gravity * Time.fixedDeltaTime;
        }

        Vector2 motion = velocity * Time.fixedDeltaTime;
        Vector2 horizontalMotion = Vector2.Scale(motion, Vector2.right);
        Vector2 verticalMotion = Vector2.Scale(motion, Vector2.up);

        motion = ApplyMotion(horizontalMotion);
        motion += ApplyMotion(verticalMotion);

        velocity = motion / Time.fixedDeltaTime;

        // 地面に立っているか？
        UpdateGroundedState();

        if (isGrounded) {
            lastGroundVelocity = groundVelocity;
        }
    }
}

} // namespaceはここまで