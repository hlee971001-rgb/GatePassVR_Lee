# 공항 기본 공간 구성 - Plan (담당 이씨)

## 무엇을 만드는가
Airport_Lee Scene에서 공항 모델 위에 START부터 마지막 구역까지 7개 구역을 확정하고,
각 구역에서 플레이어가 막힘 없이 체험할 수 있는 상태로 만든다.

## 왜 만드는가
김씨의 기능(Point & Hold, Scanner, Hand-over)과 이씨의 콘텐츠(표지판, NPC, 오브젝트)를
붙일 위치가 먼저 정해져야 한다. 업무관리표 완료 기준은 "필요 구역이 구분되고 이동 가능"이다.

## 현재 상태 (2026-10-02)
- Scene: Assets/_GatePassVR/Scenes/Airport_Lee.unity
- 공항 모델: Sketchfab "Airport Final Big Scene" (Git 제외, README 참고)
- 이동 지점: DestinationPoints 아래 Dest_ 7개
- 이동 방식: FadeMoveController.MoveTo() (Fade Out → XR Origin 이동 → Fade In)
- 현재는 WorldUI_CheckIn/Btn_NextArea 버튼으로 이동한다. Point & Hold 연결은 김씨 담당.

## 이동 방식 전제 (CLAUDE.md §13)
플레이어는 걷지 않는다. Point & Hold → 게이지 → Fade → Dest_ 지점으로 순간이동한다.
따라서 공간은 "걷는 길"이 아니라 "서는 지점(Dest_)"을 기준으로 점검한다.

## 구역 (7개, Dest_ 지점 기준)
| 순서 | 구역 | 지점 | 위치 (x, y, z) | 비고 |
|---|---|---|---|---|
| 1 | 시작 (로비) | Dest_Start | (-15.1, 0.4, -36.6) | START |
| 2 | 체크인 | Dest_CheckIn | (10.4, 0.4, -53.9) | 여권 전달, 수하물, 탑승권 |
| 3 | 보안검색 | Dest_Security | (-2.8, 0.4, -41.4) | Security Tray |
| 4 | 탑승 게이트 | Dest_Boarding | (14.0, 5.5, -38.5) | 2층, 여권·탑승권 Scanner 포함 |
| - | (비행) | - | - | Fade와 안내방송으로 대체 |
| 5 | 도착 | Dest_Arrival | (32.0, 0.0, -80.7) | |
| 6 | 입국심사 | Dest_Immigration | (14.0, 5.5, -31.9) | 2층, General Line |
| 7 | 수하물 수취 | Dest_Baggage | (-2.8, 0.4, -29.0) | 마지막 구역, EXIT 포함 |

CLAUDE.md §17의 "여권·탑승권 Scanner"와 "EXIT"는 별도 지점을 두지 않는다(이씨 결정).
Scanner는 Boarding 구역 안의 상호작용으로, EXIT는 Baggage 구역의 마지막 Point & Hold로 처리한다.
구체적인 연결 방식은 김씨와 협의한다.

## 점검 기준
- 각 Dest_ 지점 바로 아래에 바닥 Collider가 있다.
- 각 Dest_ 지점이 벽이나 오브젝트 안에 묻혀 있지 않다.
- 각 Dest_ 지점의 방향(Y 회전)이 그 구역의 상호작용 대상(카운터, 게이트 등)을 향한다.
- 상호작용 대상은 지점에서 손이 닿는 거리(약 0.6~0.8m 앞, 높이 약 1m)에 있다.
- 각 지점에서 다음 목적지가 보인다.
- 입국 구역(Arrival 이후)에서 출국 구역이 보여 헷갈리지 않는다.
- 여권, 짐 등을 떨어뜨리면 바닥과 카운터 위에 멈춘다.

## 결과물
- Airport_Lee Scene의 Dest_ 지점 위치·방향 조정
- 바닥, 카운터 등 필요한 곳의 단순 Collider (모델 전체 Mesh Collider는 쓰지 않음, CLAUDE.md §24)
- 점검용 Editor 스크립트: Assets/_GatePassVR/Scripts/Editor/AirportLayoutValidator.cs
- 점검 결과 기록 (context-notes.md)

## 성공 조건과 검증 방법
| 성공 조건 | 검증 방법 |
|---|---|
| 7개 구역이 구분된다 | Hierarchy와 Scene 뷰에서 확인 |
| 지점 아래 바닥 Collider, 지점이 오브젝트에 묻히지 않음 | 점검 메뉴 실행 결과 경고 0개 |
| 다음 목적지가 보이고 상호작용 대상에 손이 닿음 | Play 모드에서 지점마다 직접 확인 (이씨) |
| START부터 Baggage까지 순서대로 이동 가능 | Play 모드에서 7개 지점 순서대로 이동 (이씨) |
| Compile Error 0개 | Unity Console 확인 |

