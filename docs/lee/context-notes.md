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

## 2026-11-13 [08] Placement Zone 기본 구조

### 결정 사항
- 팀 저장소(KJY-1204/GatePass_VR develop)의 김씨 코드 방식을 따른다: 순수 상태 클래스(HoldGaugeState) + MonoBehaviour(PointAndHoldTarget),
  namespace GatePassVR.Interaction, [SerializeField] private + UnityEvent<자기 자신>, Reset 메서드.
- 성공 후 물건은 고정한다(이씨 결정). 실제로도 벨트에 올린 가방은 가져가고, 다시 집으면 "성공 취소"가 복잡해진다. ResetZone()으로 풀린다.
- 보안검색 트레이는 이번에 적용하지 않는다(무엇을 넣을지 미정, 이씨 결정).

### 구현 메모
- 판정은 Trigger 이벤트 대신 매 프레임 판 위 상자(판 아래 5cm ~ 판 위 0.7m) OverlapBox로 집는 물건을 찾는다.
  판이 표면에서 약간 떠 있어서, 처음에는 표면에 누운 탑승권(두께 2mm)이 상자 아래로 빠져 "틀린 물건"이 감지되지 않았다 → 판 아래 5cm까지 확장.
- 떨어뜨린 탑승권이 벨트 Mesh Collider를 뚫고 바닥으로 빠졌다(얇은 물건 + 기본 충돌 검사) → 집는 물건 Rigidbody를 ContinuousDynamic으로.
- 이 프로젝트는 FadeMoveController가 Assets/Scripts(Assembly-CSharp)에 있고 Editor 도구가 그것을 참조해서,
  GatePassVR.Runtime asmdef를 Scripts 전체가 아니라 Scripts/Interaction에만 두었다.

### 팀 저장소로 옮길 때 주의
- 팀 저장소는 Assets/_GatePassVR/Scripts 전체가 GatePassVR.Runtime asmdef다. 이씨 Editor 도구 6~7개를 Scripts/Editor에 그대로 넣으면
  런타임 asmdef에 UnityEditor 코드가 들어가 빌드가 깨진다 → Scripts/Editor에 Editor 전용 asmdef(GatePassVR.Editor, includePlatforms: Editor,
  GatePassVR.Runtime 참조)를 만들어야 한다. DestTourWindow는 팀 저장소의 FadeMoveController(GatePassVR.VR, Runtime asmdef)를 참조하게 된다.
- Scripts/Interaction/GatePassVR.Runtime.asmdef(이 프로젝트 전용)는 옮기지 않는다. PlacementZone*.cs와 Tests만 옮긴다.

### 김씨 구조 검토용 요약
- 연결 방법: ScenarioManager가 PlacementZone.OnPlaced에 리스너를 달고, 현재 Step의 zoneId와 같을 때만 성공 처리.
  Step Reset 때 PlacementZone.ResetZone() 호출(물건 위치 복구는 ResetController 담당).
- onWrongObject는 GuideManager 힌트(예: "캐리어를 올려 주세요")에 쓸 수 있다.
- 원거리 잡기(레이로 잡은 채 놓기)로도 동작하는지는 김씨 Grab 설정에 달려 있다(Zone_BaggageDrop_CheckIn은 서는 지점에서 1.79m).

## 2026-11-13 [20] 안내문·표지 문구 정리

- 문구 원본은 guide-texts.md. 김씨 GuideManager.SetGuide(main, hint, voice) 형식에 맞춰 메인 안내·보조 힌트·음성 대본을 단계별로 쓴다.
- 결정(이씨): 보안검색에서 트레이에 넣는 소지품은 노트북과 가방. 처음 제안은 휴대폰·작은 가방, 다음 결정은 노트북·액체류였다가 노트북·가방으로 확정.
  노트북은 가방에서 꺼내 따로 넣는다는 교육 내용을 음성에 담는다.
- "가방"(들고 타는 가방)과 "캐리어"(맡기는 짐)를 구별한다. 수하물 찾는 곳에서 찾는 것은 체크인 때 맡긴 캐리어로 본다
  (씬의 Obj_BaggageBag_Baggage는 이름 정리 후보).
- 영어 조작 용어(홀드, 그랩)를 쓰지 않고 "가리키고 잠시 기다리세요", "손잡이 버튼"으로 쓴다. 이동 표지판 아래 문구와 같은 뜻.
- 길이 기준(메인 16자, 힌트 24자)은 VR에서 한 줄로 읽히게 하려는 이씨 기준이다. 김씨 GuideHUD가 이 프로젝트에 없어서
  실제 한 줄 표시 여부는 확인하지 못했다(UI 반영 때 김씨 확인).
- 후속: 노트북·가방 임시 물건과 트레이 Placement Zone이 씬에 없다. 체크인 여권 Hand-over 자리도 없다(김씨 협의).

### 보안검색 노트북·가방과 트레이 Placement Zone
- 트레이(Obj_SecurityTray)를 집는 물건에서 고정 물건으로 바꿨다. 트레이를 옮기는 단계는 문구표에 없다.
- 트레이를 벨트 가로로(0.6 x 0.4m) 두고 왼쪽 칸 Zone_TrayLaptop(노트북), 오른쪽 칸 Zone_TrayBag(가방) 두 PlacementZone을 만들었다.
  한 영역에 두 물건을 받게 하면 PlacementZone이 첫 성공에서 끝나고, 겹친 두 영역은 서로를 "틀린 물건"으로 보고 판이 겹쳐 깜빡인다.
  칸을 나누면 기존 컴포넌트를 그대로 쓸 수 있다. 세로로 두면 뒤쪽 칸이 서는 지점에서 1m를 넘어 가로로 했다.
- 노트북은 벨트 입구 오른쪽 빈 곳(0.81m), 가방은 플레이어 오른쪽 바닥. 처음엔 오른쪽에 받침대를 세웠는데 이동 표지판을 가려서 뺐다.
- 공항 모델의 장식 배낭(17.8.Model.002, Collider 없음)이 트레이 위에 겹쳐 "가방"과 헷갈려서 Scene에서만 껐다
  (AirportObjectBuilder.HiddenDecorations, 모델 원본은 그대로).
- Play 모드 시험: 가방을 노트북 칸에 놓으면 노트북 칸 onWrongObject 1회, 각자 칸에 놓으면 두 칸 모두 성공·고정.
- 문구표 S3-1·S3-2 보조 힌트를 "트레이의 왼쪽/오른쪽 초록 칸에 놓으세요"로 바꿨다.

## 2026-11-13 [22] NPC 배치 및 상호작용 지점

- 공항 모델에 직원으로 쓸 사람 모델이 없다(사람 크기 물체 17.4.Model은 건물 끝 자판기류). 임시 사람 모양(캡슐+구)으로 두고
  실제 에셋은 라이선스 확인 뒤 교체한다. AirportObjectBuilder의 표(Kind.Npc)와 NpcRoles로 만든다.
- NpcStation(Scripts/Interaction): roleId, displayName, interactionPoint. 김씨가 Hand-over·Scanner를 연결할 때 이 지점을 쓴다.
  체크인 직원 → Zone_HandOver_CheckIn, 탑승구 직원 → Zone_Scan_Boarding, 입국심사관 → Zone_HandOver_Immigration, 보안검색 요원은 지점 없음.
- 여권 건네는 자리 2곳은 Placement Zone(받는 물건 Obj_Passport_Start, lockOnPlaced=false)으로 임시 동작. 건넨 여권을 돌려받아
  다음 단계(스캔, 입국심사)에 또 쓰기 때문에 고정하지 않는다. AttachPlacementZones는 다른 지점 물건을 접미사까지 쓴 이름으로 찾는다.
- 처음엔 NPC에 Collider가 없어서 표지판 도구가 NPC를 못 보고, 입국심사관이 '수하물 찾는 곳' 이동 표지판 바로 앞에 섰다.
  NPC에 몸 Capsule Collider를 넣고 이동 표지판·노란 판을 다시 만들었다(입국심사 이동 표지판 -15° → +20°).
  체크인 직원도 -0.2m에 서면 '보안검색' 이동 표지판 아래 모서리를 가려 0m로 옮겼다.
  → 순서: Build Temp Objects → Build Move Signs → Build Zone Signs → Validate.
