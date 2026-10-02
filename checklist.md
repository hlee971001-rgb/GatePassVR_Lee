# 공항 기본 공간 구성 - Checklist (담당 이씨)

Unity에서 실제로 확인한 항목만 체크한다. 추측으로 체크하지 않는다.

## 준비 (완료)
- [x] SampleScene → Airport_Lee 이름 변경, _GatePassVR/Scenes로 이동
- [x] Git 설정, GitHub(GatePassVR_Lee) Push, 공항 모델 원본 Git 제외
- [x] XR Interaction Toolkit 3.5.1 업그레이드 (팀 develop과 버전 통일), Console 오류 0개 확인

## ① 필수 공항 구역 구분과 기본 동선 배치
- [x] 7개 구역과 Dest_ 지점 순서 확정 (plan.md 표)
- [ ] 각 Dest_ 지점이 해당 구역(카운터, 검색대, 게이트 등) 앞에 있는지 Scene 뷰에서 확인
- [ ] 각 Dest_ 지점의 방향(Y 회전)을 상호작용 대상 쪽으로 조정
- [ ] Scanner(Boarding 구역), EXIT(Baggage 구역) 연결 방식 김씨와 협의

## ② 플레이어 이동 폭·충돌·진입 가능 구역 확인
- [ ] AirportLayoutValidator 작성 (지점 아래 바닥 Collider, 지점 겹침, 다음 지점 가림 여부 검사)
- [ ] Compile Error 0개 확인
- [ ] 7개 구역 바닥에 단순 Collider(Box) 배치
- [ ] 체크인 카운터, 보안검색대, 입국심사 부스 위에 Collider 배치
- [ ] 점검 메뉴 실행 결과 경고 0개

## ③ START~EXIT 동선으로 이동하며 막힘·불필요 공간 점검
- [ ] Play 모드에서 7개 지점 순서대로 이동 (임시로 Btn_NextArea 또는 점검 메뉴 사용)
- [ ] 각 지점에서 다음 목적지가 보이는지 확인
- [ ] 각 지점에서 상호작용 대상에 손이 닿는지 확인
- [ ] Arrival 이후 출국 구역이 보이는지 확인
- [ ] 문제를 context-notes.md에 기록하고 수정 후 다시 확인
- [ ] 커밋, GitHub Push
