using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip ammoPickupSound;
    public float soundVolume = 0.8f;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player picked up ammo");

            WeaponController weapon = Object.FindFirstObjectByType<WeaponController>();

            if (weapon != null)
            {
                weapon.GetAmmo();
            }

            if (ammoPickupSound != null)
            {
                AudioSource.PlayClipAtPoint(ammoPickupSound, transform.position, soundVolume);
            }

            Destroy(gameObject);
        }
    }
}