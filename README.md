# GatePassVR_Lee

GatePass VR 프로젝트의 이씨 개인 작업용 Unity 프로젝트입니다. 공항 기본 공간 구성 등 콘텐츠 작업을 여기서 진행한 뒤 팀 저장소로 옮깁니다.

- Unity 6000.3.10f1 (Unity 6.3 LTS)
- 메인 Scene: `Assets/_GatePassVR/Scenes/Airport_Lee.unity`

## 공항 모델 받기 (Unity를 열기 전에 꼭 먼저)

공항 모델은 라이선스상 재배포할 수 없어서 이 저장소에 포함되어 있지 않습니다. 각자 직접 받아서 넣어야 합니다.

1. [Airport Final Big Scene (assetfactory, Sketchfab)](https://sketchfab.com/3d-models/airport-final-big-scene-88c17adc828a4952a2d06737fb62cfa4)에서 **Original format**으로 다운로드합니다. Autoconverted 형식은 파일 구성이 달라 연결되지 않습니다.
2. 압축을 풀어 아래 위치에 같은 이름으로 넣습니다.
   - `Assets/Environments/source/Airport7_FINALc_EscenaMontada.fbx`
   - `Assets/Environments/textures/Texture_airport256b.png`
3. 그 다음 Unity로 프로젝트를 엽니다.

모델 없이 Unity를 먼저 열면 Unity가 `.meta` 파일을 지워 Scene과의 연결이 끊깁니다. 이렇게 됐다면 Unity를 닫고 `git checkout -- Assets/Environments`로 `.meta`를 되돌린 뒤 2번부터 다시 진행합니다.

## 에셋 출처

- Airport model: "Airport Final Big Scene" by assetfactory on Sketchfab, Sketchfab Free Standard License
- Font: Noto Sans KR, SIL Open Font License
