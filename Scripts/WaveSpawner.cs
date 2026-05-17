using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public int zombieCount;
        public Transform[] spawnPoints;
    }

    [Header("Zombie")]
    public EnemyController enemyPrefab;
    public Transform[] patrolPoints;

    [Header("Waves")]
    public Wave[] waves;
    public float waveClearedDelay = 3f;
    public float timeBetweenWaves = 5f;
    public float timeBetweenSpawns = 1f;

    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text zombiesLeftText;
    public TMP_Text scoreText;
    public TMP_Text waveMessageText;
    public GameObject youWinText;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip waveIncomingSound;

    [Header("Survived Screen")]
    public GameObject survivedScreen;
    public TMP_Text survivedKillsText;

    private int currentWaveIndex = 0;
    private int zombiesAlive = 0;
    private int score = 0;
    private bool spawningWave;
    private bool wavesStarted;

    void Start()
    {
        if (youWinText != null)
        {
            youWinText.SetActive(false);
        }

        if (survivedScreen != null)
        {
            survivedScreen.SetActive(false);
        }

        if (waveMessageText != null)
        {
            waveMessageText.gameObject.SetActive(false);
        }

        UpdateUI();

       
    }

    public void BeginWaves()
    {
        if (wavesStarted)
        {
            return;
        }

        wavesStarted = true;
        StartCoroutine(StartFirstWaveAfterDelay());
    }

    IEnumerator StartFirstWaveAfterDelay()
    {
        yield return new WaitForSeconds(waveClearedDelay);
        StartCoroutine(StartWave());
    }

    IEnumerator StartNextWaveAfterDelay()
    {
        yield return new WaitForSeconds(waveClearedDelay);

        if (currentWaveIndex >= waves.Length)
        {
            WinGame();
        }
        else
        {
            StartCoroutine(StartWave());
        }
    }

    IEnumerator StartWave()
    {
        spawningWave = true;

        if (currentWaveIndex < waves.Length)
        {
            if (waveMessageText != null)
            {
                waveMessageText.gameObject.SetActive(true);

                if (currentWaveIndex == 0)
                {
                    waveMessageText.text = "WAVE 1 STARTING";
                }
                else if (currentWaveIndex == waves.Length - 1)
                {
                    waveMessageText.text = "FINAL WAVE INCOMING";
                }
                else
                {
                    waveMessageText.text = "WAVE " + (currentWaveIndex + 1) + " INCOMING";
                }
            }

            if (audioSource != null && waveIncomingSound != null)
            {
                audioSource.PlayOneShot(waveIncomingSound);
            }
        }

        yield return new WaitForSeconds(timeBetweenWaves);

        if (waveMessageText != null)
        {
            waveMessageText.gameObject.SetActive(false);
        }

        if (currentWaveIndex >= waves.Length)
        {
            WinGame();
            yield break;
        }

        Wave wave = waves[currentWaveIndex];

        zombiesAlive = wave.zombieCount;
        UpdateUI();

        Debug.Log("Starting " + wave.waveName + " with " + wave.zombieCount + " zombies");

        for (int i = 0; i < wave.zombieCount; i++)
        {
            SpawnZombie(wave, i);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        spawningWave = false;
    }

    void SpawnZombie(Wave wave, int spawnIndex)
    {
        if (enemyPrefab == null || wave.spawnPoints.Length == 0)
        {
            return;
        }

        Transform spawnPoint = wave.spawnPoints[spawnIndex % wave.spawnPoints.Length];

        Vector3 spawnPosition = spawnPoint.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(spawnPosition, out hit, 5f, NavMesh.AllAreas))
        {
            spawnPosition = hit.position;
        }

        EnemyController enemy = Instantiate(enemyPrefab, spawnPosition, spawnPoint.rotation);

        enemy.patrolPoints = patrolPoints;
        enemy.SetStartingPatrolIndex(spawnIndex);
        enemy.pointsHolder = null;
    }

    public void EnemyKilled()
    {
        zombiesAlive--;
        score += 1;

        UpdateUI();

        if (zombiesAlive <= 0 && !spawningWave)
        {
            currentWaveIndex++;
            spawningWave = true;
            StartCoroutine(StartNextWaveAfterDelay());
        }
    }

    void UpdateUI()
    {
        if (waveText != null)
        {
            waveText.text = "WAVE " + (currentWaveIndex + 1) + "/" + waves.Length;
        }

        if (zombiesLeftText != null)
        {
            zombiesLeftText.text = "Zombies LEFT: " + zombiesAlive;
        }

        if (scoreText != null)
        {
            scoreText.text = "KILLS: " + score;
        }
    }

    public int GetScore()
    {
        return score;
    }

    void WinGame()
    {
        if (youWinText != null)
        {
            youWinText.SetActive(true);
        }

        if (survivedScreen != null)
        {
            survivedScreen.SetActive(true);
        }

        if (survivedKillsText != null)
        {
            survivedKillsText.text = "ZOMBIES KILLED: " + score;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        Debug.Log("YOU WIN!");
    }
}