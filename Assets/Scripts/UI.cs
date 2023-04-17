/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI関連の機能を使う
using TMPro; // Text Mesh Proを使う

// C#のnamespace（名前空間）を使うと、他のパッケージなどと、
// class名の重複を避ける。特に、「Manager」、「Player」、「UI」は
// よく使われる単語なので、namespaceを使うとその冒頭に選んだ
// 名前空間の名称（ここはMicrogame）が自動的に追加される。
// 例えば、「Player」が「Microgame.Player」になる。
namespace Microgame
{

// UIコンポーネントはゲームのGUI関連の演出や管理を行うコンポーネント。
// 基本的なレベルでは各画面の表示切り替えを制御する。
public class UI : MonoBehaviour
{
    // 各画面をUnityのインスペクターで指定できるようにする。
    public GameObject startScreen;
    public GameObject clearScreen;
    public GameObject failScreen;

    // スタート画面の表示を切り替える。
    public void ToggleStartScreen(bool visible)
    {
        startScreen.SetActive(visible);
        if (visible) {
            // 表示なら他の画面を非表示にする。
            ToggleClearScreen(false);
            ToggleFailScreen(false);
        }
    }

    // クリア画面の表示を切り替える。
    public void ToggleClearScreen(bool visible)
    {
        clearScreen.SetActive(visible);
        if (visible) {
            // 表示なら他の画面を非表示にする。
            ToggleStartScreen(false);
            ToggleFailScreen(false);
        }
    }

    // 失敗画面の表示を切り替える。
    public void ToggleFailScreen(bool visible)
    {
        failScreen.SetActive(visible);
        if (visible) {
            // 表示なら他の画面を非表示にする。
            ToggleClearScreen(false);
            ToggleStartScreen(false);
        }
    }

    // プレー画面を表示する。
    public void ShowPlayScreen()
    {
        // UIをすべて非表示にする。
        ToggleClearScreen(false);
        ToggleStartScreen(false);
        ToggleFailScreen(false);
    }


    // 「Start」はコンポーネントが生成され、最初のフレームが
    // 処理される直前に一度だけ実行される。ここで初期設定を行う。
    void Start()
    {
        
    }

    // 「Update」は毎フレーム毎に実行される。１秒間60回以上実行される事があるから、
    // なるべく重い処理を行わないといい。また、実行されるタイミングは様々な条件によって
    // 変わるので、「Time.deltaTime」などを使って必要に応じて時間を確かめる。
    void Update()
    {
        
    }
}

} // namespaceはここまで