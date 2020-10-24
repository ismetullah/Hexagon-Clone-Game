using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(SwipeManager))]
public class InputManager : MonoBehaviour
{
    #region Private Variables
    private GridManager gridManager;
    private GameManager gameManager;
    #endregion

    #region Event Functions
    void Start()
    {
        gameManager = GameManager.Instance;
        gridManager = GridManager.Instance;
        SwipeManager swipeManager = GetComponent<SwipeManager>();
        swipeManager.onSwipe += HandleSwipe;
        swipeManager.onTap += HandleTap;
        swipeManager.SetGetObjectPositionDelegate(GetSelectedHexPosition);
    }
    #endregion

    #region Private Methods
    private void HandleSwipe(SwipeAction swipeAction)
    {
        if (!CanInteract()) return;
        if (swipeAction.direction == SwipeDirection.None) return;

        gridManager.RotateSelected(swipeAction.direction == SwipeDirection.Clockwise);
    }

    private void HandleTap(SwipeAction swipeAction)
    {
        if (!CanInteract()) return;

        Collider2D collider = swipeAction.collider;
        if (collider != null && collider.transform.tag == "Hex")
            gridManager.SelectGroup(swipeAction.collider, swipeAction.endWPPosition);
    }

    private bool CanInteract()
    {
        return gridManager.CanInteract() && gameManager.GetGameState() == GameState.PLAYING;
    }

    private Vector3? GetSelectedHexPosition()
    {
        try
        {
            return gridManager.GetSelectedHex().gameObject.transform.position;
        }
        catch (Exception)
        {
            return null;
        }
    }
    #endregion
}