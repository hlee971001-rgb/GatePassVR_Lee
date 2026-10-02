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

- 표지판 문구는 7개 모두 "다음 구역으로 이동"으로 통일(이씨 결정, 위 표의 문구는 처음 계획). 가는 곳은 구역 표지판과 안내로 알린다.
- 판 1.2m x 0.45m, 바닥에서 2.0m 높이에 거는 형태, Noto Sans KR 큰 글씨, Box Collider(Ray용).
- 서 있는 지점에서 정면 좌우 45° 안, 2~6m 거리, 가리는 물체 없이 보이는 곳.
- Editor 도구로 생성해 수치만 바꿔 다시 만들 수 있게 한다.
- 기존 Btn_NextArea는 김씨 기능이 붙을 때까지 남겨 둔다.

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
