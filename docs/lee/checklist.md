# 공항 기본 공간 구성 - Checklist (담당 이씨)

Unity에서 실제로 확인한 항목만 체크한다. 추측으로 체크하지 않는다.

## 준비 (완료)
- [x] SampleScene → Airport_Lee 이름 변경, _GatePassVR/Scenes로 이동
- [x] Git 설정, GitHub(GatePassVR_Lee) Push, 공항 모델 원본 Git 제외
- [x] XR Interaction Toolkit 3.5.1 업그레이드 (팀 develop과 버전 통일), Console 오류 0개 확인

## ① 필수 공항 구역 구분과 기본 동선 배치
- [x] 7개 구역과 Dest_ 지점 순서 확정 (plan.md 표)
- [x] 각 Dest_ 지점이 해당 구역(카운터, 검색대, 게이트 등) 앞에 있는지 확인 (Capture Dest Views 사진)
- [x] 각 Dest_ 지점의 방향(Y 회전)을 상호작용 대상 쪽으로 조정, 다시 캡처해 확인
- [ ] Scanner(Boarding 구역), EXIT(Baggage 구역) 연결 방식 김씨와 협의

## ② 플레이어 이동 폭·충돌·진입 가능 구역 확인
- [x] AirportLayoutValidator 작성 (지점 아래 바닥, 몸 겹침, 손 닿는 범위의 상호작용 대상 검사)
- [x] Compile Error 0개 확인
- [x] 1·2층 바닥 Collider 배치 (Box 대신 07.Modulo1 Mesh Collider, context-notes 참고)
- [x] 체크인 카운터, 보안검색대, 심사 부스, 수하물 컨베이어에 Mesh Collider 배치
- [x] 점검 메뉴(Validate Colliders) 실행 결과 경고 0개

## ③ START~EXIT 동선으로 이동하며 막힘·불필요 공간 점검
- [x] Play 모드에서 7개 지점 순서대로 이동 (Dest Tour Window 사용)
- [x] 각 지점에서 다음 목적지가 보이는지 확인
- [x] 각 지점에서 상호작용 대상에 손이 닿는지 확인 (Baggage 배낭은 장식이라 제외, context-notes 참고)
- [x] Arrival 이후 출국 구역이 보이는지 확인
- [x] 문제를 context-notes.md에 기록하고 수정 후 다시 확인 (1차 피드백 반영, 2차 점검 문제없음)
- [x] 커밋, GitHub Push

---

# 오브젝트·표지판·이동 포인트 배치 - Checklist (담당 이씨)

## 준비
- [x] plan.md, checklist.md, context-notes.md에 업무 추가
- [ ] 김씨에게 공유: 여권 시작 방식, PointAndHoldTarget을 표지판에 붙이는 방식, Scanner·EXIT 처리

## ① 이동 표지판 7개
- [x] 이동 표지판 생성 Editor 도구 작성, Compile Error 0개
- [x] 7개 생성, 점검 도구에 "다음 표지판 보임" 검사 추가
- [x] 검사 경고 0개, _fwd 캡처로 확인
- [x] (11/13) 안내 기둥형으로 다시 만들기: 목적지 문구, 방향 화살표, 조작 안내, 고정 폰트. 지점 7곳 캡처로 이씨 확인

## ② 구역 이름 표지판 (5개, 출구 보류)
- [x] 생성, 캡처로 확인 (5개, 출구는 EXIT 위치 확정 후 추가)
- [x] (11/13) 고정 폰트로 다시 만들기, 이동 표지판과 겹침 없음 캡처로 확인
- [x] (11/13) 작고 높게(1.6x0.45m, 3m 이상), 1층 천장 매달기·2층 부스 앞면 부착, 출국/입국 띠. 지점 5곳 캡처로 이씨 확인

