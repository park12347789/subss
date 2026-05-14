using UnityEngine;

namespace SystemicOverload.Weapons
{
    [CreateAssetMenu(
        fileName = "WPN_NewWeapon",
        menuName = "Systemic Overload/Homework/Weapon Data",
        order = 10)]
    public sealed class WeaponData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string weaponName = "New Weapon";
        [SerializeField] private string jobName = "Unassigned";

        [Header("Combat Stats")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float range = 2.0f;
        [SerializeField] private float attacksPerSecond = 1.0f;

        [Header("Special Conditions")]
        [SerializeField] private bool usesAmmo;
        [SerializeField] private int ammoCostPerAttack;
        [SerializeField] private bool isMelee;

        public string WeaponName => weaponName;
        public string JobName => jobName;
        public int Damage => damage;
        public float Range => range;
        public float AttacksPerSecond => attacksPerSecond;
        public bool UsesAmmo => usesAmmo;
        public int AmmoCostPerAttack => usesAmmo ? ammoCostPerAttack : 0;
        public bool IsMelee => isMelee;

        public string BuildConsoleLine()
        {
            string ammoText = UsesAmmo ? $"Ammo Cost {AmmoCostPerAttack}" : "No Ammo";
            string rangeTypeText = IsMelee ? "Melee" : "Ranged";

            return
                $"[{JobName}] {WeaponName} | Damage {Damage} | Range {Range:0.##} | " +
                $"Attack Speed {AttacksPerSecond:0.##}/s | {ammoText} | {rangeTypeText}";
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(weaponName))
            {
                weaponName = name;
            }

            if (string.IsNullOrWhiteSpace(jobName))
            {
                jobName = "Unassigned";
            }

            damage = Mathf.Max(0, damage);
            range = Mathf.Max(0.0f, range);
            attacksPerSecond = Mathf.Max(0.01f, attacksPerSecond);
            ammoCostPerAttack = usesAmmo ? Mathf.Max(1, ammoCostPerAttack) : 0;
        }
    }
}
