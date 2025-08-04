using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;



public class SurvivalGameManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<GameObject> spawnPoints;
    public List<GameObject> enemyPrefabs;
    public float spawnDelay = 1f;
    public float checkRadius = 1f;
    public LayerMask enemyLayer;

    [Header("Enemy Count Per Round")]
    public int round1to2Count = 2;
    public int round3to5Count = 4;
    public int round6PlusCount = 6;

    [Header("UI")]
    public TMP_Text roundText;
    public TMP_Text killText;

    public TMP_Text highScoreText;

    [Header("Round Settings")]
    public int currentRound = 1;
    private int enemiesToSpawn;
    private int enemiesAlive = 0;
    private int totalKills = 0;

    [Header("Player Management")]
    public List<GameObject> playerObjects; // daftar pemain yang dimonitor
    public GameObject gameOverUI;
    public GameObject highScoreUI;
    public float gameOverDelay = 2f; // waktu tunggu sebelum pindah scene
    public int gameOverSceneIndex = 0; // <-- tambahkan ini

    private bool gameOverTriggered = false;

    private bool roundInProgress = false;

    void Start()
    {
        UpdateUI();
        StartCoroutine(StartRound());
    }

    void Update()
    {
        if (!gameOverTriggered)
        {
            CheckPlayersAlive();
        }
    }

    void CheckPlayersAlive()
    {
        // Hapus player yang sudah tidak ada
        playerObjects.RemoveAll(p => p == null);

        if (playerObjects.Count == 0)
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        gameOverTriggered = true;

        if (gameOverUI != null) gameOverUI.SetActive(true);
        if (highScoreUI != null) highScoreUI.SetActive(true);

        StartCoroutine(GoToNextSceneAfterDelay());

        PlayerPrefs.SetInt("HighScore", Mathf.Max(PlayerPrefs.GetInt("HighScore", 0), totalKills));
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "High Score: " + highScore;
        }

        PlayerPrefs.Save();

    }

    IEnumerator GoToNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);

        // Ganti ke scene index yang di-set secara publik
        SceneManager.LoadScene(gameOverSceneIndex);
    }



    IEnumerator StartRound()
    {
        if (roundInProgress) yield break; // << MENCEGAH multiple StartRound()
        roundInProgress = true;

        Debug.Log("Starting Round " + currentRound);

        enemiesToSpawn = GetEnemyCountForRound(currentRound);
        enemiesAlive = enemiesToSpawn;

        List<GameObject> tempSpawnPoints = new List<GameObject>(spawnPoints);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (tempSpawnPoints.Count == 0)
                tempSpawnPoints = new List<GameObject>(spawnPoints);

            GameObject point = tempSpawnPoints[Random.Range(0, tempSpawnPoints.Count)];
            tempSpawnPoints.Remove(point);

            StartCoroutine(SpawnEnemyWithDelay(point));
            yield return new WaitForSeconds(spawnDelay);
        }

        roundInProgress = false;
    }


    IEnumerator SpawnEnemyWithDelay(GameObject point)
    {
        int maxAttempts = 10;
        int attempt = 0;

        while (attempt < maxAttempts)
        {
            Vector2 pos = point.transform.position;
            Collider2D hit = Physics2D.OverlapCircle(pos, checkRadius, enemyLayer);

            if (hit == null)
            {
                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
                GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

                Vector2 dir = point.transform.up;
                enemy.transform.up = dir;
                yield break;
            }
            attempt++;
            yield return new WaitForSeconds(0.2f);
        }

        Debug.LogWarning("Gagal spawn enemy: terlalu padat di area.");
    }

    int GetEnemyCountForRound(int round)
    {
        if (round <= 2) return round1to2Count;
        if (round <= 5) return round3to5Count;
        return round6PlusCount;
    }

    public void OnEnemyDestroyed()
    {
        enemiesAlive--;
        totalKills++;
        UpdateUI();

        if (enemiesAlive <= 0 && !roundInProgress)
        {
            currentRound++;
            StartCoroutine(StartRound());
        }
    }

    void UpdateUI()
    {
        if (roundText != null)
            roundText.text = "Round: " + currentRound;

        if (killText != null)
            killText.text = "Kills: " + totalKills;
    }

    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (var point in spawnPoints)
        {
            if (point != null)
                Gizmos.DrawWireSphere(point.transform.position, checkRadius);
        }
    }
}
