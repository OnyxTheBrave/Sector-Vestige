#!/usr/bin/env python3
"""Check (and optionally fix) existing SPDX headers.

Behavior:
  - Scans git-tracked files with extensions supported by update_pr_reuse_headers.py
  - Only considers files that already contain an SPDX-License-Identifier line near the top.
  - Determines expected license via .reuse/path-licenses.json (same rules as other scripts).
  - Reports mismatches. Exit code 0 if all good, 1 if mismatches (unless --fix used and all fixed).

Usage:
    python Tools/reuse_header_check.py                        # report only
    python Tools/reuse_header_check.py --json                 # report JSON
    python Tools/reuse_header_check.py --fix                  # fix mismatches in-place (license only, authors preserved)
    python Tools/reuse_header_check.py --filter "Content.Client/**/*.cs"  # include only these
    python Tools/reuse_header_check.py --exclude "Content.Client/_Harmony/**"  # process everything except excluded

Options:
    --fix              Fix incorrect license headers (rebuilds header via process_file; preserves authors collected from git)
    --json             Output machine-readable JSON summary
    --filter GLOB..    Limit to matching paths (multiple allowed)
    --exclude GLOB..   Exclude matching paths (applied after --filter)
    --license LBL      Override fallback license label (default: script default)

Exit codes:
  0 = success / no mismatches (or all fixed)
  1 = mismatches found (and not fully fixed)
  2 = internal error
"""

# SPDX-License-Identifier: MIT

from __future__ import annotations

import os
import sys
import json
import fnmatch
import subprocess
from pathlib import Path
from typing import List, Dict, Any

SCRIPT_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPT_DIR.parent

for p in (str(REPO_ROOT), str(SCRIPT_DIR)):
    if p not in sys.path:
        sys.path.insert(0, p)

try:
    import update_pr_reuse_headers as reuse  # type: ignore
except ModuleNotFoundError:
    try:
        import Tools.update_pr_reuse_headers as reuse  # type: ignore
    except ModuleNotFoundError as e:  # pragma: no cover
        print(f"FATAL: cannot import update_pr_reuse_headers: {e}", file=sys.stderr)
        sys.exit(2)


def git_ls_files() -> List[str]:
    try:
        out = subprocess.check_output(["git", "ls-files"], cwd=REPO_ROOT, text=True, encoding='utf-8', errors='ignore')
        return [l for l in (ln.strip() for ln in out.splitlines()) if l]
    except Exception as ex:  # pragma: no cover
        print(f"Warning: git ls-files failed, walking FS ({ex})", file=sys.stderr)
        files: List[str] = []
        for root, _dirs, fs in os.walk(REPO_ROOT):
            if ".git" in root.split(os.sep):
                continue
            for f in fs:
                files.append(os.path.relpath(os.path.join(root, f), REPO_ROOT))
        return files


def load_license_rules() -> List[dict]:
    rules: List[dict] = []
    default_file = REPO_ROOT / ".reuse" / "path-licenses.json"
    if default_file.exists():
        try:
            with default_file.open('r', encoding='utf-8') as f:
                data = json.load(f)
            if isinstance(data, dict) and "rules" in data:
                for r in data["rules"]:
                    if isinstance(r, dict) and "pattern" in r and "license" in r:
                        rules.append({"pattern": str(r["pattern"]), "license": str(r["license"])})
        except Exception as ex:
            print(f"Warning: failed to read license rules: {ex}", file=sys.stderr)
    return rules


def license_for_path(path: str, fallback_id: str, rules: List[dict]) -> str:
    p = path.replace('\\', '/').lstrip('./')
    matches: List[tuple[int, str]] = []
    for r in rules:
        pat = r.get("pattern", "")
        lic = r.get("license", "")
        if pat and lic and fnmatch.fnmatchcase(p, pat):
            matches.append((len(pat), lic))
    if not matches:
        return fallback_id
    _, lic_label = max(matches, key=lambda t: t[0])
    return reuse._resolve_license_id(lic_label)  # type: ignore[attr-defined]


