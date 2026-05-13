# Data and Resource Management

## 1. Data-Driven 설계 목표
- 시스템 로직과 밸런스 데이터를 분리해 콘텐츠 생산 속도를 높입니다.
- 기획 변경이 코드 수정/재배포 없이 가능한 구조를 유지합니다.
- 런타임 수정값과 원본 데이터를 분리해 데이터 오염을 방지합니다.

## 2. ScriptableObject Data Schema

## 2.1 StatData
- 사용 대상: Player, Enemy 공통 기본 능력치
- 필드 예시:
  - `MaxHP`
  - `MoveSpeed`
  - `Defense`
  - `CriticalRate`
- 규칙:
  - 런타임 체력은 별도 Runtime State로 복사해서 사용
  - 원본 SO는 절대 직접 변경하지 않음

## 2.2 WeaponData
- 사용 대상: 무기/스킬 발사 로직
- 필드 예시:
  - `BaseDamage`
  - `FireRate`
  - `ProjectileAddress`
  - `MuzzleVFXAddress`
  - `ImpactVFXAddress`
  - `FireSFX`
- 규칙:
  - Prefab 직접 참조 대신 가능하면 Address Key를 우선 사용
  - Weapon 변경 시 HUD, SFX, VFX 연동 포인트를 문서화

## 2.3 WaveData
- 사용 대상: Stage 진행/스폰 제어
- 필드 예시:
  - `WaveIndex`
  - `SpawnEntries`(Enemy Type, Count, Interval)
  - `WaveDuration`
  - `EliteRule`
- 규칙:
  - Wave 스크립트는 순수 실행기(Executor)로 유지
  - 구성 정보는 WaveData 에셋에서만 관리

## 3. Addressables 운영 정책

### 3.1 Group and Label
- Group 예시:
  - `Characters`
  - `Weapons`
  - `VFX`
  - `Stages`
- Label 예시:
  - `phase1_core`, `phase5_vfx`, `enemy_common`, `boss`
- 규칙:
  - Group은 배포/캐시 정책 단위
  - Label은 런타임 로드 집합 단위

### 3.2 Load Sequence
1. Boot 단계에서 공통 UI/핵심 에셋 초기화
2. Stage 진입 시 Stage 환경 + 기본 Enemy 선로드
3. Wave 진행 중 다음 Wave 에셋 프리로드
4. 전환 시 불필요 에셋 즉시 릴리즈

### 3.3 Unload and Lifetime
- `Addressables.Release(handle)` 호출 주체를 명확히 분리합니다.
- 로딩 주체가 해제 주체가 되도록 소유권을 일관되게 유지합니다.
- Scene 전환 시 잔존 핸들을 점검해 누수를 방지합니다.

## 4. Object Pool + Addressables 연계
- Pool의 원본 Prefab은 `Addressables`로 획득합니다.
- 초기화 시점:
  - Stage 로드 완료 후 핵심 Pool 선생성
- 종료 시점:
  - Pool 비우기 -> 핸들 해제 -> Scene 언로드 순서 고정

## 5. Failure Handling
- 로드 실패 시:
  - 대체 리소스(Fallback) 또는 재시도 정책 적용
  - 사용자에게 최소 UI 피드백 제공
- 데이터 누락 시:
  - Stage 시작 차단 또는 안전 기본값 적용
  - 에디터 Validation 로그를 통해 원인 추적

## 6. 데이터 운영 체크리스트
- 신규 Enemy 추가가 `StatData`, `WaveData`, Address Label만으로 가능한가?
- Weapon 밸런스 변경이 코드 수정 없이 가능한가?
- Stage 전환 후 Addressable 핸들이 모두 해제되는가?
- 전투 중 로딩으로 인한 Hitch가 허용 범위 내인가?
