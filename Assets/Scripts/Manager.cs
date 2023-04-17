/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// C#のnamespace（名前空間）を使うと、他のパッケージなどと、
// class名の重複を避ける。特に、「Manager」、「Player」、「UI」は
// よく使われる単語なので、namespaceを使うとその冒頭に選んだ
// 名前空間の名称（ここはMicrogame）が自動的に追加される。
// 例えば、「Player」が「Microgame.Player」になる。
namespace Microgame
{

// ゲームのマネージャは、ゲームの流れ全体を総括するコンポーネント。
// 主な役割は、プレーヤーのスポーンやスコアなどの管理だが、
// ゲームを構成する各「部品」のやり取りを仲介する機能もある。
public class Manager : MonoBehaviour
{
    public UI ui; // 型は参照したいスクリプト名になっている
    
    public AudioSource bgmAudioSource; // BMG用のオーディオソース

    public GameObject playerPrefab; // プレーヤーのプレハブ

    public Transform playerSpawnPoint; // プレーヤーがスポーンする位置

    GameObject playerObject; // プレーヤー

    // プレーヤーを指定の位置にスポーンする
    void SpawnPlayer(Vector3 position)
    {
        if (playerObject != null) {
            // プレーヤーがすでにいる。同時に２つのプレーヤーを作らないので、
            // 削除する。
            Destroy(playerObject); // プレーヤーを削除する
            playerObject = null; // 古くなった参照を消す
        }

        // プレハブから新しいゲームオブジェクトを生成する
        playerObject = Instantiate(playerPrefab);

        // 生成したゲームオブジェクトをスポーン位置に移動する
        playerObject.transform.position = position;

        // プレーヤーオブジェクトの「BasePlayer」コンポーネントを取得する
        Player player = playerObject.GetComponent<Player>();
    }

    // ゲームプレイを開始する
    public void GameStart()
    {
        // ゲームを開始する
        SpawnPlayer(playerSpawnPoint.position);

        // ゲームプレーが開始するので、全ての画面を隠す
        ui.ShowPlayScreen();
    }

    // ゲームをクリアさせる
    void GameClear()
    {
        // ゲームクリアした
        Destroy(playerObject); // プレーヤーを削除する
        playerObject = null; // 古くなった参照を消す

        // クリア画面を表示させる
        ui.ToggleClearScreen(true); // 最後に表示する

        bgmAudioSource.Stop(); // BGMを止める
    }

    // ゲームを「失敗」にする
    void GameFail()
    {
        // 失敗した
        Destroy(playerObject); // プレーヤーを削除する
        playerObject = null; // 古くなった参照を消す

        // クリア失敗画面を表示させる
        ui.ToggleFailScreen(true); // 最後に表示する

        bgmAudioSource.Stop(); // BGMを止める
    }

    // 「Start」はコンポーネントが生成され、最初のフレームが
    // 処理される直前に一度だけ実行される。ここで初期設定を行う。
    void Start()
    {
        // スタート画面を表示
        ui.ToggleStartScreen(true); // 最後に表示する
    }

    // 「Update」は毎フレーム毎に実行される。１秒間60回以上実行される事があるから、
    // なるべく重い処理を行わないといい。また、実行されるタイミングは様々な条件によって
    // 変わるので、「Time.deltaTime」などを使って必要に応じて時間を確かめる。
    void Update()
    {
        
    }
}

} // namespaceはここまで