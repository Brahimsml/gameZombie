using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public TMP_Text CurrentAmmoText;
    public TMP_Text RemainingAmmoText;
    public TMP_Text HealthText;

    public void UpdateAmmoText(int currentAmmo, int remainingAmmo)
    {
        CurrentAmmoText.text = currentAmmo.ToString();
        RemainingAmmoText.text = "/" + remainingAmmo.ToString();
    }

    public void UpdateHealthText(float currentHealth)
    {
        HealthText.text = currentHealth.ToString();
    }
}