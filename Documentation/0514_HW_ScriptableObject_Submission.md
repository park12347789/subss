# 0514 H.W ScriptableObject Weapon Data

## Assignment Target

- Selected job: Assassin
- Scene: `Assets/01.Scenes/HW_0514_WeaponDataScene.unity`
- Runtime object: `HW_0514_WeaponDataRoot/WeaponTester`
- Data folder: `Assets/GameData/Weapons`

## WeaponData Fields

`WeaponData` is a ScriptableObject asset. It stores weapon balance data only.

- Basic info: weapon name, job name
- Combat stats: damage, range, attacks per second
- Special conditions: uses ammo, ammo cost per attack, is melee

## Created Weapon Assets

| Asset | Weapon | Damage | Range | Attack Speed | Ammo | Melee |
| --- | --- | ---: | ---: | ---: | --- | --- |
| `WPN_Assassin_Dagger.asset` | 단검 | 18 | 2.0 | 3.2/s | No | Yes |
| `WPN_Assassin_PoisonDagger.asset` | 독 단검 | 26 | 2.2 | 2.4/s | No | Yes |
| `WPN_Assassin_ThrowingKnife.asset` | 투척 칼 | 14 | 16.0 | 2.8/s | 1 per attack | No |

## Console Output Check

When the scene starts, `WeaponTester` reads the three assigned ScriptableObject assets and logs each weapon line.
During play mode, pressing `Space` uses the ammo weapon (`투척 칼`). Runtime ammo starts at `3`, decreases by `1`, and the use is blocked when ammo is lower than the required cost. This runtime ammo value is kept in `WeaponTester`, not inside the `WeaponData` ScriptableObject asset. The on-screen `투척 칼 Ammo` TextMesh is updated at the same time.

Expected output shape:

```text
[WeaponTester] Assigned weapon count: 3
[WeaponTester] Slot 1: [암살자] 단검 | Damage 18 | Range 2 | Attack Speed 3.2/s | No Ammo | Melee
[WeaponTester] Slot 2: [암살자] 독 단검 | Damage 26 | Range 2.2 | Attack Speed 2.4/s | No Ammo | Melee
[WeaponTester] Slot 3: [암살자] 투척 칼 | Damage 14 | Range 16 | Attack Speed 2.8/s | Ammo Cost 1 | Ranged
[WeaponTester] Press Space to use 투척 칼. Current Ammo: 3
[WeaponTester] Used 투척 칼. Ammo -1, remaining 2.
[WeaponTester] Used 투척 칼. Ammo -1, remaining 1.
[WeaponTester] Used 투척 칼. Ammo -1, remaining 0.
[WeaponTester] 투척 칼 blocked. Ammo is 0, need 1.
```