- 2층 부스 노란 판은 이동 표지판이 실제로 놓인 쪽의 반대편에 붙도록 바꿨다(FacilityMountYaw는 각도 크기만 사용).
- 체크인 카운터 위 모니터가 오른쪽 앞(0~+0.5m, 앞 1.1~1.35m)을 차지해서 여권 건네는 자리를 -0.2m, 탑승권을 -0.45m로 옮겼다.
- 역할 이름표(머리 위 "체크인 직원" 등)는 유지한다(이씨 결정, 캡처 확인 후). 초보자가 누구에게 무엇을 하는지 바로 알 수 있다.
  → 11/25 바뀜: Quest에서 본 뒤 이씨 요청으로 글자와 판을 모두 지웠다(아래 11/25 항목).
- 김씨에게 공유: NPC Collider는 XRI 레이에 걸린다. 이동 표지판이 NPC 뒤에 있지 않게 배치했지만, 레이 마스크에서 NPC를 뺄지 김씨가 판단.

## 2026-11-13 [21] 안내 음성·효과음

- 음성은 Azure Speech `ko-KR-InJoonNeural`(성인 남성, 이씨 결정). 이 PC의 Windows TTS는 Heami(여성)뿐이라 쓰지 않았다.
- 이용 조건 확인(11/13): Microsoft Product Terms의 TTS 출력물 사용권은 "paid tier TTS Service only"로 인용됨 → S0로 만든다.
  Microsoft AI 서비스 행동강령 3항은 합성 음성임을 알리라고 요구 → 시작 화면·크레딧에 고지(C-AIVOICE). Microsoft 출처 표기 의무는 찾지 못함.
  Google Cloud TTS는 생성형 AI 약관의 18세 미만 대상 서비스 제한 가능성 때문에 제외. Amazon Polly(Hyunduk)는 고지 의무가 명시되지 않은 대안.
- Unity MCP의 generate_audio는 fal.ai 유료 키가 필요하고 설정되지 않아 쓰지 않았다. 효과음은 스크립트로 직접 합성.
- 파일 이름을 guide-texts.md ID와 같게(VO_S2-1.wav) 해서 김씨가 Step마다 바로 연결하게 했다. 문구를 고치면 guide-texts.md → generate_voice.py 다시 실행.
- GuideManager는 10초 무반응 때 같은 클립을 다시 재생하므로, 기내 안내방송 차임은 VO_S6-1 안에 붙였다(다시 재생될 때도 차임이 같이 나옴).

## 2026-11-20 팀 저장소 develop으로 옮기기 계획

- 팀 저장소 KJY-1204/GatePass_VR의 CLAUDE.md·plan·checklist·context-notes를 다시 읽고 이씨 작업 기준으로 삼는다(이씨 지시).
- 두 저장소는 Git 기록이 이어져 있지 않다. 개인 저장소 첫 커밋 a6d4a9f(10/2)는 팀 커밋을 포함하지 않는다.
  그래서 브랜치째 머지·직접 푸시는 불가(강제 푸시는 김씨 기록을 지움). 팀 develop에서 새 브랜치를 만들어 이씨 파일만 옮긴다.
- 팀 develop 마지막 커밋은 bcfb347(2026-09-11). 그 뒤 김씨가 안 올린 작업이 있을 수 있어 옮기기 전에 확인한다.
- hlee971001-rgb 계정은 팀 저장소 쓰기 권한이 있다(git push --dry-run으로 확인, 실제로 올린 것 없음).
- Airport_Lee Scene이 참조하는데 팀 저장소에 없는 파일 5개를 찾았다. 공항 모델 .fbx.meta, NotoSansKR-Medium SDF Signs,
  XRI 3.5.1 XR Device Simulator 샘플 prefab, Assets/Scripts/FadeMoveController.cs, VRTemplateAssets Torus Cut.fbx.
  같은 경로인데 GUID가 다른 파일 검사는 시간이 오래 걸려 끝내지 못했다. 옮긴 뒤 Unity Missing 검사로 확인한다.
- 개인 FadeMoveController.cs는 팀 것과 같은 클래스(GatePassVR.VR.FadeMoveController)지만 GUID가 다르다.
  옮기지 않고 참조만 팀 스크립트로 바꿔야 한다.
- 11/20 Unity 자동 변경 5개(OpenXR 설정 2개, Quality URP Config, SampleScene.scenetemplate, 동적 폰트 SDF)를 이씨 확인 후
  별도 커밋(26a7ced)으로 feature/lee-audio에 올렸다. 11/13에는 같은 종류를 커밋하지 않기로 했었다. 팀 저장소로는 옮기지 않는다.
- PR #1(feature/lee-audio → 개인 main)을 11/20에 만들었다. [21] 결정은 "음성까지 만들고 들어 본 뒤 main에 합친다"라서
  음성 22개 생성·청취 전에는 머지하지 않는다.

## 2026-11-20 팀 저장소 이전을 1차·2차로 나눔

- 이씨 결정: 안내 음성·효과음 정리가 끝나기 전에, 지금까지 한 콘텐츠 작업을 먼저 팀 develop에 올린다.
  1차는 개인 main에 있는 콘텐츠, 2차는 효과음·안내 음성이다.
- 이유: 콘텐츠는 음성 없이도 동작한다. develop에 늦게 합칠수록 김씨 작업과 충돌이 커지고, 11월은 안정화 시기라 빨리 같이 검증하는 게 낫다.
  PR이 작아야 김씨가 검토하기 쉽다.
- 처음 표에 있던 효과음 6개, Tools/generate_sfx.py·generate_voice.py, guide-texts.md C-AIVOICE, README 음원 출처는
  지금 개인 feature/lee-audio에만 있어서 2차로 옮겼다. guide-texts.md와 README 공항 모델 안내는 main 버전으로 1차에 넣는다.
- 음성 22개는 1차 머지 후 팀 저장소 폴더에서 develop 기준 feature/lee-audio를 새로 만들어 생성한다.
  개인 저장소에서 만들고 다시 옮기면 같은 일을 두 번 하게 된다.
- 개인 커밋 31개는 팀으로 옮기지 않는다. 파일의 최종 상태만 복사해 새 커밋으로 올린다. 기록은 개인 저장소에 남는다.
- 개인 PR #1은 열어 둔다(이씨 결정). 2차 이후 기록용으로 둘지 닫을지는 이씨가 정한다.

## 2026-11-20 1차 이전 실행 (팀 저장소 feature/lee-content)

- 이씨 결정: 김씨 확인이 필요한 것 중 씬이 쓰지 않는 Editor 도구 7개와 Editor asmdef는 1차에서 빼고 나중에 같이 올린다.
  씬이 쓰는 FadeMoveController와 XR Device Simulator 샘플은 뺄 수 없어서 아래처럼 처리했다.
- FadeMoveController: 개인 Assets/Scripts/FadeMoveController.cs는 옮기지 않았다. 팀 Scripts/VR/FadeMoveController.cs와 코드가 같고
  주석 인코딩만 달라서 직렬화 필드도 같다. Airport_Lee의 스크립트 참조 1곳을 팀 GUID(99613e98…)로 바꿨다. 김씨 코드는 고치지 않았다.
- XR Device Simulator 샘플(XRI 3.5.1): 이씨 결정으로 샘플 폴더를 그대로 넣었다. 팀 XRI도 3.5.1이고 XR 설정 파일은 바꾸지 않았다.
- Airport_Lee는 SampleScene을 복사해 만든 Scene이라 Scene·라이트맵·LightingData의 GUID가 팀 SampleScene과 같았다.
  그대로 넣으면 Unity가 한쪽 GUID를 바꿔 연결이 깨질 수 있어서, Scene .meta에 새 GUID를 주고 라이트맵 폴더(Airport_Lee/)는 옮기지 않았다.
  LightingData.asset은 바이너리라 안의 GUID를 고칠 수 없다. Scene의 m_LightingDataAsset을 비웠으니 팀 프로젝트에서 라이트를 다시 Bake해야 한다.
- 이씨 문서는 docs/lee/에 둔다(이씨 결정). 공항 모델 받는 방법은 docs/lee/README.md. 팀 README·루트 문서는 바꾸지 않았다.
- 팀에 이미 있는 _GatePassVR/Art/Temp.meta는 팀 것을 그대로 두었다.
- 커밋 기록은 옮기지 않았다. 개인 main(681e665)의 파일 최종 상태를 복사했다.

