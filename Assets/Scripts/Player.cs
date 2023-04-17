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

// Playerコンポーネントはプレーヤーキャラクターの操作と
// 演出の制御を担当する。ゲームが複雑になると機能を複数の
// コンポーネント、またはゲームオブジェクトに分けるといい
// 場合もあるが、単純なゲームではプレーヤーに関連する全ての
// 機能をここで実装する。
public class Player : MonoBehaviour
{
    // 発射した時の効果音
    public AudioClip fireSound;

    // 発射できるか？状況によって、発射できなくしたい時に、
    // これをfalseにする。例としてアニメーションで切り替えていて、
    // 発射演出中に発射を制限している。
    public bool canFire = true;

    // Player以外（特にManager）のゲームオブジェクトがここで発生した
    // イベントに対して反応するためにC#のActionを用意する。
    public Action OnFireActions;

    // ここに使うコンポーネントへの参照を用意する。
    // PlayerInputは必要だが、スクリプトでは参照を
    // 使わない。
    AudioSource audioSource;
    Animator animator;


    // 「Start」はコンポーネントが生成され、最初のフレームが
    // 処理される直前に一度だけ実行される。ここで初期設定を行う。
    void Start()
    {
        // 「Start」でよく行う作業：依存するコンポーネントへの参照を取得する。
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>(); // アニメーションコントローラへの参照を取得
    }

    // 「Update」は毎フレーム毎に実行される。１秒間60回以上実行される事があるから、
    // なるべく重い処理を行わないといい。また、実行されるタイミングは様々な条件によって
    // 変わるので、「Time.deltaTime」などを使って必要に応じて時間を確かめる。
    void Update()
    {

    }

    // このメソッドはPlayerInputコンポーネントに呼び出される。
    void OnFire(InputValue inputValue)
    {
        if (canFire) {
            // アニメーターに通知を送信して、演出を切り替えてもらう。
            animator.SetTrigger("Fire");

            // 効果音を再生する。
            audioSource.PlayOneShot(fireSound);

            // Actionを監視しているスクリプトがあれば実行する。
            OnFireActions?.Invoke();
        }
    }
} 

} // namespaceはここまで
