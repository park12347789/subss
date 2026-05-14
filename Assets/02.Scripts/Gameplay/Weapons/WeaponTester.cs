using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SystemicOverload.Weapons
{
    public sealed class WeaponTester : MonoBehaviour
    {
        [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();
        [SerializeField] private int ammoWeaponIndex = 2;
        [SerializeField] private int startingAmmo = 3;
        [SerializeField] private string consumeAmmoKeyName = "Space";
        [SerializeField] private TextMesh ammoStatusLabel;

        public IReadOnlyList<WeaponData> Weapons => weapons;

        private int currentAmmo;

        private void Start()
        {
            currentAmmo = Mathf.Max(0, startingAmmo);
            LogWeaponData();
            LogAmmoUseGuide();
            UpdateAmmoStatusLabel("Ready");
        }

        private void Update()
        {
            if (!WasConsumeAmmoPressedThisFrame())
            {
                return;
            }

            TryConsumeAmmoWeapon();
        }

        public void LogWeaponData()
        {
            if (weapons == null || weapons.Count == 0)
            {
                Debug.LogWarning("[WeaponTester] No WeaponData assets are assigned.", this);
                return;
            }

            Debug.Log($"[WeaponTester] Assigned weapon count: {weapons.Count}", this);

            for (int index = 0; index < weapons.Count; index++)
            {
                WeaponData weapon = weapons[index];
                if (weapon == null)
                {
                    Debug.LogWarning($"[WeaponTester] Slot {index + 1} is empty.", this);
                    continue;
                }

                Debug.Log($"[WeaponTester] Slot {index + 1}: {weapon.BuildConsoleLine()}", weapon);
            }
        }

        private void LogAmmoUseGuide()
        {
            WeaponData ammoWeapon = ResolveAmmoWeapon();
            if (ammoWeapon == null)
            {
                Debug.LogWarning("[WeaponTester] No ammo weapon is assigned for the consume-key test.", this);
                return;
            }

            Debug.Log(
                $"[WeaponTester] Press {consumeAmmoKeyName} to use {ammoWeapon.WeaponName}. " +
                $"Current Ammo: {currentAmmo}",
                this);
        }

        private void TryConsumeAmmoWeapon()
        {
            WeaponData ammoWeapon = ResolveAmmoWeapon();
            if (ammoWeapon == null)
            {
                Debug.LogWarning("[WeaponTester] Ammo consume failed: no ammo weapon found.", this);
                return;
            }

            int ammoCost = Mathf.Max(1, ammoWeapon.AmmoCostPerAttack);
            if (currentAmmo < ammoCost)
            {
                UpdateAmmoStatusLabel("Blocked");
                Debug.Log(
                    $"[WeaponTester] {ammoWeapon.WeaponName} blocked. Ammo is {currentAmmo}, " +
                    $"need {ammoCost}.",
                    this);
                return;
            }

            currentAmmo -= ammoCost;
            UpdateAmmoStatusLabel("Used");
            Debug.Log(
                $"[WeaponTester] Used {ammoWeapon.WeaponName}. Ammo -{ammoCost}, " +
                $"remaining {currentAmmo}.",
                this);
        }

        private void UpdateAmmoStatusLabel(string state)
        {
            if (ammoStatusLabel == null)
            {
                return;
            }

            WeaponData ammoWeapon = ResolveAmmoWeapon();
            string weaponName = ammoWeapon != null ? ammoWeapon.WeaponName : "Ammo Weapon";
            ammoStatusLabel.text =
                $"{weaponName} Ammo\n" +
                $"{currentAmmo}/{startingAmmo}\n" +
                $"{state}";
            ammoStatusLabel.color = currentAmmo > 0 ? Color.black : Color.red;
        }

        private WeaponData ResolveAmmoWeapon()
        {
            if (weapons == null || weapons.Count == 0)
            {
                return null;
            }

            if (ammoWeaponIndex >= 0 && ammoWeaponIndex < weapons.Count)
            {
                WeaponData indexedWeapon = weapons[ammoWeaponIndex];
                if (indexedWeapon != null && indexedWeapon.UsesAmmo)
                {
                    return indexedWeapon;
                }
            }

            for (int index = 0; index < weapons.Count; index++)
            {
                WeaponData weapon = weapons[index];
                if (weapon != null && weapon.UsesAmmo)
                {
                    return weapon;
                }
            }

            return null;
        }

        private bool WasConsumeAmmoPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }

        private void OnValidate()
        {
            ammoWeaponIndex = Mathf.Max(0, ammoWeaponIndex);
            startingAmmo = Mathf.Max(0, startingAmmo);
        }
    }
}