### 1차 검증 (2026-11-20, C:\GatePass_VR, Unity 6000.3.10f1)

- Compile Error 0개, Console Error 0개. 경고는 공항 모델 FBX의 self-intersecting 폴리곤 경고(개인 프로젝트와 같음)와
  OpenXRPackageSettings 임포트 경고뿐.
- Airport_Lee: GameObject 1,045개, Missing Script 0, Missing Prefab 0. FadeMoveController 1, PlacementZone 5, NpcStation 4, XR Device Simulator 연결됨.
- Missing Reference 34개는 전부 팀 VR 템플릿 프리팹 6개(Complete XR Origin Set Up Hands Variant, Tutorial Player, Spatial Panel Scroll,
  List Item Slider/Dropdown/Toggle) 안에 원래 있는 것이다. 프리팹 자체를 열어 세어도 같은 34개라서 이전 때문에 생긴 문제가 아니다.
- EditMode 테스트 20개 통과(김씨 GuideReguideTimer 5, HoldGaugeState 6, PlacementZoneState 9).
- 라이트를 다시 Bake했다(ProgressiveGPU). 새 라이트맵·LightingData의 GUID는 팀 파일과 겹치지 않는다.
  Scene 저장 때 템플릿 UI 프리팹 인스턴스의 RectTransform 값(앵커·크기·위치) 21곳이 레이아웃 계산값으로 바뀌었다.
- 미검증: Validate Colliders, Dest Tour 7개 지점 이동(둘 다 1차에서 뺀 Editor 도구가 필요). Play 모드 동선 확인, Quest 3S 실기기.
- Unity가 열리면서 팀 설정 파일 5개(OpenXRPackageSettings, ProjectSettings, ShaderGraphSettings, 패키지 설정 2개)를 자동으로 바꿨다.
  1차에 넣지 않았다. C:\Users\User-29\GatePass_VR는 예전 팀 사본(main 12c2b3b, 6000.3.23f1)이라 쓰지 않는다.
- 팀 PR: KJY-1204/GatePass_VR PR #1 (feature/lee-content → develop, 커밋 8개, 파일 184개, 삭제 0). 김씨 리뷰·머지 대기.
- 앞으로 팀 작업은 C:\GatePass_VR에서 한다. 개인 저장소 문서는 11/20 기준으로 팀 docs/lee와 맞춰 두었다.

## 2026-11-23 2차(음원) 순서 바꿈: 개인 먼저, 팀에 복사

- 확인: 1차 팀 PR #1(feature/lee-content → develop)이 머지됐다(팀 develop 머지 커밋 a2938a7).
- 결정(이씨): 음성 22개를 개인 저장소 C:\GatePassVR_Lee의 feature/lee-audio에서 먼저 만들고, 들어 본 뒤 개인 GitHub에 푸시한다.
  그다음 팀 저장소 C:\GatePass_VR에서 develop 기준 feature/lee-audio를 새로 만들어 파일을 .meta와 함께 복사하고 팀 develop으로 PR한다.
- 이유: 개인 저장소에 완성본 백업이 남고, 확인을 마친 결과만 팀에 올릴 수 있다. 음원 파일은 다른 파일을 참조하지 않아서 복사가 단순하고,
  .meta를 같이 옮기면 GUID가 유지된다.
- 11/20 계획(팀 폴더에서 바로 생성)과 다른 점: 복사 단계가 하나 늘고, 팀 Scene에서의 재생 확인은 복사 후 다시 해야 한다.
- 두 저장소는 Git 기록이 이어져 있지 않아서 개인 브랜치를 팀에 그대로 푸시·병합할 수 없다. 1차와 같이 파일만 복사해 새 커밋으로 올린다.
- 11/23 기준 AZURE_SPEECH_KEY·AZURE_SPEECH_REGION 사용자 환경 변수는 아직 설정되지 않았다.

## 2026-11-23 안내 음성 22개 생성 (개인 저장소 feature/lee-audio)

- Azure Speech S0(Azure for Students 구독, koreacentral), `ko-KR-InJoonNeural`로 `Tools/generate_voice.py`를 실행해 VO_*.wav 22개를 만들었다.
  스크립트는 고치지 않았다. 환경 변수는 사용자 범위에 있고, 실행할 때 그 값을 프로세스에 읽어 넣었다. 키는 출력·커밋하지 않았다.
- 결과: 22개 모두 24kHz 모노 16bit, 0초나 무음 파일 없음, 합계 182.5초. 가장 긴 것은 VO_S6-1(차임 포함 14.25초), 가장 짧은 것은 VO_S4-2(3.83초).
- Unity(GatePassVR_Lee, 6000.3.10f1) 가져오기: 효과음과 같은 기본 설정(Vorbis, Decompress On Load). Console Error·Warning 0개.
- Play 모드: 22개 모두 로드됨, 임시 AudioSource로 VO_START를 재생해 재생 시간이 진행되는 것을 확인했다. Play 모드를 끄면서 임시 오브젝트는 사라졌다.
- 이씨 청취(11/23): 22개 모두 수정 없음. 나중에 고칠 줄은 `python Tools/generate_voice.py --only <ID>`로 그 줄만 다시 만든다.

## 2026-11-23 2차 이전 (팀 저장소 feature/lee-audio)

- C:\GatePass_VR에서 develop(a2938a7, 1차 머지 포함)을 받아 feature/lee-audio를 만들었다.
- 복사: Audio/SFX·Voice와 각 폴더 .meta(GUID 유지), Tools/generate_sfx.py·generate_voice.py, docs/lee의 plan·checklist·context-notes·guide-texts(개인 최신본).
  docs/lee/README.md는 팀용 머리말을 두고 음원 출처 두 줄만 더했다. Audio.meta와 .gitkeep은 팀 것을 그대로 두었다.
- 팀 저장소는 문구표가 docs/lee/guide-texts.md에 있어서 generate_voice.py가 docs/lee/와 루트를 차례로 찾게 고쳤다(개인 dd80a39). 양쪽에서 --dry-run 22줄 확인.
- 검증(C:\GatePass_VR, Unity 6000.3.10f1): Console Error 0개(경고는 공항 모델 FBX 경고뿐), 효과음 6개·음성 22개 Import(모노, 길이 0초 없음),
  EditMode 테스트 20/20 통과, Airport_Lee Play 모드에서 28개 모두 로드되고 VO_START 재생 진행 확인. Scene 파일은 바뀌지 않았다.
- Unity가 자동으로 바꾼 팀 설정 파일 6개(OpenXRPackageSettings, MalgunGothic SDF, ProjectSettings 등)는 이번에도 커밋하지 않았다.
- 팀 PR: KJY-1204/GatePass_VR PR #2 (feature/lee-audio → develop, 커밋 4개, 파일 65개). 남은 일: 김씨 리뷰·머지, 김씨 연결(VO_<ID> → GuideManager.SetGuide, 효과음 재생 위치, 시작 화면 AI 음성 고지), 머지 후 재검증.

## 2026-11-23 — 김씨 부재 대비 마무리 시작

- 김씨 11/23 작업(ResetController, ScenarioManager, Hand-over, Scanner, Highlight, 공통 Prefab)을 검토했다.
  김씨 plan·checklist·context-notes 내용과 코드가 일치하고 EditMode 56/56 통과, TestMap_Quest Play에서
  건네기 → 2초 뒤 반환, 틀린 물건 스캔 무시, 여권 스캔 → 다음 Step, 강조 대상 이동을 확인했다.
- 공항 적용 전에 고칠 문제 2개:
  1. HandOverZone에 returnPoint가 없으면 물건이 kinematic·Grab 비활성으로 계속 고정된다. Play에서 재현했고
     고정된 물건은 ScannerZone에서도 인식되지 않았다(`item.enabled`가 false인 물건은 건너뜀). 김씨 사용법의
     "체크인처럼 돌려주지 않으면 returnPoint 비우기"를 그대로 따르면 S4-1·S8-1에서 여권을 쓸 수 없다.
  2. ResetController.ResetCurrent는 등록된 물건을 전부 Awake 때 위치로 돌린다. 테스트 맵은 좁아 문제없지만
     공항에서는 여권이 출발 지점으로 돌아가 손이 닿지 않는다. 김씨 체크리스트에도 "Step 단위 등록으로 바꿔야
     한다"고 남아 있다.