## 범위 밖
표지판, NPC, 에셋 추가 배치, Point & Hold·Scanner 등 기능 구현(김씨), Btn_NextArea를 Point & Hold로 교체(김씨)

---

# 오브젝트·표지판·이동 포인트 배치 - Plan (담당 이씨)

## 무엇을 만드는가
Airport_Lee Scene의 Dest_ 7개 지점에 다음 세 가지를 배치한다.
1. 이동 표지판 7개: 다음 구역 이름이 적힌 안내판. Point & Hold로 가리키면 다음 구역으로 이동하는 목표물이 된다.
2. 구역 이름 표지판 6개: 지금 있는 곳을 알려주는 표지판(시설 위).
3. 오브젝트: 여권, 탑승권, 캐리어, 보안검색 트레이, 수하물 가방(임시 모양, 실제 크기)과
   기능이 붙을 영역 표시(수하물 올리는 자리, 스캔 자리, 여권 건네는 자리).

## 왜 만드는가
"어디로 가야 하는지 안다 → 무엇을 해야 하는지 안다"를 화면으로 보여 주기 위해서다.
김씨의 기능(Point & Hold, Grab, Scanner, Hand-over)이 붙을 자리를 먼저 확정한다.
10월 16일 START→EXIT 완주(M3)에 이동 표지판이 가장 먼저 필요하다.

## 결정 사항 (이씨)
- 이동 포인트는 표지판형. 표지판 문구는 한국어만.
- 오브젝트는 실제 크기의 임시 모양으로 먼저 배치하고, 에셋은 라이선스 정리 후 교체.
- 여권은 처음부터 손에 들고 시작한다(구현은 김씨 Grab 영역, context-notes 참고).

## 이동 표지판
| 서 있는 지점 | 문구 | 가는 곳 |
|---|---|---|
| Dest_Start | 체크인 | CheckIn |
| Dest_CheckIn | 보안검색 | Security |
| Dest_Security | 탑승구 | Boarding (2층) |
| Dest_Boarding | 탑승하기 | 비행(Fade) 후 Arrival |
| Dest_Arrival | 입국심사 | Immigration |
| Dest_Immigration | 수하물 찾는 곳 | Baggage |
| Dest_Baggage | 출구 | EXIT, 완료 화면 |

- (2026-11-13 변경, 이씨 확인) 문구를 위 표의 목적지로 되돌렸다. 2층·1층으로 가는 곳은 작은 글씨로 층을 붙인다.
  10/2의 "다음 구역으로 이동" 통일안은 어디로 가는지 알려 주지 못해 폐기.
- 바닥에 선 안내 기둥형: 남색 패널 1.2m x 0.7m, 패널 가운데 바닥에서 1.6m, 어두운 기둥, Box Collider(Ray용, 패널만).
  왼쪽 흰 원에 다음 구역 방향 화살표, 오른쪽 위 목적지, 아래 작은 글씨 "가리키고 잠시 기다리세요".
  흰 원 가운데 GaugeAnchor는 김씨 원형 게이지(RadialGaugeVisual)를 붙일 자리.
- 다음 구역 쪽, 정면 ±20°, 2.5~4m를 먼저 찾고, 앞이 높은 시설로 막혀 빈자리가 없으면 ±30°, 2~5m까지 넓힌다.
- 표지판 글자만 담은 고정 폰트(NotoSansKR-Medium SDF Signs) 사용.
- Editor 도구로 생성해 수치만 바꿔 다시 만들 수 있게 한다.
- 기존 Btn_NextArea는 김씨 기능이 붙을 때까지 남겨 둔다.

## 구역 이름 표지판 (2026-11-13 변경, 이씨 확인)
- "지금 여기가 어디인지" 알려 주는 배경 정보. 눈높이의 이동 표지판(행동)보다 작고 높게 둔다.
- 노란 판 1.6m x 0.45m, 검은 글씨, 왼쪽 0.4m 띠로 출국(파랑)/입국(초록) 구별. Collider 없음.
- 높이 3.0~3.6m, 4~8m 거리, 정면 ±30°. 판 위 3m 안에 천장이 있으면 줄 두 개로 매달고, 없으면 바닥 기둥 두 개로 세운다.
- 2층 심사 부스 줄(Boarding·Immigration)은 바로 앞이 부스와 벽이라 부스 앞면에 붙인다(이동 표지판 반대쪽 ±20°,
  앉은 사용자가 24° 안쪽으로 올려다보는 높이).
- 출구 구역 표지판은 EXIT 위치가 정해진 뒤 추가.