## ③ 오브젝트 배치
- [x] 임시 오브젝트 생성(실제 크기), 집는 물건에 임시 Grab (GatePass > Objects > Build Temp Objects)
- [x] 영역 표시 3개 배치
- [x] Play 모드 약 8초 동안 집는 물건 5개가 떨어지거나 뚫지 않고 제자리에 놓임
- [x] 점검 도구 "오브젝트 1m 안" 검사 경고 0개 — Zone_BaggageDrop_CheckIn(1.79m)은 이씨 결정으로 지점 유지, 검사에서 "참고"로 표시
- [ ] 김씨와 확인: 수하물 올리는 자리를 원거리 잡기·놓기로 쓸 수 있는지 (Grab 설정, [08] Placement Zone)

## ④ 점검
- [x] Dest Tour로 표지판 가독성, 오브젝트 위치 확인 (이씨 확인 11/13)
- [x] Device Simulator로 임시 Grab 확인 (이씨 확인 11/13: 여권·캐리어·탑승권·트레이·가방)
- [x] Validate Colliders 경고 0개 (11/13)
- [x] 커밋, GitHub Push (11/13)

---

# [08] Placement Zone 기본 구조 - Checklist (담당 이씨, 김씨 구조 검토)

## Trello 완료 체크리스트
- [x] Placement Zone 영역과 성공 위치 기준 설정 (물건 가운데가 판 안, 손에서 놓임, 0.3초 정지)
- [x] 대상 오브젝트를 지정 위치에 놓았을 때 판정되도록 연결 (Zone_BaggageDrop_CheckIn ← Obj_Carrier_CheckIn)
- [x] 잘못된 위치·오브젝트에서는 성공 처리되지 않는지 확인 (EditMode 테스트 + Play 모드 시험)
- [ ] Quest에서 반복 배치 테스트 후 성공 처리 확인 (이씨) — 미검증

## 작업
- [x] asmdef 구성 (GatePassVR.Runtime, GatePassVR.Tests.EditMode — 팀 저장소와 같은 이름)
- [x] PlacementZoneState 테스트 먼저 작성 → 구현, EditMode 9개 통과
- [x] PlacementZone 컴포넌트 (onPlaced/onWrongObject, 1회 성공, 성공 시 고정·색 변경, ResetZone)
- [x] AirportObjectBuilder에서 영역에 자동 연결, 집는 물건에 연속 충돌 검사(얇은 탑승권 관통 방지)
- [x] Play 모드 시험: 틀린 물건 신호 1회, 걸쳐 놓기 실패, 가운데 놓기 성공(약 0.33초 뒤), 고정·색 변경, ResetZone 후 다시 집기 가능
- [x] Device Simulator로 직접 잡아서 놓기 (이씨 확인 11/13: 캐리어를 레이로 잡아 초록 판에 놓으면 성공)
- [ ] 김씨 구조 검토 (context-notes 요약 공유)

---

# [20] 안내문·표지 문구 정리 - Checklist (담당 이씨, 김씨 UI 반영)

- [x] 문구 규칙과 용어표 작성 (guide-texts.md)
- [x] START~END 단계별 메인 안내·보조 힌트·음성 대본·틀린 행동 힌트 21줄
- [x] 보안검색 소지품을 노트북·가방으로 확정(이씨 결정), 맡기는 짐은 "캐리어"로 구별
- [x] 글자 수(메인 16자, 힌트 24자) 초과 0개, 금지 용어 0개, 표지판 도구 문구와 일치
- [ ] 김씨와 협의 항목 확정 (S1-0 여권 시작, S2-1 체크인 Hand-over, S8-2 여권 반환, EXIT, Reset 버튼)
- [ ] 김씨 UI 반영 (GuideManager·ScenarioStep), 실제 HUD 한 줄 표시 확인
- [x] 보안검색 노트북·가방 임시 물건과 트레이 Placement Zone 2칸(왼쪽 노트북, 오른쪽 가방). Play 모드 시험 통과, Device Simulator 이씨 확인(11/13)

---

# [22] NPC 배치 및 상호작용 지점 설정 - Checklist (담당 이씨, 김씨 기능 연결)

