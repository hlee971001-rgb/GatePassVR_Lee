# 공항 기본 공간 구성 - Context Notes

작업 중 정한 내용과 이유를 계속 추가한다. 다음 세션은 이 파일부터 읽는다.

## 2026-10-02

### 작업 환경
- 프로젝트: C:\GatePassVR_Lee (이씨 개인 작업용, 원래 이름 Test_1)
- GitHub: https://github.com/hlee971001-rgb/GatePassVR_Lee (Public)
- 팀 저장소: https://github.com/KJY-1204/GatePass_VR (작업 완료 후 feature/lee-airport-layout 브랜치로 옮길 예정)
- Unity 6000.3.10f1. XR 패키지는 팀 develop 브랜치와 같은 버전(XRI 3.5.1 등).
  팀과 다르게 둔 것: unity-mcp 미설치(개발 도구), Rider·Timeline 유지.

### 결정 사항
- 이동 방식은 CLAUDE.md §13 Point & Hold → Fade → 순간이동으로 확정(이씨 확인).
  공간은 걷는 길이 아니라 서는 지점(Dest_) 기준으로 점검한다.
- 구역은 기존 Dest_ 지점 7개로 한다(이씨 결정). §17의 Scanner, EXIT는 별도 지점 없이
  Scanner는 Boarding 구역, EXIT는 Baggage 구역 안에서 처리한다(이씨 확인).
- SampleScene을 Airport_Lee로 바꿔 Assets/_GatePassVR/Scenes로 옮겼다.
  이유: 팀 저장소의 SampleScene과 이름이 겹치지 않게 하고, §15 폴더 구조에 맞추기 위해.
- 공항 모델은 Sketchfab Free Standard 라이선스(2.2: 원본 파일 재배포 금지)라서
  .fbx와 텍스처 .png는 Git에서 제외하고 .meta만 올린다. 팀 저장소로 옮길 때도 같다.
- 공항 모델 Import 설정에서 Collider 자동 생성이 꺼져 있다(addColliders: 0).
  모델 전체 Mesh Collider 대신 필요한 곳에만 Box Collider를 둔다. 이유: Quest 성능(§24).
- 이 저장소는 core.autocrlf false로 설정했다. Unity 파일의 줄바꿈이 자동 변환되지 않게 하기 위해.

### 주의점
- 팀원은 Unity를 열기 전에 공항 모델을 넣어야 한다. 먼저 열면 .meta가 지워진다.
- FadeMoveController.cs는 김씨 담당 영역이라 수정하지 않는다.
  이 파일은 한글 주석이 UTF-8이 아닌 인코딩(EUC-KR)으로 저장되어 있어 일부 편집기에서 깨져 보인다. 김씨에게 공유 필요.
- FadeMoveController는 Dest_의 위치와 Y 회전으로 XR Origin을 옮긴다.
  Dest_ 지점 7개의 Y 회전이 모두 0°라서 지점마다 방향 조정이 필요하다.
- Dest_Boarding, Dest_Immigration은 높이 5.5m(2층)다.
- 출국 지점(Start, CheckIn, Security)과 입국 지점(Immigration, Baggage)이 서로 가깝다.
  도착 후 출국 구역이 보이면 헷갈릴 수 있으니 ③에서 확인한다.
- Console의 노란 경고 5개(오디오 드라이버, 시선 추적, 가상 컨트롤러 진동)는
  VR 기기 없이 PC에서 실행할 때 나오는 정상 메시지다.

### Dest_ 지점 조정 (GatePass > Capture Dest Views 사진으로 확인)
- 원래 7개 지점 모두 Y 회전 0°였고, 상호작용 대상에서 1.4~2.7m 떨어져 있었다.
- 조정 결과 (DestinationPoints 기준 Local 값)
  | 지점 | Position | Y 회전 | 앞에 있는 것 |
  |---|---|---|---|
  | Dest_Start | (11.29, 0.43, -0.64) | 90 | 터미널 안쪽, Btn_NextArea |
  | Dest_CheckIn | (37.6, 0.43, -17.95) | 90 | 체크인 카운터, 약 0.55m |
  | Dest_Security | (24.3, 0.43, -5.39) | 90 | X-ray 벨트 입구, 약 0.55m |
  | Dest_Boarding | (40.9, 5.5, -2.54) | 90 | 2층 심사 부스, 약 0.7m |
  | Dest_Arrival | (58.32, 0, -44.7) | 340 | 비행기 옆, 터미널 방향 |
  | Dest_Immigration | (40.9, 5.5, 4.06) | 90 | 2층 심사 부스, 약 0.7m |
  | Dest_Baggage | (25.3, 0.43, 7.8) | 66 | 수하물 컨베이어, 약 1m |
