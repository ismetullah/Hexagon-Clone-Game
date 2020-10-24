using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public enum GameState
{
    PLAYING, PAUSED, OVER
}

public class GameManager : MonoBehaviour
{
    #region Public Variables
    public System.Action<int> onScoreChange;
    public System.Action<int, int, int> onGameStart;
    public System.Action onGameEnd;
    public System.Action onNewBomb;
    #endregion

    #region Private Variables
    private int score;
    private GameState gameState = GameState.OVER;
    #endregion

    #region Singleton
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }
    #endregion

    #region Event Functions
    void Awake()
    {

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    #endregion

    #region Private Methods
    private void PauseGame() => Time.timeScale = 0;

    private void ResumeGame() => Time.timeScale = 1;
    #endregion

    #region Public Methods
    public void StartGame(int gridHeight, int gridWidth, int colorCount)
    {
        ResumeGame();
        // Reset score
        score = 0;
        onScoreChange?.Invoke(score);

        onGameStart?.Invoke(gridHeight, gridWidth, colorCount);
        SetGameState(GameState.PLAYING);
    }

    public void EndGame()
    {
        PauseGame();
        onGameEnd?.Invoke();
        SetGameState(GameState.OVER);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void AddScore(int count)
    {
        int prevScore = score;
        score += count * DEFAULT_SCORE;

        // Determine if new bomb should be shown.
        if ((prevScore / BOMB_APPEAR_SCORE) < (score / BOMB_APPEAR_SCORE))
        {
            onNewBomb?.Invoke();
        }

        onScoreChange?.Invoke(score);
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public void SetGameState(GameState gameState)
    {
        this.gameState = gameState;
    }
    #endregion
}
