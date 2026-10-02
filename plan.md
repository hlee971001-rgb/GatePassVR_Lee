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
