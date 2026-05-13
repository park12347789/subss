# Unity 3D 수업 (Notion) 및 Phase별 Git 브랜치

## Notion 수업 페이지

부모: [Systemic Overload - 전체 구현 로드맵 (MVP)](https://www.notion.so/35537e0b48d8816a85c3c747f658050a)

| Phase | 제목 | URL |
|-------|------|-----|
| Phase 1 | Unity 3D 수업 - Phase 1 (이동·입력·애니메이션) | https://www.notion.so/35537e0b48d881d3a77def3b281a485a |
| Phase 2 | Unity 3D 수업 - Phase 2 (전투·데미지·연동) | https://www.notion.so/35537e0b48d881e08216ec4e08c62210 |

## Git 브랜치 정책 (MVP 로드맵과 정렬)

| 브랜치 | 용도 |
|--------|------|
| `phase/phase-1-movement` | Phase 1 통합 기준선 (이동·카메라·입력·로코모션 Animator) |
| `phase/phase-2-damage-weapon` | Phase 2 통합 기준선 (데미지·무기·히트 스캔·Animator 트리거 연동) |
| `feature/<phase>-<topic>` | 세부 작업 (예: `feature/phase1-orbit-camera`) |

Phase 1 완료 시 `mvp-phase1` 태그, Phase 2 완료 시 `mvp-phase2` 태그 등은 [MVP_Roadmap_Phase_and_Release_Management.md](MVP_Roadmap_Phase_and_Release_Management.md)를 따릅니다.

**로컬 브랜치 생성 예시**

```text
git checkout -b phase/phase-1-movement
# 작업 후 PR → develop

git checkout phase/phase-1-movement
git checkout -b phase/phase-2-damage-weapon
# Phase 2 작업
```

현재 원격에 `Phase1_Movement` 브랜치도 유지됩니다. 로드맵과 동일한 슬래시 형식 통합 브랜치 **`phase/phase-1-movement`**, **`phase/phase-2-damage-weapon`** 을 원격에 생성해 두었습니다(동일 커밋에서 분기, 이후 Phase별로 앞만 진행하면 됩니다).
