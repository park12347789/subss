# Phase 2 구현 가이드 및 소스 (참고)

**저장소 반영 상태:** 아래 내용은 `Assets`에 이미 적용되었습니다(`Combat` 스크립트, `LocomotionAnimatorDriver`, `InputProvider`의 `Attack`, `Phase1Gameplay.inputactions`, 에디터 메뉴 등). 이 문서는 구조 설명과 수동 복구용으로 유지합니다.

## 1. 입력: `Attack` 액션 추가

파일: [Assets/Resources/Input/Phase1Gameplay.inputactions](Assets/Resources/Input/Phase1Gameplay.inputactions)

`Player` 맵의 `actions` 배열에 다음 항목을 추가합니다(기존 `SecondaryHold` 다음 등 아무 곳이나, JSON 쉼표만 맞추면 됩니다).

```json
{
    "name": "Attack",
    "type": "Button",
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1111111111",
    "expectedControlType": "Button",
    "processors": "",
    "interactions": "",
    "initialStateCheck": false
}
```

`bindings` 배열에 추가(Phase1 카메라가 **LMB/RMB**를 사용하므로 기본 공격은 **Space / 게임패드 South**로 두고, 주석으로 LMB 통합 시 유의사항을 남기는 것을 권장합니다).

```json
{
    "name": "",
    "id": "b2c3d4e5-f6a7-8901-bcde-f22222222222",
    "path": "<Keyboard>/space",
    "interactions": "",
    "processors": "",
    "groups": "Keyboard&Mouse",
    "action": "Attack",
    "isComposite": false,
    "isPartOfComposite": false
},
{
    "name": "",
    "id": "c3d4e5f6-a7b8-9012-cdef-333333333333",
    "path": "<Gamepad>/buttonSouth",
    "interactions": "",
    "processors": "",
    "groups": "Gamepad",
    "action": "Attack",
    "isComposite": false,
    "isPartOfComposite": false
}
```

Unity 에디터에서 해당 `.inputactions`를 연 뒤 **Generate C# Class**를 쓰고 있다면 재생성하세요.

---

## 2. Phase 1: 로코모션 Animator 브리지

### 2.1 `MovementComponent`에 공개 프로퍼티 추가

파일: [Assets/Scripts/Gameplay/Phase1/MovementComponent.cs](Assets/Scripts/Gameplay/Phase1/MovementComponent.cs)

클래스 안(필드 근처)에 다음 프로퍼티를 추가합니다.

```csharp
/// <summary>
/// 인스펙터에 설정된 최대 평면 이동 속도입니다. 애니메이션 정규화에 사용합니다.
/// </summary>
public float MaxMoveSpeed => moveSpeed;

/// <summary>
/// 0~1로 정규화된 현재 평면 속도입니다. Blend Tree `Speed` 파라미터에 바로 넣을 수 있습니다.
/// </summary>
public float NormalizedPlanarSpeed =>
    moveSpeed > 0.0001f ? Mathf.Clamp01(currentPlanarVelocity.magnitude / moveSpeed) : 0.0f;
```

### 2.2 새 스크립트 `LocomotionAnimatorDriver.cs`

경로: `Assets/Scripts/Gameplay/Phase1/LocomotionAnimatorDriver.cs`

```csharp
using UnityEngine;

namespace SystemicOverload.Phase1
{
    /// <summary>
    /// Movement/CharacterController 상태를 Animator 파라미터로 전달합니다. 클립은 Animator Controller에서 배치합니다.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [DefaultExecutionOrder(50)]
    public sealed class LocomotionAnimatorDriver : MonoBehaviour
    {
        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int IsGroundedId = Animator.StringToHash("IsGrounded");

        [SerializeField] private MovementComponent movementComponent;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float speedDampTime = 0.08f;

        private Animator animator;
        private bool hasSpeedParameter;
        private bool hasIsGroundedParameter;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            movementComponent ??= GetComponent<MovementComponent>();
            characterController ??= GetComponent<CharacterController>();
            CacheParameterAvailability();
        }

        private void OnValidate()
        {
            speedDampTime = Mathf.Max(0.0f, speedDampTime);
        }

        private void Update()
        {
            if (animator == null)
            {
                return;
            }

            if (hasSpeedParameter && movementComponent != null)
            {
                float targetSpeed = movementComponent.NormalizedPlanarSpeed;
                animator.SetFloat(SpeedId, targetSpeed, speedDampTime, Time.deltaTime);
            }

            if (hasIsGroundedParameter && characterController != null)
            {
                animator.SetBool(IsGroundedId, characterController.isGrounded);
            }
        }

        /// <summary>
        /// 런타임에 존재하는 파라미터만 갱신해, 빈 Controller에도 안전하게 동작합니다.
        /// </summary>
        private void CacheParameterAvailability()
        {
            hasSpeedParameter = false;
            hasIsGroundedParameter = false;

            if (animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Float && parameter.nameHash == SpeedId)
                {
                    hasSpeedParameter = true;
                }

                if (parameter.type == AnimatorControllerParameterType.Bool && parameter.nameHash == IsGroundedId)
                {
                    hasIsGroundedParameter = true;
                }
            }
        }
    }
}
```