- 결정(이씨): 김씨 코드는 다시 만들지 않고 이어서 쓴다. 팀 클론 feature/lee-full-flow에서 작업해 개인
  저장소(원격 `lee`)에 먼저 push하고, 나중에 같은 브랜치를 팀에 PR한다(이유는 plan.md [마무리]).
- Trello(11/23 확인): [10] Scanner는 진행중 0/4, [13] Highlight는 미착수로 남아 있지만 코드로는 구현됐다.
  보드에 "완료 11월 27일" 목록이 있어 최종 마감일일 수 있다(확인 필요).
- 주의: 팀 클론에서 씬을 전환할 때 Unity가 Airport_Lee의 Canvas 자동 계산 값(m_SizeDelta 등) 2줄을 저장했다.
  변경 의도가 없어 git checkout으로 되돌렸다. 씬을 바꾸기 전에 저장 여부를 확인할 것.

## 2026-11-23 — [14] Airport_Lee START→EXIT 연결 (김씨 부재 대비)

### 배치와 연결
- GatePassSystems·GuideHUD Prefab을 씬에 넣었다. 옛 SceneTransitionManager(FadeMoveController)는 지우고
  GatePassSystems의 FadeController를 쓴다. 옛 Canvas/FadeImage는 남아 있다(사용 안 함, 정리 후보).
- XR Origin 시작 위치를 Dest_Start로 옮겼다(김씨 점검 주의 3).
- 이동 표지판 7개(MoveSign_Dest_*)에 XRSimpleInteractable + PointAndHoldTarget(targetId Sign_<지점>, 1.2초)을 붙이고
  GaugeAnchor에 PointHoldGauge를 넣었다. 표지판이 곧 Pad다.
- 시작 버튼: StartScreen 아래 StartPad(PointHoldPad, 0.5배, 세움, targetId Pad_Start) + "시작" + C-AIVOICE 한 줄.
  표지판 글꼴(NotoSansKR-Medium SDF Signs)은 표지판 글자만 든 정적 글꼴이라 글자가 깨졌다. 같은 ttf로
  동적 글꼴 `Assets/Fonts/NotoSansKR-Medium SDF Dynamic.asset`을 만들어 썼다. 3단계(GuideHUD 폰트 교체)에도 쓸 수 있다.
- 건네기: Zone_HandOver_CheckIn/Immigration을 HandOverZone Prefab으로 바꿨다(이름 그대로, NpcStation
  interactionPoint 다시 연결). ReceivePoint는 직원 쪽 0.35m, ReturnPoint는 칸 옆 0.32m(칸 판정 상자 밖, 탑승권
  반대쪽). 둘 다 아래에 책상 Collider가 있다.
- 스캐너: Zone_Scan_Boarding을 ScannerZone Prefab으로 바꿨다. 순서 여권(S4-1) → 탑승권(S4-2), scanId = Step ID.
- PlacementZone 3개(캐리어 벨트, 트레이 왼쪽·오른쪽) onPlaced → ScenarioManager.ReportPlaced.
- 성공 ID 규칙: 표지판 Sign_<지점>, 시작 Pad_Start, 집기 Grab_Passport/Grab_BoardingPass/Grab_Carrier,
  건네기 zoneId, 돌려받기 zoneId_Returned, 스캔 S4-1/S4-2, 비행 S6-1.

### 새 스크립트·김씨 코드 수정
- `StepSuccessRelay`(이씨, Scripts/Core): 집기 성공 보고, 비행(S6-1) 16초 뒤 자동 진행(음성 14.3초),
  Step에 들어왔을 때 이미 손에 들고 있거나 이미 놓인 경우 한 프레임 뒤 따라잡기. 따라잡기가 없으면 가방을
  노트북보다 먼저 놓았을 때 S3-2에서 막힌다(Play로 확인, 따라잡기 후 S3-1→S3-3 진행).
- `ResetController` 수정(김씨 부재로 이씨 수정): `SaveCheckpoint()`·`ResetAll()` 추가, ResetCurrent는 기억한
  위치로 돌린다. 연결: onStepEntered·FadeController.onMoveCompleted → SaveCheckpoint, onScenarioCompleted →
  ResetAll. SaveCheckpoint를 부르지 않는 씬(TestMap_Quest)은 예전과 똑같이 시작 위치로 돌린다.
- 손에 든 물건은 Reset 때 놓아진다(김씨 동작 유지). 그 자리에서 떨어지므로 다시 집어야 한다.

### 시나리오 결정
- S2-1b(여권 돌려받기)와 S4-3(스캔 뒤 여권 챙기기)를 추가했다. 문구는 guide-texts.md, 음성은 generate_voice.py.
- S9-2(출구) 성공 시 Dest_Start로 이동하고 END를 보여 준다(EXIT 위치 협의 전 임시). END 성공 ID도 Pad_Start라서
  시작 버튼을 다시 가리키면 onScenarioCompleted → ResetAll → 첫 Step(START)부터 다시 한다.
  Ray를 계속 대고 있으면 START도 바로 이어서 완료되어 S1-0(여권 집기)부터 시작된다.
- S6-1(비행)은 탑승구에 선 채로 안내방송을 듣고 16초 뒤 Dest_Arrival로 이동한다(비행기 공간 없음).

### 검증 (시뮬레이터 Play, 물건 이동·집기는 코드, 표지판 Hold는 ReportHoldCompleted 호출)
- START → S1-0 → … → S9-2 → END → 다시 시작까지 진행. 안내 문구·음성이 Step마다 바뀌었다.
- 여권을 든 채 이동하면 같이 이동, 체크인 건네기 → 2초 뒤 ReturnPoint 반환 → 집으면 S2-2.
- 맵 밖으로 떨어뜨린 노트북을 Reset하면 보안검색 자리로 돌아왔다. 캐리어(벨트에 놓인 것)는 그대로.
- 탑승권을 먼저 스캔하면 진행 안 됨, 여권 → 탑승권 순서로 진행, S4-3에서 여권을 집어야 S5-1.
- 다시 시작하면 여권·캐리어·수하물 캐리어가 시작 위치, 스캐너 차례 0.
- EditMode 56/56, Compile Error 0, Missing Script·Reference 0(GatePassVR 컴포넌트).
- **주의**: 시뮬레이터 기본 컨트롤러 방향이 Dest_Boarding의 탑승하기 표지판을 향해 있어 S5-1이 저절로 Hold됐다.
  실제 기기에서는 1.2초 동안 계속 가리켜야 하지만, 정면 표지판이 실수로 눌리는지 실기기에서 볼 것.
- **미검증**: 실제 Ray 조준 Hold, 게이지 표시 방향, Quest 실기기, 이씨 음성 청취(VO_S2-1b, VO_S4-3).

## 2026-11-23 — [13] 틀린 행동 힌트·효과음·Reset 안내·한글 폰트

- `StepFeedback`(이씨, Scripts/Guidance, Airport_Lee의 StepFeedback 오브젝트):
  - onStepEntered에서 Step 번호가 늘었을 때만 SFX_Success, END에 들어가면 SFX_Complete. Reset 재진입·처음부터
    다시는 소리 없음. 스캔 Step은 스캐너 SFX_Scan과 성공음이 같이 난다.
  - PlacementZone·HandOverZone onWrongObject, ScannerZone onWrongItem → `ShowWrongHint()`. 현재 Step ID로
    guide-texts.md "틀린 행동 힌트"(7개)를 찾아 보조 힌트만 바꾸고 SFX_Retry(1초에 한 번까지). 힌트가 없는 Step의
    틀린 행동은 무시한다. 다음 Step이나 Reset에서 원래 힌트로 돌아온다.
  - GuideManager.onNoProgressLong(10초) → `ShowResetHint()`로 보조 힌트에 C-RESET. 음성은 김씨 동작대로 Step 음성을
    다시 재생한다(C-RESET 음성은 쓰지 않음).
- `GuideManager.SetHintText()` 추가(김씨 부재로 이씨 수정). SetGuide로 힌트를 바꾸면 음성이 지워져 10초 재안내가 사라져서
  힌트만 바꾸는 메서드가 필요했다.