- CheckIn, Security는 기준(0.6~0.8m)보다 조금 가깝지만 카운터와 겹치지 않아 유지. VR 점검(③)에서 다시 판단.
- Boarding과 Immigration은 같은 심사 부스 줄을 쓴다(남북 6.6m 차이).
- Boarding 구역에는 탑승구 문이 따로 없다. 부스에서 여권·탑승권 Scanner 후 탑승하는 흐름으로 쓴다.
- Baggage에서 남쪽을 보면 체크인 카운터와 보안검색대가 보인다(건물 하나라 출국·입국 공간이 같음).
  표지판 작업 때 "입국" 안내를 눈에 띄게 해서 보완한다.
- Capture Dest Views는 동서남북 4방향만 찍는다. Arrival(340°), Baggage(66°)는 가까운 방향 사진으로 확인했다.

### ② 충돌 설정 (Collider)
- 점검 도구: GatePass > Layout > Validate Colliders (실제 Collider로 검사),
  Analyze Model (Temporary Colliders) (모델 전체에 임시 Collider를 만들어 측정, Scene 변경 없음).
  결과는 LayoutCaptures/LayoutReport_*.txt (Git 제외).
- 측정 결과 1층 바닥 0.43, 2층 바닥 5.66. Dest_ Local Y를 1층 0.46, 2층 5.69로 맞췄다(부모 Y -0.03).
- Collider 방식은 Box 대신 Mesh Collider로 결정(이씨 확인). 이유: 카운터 모양이 복잡해 Box로는
  윗면 높이를 맞추기 어렵다. 필요한 시설에만 붙인다(GatePass > Layout > Add Model Colliders).
  대상: 07.Modulo1(1·2층 바닥·벽), 08.CheckInDesk, 09.SecurityCheck, 11.BoardingAreaDesk ×5, BaggageClaimBand ×13
  → 21개, 삼각형 88,357개(모델 전체 230만 개의 약 4%). Quest 성능 점검 때 다시 확인할 항목.
- Arrival 바닥은 템플릿 Environment/Grid의 Box Collider다.
- 손 닿는 범위 검사는 정면 일직선이 아니라 정면 ±30°, 높이 0.5~1.3m, 1m 안으로 한다.
  이유: 수하물 컨베이어처럼 휘어 있거나 윗면이 얇은 대상은 일직선 검사에 걸리지 않는다.
- Dest_Baggage는 컨베이어까지 1.15m라서 0.45m 당기고 방향을 61°로 바꿨다 → Local (25.69, 0.46, 8.02).
- Validate 결과 경고 0개. 손 닿는 대상까지 거리: CheckIn 0.57, Security 0.55, Boarding 0.83,
  Immigration 0.84, Baggage 0.63m.
- 템플릿 XR Origin에 이동(Move/Teleport) 기능과 Teleport Area Setup이 남아 있다.
  CLAUDE.md §13(자유 이동 금지)과 맞지 않지만 김씨 담당(XR/이동)이라 건드리지 않았다. 김씨에게 공유 필요.

### ③ 동선 점검 (Play 모드, XR Device Simulator)
- 점검 도구: GatePass > Layout > Dest Tour Window (Play 중 7개 지점을 FadeMoveController로 순서대로 이동).
- 1차 점검 피드백(이씨)과 조치
  - CheckIn, Boarding, Immigration: 카운터·부스가 너무 가깝다 → 뒤로 물림.
    CheckIn X 37.6 → 37.4 (약 0.77m), Boarding·Immigration X 40.9 → 40.75 (약 0.98m).
    Boarding·Immigration은 손 닿는 거리 기준 1m를 유지하려고 0.2m가 아니라 0.15m만 물렸다(이씨 결정).
  - Arrival: 비행기를 바라보게 → 방향 340° → 90° (탑승 계단·동체 방향).
  - Baggage: 컨베이어 위 가방이 Boarding처럼 정면에 보이게 → 위치 (25.64, 0.46, 8.17), 방향 103°.
    배낭 위치는 위에서 찍은 사진으로 계산한 값이라 _fwd 캡처로 확인 필요.
- 주의: 거리감은 모니터의 시뮬레이터 화면 기준이다. 헤드셋에서는 다르게 느껴질 수 있으므로
  Quest 실기기 테스트 때 CheckIn·Boarding·Immigration·Baggage 거리를 다시 확인한다.
- Capture Dest Views에 지점이 실제로 바라보는 방향 사진(_fwd)을 추가했다.

### 다음 세션이 알아야 할 것
- checklist ③ 진행 중. 위 조치 후 Validate와 _fwd 캡처로 확인, 이어서 Dest Tour로 2차 점검.
