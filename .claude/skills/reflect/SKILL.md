---
name: reflect
description: Reviews conversation history to identify documentation updates needed for future Claude sessions. Use at the end of coding sessions, after completing features, fixing bugs, discovering patterns, or when the user mentions updating docs, documenting learnings, reflecting on work, or wants to preserve context for future Claude runs. Use "/reflect issue" for focused mid-session fixes when something just went wrong.
---

# Reflect

Review the current conversation to identify learnings, patterns, and decisions that should be documented for future Claude sessions.

## Flow

```
┌─────────────────────────────────────────────────────┐
│ 1. SCAN          Scan conversation for work done    │
├─────────────────────────────────────────────────────┤
│ 2. BACKLOG       Update backlog items & progress.md │
├─────────────────────────────────────────────────────┤
│ 3. LEARNINGS     Store learnings in CLAUDE.md       │
├─────────────────────────────────────────────────────┤
│ 4. VERIFY        Re-read edited files, fix gaps     │
├─────────────────────────────────────────────────────┤
│ 5. CLAUDE.MD     Update repo documentation          │
├─────────────────────────────────────────────────────┤
│ 6. SKILLS        Skill-specific reflection           │
│   ├─ Has reflect.md? → Use targeted prompts         │
│   └─ No reflect.md?  → Brainstorm one with user     │
├─────────────────────────────────────────────────────┤
│ 7. REPORT        Summary of all changes             │
└─────────────────────────────────────────────────────┘
```

## Modes

### `/reflect` — Full end-of-session review
Comprehensive 7-step process. Use at the end of a session to sweep the entire conversation.

### `/reflect issue` — Focused mid-session fix
Something just went wrong or needs changing. Capture it now while the context is fresh, then move on.

**Flow:**
1. **Identify** — Scan the preceding conversation (last few exchanges) to find what happened. The user doesn't need to describe it — look for corrections, mistakes, user frustrations, workflow failures, or "that shouldn't have happened" moments.
2. **Fix** — Update the relevant skill/doc/CLAUDE.md + store a learning if warranted. Same rules as the full reflect (auto-apply clear wins, prompt for behavioural changes).
3. **Confirm** — Brief one-liner summary of what changed. No full report template.

**Skip**: Full conversation scan, backlog review, verify-all-files, brainstorm-new-reflect.md, comprehensive report. This mode is fast and focused.

## When to Use This Skill

- `/reflect` — End of a significant coding session
- `/reflect issue` — Mid-session, right after something goes wrong or the user corrects you
- After discovering undocumented patterns or conventions
- After resolving tricky bugs with non-obvious solutions
- When the user mentions "document this for future" or "remember this"

## Documentation Locations

| Location | Scope | Purpose |
|----------|-------|---------|
| `.claude/BACKLOG.md` | This repo | **PRIMARY** - Track work progress |
| `.claude/CLAUDE.md` | This repo | Repo-specific context |
| `.claude/skills/*/SKILL.md` | This repo | Project skills |

**Critical**: Backlog updates are the most important part of this skill. They ensure continuity between sessions.

## Review Process

### Step 1: Scan Conversation and Identify Work Done

Look for:

1. **Backlog items worked on** - What tasks from the backlog were addressed?
2. **Patterns Discovered** - Code patterns, naming conventions, file organization
3. **Decisions Made** - Architectural choices with rationale, trade-offs
4. **Gotchas and Workarounds** - Non-obvious bugs, framework limitations
5. **Commands and Workflows** - Build/test/deploy commands, debug procedures
6. **External Dependencies** - API quirks, third-party configurations

### Step 2: Update Backlog (MANDATORY if backlog exists)

**This step is required, not optional.** Check for `.claude/BACKLOG.md`.

#### 2a. Check What Work Was Done

1. **Read BACKLOG.md** - This is the source of truth for backlog status
2. **Identify if any items were worked on** this session
3. **If no backlog work was done** (e.g., skill improvement, research), skip to Step 3

#### 2b. Update Related Backlog Items (only if work was done)

1. **Check off completed items** - `[ ]` → `[x]`
2. **Update descriptions** if scope changed during implementation

#### 2c. Add New Items

If new work was identified during the session, add it to the backlog.

### Step 3: Store Learnings

Learnings go directly into `CLAUDE.md` or skill-specific files. For this project:

