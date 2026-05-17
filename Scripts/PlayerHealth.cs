using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Image healthBarFill;
    public GameObject gameOverScreen;
    public TMP_Text gameOverKillsText;

    [Header("Damage Effect")]
    public GameObject bloodEffect;
    public float bloodEffectTime = 0.25f;

    [Header("Heal Effect")]
    public GameObject healEffect;
    public float healEffectTime = 0.25f;

    private UIController uiController;
    private bool isDead;

    void Start()
    {
        uiController = Object.FindFirstObjectByType<UIController>();

        currentHealth = maxHealth;

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        if (bloodEffect != null)
        {
            bloodEffect.SetActive(false);
        }

        if (healEffect != null)
        {
            healEffect.SetActive(false);
        }

        if (uiController != null)
        {
            uiController.UpdateHealthText(currentHealth);
        }

        UpdateHealthBar();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (uiController != null)
        {
            uiController.UpdateHealthText(currentHealth);
        }

        UpdateHealthBar();

        if (bloodEffect != null)
        {
            StartCoroutine(ShowBloodEffect());
        }

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator ShowBloodEffect()
    {
        bloodEffect.SetActive(true);

        yield return new WaitForSecondsRealtime(bloodEffectTime);

        bloodEffect.SetActive(false);
    }

    public void Heal(float healAmount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (uiController != null)
        {
            uiController.UpdateHealthText(currentHealth);
        }

        UpdateHealthBar();

        if (healEffect != null)
        {
            StartCoroutine(ShowHealEffect());
        }

        Debug.Log("Player Health: " + currentHealth);
    }

    IEnumerator ShowHealEffect()
    {
        healEffect.SetActive(true);

        yield return new WaitForSecondsRealtime(healEffectTime);

        healEffect.SetActive(false);
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log("Player Dead");

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        WaveSpawner waveSpawner = Object.FindFirstObjectByType<WaveSpawner>();

        if (gameOverKillsText != null && waveSpawner != null)
        {
            gameOverKillsText.text = "ZOMBIES KILLED: " + waveSpawner.GetScore();
        }

        Time.timeScale = 0f;
    }
}