/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Input Systemを利用する
using System; // Actionを利用する

// C#のnamespace（名前空間）を使うと、他のパッケージなどと、
// class名の重複を避ける。特に、「Manager」、「Player」、「UI」は
// よく使われる単語なので、namespaceを使うとその冒頭に選んだ
// 名前空間の名称（ここはMicrogame）が自動的に追加される。
// 例えば、「Player」が「Microgame.Player」になる。
namespace Microgame
{

// 以下のような行を追加することによって、
// 必須コンポーネントが指定できる。本スクリプトを
// ゲームオブジェクトに追加すると、これらの
// コンポーネントが不足しているなら、自動的に
// 追加される。ここは、「Animator」、「AudioSource」と
// 「PlayerInput」を必要とマークしている。
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(KinematicMotion))]

// Playerコンポーネントはプレーヤーキャラクターの操作と
// 演出の制御を担当する。ゲームが複雑になると機能を複数の
// コンポーネント、またはゲームオブジェクトに分けるといい
// 場合もあるが、単純なゲームではプレーヤーに関連する全ての
// 機能をここで実装する。
public class Player : MonoBehaviour
{
    [Header("Sounds")]
    // 発射した時の効果音
    public AudioClip fireSound;

    [Header("Jump")]
    public float jumpVelocity = 5f;
    public float jumpGravityMultiplier = 0.6f;

    [Header("Motion")]
    public float maxWalkSpeed = 3f;
    public float walkAcceleration = 2f;
    public float walkBreakForce = 3f;

    public float maxAirSpeed = 3f;
    public float airAcceleration = 2f;
    public float airBreakForce = 3f;

    [Header("Abilities")]
    // 発射できるか？状況によって、発射できなくしたい時に、
    // これをfalseにする。例としてアニメーションで切り替えていて、
    // 発射演出中に発射を制限している。
    public bool canFire = true;
    public bool canJump = true;

    // Player以外（特にManager）のゲームオブジェクトがここで発生した
    // イベントに対して反応するためにC#のActionを用意する。
    public Action OnFireActions;

    // ここに使うコンポーネントへの参照を用意する。
    // PlayerInputは必要だが、スクリプトでは参照を
    // 使わない。
    AudioSource audioSource;
    Animator animator;
    KinematicMotion kinematicMotion;

    float regularGravityMultiplier = 1f;

    bool isJumping = false;

    Vector2 motionInput = Vector2.zero;

    // 「Start」はコンポーネントが生成され、最初のフレームが
    // 処理される直前に一度だけ実行される。ここで初期設定を行う。
    void Start()
    {
        // 「Start」でよく行う作業：依存するコンポーネントへの参照を取得する。
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>(); // アニメーションコントローラへの参照を取得
        kinematicMotion = GetComponent<KinematicMotion>();

        regularGravityMultiplier = kinematicMotion.gravityMultiplier;
    }

    // 「Update」は毎フレーム毎に実行される。１秒間60回以上実行される事があるから、
    // なるべく重い処理を行わないといい。また、実行されるタイミングは様々な条件によって
    // 変わるので、「Time.deltaTime」などを使って必要に応じて時間を確かめる。
    void Update()
    {

    }

    void ApplyGroundMotion()
    {
        if (Mathf.Abs(motionInput.x) > 0) {
            kinematicMotion.AddImpulse(kinematicMotion.groundRight * motionInput.x * walkAcceleration * Time.fixedDeltaTime);
        }
        else {
            Vector2 groundVelocity = kinematicMotion.groundVelocity;
            Vector2 breakDirection = groundVelocity.normalized * -1f;
            Vector2 breakImpulse = breakDirection * Mathf.Min(walkBreakForce * Time.fixedDeltaTime, groundVelocity.magnitude);
            kinematicMotion.AddImpulse(breakImpulse);
        }

        float groundSpeed = kinematicMotion.groundVelocity.magnitude;
        if (groundSpeed > maxWalkSpeed) {
            Vector2 groundVelocity = kinematicMotion.groundVelocity;
            Vector2 breakDirection = groundVelocity.normalized * -1f;
            Vector2 breakImpulse = breakDirection * (groundSpeed - maxWalkSpeed);
            kinematicMotion.AddImpulse(breakImpulse);
        }
    }

    void ApplyAirMotion()
    {
        if (Mathf.Abs(motionInput.x) > 0) {
            kinematicMotion.AddImpulse(Vector2.right * motionInput.x * airAcceleration * Time.fixedDeltaTime);
        }
        else {
            float airVelocityX = kinematicMotion.velocity.x; 
            float breakDirection = Mathf.Sign(airVelocityX) * -1f;
            Vector2 breakImpulse = breakDirection * Vector2.right * Mathf.Min(airBreakForce * Time.fixedDeltaTime, Mathf.Abs(airVelocityX));
            kinematicMotion.AddImpulse(breakImpulse);
        }

        float airSpeedX = Mathf.Abs(kinematicMotion.velocity.x);
        float speedLimit = maxAirSpeed;
        if (Mathf.Sign(kinematicMotion.velocity.x) == Mathf.Sign(kinematicMotion.lastGroundVelocity.x)) {
            speedLimit = Mathf.Max(Mathf.Abs(kinematicMotion.lastGroundVelocity.x), speedLimit);
        }
        if (airSpeedX > speedLimit) {
            Vector2 breakImpulse = Vector2.right * (airSpeedX - speedLimit) * -Mathf.Sign(kinematicMotion.velocity.x);
            kinematicMotion.AddImpulse(breakImpulse);
        }
    }

    void FixedUpdate()
    {
        if (isJumping && kinematicMotion.isFalling) {
            isJumping = false;
        }

        if (isJumping) {
            kinematicMotion.gravityMultiplier = regularGravityMultiplier * jumpGravityMultiplier;
        }
        else {
            kinematicMotion.gravityMultiplier = regularGravityMultiplier;
        }

        if (kinematicMotion.isGrounded) {
            ApplyGroundMotion();
        }
        else {
            ApplyAirMotion();
        }
    }

    bool CanJump()
    {
        if (canJump) {
            return kinematicMotion.isGrounded && !isJumping;
        }
        else {
            return false;
        }
    }


    void OnJump(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            if (CanJump()) {
                isJumping = true;
                kinematicMotion.AddImpulse(Vector2.up * jumpVelocity);
            }
        }
        else {
            isJumping = false;
        }
    }

    void OnMove(InputValue inputValue)
    {
        motionInput = inputValue.Get<Vector2>();
    }
} 

} // namespaceはここまで
