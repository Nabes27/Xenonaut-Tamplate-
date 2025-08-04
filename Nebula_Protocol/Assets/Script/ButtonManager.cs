using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    [Header("Transisi Hitam")]
    public Image transisiHitam;
    public float fadeDuration = 1f;

    void Start()
    {
        if (transisiHitam != null)
        {
            transisiHitam.gameObject.SetActive(false);
            Color clr = transisiHitam.color;
            clr.a = 0f;
            transisiHitam.color = clr;
        }
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        StartCoroutine(LoadSceneRoutine(sceneIndex));
    }

    IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration)); // Fade Out tetap jalan saat pause

        // Reset Time.timeScale sebelum load scene
        Time.timeScale = 1f;

        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogWarning("Scene index tidak valid: " + sceneIndex);
        }
    }

    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (transisiHitam == null) yield break;

        transisiHitam.gameObject.SetActive(true);
        Color clr = transisiHitam.color;
        clr.a = startAlpha;
        transisiHitam.color = clr;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            clr.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            transisiHitam.color = clr;

            elapsed += Time.unscaledDeltaTime; // Gunakan unscaled agar tetap berjalan saat pause
            yield return null;
        }

        clr.a = endAlpha;
        transisiHitam.color = clr;

        if (endAlpha == 0f)
            transisiHitam.gameObject.SetActive(false);
    }

    // Fungsi lainnya yang juga pakai fade
    public void Sleep() => StartCoroutine(SleepAndAdvanceDay());

    IEnumerator SleepAndAdvanceDay()
    {
        yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration));
        yield return new WaitForSecondsRealtime(0.5f); // Realtime agar tetap jalan saat pause
        yield return StartCoroutine(FadeScreen(1f, 0f, fadeDuration));
    }

    public void ChangeToMorning() => StartCoroutine(ChangeToMorningRoutine());

    IEnumerator ChangeToMorningRoutine()
    {
        yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration));
        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(FadeScreen(1f, 0f, fadeDuration));
    }

    // Fungsi tambahan lainnya
    public void ToggleGameObject(GameObject target)
    {
        if (target != null)
            target.SetActive(!target.activeSelf);
    }

    public void ActivateObject(GameObject target)
    {
        if (target != null)
            target.SetActive(true);
    }

    public void DeactivateObject(GameObject target)
    {
        if (target != null)
            target.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void StopAndDestroyGameMusic()
    {
        StartCoroutine(FadeOutAndDestroyMusic());
    }

    IEnumerator FadeOutAndDestroyMusic()
    {
        GameObject musicObj = GameObject.FindWithTag("GameMusic");
        if (musicObj != null)
        {
            AudioSource audio = musicObj.GetComponent<AudioSource>();
            if (audio != null)
            {
                float startVolume = audio.volume;

                while (audio.volume > 0f)
                {
                    audio.volume -= startVolume * Time.unscaledDeltaTime / fadeDuration;
                    yield return null;
                }

                audio.Stop();
            }

            Destroy(musicObj);
        }

        yield return null;
    }
}
