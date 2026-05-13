# Gameplay Systems: Movement, AI, Combat

## 1. Movement and Physics

### 1.1 Player Control Scheme
- 이동: `W`, `A`, `S`, `D`
- 카메라 Free Look: `LMB + Drag` (캐릭터 Rotation 미개입)
- 카메라/캐릭터 동기 회전: `RMB + Drag` (카메라 Yaw와 캐릭터 Yaw 동기화)
- 마우스 전진 이동: `LMB + RMB Hold` (키보드 `W` 없이 전진)
- Zoom: `Mouse Wheel` (최소 거리에서 FPP 전환)
- 공격: Mouse Left Button(기본), 보조 입력은 추후 확장

### 1.2 Rotation Model
- `RMB + Drag` 중에는 카메라가 바라보는 Yaw로 캐릭터를 즉시 보간 회전합니다.
- `LMB + Drag` 중에는 카메라만 Orbit하고 캐릭터 방향은 유지합니다.
- 필요 시 `useMouseRaycastRotation` 옵션으로 Mouse Raycast 조준 회전을 병행할 수 있습니다.

### 1.3 Translation Model
- 입력 벡터를 카메라 기준 평면 벡터로 변환합니다.
- 가속/감속 커브를 적용해 급격한 떨림을 방지합니다.
- 상태 이상(`Slow`, `Stun`)은 최종 속도 스칼라 계층에서 곱셈 적용합니다.
- `LMB + RMB` 동시 입력은 전진 입력을 합성해 한 손 이동 UX를 제공합니다.

### 1.4 Collision and Ground Rule
- 이동 계산은 지면 기준 평면에서 수행합니다.
- 경사/단차 환경은 `CharacterController` 또는 Rigidbody 기반 정책 중 하나로 고정합니다.
- MVP에서는 한 정책만 채택해 물리 동작 일관성을 유지합니다.

### 1.5 Camera Spring Arm and Auto-Follow
- 카메라는 Pivot 기준 Orbit + Spring Arm 거리 기반으로 동작합니다.
- 캐릭터-카메라 사이에 장애물이 있으면 SphereCast로 충돌 거리를 Override해 카메라를 당깁니다.
- 장애물에서 벗어나면 유저 Zoom 거리로 자연 복귀합니다.
- Auto-Follow 모드:
  - `Always`: 항상 캐릭터 후방 정렬
  - `MovingOnly`: 이동 중일 때만 후방 정렬
  - `Manual`: 자동 보정 없음

### 1.6 Environment Transition
- 카메라 렌즈 높이가 수면 높이(`waterSurfaceHeight`) 아래로 내려가면 Underwater 효과를 활성화합니다.
- 효과는 `GameObject` Root 토글 방식으로 연결하며, VFX/Volume/Post-Process를 묶어 운용합니다.

## 2. AI and Navigation

### 2.1 NavMesh Setup Rule
- Stage Bake 시 Enemy 이동 가능한 영역을 명시합니다.
- 동적 장애물은 `NavMeshObstacle`과 Carve 정책을 분리해 사용합니다.
- Spawn 직후 Agent Warp 위치를 보정해 비정상 경로 계산을 방지합니다.

### 2.2 Behavior Graph Rule
- `Selector`
  - `CanAttack`가 참이면 Attack Sequence 실행
  - 아니면 Chase/Patrol 분기
- `Sequence`
  - `CheckTarget -> MoveToTarget -> PlayAttack -> ApplyDamage`
- `Blackboard`
  - `TargetTransform`
  - `DistanceToTarget`
  - `HasLineOfSight`
  - `CurrentState`

### 2.3 Enemy Decision Tick
- 매 프레임 전체 재평가 대신 주기 평가(예: 0.1~0.2초)를 권장합니다.
- 시야/거리 판정은 물리 쿼리를 최소화하기 위해 우선 거리 필터 후 Raycast를 수행합니다.

## 3. Combat and Timing

### 3.1 Weapon Fire Loop
1. 입력 수신
2. 쿨다운 확인
3. 발사 실행(Projectile or Hit Scan)
4. 피드백(VFX/SFX/Camera Impulse)
5. 쿨다운 갱신

### 3.2 Coroutine Timing Policy
- 연사 속도: `WaitForSeconds` 또는 고정 시간 누적 타이머
- 상태 이상: 시작 시점 기록 + 종료 시점 자동 회수
- 중첩 상태는 덮어쓰기/갱신/누적 정책을 명확히 분리합니다.

### 3.3 Damage Pipeline
- 공격 결과는 `IDamageable`로 전달합니다.
- 최종 Damage 계산 순서 예시:
  - BaseDamage
  - 공격자 버프/치명타
  - 방어력/피해감소
  - 최종 Clamp

## 4. Object Pooling Integration

### 4.1 Pool Target
- Projectile
- Impact VFX
- Hit Text/UI World Element(선택)

### 4.2 Spawn/Despawn Rule
- Spawn 시 초기 상태를 완전 초기화합니다.
- Lifetime 종료 또는 충돌 시 Pool로 반납합니다.
- 비활성화 시 Trail/Particle 잔존 상태를 정리합니다.

### 4.3 Capacity Rule
- 초기 Pool Capacity는 예상 동시 사용량의 1.2~1.5배 권장
- Overflow 발생 시 확장 허용 여부를 타입별로 설정합니다.

## 5. MVP Acceptance (Gameplay)
- 조준/회전이 입력 체감 기준에서 자연스럽게 동작합니다.
- Enemy가 NavMesh 기반으로 목표를 안정적으로 추적합니다.
- 연사 전투에서도 Garbage Allocation 급증 없이 유지됩니다.
- 피격/처치 피드백이 즉시 반영되어 전투 가독성이 확보됩니다.
