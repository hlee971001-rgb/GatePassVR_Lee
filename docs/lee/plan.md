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

---

# [20] 안내문·표지 문구 정리 - Plan (담당 이씨, 김씨 UI 반영)

## 무엇을 만드는가
START → STEP 1~9 → EXIT 전체 흐름의 안내 문구표(guide-texts.md). 완료 기준(업무관리표): "안내 문구 확정·UI 반영".
이씨는 문구를 확정하고, 김씨가 GuideManager.SetGuide(메인, 힌트, 음성)·ScenarioStep에 반영한다.
같은 표가 [12] 안내문 입력, [11] Step 데이터 입력, [21] 안내 음성 대본의 원본이다.

## 내용
- 한 화면에 행동 하나 단위(ID S1-1 등)로 메인 안내(16자 이내), 보조 힌트(24자 이내), 음성 대본, 틀린 행동 힌트.
- 시작·비행 안내방송·완료 화면 문구, 용어표, 표지판 문구 모음, 김씨와 협의할 것.

## 검증
- 글자 수 기준을 넘는 줄 0개(스크립트로 확인), 금지 용어 0개, 표지판 도구 문구와 일치.
- 실제 HUD 한 줄 표시는 UI 반영 때 김씨 확인.

---

# [22] NPC 배치 및 상호작용 지점 설정 - Plan (담당 이씨, 김씨 기능 연결)

## 무엇을 만드는가
애니메이션 없는 정적 NPC 4명과 그 NPC에게 하는 상호작용 지점. 완료 기준(업무관리표): "애니메이션 없이 역할 수행 가능".

## 내용
- NPC(임시 사람 모양, 키 약 1.7m, 역할별 제복 색, 머리 위 역할 이름표, 몸 Collider), 서는 지점을 바라봄.
  체크인 직원(카운터 뒤) · 보안검색 요원(벨트 왼쪽) · 탑승구 직원(부스 안) · 입국심사관(부스 안).
- NpcStation 컴포넌트: 역할 ID, 이름, 상호작용 지점(Zone)을 묶어 김씨가 기능을 연결할 때 쓴다.
- 여권 건네는 자리 2곳(체크인 새로 추가, 입국심사 기존)은 김씨 Hand-over 전까지 Placement Zone으로 임시 동작(고정하지 않음).
- 실제 사람 에셋은 라이선스 확인 뒤 교체(공항 모델에는 직원으로 쓸 사람 모델이 없음).

## 검증
- Validate 경고 0개(표지판이 NPC에 가려지지 않음), 지점 4곳 캡처, Play 모드에서 여권을 두 자리에 차례로 놓아 성공.
- 역할이 이해되는지 Dest Tour로 이씨 확인(Trello 3번).

---

# [21] 안내 음성·효과음 정리 - Plan (담당 이씨, 김씨 시스템 연결)

## 무엇을 만드는가
START → STEP 1~9 → END 안내 음성 22개(guide-texts.md "음성 대본" 21줄 + C-RESET)와 효과음 6개.
완료 기준(업무관리표): "필요 음원 준비 및 재생".

## 결정 사항 (이씨, 2026-11-13)
- 음성: Azure Speech 미리 만들어진 음성 `ko-KR-InJoonNeural`(성인 남성). 유료 S0 리소스로 생성한다
  (Product Terms의 출력물 사용권이 유료 등급 고객에만 명시됨).
- AI 음성 고지: Microsoft AI 서비스 행동강령 3항(합성 음성임을 알림)에 따라 시작 화면과 크레딧에
  "안내 음성은 AI로 합성한 음성입니다"를 넣는다(guide-texts.md C-AIVOICE). Microsoft 출처 표기 의무는 없음.
- 효과음: 외부 음원 대신 스크립트로 직접 합성한다(라이선스 문제 없음).
- 브랜치: feature/lee-audio. 음성까지 만들고 이씨가 들어 본 뒤 main에 합친다.

