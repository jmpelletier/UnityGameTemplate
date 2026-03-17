using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microgame 
{

// 「Rigidbody2D」と「Collider2D」を必要にする。
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class KinematicMotion : MonoBehaviour
{
    [Header("重力")] // インスペクターに見出しを表示する
    public bool useGravity = true; // 重力の影響を受けるか？
    public float gravityMultiplier = 1f; // これで重力の調整ができる

    [Header("衝突判定")]
    public float margin = 0.01f; // 衝突のマージン
    public float groundCheckDistance = 0.02f; // 地面までの最大の距離
    public float maxGroundAngleDegrees = 45; // 地面の最大の角度

    public float minSpeed = 0.01f;
    public float maxSpeed = 10f;

    Rigidbody2D rb;

    // オブジェクトの速度。C#のアクセサ機能を使って、外部からは読み取り専用にする。
    [field: SerializeField]
    public Vector2 velocity {get; private set;}

    // オブジェクトが地面に立っているかどうか。
    public bool isGrounded  {get; private set;}

    public float groundDistance {get; private set;}

    // 地面の法線（地面に立っていない時は真上を指す）
    public Vector2 groundNormal {get; private set;}

    // 地面に対しての「右」方向
    public Vector2 groundRight {
        get {
            return new Vector2(groundNormal.y, -groundNormal.x);
        }
    }
    

    // 衝突判定に必要な変数。
    // 効率をよくするために、メソッドの外で宣言しておく。
    RaycastHit2D[] hits = new RaycastHit2D[8];
    ContactFilter2D contactFilter = new ContactFilter2D();


    

    public Vector2 groundVelocity {
        get {
            if (velocity.magnitude > minSpeed) {
                return groundRight * Vector2.Dot(velocity, groundRight);
                // return Mathf.Abs(Vector2.Dot(velocity, groundRight)) * velocity.normalized;
            }
            else {
                return Vector2.zero;
            }
        }
    }

    public Vector2 lastGroundVelocity { get; private set; }

    // オブジェクトが受ける重力の力を返す。アクセサで読み取り専用にする。
    // さらに、「get」で計算して値を返す。
    public Vector2 gravity
    {
        get
        {
            // プロジェクト設定の重力をオブジェクト別の度合いで調整する
            return Physics2D.gravity * gravityMultiplier;
        }
    }

    public Vector2 gravityDown
    {
        get
        {
            return Physics2D.gravity.normalized;
        }
    }

    public Vector2 gravityUp
    {
        get
        {
            return gravityDown * -1f;
        }
    }

    public Vector2 gravityRight
    {
        get
        {
            return new Vector2(-gravityDown.y, gravityDown.x);
        }
    }

    public Vector2 gravityLeft
    {
        get
        {
            return gravityRight * -1f;
        }
    }

    public bool isFalling {
        get {
            return Vector2.Dot(gravity, velocity) > 0f;
        }
    }
    
    

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
        groundDistance = Mathf.Infinity;
        groundNormal = gravityUp;
        lastGroundVelocity = Vector2.zero;
    }

    void UpdateGroundedState()
    {
        Vector2 gravityDirection = Physics2D.gravity.normalized; // 重力の方向
        // 重力の方向へキャストする
        int hitCount = rb.Cast(gravityDirection, contactFilter, hits, groundCheckDistance);
        
        // キャストの結果を確認する前に、立っている状態を初期値に戻す
        // これが地面を検出したかった場合の値になる
        isGrounded = false;
        groundDistance = Mathf.Infinity;
        groundNormal = gravityUp; // 地面の法線は重力の反対方向へ向かう
        
        // 衝突したオブジェクトを確認する
        for (int i = 0; i <hitCount; i++) {
            // 重力の方向と衝突した面の法線を比較する
            if (IsGroundNormal(hits[i].normal)) {
                // 2つの方向の差がしきい値より低いので地面になっている
                isGrounded = true; 
                groundDistance = hits[i].distance;
                groundNormal = hits[i].normal; // 衝突のあった面の法線を記憶する

                break;
            }
        }
    }

    bool IsGroundNormal(Vector2 normal)
    {
        return Vector2.Angle(gravityUp, normal) < maxGroundAngleDegrees;
    }

    Vector2 ApplyMotion(Vector2 motion)
    {
        // 衝突判定
        float distance = motion.magnitude; // 移動距離
        Vector2 direction = motion.normalized; // 移動の方向

        // Rigidbody2Dを使って、指定の方向と距離で動いたら何かに衝突するかを尋ねる。
        // 返される値は衝突した回数になる。衝突に関する詳細情報はhitsの中に記憶される。
        int hitCount = rb.Cast(direction, contactFilter, hits, distance + margin);
        for (int i = 0; i < hitCount; i++) // 衝突を一つずつ確認する。
        {
            // 最も近い衝突の地点までしか動かないように移動距離を調整する。
            distance = Mathf.Min(distance, hits[i].distance - margin);
        }

        // オブジェクトを移動させる
        motion = direction * distance;
        rb.position += motion; // Rigidbody2D経由でオブジェクトを動かす
        return motion; // 実際に動いた距離を返す
    }

    Vector2 Slide(Vector2 delta)
    {
        //if (isGrounded && delta.magnitude > 0.01f) {
        if (delta.magnitude > 0.01f) {
            float dot = Vector2.Dot(delta, groundRight);
            // Vector2 slideDirection = groundRight * Mathf.Sign(dot);
            Vector2 projection = groundRight * dot;
            return ApplyMotion(projection);
        }
        return Vector2.zero;
    }

    void FixedUpdate()
    {
        // 重力
        if (useGravity && !isGrounded) {
            velocity += gravity * Time.fixedDeltaTime;
        }

        Vector2 motion = velocity * Time.deltaTime;
        Vector2 actualMotion = ApplyMotion(motion);

        velocity = actualMotion / Time.deltaTime;

        if (velocity.magnitude < minSpeed) {
            velocity = Vector2.zero;
        }

        if (velocity.magnitude > maxSpeed) {
            velocity = velocity.normalized * maxSpeed;
        }
        // // 重力
        // if (useGravity && !isGrounded) {
        //     velocity += gravity * Time.fixedDeltaTime;
        // }

        // // １フレームでの移動距離を計算する
        // Vector2 motion = velocity * Time.fixedDeltaTime;

        // // xとy軸別の動きを取得する
        // // Vector2 motionX = new Vector2(motion.x, 0);
        // // Vector2 motionY = new Vector2(0, motion.y);
        // Vector2 motionX = gravityRight * Vector2.Dot(gravityRight, motion);
        // Vector2 motionY = gravityUp * Vector2.Dot(gravityUp, motion);

        // // Debug.DrawRay(transform.position, motionX * 100f, Color.red);
        // // Debug.DrawRay(transform.position, motionY * 100f, Color.blue);

        // Debug.DrawRay(transform.position, motion.normalized, Color.green);

        // // Debug.DrawRay(transform.position, gravityRight, Color.magenta);
        // // Debug.DrawRay(transform.position, gravityUp, Color.cyan);

        // // Debug.DrawRay(transform.position, groundRight, Color.red);
        // // Debug.DrawRay(transform.position, groundNormal, Color.blue);

        // // 移動を適用して実際に移動した距離を記憶する
        // Vector2 appliedMotionY = ApplyMotion(motionY);
        // Vector2 appliedMotionX = ApplyMotion(motionX);
        // Vector2 appliedMotion = appliedMotionX + appliedMotionY;

        // // appliedMotion += Slide(motion - appliedMotion);

        // // 速度を更新する
        // velocity = appliedMotion / Time.fixedDeltaTime;

        //  if (velocity.magnitude < minSpeed) {
        //      velocity = Vector2.zero;
        //  }

        // 地面に立っているか？
        UpdateGroundedState();

        if (isGrounded)
        {
            lastGroundVelocity = groundVelocity;

            Vector2 groundAdjust = gravityDown * Mathf.Max(0f, groundDistance - margin);
            rb.position += groundAdjust;
        }
    }

    void OnDrawGizmos()
    {
        if (!enabled) {
            return;
        }

        if (isGrounded) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, groundCheckDistance);
        }

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.down * margin * 0.5f, new Vector3(0.5f, margin, 1f));
    } 
}

} // namespaceはここまで