def extract_header_license(path: Path) -> str | None:
    try:
        with path.open('r', encoding='utf-8', errors='ignore') as f:
            # Read first 60 lines only
            lines = [next(f) for _ in range(60)]
    except StopIteration:
        pass  # fewer than 60 lines
    except FileNotFoundError:
        return None
    except Exception:  # pragma: no cover
        return None
    content = ''.join(lines) if 'lines' in locals() else ''
    for line in content.splitlines():
        if 'SPDX-License-Identifier:' in line:
            # Extract after colon
            parts = line.split('SPDX-License-Identifier:', 1)
            if len(parts) == 2:
                return parts[1].strip().lstrip('*').strip()
    return None


def normalize_license(expr: str | None) -> str | None:
    if not expr:
        return None
    # Collapse whitespace, uppercase AND/OR tokens to canonical form
    tokens = []
    for raw in expr.replace('\t', ' ').split():
        up = raw.upper()
        if up in ("AND", "OR"):
            tokens.append(up)
        else:
            tokens.append(raw)
    return ' '.join(tokens)


def check_files(files: List[str], fallback_label: str, fix: bool, filters: List[str] | None, excludes: List[str] | None, json_out: bool) -> int:
    supported_exts = set(reuse.COMMENT_STYLES.keys())
    rules = load_license_rules()
    fallback_id = reuse._resolve_license_id(fallback_label)  # type: ignore[attr-defined]

    if filters:
        filtered: List[str] = []
        for f in files:
            for gl in filters:
                if fnmatch.fnmatchcase(f, gl):
                    filtered.append(f)
                    break
        files = filtered

    # Apply exclusions
    if excludes:
        remaining: List[str] = []
        for f in files:
            skip = False
            for gl in excludes:
                if fnmatch.fnmatchcase(f, gl):
                    skip = True
                    break
            if not skip:
                remaining.append(f)
        files = remaining

    candidates = [f for f in files if Path(f).suffix in supported_exts]

    mismatches: List[Dict[str, Any]] = []
    checked = 0

    # Ensure we do not add ourselves when fixing
    if fix:
        os.environ["REUSE_SKIP_ADD_CURRENT"] = "true"
        os.environ["REUSE_FORCE_LICENSE"] = "true"

    for rel in candidates:
        path = REPO_ROOT / rel
        header_license = extract_header_license(path)
        if not header_license:
            continue  # skip files without existing header
        expected = license_for_path(rel, fallback_id, rules)
        norm_header = normalize_license(header_license)
        norm_expected = normalize_license(expected)
        if norm_header != norm_expected:
            entry = {
                "file": rel,
                "current": header_license,
                "expected": expected,
            }
            mismatches.append(entry)
            if fix:
                # Re-run process_file with expected license id
                try:
                    changed = reuse.process_file(rel, expected)  # type: ignore[attr-defined]
                    entry["fixed"] = bool(changed)
                except Exception as ex:  # pragma: no cover
                    entry["error"] = str(ex)
        checked += 1

    if json_out:
        print(json.dumps({
            "checked": checked,
            "mismatches": mismatches,
            "mismatch_count": len(mismatches),
            "fixed_count": sum(1 for m in mismatches if m.get("fixed")),
        }, indent=2))
    else:
        print(f"Checked files with headers: {checked}")
        if mismatches:
            print(f"Mismatches: {len(mismatches)}")
            for m in mismatches[:50]:  # limit display
                print(f"  {m['file']}: {m['current']} -> {m['expected']}" + (" (fixed)" if m.get("fixed") else ""))
            if len(mismatches) > 50:
                print(f"  ... {len(mismatches) - 50} more")
        else:
            print("All header licenses match expectations.")

    if mismatches and not fix:
        return 1
    if mismatches and fix and any(not m.get("fixed") for m in mismatches):
        return 1
    return 0


def main():  # pragma: no cover
    import argparse
    parser = argparse.ArgumentParser(description="Check existing SPDX headers against path-based license rules")
    parser.add_argument("--fix", action="store_true", help="Fix mismatched headers in place")
    parser.add_argument("--json", action="store_true", help="Output JSON report")
    parser.add_argument("--filter", nargs="*", default=None, help="Glob(s) to restrict files")
    parser.add_argument("--exclude", nargs="*", default=None, help="Glob(s) to exclude (applied after --filter)")
    parser.add_argument("--license", default=reuse.DEFAULT_LICENSE_LABEL, help="Fallback license label (default script value)")
    args = parser.parse_args()

    files = git_ls_files()
    rc = check_files(files, args.license, args.fix, args.filter, args.exclude, args.json)
    sys.exit(rc)


if __name__ == "__main__":  # pragma: no cover
    main()
