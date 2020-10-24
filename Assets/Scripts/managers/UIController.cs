using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region Public Variables
    public GameObject menu;

    [Header("Texts")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI menuTitleText;

    [Header("Buttons")]
    public Button quitBtn;
    public Button startBtn;

    [Header("Menu Items")]
    public MenuItem heightItem;
    public MenuItem widthItem;
    public MenuItem colorItem;
    #endregion

    #region Private Variables
    private GameManager gameManager;
    #endregion

    #region Event Functions
    void Start()
    {
        InitVariables();
    }

    private void Update()
    {
        if (IsBackPressed())
        {
            OnBackPressed();
        }
    }
    #endregion

    #region Private Methods
    private void InitVariables()
    {
        gameManager = GameManager.Instance;
        gameManager.onScoreChange += OnScoreChange;
        gameManager.onGameEnd += OnGameEnd;
        scoreText.text = "0";
        quitBtn.onClick.AddListener(OnClickQuit);
        startBtn.onClick.AddListener(OnClickStart);
    }

    private void OnClickStart()
    {
        gameManager.StartGame(heightItem.GetValue(), widthItem.GetValue(), colorItem.GetValue());
        menuTitleText.text = "HexFall";
        HideMenu();
    }

    private void OnClickQuit()
    {
        gameManager.Quit();
    }

    private void OnScoreChange(int score)
    {
        scoreText.text = score.ToString();
    }

    private void OnGameEnd()
    {
        menuTitleText.text = "Game Over";
        ShowMenu();
    }

    private bool IsBackPressed()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    private void OnBackPressed()
    {
        if (gameManager.GetGameState() == GameState.OVER)
        {
            gameManager.Quit();
        }
        else
        {
            if (menu != null)
            {
                gameManager.SetGameState(menu.activeSelf ? GameState.PLAYING : GameState.PAUSED);
                menu.SetActive(!menu.activeSelf);
            }
        }
    }
    #endregion

    #region Public Variables
    public void ShowMenu()
    {
        if (menu != null)
        {
            gameManager.SetGameState(GameState.PAUSED);
            menu.SetActive(true);
        }
    }

    public void HideMenu()
    {
        if (menu != null)
        {
            gameManager.SetGameState(GameState.PLAYING);
            menu.SetActive(false);
        }
    }
    #endregion
}
