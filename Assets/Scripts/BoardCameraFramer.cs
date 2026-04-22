using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BoardCameraFramer : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float padding = 1.5f;
    [SerializeField] private bool frameOnStart = true;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
        }
    }

    private void Start()
    {
        if (frameOnStart)
        {
            FrameBoard();
        }
    }

    public void FrameBoard()
    {
        if (targetCamera == null || boardManager == null || !targetCamera.orthographic)
        {
            return;
        }

        if (!boardManager.GetActiveBoardBounds(out Bounds bounds))
        {
            return;
        }

        Vector3 cameraPosition = targetCamera.transform.position;
        cameraPosition.x = bounds.center.x;
        cameraPosition.y = bounds.center.y;
        targetCamera.transform.position = cameraPosition;

        float verticalSize = bounds.extents.y + padding;
        float horizontalSize = (bounds.extents.x + padding) / Mathf.Max(0.01f, targetCamera.aspect);
        targetCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }
}