- **Unity gotchas** → `.claude/skills/game-dev/references/unity-gotchas.md`
- **Project conventions** → `.claude/CLAUDE.md`
- **Game architecture patterns** → `.claude/skills/game-dev/SKILL.md`

### Step 4: Verify Session Changes Are Complete

**This step catches gaps in work done during the session.** Don't just document what happened — verify that what was written is actually correct and complete.

For every file **created or edited** during the session (skills, agents, configs, workflow files — not application code):

1. **Re-read the file** in full
2. **Check for incomplete logic** — are all branches/paths handled? Are there TODO-like gaps?
3. **Check for consistency** — do references match between files that interact?
4. **Auto-fix** any gaps found

### Step 5: Read and Update CLAUDE.md

Find and read the repo's CLAUDE.md:

- Add new patterns/gotchas discovered
- Update outdated information
- Fill documentation gaps

**Auto-apply changes that are clearly beneficial.**

| Finding | Action |
|---------|--------|
| New pattern/gotcha | Add to CLAUDE.md |
| Outdated info | Update existing section |
| Issue found but not fixed | Create new backlog item |
| Reusable workflow | **Prompt user first**, then create skill |
| One-off fix | Skip documenting |

### Step 6: Skill-Specific Reflection

Each skill can have a `reflect.md` file containing targeted questions and prompts for what to look for in the conversation.

#### 6a. Identify Skills Used or Mentioned

Scan the conversation for skills explicitly invoked or relevant to the work done.

#### 6b. Read reflect.md for Each Skill

Check `.claude/skills/{skill-name}/reflect.md`. If found, use the prompts to review the conversation.

#### 6c. Apply Findings

| Finding type | Action |
|-------------|--------|
| New example or edge case | Add to SKILL.md |
| Missing instruction | Add to SKILL.md |
| New reflect prompt discovered | Add to reflect.md |
| Behavioural change needed | **Prompt user first** |

#### 6d. Skills Without reflect.md

If a skill was used but has no `reflect.md`, **brainstorm one with the user** before moving on.

### Step 7: Report Summary

```markdown
# Documentation Review

## Session Summary
[What was accomplished]

## Backlog Updates
- **Items updated**: [item names and status changes]
- **New items created**: [any new backlog items]

## Learnings
- [List of learnings saved to CLAUDE.md or skill files]

## CLAUDE.md Updates
- [List of updates, or "No updates needed"]

## Skill-Specific Reflection
- **{skill-name}**: [Findings from targeted prompts]

## Skill Updates
- [List of skill files updated, or "No updates needed"]
```

## What to Document vs Skip

### DO Document

- Recurring patterns specific to this codebase
- Non-obvious conventions ("Use X because Y")
- Environment/build quirks
- Testing patterns and requirements
- Dependency compatibility issues

### DON'T Document

- Standard language conventions
- One-time fixes (typos, simple bugs)
- Temporary workarounds (unless they'll persist)
- Sensitive information (credentials, secrets)

## Skills Management

### When to Create a New Skill

Create a skill when a workflow is:
- **Reusable** - applies across the project, not a one-off
- **Multi-step** - involves a sequence of actions
- **Non-obvious** - requires domain knowledge or specific patterns
- **Frequently used** - you've done it multiple times

**IMPORTANT: Always Prompt Before Creating Skills.** Never auto-create skills.

### When to Update Existing Skills

**Auto-apply** typo fixes, clarifications, examples.
**Prompt user first** for changes to core behaviour.

### Skill Structure

Skills live in `.claude/skills/{skill-name}/SKILL.md`.

### Skills vs CLAUDE.md

| Use Skill | Use CLAUDE.md |
|-----------|---------------|
| Multi-step workflow | Single convention |
| Needs user interaction | Static reference |
| Has trigger phrases | Always-on context |

## Documentation Style Guide

**Keep it concise** - actionable and brief, not verbose explanations.
**Use code examples** - show exactly what to do.
**Include the "why"** - rationale helps future decisions.

## Anti-Patterns

| Don't | Do Instead |
|-------|------------|
| Document everything | Focus on non-obvious learnings |
| Duplicate external docs | Link to official docs |
| Write lengthy explanations | Keep it brief and actionable |
| Include sensitive data | Reference secrets by name only |
| Hesitate on clear wins | Auto-apply beneficial updates |
