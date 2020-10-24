using UnityEngine;
using System.Collections;

public struct SwipeAction
{
    public SwipeDirection direction;
    public Vector2 rawDirection;
    public Vector2 startPosition;
    public Vector2 endPosition;
    public Vector2 endWPPosition;
    public float startTime;
    public float endTime;
    public float duration;
    public bool longPress;
    public float distance;
    public float longestDistance;
    public Collider2D collider;

    public override string ToString()
    {
        return string.Format("[SwipeAction: {0}, From {1}, To {2}, Delta {3}, Time {4:0.00}s]", direction, rawDirection, startPosition, endPosition, duration);
    }
}

public enum SwipeDirection
{
    None,
    Clockwise,
    AntiClockwise
}

public class SwipeManager : MonoBehaviour
{
    public delegate Vector3? GetObjectPositionDelegate();
    private GetObjectPositionDelegate GetObjectPosition;

    public System.Action<SwipeAction> onSwipe;
    public System.Action<SwipeAction> onTap;


    [Range(0f, 200f)]
    public float minSwipeLength = 100f;

    private bool isValidTap = false;

    Vector2 currentSwipe;
    SwipeAction currentSwipeAction = new SwipeAction();

    void Update()
    {
        DetectSwipe();
    }

    public void DetectSwipe()
    {
        var touches = InputHelper.GetTouches();
        if (touches.Count > 0)
        {
            Touch t = touches[0];

            if (t.phase == TouchPhase.Began)
            {
                isValidTap = true;
                ResetCurrentSwipeAction(t);
            }

            if (t.phase == TouchPhase.Moved && isValidTap)
            {
                UpdateCurrentSwipeAction(t);
                // Make sure it was a legit swipe, not a tap
                if (currentSwipeAction.distance < minSwipeLength || currentSwipeAction.longPress) // Didnt swipe enough or this is a tap
                {
                    currentSwipeAction.direction = SwipeDirection.None; // Invalidate current swipe action
                    return;
                }
                isValidTap = false;
                onSwipe?.Invoke(currentSwipeAction);
            }

            if (t.phase == TouchPhase.Ended)
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(currentSwipeAction.endPosition);
                Collider2D collider = Physics2D.OverlapPoint(wp);
                if (isValidTap && collider != null)
                {
                    currentSwipeAction.collider = collider;
                    currentSwipeAction.endWPPosition = wp;
                    onTap?.Invoke(currentSwipeAction);
                    isValidTap = false;
                }
            }
        }
    }

    void ResetCurrentSwipeAction(Touch t)
    {
        currentSwipeAction.duration = 0f;
        currentSwipeAction.distance = 0f;
        currentSwipeAction.longestDistance = 0f;
        currentSwipeAction.longPress = false;
        currentSwipeAction.startPosition = new Vector2(t.position.x, t.position.y);
        currentSwipeAction.startTime = Time.time;
        currentSwipeAction.endPosition = currentSwipeAction.startPosition;
        currentSwipeAction.endTime = currentSwipeAction.startTime;
        currentSwipeAction.collider = null;
    }

    void UpdateCurrentSwipeAction(Touch t)
    {
        currentSwipeAction.endPosition = new Vector2(t.position.x, t.position.y);
        currentSwipeAction.endTime = Time.time;
        currentSwipeAction.duration = currentSwipeAction.endTime - currentSwipeAction.startTime;
        currentSwipe = currentSwipeAction.endPosition - currentSwipeAction.startPosition;
        currentSwipeAction.rawDirection = currentSwipe;
        currentSwipeAction.direction = GetSwipeDirection(currentSwipe);
        currentSwipeAction.distance = Vector2.Distance(currentSwipeAction.startPosition, currentSwipeAction.endPosition);
        if (currentSwipeAction.distance > currentSwipeAction.longestDistance) // If new distance is longer than previously longest
        {
            currentSwipeAction.longestDistance = currentSwipeAction.distance; // Update longest distance
        }
    }

    SwipeDirection GetSwipeDirection(Vector2 direction)
    {
        var swipeDirection = SwipeDirection.None;

        Vector3 currentPosition = currentSwipeAction.endPosition;
        Vector3 startPosition = currentSwipeAction.startPosition;

        float distanceX = currentPosition.x - startPosition.x;
        float distanceY = currentPosition.y - startPosition.y;

        Vector3? objectPosition = GetObjectPosition();
        if ((Mathf.Abs(distanceX) > minSwipeLength || Mathf.Abs(distanceY) > minSwipeLength) && objectPosition.HasValue)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(objectPosition.Value);

            bool triggerOnX = Mathf.Abs(distanceX) > Mathf.Abs(distanceY);
            bool swipeRightUp = triggerOnX ? distanceX > 0 : distanceY > 0;
            bool touchThanHex = triggerOnX ? currentPosition.y > screenPosition.y : currentPosition.x > screenPosition.x;
            bool clockWise = triggerOnX ? swipeRightUp == touchThanHex : swipeRightUp != touchThanHex;
            if (clockWise)
                swipeDirection = SwipeDirection.Clockwise;
            else
                swipeDirection = SwipeDirection.AntiClockwise;
        }

        return swipeDirection;
    }

    public void SetGetObjectPositionDelegate(GetObjectPositionDelegate deleg)
    {
        this.GetObjectPosition = deleg;
    }
}