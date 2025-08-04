using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; } = true; // Awal = pause

    public GameObject pauseUI;
    public GameObject resumeUI;

    void Start()
    {
        Time.timeScale = 0f; // Set awal pause

        if (pauseUI != null) pauseUI.SetActive(true);
        if (resumeUI != null) resumeUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;

        if (pauseUI != null) pauseUI.SetActive(IsPaused);
        if (resumeUI != null) resumeUI.SetActive(!IsPaused);
    }
}