## Trello 체크리스트
- [x] NPC 역할별 위치와 상호작용 지점 배치 (4명, 체크인 여권 건네는 자리 추가)
- [x] NPC와 필요한 기능 연결 — 이씨 범위: NpcStation으로 지점 연결, 여권 건네는 자리는 Placement Zone 임시 동작. 김씨 Hand-over·Scanner 연결은 남음
- [ ] 애니메이션 없이도 역할과 상호작용 흐름이 이해되는지 플레이 테스트 (이씨) — 미검증

## 작업
- [x] NpcStation 컴포넌트(역할 ID, 이름, 상호작용 지점)
- [x] AirportObjectBuilder에 NPC 종류 추가(몸통·머리·이름표·몸 Collider, 서는 지점을 바라봄)
- [x] NPC 몸 Collider로 표지판 도구가 NPC 뒤를 피함, 이동 표지판·노란 판 다시 만들기, Validate 경고 0개
- [x] Play 모드: 여권을 체크인·입국심사 자리에 차례로 놓아 둘 다 성공, 체크인 뒤에도 여권을 다시 집을 수 있음
- [x] 문구표 S2-1을 "여권을 카운터의 초록 칸에 놓으세요"로 맞춤
- [x] 머리 위 역할 이름표 유지 (이씨 결정 11/13)
- [ ] 김씨 협의: Hand-over·Scanner를 NpcStation 지점에 연결, 입국심사관 여권 반환 방식(S8-2)
- [ ] 실제 사람 에셋 교체(라이선스 확인 후)

---

# [21] 안내 음성·효과음 정리 - Checklist (담당 이씨, 김씨 시스템 연결)

## Trello 완료 기준: 필요 음원 준비 및 재생
- [x] 음성 서비스 결정: Azure Speech `ko-KR-InJoonNeural`, 유료 S0, AI 음성 고지(이씨 결정 11/13)
- [x] 효과음 6개 합성 (`Tools/generate_sfx.py`), Unity Console Error 0개 (24kHz 모노, Vorbis, 11/13)
- [x] 효과음 6개를 이씨가 직접 들어 확인 (11/23, 수정 없음)
- [x] 음성 생성 스크립트 작성 (`Tools/generate_voice.py`), `--dry-run` 22줄 확인
- [x] AI 음성 고지 문구 추가 (guide-texts.md C-AIVOICE, README 출처)
- [x] Azure Speech S0 리소스 생성, 환경 변수 설정 (이씨, Azure for Students 구독, 11/23)
- [x] 음성 22개 생성, 길이 0초 없음, Console Error·Warning 0개 (24kHz 모노, Vorbis, 합계 182.5초, 11/23)
- [x] 이씨 프로젝트 Play 모드에서 재생 확인 (22개 로드, VO_START 재생 진행 확인, 11/23)
- [x] 이씨가 직접 들어 발음·속도 확인 (11/23, 22개 모두 수정 없음)
- [ ] feature/lee-audio → main 병합, GitHub Push
- [ ] 김씨 연결: VO_<ID> → GuideManager.SetGuide, 효과음 재생 위치, 시작 화면 AI 음성 고지

---

# [이전] 팀 저장소 develop으로 옮기기 - Checklist (담당 이씨, 김씨 확인)

## 준비
- [x] 두 저장소 기록이 이어져 있지 않음 확인 (개인 a6d4a9f, 팀 develop bcfb347)
- [x] 팀 저장소 쓰기 권한 확인 (hlee971001-rgb, dry-run 11/20)
- [x] 옮길 파일·옮기지 않을 파일·충돌 지점 정리 (plan.md)
- [ ] 김씨 확인: 안 올린 작업 여부 — 팀 PR #1 설명에서 요청함(11/20)
- [ ] 김씨 확인: FadeMoveController 참조 처리, Editor asmdef, 이씨 문서 위치, XR Device Simulator 샘플 — 팀 PR #1 리뷰에서 확인
- [x] 김씨 확인이 필요한 것 중 씬이 쓰지 않는 Editor 도구·asmdef는 1차에서 빼고 진행하기로 결정 (이씨 11/20). 문서는 docs/lee/, 샘플은 넣기
- [x] PR #1(feature/lee-audio → 개인 main) 처리 결정 — 열어 둔다(이씨 11/20). 원래 계획은 음성까지 만들고 들어 본 뒤 머지
- [x] 1차(콘텐츠)·2차(음원)로 나눠 옮기기로 결정 (이씨 11/20, plan.md)

