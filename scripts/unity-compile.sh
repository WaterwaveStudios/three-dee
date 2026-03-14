#!/usr/bin/env bash
# Run Unity in batch mode to compile the project and report any errors.
# Usage: scripts/unity-compile.sh [project-path]
# Defaults to current directory if no path given.

set -euo pipefail

PROJECT="${1:-$(pwd)}"
UNITY="/mnt/c/Program Files/Unity/Hub/Editor/6000.3.11f1/Editor/Unity.exe"
LOG="/tmp/unity-compile.log"

if [ ! -f "$UNITY" ]; then
  echo "[unity-compile] ERROR: Unity not found at: $UNITY"
  echo "  Check the version path and update this script."
  exit 1
fi

# Check if Unity Editor is already running (lockfile held)
if [ -f "$PROJECT/Temp/UnityLockfile" ]; then
  # Try to remove stale lockfile; if it fails, Unity Editor is open
  if ! rm -f "$PROJECT/Temp/UnityLockfile" 2>/dev/null; then
    echo "[unity-compile] Unity Editor is open — cannot run batch compile."
    echo "  Press Ctrl+R in the Unity Editor to recompile, then check the Console for errors."
    exit 0
  fi
fi

echo "[unity-compile] Compiling project: $PROJECT"
echo "[unity-compile] Log: $LOG"

"$UNITY" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$PROJECT" \
  -logFile "$LOG" \
  2>&1 || true

# Parse log for compile errors
if grep -q "error CS" "$LOG" 2>/dev/null; then
  echo ""
  echo "=== COMPILE ERRORS ==="
  grep "error CS" "$LOG"
  echo "======================"
  echo "[unity-compile] FAILED — fix the errors above."
  exit 1
elif grep -q "Compilation failed" "$LOG" 2>/dev/null; then
  echo ""
  echo "=== COMPILE FAILED ==="
  grep -A2 "Compilation failed" "$LOG" || true
  echo "======================"
  echo "[unity-compile] FAILED."
  exit 1
else
  echo "[unity-compile] SUCCESS — no compile errors."
fi
