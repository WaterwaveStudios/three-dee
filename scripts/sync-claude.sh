#!/bin/bash
# Sync Claude config from .g (personal overlay) to .claude (shared repo config)
# Run this after updating skills or CLAUDE.md in .g to push changes to the repo
#
# Usage: ./scripts/sync-claude.sh

set -e

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
G_CLAUDE="$REPO_ROOT/.g/.claude"
CLAUDE_DIR="$REPO_ROOT/.claude"

if [ ! -d "$G_CLAUDE" ]; then
    echo "No .g/.claude directory found — nothing to sync."
    echo "This script is for maintainers who use the .g standard."
    exit 0
fi

echo "Syncing from .g/.claude → .claude ..."

# Sync CLAUDE.md
if [ -f "$G_CLAUDE/CLAUDE.md" ]; then
    cp "$G_CLAUDE/CLAUDE.md" "$CLAUDE_DIR/CLAUDE.md"
    echo "  ✓ CLAUDE.md"
fi

# Sync project-specific skills (add more as needed)
SKILLS="game-dev"

for skill in $SKILLS; do
    src="$G_CLAUDE/skills/$skill"
    dest="$CLAUDE_DIR/skills/$skill"

    if [ -d "$src" ]; then
        mkdir -p "$dest"
        cp -R "$src/"* "$dest/"
        echo "  ✓ skills/$skill"
    else
        echo "  ⚠ skills/$skill not found in .g — skipping"
    fi
done

echo "Done. Review changes with: git diff .claude/"
