using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteAlways]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class SlideChecker : MonoBehaviour
{
    public Transform target;
    public float margin = 0.1f;
    public float maxGroundAngleDegrees = 45;
    public int maxIterations = 3;

    Rigidbody2D rb;
    BoxCollider2D coll;


    Vector2 slidePosition = Vector2.zero;
    Vector3 delta = Vector2.zero;

    RaycastHit2D[] hits = new RaycastHit2D[8];
    int hitCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
    }

    //bool Cast(Vector2 origin, Vector2 delta, out RaycastHit2D hit)
    bool Cast(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit)
    {
        distance += margin;
        bool collided = false;

        hit = new RaycastHit2D();
        int hitCount = Physics2D.BoxCastNonAlloc(origin, coll.bounds.size, 0f, direction, hits, distance);
        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; i++)
            {
                bool hitSelf = hits[i].rigidbody == rb;
                bool wrongDirection = Vector2.Dot(direction, hits[i].point - origin) < 0;
                if (hits[i].distance < distance && !hitSelf && !wrongDirection)
                {
                    distance = hits[i].distance;
                    hit = hits[i];
                    collided = true;
                }
            }
        }

        return collided;
    }

    Vector2 Slide(Vector2 origin, Vector2 direction, float distance, int iterations = 0)
    {
        RaycastHit2D hit;
        if (Cast(origin, direction, distance, out hit))
        {
            float contactDistance = hit.distance - margin;
            Vector2 contactPosition = origin + direction * contactDistance;

            if (iterations < maxIterations)
            {
                
                if (IsGroundNormal(hit.normal) || CanSlideOnCeiling(hit.normal, direction))
                {
                    float distanceRemaining = distance - contactDistance;

                    Vector2 groundRight = new Vector2(hit.normal.y, -hit.normal.x);
                    Vector2 slideDelta = new Vector2(direction.x * distanceRemaining, 0);
                    Vector2 projection = groundRight * Vector2.Dot(groundRight, slideDelta);
                    return Slide(contactPosition, projection.normalized, projection.magnitude, iterations + 1);
                }
            }

            return contactPosition;
        }

        return origin + direction * distance;
    }

    bool IsGroundNormal(Vector2 normal)
    {
        return Vector2.Angle(Vector2.up, normal) < maxGroundAngleDegrees;
    }

    bool CanSlideOnCeiling(Vector2 normal, Vector2 direction)
    {
        return normal.y < 0f && Vector2.Dot(normal, direction) < 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            Vector2 delta = target.position - transform.position;
            Vector2 horizontalDelta = new Vector2(delta.x, 0);
            Vector2 verticalDelta = new Vector2(0, delta.y);
            //slidePosition = Slide(transform.position, verticalDelta.normalized, verticalDelta.magnitude);
            //slidePosition = Slide(slidePosition, horizontalDelta.normalized, horizontalDelta.magnitude);

            slidePosition = Slide(transform.position, delta.normalized, delta.magnitude);
        }
        
    }

    private void OnDrawGizmos()
    {
        if (enabled && target)
        {
            Vector2 d = target.position - transform.position;
            Vector2 direction = d.normalized;

            Vector3 colliderSize = coll.bounds.size;
            Vector3 marginColliderSize = colliderSize + Vector3.one * margin * 2f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(target.position, coll.bounds.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(slidePosition, colliderSize);
            Gizmos.DrawWireCube(slidePosition, marginColliderSize);

            //Gizmos.color = Color.yellow;
            //for (int i = 0; i < hitCount; i++)
            //{
            //    if (hits[i].rigidbody != rb)
            //    {
            //        Vector3 p = direction * (hits[i].distance - margin);
            //        Gizmos.DrawWireCube(transform.position + p, colliderSize);
            //        Gizmos.DrawWireCube(transform.position + p, marginColliderSize);
            //    }

            //}

            if (hitCount > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(slidePosition, colliderSize);
                Gizmos.DrawWireCube(slidePosition, marginColliderSize);
            }
        }
    }
}
