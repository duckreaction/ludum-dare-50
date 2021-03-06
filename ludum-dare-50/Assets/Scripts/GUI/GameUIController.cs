using System;
using System.Collections;
using System.Collections.Generic;
using DuckReaction.Common;
using Enemies;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace GUI
{
    public class GameUIController : MonoBehaviour
    {
        [SerializeField] ChooseYourEnemy _chooseUi;
        [SerializeField] StartEndLevel _startEndLevelUi;
        [SerializeField] UIDocument _victoryUi;
        [SerializeField] UIDocument _tutorialUi;

        [Inject(Optional = true)] SignalBus _signalBus;
        [Inject(Optional = true)] MainGameState _gameState;

        ChessPiece.Type _currentType;

        void Start()
        {
            SetVisibleAllUis(false);
            _signalBus?.Subscribe<GameEvent>(OnGameEventReceived);
        }

        void Update()
        {
            SetVisibleUi(3, _gameState != null && _gameState.state == MainGameState.State.Tutorial);
        }

        void OnGameEventReceived(GameEvent gameEvent)
        {
            if (gameEvent.Is(GameEventType.StartChooseEnemy))
                StartChooseEnemy();
            else if (gameEvent.Is(GameEventType.StartShowScore))
                StartShowScore();
            else if (gameEvent.Is(GameEventType.PlayGame))
                SetVisibleAllUis(false);
            else if (gameEvent.Is(GameEventType.Victory))
                ShowVictory();
        }

        void SetVisibleAllUis(bool isVisible)
        {
            _chooseUi.gameObject.SetActive(isVisible);
            _startEndLevelUi.gameObject.SetActive(isVisible);
            _victoryUi.gameObject.SetActive(isVisible);
            _tutorialUi.gameObject.SetActive(isVisible);
        }

        void SetVisibleUi(int uiIndex, bool isVisible)
        {
            if (uiIndex == 0)
                _chooseUi.gameObject.SetActive(isVisible);
            else if (uiIndex == 1)
                _startEndLevelUi.gameObject.SetActive(isVisible);
            else if (uiIndex == 2)
                _victoryUi.gameObject.SetActive(isVisible);
            else if (uiIndex == 3)
                _tutorialUi.gameObject.SetActive(isVisible);
        }

        [ContextMenu("Start choose enemy")]
        void StartChooseEnemy()
        {
            SetVisibleUi(0, true);
            _chooseUi.Refresh();
        }

        [ContextMenu("Start show score")]
        void StartShowScore()
        {
            SetVisibleUi(1, true);
            _startEndLevelUi.ShowScore(_gameState == null
                ? new()
                {
                    type = Score.Type.Fail
                }
                : _gameState.LastScore);
        }

        [ContextMenu("Test select the rook")]
        void TestPlayerChoose()
        {
            PlayerChoose(ChessPiece.Type.Rook);
        }

        [ContextMenu("Test show score perfect")]
        void TestShowPerfectScore()
        {
            SetVisibleUi(1, true);
            _startEndLevelUi.ShowScore(new()
            {
                type = Score.Type.Perfect
            });
        }

        public void PlayerChoose(ChessPiece.Type type)
        {
            _currentType = type;
            SetVisibleUi(0, false);
            SetVisibleUi(1, true);
            _startEndLevelUi.ShowEnemyName(_currentType.ToString().ToLower());
        }

        public void EndShowTitle(StartEndLevel.Type type)
        {
            Debug.Log("End " + type.ToString());
            SetVisibleAllUis(false);
            if (type == StartEndLevel.Type.showEnemy)
            {
                _signalBus?.Fire(new GameEvent(GameEventType.EnemySelected, _currentType));
            }
            else
            {
                _signalBus?.Fire(new GameEvent(GameEventType.EndShowScore, _currentType));
            }
        }

        [ContextMenu("Show victory")]
        public void ShowVictory()
        {
            SetVisibleUi(2, true);
        }
    }
}