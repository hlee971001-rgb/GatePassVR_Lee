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
- [ ] Dest Tour로 표지판 가독성, 오브젝트 위치 확인 (이씨) — 미검증
- [ ] Device Simulator로 임시 Grab 확인 (이씨) — 미검증
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
- [ ] Device Simulator로 직접 잡아서 놓기 (이씨) — 미검증
- [ ] 김씨 구조 검토 (context-notes 요약 공유)
