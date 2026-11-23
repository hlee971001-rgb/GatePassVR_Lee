# 이씨 작업 문서 (콘텐츠·에셋·오디오)

이씨가 개인 저장소(hlee971001-rgb/GatePassVR_Lee)에서 작업한 기록을 옮겨 둔 폴더입니다. 팀 루트의 plan·checklist·context-notes는 김씨 문서라 건드리지 않습니다.

- plan.md, checklist.md, context-notes.md: 이씨 업무별 계획·체크리스트·결정 기록
- guide-texts.md: 안내문·표지 문구 (ID별)
- test-cases.md: 기능 테스트 항목과 결과, 발견 문제
- assets-and-tests.md: 에셋·라이선스 목록과 테스트 결과 요약 (업무 [29])
- 빌드 방법: 팀 루트 `docs/build-guide.md` (업무 [28])
- 메인 Scene: `Assets/_GatePassVR/Scenes/Airport_Lee.unity`

## 공항 모델 받기 (Unity를 열기 전에 꼭 먼저)

공항 모델은 라이선스상 재배포할 수 없어서 저장소에 포함되어 있지 않습니다. 각자 직접 받아서 넣어야 합니다.

1. [Airport Final Big Scene (assetfactory, Sketchfab)](https://sketchfab.com/3d-models/airport-final-big-scene-88c17adc828a4952a2d06737fb62cfa4)에서 **Original format**으로 다운로드합니다. Autoconverted 형식은 파일 구성이 달라 연결되지 않습니다.
2. 압축을 풀어 아래 위치에 같은 이름으로 넣습니다.
   - `Assets/Environments/source/Airport7_FINALc_EscenaMontada.fbx`
   - `Assets/Environments/textures/Texture_airport256b.png`
3. 그 다음 Unity로 프로젝트를 엽니다.

모델 없이 Unity를 먼저 열면 Unity가 `.meta` 파일을 지워 Scene과의 연결이 끊깁니다. 이렇게 됐다면 Unity를 닫고 `git checkout -- Assets/Environments`로 `.meta`를 되돌린 뒤 2번부터 다시 진행합니다.

## 에셋 출처

- Airport model: "Airport Final Big Scene" by assetfactory on Sketchfab, Sketchfab Free Standard License
- Font: Noto Sans KR, SIL Open Font License
- 안내 음성(24개): Microsoft Azure Speech 미리 만들어진 음성 `ko-KR-InJoonNeural`로 합성(AI 합성 음성, 유료 S0 등급). 생성 스크립트 `Tools/generate_voice.py`
- 효과음: `Tools/generate_sfx.py`로 직접 합성
