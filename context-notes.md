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

### 다음 세션이 알아야 할 것
- (작업 진행하면서 추가)