아래 옮기기·검증·마무리는 1차(콘텐츠)다. 효과음·음성 생성 스크립트는 1차에 넣지 않는다.

## 옮기기
- [x] 팀 저장소 clone(C:\GatePass_VR), develop에서 feature/lee-content 생성
- [x] 공항 모델 README대로 넣기 (Unity 열기 전)
- [x] 옮기는 파일 복사 (.meta 포함)
- [x] .gitignore에 공항 모델·LayoutCaptures 규칙 추가
- [ ] Scripts/Editor에 GatePassVR.Editor asmdef 추가 — 1차에서 뺌, Editor 도구 7개와 같이 따로 PR
- [x] FadeMoveController 참조를 팀 스크립트로 정리 (참조 1곳 GUID 교체)
- [x] Airport_Lee Scene·라이트맵 GUID가 팀 SampleScene과 같아 Scene에 새 GUID, 라이트 다시 Bake

## 검증 (11/20, C:\GatePass_VR, Unity 6000.3.10f1)
- [x] Compile Error 0개
- [x] Airport_Lee Missing Script·Prefab 0개. Missing Reference 34개는 팀 VR 템플릿 프리팹에 원래 있던 것
- [x] EditMode 테스트 전부 통과 (20/20)
- [ ] Validate Colliders 경고 0개 — 미검증 (Editor 도구 필요)
- [ ] Dest Tour 7개 지점 이동 — 미검증 (Editor 도구 필요)
- [x] 공항 모델 원본 미포함, 김씨 Scene·Prefab 변경 없음