**Animator Controller 쪽 권장(사용자가 클립 배치)**

- Float `Speed` (0 = Idle, 1 = 이동 최대에 가깝게)
- Bool `IsGrounded`
- Phase 2와 연동할 **Trigger** `AttackTrig` (아래 Combat 참고)
- `Base Layer`에 1D Blend Tree: 파라미터 `Speed`, 임계값 0 / 1에 각각 Idle·Move 클립 할당

---

## 3. Phase 2: 전투 스크립트

### 3.1 `Assets/Scripts/Gameplay/Combat/DamagePayload.cs`

```csharp
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 한 번의 피해 적용에 필요한 최소 데이터입니다.
    /// </summary>
    public struct DamagePayload
    {
        public float Amount;
        public Transform Attacker;
    }
}
```

### 3.2 `Assets/Scripts/Gameplay/Combat/IDamageable.cs`

```csharp
namespace SystemicOverload.Combat
{
    /// <summary>
    /// 데미지 수신 계약입니다.
    /// </summary>
    public interface IDamageable
    {
        void ApplyDamage(in DamagePayload payload);
        bool IsAlive { get; }
    }
}
```

### 3.3 `Assets/Scripts/Gameplay/Combat/HealthComponent.cs`

```csharp
using System;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 체력 및 사망 이벤트를 관리합니다.
    /// </summary>
    public sealed class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100.0f;
        [SerializeField] private float currentHealth = 100.0f;

        public event Action<float, float> Damaged;
        public event Action Died;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0.0f;

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1.0f, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
        }

        public void ApplyDamage(in DamagePayload payload)
        {
            if (!IsAlive)
            {
                return;
            }

            float appliedAmount = Mathf.Max(0.0f, payload.Amount);
            if (appliedAmount <= 0.0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0.0f, currentHealth - appliedAmount);
            Damaged?.Invoke(appliedAmount, currentHealth);

            if (currentHealth <= 0.0f)
            {
                Died?.Invoke();
            }
        }

        public void ResetHealthToFull()
        {
            currentHealth = maxHealth;
        }
    }
}
```

### 3.4 `Assets/Scripts/Gameplay/Combat/CombatComponent.cs`

```csharp
using SystemicOverload.Phase1;
using UnityEngine;

namespace SystemicOverload.Combat
{
    /// <summary>
    /// 기본 히트 스캔 공격과 발사 간격을 처리합니다.
    /// </summary>
    [RequireComponent(typeof(InputProvider))]
    public sealed class CombatComponent : MonoBehaviour
    {
        private const string AttackTriggerParameterName = "AttackTrig";

        [Header("Weapon")]
        [SerializeField] private float damage = 12.0f;
        [SerializeField] private float shotsPerSecond = 4.0f;
        [SerializeField] private float maxRange = 40.0f;
        [SerializeField] private float rayOriginHeight = 1.0f;
        [SerializeField] private float rayStartForwardOffset = 0.35f;
        [SerializeField] private LayerMask hitLayerMask = ~0;

        [Header("References")]
        [SerializeField] private MovementComponent movementComponent;
        [SerializeField] private Animator animator;

        private InputProvider inputProvider;
        private float nextAllowedShotTime;

        private static readonly int AttackTriggerHash = Animator.StringToHash(AttackTriggerParameterName);

        private void Awake()
        {
            inputProvider = GetComponent<InputProvider>();
            movementComponent ??= GetComponent<MovementComponent>();
        }

        private void OnValidate()
        {
            damage = Mathf.Max(0.0f, damage);
            shotsPerSecond = Mathf.Max(0.01f, shotsPerSecond);
            maxRange = Mathf.Max(0.1f, maxRange);
        }

        private void Update()
        {
            if (!inputProvider.WasAttackPressedThisFrame)
            {
                return;
            }

            if (Time.time < nextAllowedShotTime)
            {
                return;
            }

            float interval = 1.0f / shotsPerSecond;
            nextAllowedShotTime = Time.time + interval;

            TryFireHitScan();
            TrySetAttackTrigger();
        }

        private void TryFireHitScan()
        {
            Vector3 origin = transform.position + Vector3.up * rayOriginHeight + transform.forward * rayStartForwardOffset;
            Vector3 direction = ResolveFireDirection();
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = transform.forward;
            }
            else
            {
                direction.Normalize();
            }

            if (!Physics.Raycast(origin, direction, out RaycastHit hitInfo, maxRange, hitLayerMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            if (hitInfo.collider != null && hitInfo.collider.transform.IsChildOf(transform))
            {
                return;
            }

            IDamageable damageable = hitInfo.collider.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            DamagePayload payload = new DamagePayload
            {
                Amount = damage,
                Attacker = transform
            };
            damageable.ApplyDamage(in payload);
        }

        private Vector3 ResolveFireDirection()
        {
            if (movementComponent != null)
            {
                Vector3 toAim = movementComponent.LastAimPoint - transform.position;
                toAim.y = 0.0f;
                if (toAim.sqrMagnitude > 0.0001f)
                {
                    return toAim.normalized;
                }
            }

            return transform.forward;
        }

        private void TrySetAttackTrigger()
        {
            if (animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == AttackTriggerParameterName && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(AttackTriggerHash);
                    return;
                }
            }
        }
    }
}
```

