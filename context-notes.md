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
- Baggage의 "손 닿는 대상 0.70m"는 컨베이어 가장자리까지 거리다. 화면 정면의 배낭은 벨트 안쪽에 있어
  약 1.9m(위 사진으로 어림, ±0.2m) 떨어져 있고 손이 닿지 않는다.
  이 배낭은 공항 모델의 장식용 물체(Collider·Grab 없음)라 그대로 둔다(이씨 결정).
  → 오브젝트 배치 작업 때 집을 수 있는 가방을 따로 만들어 컨베이어 가장자리, 서는 지점에서 1m 안에 놓는다.

- 조치 후 Validate 경고 0개, _fwd 캡처로 Arrival(비행기)·Baggage(배낭) 정면 확인.
- 2차 점검(Dest Tour, 이씨): 7개 지점 모두 문제없음. checklist ①~③ 완료.

### 다음 세션이 알아야 할 것
- "공항 기본 공간 구성" 업무는 시뮬레이터 기준으로 완료. 남은 것은 Scanner·EXIT 처리 방식 김씨와 협의(이씨).
- Quest 실기기에서 다시 볼 것: CheckIn·Boarding·Immigration·Baggage 거리감, Mesh Collider 21개(삼각형 88,357개) 성능.
- 김씨에게 공유할 것: 템플릿 XR Origin의 Move/Teleport 기능과 Teleport Area Setup(§13 자유 이동 금지와 충돌),
  FadeMoveController.cs 한글 주석 인코딩(EUC-KR).
- 팀 저장소로 옮길 때: feature/lee-airport-layout 브랜치, 공항 모델 .fbx·.png 제외(.meta만), Packages 폴더는 옮기지 않음.
- 다음 이씨 업무 후보: 오브젝트·표지판·이동 포인트 배치(집을 수 있는 가방은 Baggage 컨베이어 가장자리 1m 안).

## 2026-10-02 오브젝트·표지판·이동 포인트 배치 시작

### 결정 사항 (이씨)
- 이동 포인트는 표지판형: 다음 구역 이름이 적힌 안내판을 Point & Hold로 가리키면 이동한다.
  표지판과 이동 포인트를 하나로 합쳐 "어디로 가야 하는지"를 바로 보이게 하기 위해서다.
- 표지판 문구는 한국어만.
- 오브젝트는 실제 크기의 임시 모양 먼저. 에셋은 라이선스 정리 후 교체(공항 모델처럼 재배포 문제 방지).
- 여권은 처음부터 손에 들고 시작한다.
  배치만으로는 안 되고 시작 시 손이 여권을 잡은 상태여야 하므로 김씨(Grab) 영역이다.
  XR Interaction Toolkit 손 Interactor의 Starting Selected Interactable(코드 없이 Inspector 설정)을 김씨에게 제안.
  이씨 작업에서는 여권 임시 물체를 Start 근처에 둔다.

### 주의점
- 표지판은 바닥에 세우지 않고 2.0m 높이에 거는 형태로 한다. 카운터 위로 기둥이 지나가지 않게 하기 위해서다.
- 집는 물건의 임시 Grab은 XRI 기본 XRGrabInteractable만 쓴다(§16.1). 동작 방식 확정은 김씨.

### ① 이동 표지판 7개 (GatePass > Signs > Build Move Signs)
- MoveSigns 아래 MoveSign_<지점이름>. 남색 판 1.2 x 0.45m, 바닥에서 2.0m, 흰 글씨 Noto Sans KR, Box Collider.
  판 Material은 Assets/_GatePassVR/Art/Temp/M_TempSignBoard.mat 하나를 공유한다.
- 위치는 MoveSignBuilder의 표(지점, 문구, 정면 기준 각도, 거리)로 정한다. 표의 위치가 가려지면
  정면 ±40°, 2~6m 안에서 가장 가까운 빈자리를 자동으로 찾는다(판 가운데와 네 모서리가 보이고 다른 Collider와 겹치지 않음).
  Collider가 없는 장식(운항 정보판 등)은 피하지 못하므로 _fwd 캡처로 확인한다.
- 결과: Start 체크인 +30° 4m / CheckIn 보안검색 -40° 2.5m / Security 탑승구 -15° 2m(게이트에 가려 자동 조정) /
  Boarding 탑승하기 +35° 2m(부스에 가려 자동 조정) / Arrival 입국심사 -40° 4m /
  Immigration 수하물 찾는 곳 -40° 3m / Baggage 출구 -30° 4m. Validate 경고 0개, _fwd 사진 7장 모두 읽힘.
