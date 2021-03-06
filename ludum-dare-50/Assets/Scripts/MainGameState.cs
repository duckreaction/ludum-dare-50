using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DuckReaction.Common;
using Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class MainGameState : MonoBehaviour
{
    public enum State
    {
        Unknown,
        Start,
        Tutorial,
        GameOverTutorial,
        ChooseEnemy,
        EnemyIntro,
        Play,
        Win,
        GameOver,
        Victory,
        ShowScore
    }

    [SerializeField] string[] _firstScenes = {"Scenes/Home"};
    [SerializeField] string[] _playScenes = {"Scenes/MainScene"};

    public State state { get; private set; } = State.Unknown;

    public int TotalStars => _stars.Values.Sum();

    public bool QueenIsLocked => TotalStars < 7;
    public bool QueenIsDead => _stars.GetValueOrDefault(ChessPiece.Type.Queen, 0) > 0;

    public Score LastScore { get; private set; } = new()
    {
        type = Score.Type.Fail
    };

    public bool TutorialSuccess { get; private set; }

    [Inject] SceneService _sceneService;
    [Inject] SignalBus _signalBus;

    ChessPiece.Type _currentEnemyType;
    Dictionary<ChessPiece.Type, int> _stars = new();

    void Start()
    {
        _signalBus.Subscribe<GameEvent>(OnGameEventReceived);
    }

    void OnGameEventReceived(GameEvent gameEvent)
    {
        if (gameEvent.Is(GameEventType.ClickNext))
            Next();
        else if (gameEvent.Is(GameEventType.EndScreenTransition))
            OnEndScreenTransition();
        else if (gameEvent.Is(GameEventType.LevelAnimationEnd))
            StateShowScore();
        else if (gameEvent.Is(GameEventType.EnemySelected))
            StatePlay(gameEvent.GetParam<ChessPiece.Type>());
        else if (gameEvent.Is(GameEventType.LevelGameOver))
            OnLevelGameOver();
        else if (gameEvent.Is(GameEventType.LevelWin))
            OnLevelWin(gameEvent.GetParam<Score>());
        else if (gameEvent.Is(GameEventType.EndShowScore))
            OnEndShowScore();
    }

    void Update()
    {
        if (state == State.Unknown)
        {
            StateStart();
        }
    }

    void StateStart()
    {
        state = State.Start;
        if (SceneManager.sceneCount == 1)
            _sceneService.StartSceneTransition(Array.Empty<string>(), _firstScenes);
    }

    void Next()
    {
        if (state == State.Start)
            StateTutorial();
    }

    [ContextMenu("Start tutorial")]
    public void TestTutorial()
    {
        state = State.Tutorial;
        _currentEnemyType = ChessPiece.Type.Pawn;
        OnEndScreenTransition();
    }

    [ContextMenu("Choose enemy")]
    public void TestChooseEnemy()
    {
        StateChooseEnemy();
    }

    void OnEndScreenTransition()
    {
        if (state == State.Tutorial)
        {
            StartCoroutine(WaitSceneLoadedAndPlayGameCoroutine());
        }
    }

    IEnumerator WaitSceneLoadedAndPlayGameCoroutine()
    {
        // La sc??ne vient d'??tre charg??e, il faut attendre que tous les objets soit initialis??s
        yield return null;
        yield return null;
        _signalBus.Fire(new GameEvent(GameEventType.PlayGame, _currentEnemyType));
    }

    void StateTutorial()
    {
        state = State.Tutorial;
        _currentEnemyType = ChessPiece.Type.Pawn;
        _sceneService.StartSceneTransition(_firstScenes, _playScenes);
    }

    void ReplayTutorial()
    {
        state = State.Tutorial;
        _currentEnemyType = ChessPiece.Type.Pawn;
        _signalBus.Fire(new GameEvent(GameEventType.PlayGame, _currentEnemyType));
    }

    void StatePlay(ChessPiece.Type type)
    {
        state = State.Play;
        _currentEnemyType = type;
        _signalBus.Fire(new GameEvent(GameEventType.PlayGame, _currentEnemyType));
    }

    void StateShowScore()
    {
        state = State.ShowScore;
        _signalBus.Fire(new GameEvent(GameEventType.StartShowScore));
    }

    void StateChooseEnemy()
    {
        state = State.ChooseEnemy;
        _signalBus.Fire(new GameEvent(GameEventType.StartChooseEnemy));
    }

    void OnLevelWin(Score score)
    {
        LastScore = score;
        if (_currentEnemyType != ChessPiece.Type.Pawn)
        {
            // Le pion ne compte pas dans les scores
            var starCount = score.StarCount;
            _stars[_currentEnemyType] = Math.Max(_stars.GetValueOrDefault(_currentEnemyType, 0), starCount);
        }
        else
        {
            TutorialSuccess = true;
        }
    }

    void OnLevelGameOver()
    {
        LastScore = new()
        {
            type = Score.Type.Fail
        };
        _stars.Clear();
    }

    void OnEndShowScore()
    {
        if (QueenIsDead)
        {
            state = State.Victory;
            _signalBus.Fire(new GameEvent(GameEventType.Victory));
        }
        else if (TutorialSuccess)
            StateChooseEnemy();
        else
            ReplayTutorial();
    }

    public int GetStarCount(ChessPiece.Type item2)
    {
        return _stars.GetValueOrDefault(item2, 0);
    }
}