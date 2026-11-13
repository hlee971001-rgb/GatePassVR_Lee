# guide-texts.md의 음성 대본을 Azure Speech TTS로 읽어 Assets/_GatePassVR/Audio/Voice/VO_<ID>.wav로 저장하는 스크립트
import argparse
import os
import sys
import urllib.error
import urllib.request
import wave
from pathlib import Path
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parent.parent
GUIDE = ROOT / "guide-texts.md"
OUT_DIR = ROOT / "Assets" / "_GatePassVR" / "Audio" / "Voice"
CHIME = ROOT / "Assets" / "_GatePassVR" / "Audio" / "SFX" / "SFX_Chime.wav"

VOICE = "ko-KR-InJoonNeural"
OUTPUT_FORMAT = "riff-24khz-16bit-mono-pcm"  # SFX와 같은 24kHz 모노 16bit
BROADCAST_PREFIX = "(안내방송)"
BROADCAST_IDS = {"S6-1"}  # 앞에 SFX_Chime을 붙이는 대본

# 문구표 원본은 그대로 두고, 읽을 때만 발음을 바꾼다.
PRONUNCIATION = {
    "GatePass VR": "게이트패스 브이알",
}


def read_scripts():
    """guide-texts.md 표에서 (ID, 음성 대본)을 순서대로 읽는다. '음성 대본' 칸이 없거나 '-'인 줄은 건너뛴다."""
    scripts, voice_col = [], None
    for line in GUIDE.read_text(encoding="utf-8").splitlines():
        if not line.startswith("|"):
            voice_col = None  # 표가 끝나면 열 위치를 잊는다
            continue
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if cells and cells[0] == "ID":
            voice_col = cells.index("음성 대본") if "음성 대본" in cells else None
            continue
        if voice_col is None or set(cells[0]) <= {"-", " "} or voice_col >= len(cells):
            continue
        text = cells[voice_col]
        if text in ("-", "") or text.startswith("("):
            if not text.startswith(BROADCAST_PREFIX):
                continue
        scripts.append((cells[0], text.removeprefix(BROADCAST_PREFIX).strip()))
    return scripts


def to_speech_text(text):
    for src, dst in PRONUNCIATION.items():
        text = text.replace(src, dst)
    return text


def synthesize(text, key, region):
    ssml = (
        '<speak version="1.0" xml:lang="ko-KR">'
        f'<voice name="{VOICE}">{escape(text)}</voice></speak>'
    )
    req = urllib.request.Request(
        f"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1",
        data=ssml.encode("utf-8"),
        headers={
            "Ocp-Apim-Subscription-Key": key,
            "Content-Type": "application/ssml+xml",
            "X-Microsoft-OutputFormat": OUTPUT_FORMAT,
            "User-Agent": "gatepass-voice-generator",
        },
    )
    with urllib.request.urlopen(req, timeout=60) as resp:
        return resp.read()


def save_wav(path, riff_bytes, prepend_chime):
    tmp = path.with_suffix(".tmp.wav")
    tmp.write_bytes(riff_bytes)
    with wave.open(str(tmp), "rb") as src:
        params, frames = src.getparams(), src.readframes(src.getnframes())
    tmp.unlink()
    if prepend_chime:
        with wave.open(str(CHIME), "rb") as chime:
            if (chime.getframerate(), chime.getnchannels(), chime.getsampwidth()) != (
                params.framerate, params.nchannels, params.sampwidth):
                raise RuntimeError("SFX_Chime 형식이 음성과 다릅니다. generate_sfx.py를 먼저 실행하세요.")
            gap = b"\x00" * int(params.framerate * 0.3) * params.sampwidth * params.nchannels
            frames = chime.readframes(chime.getnframes()) + gap + frames
    with wave.open(str(path), "wb") as dst:
        dst.setnchannels(params.nchannels)
        dst.setsampwidth(params.sampwidth)
        dst.setframerate(params.framerate)
        dst.writeframes(frames)
    return len(frames) / (params.framerate * params.sampwidth * params.nchannels)


def main():
    sys.stdout.reconfigure(encoding="utf-8")  # Windows 콘솔 기본 인코딩(cp949)에서 한글이 깨지지 않게
    sys.stderr.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--dry-run", action="store_true", help="API를 부르지 않고 읽은 대본만 출력")
    parser.add_argument("--only", nargs="*", help="이 ID만 다시 만든다 (예: --only S2-1 END)")
    args = parser.parse_args()

    scripts = read_scripts()
    if args.only:
        scripts = [s for s in scripts if s[0] in args.only]
    if args.dry_run:
        for sid, text in scripts:
            print(f"VO_{sid}\t{to_speech_text(text)}")
        print(f"총 {len(scripts)}줄")
        return 0

    key, region = os.environ.get("AZURE_SPEECH_KEY"), os.environ.get("AZURE_SPEECH_REGION")
    if not key or not region:
        print("환경 변수 AZURE_SPEECH_KEY, AZURE_SPEECH_REGION이 필요합니다.", file=sys.stderr)
        return 1

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    for sid, text in scripts:
        try:
            audio = synthesize(to_speech_text(text), key, region)
        except urllib.error.HTTPError as e:
            print(f"VO_{sid} 실패: HTTP {e.code} {e.reason}", file=sys.stderr)
            return 1
        seconds = save_wav(OUT_DIR / f"VO_{sid}.wav", audio, sid in BROADCAST_IDS)
        print(f"VO_{sid}.wav\t{seconds:.2f}s")
    print(f"총 {len(scripts)}개")
    return 0


if __name__ == "__main__":
    sys.exit(main())