- Validate의 표지판 거리 검사에는 0.01m 계산 오차 여유를 둔다(2.0m에 둔 표지판이 1.999m로 계산되는 문제).
- 폰트(NotoSansKR-Medium SDF)는 Dynamic 모드라 Unity가 글자를 쓸 때마다 파일이 바뀐다. 실행 시 다시 만들어지므로 커밋하지 않는다.

### ② 구역 이름 표지판 (GatePass > Signs > Build Zone Signs)
- 이동 표지판과 같은 도구로 합쳤다(MoveSignBuilder → AirportSignBuilder). 종류별로 모양만 다르다.
  구역 표지판: 노란 판 2.0 x 0.6m, 검은 글씨, 높이 3.0m(가려지면 3.5/4.0m), Collider 없음(Point & Hold Ray를 막지 않게).
- 표에 표지판마다 높이를 적는다. Security "보안검색"은 바로 아래 이동 표지판 "탑승구"와 붙어 보여 3.5m로 올렸다.
- 출구 구역 표지판은 뺐다(이씨 결정). Baggage의 이동 표지판 "출구"와 위아래로 겹쳐 같은 말이 두 번 보였고,
  실제 EXIT 위치가 아직 정해지지 않았다. EXIT 처리를 김씨와 정한 뒤 그 자리에 추가한다.
- 표지판 판 Material 2개(M_TempSignBoard, M_TempZoneSignBoard)는 URP Unlit이다.
  조명 때문에 노란색이 겨자색으로 탁해 보여서 바꿨다. 실제 공항의 빛나는 안내판처럼 보이고 계산도 가볍다.
- 결과: CheckIn 체크인 / Security 보안검색(3.5m) / Boarding 탑승구(2m로 자동 조정) /
  Immigration 입국심사(2m로 자동 조정) / Baggage 수하물 찾는 곳. Validate 경고 0개, _fwd 사진으로 구별·겹침 없음 확인.

### Quest 3S에서 보이는지 점검 (A 캡처 / B 검사 / C 배치)
- 공통 기준 SignViewRules.cs: 눈높이 서서 1.6m·앉아서 1.2m, 편하게 보이는 범위 정면 ±30°·위 25°,
  Quest 3S 시야 좌우 96°·위아래 90°(대략). 일반적인 VR 권장값이라 실기기 확인 후 조정한다.
- A: Capture Dest Views가 _q3s(서서), _q3s_seated(앉아서) 사진을 추가로 찍고, 편하게 보이는 범위를 초록 선으로 표시한다.
  기존 캡처(가로 약 120°)는 헤드셋보다 넓어서 화면 끝 표지판이 실제로는 시야 밖일 수 있었다.
- B: Validate가 표지판마다 좌우 각도, 서서·앉아서 올려다보는 각도, 두 눈높이에서의 가림을 검사한다(좌우 0.5° 계산 오차 여유).
- C: 표지판 도구가 위 기준을 만족하는 자리만 고른다. 표의 각도도 ±30° 안으로 맞춘다
  (처음에는 표의 위치를 기준 확인 없이 먼저 통과시키는 버그가 있었다).
  다른 표지판·시설과 좌우 0.2m, 위아래·앞뒤 0.1m 여유를 두고 자리를 찾는다.
- 2층 Boarding·Immigration은 바로 앞이 3.7m 부스라서 구역 표지판 높이 후보에 2.1m(부스 앞면 안내판 높이)를 추가했다.
  앉은 사용자 기준 25° 이하가 되려면 2m 거리에서 2.1m 이하여야 하기 때문이다.
- 결과: Validate 경고 0개. 앉아서 올려다보는 각도 최대 24°. Security 구역 표지판은 +30°, 5m로 옮겨 이동 표지판과 분리됨.
- 남은 점: 2층에서 노란 구역 표지판과 남색 이동 표지판이 화면상 가깝게 붙어 보인다(색은 구별됨). 실기기에서 헷갈리는지 확인.
- 주의: 코드를 여러 파일 나눠 고치는 도중 Unity가 컴파일하면 뒤에 고친 파일이 반영되지 않을 수 있다.
  결과를 볼 때 Library/ScriptAssemblies/Assembly-CSharp-Editor.dll 시각이 마지막 코드 수정보다 뒤인지 확인한다.
