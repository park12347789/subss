# Phase 2 도입에 따른 Phase 1 변경 사항 정리

이 문서는 **Phase 2(데미지·기본 무기)**를 도입하면서 **Phase 1(이동·카메라·입력)** 쪽에 생기는 **변경·주의·호환**을 정리합니다. 구현은 [Phase2_Implementation_Guide_and_Source.md](Phase2_Implementation_Guide_and_Source.md)를 따릅니다.

## 1. 변경이 없는 Phase 1 핵심

다음은 Phase 2를 추가해도 **원칙적으로 동작 정의가 바뀌지 않습니다**.

- `CharacterController` 기반 `MovementComponent`의 이동·가속·감속·중력
- `Phase1OrbitCameraController`의 Orbit, Zoom, Spring Arm, FPP 전환
- `InputProvider`의 `Move` / `Look` / `PointerPosition` / `Zoom` / `PrimaryHold` / `SecondaryHold` 의미

즉, **이동·카메라 수학과 Phase1 입력 스킴 자체는 유지**하는 것이 목표입니다.

## 2. 입력(Input) 계층의 변경

### 2.1 새 액션 `Attack`

- Phase 2에서 **공격 입력**이 필요하므로 `Phase1Gameplay.inputactions`의 `Player` 맵에 **`Attack` 버튼 액션**이 추가됩니다.
- **중요**: Phase 1 카메라는 **LMB(`PrimaryHold`) / RMB(`SecondaryHold`)**를 이미 사용합니다. 문서상 “공격 = LMB”와 충돌할 수 있어, 구현 가이드에서는 기본 바인딩을 **`Space` + 게임패드 `buttonSouth`**로 두었습니다.
- **Phase 1 동작에 미치는 영향**:
  - `InputProvider`가 `Attack` 액션을 **샘플링**하게 되므로, **에셋이 갱신되지 않은 구버전**과는 런타임 호환되지 않습니다(액션 누락 시 초기화 실패).
  - `Attack`은 **이동/카메라 액션과 별도**이므로, 기존 Phase1 입력 처리 순서(`DefaultExecutionOrder(-100)` 등)는 유지한 채 **추가 프로퍼티만 늘어나는 형태**입니다.

### 2.2 `InputProvider` 공개 API 확장

- `WasAttackPressedThisFrame` 같은 **한 프레임 입력**이 추가됩니다.
- **Phase 1 전용 씬**에서도 `Attack` 액션이 존재해야 하므로, **Phase 1 Validation Scene만 쓰는 경우에도** `.inputactions`는 동일하게 최신화하는 것이 안전합니다.

## 3. Player 액터 구성(컴포넌트)의 변경

### 3.1 Animator 계열 추가 (Phase 1 애니메이션 요구사항)

- Phase 1 로드맵에 맞춰 Player에 **`Animator` + `LocomotionAnimatorDriver`**가 붙습니다.
- **Phase 1 이동 로직(`MovementComponent`)은 수정 최소화**하고, 대신 **읽기 전용 프로퍼티**(`MaxMoveSpeed`, `NormalizedPlanarSpeed`)를 통해 애니메이션 쪽이 속도를 가져갑니다.
- **영향**:
  - Player 프리팹/씬에 **Animator Controller 할당**이 필요합니다(클립은 사용자가 배치).
  - Controller에 `Speed`/`IsGrounded` 파라미터가 없으면 드라이버는 **조용히 스킵**하도록 설계되어, **컨트롤러 미할당 초기 단계**에서도 크래시하지 않게 할 수 있습니다.

### 3.2 Phase 2 전투 컴포넌트 추가

- `CombatComponent`는 **`InputProvider`에 강하게 의존**합니다(`RequireComponent`).
- `CombatComponent`는 조준 방향을 위해 **`MovementComponent.LastAimPoint`**를 우선 사용합니다.
  - **Phase 1 변경점**: `useMouseRaycastRotation`이 꺼져 있으면 `LastAimPoint`가 갱신되지 않을 수 있어, **공격 조준이 캐릭터 정면 위주**가 됩니다. 전투 UX를 위해 Phase 2 씬에서는 `useMouseRaycastRotation`을 켜는 것을 권장합니다(기존 Phase1 수학은 그대로, 옵션만 조정).

## 4. 데이터·레이어·레이캐스트 정책

### 4.1 레이캐스트와 콜라이더

- `CombatComponent`는 히트 스캔을 위해 `Physics.Raycast`를 사용합니다.
- **자기 자신(플레이어 캡슐)**에 맞지 않도록 원점 오프셋·`transform.IsChildOf` 가드를 둡니다.
- **Phase 1 Ground**와의 충돌 가능성: 레이가 바닥을 먼저 맞으면 적이 아닌 바닥에 피해가 들어갈 수 있습니다. 필요 시:
  - **레이 시작 높이**를 올리거나
  - **Hit Layer Mask**에서 Ground 레이어를 제외하세요.

### 4.2 체력(`HealthComponent`)

- Player에도 `HealthComponent`를 붙이면 **Phase 1에서는 없던 “피격 가능 플레이어”**가 됩니다.
- Phase 1 Movement 전용 검증만 할 때는 Player에서 **전투 컴포넌트만 빼거나**, Dummy만 두는 편이 분리가 쉽습니다.

## 5. 에디터/씬 워크플로

- `PhaseValidationSceneTool`에 Phase 2 씬 생성 메뉴를 추가하면, **Phase 1 씬 생성 로직을 재사용**하면서 컴포넌트만 얹게 됩니다.
- **Phase 1 전용 씬**과 **Phase 2 씬**을 분리하면 회귀 범위가 명확해집니다(문서의 Smoke/Regression 정책과 일치).

## 6. 요약 표

| 영역 | Phase 1만 사용 시 | Phase 2 추가 후 |
|------|-------------------|-----------------|
| 이동·카메라 핵심 | 동일 | 동일 |
| `.inputactions` | 구버전 가능 | `Attack` 포함 **최신 필요** |
| `InputProvider` | 기존 API | `Attack` 관련 API 추가 |
| Player 구성 | Input+Move+CC+Cam | +Animator+LocomotionDriver+(Health)+Combat 권장 |
| 조준/공격 방향 | 옵션에 따름 | `LastAimPoint` 활용 시 레이캐스트 회전 옵션 권장 |
| 검증 | Movement 중심 | HitScan·체력·입력 추가 |

## 7. 다음 단계(Phase 3 예고)

- `Object Pool`/`ScriptableObject` 기반 무기 데이터로 넘어가면, `CombatComponent`의 직렬화 필드가 **`WeaponData`**로 이전되며 Phase 2의 “인라인 수치”는 축소될 수 있습니다. 그때도 **입력·애니메이션 파라미터 계약(`AttackTrig` 등)**은 유지하는 편이 안전합니다.
