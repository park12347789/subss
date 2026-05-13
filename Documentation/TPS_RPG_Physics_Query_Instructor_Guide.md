# TPS RPG Physics Query Instructor Guide

## 수업 목표
- 기존 TPS 이동/카메라를 유지한 채 Physics Query 기반 RPG 기능을 확장한다.
- `Raycast`, `SphereCast`, `OverlapSphere`, `CapsuleCast`, `NonAlloc` 선택 기준을 실습으로 체득한다.
- 기능 구현보다 `LayerMask`, 거리/반경 제한, 예외 처리, Debug 시각화를 함께 완성한다.

## 사전 준비
- Scene: `Assets/01.Scenes/TPS_Base.unity`
- Player: `Assets/03.Prefab/JH_PlayerArmature.prefab`
- Camera: `Assets/03.Prefab/JH_MainCamera.prefab`, `Assets/03.Prefab/JH_PlayerFollowCamera.prefab`
- InputAction: `Assets/Resources/Input/Phase1Gameplay.inputactions`
- 핵심 스크립트
  - `Assets/02.Scripts/Gameplay/Phase1/InputProvider.cs`
  - `Assets/02.Scripts/Gameplay/Interaction/TpsRayInteractor.cs`
  - `Assets/02.Scripts/Gameplay/Combat/TpsMeleeAttackComponent.cs`
  - `Assets/02.Scripts/Gameplay/Combat/TpsMagicSphereCastComponent.cs`
  - `Assets/02.Scripts/Gameplay/Combat/TpsGroundAoeSkillComponent.cs`
  - `Assets/02.Scripts/Gameplay/Phase1/TpsDashCollisionComponent.cs`
  - `Assets/02.Scripts/Gameplay/Phase1/TpsPhysicsQueryBootstrap.cs`

## Scene 세팅 체크리스트 (강의 시작 10분)
1. Layer 생성: `Enemy`, `Interactable`, `Obstacle`, `Ground`
2. 테스트 오브젝트 배치
   - NPC/Chest: `Collider + DebugInteractable`, Layer `Interactable`
   - Enemy_Dummy 여러 개: `Collider + HealthComponent`, Layer `Enemy`
   - Wall: Collider, Layer `Obstacle`
   - Terrain/Plane: Collider, Layer `Ground`
3. Game 뷰에서 중앙 조준이 유지되는지 확인
4. Play 진입 시 Player에 실습 컴포넌트가 자동 부착되는지 확인

## 데모 진행 순서 (권장 50~60분)

### 1) 상호작용 (E) - 8분
- 개념: 카메라 중앙 한 줄 판정은 `Raycast`가 가장 명확하다.
- 시연
  - NPC를 바라볼 때 prompt 로그가 출력되는지 확인
  - `E` 입력 시 `DebugInteractable.Interact()` 로그 확인
- 핵심 질문
  - 왜 `ViewportPointToRay(0.5, 0.5)`를 쓰는가?
  - 왜 `Interactable LayerMask`가 필요한가?

### 2) 근접 공격 (F) - 10분
- 개념: 근접은 방향선보다 순간 범위가 중요하므로 `OverlapSphere`.
- 시연
  - `AttackPoint` 기준 반경 안 Enemy만 피격
  - 동일 Enemy의 Multi-Collider 중복 피격 방지 확인
- 확장 과제
  - 쿨다운 변경
  - 전방 각도 제한 추가

### 3) 마법탄 (Q) - 10분
- 개념: TPS 조준 보정을 위해 `SphereCast`로 두께 부여.
- 시연
  - `radius`를 0.1 -> 0.35 -> 0.8 순서로 바꿔 피격 차이 비교
  - 빗맞는 상황에서 `Raycast` 대비 차이 설명

### 4) 광역 스킬 (R) + NonAlloc - 14분
- 개념
  - 1차: Ground를 `Raycast`로 찍고
  - 2차: 중심점에서 `OverlapSphereNonAlloc`으로 다중 검출
- 시연
  - 중심 거리 비례 피해(`1 - d/radius`) 확인
  - 버퍼 크기를 2로 줄여 누락 경고를 의도적으로 재현
  - 다시 32로 복구 후 정상 동작 확인

### 5) 대시 충돌 (Shift) - 10분
- 개념: 캐릭터 몸체를 반영하려면 `CapsuleCast`.
- 시연
  - 벽 없을 때 전체 대시
  - 벽 앞에서 `hit.distance - safetyDistance`까지만 이동
  - Debug ray 색상(충돌 red, 정상 green) 확인

## 디버깅 가이드 (학생 질문 대응)
- 상호작용이 안 됨
  - Camera null 여부 -> `MainCamera` 태그 확인
  - 대상 Collider/Layer 확인
  - `Interactable` 컴포넌트 존재 여부 확인
- 공격이 안 맞음
  - 대상이 `IDamageable` 구현(예: `HealthComponent`)인지 확인
  - `enemyMask`에 `Enemy` Layer 포함 여부 확인
- 광역 피해가 일부만 들어감
  - `bufferSize` 부족 여부와 Warning 로그 확인
- 대시가 벽을 뚫음
  - `obstacleMask` 설정, 벽 Collider 여부, `safetyDistance` 과소값 확인

## Profiler 실습 가이드 (10분)
1. `Window > Analysis > Profiler` 오픈
2. CPU Usage에서 Play 데이터 수집
3. AOE를 10회 사용(버퍼 32)
4. 비교 실험: `OverlapSphere` 버전(임시)과 `OverlapSphereNonAlloc` 버전
5. 기록 항목
   - 평균 피격 수
   - `bufferSize`
   - `GC.Alloc` 변화
   - 경고 로그 발생 여부

## 평가 루브릭 (100점)
- API 선택 근거 (20): 상황에 맞는 Query 선택 설명 가능
- Layer/거리/반경 제한 (15): 불필요 검출 억제
- RPG 결과 연결 (25): 상호작용/피해/스킬/대시 실동작
- Debug 시각화 (15): Ray/Gizmos/로그로 검증 가능
- 최적화 적용 (15): NonAlloc 또는 감지 주기 최적화
- 코드 구조 (10): 컴포넌트 분리, 입력/검출/결과 흐름 명확

## 강사용 마무리 멘트
- Physics Query는 기능 구현 도구이면서도 성능 도구다.
- “한 줄(Raycast) / 두께(SphereCast) / 범위(Overlap) / 몸체(CapsuleCast) / 반복(NonAlloc)” 판단 프레임을 계속 유지하게 지도한다.