## 마무리
- [x] 의미 단위 커밋 8개, 푸시, 팀 develop으로 PR (KJY-1204/GatePass_VR PR #1, base develop)
- [ ] 김씨 리뷰 후 머지, 머지된 develop 다시 열어 재검증
- [ ] Editor 도구 7개 + GatePassVR.Editor asmdef PR, Validate Colliders·Dest Tour 확인

## 2차 (음원, 1차 머지 후) — 개인 먼저, 팀에 복사 (이씨 결정 11/23)
- [x] 1차 팀 PR #1 develop 머지 확인 (a2938a7, 11/23)
- [x] 2차 순서 결정: 개인 저장소에서 음성 생성·청취·푸시 후 팀 저장소에 복사 (이씨 11/23, plan.md)

### 개인 저장소 (C:\GatePassVR_Lee, feature/lee-audio)
- [x] Azure Speech S0 리소스 생성, 환경 변수 설정 (이씨, 키는 커밋하지 않음, 11/23)
- [x] 음성 22개 생성, 길이 0초 없음, Console Error 0개 (11/23)
- [x] Play 모드 재생 확인 (11/23)
- [x] 효과음 6개 이씨 청취 (11/23, 수정 없음)
- [x] 음성 22개 이씨 청취, 발음·속도 확인 (11/23, 수정 없음)
- [x] 의미 단위 커밋, 개인 GitHub 푸시 (feature/lee-audio, 11/23)

### 팀 저장소 (C:\GatePass_VR)
- [x] 머지된 develop(a2938a7)에서 feature/lee-audio 새로 만들기 (11/23)
- [x] 효과음 6개·음성 22개, Tools/generate_sfx.py·generate_voice.py, C-AIVOICE·README 음원 출처 복사 (.meta 포함, 11/23)
- [x] Compile Error 0개, EditMode 테스트 통과(20/20), Airport_Lee Play 모드 재생 확인 (11/23)
- [x] 의미 단위 커밋 4개, 푸시, 팀 develop으로 PR (KJY-1204/GatePass_VR PR #2, base develop, 11/23)
- [ ] 김씨 연결: VO_<ID> → GuideManager.SetGuide, 효과음 재생 위치, 시작 화면 AI 음성 고지
- [ ] 머지된 develop 다시 열어 재검증
- [ ] 개인 PR #1 처리 (기록용으로 둘지, 닫을지 이씨 결정)

---

# [마무리] 김씨 부재 대비, START→EXIT 연결부터 최종 빌드까지 - Checklist (담당 이씨)

## 1. 준비
- [x] 김씨 11/23 작업 검토: 문서와 동작 일치, EditMode 56/56, TestMap_Quest Play 확인 (11/23)
- [x] 팀 클론에 feature/lee-full-flow 생성 (develop b0cddc5 기준, 11/23)
- [x] 개인 저장소를 원격 `lee`로 추가 (11/23)
- [ ] 팀에 마무리 담당 알림 (이씨가 직접 보냄)
- [x] 개인 저장소에 전체 push (원격 lee, feature/lee-full-flow, 11/23). 팀 저장소도 공개라 새로 공개되는 내용 없음, 이씨 결정

## 2. [14] Airport_Lee START→EXIT 연결
- [x] 시스템 Prefab 배치와 씬 참조 연결 (GatePassSystems, GuideHUD, XR Origin을 Dest_Start로, 옛 SceneTransitionManager 제거, 11/23)
- [x] 이동 표지판 7개에 Point & Hold·게이지, 시작 버튼(StartPad)·AI 음성 고지 글씨 (11/23)
- [x] 건네기 칸 2개 → HandOverZone, 탑승구 → ScannerZone(S4-1 여권, S4-2 탑승권) (11/23)
- [x] 체크인 여권 돌려받기: returnPoint + S2-1b 단계·음성 추가 (11/23, S2-3 문구 대신 별도 단계로 바꿈: 한 화면 한 행동)
- [x] 스캔 뒤 여권 챙기기: S4-3 단계·음성 추가 (11/23, Play에서 여권을 두고 가면 S8-1에서 막히는 것 발견)
- [x] ResetController: Step 진입·이동 완료 때 위치를 기억해 그 위치로 되돌림, 다시 시작할 때만 처음 위치 (11/23)
- [x] 집기·비행·미리 놓기 연결 StepSuccessRelay (11/23)
- [x] Step 23개 데이터·음성 연결, 시뮬레이터 Play로 START→END→다시 시작 확인 (11/23, 표지판 Hold는 코드 호출, 실제 Ray 조준은 미확인)
- [ ] 이씨 음성 2개 청취 (VO_S2-1b, VO_S4-3)
- [ ] 실제 Ray로 표지판·시작 버튼 Hold 확인 (시뮬레이터 또는 Quest)

## 3. [13] 힌트·고지·폰트
- [x] 틀린 물건 힌트 문구 연결: StepFeedback이 현재 Step의 "틀린 행동 힌트"를 보조 힌트에 표시 + SFX_Retry (11/23)
- [x] 효과음 연결: 앞으로 진행 SFX_Success, END SFX_Complete, 틀린 행동 SFX_Retry, 스캔 SFX_Scan(김씨 Prefab), 안내방송 Chime(VO_S6-1 안) (11/23)
- [x] 약 10초 무반응이면 보조 힌트에 C-RESET 표시, C-RESET 문구·음성을 B·Y 버튼으로 수정 (11/23)
- [x] 시작 화면 AI 음성 고지 (C-AIVOICE, 2단계에서 StartScreen에 넣음)
- [x] 한글 폰트 Noto Sans KR 교체: GuideHUD Prefab MainGuideText·HintText → NotoSansKR-Medium SDF Dynamic, 맑은 고딕 참조 0 (11/23)
- [ ] 이씨 음성 청취 (VO_C-RESET 새로 만듦)
- [ ] Quest 실기기에서 한글 표시 확인 (5단계. 11/24 첫 화면 캡처에서 안내 HUD 한글 정상, 표지판·시작 화면은 착용 확인 남음)

## 4~7
- [x] [23] 기능 테스트 목록 docs/lee/test-cases.md, 시뮬레이터로 25개 항목 확인 (11/23)
- [x] [24] 시뮬레이터 START→END 완주 3회, 3회차는 표지판 실제 Ray·손으로 놓기 (11/23)
- [x] [26] 막힘 수정: 돌려준 여권 날아감(B5), 원거리 잡기 물건이 칸에 저절로 들어감(B6), 두고 온 서류(B7) (11/23)
- [ ] [24] 조씨·김씨와 공동 완주, Quest 완주
- [x] [25] 빌드 전 F1(Airport_Lee 0번)·F2(com.gatepass.gatepassvr)·F3(시뮬레이터 EditorOnly) (11/24)
- [x] [25] 배치 빌드 스크립트 QuestBuild, APK 빌드 성공, Quest 3S 설치·실행, 예외 0, 첫 화면 캡처 (11/24)
- [ ] [25] Quest 착용 완주: 실제 손잡이·B/Y 버튼, 표지판 Hold, 한글, 프레임, 소리 (이씨가 착용)
- [ ] [25] 시작 직후 "missing script" 경고 2줄(Game Object '<null>', 에셋 쪽) 원인 확인
- [x] [28] 기술 설정·빌드 문서 `docs/build-guide.md` 초안 (11/23, 현재 설정값을 Unity에서 읽어 작성)
- [x] [29] 에셋·라이선스·테스트 결과 `docs/lee/assets-and-tests.md` (11/23)
- [ ] [28] 다른 팀원이 문서만 보고 다시 빌드 확인, Quest 빌드 후 빠진 절차 채우기
- [x] 크레딧: END 화면 시작 버튼 오른쪽에 작게 (이씨 결정 11/23)
- [x] 시작 화면 글씨를 안내 HUD(눈높이 띠) 아래로 내림 (11/23, 캡처에서 "시작"이 HUD에 가려짐)
- [x] [29] PolyOne Free VR Hands: 출처 확인 못 해 제거, 공용 Rig를 XRI 기본 컨트롤러 모델로 되돌림 (11/24, 김씨에게 알림은 이씨가)
- [x] 기본 컨트롤러 모델 Quest 착용 확인: 컨트롤러만 보이고 손 없음 (11/24 이씨 확인)
- [x] 기본 컨트롤러 모델 상태에서 시작 버튼 Hold·여권 잡기·표지판 Hold 이동 정상 (11/24 이씨 Quest 확인)
- [ ] [30] 최종 빌드와 백업

# 컨트롤러를 쥔 손 모양 (XR Hands 기본 손) - Checklist (담당 이씨, 11/24)
- [x] 후보 조사: 무료·출처 표기 불필요 → XR Hands 샘플 손 선택 (PolyOne 출처는 Sketchfab CC BY 4.0으로 확인)
- [x] ControllerHandPose 컴포넌트와 EditMode 테스트 2개
- [x] 손 프리팹 2개(Prefabs/VR) 만들고 공용 Rig의 Left/Right Controller에 붙이기, UniversalController 끄기
- [x] 에디터 렌더 캡처(4방향)로 자세·위치 조정, 양손 대칭 확인
- [x] 컴파일 오류 0, EditMode 58/58, APK 빌드·설치
- [x] Quest 착용: 쥔 손이 보임 (11/24 이씨 확인, adb 캡처에는 맨손 손만 찍혀 직접 보지는 못함)
- [ ] 쥔 손 상태에서 잡기·Hold 다시 확인, 엄지·크기 어색함 여부
- [ ] 김씨에게 공용 프리팹 변경 알림 (이씨)