- C-RESET: "메뉴 버튼" → "B나 Y 버튼"(김씨 ResetController 기본 입력). VO_C-RESET 다시 생성.
- GuideHUD Prefab 글꼴을 맑은 고딕(Windows 전용 DynamicOS) → NotoSansKR-Medium SDF Dynamic(ttf 포함, OFL)으로 바꿨다.
  김씨 context-notes의 "최우선 후속 조치 — 한글 폰트 교체"다. TestMap_Quest도 같은 Prefab이라 함께 바뀐다.
- **주의**: 동적 글꼴은 Play 중 쓰인 글자를 아틀라스에 추가해서 `.asset`이 바뀐다. 그 변경은 커밋하지 않고 되돌린다
  (빌드 때 동적 데이터는 비워짐).
- 검증(시뮬레이터 Play): S3-1에서 가방을 노트북 칸에 놓으면 힌트 "노트북을 왼쪽 초록 칸에 넣어 주세요", 10초 뒤
  "막히면 B나 Y 버튼으로 다시 하세요", Reset 후 원래 힌트, 진행 때 효과음 재생, END까지 글자 깨짐 경고 0.
  EditMode 56/56, Compile Error 0. **미검증**: 소리 크기·겹침을 귀로 들어 보기, Quest 한글 표시.

## 2026-11-23 — [23]·[24] 시뮬레이터 테스트와 [26] 막힘 수정

- 테스트 항목·결과는 `docs/lee/test-cases.md`에 둔다(기능 25개, 완주 3회, 발견 문제 B1~B7).
- 시뮬레이터 조준 방법: XRDeviceSimulator의 `m_RightControllerState.deviceRotation`(XR Origin 기준)과
  `m_RightControllerEuler`를 함께 바꾼다. 손 위치는 `devicePosition`(CameraFloorOffsetObject 기준). 잡기는
  `XRInteractionManager.SelectEnter`. 김씨 context-notes의 방법과 같다.
- B5 원인(중요): XRGrabInteractable은 놓을 때 Drop()에서 kinematic을 되돌리고, 던지기 속도는 다음 Late 단계 Detach()에서
  넣는다. HandOverZone이 받은 물건을 `enabled = false`로 두면 그 Detach가 미뤄졌다가 돌려줄 때(다시 켤 때) 실행되어
  물건이 날아갔다. 이동 직후에 건네면 속도 기록에 순간이동이 들어 있어 매우 크다. 건넬 때와 Reset 때 그 한 프레임만
  `throwOnDetach`를 끄고 다음 프레임에 되돌린다(HandOverZone·ResetController, 김씨 부재로 이씨 수정).
- B6: 공항 물건 6개 XRGrabInteractable `farAttachMode = Near`. 원거리로 잡아도 손으로 온다. NearFarInteractor 기본은 Far라
  여권이 몸 앞 1.2m에 떠 있다가 이동 직후 체크인 칸에 저절로 들어가 건네졌다(S2-1 음성이 바로 끊김).
- B7: `CarryAlong`(이씨, Scripts/Core, StepSuccessRelay 오브젝트). FadeController.onMoveCompleted에서
  RearmHoldTargets → BringFarItems → SaveCheckpoint 순서. 손에 없고, 고정·건넨 상태가 아니고, 4m 넘게 떨어진
  여권·탑승권을 플레이어 오른쪽 발옆(0.35m, 0.25m 간격)으로 옮긴다. 보안검색은 바로 앞 0.6m 안에 벨트가 있어 앞이 아니라
  옆에 둔다(7개 지점 모두 오른쪽 0.75m 안에 막힘 없음 확인).
- 같이 확인: 표지판 게이지는 아이콘 둘레에 초록 원으로 차고 플레이어 쪽을 향한다(캡처). 안내 HUD 글꼴 정상.
  머리에 붙은 HUD가 뒤 표지판과 겹쳐 읽기 어려운 장면이 있다(김씨 UI, 실기기에서 판단).
- 동적 글꼴 `.asset`은 Play할 때마다 바뀐다. 이번에도 되돌렸다. "Importer generated inconsistent result" 경고가 한 번
  났는데 되돌린 뒤 다시 가져오면 사라진다.

## 2026-11-23 — [28] 빌드 문서·[29] 에셋·테스트 정리

- `docs/build-guide.md`(팀 루트 docs, 원래 김씨 업무)와 `docs/lee/assets-and-tests.md`를 만들었다. 설정값은 Unity에서
  PlayerSettings·XR 설정·Build Profile을 직접 읽었다.
- 최종 빌드 전 발견(build-guide 5번 F1~F5): Build Profile에 Airport_Lee가 없고 SampleScene이 0번, Package Name이 템플릿 기본값
  `com.DefaultCompany.VRTemplate`, Airport_Lee 안에 XR Device Simulator 오브젝트가 있다. 5단계(Quest 빌드) 때 정한다.
- PolyOne Free VR Hands(김씨 9/4 추가)는 라이선스 원문을 아직 못 봤다.

## 2026-11-23 — END 화면 크레딧

- 이씨 결정: 크레딧은 END 화면에 작게. `StartScreen/CreditsText`(Noto 동적 글꼴 0.7, `<mark>`로 어두운 띠)를 시작 버튼 오른쪽
  (Dest_Start 앞 1.2m, 오른쪽 1.7m, 높이 1.3m)에 두고 `ShowDuringSteps`(이씨, Scripts/Guidance)가 END Step에서만 켠다.
  내용: 공항 모델(assetfactory, Sketchfab), Noto Sans KR(OFL), Azure AI 합성 음성, 효과음 팀 제작. 팀원 이름은 넣지 않음.
- 안내 HUD는 눈 앞 1.3m에 높이 1.58~2.05m 띠로 떠 있어서, 그 뒤 같은 방향의 "시작" 글씨가 가려졌다. 시작 화면을 내렸다:
  StartPad 1.20m, "시작" 1.49m, AI 음성 고지 0.85m(바닥 0.43 기준 위치 값).
- **주의(편집 코드)**: 부모가 일반 Transform인 TextMeshPro(RectTransform)는 코드에서 `transform.position`을 바꿔도 저장할 때
  `m_AnchoredPosition`이 따라가지 않아 위치가 되돌아갔다. SerializedObject로 `m_AnchoredPosition`(x,y)과 `m_LocalPosition`을
  직접 넣어야 저장된다. 처음 만든 CreditsText가 (0, 0, z)로 저장된 원인이다.

## 2026-11-24 — [25] Quest 빌드·설치 (5단계)

- 기기: Quest 3S가 이 PC에 USB로 연결되어 있다(Unity 내장 adb로 확인). 이씨 PC에서 바로 설치할 수 있다.
- 빌드 전 수정: F1 Build Profile 0번 Airport_Lee(SampleScene·TestMap_Quest 끔), F2 Package `com.gatepass.gatepassvr`·
  Company `GatePass`(이씨 결정), F3 XR Device Simulator 프리팹 인스턴스에 `EditorOnly` 태그. 태그는 빌드에서만 빠지므로
  에디터 Play 조작은 그대로다. 오브젝트를 끄거나 지우는 것보다 바뀌는 것이 적다.
- `QuestBuild.BuildAndroid`(Scripts/Editor)와 Editor 전용 asmdef `GatePassVR.Editor`를 추가했다. Scripts 폴더 전체가
  `GatePassVR.Runtime` asmdef에 속해 있어서 Editor 폴더 이름만으로는 런타임에서 빠지지 않는다. Editor asmdef가 꼭 필요하다.
- 결과: 배치 빌드 성공(처음 약 15분), APK 129MB, `adb install -r` 성공. 실행하면 OpenXR 세션 FOCUSED, Scene 로드, 예외 0.
  `adb shell screencap`으로 헤드셋 화면(양눈)을 캡처할 수 있다. START 화면에서 공항 모델·안내 HUD 한글·화살표 정상.
- 로그 관찰: 시작 직후(Scene 로드 전) `The referenced script ... is missing!` 경고 2줄, Game Object가 `<null>`이라 Scene
  오브젝트가 아니라 에셋(Preloaded XR 설정 등) 쪽으로 보인다. 진행에는 영향 없음, 원인 미확인. `xrDiscoverSpacesMETA` 실패는
  Meta OpenXR이 지원 안 되는 확장을 찾는 로그로 무시해도 된다.
