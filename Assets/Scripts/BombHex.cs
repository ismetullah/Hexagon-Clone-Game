using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BombHex : Hex
{
    #region Public Variables
    [Header("Properties For Bomb")]
    public TextMeshPro text;
    #endregion

    #region Private Variables
    private int timer;
    #endregion

    #region Event Functions
    void Start()
    {
        InitVariables();
    }
    #endregion

    #region Private Methods
    private void InitVariables()
    {
        timer = Constants.DEFAULT_BOMB_TIMER_COUNT;
        text.text = timer.ToString();
    }
    #endregion

    #region Helper Methods
    /**
     * Counts down the timer
     * returns the timer.
     */
    public int CountDown()
    {
        --timer;
        text.text = timer.ToString();
        return timer;
    }

    public override void ShowOutline()
    {
        if (outline == null) return;
        base.ShowOutline();
        text.GetComponent<MeshRenderer>().sortingOrder = 4;
    }

    public override void HideOutline()
    {
        if (outline == null) return;
        base.HideOutline();
        text.GetComponent<MeshRenderer>().sortingOrder = 2;
    }
    #endregion
}