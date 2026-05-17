using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float healAmount = 25f;
    public float rotateSpeed = 80f;

    [Header("Sound")]
    public AudioClip healthPickupSound;
    public float soundVolume = 0.8f;

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);

            if (healthPickupSound != null)
            {
                AudioSource.PlayClipAtPoint(healthPickupSound, transform.position, soundVolume);
            }

            Destroy(gameObject);
        }
    }
}