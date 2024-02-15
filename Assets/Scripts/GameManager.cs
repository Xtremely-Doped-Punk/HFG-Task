using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HFG
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static bool ResetAndCheckDebugLoops { get { loopcount = Instance.DebugLoopCount; return loopcount <= 0; } }
        public static int loopcount = 0;
        [SerializeField] private int DebugLoopCount = 0;

        [SerializeField] BoardSetupUI board;

        // A maps to 0 and so on in terms of indices
        [SerializeField] List<int> initalTilePositons;
        [SerializeField] List<int> finalTilePositons;

        [SerializeField] private Button retryBtn;
        [SerializeField] private TMP_Text finalPositionText;
        [SerializeField] private TMP_Text gameOverTxt;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartGame();
            retryBtn.onClick.AddListener(StartGame);
        }

        void StartGame()
        {
            int initCount = initalTilePositons.Count, finalCount = finalTilePositons.Count;
            if (initCount != finalCount)
            {
                Debug.LogError($"Tile count for inital config:{initCount} doesn't match with final config:{finalCount}");
                GameOver();
                return;
            }
            if (initCount == 0)
            {
                GameOver();
                return;
            }

            board = BoardSetupUI.Instance;
            board.InitializeBoard(initalTilePositons.ToHashSet());
            finalPositionText.text = $"Result needed:\n{{{String.Join(", ", finalTilePositons.ToArray())}}}";

            gameOverTxt.gameObject.SetActive(false);
            //retryBtn.gameObject.SetActive(false);
        }
        public void CheckGameOver()
        {
            if (board.OccupiedNodes.SetEquals(finalTilePositons))
                GameOver();
        }

        private void GameOver()
        {
            board.enabled = false;
            gameOverTxt.gameObject.SetActive(true);
            board.DisableAllTiles();
            //retryBtn.gameObject.SetActive(true);
        }
    }
}