using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Public Variables
    public Camera Camera;
    public float xOffset = 1f;
    public float yOffset = 3f;
    #endregion

    #region Singleton
    private static CameraController _instance;
    public static CameraController Instance { get { return _instance; } }
    #endregion

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

    #region Helper Methods
    /**
     * Position the camera according to the size of the grid object
     */
    public void PositionCamera(GameObject objectToView)
    {
        Bounds targetBounds = objectToView.GetBounds();
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = targetBounds.size.x / targetBounds.size.y;

        if (screenRatio >= targetRatio)
        {
            Camera.orthographicSize = targetBounds.size.y / 2 + yOffset;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            Camera.orthographicSize = targetBounds.size.y / 2 * differenceInSize + xOffset;
        }

        transform.position = new Vector3(targetBounds.center.x, targetBounds.center.y + 0.5f, -1f);
    }
    #endregion
}
