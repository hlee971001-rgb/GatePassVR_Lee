# GatePass VR 에셋·테스트 결과 정리 (업무 [29], 담당 이씨)

- 작성: 2026-11-23 (이씨). 기준 브랜치 `feature/lee-full-flow`.
- 라이선스는 각 출처 페이지·파일에 적힌 내용으로 정리했다. "확인 필요"는 원문을 아직 보지 않은 항목이다.

## 1. 에셋 목록과 라이선스

| 에셋 | 위치 | 출처 | 라이선스 | 저장소 포함 | 표기 |
|---|---|---|---|---|---|
| 공항 모델 "Airport Final Big Scene" | `Assets/Environments/source/Airport7_FINALc_EscenaMontada.fbx`, `textures/Texture_airport256b.png` | assetfactory, Sketchfab | Sketchfab Free Standard | **아니오** (원본 재배포 금지, `.meta`만 포함. 받는 방법은 README) | 크레딧에 표기 |
| Noto Sans KR Medium | `Assets/Fonts/NotoSansKR-Medium.ttf`, TMP 글꼴 2개(Signs 정적, Dynamic 동적) | Google Fonts | SIL Open Font License 1.1 | 예 | 크레딧에 표기(OFL은 글꼴 파일과 함께 라이선스 고지 권장) |
| 안내 음성 24개 `VO_*.wav` | `Assets/_GatePassVR/Audio/Voice/` | Microsoft Azure Speech `ko-KR-InJoonNeural`, `Tools/generate_voice.py`로 생성 | 유료 S0 등급 출력물(Microsoft Product Terms). AI 합성 음성 고지 필요(Microsoft AI 서비스 행동강령) | 예 | 시작 화면 "안내 음성은 AI로 합성한 음성입니다"(C-AIVOICE), 크레딧 |
| 효과음 6개 `SFX_*.wav` | `Assets/_GatePassVR/Audio/SFX/` | `Tools/generate_sfx.py`로 직접 합성 | 팀 제작 | 예 | 없음 |
| VR 템플릿 에셋 | `Assets/VRTemplateAssets/` (Torus Cut 모델, Inter 글꼴 등) | Unity VR Template | Unity Companion License (Inter 글꼴은 OFL) | 예 | 없음 |
| XRI 샘플 | `Assets/Samples/XR Interaction Toolkit/3.5.1/` (Starter Assets, XR Device Simulator) | Unity XR Interaction Toolkit | Unity Companion License | 예 | 없음 |
| TextMesh Pro 기본 리소스 | `Assets/TextMesh Pro/` (LiberationSans) | Unity | Unity Companion License, LiberationSans는 OFL | 예 | 없음 |
| VR 손 모델 "Free VR Hands" | `Assets/PolyOne/Free VR Hands/` | PolyOne (김씨가 9/4 추가) | **확인 필요** (Unity Asset Store 무료 에셋으로 보임, 표준 EULA면 프로젝트 안 사용 가능) | 예 | 확인 후 결정 |
| 임시 물건·NPC·표지판 | `Airport_Lee`의 TempObjects, MoveSigns, ZoneSigns | 이씨가 기본 도형·TMP로 제작 | 팀 제작 | 예 | 없음 |

### 크레딧에 넣을 문구 (안)
```
공항 모델: "Airport Final Big Scene" by assetfactory (Sketchfab)
글꼴: Noto Sans KR (SIL Open Font License 1.1)
안내 음성: Microsoft Azure AI Speech로 합성한 AI 음성입니다
```
(2026-11-23 이씨 결정) END 화면에 작게 넣었다. Airport_Lee `StartScreen/CreditsText`, 시작 버튼 오른쪽, END Step에서만 보인다(`ShowDuringSteps`). 위 세 줄에 "효과음: GatePass VR 팀 제작"을 더했다. 팀원 이름은 넣지 않았다.

### 남은 확인
- [ ] PolyOne Free VR Hands 라이선스 원문(김씨가 받은 곳)
- [x] 크레딧 위치: END 화면 (11/23)
- [ ] Sketchfab Free Standard의 표기 의무 여부 원문 확인(지금은 표기하는 쪽으로 정리)
- [ ] NPC를 실제 사람 모델로 바꿀지(바꾸면 라이선스 다시 정리)

## 2. 테스트 결과

### 자동 테스트 (EditMode, 2026-11-23)
56개 모두 통과. Compile Error 0.

| 테스트 파일 | 대상 |
|---|---|
| `GuideReguideTimerTests` | 5초·10초 재안내 타이머 |
| `HoldGaugeStateTests` | Point & Hold 게이지 |
| `PlacementZoneStateTests` | Placement Zone 판정 |
| `HandOverZoneStateTests` | 건네기·반환·돌려받기 |
| `ScannerZoneStateTests` | 스캔 순서·머무름 시간 |
| `HighlightStateTests` | 목표 강조 단계 |
| `ScenarioProgressTests` | Step 진행 |

### 기능·완주 테스트
- 항목별 결과는 `docs/lee/test-cases.md` (기능 25개, 완주 3회, 발견 문제 B1~B7).
- 요약: XR Device Simulator로 START → END → 다시 시작 완주 3회. 3회차는 표지판·시작 버튼을 실제 Ray로 가리키고
  물건은 손을 칸 위로 옮겨 놓았다. 발견한 막힘 7개는 모두 고쳤다.
- 화면 확인: 표지판 게이지 방향, 안내 HUD 한글 글꼴(Noto Sans KR).

### 아직 안 한 테스트
| 테스트 | 필요한 것 | 업무 |
|---|---|---|
| Quest 실기기 완주, 손잡이 버튼·B/Y 버튼 | Quest 3S | [25] |
| Quest에서 한글 표시, 프레임 저하 | Quest 3S | [25] |
| 음성·효과음 크기와 겹침 청취 | 이씨 | [21] |
| 팀 공동 완주 | 김씨·조씨 | [24] |
| 이 문서와 `docs/build-guide.md`만 보고 다시 빌드 | 다른 팀원 | [28] |
