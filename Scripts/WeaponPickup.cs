using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public int weaponIndexToUnlock = 1;

    [Header("Pickup Rotate")]
    public float rotateSpeed = 80f;

    [Header("Sound")]
    public AudioClip pickupSound;
    public float soundVolume = 0.8f;

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponController weaponController = other.GetComponent<WeaponController>();

            if (weaponController != null)
            {
                weaponController.UnlockWeapon(weaponIndexToUnlock);

                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
                }

                Destroy(gameObject);
            }
        }
    }
}