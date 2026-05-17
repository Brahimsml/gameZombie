using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject gameOverScreen;
    public GameObject survivedScreen;
    public GameObject mainMenuScreen;

    private static bool startGameImmediately = false;

    void Start()
    {
        if (startGameImmediately)
        {
            startGameImmediately = false;
            StartGameplay();
        }
        else
        {
            ShowMainMenuAtStart();
        }
    }

    void ShowMainMenuAtStart()
    {
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mainMenuScreen != null)
        {
            mainMenuScreen.SetActive(true);
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        if (survivedScreen != null)
        {
            survivedScreen.SetActive(false);
        }
    }

    void StartGameplay()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mainMenuScreen != null)
        {
            mainMenuScreen.SetActive(false);
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        if (survivedScreen != null)
        {
            survivedScreen.SetActive(false);
        }

        WaveSpawner waveSpawner = Object.FindFirstObjectByType<WaveSpawner>();

        if (waveSpawner != null)
        {
            waveSpawner.BeginWaves();
        }
    }

    public void PlayGame()
    {
        startGameImmediately = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartGame()
    {
        startGameImmediately = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        if (survivedScreen != null)
        {
            survivedScreen.SetActive(false);
        }

        if (mainMenuScreen != null)
        {
            mainMenuScreen.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}