- 이동 표지판 문구를 7개 모두 "다음 구역으로 이동"으로 통일했다(이씨 결정). AirportSignBuilder의 NextAreaText 한 곳에서 바꾼다.
  가는 곳은 노란 구역 표지판과 GuideManager 안내(김씨)로 알린다. 표의 각 줄 옆 주석에 가는 곳을 남겼다.
  문구가 길어져 글씨가 작아졌다. 가장 먼 4m 표지판도 Quest 3S 시야 사진에서 읽히지만, 실기기에서 다시 확인한다.
- Start에서는 기존 Btn_NextArea(초록 버튼, 같은 문구)와 새 이동 표지판이 함께 보인다. Point & Hold가 붙으면 Btn_NextArea 정리 필요.
- 기존 초록 버튼 WorldUI_CheckIn/Btn_NextArea를 삭제했다(이씨 결정). 다른 오브젝트의 참조가 없음을 확인했고,
  Play 중 이동 테스트는 Dest Tour 창으로 한다(같은 FadeMoveController 사용).
- Dest_ 오브젝트의 Scene 뷰 이름표(아이콘)는 편집 화면에서만 보이고 게임에는 나오지 않는다. Game 뷰에서는 Gizmos를 꺼서 숨긴다.
  Dest_를 비활성화해서 숨기면 안 된다: GameObject.Find가 꺼진 오브젝트를 찾지 못해 도구가 멈춘다.
  (한 번 꺼졌다가 다시 켰다. Dest Tour 창은 DestinationPoints 아래에서 찾도록 고쳐 꺼져 있어도 동작한다.)

## 2026-11-13 작업 환경 이전, 오브젝트 배치, 이동 표지판 다시 만들기

### 작업 환경
- 프로젝트 위치를 D:\GatePassVR_Lee로 옮겼다(GitHub에서 clone). 공항 모델은 README 방법대로 다시 넣었다.
- Claude Code가 Unity를 직접 조작할 수 있게 MCP for Unity 패키지(com.coplaydev.unity-mcp, 팀 develop과 같은 #main)를 추가했다.
  연결 설정 .mcp.json(UnityMCP, http://127.0.0.1:8080/mcp)은 개인 설정이라 .git/info/exclude로 이 PC에서만 제외했다.
  Unity에서 Window > MCP for Unity > Toggle MCP Window > Start Server를 눌러야 연결된다.
- Unity가 처음 열 때 자동으로 바꾼 파일(OpenXR 설정, Quality URP Config, SampleScene.scenetemplate, 동적 폰트 SDF)은 커밋하지 않았다.

### ③ 오브젝트 배치 (GatePass > Objects > Build Temp Objects)
- AirportObjectBuilder의 표(지점, 이름, 종류, 크기, 지점 기준 오른쪽·앞 거리)로 TempObjects 아래에 만든다. 이름은 <이름>_<지점>.
  - 집는 물건(Obj_): Box Collider + Rigidbody + XRGrabInteractable(XRI 기본, 동작 확정은 김씨).
  - 영역 표시(Zone_): 반투명 초록 Unlit 판 + Trigger Box Collider. Placement·Scanner·Hand-over가 붙을 자리.
  - 여권은 시작 시 손에 드는 방식(김씨)이 생기기 전까지 Start 앞 임시 받침대(Stand_Passport, 0.9m) 위에 둔다.
- 높이는 물건 가운데와 네 모서리 아래 표면 중 가장 높은 곳으로 정한다. 가운데 한 점만 쟀더니 트레이가 X-ray 벨트 롤러 틈(0.76m)에
  박혀 Play 시 19° 기울었다. 고친 뒤 기울기 1° 이내.
- 트레이는 어두운 벨트 위에서 안 보여서 밝은 회색으로 바꿨다. 색별 Material 6개(M_Temp*.mat)를 공유한다.
- 점검 도구에 "오브젝트가 서는 지점에서 손 닿는 범위(수평 1m, 높이 1.3m 이하)에 있는지" 검사를 추가했다.
  결과: 여권 0.60, 캐리어 0.35, 탑승권 0.86, 트레이 0.77, 스캔 자리 0.99, 여권 건네는 자리 0.99, 가방 0.39m.
  - 스캔·여권 건네는 자리: 부스 책상 앞 끝이 지점에서 약 0.98m라 책상 앞 끝에 붙였다.
- 남은 문제: 체크인 수하물 올리는 자리(Zone_BaggageDrop)가 1.79m. 체크인 벨트가 카운터 왼쪽 1.5m부터 시작해 지금 서는 지점에서 손이 안 닿는다.
  방법 1) Dest_CheckIn을 왼쪽으로 약 1m 옮겨 카운터와 벨트 사이에 서게 한다(10/2 1차 피드백으로 정한 위치를 바꿈).
  방법 2) 그대로 두고 XRI 원거리 잡기/놓기와 김씨 Placement Zone 구현에 맡긴다. 이씨 결정 필요.
