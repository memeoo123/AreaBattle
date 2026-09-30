#!/usr/bin/env python3
"""Safely unpack UnityWebData/TuanjieWebData containers with hash evidence."""

from __future__ import annotations

import argparse
import hashlib
import json
import struct
from pathlib import Path, PurePosixPath


SIGNATURES = (b"UnityWebData1.0\0", b"TuanjieWebData1.0\0")


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def safe_relative_path(raw: str) -> Path:
    pure = PurePosixPath(raw.replace("\\", "/"))
    if pure.is_absolute() or any(part in ("", ".", "..") for part in pure.parts):
        raise ValueError(f"unsafe embedded path: {raw!r}")
    return Path(*pure.parts)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("--output-root", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    args = parser.parse_args()

    source_path = args.source.resolve()
    output_root = args.output_root.resolve()
    report_path = args.report.resolve()
    data = source_path.read_bytes()
    signature = next((item for item in SIGNATURES if data.startswith(item)), None)
    if signature is None:
        raise ValueError("unsupported WebData signature")

    cursor = len(signature)
    if cursor + 4 > len(data):
        raise ValueError("truncated WebData header")
    header_end = struct.unpack_from("<I", data, cursor)[0]
    cursor += 4
    if not cursor <= header_end <= len(data):
        raise ValueError(f"invalid header end: {header_end}")

    entries = []
    occupied = []
    while cursor < header_end:
        if cursor + 12 > header_end:
            raise ValueError("truncated WebData entry")
        offset, size, path_length = struct.unpack_from("<III", data, cursor)
        cursor += 12
        if cursor + path_length > header_end:
            raise ValueError("embedded path extends beyond header")
        raw_path = data[cursor : cursor + path_length].decode("utf-8", "strict")
        cursor += path_length
        if offset < header_end or offset + size > len(data):
            raise ValueError(f"entry outside payload bounds: {raw_path!r}")
        relative = safe_relative_path(raw_path)
        destination = (output_root / relative).resolve()
        if output_root not in destination.parents:
            raise ValueError(f"resolved path escapes output root: {raw_path!r}")
        for prior_start, prior_end, prior_name in occupied:
            if offset < prior_end and prior_start < offset + size:
                raise ValueError(f"overlapping entries: {prior_name!r} and {raw_path!r}")
        occupied.append((offset, offset + size, raw_path))
        payload = data[offset : offset + size]
        destination.parent.mkdir(parents=True, exist_ok=True)
        destination.write_bytes(payload)
        entries.append(
            {
                "path": relative.as_posix(),
                "offset": offset,
                "size": size,
                "sha256": sha256(payload),
                "headerHex": payload[:32].hex(" "),
            }
        )

    report = {
        "schemaVersion": "1.0",
        "source": {
            "path": str(source_path),
            "size": len(data),
            "sha256": sha256(data),
        },
        "signature": signature[:-1].decode("ascii"),
        "headerEnd": header_end,
        "outputRoot": str(output_root),
        "entryCount": len(entries),
        "entries": entries,
    }
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"signature": report["signature"], "headerEnd": header_end, "entryCount": len(entries)}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