- **주의**: 배치 빌드는 Library를 Android로 바꾼다. 다음에 Unity를 열면 Android 플랫폼으로 열린다. 빌드·테스트 뒤 Unity가
  바꾸는 설정 파일(URP 3개, GraphicsSettings, UnityConnectSettings, preloadedAssets, OpenXRPackageSettings, 동적 글꼴)은 되돌렸다.
- **미검증**: 착용 상태의 완주(손잡이·B/Y 버튼, 표지판 Hold, 시작 화면·표지판 한글, 프레임, 소리 크기).

## 2026-11-24 — PolyOne 손 제거, 기본 컨트롤러 모델로 되돌림 (김씨 부재로 이씨 수정)

- 이유(이씨 결정): 김씨가 9/4 넣은 PolyOne "Free VR Hands"는 출처·라이선스 원문을 확인하지 못했다. Asset Store 에셋이라면
  게임 안 사용은 되지만 원본 파일을 공개 저장소에 올리는 것은 보통 허용되지 않는다. 손 모델은 기획 요구사항이 아니다.
- 방법: 공용 Rig `Complete XR Origin Set Up Hands Variant.prefab`을 처음 커밋 `73f4c76`(템플릿 원본)으로 되돌린 뒤,
  손과 무관한 `51c8997`의 "Affordance Callouts Left/Right 끄기"만 다시 적용했다. 그 사이 커밋(64aa4a9~cbff6e8)은 모두 손 관련
  (손 배치, 방향, 손목 캡, updateWhenOffscreen, 손가락 굽힘)이다. 결과: UniversalController(XRI Starter Assets 컨트롤러 모델)가
  다시 보이고 버튼 말풍선은 꺼진 상태.
- 함께 지움: `Assets/PolyOne` 전체, `Scripts/VR/FingerCurlAnimator.cs`·`HandGripInputDriver.cs`(이 프리팹에서만 쓰였고 테스트 없음,
  git 기록에서 되살릴 수 있다).
- 그대로 둠: TestMap_Quest의 PolyOne 머티리얼 덮어쓰기 2줄. 김씨 노트(9/11)의 "지워진 손목 캡 잔재"로 이미 없는 대상을 가리켜
  아무 역할이 없다. 김씨 Scene이라 손대지 않았다.
- 검증: 배치 컴파일 오류 0, EditMode 56/56, 로그에 Missing 경고 없음. APK 다시 빌드·설치, Unity 예외 0.
  Quest 3S 착용: 이씨가 "컨트롤러 모양만 보이고 손은 안 보임"을 직접 확인(11/24). adb 캡처는 헤드셋 재부팅·벗음으로 실패.
- 시작 직후 "missing script" 경고 2줄은 손을 지운 뒤에도 그대로라 손 모델과는 무관하다.
- **미검증**: 기본 컨트롤러 모델 상태에서 잡기·표지판 Hold를 다시 해 보지는 않았다(손 모델은 보이는 것만 바꾸는 부분이라
  상호작용 영향은 없을 것으로 보지만 확인 전).
- (11/24 추가) 기본 컨트롤러 모델 상태에서 시작 버튼 Hold, 여권 잡기, 표지판 Hold 이동이 정상(이씨 Quest 확인).
- 시작 직후 잠깐 보이는 손은 PolyOne이 아니라 XRI 샘플 `XR Origin Hands (XR Rig)`의 핸드 트래킹 손(`Left/RightHandQuestVisual`,
  Unity Companion License)이다. `XRInputModalityManager`가 컨트롤러를 쥐지 않으면 맨손, 쥐면 컨트롤러로 바꾼다. 템플릿 기본 동작이라 그대로 둔다.
  맨손으로는 손잡이·B/Y 버튼을 못 쓰므로 컨트롤러 전용으로 하려면 OpenXR Hand Tracking을 끄는 방법이 있다(결정 안 함).

## 2026-11-24 — 컨트롤러를 쥔 손 모양 (XR Hands 기본 손, 김씨 부재로 이씨가 공용 Rig 수정)

- 후보 조사: 무료·출처 표기 불필요 조건으로 XR Hands 샘플 손(Unity Package Distribution License, 출처 표기 조항 없음)을 골랐다.
  CC0 후보(BlendSwap 22269·2429, itch PSX FPS Hands)는 변환이 필요하거나 화풍이 맞지 않았다. PolyOne 손 출처는 Sketchfab
  "Free Pack - VR Hands (Rigged)" CC BY 4.0(출처 표기 필요)으로 확인했다.
- 모델 `Samples/XR Hands/1.8.1/HandVisualizer/Models/Left·RightHand.fbx` + 머티리얼 `Unity_Hand_Medium`(XRI Hands Demo).
  Quest 맨손 핸드 트래킹 손과 같은 조합이라 쥘 때와 놓을 때 모습이 같다.
- 뼈 규칙: 모델 +Z 손가락, +Y 손등, -Y 손바닥, 관절 로컬 X로 굽히면 손바닥 쪽으로 굽는다. 왼손은 X 대칭(L_ 접두사).
- `ControllerHandPose`(Scripts/VR): Awake에서 한 번만 굽힘(검지 55/55/30, 나머지 90/95/55, 엄지 20/25, 엄지 뿌리 -30° 회전).
  Inspector에서 조정. 테스트 `ControllerHandPoseTests`(손가락만 굽힘, 두 번 불러도 한 번).
- 프리팹 `Prefabs/VR/ControllerHand_Left·Right`: 루트(위치 0) 아래 손 모델에 위치·회전을 넣었다. 공용 Rig의 `Camera Offset/Left·Right
  Controller` 바로 아래에 붙이고 `Left·Right Controller Visual/UniversalController`를 껐다. `Right Controller Visual`은 X 스케일 -1·
  Y 180°라 그 아래에 두면 좌우가 뒤집힌다(김씨 9/11 문제의 원인).
- 컨트롤러 기준점은 링(앞 위)이고 손잡이는 뒤(-Z)로 약 20° 내려간다. 손바닥 뼈를 (±0.032, -0.032, -0.085)에, 손가락이 아래로,
  손등이 바깥, 검지 쪽이 앞을 향하게 놓았다. 위치·각도는 임시 배치 스크립트로 프리팹을 만들고 4방향 렌더(배치 모드에서
  -nographics 없이 Camera.Render)로 확인하며 맞췄다. 임시 스크립트는 지웠다(만든 값은 프리팹에 남아 있다).
- 맨손일 때는 기존 핸드 트래킹 손이 보인다(XRInputModalityManager, 바꾸지 않음).
- 검증: 컴파일 오류 0, EditMode 58/58, APK 빌드·설치, Quest 3S에서 이씨가 "쥔 손이 보인다" 확인. adb 캡처에는 컨트롤러를
  놓은 순간의 맨손 손만 찍혀 쥔 손을 화면으로 보지는 못했다.
- **미검증**: 쥔 손 상태에서 잡기·Hold 재확인(보이는 것만 바뀐 변경), 엄지 위치·손 크기의 어색함.
- **주의**: 빌드 때 Unity가 `Unity_Hand_Medium.mat`의 투명 키워드를 다시 계산해 저장한다. 다른 빌드 부산물처럼 되돌린다.