- Dest Tour 가독성, Device Simulator 임시 Grab은 아직 이씨가 직접 확인하지 않았다(미검증).

### 이동 표지판 다시 만들기 (이씨 확인)
- 10/2 버전의 문제: 7개 모두 "다음 구역으로 이동"이라 어디로 가는지 모름, 문구가 길어 글씨가 작고 동적 폰트로 들쭉날쭉,
  2m 높이에 떠 있어 스티커처럼 보임, 버튼(Point & Hold 목표)처럼 보이지 않음, 위치가 다음 구역 방향과 무관.
- 바꾼 것: 목적지 문구 + 방향 화살표 + "가리키고 잠시 기다리세요", 바닥에 선 안내 기둥(패널 가운데 1.6m), 다음 구역 쪽 ±20° 우선,
  표지판 글자만 담은 고정 폰트(Assets/Fonts/NotoSansKR-Medium SDF Signs.asset). 문구를 바꾸고 도구를 다시 실행하면 새 글자도 추가된다.
- Security·Boarding·Immigration은 바로 앞이 X-ray 기계·심사 부스로 막혀 ±20° 안에 자리가 없어 ±30°까지 넓혀 놓였다(Security +30°, 4m).
- 화살표는 서 있는 지점에서 본 다음 Dest_ 방향(대략)이다. 출구 화살표(↑)는 EXIT 위치가 정해지면 조정한다.
- 김씨에게 공유: 흰 원 가운데 GaugeAnchor에 원형 게이지를, 표지판 루트(Box Collider)에 PointAndHoldTarget을 붙이면 된다.

### 구역 이름 표지판 다시 만들기 (이씨 확인)
- 문제: 노란 판이 이동 표지판보다 크고 밝아 행동 표지판보다 먼저 눈에 띔(위계 역전), 2층 부스에서는 2m 거리·2.1m 높이로
  화면 1/3을 덮음, 허공에 떠 있음, 같은 건물이라 보이는 출국·입국 구역이 구별되지 않음.
- 바꾼 것: 1.6x0.45m로 줄이고 3m 이상·4m 이상 거리, 왼쪽 출국(파랑)/입국(초록) 띠.
  판 위 3m 안에 천장이 있으면 줄 두 개로 매달고(1층 천장은 바닥에서 약 4.5m), 없으면 바닥 기둥 두 개.
- 2층 Boarding·Immigration: 천장이 13m 이상이고 지점 약 2.9m 앞이 벽, 그 앞은 높이 3.7m 부스라 어떤 높이·거리로도
  매달거나 세울 자리가 없었다. AirportSignBuilder.FacilityMountYaw로 부스 앞면에 붙인다(이동 표지판 반대쪽 ±20°).
  높이는 부스까지 거리에서 앉은 사용자 24°가 되게 계산한다(탑승구 2.4m·2.28m, 입국심사 1.8m·2.01m).
- 주의: 부스의 유리 원통 같은 모델 일부는 Collider가 없어서, 처음에 입국심사 판 절반이 원통 뒤에 묻혔다.
  부스 앞면을 잴 때는 모델 전체에 임시 Mesh Collider를 만들고(Analyze와 같은 방식), 판 폭 0.2m 간격·세 높이로 잰다.
- 남은 점: 2층 두 곳은 공간이 좁아 1층보다 크게 보인다. 1층 4.5m 거리의 긴 이름("수하물 찾는 곳")은 실기기에서 읽히는지 확인 필요.

### 체크인 수하물 올리는 자리: 지점 유지 (이씨 결정)
- Zone_BaggageDrop_CheckIn은 서는 지점에서 1.79m라 손이 닿지 않지만 Dest_CheckIn을 옮기지 않는다(10/2 1차 피드백으로 정한 위치 유지).
  대신 XRI 원거리 잡기·놓기(레이로 잡은 채 먼 곳에 놓기)로 처리할 수 있는지 김씨 Grab 설정과 [08] Placement Zone 구현에서 확인한다.
  원거리로 자연스럽지 않으면 Dest_CheckIn을 왼쪽으로 약 1m 옮기는 방법(카운터·벨트 둘 다 1m 안팎)을 다시 검토한다.
- AirportLayoutValidator.AcceptedOutOfReach에 등록해 Validate에서 경고 대신 "참고"로 보고한다. Validate 경고 0개.
