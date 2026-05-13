# VFX and Feedback Pipeline

## 1. 목표
- 전투 정보를 시각/청각/카메라 반응으로 즉시 전달합니다.
- 정보 가독성과 타격감을 동시에 확보합니다.
- 고빈도 전투 상황에서도 프레임 안정성을 유지합니다.

## 2. VFX 역할 분리

### 2.1 Shuriken (Particle System)
- 용도:
  - 피격 스파크
  - 탄피/먼지
  - 짧은 단발성 타격 이펙트
- 장점:
  - 제작 속도가 빠르고 소규모 이펙트에 효율적

### 2.2 VFX Graph
- 용도:
  - 장판형 스킬
  - 대규모 마법/포탈
  - 다량 입자 기반 연출
- 장점:
  - GPU 가속 기반 대량 파티클 연산에 유리

## 3. Camera Feedback (`Cinemachine Impulse`)

### 3.1 적용 원칙
- 강한 타격 이벤트에서만 Impulse를 발행합니다.
- 연속 타격 시 과도한 누적 흔들림을 제한합니다.
- Boss/Elite/Normal 공격 강도를 분리해 프로파일을 다르게 운용합니다.

### 3.2 권장 프로파일
- `LightHit`: 짧고 약한 진동
- `HeavyHit`: 중간 길이/강도
- `UltimateHit`: 강한 순간 반응 + 짧은 감쇠

## 4. Hit Feedback Chain
1. Attack 실행
2. Impact 판정
3. Hit VFX Spawn
4. Hit SFX 재생
5. Damage Number/UI 반영
6. 필요 시 Camera Impulse 발행

## 5. 성능 가이드
- 반복 생성되는 VFX는 Pool로 관리합니다.
- `VFX Graph` 출력량은 플랫폼 기준으로 단계별 품질 옵션을 둡니다.
- 모바일/저사양 타깃이 포함되면 Particle Budget을 별도 정의합니다.
- Scene당 동시 활성 VFX 수를 모니터링하고 상한값을 운영합니다.

## 6. 아트/기획 협업 규칙
- 이펙트 프리셋은 Naming Rule을 통일합니다.
  - 예: `VFX_Hit_Bullet_Small`, `VFX_Skill_Fire_Area_Large`
- 이펙트 변경 시 아래 항목을 함께 검토합니다.
  - 가독성(정보 전달)
  - 비용(프레임/메모리)
  - 게임플레이 영향(피격 인지, 위험도 인지)

## 7. 검증 체크리스트
- 피격 여부가 1프레임 단위로 명확히 인지되는가?
- Camera Shake가 조준을 방해하지 않는가?
- 다수 Enemy 동시 전투에서도 VFX가 프레임 급락을 유발하지 않는가?
- VFX 교체/추가 시 Addressables 로딩 정책과 충돌이 없는가?