## 구조
- `Tools/generate_voice.py`: guide-texts.md 표를 읽어 Azure TTS REST로 `Assets/_GatePassVR/Audio/Voice/VO_<ID>.wav` 생성.
  키는 환경 변수 `AZURE_SPEECH_KEY`, `AZURE_SPEECH_REGION`. 발음 바꿈표(예: GatePass VR → 게이트패스 브이알)는 스크립트에만 둔다.
  S6-1 기내 안내방송은 앞에 SFX_Chime을 붙인다. `--dry-run`은 API 없이 읽은 대본만 출력.
- `Tools/generate_sfx.py`: `Assets/_GatePassVR/Audio/SFX/SFX_*.wav` 6개 합성(24kHz 모노 16bit, 음성과 같은 형식).
  Success(성공), Retry(다시 하기 힌트), GaugeFill(게이지 1.5초, PointAndHoldTarget.holdDuration 기본값), Scan(스캔),
  Chime(안내방송), Complete(완료).
- 재생 연결은 김씨: GuideManager.SetGuide(메인, 힌트, voice)에 VO_<ID>를 넣는다. 효과음 재생 코드는 아직 없음.

## 성공 조건과 검증
| 성공 조건 | 검증 방법 |
|---|---|
| 대본 22줄을 빠짐없이 읽음 | `--dry-run` 출력 22줄 |
| 음성 22개·효과음 6개 생성, 길이 0초 없음 | 스크립트 출력과 파일 확인 |
| Unity 가져오기 오류 없음 | Console Error 0개 |
| 이씨 프로젝트에서 재생됨 | Play 모드 AudioSource 재생 |
| 듣기에 자연스러움 | 이씨가 직접 들어 확인 (그 전까지 미검증) |

---

# [이전] 팀 저장소 develop으로 옮기기 - Plan (담당 이씨, 김씨 확인)

## 무엇을 하는가
개인 저장소(hlee971001-rgb/GatePassVR_Lee)의 이씨 작업을 팀 저장소(KJY-1204/GatePass_VR)의 develop에 PR로 합친다.
두 저장소는 Git 기록이 이어져 있지 않다(개인 첫 커밋 a6d4a9f 10/2, 팀 develop bcfb347 9/11). 그래서 브랜치째 머지하지 않고,
팀 develop에서 새 브랜치를 만들어 이씨 파일만 옮겨 담는다(CLAUDE.md §19, §19.2).

## 왜 하는가
M3(START→EXIT 완주)와 김씨 기능 연결(Point & Hold, Hand-over, Scanner, GuideManager 음성)은 같은 프로젝트에 있어야 할 수 있다.
옮긴 뒤에는 팀 저장소를 받은 폴더에서만 작업하고, 개인 저장소는 백업으로 남긴다.

## 두 번에 나눠 옮긴다 (2026-11-20 이씨 결정)
처음 계획은 한 번에 옮기는 것이었다. 안내 음성이 아직 없으므로 두 번으로 나눈다.
- 1차 (콘텐츠): 개인 main에 있는 것. 김씨 확인을 받으면 바로 옮긴다.
- 2차 (음원): 효과음·안내 음성. (2026-11-23 이씨 결정으로 바꿈) 음성 22개를 개인 저장소(C:\GatePassVR_Lee) feature/lee-audio에서
  먼저 만들고 이씨가 들어 본 뒤 개인 GitHub에 푸시한다. 그다음 팀 저장소(C:\GatePass_VR)에서 develop 기준 feature/lee-audio를 새로 만들어
  음원 파일을 .meta와 함께 복사하고 팀 develop으로 PR한다. 개인 저장소에 완성본 백업을 남기고, 확인을 마친 결과만 팀에 올리기 위해서다.
  (11/20 원래 계획: 음성을 팀 폴더에서 바로 생성. 복사 단계 하나를 줄이려던 것이었다.)
- 이유: 콘텐츠는 음성 없이도 동작하고, develop에 늦게 합칠수록 김씨 작업과 충돌이 커진다. 11월은 안정화 시기라 develop에서 빨리 같이 검증한다.
  PR을 둘로 나누면 김씨가 검토하기도 쉽다(CLAUDE.md §19 의미 단위).
- 커밋은 옮기지 않는다. 파일의 최종 상태만 복사해 팀 브랜치에 새 커밋으로 올린다. 개인 저장소 커밋 기록은 개인 저장소에 남는다.

