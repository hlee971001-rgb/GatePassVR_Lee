# GatePass VR 기술 설정과 빌드 방법 (업무 [28])

- 원래 담당: 김씨. 김씨 부재에 대비해 이씨가 2026-11-23 현재 프로젝트 설정값을 읽어 작성했다.
- 상태: **이 문서만 보고 다른 팀원이 다시 빌드할 수 있는지는 아직 확인하지 않았다**([28] 세 번째 항목). Quest 빌드를
  실제로 해 본 뒤(이씨 5단계) 빠진 절차를 채운다.

## 1. 사용 버전

| 항목 | 값 |
|---|---|
| Unity | 6000.3.10f1 (Unity 6.3 LTS) |
| 렌더 파이프라인 | URP 17.3.0, 기본 설정 `Performance URP Config` |
| XR | OpenXR 1.17.1, XR Plug-in Management 4.6.0, Meta OpenXR 2.5.1, XR Hands 1.8.1, XR Core Utils 2.6.0 |
| 상호작용 | XR Interaction Toolkit 3.5.1 (Starter Assets, XR Device Simulator 샘플 사용) |
| 입력 | Input System 1.19.0 |
| 기타 | AR Foundation 6.5.0, Composition Layers 2.5.0, Android XR OpenXR 1.3.1 (템플릿에서 들어옴) |
| 개발 도구 | `com.coplaydev.unity-mcp` (Git URL 패키지, AI 도구 연결용. 처음 열 때 Git과 인터넷이 필요) |
| 대상 기기 | Meta Quest 3S (김씨 실기기 검증 기기) |

패키지 버전은 바꾸지 않는다(CLAUDE.md). 정확한 목록은 `Packages/manifest.json`이 원본이다.

## 2. 프로젝트 받기

1. Unity Hub에서 Unity 6000.3.10f1을 설치하고, 모듈에서 **Android Build Support**(OpenJDK, Android SDK & NDK Tools 포함)를 함께 설치한다.
2. `git clone https://github.com/KJY-1204/GatePass_VR.git` 후 `develop` 브랜치를 받는다.
3. **Unity로 열기 전에** 공항 모델을 넣는다. 라이선스상 저장소에 없다. 방법은 `docs/lee/README.md`의 "공항 모델 받기"를 따른다.
   모델 없이 먼저 열면 `.meta`가 지워져 Scene 연결이 끊긴다.
4. Unity Hub에서 프로젝트를 연다. 처음 열 때 패키지를 받느라 시간이 걸린다.

## 3. 주요 설정 (2026-11-23 현재값)

### XR Plug-in Management
| 플랫폼 | 로더 | 시작 시 초기화 |
|---|---|---|
| Android | OpenXR | 켬 |
| Windows(Standalone) | OpenXR | 켬 |

### OpenXR (Android)
- 켠 기능: Meta Quest Support, Oculus Touch Controller Profile, Meta Quest Touch Plus·Touch Pro Controller Profile,
  Hand Tracking, Meta Hand Tracking Aim, Composition Layers, Display Utilities
- Render Mode: Single Pass Instanced

### Player (Android)
| 항목 | 값 |
|---|---|
| Scripting Backend | IL2CPP |
| Target Architecture | ARM64 |
| Minimum / Target API Level | 34 / 34 |
| Graphics API | Vulkan (자동 선택 끔) |
| Color Space | Linear |
| Version / Bundle Version Code | 1.0 / 1 |
| Company / Product | DefaultCompany / GatePass_VR |
| Package Name | `com.DefaultCompany.VRTemplate` (템플릿 기본값, 아래 5번 참고) |

### 품질
- Android 기본 품질 단계: Low

### Scene
| Scene | 용도 |
|---|---|
| `Assets/_GatePassVR/Scenes/Airport_Lee.unity` | **본편** (START → EXIT 전체) |
| `Assets/_GatePassVR/Scenes/TestMap_Quest.unity` | 김씨 기능 시험장(실기기 검증용) |
| `Assets/_GatePassVR/Scenes/SmokeTest_PointHoldFade.unity` | 컴포넌트 단위 시험 |
| `Assets/Scenes/SampleScene.unity` | VR 템플릿 기본 Scene (쓰지 않음) |

현재 Build Profile Scene 목록: `SampleScene`, `TestMap_Quest` (둘 다 켜짐). **본편 Airport_Lee가 들어 있지 않다.**

## 4. Quest 빌드 절차

1. Quest를 개발자 모드로 바꾼다(Meta Horizon 앱 → 기기 → 개발자 모드). USB-C로 PC에 연결하고 헤드셋 안에서 USB 디버깅을 허용한다.
2. Unity: File → Build Profiles → Android 선택 → **Switch Platform**. 처음 전환은 오래 걸린다.
3. Build Profiles의 Scene 목록을 확인한다. 본편 빌드라면 `Airport_Lee`를 0번으로 둔다(5번 참고).
4. Run Device에서 Quest를 고른다(안 보이면 Refresh, 헤드셋에서 USB 디버깅 허용 다시 확인).
5. **Build And Run**. `.apk` 저장 위치를 물으면 저장소 밖(예: `Builds/`, Git에 올리지 않음)을 고른다.
6. 헤드셋에서 앱이 바로 실행된다. 다시 실행할 때는 앱 목록 → 출처를 알 수 없음(Unknown Sources)에서 연다.

## 5. 최종 빌드([30]) 전에 정해야 할 것 (2026-11-23 발견)

| # | 내용 | 이유 |
|---|---|---|
| F1 | Build Profile Scene 목록에 `Airport_Lee`를 0번으로 추가하고 `SampleScene`은 끈다 | 지금은 앱을 켜면 템플릿 Scene이 먼저 뜬다 |
| F2 | Package Name을 `com.DefaultCompany.VRTemplate`에서 팀 이름으로 바꾼다(예: `com.gatepass.gatepassvr`), Company도 바꾼다 | 템플릿 기본값. 다른 템플릿 앱과 겹쳐 덮어쓸 수 있다 |
| F3 | `Airport_Lee`에 들어 있는 `XR Device Simulator` 오브젝트를 기기 빌드에서 빼거나 끈다 | 에디터 시험용이다. 빌드에 들어가면 실제 컨트롤러 입력과 섞일 수 있다(미검증) |
| F4 | Bundle Version Code를 빌드할 때마다 1씩 올린다 | 같은 기기에 덮어 설치할 때 필요 |
| F5 | 한글 글꼴: 안내 HUD·시작 화면은 `Assets/Fonts/NotoSansKR-Medium SDF Dynamic.asset`(ttf 포함), 표지판은 `NotoSansKR-Medium SDF Signs`(정적) | 맑은 고딕(Windows 전용)은 11/23에 모두 교체했다. Quest에서 한글이 보이는지 확인 |

## 6. 에디터에서 시험하기 (기기 없이)
- `Airport_Lee`를 열고 Play. Scene 안의 XR Device Simulator로 조작한다(Tab: 조작 대상 전환, 마우스: 방향, WASD: 이동).
- B 또는 Y 버튼(시뮬레이터에서는 해당 키)으로 현재 단계를 다시 할 수 있다.
- EditMode 테스트: Window → General → Test Runner → EditMode → Run All (2026-11-23 기준 56개 통과).

## 7. 기록 위치
- 김씨 작업 결정: 루트 `plan.md`, `checklist.md`, `context-notes.md`
- 이씨 작업 결정: `docs/lee/` (테스트 항목과 결과 `docs/lee/test-cases.md`)
