#!/bin/bash
# Compile Unity project in batch mode and check for errors
# Usage: unity-compile.sh <project-path> [unity-editor-path]

PROJECT_PATH="${1:-.}"
UNITY="${2:-/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity}"
LOG="/tmp/unity-compile-$(date +%s).log"

# Remove stale lock file
rm -f "$PROJECT_PATH/Temp/UnityLockfile" 2>/dev/null

echo "Compiling Unity project at $PROJECT_PATH..."
"$UNITY" -projectPath "$PROJECT_PATH" -batchmode -quit -logFile "$LOG" 2>&1

EXIT_CODE=$?
ERRORS=$(grep "error CS" "$LOG" 2>/dev/null)

if [ -n "$ERRORS" ]; then
    echo "COMPILE ERRORS:"
    echo "$ERRORS"
    exit 1
elif grep -q "Exiting batchmode successfully" "$LOG"; then
    echo "Compilation successful"
    exit 0
else
    echo "Unity exited with code $EXIT_CODE — check log: $LOG"
    exit $EXIT_CODE
fi