## 시작 화면 정리 (11/24, 이씨 의견 → Claude 추천 → 이씨 승인)
- 이씨가 말한 "안내 음성 문구"는 AI 음성 고지(`AiVoiceNotice`, "안내 음성은 AI로 합성한 음성입니다")였다. 바닥 쪽(시작 지점 바닥+0.4m)에 흐린 글씨라 안 보였다. 지금은 버튼 바로 아래, 남색 바탕 흰 글씨.
- 시작 지점 바닥은 y=0.43, 버튼은 예전 y=1.2(바닥+0.77m, 허리 아래)라서 HUD에 가려졌다. 버튼을 바닥+1.25m, 제목 카드를 바닥+1.77m로 올렸다. 플레이어와 버튼 거리는 1.3m.
- 시작 버튼: 공용 `PointHoldPad` 프리팹은 그대로 두고 씬의 Pad_Start만 바꿨다. 원판 MeshRenderer 끔(콜라이더는 그대로라 누르는 판정 동일), `StartButtonFace` 캔버스(빛·파란 원·흰 트랙·"시작")를 붙이고 게이지를 트랙 크기로 키움. 패드가 옆으로 90도 눕혀 있어 캔버스는 월드 회전으로 플레이어를 향하게 맞췄다(로컬 회전으로 맞추면 글자가 눕거나 뒤집힘).
- 버튼 이미지 3장(`Art/UI/StartButton_*.png`)은 스크립트로 직접 만든 흰 원·빛·고리라 라이선스 문제 없음. 빛은 `PulseGraphic`으로 2초 주기로 숨 쉬듯 밝아진다.
- 보이고 숨기기는 모두 StartScreen에 붙인 `ShowDuringSteps`(onStepEntered → Refresh)로 한다.
  - StartTitle(제목+설명): START에만.
  - StartButtonGroup(버튼+AI 고지): START·END에만. END에서 다시 하기에 버튼이 필요하다.
  - 안내 HUD 글씨 패널(GuideHUD/Canvas): START 동안 숨김. GuideHUD 루트를 끄면 GuideManager·음성까지 꺼지므로 Canvas만 끈다. 화살표도 Canvas 아래라 같이 숨는다.
  - 시작 지점 체크인 표지판은 `MoveSign_Dest_Start`(목표 ID Sign_Start)다. `MoveSign_Dest_CheckIn`이 아니니 헷갈리지 말 것. START·S1-0 동안 숨기고 S1-1부터 보인다.
- 예전 "시작" 글자(StartLabel)는 버튼 글자가 대신해서 지웠다.
- 안내 HUD: 공용 GuideHUD 프리팹은 그대로, 씬 오버라이드로 Canvas 크기 0.0018→0.0013, localOffset (0,-0.27,1.5)(눈높이 약 10도 아래), lazyAngle 30.
- 공용 스크립트 변경(김씨에게 알릴 것). 모두 기본값이면 예전과 같게 동작한다.
  - `GuideUIFollow`: lazyAngle·recenterSpeed 옵션. 0이면 예전처럼 딱 붙음. 켜면 좌우만 따라오고 위아래 고개에는 흔들리지 않는다.
  - `ShowDuringSteps`: hideDuringSteps 옵션.
  - `GoalIndicatorArrow`: 기준 방향을 카메라에서 화살표 자신의 forward로 바꿨다. 딱 붙은 HUD에서는 둘이 같아서 결과가 같다.
- 씬 작업은 임시 에디터 스크립트를 배치 모드로 한 번 실행해서 했고, 스크립트는 지웠다. 미리보기는 같은 방식으로 Camera.Render 캡처. 편집 모드 미리보기에서는 게이지·화살표가 흰 네모로 보인다. 실행 때 Awake에서 모양이 만들어지기 때문이다.
- 미검증: 에디터 Play(시뮬레이터)와 Quest에서 시작 버튼 Hold → 여권 단계 진행, HUD 다시 보임, END에서 버튼 다시 보임. 이씨 확인 필요.

## 안내 HUD 화살표 끄기와 가려짐 해결 (11/25)
- 이씨 Quest 확인 의견: HUD 위 화살표(GoalArrow)가 헷갈린다, 체크인 위치에서 안내 박스가 정면 물체에 가려진다.
- 화살표: Airport_Lee에서만 끔. ScenarioManager.goalArrow 연결을 비우고(SetGoal이 오브젝트를 다시 켜기 때문) GoalArrow 오브젝트를 꺼 두었다. 공용 GuideHUD 프리팹과 GoalIndicatorArrow 스크립트는 그대로라 TestMap_Quest는 예전과 같다.
- 가려짐: HUD가 1.5m 앞 실제 공간에 있어 더 가까운 카운터·직원 뒤로 들어간다고 보았다(캡처 확인 못 함, Unity가 열려 있었음). HUD 위치를 위치마다 맞추는 대신 항상 앞에 그리는 방식을 골랐다.
  - 새 컴포넌트 `UIAlwaysOnTop`을 Airport_Lee의 GuideHUD/Canvas에 씬 오버라이드(AddedComponent)로 붙였다.
  - Awake에서 자식 Graphic마다 머티리얼 사본을 만들어 `unity_GUIZTestMode=Always`, renderQueue=Overlay로 둔다. TMP 글자는 `fontMaterial`(글자별 사본)을 쓰므로 같은 글꼴을 쓰는 표지판에는 영향이 없다.
  - 알려진 부작용: 잡은 여권을 박스 쪽으로 들면 글자가 여권 위에 겹쳐 보인다.
- 검증: 컴파일 오류 0, EditMode 66/66(새 테스트 2개 포함). Quest에서 화살표 없음, 체크인 위치에서 박스 안 가려짐을 이씨가 확인(11/25).
- Unity를 연 동안·빌드 중 저절로 바뀐 파일(글꼴 2개, 손 머티리얼, OpenXR, URP 설정 3개, GraphicsSettings, ProjectSettings, UnityConnectSettings)은 되돌리기 명령이 승인되지 않아 작업 트리에 남겨 두고 커밋에서만 뺐다.

## 직원 머리 위 이름표 삭제 (11/25)
- 이씨 요청: 직원 머리 위 역할 문구와 문구가 쓰인 판을 모두 지운다. 11/13의 "이름표 유지" 결정을 바꾼 것이다.
- 지운 것: TempObjects/Npc_* 4명(체크인 직원, 보안검색 요원, 탑승구 직원, 입국심사관) 아래 RoleLabel(글자)와 LabelBoard(판, 머리 위 1.95m). 이 두 오브젝트를 쓰는 스크립트는 없었다. 직원은 몸과 머리 모양만 남는다.
- 작업 방식: 임시 에디터 스크립트를 배치 모드로 한 번 실행(정확히 8개일 때만 저장)하고 스크립트는 지웠다. Scene 전후 문서를 비교해 다른 변경이 없음을 확인했다.
- 이번 빌드는 Mac에서 했다. 공항 .fbx·.png는 Git에 없어서 Sketchfab에서 원본(fbx)을 다시 받아 같은 이름으로 넣었다. .meta는 Git 것을 그대로 써서 Scene 연결이 맞는다. 모델 없이 Unity를 열면 .meta가 지워지니, 모델을 먼저 넣고 연다.
- Mac 빌드 APK는 Windows 빌드와 디버그 서명 키가 달라 덮어쓰기 설치가 안 된다. Quest에서 앱을 지우고 설치했다. 두 PC를 번갈아 쓰면 매번 같은 일이 생긴다.
- 검증: 컴파일 오류 0, EditMode 66/66. Quest에서 직원 위 글자·판 없음을 이씨가 확인(11/25).

## 시작 화면 제목 카드 내리기 (11/25)
- 이씨 Quest 확인 의견: "공항 체험을 시작해요" 카드가 시작 버튼보다 너무 위에 떠 있다.
- 측정(월드 높이): 시작 버튼 중심 1.68m, 바깥 고리(Track) 위 끝 1.83m, 빛(Glow) 위 끝 1.91m. 카드는 2.08~2.32m라 고리와 약 26cm 떨어져 있었다.
- StartTitle anchoredPosition.y를 2.2 → 2.0으로 바꿨다. 카드 1.88~2.12m, 고리와 약 6cm. 앉은 눈높이 1.2m에서도 덜 올려다본다.
- 다른 오브젝트(버튼, AI 음성 고지 1.30~1.60m)는 그대로다.
- 다음 작업으로 정한 것: 체크인 카운터 뒤 "보안검색" 표지판은 MoveSign_Dest_CheckIn(Sign_CheckIn, S2-4 성공 ID)이다. 표지판 이름은 목적지가 아니라 서 있는 지점 기준이다. ShowDuringSteps(stepIds S2-4)를 StartScreen에 붙여 S2-4에만 보이게 할 계획.

## 여권 겉모양 직접 제작 (11/25)
- 이씨가 임시 빨간 상자 여권을 바꾸고 싶어 했다. 검토한 후보와 버린 이유:
  - Sketchfab "passport"(Cyber king): Free Standard라 쓸 수는 있지만 미국 여권(사증면 독수리)이라 한국인 출국 체험과 맞지 않고, 원본 파일이 없어 glTF→FBX 변환이 필요, 재배포 금지라 Git 제외·각자 다운로드.
  - Sketchfab "Passport"(FrancescoMilanese): Editorial 라이선스(보도·비평 전용)이고 다운로드도 막혀 있어 사용 불가.
  - 한국 여권 이미지 + meshy.ai: 사진 저작권·나라문장 규정·AI 결과물 라이선스와 얇은 물건·글자 품질 문제로 추천하지 않음.