## 기준 시점 (2026-11-20 확인)
- 팀 develop: bcfb347 (2026-09-11, 김씨 마지막 커밋). 쓰기 권한 있음(hlee971001-rgb, dry-run 확인).
- 개인: main 90878ab, feature/lee-audio 26a7ced(main 포함, PR #1 열림·머지 안 함).
- 11/20 정리 후: 개인 main dec191a(Unity 자동 변경 d40e2b4, 이 계획 dec191a 포함), feature/lee-audio df0a7d0(main + 음원 커밋 3개).
  PR #1은 열어 둔다(이씨 결정). 1차는 main, 2차는 feature/lee-audio의 음원 커밋 3개(f8e6410, 689ce79, df0a7d0)가 원본이다.

## 1차에 옮기는 것 (.meta 함께, 개인 main 기준)
| 대상 | 경로 |
|---|---|
| Scene과 라이트맵 | Assets/_GatePassVR/Scenes/Airport_Lee.unity, Airport_Lee/ |
| 임시 머티리얼 20개 | Assets/_GatePassVR/Art/Temp/M_Temp*.mat |
| Editor 도구 7개 | Assets/_GatePassVR/Scripts/Editor/*.cs + 새 Editor asmdef |
| 런타임 스크립트 3개 | Scripts/Interaction/PlacementZone.cs, PlacementZoneState.cs, NpcStation.cs |
| EditMode 테스트 | Tests/EditMode/PlacementZoneStateTests.cs |
| 표지판 폰트 | Assets/Fonts/NotoSansKR-Medium.ttf, NotoSansKR-Medium SDF Signs.asset |
| 공항 모델 .meta만 | Assets/Environments/ (.fbx·.png는 라이선스상 제외) |
| Scene이 쓰는 템플릿·샘플 | Assets/VRTemplateAssets/Models/Environment/Torus Cut.fbx, XRI 3.5.1 XR Device Simulator 샘플 |
| 문서 | guide-texts.md(main 버전), README 공항 모델 안내 |
| .gitignore 규칙 | 공항 모델 .fbx·.png 제외, /LayoutCaptures/ 제외 (추가만, 기존 규칙 유지) |

## 2차에 옮기는 것 (.meta 함께, 개인 feature/lee-audio 기준)
| 대상 | 경로 |
|---|---|
| 효과음 6개 | Assets/_GatePassVR/Audio/SFX/ (Audio.meta, SFX.meta 포함) |
| 음원 생성 도구 | Tools/generate_sfx.py, Tools/generate_voice.py |
| 문서 추가분 | guide-texts.md의 AI 음성 고지(C-AIVOICE), README 음원 출처 줄 |
| 안내 음성 22개 | 개인 저장소에서 generate_voice.py로 생성한 VO_*.wav (Azure Speech S0, 11/23 변경) |
| [21] 계획·체크리스트·결정 기록 | 이씨 문서 위치(충돌 3)가 정해진 곳에 함께 |

## (이전 표, 기록용) 한 번에 옮기려던 목록
| 대상 | 경로 |
|---|---|
| Scene과 라이트맵 | Assets/_GatePassVR/Scenes/Airport_Lee.unity, Airport_Lee/ |
| 임시 머티리얼 20개 | Assets/_GatePassVR/Art/Temp/M_Temp*.mat |
| 효과음 6개 | Assets/_GatePassVR/Audio/SFX/ |
| Editor 도구 7개 | Assets/_GatePassVR/Scripts/Editor/*.cs + 새 Editor asmdef |
| 런타임 스크립트 3개 | Scripts/Interaction/PlacementZone.cs, PlacementZoneState.cs, NpcStation.cs |
| EditMode 테스트 | Tests/EditMode/PlacementZoneStateTests.cs |
| 표지판 폰트 | Assets/Fonts/NotoSansKR-Medium.ttf, NotoSansKR-Medium SDF Signs.asset |
| 공항 모델 .meta만 | Assets/Environments/ (.fbx·.png는 라이선스상 제외) |
| Scene이 쓰는 템플릿·샘플 | Assets/VRTemplateAssets/Models/Environment/Torus Cut.fbx, XRI 3.5.1 XR Device Simulator 샘플 |
| 도구·문서 | Tools/generate_sfx.py, Tools/generate_voice.py, guide-texts.md, README 공항 모델 안내 |
| .gitignore 규칙 | 공항 모델 .fbx·.png 제외, /LayoutCaptures/ 제외 (추가만, 기존 규칙 유지) |

## 옮기지 않는 것
- Assets/Scripts/FadeMoveController.cs: 팀 저장소 Scripts/VR/FadeMoveController.cs와 같은 클래스(GatePassVR.VR). 둘 다 있으면 컴파일 에러.
- Scripts/Interaction/GatePassVR.Runtime.asmdef, Tests/EditMode/GatePassVR.Tests.EditMode.asmdef: 팀 것과 이름·내용이 같다. 팀 것을 쓴다.
- 폴더 .meta(_GatePassVR/Art.meta 등): 팀 것을 그대로 둔다.
- ProjectSettings, Packages, Assets/Settings, Assets/XR, XRI 설정, TextMesh Pro, CompositionLayers, 26a7ced(Unity 자동 변경 5개, 11/20 정리 후 main의 d40e2b4).
- 동적 폰트 NotoSansKR-Medium SDF.asset(Scene에서 쓰지 않음, 실행 때마다 바뀜). 표지판 도구가 필요로 하면 그때 다시 판단.

## 해결해야 할 충돌
1. FadeMoveController GUID: Scene과 DestTourWindow가 개인 프로젝트의 FadeMoveController(guid 502ad16d…)를 쓴다.
   팀 FadeMoveController의 GUID로 바꾸거나 Unity에서 다시 연결해야 한다. 방법은 김씨와 정한다.
2. Editor asmdef: 팀은 Scripts 전체가 GatePassVR.Runtime이라 Scripts/Editor의 도구가 런타임에 들어가 빌드가 깨진다.
   Scripts/Editor에 GatePassVR.Editor(includePlatforms: Editor, GatePassVR.Runtime 참조)를 만든다. 김씨 구조 영역이라 먼저 공유.
3. 루트 plan.md·checklist.md·context-notes.md: 팀 저장소에 김씨 문서가 있다. 덮어쓰지 않는다. 이씨 문서를 어디에 둘지 김씨와 정한다
   (후보: 팀 문서에 이씨 절 추가, 또는 docs/lee/로 분리).
4. XR Device Simulator 샘플: 팀 저장소에 없다. 샘플 추가는 XR 설정(김씨)과 겹치므로 확인 후 넣는다.

## 순서
1. 김씨에게 확인: 팀 저장소에 아직 안 올린 작업이 있는지, 위 충돌 1~4의 처리 방법.
2. 팀 저장소를 별도 폴더에 clone, develop에서 feature/lee-content 생성. 공항 모델을 README대로 먼저 넣는다(먼저 열면 .meta가 지워짐).
3. 옮기는 것만 복사, Editor asmdef 추가, FadeMoveController 참조 정리.
4. Unity로 열어 검증(아래), 의미 단위로 커밋.
5. 푸시, 팀 develop으로 PR. 김씨 리뷰 후 머지, 머지된 develop을 다시 열어 재검증(§14.2-12).
6. 이후 작업은 팀 저장소 폴더에서 feature/lee-* 브랜치로. 개인 저장소는 백업.

위 1~5가 1차다. 1차 PR 대상(base)이 main이 아니라 develop인지 PR 만들기 전에 확인한다.

### 2차 순서 (1차 머지 후, 2026-11-23 바꿈)
1차 팀 PR #1은 develop에 머지됐다(머지 커밋 a2938a7, 11/23 확인).

개인 저장소(C:\GatePassVR_Lee, feature/lee-audio)
1. Azure Speech S0 리소스·환경 변수 준비(이씨, 키는 커밋하지 않음).
2. 음성 22개 생성, Unity로 열어 Import 경고·길이 0초·Console Error 확인, Play 모드 재생 확인.
3. 이씨가 효과음 6개와 음성 22개를 들어 발음·속도 확인.
4. 의미 단위로 커밋, 개인 GitHub에 푸시.

팀 저장소(C:\GatePass_VR)
5. 머지된 develop을 받아 feature/lee-audio를 새로 만든다.
6. 개인 feature/lee-audio에서 2차 표의 파일을 복사한다(.meta 포함, 커밋 기록은 옮기지 않음).
7. Unity로 열어 검증, 의미 단위로 커밋, 팀 develop으로 PR. 김씨가 GuideManager·효과음 재생 위치에 연결.
8. 머지된 develop 재검증. 개인 저장소 PR #1은 기록용으로 남긴다(머지 여부는 이씨가 정함).

## 성공 조건과 검증
| 성공 조건 | 검증 방법 |
|---|---|
| Compile Error 0개 | Unity Console |
| Airport_Lee에 Missing Script·Missing Prefab·Missing Reference 없음 | Scene 열기, Console, Inspector 확인 |
| 기존 EditMode 테스트(김씨 HoldGauge·GuideReguide)와 PlacementZoneState 9개 통과 | Test Runner EditMode |
| Validate Colliders 경고 0개 | GatePass > Layout > Validate Colliders |
| Dest Tour로 7개 지점 이동 | Play 모드 + XR Device Simulator |
| 공항 모델 .fbx·.png가 커밋되지 않음 | git status, PR 파일 목록 |
| 김씨 Scene(SmokeTest_PointHoldFade, TestMap_Quest)·Prefab 변경 없음 | PR 파일 목록 |

위 표는 1차·2차 모두 적용한다. 2차에 더하는 조건은 아래와 같다.

| 2차 성공 조건 | 검증 방법 |
|---|---|
| 효과음 6개·음성 22개 Import 경고 없음, 길이 0초 없음 | Unity Console, Inspector |
| Play 모드에서 재생됨 | Airport_Lee Play 모드 |
| 이씨가 직접 들어 확인 | 이씨 청취 (못 하면 "미검증") |
| Azure 키가 커밋되지 않음 | git diff, PR 파일 목록 |

## 범위 밖
옮기면서 기능 고치기, 김씨 코드 수정, 패키지·프로젝트 설정 변경, 음성 22개 생성([21] 업무에서 따로).
(11/20 변경) 음성 22개 생성은 1차 범위 밖이고 2차에서 한다.

---

# [마무리] 김씨 부재 대비, START→EXIT 연결부터 최종 빌드까지 - Plan (담당 이씨)

## 무엇을 하는가
김씨가 프로젝트에 참여하지 못할 경우를 대비해, Trello에 남은 김씨 카드([13] 힌트, [14], [23]~[26], [28], [30])를
이씨가 이어서 마무리한다. 김씨가 2026-11-23에 develop에 올린 ScenarioManager·Hand-over·Scanner·Highlight·
ResetController·공통 Prefab은 **다시 만들지 않고 그대로 쓴다**(테스트 56개 통과, 문서와 동작 일치 확인).

## 왜 이렇게 하는가 (2026-11-23 이씨 결정)
- 김씨 코드는 검증되어 있어 다시 만들면 시간만 들고 팀 저장소에 같은 역할 스크립트가 두 벌 생긴다.
- 최종 목표일(11/20)이 지났고 Trello에 "완료 11월 27일" 목록이 있다. 끝까지 플레이 가능한 빌드가 먼저다.
- [14]의 콘텐츠 적용은 원래 이씨 역할(CLAUDE.md §18 "김씨 기능의 콘텐츠 적용")이고 Airport_Lee는 이씨 씬이다.

## 저장소 흐름
- 팀 클론 `C:\GatePass_VR`에서 develop(b0cddc5)으로부터 `feature/lee-full-flow`를 만들어 작업한다.
- 원격 `lee`(개인 GatePassVR_Lee)에 먼저 push하고, 준비되면 같은 브랜치를 `origin`(팀)에 push해 develop으로 PR한다.
  복사 방식이 아니라 김씨 커밋 위에 이어서 쌓으므로 김씨가 돌아와도 그대로 이어받을 수 있다.
- 김씨 스크립트를 고칠 때는 커밋과 context-notes에 "김씨 부재로 이씨 수정, 이유"를 남긴다.

## 단계
1. 준비: 브랜치·원격, 이 계획 기록, 팀 알림(이씨가 직접 보냄).
2. [14] Airport_Lee에 GatePassSystems·GuideHUD·PointHoldPad·HandOverZone·ScannerZone 배치, guide-texts.md ID 순서로
   Step 데이터와 VO_<ID> 22개 연결. 출국(S0~S4) 먼저 Play 확인, 그다음 입국(S5~EXIT).
   - 문제 1: 체크인에서 건넨 여권이 돌려받는 자리 없이 고정되면 S4-1·S8-1에서 쓸 수 없다(Play로 재현). 체크인에도
     returnPoint를 두고 S2-3에서 여권도 받게 한다(문구 한 줄 수정).
   - 문제 2: ResetController가 등록된 물건을 전부 맨 처음 위치로 돌린다. 공항에서는 여권이 출발 지점으로 가서 막힌다.
     Step 단위로 되돌리게 고친다.
3. [13] 틀린 물건 힌트 연결, 시작 화면 AI 음성 고지(C-AIVOICE), Quest용 한글 폰트(Noto Sans KR) 교체.
4. [23]·[24] 기능 테스트 목록, 시뮬레이터 START→EXIT 완주, 막힘 수정([26]).
5. [25] Quest 빌드·실기기 확인(기기 필요).
6. [28] 기술 설정·빌드 문서, [29] 에셋·테스트 결과 문서.
7. [30] 최종 빌드와 백업.

## 성공 조건과 검증
- Airport_Lee에서 START→EXIT를 시뮬레이터 Play로 끊김 없이 완주한다(Step 전환·안내·음성·이동).
- Compile Error 0, EditMode 전체 통과, Missing Script·Reference 0(GatePassVR 컴포넌트 기준).
- Quest 실기기에서 한글이 깨지지 않고 완주된다(5단계, 기기 없으면 미검증으로 남김).

## 범위 밖
- 김씨 기능 재구현, NPC Animation, 새 상호작용 종류 추가.

---

# 컨트롤러를 쥔 손 모양 (Unity XR Hands 기본 손) - Plan (담당 이씨, 2026-11-24)

## 무엇을 하는가
PolyOne 손(출처 표기 필요 CC BY 4.0)을 뺀 뒤, 컨트롤러를 쥔 상태에서도 손 모양이 보이게 한다. 모델은 이미 프로젝트에 있는
XR Hands 샘플 `LeftHand.fbx`/`RightHand.fbx`(출처 표기 조항 없음)와 XRI 샘플 머티리얼 `Unity_Hand_Medium`(맨손 핸드 트래킹 손과 같은 모습)을 쓴다.

## 왜 (이씨 결정)
"여권을 잡으세요" 같은 장면은 손 모양이 더 자연스럽다. 새 에셋을 받지 않아 라이선스 확인과 저장소 문제가 없다.

## 방법
- 손가락은 고정 자세로 굽힌다(버튼에 따라 움직이지 않음, 마감 고려). Quest 런타임이 컨트롤러로 손 모양을 만들어 주는 기능
  (XR_EXT_hand_tracking_data_source)은 설치된 패키지에 없다.
- `ControllerHandPose`(Scripts/VR): 시작할 때 한 번 손가락 관절을 각자 X축으로 굽힌다. 각도는 Inspector에서 조정.
- 손 프리팹 2개(Prefabs/VR)를 공용 Rig의 `Left/Right Controller` 바로 아래에 붙이고 `UniversalController`를 끈다.
  `Controller Visual`은 오른쪽이 X 스케일 -1이라 그 아래에 두지 않는다(김씨 "오른손이 왼손처럼 보임" 원인).
- 맨손일 때는 기존 핸드 트래킹 손이 그대로 보인다(XRInputModalityManager).

## 성공 조건과 검증
- 컴파일 오류 0, EditMode 전체 통과(자세를 두 번 적용해도 한 번만 굽는지 테스트 추가).
- 에디터 렌더 캡처로 손이 컨트롤러를 감싼 모습 확인 → Quest 착용 확인(손 방향·위치, 잡기·Hold 그대로).
- 공용 프리팹은 김씨 소유라 변경 목적을 김씨에게 알린다(이씨).

## 범위 밖
- 버튼 입력에 따른 손가락 움직임, 손과 물체의 접촉 자세 맞추기.
