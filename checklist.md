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