- 결정: 상자 여권은 그대로 두고 겉모양만 직접 만든다. `Scripts/Editor/PassportArtBuilder.cs`(메뉴 GatePassVR/Build Passport Art)가 만든다.
  - T_Passport.png(2048, Quest 최대 1024로 import): 남색 바탕, 금색 "여권 / PASSPORT", 이중 테두리, 지구본 문양(나라문장 대신), "체험용 · NOT A TRAVEL DOCUMENT". 아래쪽 띠는 책등(남색)과 속지 옆면(종이색 줄, 위아래 끝은 표지 두께).
  - 글자는 빈 Preview Scene에 TMP(Noto Dynamic)와 LineRenderer를 놓고 정사영 카메라로 찍어 만든다. 그래서 에디터 asmdef에 Unity.TextMeshPro 참조를 추가했다.
  - Passport_Mesh: 단위 상자에 면별 UV. 윗면(+y)=앞표지(위쪽이 +z), 아랫면=뒤표지, -x=책등, 나머지=속지. 오브젝트 크기 (0.088, 0.005, 0.125)로 실제 크기가 된다.
- Scene에서는 Obj_Passport_Start의 MeshFilter·MeshRenderer 참조 2줄만 바뀌었다. 강조 표시는 _BaseColor 색을 곱하므로 텍스처 위에도 보인다(Quest 확인).
- M_TempPassport는 이제 쓰는 곳이 없지만 지우지 않았다.

## 캐리어 모델 교체 (11/25)
- 이씨가 고른 "Suitcase" by moh1t(https://sketchfab.com/3d-models/suitcase-af389b9c83c146dd88c3f91246f1fb97). CC BY 4.0이라 출처·라이선스·수정 사실을 표기해야 하고, 공유가 허용돼 FBX를 Git에 올린다(공항 모델과 다름).
- 원본은 Cinema 4D에서 내보낸 FBX(부품 130여 개, 삼각형 161,152, 손잡이 편 높이 약 2.06m). `Scripts/Editor/SuitcaseBuilder.cs`(메뉴 GatePassVR/Build Suitcase)가 정리한다.
  - 0.4배(약 0.45×0.81×0.20m), 머티리얼별로 합쳐 서브메시 5개(Shell 민트, Metal, Black, Wheel, Dark) 한 메시.
  - 처음엔 S9-1 힌트("주황 캐리어")에 맞춰 몸통을 주황으로 했으나, 이씨 요청으로 Sketchfab 미리보기 색인 민트색(0.33, 0.83, 0.80)으로 바꾸고 힌트를 "민트색 캐리어"로 고쳤다. FBX 안의 원래 머티리얼 색은 살구색이라 크레딧의 "색 수정" 표기는 그대로 둔다.
  - Unity 6.3 `MeshLodUtility.GenerateMeshLods`로 부품마다 LOD를 만들고, 목표 이상인 가장 가벼운 단계의 인덱스만 뽑는다(몸통 껍데기 15%, 손잡이·모서리·바퀴 35%, 120개 이하 부품은 그대로). 결과 62,932 삼각형을 `Suitcase_Mesh.asset`으로 저장.
  - 처음에는 색별로 합친 뒤 한꺼번에 줄여(12,765) 떨어진 부품 사이가 메워졌다. 손잡이 막대 두 개가 세모난 판이 되어 그림자가 생기고, 바퀴가 가시처럼 깨졌다(이씨 Quest 의견). 그래서 부품별로 바꿨다.
  - 주의: 작은 부품은 CombineMeshes가 16비트 인덱스로 만든다. 인덱스를 읽을 때 형식을 확인해야 한다(32비트로 읽어 메시가 깨진 적 있음). 합친 메시는 정점·인덱스만 옮긴 새 메시로 다시 만든다.
  - 씬: 캐리어 두 개의 스케일을 1로 돌리고(납작하게 늘린 부모 아래 자식이 찌그러지지 않게) 원래 상자 렌더러는 끔. 자식 `SuitcaseVisual`에 메시를 입힘. 수하물 찾는 곳 것은 X축 90도로 눕힘. BoxCollider를 모델 크기로 맞추고 바닥 높이를 지키도록 조금 올림. 잡기·짐 놓는 자리 연결은 그대로.
- 보안검색의 "가방"(Obj_Bag_Security)에는 쓰지 않는다. 캐리어(맡기는 짐)와 가방(들고 타는 짐)을 모양으로도 구분하기 위해서다.
- END 크레딧에 `캐리어 모델: "Suitcase" by moh1t (Sketchfab, CC BY 4.0, 색·폴리곤 수정)` 한 줄을 넣었다.

## 잡은 물건이 손에 쥔 모습으로 보이게 (11/25)
- 이씨 Quest 의견: 여권을 잡으면 손에 쥔 게 아니라 엄지 위에 얹혀 있다.
- 원인: 물건의 XRGrabInteractable에 Attach Transform이 없어 물건 가운데가 Near-Far Interactor 기준점에 붙는다(Match Attach Position·Rotation 켜짐). 그 기준점은 손바닥보다 약 9cm 앞, 엄지 위다.
- 손 관절 위치(ControllerHandPose 적용 후)를 기준점 공간에서 재서 주먹 안쪽 빈 곳을 (0, -0.05, -0.075)m로 잡았다. 좌우 손이 x로 대칭이라 x=0이면 양손 모두 맞는다.
- `Scripts/Editor/GrabPointBuilder.cs`(메뉴 GatePassVR/Build Grab Points): 물건을 그 빈 곳에 원하는 방향으로 놓았을 때 기준점이 오는 자리에 `GrabPoint` 자식을 만들고 attachTransform에 연결한다(물건 위치는 원래대로 되돌림).
  - 여권·탑승권(Card): 길이 방향을 위로 조금 앞으로 세워 아래쪽 끝을 쥠. 노트북(HangFlat): 한쪽 가장자리를 쥐고 아래로 늘어뜨림. 가방(손잡이 없는 상자): 윗면 2.5cm 위. 캐리어: 손잡이 맨 위 막대(메시에서 계산)를 주먹 축(앞뒤)을 따라 쥠.
  - 확인용 `CaptureOnly`는 손가락을 굽혀(ControllerHandPose.Apply) 찍기 때문에 씬을 저장하지 않는다. 저장은 `BuildAndApply`로만 한다.
- 손가락이 물건 모양대로 감싸지는 것은 아니다(고정된 주먹 안에 끼워 넣는 방식). 가까이 보면 조금 겹칠 수 있다.
- 에디터 asmdef에 GatePassVR.Runtime·Unity.XR.Interaction.Toolkit 참조를 추가했다.

## 탑승권 겉모양 직접 제작 (11/25)
- 이씨 요청: 임시 흰 상자 탑승권을 바꾼다. 여권과 같은 방식(직접 그림)으로 결정.
- `Scripts/Editor/BoardingPassArtBuilder.cs`(메뉴 GatePassVR/Build Boarding Pass Art)가 만든다. 빈 Preview Scene에 TMP(Noto Dynamic)·Unlit 사각형을 놓고 정사영으로 찍어 2048 아틀라스(Quest 1024)로 저장.
- 세로형으로 그렸다. 카운터 위에서 탑승권 길이(z)가 플레이어 앞뒤로 놓이고, GrabPoint(Card)도 길이를 위로 세워 쥐기 때문에 두 경우 모두 바로 읽힌다.
- 내용: 가상 항공사 "GATEPASS AIR"(실제 항공사 이름·로고 안 씀), ICN(서울/인천)→NRT(도쿄/나리타), 편명 GP 101, 좌석 12A, 탑승구 15, 탑승 10:20, 승객 GATEPASS / TRAVELER, 읽히지 않는 장식 바코드, 절취선, "체험용 · NOT A REAL TICKET". 문서·씬에 정해진 목적지·탑승구가 없어 제안안대로 정함(안내 문구는 "목적지 공항"이라 충돌 없음).
- 씬에서는 Obj_BoardingPass_CheckIn의 MeshFilter·MeshRenderer 참조 2줄만 바뀌었다. S2-3 힌트 "카운터 위 흰 탑승권"은 바탕이 흰색이라 그대로 둔다.

