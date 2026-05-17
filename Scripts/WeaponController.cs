using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    public float range;
    public Transform cam;
    public LayerMask valid_Layer;

    [Header("Effects")]
    public GameObject[] muzzleFlares;
    public GameObject impactEffect, damageEffect;

    public float flareDisplayTime = 0.1f;
    private float flareCounter;

    private bool canAutoFire;
    private float timeBetweenShots;
    private float shotsCounter = 0.2f;

    private int currentAmmo;
    private int ClipSize;
    private int remainingAmmo;
    private float damageAmount;

    [Header("Ammo Pickup")]
    public int akmAmmoPickupAmount = 15;
    public int mp7AmmoPickupAmount = 30;

    [Header("Weapon Switching")]
    public GameObject[] weaponModels;
    public bool[] weaponUnlocked;
    public int currentWeaponIndex = 0;

    [Header("Weapon Stats")]
    public int[] currentAmmoPerWeapon;
    public int[] clipSizePerWeapon;
    public int[] remainingAmmoPerWeapon;
    public float[] damagePerWeapon;
    public float[] timeBetweenShotsPerWeapon;
    public bool[] canAutoFirePerWeapon;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip gunShotSound;
    public AudioClip reloadSound;
    public AudioClip outOfAmmoSound;
    public AudioClip weaponSwitchSound;
    public float outOfAmmoSoundCooldown = 0.25f;

    private float outOfAmmoCounter;
    private UIController uiController;

    void Start()
    {
        uiController = Object.FindFirstObjectByType<UIController>();

        SetupWeaponArrays();

        weaponUnlocked[0] = true;

        EquipWeaponSilent(0);

        Reload(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        if (outOfAmmoCounter > 0f)
        {
            outOfAmmoCounter -= Time.deltaTime;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            EquipWeapon(0);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            EquipWeapon(1);
        }

        if (flareCounter > 0)
        {
            flareCounter -= Time.deltaTime;
        }
        else
        {
            HideAllMuzzleFlares();
        }
    }

    void SetupWeaponArrays()
    {
        int weaponCount = weaponModels != null && weaponModels.Length > 0 ? weaponModels.Length : 2;

        if (weaponUnlocked == null || weaponUnlocked.Length != weaponCount)
        {
            weaponUnlocked = new bool[weaponCount];
        }

        if (currentAmmoPerWeapon == null || currentAmmoPerWeapon.Length != weaponCount)
        {
            currentAmmoPerWeapon = new int[weaponCount];
        }

        if (clipSizePerWeapon == null || clipSizePerWeapon.Length != weaponCount)
        {
            clipSizePerWeapon = new int[weaponCount];
        }

        if (remainingAmmoPerWeapon == null || remainingAmmoPerWeapon.Length != weaponCount)
        {
            remainingAmmoPerWeapon = new int[weaponCount];
        }

        if (damagePerWeapon == null || damagePerWeapon.Length != weaponCount)
        {
            damagePerWeapon = new float[weaponCount];
        }

        if (timeBetweenShotsPerWeapon == null || timeBetweenShotsPerWeapon.Length != weaponCount)
        {
            timeBetweenShotsPerWeapon = new float[weaponCount];
        }

        if (canAutoFirePerWeapon == null || canAutoFirePerWeapon.Length != weaponCount)
        {
            canAutoFirePerWeapon = new bool[weaponCount];
        }

        // Weapon 0 = AKM
        clipSizePerWeapon[0] = 15;
        remainingAmmoPerWeapon[0] = 45;
        damagePerWeapon[0] = 35f;
        timeBetweenShotsPerWeapon[0] = 0.25f;
        canAutoFirePerWeapon[0] = true;

        // Weapon 1 = MP7
        if (weaponCount > 1)
        {
            clipSizePerWeapon[1] = 30;
            remainingAmmoPerWeapon[1] = 90;
            damagePerWeapon[1] = 18f;
            timeBetweenShotsPerWeapon[1] = 0.12f;
            canAutoFirePerWeapon[1] = true;
        }
    }

    public void UnlockWeapon(int weaponIndex)
    {
        if (weaponModels == null || weaponIndex < 0 || weaponIndex >= weaponModels.Length)
        {
            return;
        }

        weaponUnlocked[weaponIndex] = true;

        if (currentAmmoPerWeapon[weaponIndex] <= 0)
        {
            ReloadWeapon(weaponIndex, false);
        }

        EquipWeapon(weaponIndex);

        Debug.Log("Weapon unlocked: " + weaponIndex);
    }

 public void EquipWeapon(int weaponIndex)
{
    if (weaponModels == null || weaponIndex < 0 || weaponIndex >= weaponModels.Length)
    {
        return;
    }

    if (weaponUnlocked == null || weaponIndex >= weaponUnlocked.Length || !weaponUnlocked[weaponIndex])
    {
        Debug.Log("Weapon not unlocked yet: " + weaponIndex);
        return;
    }

    if (weaponIndex == currentWeaponIndex)
    {
        return;
    }

    for (int i = 0; i < weaponModels.Length; i++)
    {
        if (weaponModels[i] != null)
        {
            weaponModels[i].SetActive(i == weaponIndex);
        }
    }

    currentWeaponIndex = weaponIndex;
    HideAllMuzzleFlares();
    UpdateCurrentWeaponInfo();

    if (audioSource != null && weaponSwitchSound != null)
    {
        audioSource.PlayOneShot(weaponSwitchSound);
    }
}

    void EquipWeaponSilent(int weaponIndex)
    {
        if (weaponModels == null || weaponIndex < 0 || weaponIndex >= weaponModels.Length)
        {
            return;
        }

        for (int i = 0; i < weaponModels.Length; i++)
        {
            if (weaponModels[i] != null)
            {
                weaponModels[i].SetActive(i == weaponIndex);
            }
        }

        currentWeaponIndex = weaponIndex;
        HideAllMuzzleFlares();
        UpdateCurrentWeaponInfo();
    }

    void UpdateCurrentWeaponInfo()
    {
        currentAmmo = currentAmmoPerWeapon[currentWeaponIndex];
        ClipSize = clipSizePerWeapon[currentWeaponIndex];
        remainingAmmo = remainingAmmoPerWeapon[currentWeaponIndex];
        damageAmount = damagePerWeapon[currentWeaponIndex];
        timeBetweenShots = timeBetweenShotsPerWeapon[currentWeaponIndex];
        canAutoFire = canAutoFirePerWeapon[currentWeaponIndex];

        if (uiController != null)
        {
            uiController.UpdateAmmoText(currentAmmo, remainingAmmo);
        }
    }

    public void shoot()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        if (currentAmmoPerWeapon[currentWeaponIndex] <= 0)
        {
            PlayOutOfAmmoSound();
            return;
        }

        if (audioSource != null && gunShotSound != null)
        {
            audioSource.PlayOneShot(gunShotSound);
        }

        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.forward, out hit, range, valid_Layer))
        {
            Debug.Log("hit " + hit.transform.name);

            if (hit.transform.CompareTag("Enemy"))
            {
                Instantiate(damageEffect, hit.point, Quaternion.identity);

                EnemyController enemy = hit.transform.GetComponent<EnemyController>();

                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerWeapon[currentWeaponIndex]);
                }
            }
            else
            {
                Instantiate(impactEffect, hit.point, Quaternion.identity);
            }
        }

        ShowCurrentMuzzleFlare();

        flareCounter = flareDisplayTime;
        shotsCounter = timeBetweenShotsPerWeapon[currentWeaponIndex];

        currentAmmoPerWeapon[currentWeaponIndex]--;

        UpdateCurrentWeaponInfo();
    }

    void PlayOutOfAmmoSound()
    {
        if (outOfAmmoCounter > 0f)
        {
            return;
        }

        if (audioSource != null && outOfAmmoSound != null)
        {
            audioSource.PlayOneShot(outOfAmmoSound);
        }

        outOfAmmoCounter = outOfAmmoSoundCooldown;

        if (BothWeaponsFullyOutOfAmmo())
        {
            Debug.Log("Both weapons are out of ammo");
        }
        else
        {
            Debug.Log("Current weapon clip is empty");
        }
    }

    bool BothWeaponsFullyOutOfAmmo()
    {
        if (currentAmmoPerWeapon == null || remainingAmmoPerWeapon == null)
        {
            return false;
        }

        for (int i = 0; i < currentAmmoPerWeapon.Length; i++)
        {
            int current = currentAmmoPerWeapon[i];

            int reserve = 0;

            if (i < remainingAmmoPerWeapon.Length)
            {
                reserve = remainingAmmoPerWeapon[i];
            }

            if (current > 0 || reserve > 0)
            {
                return false;
            }
        }

        return true;
    }

    void ShowCurrentMuzzleFlare()
    {
        HideAllMuzzleFlares();

        if (muzzleFlares == null)
        {
            return;
        }

        if (currentWeaponIndex >= 0 && currentWeaponIndex < muzzleFlares.Length)
        {
            if (muzzleFlares[currentWeaponIndex] != null)
            {
                muzzleFlares[currentWeaponIndex].SetActive(true);
            }
        }
    }

    void HideAllMuzzleFlares()
    {
        if (muzzleFlares == null)
        {
            return;
        }

        for (int i = 0; i < muzzleFlares.Length; i++)
        {
            if (muzzleFlares[i] != null)
            {
                muzzleFlares[i].SetActive(false);
            }
        }
    }

    public void ShootHeld()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        if (canAutoFirePerWeapon[currentWeaponIndex])
        {
            shotsCounter -= Time.deltaTime;

            if (shotsCounter <= 0)
            {
                shoot();
            }
        }
    }

    public void Reload()
    {
        Reload(true);
    }

    private void Reload(bool playSound)
    {
        ReloadWeapon(currentWeaponIndex, playSound);
        UpdateCurrentWeaponInfo();
    }

    private void ReloadWeapon(int weaponIndex, bool playSound)
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        if (weaponIndex < 0 || weaponIndex >= clipSizePerWeapon.Length)
        {
            return;
        }

        if (remainingAmmoPerWeapon[weaponIndex] <= 0 || currentAmmoPerWeapon[weaponIndex] == clipSizePerWeapon[weaponIndex])
        {
            return;
        }

        if (playSound && audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        remainingAmmoPerWeapon[weaponIndex] += currentAmmoPerWeapon[weaponIndex];

        if (remainingAmmoPerWeapon[weaponIndex] >= clipSizePerWeapon[weaponIndex])
        {
            currentAmmoPerWeapon[weaponIndex] = clipSizePerWeapon[weaponIndex];
            remainingAmmoPerWeapon[weaponIndex] -= clipSizePerWeapon[weaponIndex];
        }
        else
        {
            currentAmmoPerWeapon[weaponIndex] = remainingAmmoPerWeapon[weaponIndex];
            remainingAmmoPerWeapon[weaponIndex] = 0;
        }
    }

    public void GetAmmo()
    {
        Debug.Log("ammo picked up");

        if (remainingAmmoPerWeapon != null && remainingAmmoPerWeapon.Length > 0)
        {
            remainingAmmoPerWeapon[0] += akmAmmoPickupAmount;
        }

        if (remainingAmmoPerWeapon != null && remainingAmmoPerWeapon.Length > 1)
        {
            remainingAmmoPerWeapon[1] += mp7AmmoPickupAmount;
        }

        UpdateCurrentWeaponInfo();
    }
}