---

## 4. `InputProvider` 수정 요약

파일: [Assets/Scripts/Gameplay/Phase1/InputProvider.cs](Assets/Scripts/Gameplay/Phase1/InputProvider.cs)

- 상수 추가: `private const string AttackActionName = "Attack";`
- 필드: `private InputAction attackAction;`
- `public bool WasAttackPressedThisFrame { get; private set; }`
- `EnsureInputActionsInitialized` 안에서 `attackAction = gameplayMap.FindAction(AttackActionName, true);`
- `BindCallbacks` / `UnbindCallbacks`에 `attackAction` 등록
- `SampleActions` 마지막에 `WasAttackPressedThisFrame = attackAction.WasPressedThisFrame();`
- `ClearRuntimeState`에 `WasAttackPressedThisFrame = false;`

(`Attack` 액션이 없는 구버전 에셋을 쓰면 예외가 나므로, 반드시 `.inputactions`를 먼저 갱신하세요.)

---

## 5. 에디터: 플레이스홀더 클립 + Animator Controller 생성 (선택)

경로 예: `Assets/Editor/Phase1LocomotionAnimatorFactory.cs`

- 메뉴 예: `Tools/Systemic Overload/Animation/Create Phase1 Locomotion Placeholder Controller`
- `AnimationClip` 2개(Idle/Move)를 1프레임 더미로 만들고, `AnimatorController`에 Blend Tree(`Speed`) 구성
- `AttackTrig` 트리거와 `Any State` → `Attack` → `Locomotion` 전이는 프로젝트 취향에 맞게 추가

(Unity `AnimatorController`/`BlendTree` API는 버전별로 다소 차이가 있어, 여기서는 **에디터에서 수동 생성**해도 `LocomotionAnimatorDriver`와 호환됩니다.)

---

## 6. Phase 2 Validation Scene

[Assets/Editor/PhaseValidationSceneTool.cs](Assets/Editor/PhaseValidationSceneTool.cs)에 메뉴를 추가해 다음을 배치합니다.

- `Player`: 기존 Phase1 구성 + `Animator` + `LocomotionAnimatorDriver` + `HealthComponent` + `CombatComponent`
- `Dummy`: `Capsule` + `HealthComponent` (및 선택적 `Rigidbody` 없음)
- `CombatComponent.hitLayerMask`가 Dummy 콜라이더를 맞도록 레이어/마스크 설정

문서상 경로: `Assets/01.Scenes/PhaseValidation/Phase_02_DamageWeaponValidation.unity`

---

## 7. 검증 체크리스트

- Space(또는 바인딩한 키)로 발사 시 Dummy `Health` 감소
- `useMouseRaycastRotation`이 켜져 있으면 `LastAimPoint` 기준으로 조준 방향이 잡히는지
- Animator에 `Speed`/`IsGrounded`/`AttackTrig`가 있을 때만 값이 들어가는지(없어도 예외 없음)

다음 문서: [Phase1_vs_Phase2_Change_Log.md](Phase1_vs_Phase2_Change_Log.md)에서 Phase1 대비 변경점을 정리합니다.
