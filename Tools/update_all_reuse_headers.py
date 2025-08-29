#!/usr/bin/env python3
"""Mass-update SPDX REUSE headers for every tracked source file.

Usage examples:
  python Tools/update_all_reuse_headers.py                     # real run
  python Tools/update_all_reuse_headers.py --dry-run           # preview only
  python Tools/update_all_reuse_headers.py --filter "Content.Client/**/*.cs"
  python Tools/update_all_reuse_headers.py --force --no-add-current

Flags:
  --force           Force recalculation of license (sets REUSE_FORCE_LICENSE=true)
  --no-add-current  Do not add current git user as author (sets REUSE_SKIP_ADD_CURRENT=true)
  --license         Fallback license label (default: script default, usually 'mit')
  --filter          One or more glob patterns to limit processed files
  --dry-run         Report only; no writes

Environment overrides respected:
  REUSE_LICENSE_MAP_JSON / REUSE_LICENSE_MAP_PATH  Additional path->license rules
  REUSE_STRIP_EMAILS=true    Strip emails from author lines

This script reuses logic from update_pr_reuse_headers.py.
"""

# SPDX-License-Identifier: MIT

from __future__ import annotations

import os
import json
import fnmatch
import subprocess
from pathlib import Path
from typing import List, Dict
import sys as _sys

# Robust import of sibling module
_SCRIPT_DIR = Path(__file__).resolve().parent
_REPO_ROOT = _SCRIPT_DIR.parent
for p in (str(_REPO_ROOT), str(_SCRIPT_DIR)):
    if p not in _sys.path:
        _sys.path.insert(0, p)
try:
    import update_pr_reuse_headers as reuse  # type: ignore
except ModuleNotFoundError:  # pragma: no cover
    try:
        import Tools.update_pr_reuse_headers as reuse  # type: ignore
    except ModuleNotFoundError as e:  # pragma: no cover
        raise SystemExit(f"Failed to import update_pr_reuse_headers: {e}")

REPO_PATH = str(_REPO_ROOT)


def git_ls_files() -> List[str]:
    try:
        out = subprocess.check_output(["git", "ls-files"], cwd=REPO_PATH, text=True, encoding="utf-8", errors="ignore")
        return [l.strip() for l in out.splitlines() if l.strip()]
    except Exception as ex:  # pragma: no cover
        print(f"Failed to list git files: {ex}")
        files: List[str] = []
        for root, _dirs, fs in os.walk(REPO_PATH):
            if ".git" in root.split(os.sep):
                continue
            for f in fs:
                files.append(os.path.relpath(os.path.join(root, f), REPO_PATH))
        return files


def _normalize_rules(data) -> List[dict]:
    rules: List[dict] = []
    if isinstance(data, dict) and "rules" in data:
        for r in data["rules"]:
            if isinstance(r, dict) and "pattern" in r and "license" in r:
                rules.append({"pattern": str(r["pattern"]), "license": str(r["license"])})
    elif isinstance(data, dict):
        for k, v in data.items():
            rules.append({"pattern": str(k), "license": str(v)})
    elif isinstance(data, list):
        for r in data:
            if isinstance(r, dict) and "pattern" in r and "license" in r:
                rules.append({"pattern": str(r["pattern"]), "license": str(r["license"])})
    return rules


def load_license_rules() -> List[dict]:
    rules: List[dict] = []
    env_json = os.environ.get("REUSE_LICENSE_MAP_JSON")
    if env_json:
        try:
            data = json.loads(env_json)
            rules.extend(_normalize_rules(data))
        except Exception as ex:
            print(f"Warning: could not parse REUSE_LICENSE_MAP_JSON: {ex}")
    if not rules:
        alt_path = os.environ.get("REUSE_LICENSE_MAP_PATH")
        if alt_path and os.path.exists(alt_path):
            try:
                with open(alt_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                rules.extend(_normalize_rules(data))
            except Exception as ex:
                print(f"Warning: failed to read {alt_path}: {ex}")
    default_file = os.path.join(REPO_PATH, ".reuse", "path-licenses.json")
    if os.path.exists(default_file):
        try:
            with open(default_file, 'r', encoding='utf-8') as f:
                data = json.load(f)
            rules.extend(_normalize_rules(data))
        except Exception as ex:
            print(f"Warning: failed to read {default_file}: {ex}")
    dedup: Dict[str, str] = {}
    for r in rules:
        pat = r.get("pattern", "")
        lic = r.get("license", "")
        if pat and lic:
            dedup[pat] = lic
    return [{"pattern": k, "license": v} for k, v in dedup.items()]


def license_for_path(path: str, default_id: str, rules: List[dict]) -> str:
    if not rules:
        return default_id
    p = path.replace('\\', '/').lstrip('./')
    matches: List[tuple[int, str]] = []
    for r in rules:
        pat = r.get("pattern", "")
        lic = r.get("license", "")
        if not pat or not lic:
            continue
        if fnmatch.fnmatchcase(p, pat):
            matches.append((len(pat), lic))
    if not matches:
        return default_id
    _, lic_label = max(matches, key=lambda t: t[0])
    return reuse._resolve_license_id(lic_label)  # type: ignore[attr-defined]


def main():  # pragma: no cover
    import argparse
    parser = argparse.ArgumentParser(description="Mass update SPDX headers across repository")
    parser.add_argument("--license", default=reuse.DEFAULT_LICENSE_LABEL, help="Fallback license label (e.g. mit, agpl, mit+agpl)")
    parser.add_argument("--dry-run", action="store_true", help="Only report changes")
    parser.add_argument("--filter", nargs="*", default=None, help="Optional glob(s) to restrict which files are processed")
    parser.add_argument("--force", action="store_true", help="Force recalculation of license even if header exists (sets REUSE_FORCE_LICENSE=true)")
    parser.add_argument("--no-add-current", action="store_true", help="Do not add current git user to author list (sets REUSE_SKIP_ADD_CURRENT=true)")
    args = parser.parse_args()

    if args.force:
        os.environ["REUSE_FORCE_LICENSE"] = "true"
    if args.no_add_current:
        os.environ["REUSE_SKIP_ADD_CURRENT"] = "true"

    fallback_id = reuse._resolve_license_id(args.license)  # type: ignore[attr-defined]
    rules = load_license_rules()
    tracked = git_ls_files()
    exts = set(reuse.COMMENT_STYLES.keys())

    if args.filter:
        filtered: List[str] = []
        for f in tracked:
            for gl in args.filter:
                if fnmatch.fnmatchcase(f, gl):
                    filtered.append(f)
                    break
        tracked = filtered

    candidates = [f for f in tracked if os.path.splitext(f)[1] in exts]

    changed = 0
    processed = 0
    for f in candidates:
        file_license = license_for_path(f, fallback_id, rules)
        if args.dry_run:
            processed += 1
            continue
        if reuse.process_file(f, file_license):  # type: ignore[attr-defined]
            changed += 1
        processed += 1

    print("--- Summary ---")
    print(f"Processed: {processed}")
    if args.dry_run:
        print("Dry run: no files modified.")
    else:
        print(f"Modified: {changed}")
        if changed:
            print("Review and commit the changes.")


if __name__ == "__main__":  # pragma: no cover
    main()
