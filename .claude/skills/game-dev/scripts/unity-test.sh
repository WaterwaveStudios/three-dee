#!/bin/bash
# Run Unity tests in batch mode and report results
# Usage: unity-test.sh <project-path> [test-platform] [unity-editor-path]

PROJECT_PATH="${1:-.}"
PLATFORM="${2:-EditMode}"
UNITY="${3:-/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity}"
RESULTS="/tmp/unity-test-results-$(date +%s).xml"
LOG="/tmp/unity-test-$(date +%s).log"

# Remove stale lock file
rm -f "$PROJECT_PATH/Temp/UnityLockfile" 2>/dev/null

echo "Running $PLATFORM tests for $PROJECT_PATH..."
"$UNITY" -runTests -projectPath "$PROJECT_PATH" -testPlatform "$PLATFORM" \
    -testResults "$RESULTS" -batchmode -logFile "$LOG" 2>&1

# Parse results
if [ -f "$RESULTS" ]; then
    PASSED=$(grep -o 'passed="[0-9]*"' "$RESULTS" | head -1 | grep -o '[0-9]*')
    FAILED=$(grep -o 'failed="[0-9]*"' "$RESULTS" | head -1 | grep -o '[0-9]*')
    TOTAL=$(grep -o 'total="[0-9]*"' "$RESULTS" | head -1 | grep -o '[0-9]*')
    RESULT=$(grep -o 'result="[A-Za-z]*"' "$RESULTS" | head -1 | grep -o '"[A-Za-z]*"')

    echo "Result: $RESULT — $PASSED passed, $FAILED failed ($TOTAL total)"

    if [ "$FAILED" -gt 0 ] 2>/dev/null; then
        echo "FAILURES:"
        grep 'result="Failed"' "$RESULTS" | grep -o 'fullname="[^"]*"' | sed 's/fullname="//;s/"//'
        exit 1
    fi
    exit 0
else
    echo "No test results — check log: $LOG"
    ERRORS=$(grep "error CS\|Scripts have" "$LOG" 2>/dev/null)
    if [ -n "$ERRORS" ]; then
        echo "COMPILE ERRORS prevented test run:"
        echo "$ERRORS"
    fi
    exit 1
fi
