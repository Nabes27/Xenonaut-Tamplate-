using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class IndicatorManager : MonoBehaviour
{
    [Header("Camera & Canvas")]
    public Camera mainCamera;
    public Canvas canvas;

    [Header("Layer Masks")]
    public LayerMask playerLayer;
    public LayerMask enemyLayer;

    [Header("Indicator Prefabs")]
    public GameObject playerIndicatorPrefab;
    public GameObject enemyIndicatorPrefab;

    [Header("Scan Settings")]
    public float scanRadius = 100f;
    public float refreshRate = 0.2f;

    private Dictionary<Transform, GameObject> activeIndicators = new Dictionary<Transform, GameObject>();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        StartCoroutine(IndicatorLoop());

    }

IEnumerator IndicatorLoop()
{
    while (true)
    {
        UpdateIndicators();
        yield return new WaitForSecondsRealtime(refreshRate);
    }
}


    void UpdateIndicators()
    {
        List<Transform> targets = new List<Transform>();

        // Scan player & enemy layer
        Collider2D[] hitsPlayer = Physics2D.OverlapCircleAll(mainCamera.transform.position, scanRadius, playerLayer);
        Collider2D[] hitsEnemy = Physics2D.OverlapCircleAll(mainCamera.transform.position, scanRadius, enemyLayer);

        foreach (var hit in hitsPlayer) targets.Add(hit.transform);
        foreach (var hit in hitsEnemy) targets.Add(hit.transform);

        // Cleanup old indicators
        List<Transform> toRemove = new List<Transform>();
        foreach (var pair in activeIndicators)
        {
            if (!targets.Contains(pair.Key))
            {
                Destroy(pair.Value);
                toRemove.Add(pair.Key);
            }
        }
        foreach (var tr in toRemove) activeIndicators.Remove(tr);

        // Update/Create new indicators
        foreach (var target in targets)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);

            bool isVisible = screenPos.z > 0 &&
                             screenPos.x > 0 && screenPos.x < Screen.width &&
                             screenPos.y > 0 && screenPos.y < Screen.height;

            if (!isVisible)
            {
                // Buat jika belum ada
                if (!activeIndicators.ContainsKey(target))
                {
                    GameObject prefab = playerLayer == (playerLayer | (1 << target.gameObject.layer))
                        ? playerIndicatorPrefab
                        : enemyIndicatorPrefab;

                    if (prefab != null)
                    {
                        GameObject indicator = Instantiate(prefab, canvas.transform);
                        activeIndicators.Add(target, indicator);
                    }
                }

                // Update posisi
                if (activeIndicators.ContainsKey(target))
                {
                    Vector3 clampedPos = ClampToScreen(screenPos);
                    activeIndicators[target].transform.position = clampedPos;
                }
            }
            else
            {
                // Jika target terlihat, hapus indikator
                if (activeIndicators.ContainsKey(target))
                {
                    Destroy(activeIndicators[target]);
                    activeIndicators.Remove(target);
                }
            }
        }
    }

    Vector3 ClampToScreen(Vector3 screenPos)
    {
        screenPos.z = 0;
        float border = 40f;

        screenPos.x = Mathf.Clamp(screenPos.x, border, Screen.width - border);
        screenPos.y = Mathf.Clamp(screenPos.y, border, Screen.height - border);

        return screenPos;
    }
}