## 오브젝트
| 지점 | 오브젝트 | 크기 | 위치 |
|---|---|---|---|
| Start | 여권 | 12.5 x 8.8 x 0.5cm | 처음부터 손에 듦 (임시로 Start 근처) |
| CheckIn | 캐리어 | 40 x 25 x 60cm | 플레이어 옆 바닥 |
| CheckIn | 수하물 올리는 자리 | 영역 표시 | 카운터 옆 벨트 위 |
| CheckIn | 탑승권 | 20 x 8cm | 직원 쪽 카운터 위 |
| Security | 보안검색 트레이 | 60 x 40 x 8cm | 벨트 입구 |
| Boarding | 여권·탑승권 스캔 자리 | 영역 표시 | 부스 책상 위 |
| Immigration | 여권 건네는 자리 | 영역 표시 | 부스 책상 위 |
| Baggage | 집을 수 있는 가방 | 40 x 25 x 60cm | 컨베이어 가장자리, 서는 지점에서 1m 안 |

- 집는 물건에는 XR Interaction Toolkit 기본 Grab만 임시로 붙인다(기능 확정은 김씨).

## 성공 조건과 검증 방법
| 성공 조건 | 검증 방법 |
|---|---|
| 각 지점에서 다음 이동 표지판이 보인다 | 점검 도구: 정면 ±45°, 2~6m, 가림 없음 → 경고 0개. _fwd 캡처 |
| 오브젝트가 손 닿는 1m 안에 있다 | 점검 도구 경고 0개 |
| 표지판 글씨가 읽힌다 | Dest Tour로 지점마다 확인 (이씨) |
| 임시 Grab으로 집고 놓을 수 있다 | Play + Device Simulator (이씨) |
| Compile Error 0개, 기존 Validate 경고 0개 유지 | Unity Console, Validate Colliders |

## 범위 밖
Point & Hold·Grab·Scanner·Hand-over 기능 구현, 여권을 손에 든 채 시작하는 설정(김씨),
표지판 문구 확정(다음 업무 "안내문/표지 문구 확정"), 실제 에셋 교체(에셋 라이선스 정리 후), NPC 배치

---

# [08] Placement Zone 기본 구조 - Plan (담당 이씨, 김씨 구조 검토)

## 무엇을 만드는가
지정 물체를 지정 위치에 놓으면 성공 처리하는 재사용 컴포넌트. 완료 기준(업무관리표): "지정 위치에 놓으면 성공 처리".

## 구조 (팀 저장소 김씨 코드와 같은 방식)
- PlacementZoneState (Scripts/Interaction, 순수 C#): 판정만. HoldGaugeState처럼 EditMode 테스트 가능.
  성공 = 받는 물건이 손에서 놓인 채 settleTime(0.3초) 동안 가만히 영역 안. 성공은 1회만, Reset으로 다시 시도.
  받지 않는 물건이 놓이면 WrongObject를 들어올 때마다 1회.
- PlacementZone (Scripts/Interaction, MonoBehaviour): 영역 판(BoxCollider)에 붙인다.
  Inspector: zoneId, acceptedItems(XRGrabInteractable), settleTime, restingSpeed, detectHeight, lockOnPlaced, placedColor.
  이벤트: onPlaced(PlacementZone), onWrongObject(PlacementZone). 다음 Step은 정하지 않는다(§16.2, ScenarioManager 담당).
  성공 시 물건 고정(lockOnPlaced, 이씨 결정: 고정), 판 색을 MaterialPropertyBlock으로 밝게. ResetZone()으로 판정·고정 해제.
  물건 가운데가 판 가로·세로 안에 있어야 성공(걸쳐 놓은 것은 성공 아님).
- asmdef: GatePassVR.Runtime(이 프로젝트에서는 Scripts/Interaction에만), GatePassVR.Tests.EditMode — 팀 저장소와 같은 이름.

## 적용 범위
- Zone_BaggageDrop_CheckIn ← Obj_Carrier_CheckIn (AirportObjectBuilder.PlacementZones 표).
- 보안검색 트레이는 무엇을 넣을지 정해진 뒤 적용(이씨 결정). 스캔·여권 건네는 자리는 김씨 Scanner·Hand-over.

## 성공 조건과 검증
| 성공 조건 | 검증 방법 |
|---|---|
| 판정 로직(1회 성공, 손에 듦·움직임·틀린 물건·Reset) | EditMode 테스트 9개 통과 |
| 씬에서 캐리어를 놓으면 성공, 틀린 물건은 성공 아님 | Play 모드 스크립트 시험 |
| 실제 손으로 잡아 놓기 | Device Simulator (이씨) |
| Quest 반복 배치 | 실기기 (이씨) |
