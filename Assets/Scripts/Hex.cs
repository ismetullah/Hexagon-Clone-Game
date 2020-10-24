using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Neighbors
{
    public Neighbor? up;
    public Neighbor? upRight;
    public Neighbor? downRight;
    public Neighbor? down;
    public Neighbor? downLeft;
    public Neighbor? upLeft;
}

public struct Neighbor
{
    public Neighbor(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public int x, y;

    public override string ToString()
    {
        return string.Format("[Neighbor: x -> {0}, y -> {1}]", x, y);
    }
}

public struct HexObject
{
    public HexObject(Hex hex) 
    {
        this.hex = hex;
        this.x = hex.GetX();
        this.y = hex.GetY();
        this.pos = hex.GetPosition();
    }
    public Hex hex;
    public int x, y;
    public Vector3 pos;
}
public class Hex : MonoBehaviour
{
    #region Public Variables
    public GameObject outline;

    [Header("Properties For Hex")]
    public SpriteRenderer spriteRenderer;
    #endregion

    #region Private Variables
    private int x;
    private int y;
    private Vector3 oldPosition;
    private Vector3 targetPosition;

    private Task moveTask;

    private float moveTime = Constants.DEFAULT_HEX_MOVE_TIME;
    #endregion

    #region Structs

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
        if (moveTask != null)
            moveTask.Stop();
    }

    private IEnumerator Move()
    {
        float startTime = Time.time;
        while (Time.time < startTime + moveTime)
        {
            transform.position = Vector3.Lerp(oldPosition, targetPosition, (Time.time - startTime) / moveTime);
            yield return null;
        }
        transform.position = targetPosition;
    }
    #endregion

    #region Helper Methods

    public virtual void ShowOutline()
    {
        if (outline == null) return;
        outline.SetActive(true);
        outline.GetComponent<SpriteRenderer>().sortingOrder = 2;
        spriteRenderer.sortingOrder = 3;
        // Move forward
        Vector3 currentPos = this.transform.position;
        this.transform.position = new Vector3(currentPos.x, currentPos.y, 3f);
    }

    public virtual void HideOutline()
    {
        if (outline == null) return;
        outline.SetActive(false);
        outline.GetComponent<SpriteRenderer>().sortingOrder = 0;
        spriteRenderer.sortingOrder = 1;
        // Reset z axis
        Vector3 currentPos = this.transform.position;
        this.transform.position = new Vector3(currentPos.x, currentPos.y, 0f);
    }

    public void CopyWithAnimation(HexObject hex)
    {
        SetX(hex.x);
        SetY(hex.y);
        MoveTo(hex.pos);
    }

    public bool IsMoving()
    {
        return !(GetComponent<Rigidbody2D>().velocity == Vector2.zero);
    }

    public void MoveTo(Vector2 pos)
    {
        if (moveTask != null && moveTask.Running)
            moveTask.Stop();

        this.oldPosition = transform.position;
        this.targetPosition = pos;
        moveTask = new Task(Move());
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public Color GetColor()
    {
        return spriteRenderer.color;
    }

    public void SetX(int x)
    {
        this.x = x;
    }

    public int GetX()
    {
        return this.x;
    }
    public void SetY(int y)
    {
        this.y = y;
    }

    public int GetY()
    {
        return this.y;
    }

    public Vector3 GetPosition()
    {
        return this.targetPosition;
    }

    public void SetPosition(Vector3 pos)
    {
        this.targetPosition = pos;
    }
    #endregion
}
