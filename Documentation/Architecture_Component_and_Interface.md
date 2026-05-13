# Architecture: Component and Interface

## 1. 설계 원칙
- 모든 Gameplay Actor는 `Composition` 중심으로 구성합니다.
- 단일 Component는 단일 책임만 가집니다.
- 서로 다른 시스템은 `Interface` 또는 Event로만 통신합니다.
- 직접적인 하드 참조는 Inspector 연결 또는 Service Locator 최소 범위에서만 허용합니다.

## 2. Component 책임 경계

### 2.1 HealthComponent
- 책임:
  - 체력 값 유지
  - Damage 계산 진입점 제공
  - 사망 상태 전이 및 Event 발행
- 비책임:
  - 애니메이션 재생
  - 점수 반영
  - 아이템 드랍 로직

### 2.2 MovementComponent
- 책임:
  - 이동 입력을 월드 이동으로 변환
  - 가속/감속, 회전 보간
  - 이동 가능 상태(예: Stun/Root) 반영
- 비책임:
  - 공격 실행
  - 입력 수집

### 2.3 CombatComponent
- 책임:
  - 공격 가능 여부 판단(쿨다운/상태)
  - Projectile 발사 또는 Hit 처리
  - 무기 데이터(`WeaponData`) 적용
- 비책임:
  - 타겟 탐색 AI
  - 피격 체력 계산

### 2.4 InputProvider
- 책임:
  - `Input System` Action을 표준 Command로 정규화
  - Player와 AI 입력 소스의 형식 일관성 유지
- 비책임:
  - 이동 계산
  - 전투 판정

## 3. Interface 계약

### 3.1 IDamageable
- 목적:
  - Damage 소비 주체를 통일된 계약으로 추상화
- 구현 대상:
  - Player, Enemy, 파괴 가능 Prop
- 최소 요구:
  - Damage 수신 함수
  - 생존 상태 확인 가능 함수

### 3.2 IInteractable
- 목적:
  - 상호작용 대상(Item, Lever, Shrine)의 진입 규약 통일
- 구현 대상:
  - Pickup, Trigger Object, Stage Mechanism
- 최소 요구:
  - 상호작용 가능 여부 확인
  - 상호작용 실행 함수

## 4. 의존성 규칙
- `MovementComponent`는 `CombatComponent`를 참조하지 않습니다.
- `CombatComponent`는 `HealthComponent`를 직접 수정하지 않고 `IDamageable`을 호출합니다.
- UI는 직접 Component 내부 값을 Pull하지 않고 Event 기반으로 갱신합니다.
- AI는 타겟 참조 시에도 `IDamageable`/`Transform` 인터페이스 계층으로 접근합니다.

## 5. Event 흐름 표준
- `OnDamaged`
  - UI Hit Flash, 피격 VFX, SFX
- `OnDead`
  - Enemy 제거 예약, Drop 처리, Score 반영
- `OnWeaponChanged`
  - HUD 갱신, 사운드 상태 갱신

## 6. 씬 프리팹 구성 규칙
- Player Prefab
  - `InputProvider`, `MovementComponent`, `CombatComponent`, `HealthComponent`
- Enemy Prefab
  - `MovementComponent`(AI 입력), `CombatComponent`, `HealthComponent`, `NavMeshAgent` 관련 구성
- 분리 원칙:
  - 공통 기능은 재사용 Component로 추출
  - 타입별 차이는 데이터(`ScriptableObject`)로 분리

### 6.1 Validation Scene 최소 구성 규칙
- 경로/네이밍:
  - `Assets/01.Scenes/PhaseValidation/Phase_0N_<Feature>Validation.unity`
- 최소 오브젝트:
  - `Ground`(이동/충돌 검증용)
  - `Main Camera`(Phase 카메라 정책 반영)
  - Phase 핵심 시스템 오브젝트(`Player`, `Enemy`, `Spawner` 등)
- 연결 규칙:
  - Validation Scene의 핵심 오브젝트는 해당 Phase 컴포넌트만 포함해야 합니다.
  - 상위 Phase 기능 의존이 필요하면 명시적으로 분리 오브젝트를 둡니다.
  - Inspector 의존 필드가 있는 컴포넌트는 Scene 생성 Tool에서 자동 연결합니다.

## 7. 아키텍처 검증 체크리스트
- 신규 기능이 기존 Component 수정 없이 추가 가능한가?
- 시스템 간 직접 참조 없이 인터페이스로 연결되는가?
- 특정 시스템 제거 시 연쇄 수정 범위가 작게 유지되는가?
- AI/Player가 동일 전투 계약(`IDamageable`)을 공유하는가?
