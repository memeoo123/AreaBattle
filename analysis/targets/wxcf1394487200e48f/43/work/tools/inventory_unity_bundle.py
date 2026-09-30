#!/usr/bin/env python3
"""Create a deterministic, read-only inventory of a Unity asset bundle."""

from __future__ import annotations

import argparse
import hashlib
import json
import sys
from collections import Counter
from pathlib import Path


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("bundle", type=Path)
    parser.add_argument("--vendor-root", type=Path, required=True)
    parser.add_argument("--output", type=Path)
    return parser.parse_args()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def main() -> int:
    args = parse_args()
    bundle = args.bundle.resolve()
    vendor_root = args.vendor_root.resolve()
    sys.path.insert(0, str(vendor_root))

    import UnityPy  # type: ignore[import-not-found]

    environment = UnityPy.load(str(bundle))
    type_counts = Counter(obj.type.name for obj in environment.objects)
    objects = []
    for obj in environment.objects:
        asset_file = getattr(obj, "assets_file", None)
        try:
            object_name = obj.peek_name()
        except Exception as exc:  # Keep inventorying when one type lacks a name reader.
            object_name = None
        objects.append(
            {
                "pathId": obj.path_id,
                "type": obj.type.name,
                "assetFile": getattr(asset_file, "name", None),
                "name": object_name,
            }
        )

    containers = []
    for key, value in environment.container.items():
        if isinstance(key, str):
            name, pointer = key, value
        else:
            name, pointer = str(value), key
        path_id = getattr(pointer, "path_id", None)
        if path_id is None and isinstance(pointer, (tuple, list)) and pointer:
            path_id = getattr(pointer[-1], "path_id", None)
        containers.append({"name": name, "pathId": path_id})
    containers.sort(key=lambda item: (item["name"], item["pathId"] or -1))

    report = {
        "schemaVersion": "1.0",
        "tool": {
            "name": "UnityPy",
            "version": getattr(UnityPy, "__version__", "unknown"),
            "vendorRoot": str(vendor_root),
        },
        "source": {
            "path": str(bundle),
            "size": bundle.stat().st_size,
            "sha256": sha256_file(bundle),
        },
        "objectCount": len(objects),
        "containerCount": len(containers),
        "typeCounts": dict(sorted(type_counts.items())),
        "containers": containers,
        "objects": objects,
    }

    rendered = json.dumps(report, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    else:
        print(rendered, end="")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
