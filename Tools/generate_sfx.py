# GatePass VR 효과음 6개를 사인파로 합성해 Assets/_GatePassVR/Audio/SFX에 WAV로 저장하는 스크립트
import math
import struct
import sys
import wave
from pathlib import Path

RATE = 24000  # 안내 음성(Azure riff-24khz-16bit-mono-pcm)과 같은 형식
OUT_DIR = Path(__file__).resolve().parent.parent / "Assets" / "_GatePassVR" / "Audio" / "SFX"


def note(freq, dur, vol=0.5, attack=0.01, release=None, harmonics=(1.0, 0.3, 0.1)):
    """부드러운 종소리 느낌의 음 하나. 시작·끝을 서서히 올리고 내려 '딱' 소리를 막는다."""
    n = int(RATE * dur)
    release = dur * 0.8 if release is None else release
    out = []
    for i in range(n):
        t = i / RATE
        env = min(1.0, t / attack) if attack > 0 else 1.0
        if t > dur - release:
            env *= max(0.0, (dur - t) / release)
        s = sum(a * math.sin(2 * math.pi * freq * (k + 1) * t) for k, a in enumerate(harmonics))
        out.append(vol * env * s / sum(harmonics))
    return out


def sweep(f0, f1, dur, vol=0.35):
    """게이지가 차는 동안 높아지는 소리. 마지막 0.05초만 서서히 줄인다."""
    n = int(RATE * dur)
    out, phase = [], 0.0
    for i in range(n):
        t = i / RATE
        freq = f0 + (f1 - f0) * (t / dur)
        phase += 2 * math.pi * freq / RATE
        env = min(1.0, t / 0.02) * min(1.0, (dur - t) / 0.05)
        out.append(vol * env * (0.8 * math.sin(phase) + 0.2 * math.sin(2 * phase)))
    return out


def silence(dur):
    return [0.0] * int(RATE * dur)


def mix(*parts):
    """(시작 시각, 샘플) 묶음을 겹쳐 하나로 만든다."""
    length = max(int(start * RATE) + len(s) for start, s in parts)
    out = [0.0] * length
    for start, s in parts:
        off = int(start * RATE)
        for i, v in enumerate(s):
            out[off + i] += v
    return out


SOUNDS = {
    # 행동 성공: 짧게 올라가는 두 음
    "SFX_Success": lambda: mix((0.0, note(880, 0.25)), (0.10, note(1318.5, 0.35))),
    # 다시 하기 힌트: 꾸짖지 않도록 낮고 부드럽게 내려가는 두 음 (CLAUDE.md §13 Game Over 없음)
    "SFX_Retry": lambda: mix((0.0, note(523.3, 0.22, vol=0.4)), (0.14, note(440, 0.3, vol=0.4))),
    # Point & Hold 게이지: PointAndHoldTarget.holdDuration 기본값 1.5초 동안 높아짐
    "SFX_GaugeFill": lambda: sweep(330, 660, 1.5),
    # 스캔 인식: 삐빅
    "SFX_Scan": lambda: mix((0.0, note(1760, 0.08, attack=0.003, release=0.02, harmonics=(1.0,))),
                            (0.11, note(1760, 0.08, attack=0.003, release=0.02, harmonics=(1.0,)))),
    # 기내 안내방송 차임: 딩-동-딩 (VO_S6-1 앞에 붙는다)
    "SFX_Chime": lambda: mix((0.0, note(784, 0.9, vol=0.45)), (0.45, note(659.3, 0.9, vol=0.45)),
                             (0.9, note(523.3, 1.2, vol=0.45))),
    # 체험 완료: 도-미-솔-도
    "SFX_Complete": lambda: mix((0.0, note(523.3, 0.5)), (0.15, note(659.3, 0.5)),
                                (0.30, note(784, 0.5)), (0.45, note(1046.5, 0.8))),
}


def write_wav(path, samples):
    peak = max(abs(v) for v in samples) or 1.0
    scale = min(1.0, 0.5 / peak)  # 최고 음량을 약 -6dB로 맞춘다
    with wave.open(str(path), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(b"".join(struct.pack("<h", int(v * scale * 32767)) for v in samples))


def main():
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    for name, make in SOUNDS.items():
        samples = make() + silence(0.05)
        path = OUT_DIR / f"{name}.wav"
        write_wav(path, samples)
        print(f"{path.name}\t{len(samples) / RATE:.2f}s")
    return 0


if __name__ == "__main__":
    sys.exit(main())
