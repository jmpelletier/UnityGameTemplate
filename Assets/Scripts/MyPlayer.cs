using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace PuzzleBox
{
    public class MyPlayer : MonoBehaviour
    {
        public int score = 0;

        public Action<int> OnScoreChanged;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        void UpdateScore(int change)
        {
            score += change;
            OnScoreChanged?.Invoke(score);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
