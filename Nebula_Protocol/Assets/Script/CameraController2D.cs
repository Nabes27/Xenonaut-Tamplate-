using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    [Header("Pan Settings")]
    public float panSpeed = 1f;
    public Vector2 panLimitMin = new Vector2(-20, -20);
    public Vector2 panLimitMax = new Vector2(20, 20);

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 10f;

    private Vector3 dragOrigin;

    void Update()
    {
        HandlePan();
        HandleZoom();
    }

    void HandlePan()
    {
        // Ganti ke klik kanan (mouse button 1)
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPos = transform.position + difference;

            // Clamp agar kamera tidak keluar dari batas
            newPos.x = Mathf.Clamp(newPos.x, panLimitMin.x, panLimitMax.x);
            newPos.y = Mathf.Clamp(newPos.y, panLimitMin.y, panLimitMax.y);

            transform.position = newPos;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera cam = Camera.main;

        if (scroll != 0f)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}