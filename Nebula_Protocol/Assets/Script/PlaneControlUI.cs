

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlaneControlUI : MonoBehaviour
{
    public Button activateButton;
    public Button deactivateButton;

    [Header("Speed Buttons")]
    public List<Button> speedButtons = new List<Button>();

    public Button missileButton;
    public TMPro.TextMeshProUGUI missileCooldownText;
    public TMPro.TextMeshProUGUI missileStatusText;

    [Header("Missile Button Colors")]
    public Color readyColor = Color.green;
    public Color waitColor = Color.red;


    [Header("Speed Levels")]
    public List<float> speedLevels = new List<float>(); // Float kecepatan

    [Header("UI Penanda")]
    public List<GameObject> objectsToEnableOnActivate = new List<GameObject>();
    public List<GameObject> objectsToDisableOnDeactivate = new List<GameObject>();

    public PlayerPlaneMovement plane;

    [Header("Speed Display")]
    public TMPro.TextMeshProUGUI speedDisplayText;

    public Button flareButton;
    public TMPro.TextMeshProUGUI flareCooldownText;

    [Header("Sound")]
    public AudioSource uiAudioSource;
    public AudioClip clickSound;

    [Header("Missile Warning Sound")]
    public AudioSource warningAudioSource; // AudioSource khusus peringatan misil
    public AudioClip missileWarningClip;   // AudioClip suara peringatan misil


    private void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }


    void Start()
    {

        activateButton.onClick.AddListener(() =>
        {
            if (plane != null)
            {
                plane.ActivateControl();
                SetObjectsActive(objectsToEnableOnActivate, true);
            }
            PlayClickSound();
        });
        //
        deactivateButton.onClick.AddListener(() =>
        {
            if (plane != null)
            {
                plane.DeactivateControl();
                SetObjectsActive(objectsToDisableOnDeactivate, false);
            }
        });


        // Pasangkan tombol ke kecepatan
        for (int i = 0; i < speedButtons.Count && i < speedLevels.Count; i++)
        {
            int index = i;
            speedButtons[i].onClick.AddListener(() =>
            {
                SetPlaneSpeed(index);
                PlayClickSound();
            });
        }

        missileButton.onClick.AddListener(() =>
        {
            if (plane != null)
            {
                plane.TryManualFireMissile();
            }
            PlayClickSound();
        });

        flareButton.onClick.AddListener(() =>
        {
            if (plane != null)
            {
                plane.DeployFlare();
            }
            PlayClickSound();
        });



    }


    void Update()
    {
        if (plane != null && speedDisplayText != null)
        {
            speedDisplayText.text = $"{plane.moveSpeed:0.0}";
        }

        if (plane != null && missileCooldownText != null && missileStatusText != null && missileButton != null)
        {
            float cd = Mathf.Max(0, plane.GetMissileCooldown());
            bool isReady = cd <= 0f;

            missileCooldownText.text = cd.ToString("0.0");
            missileStatusText.text = isReady ? "READY" : "WAIT";

            // Ubah warna tombol
            var colors = missileButton.colors;
            colors.normalColor = isReady ? readyColor : waitColor;
            colors.highlightedColor = isReady ? readyColor : waitColor;
            missileButton.colors = colors;
        }

        if (flareCooldownText != null && plane != null)
        {
            flareCooldownText.text = plane.GetFlareCooldownTime().ToString("0.0");
        }


    }


    void SetPlaneSpeed(int index)
    {
        if (plane != null && index >= 0 && index < speedLevels.Count)
        {
            plane.moveSpeed = speedLevels[index];
            Debug.Log($"Speed diubah ke: {plane.moveSpeed}");
        }

        if (speedDisplayText != null)
        {
            speedDisplayText.text = $"{plane.moveSpeed:0.0}";
        }


    }

    void SetObjectsActive(List<GameObject> objects, bool isActive)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null) obj.SetActive(isActive);
        }
    }

    public void DisableUI()
    {
        // Nonaktifkan semua UI yang berhubungan dengan kontrol pesawat
        SetObjectsActive(objectsToEnableOnActivate, false);
        SetObjectsActive(objectsToDisableOnDeactivate, false);

        // // Disable tombol-tombol juga
        // activateButton.interactable = false;
        // deactivateButton.interactable = false;

        // foreach (var btn in speedButtons)
        // {
        //     btn.interactable = false;
        // }

        plane = null; // Unlink pesawat
    }

    public void ForceSetPlane(PlayerPlaneMovement newPlane)
    {
        plane = newPlane;
        activateButton.interactable = true;
        deactivateButton.interactable = true;

        foreach (var btn in speedButtons)
        {
            btn.interactable = true;
        }

        if (speedDisplayText != null && plane != null)
        {
            speedDisplayText.text = $"{plane.moveSpeed:0.0}";
        }


    }

    public void PlayMissileWarning()
    {
        if (warningAudioSource != null && missileWarningClip != null && !warningAudioSource.isPlaying)
        {
            warningAudioSource.PlayOneShot(missileWarningClip);
        }
    }


}
