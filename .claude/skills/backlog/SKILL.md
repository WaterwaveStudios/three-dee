---
name: backlog
description: Review and select feature requests from the project backlog. Use when user says "/backlog", asks about pending work, or wants to see what tasks are waiting.
args: "[update|verify|add <jira_url>]"
---

# Backlog Skill

Review and select feature requests from the project backlog.

## Trigger

- User says `/backlog` - show and select from pending items
- User says `/backlog update` - scan conversation and add outstanding work to backlog
- User says `/backlog verify` - verify acceptance criteria for pending items
- User says `/backlog add <jira_url>` - create backlog item from Jira ticket
- User asks "what's in the backlog?" or "show me pending work"

## Behavior (No Arguments)

When `/backlog` is called with no arguments, read the project's backlog file.

**This project uses `.claude/BACKLOG.md`** — a flat markdown checklist grouped by priority sections with `[x]` (done) and `[ ]` (pending) items.

1. **Read `.claude/BACKLOG.md`** (the project backlog file)
   - Parse ALL items: completed (`[x]`) and pending (`[ ]`)
   - Group by priority section (Priority 1, Priority 2, etc.)

2. **Present ALL items** via `AskUserQuestion`:
   - Show every item — completed and pending — so the user has full context
   - Completed items should be visually distinguishable (e.g. "✅ Done" in description)
   - Pending items are selectable to start work on
   - Always include a "Skip" option

3. **If user selects a pending item**:
   - Present the full context and begin work on that item

## This Project: BACKLOG.md

**This project does NOT use progress.md or individual backlog files.** All backlog tracking is in `.claude/BACKLOG.md` — a simple flat checklist. Update `[x]` when items are done. The sections below about progress.md are from the generic skill template and do NOT apply here.

---

## Keeping progress.md Updated (generic — does not apply to this project)

**progress.md is the source of truth** for quick backlog status. It MUST be kept current.

### When to Update progress.md

- **After completing a backlog item** - move from Active/Pending to Completed
- **After selecting an item to work on** - move from Pending to Active
- **After creating new backlog items** - add to Pending list
- **At the end of `/backlog` skill execution** - always verify it's current
- **When `/update-claude-docs` is called** - check if any work was done that should update backlog status

### progress.md Format

```markdown
# Backlog Progress

## Active
1. **item-filename** - Brief description of what's being worked on

## Pending
1. **item-filename** - Brief description (priority: high/medium/low)
2. **another-item** - Another description

## Completed
- **item-filename** - Brief completion note

## Next Up
**Recommended item** - Why this should be next:
- Reasoning point 1
- Reasoning point 2
```

**Numbering**: Active and Pending sections use numbered lists so users can select items by number. Completed section uses bullet points (no selection needed).

### Integration with /update-claude-docs

When `/update-claude-docs` runs at the end of a session, it should:
1. Check if any backlog items were worked on during the session
2. Update progress.md if items were completed or status changed
3. Update the "Next Up" recommendation based on current state

## Full Behavior (Subcommands)

For `/backlog verify`, `/backlog update`, and `/backlog add`, the full file scanning behavior applies:

1. **Check backlog directory exists** at `.g/.claude/backlog/`
   - **IMPORTANT**: Check if directory exists BEFORE any creation attempt (it may be a symlink to a global location)
   - Only create with `mkdir -p` if it truly doesn't exist
   - Never blindly run mkdir - this can create a new directory alongside an existing symlink

2. **Scan the backlog directory** (only for subcommands that need it)

3. **Parse each markdown file** to extract:
   - Title (from `# heading`)
   - Status (pending/in-progress/completed/abandoned)
   - Priority (high/medium/low)
   - Created date (from filename or metadata)
   - Brief problem summary
   - Acceptance criteria (checkboxes under ## Acceptance Criteria)

4. **Check Definition of Done (DoD)** from dev skill (for verify):
   - Read `~/.g/global/skills/dev/SKILL.md`
   - Find current repo in the Repo Map section
   - Extract DoD criteria if present
   - These criteria are ADDITIONAL to the backlog item's acceptance criteria

5. **Always update progress.md** after any changes

## Output Format (No Arguments)

Use `AskUserQuestion` to present a selection UI. Show **ALL items** from the backlog — completed and pending.

**Option construction:**
- **Every item** from BACKLOG.md becomes an option (completed and pending alike)
- Label: the item name/title
- Description: include the priority section and status. For completed items prefix with "✅ Done — ". For pending items prefix with "⬜ Pending — "
- Add a final "Skip" option with description "Don't select an item right now"
- AskUserQuestion supports max 4 options, so if there are more items, group or summarize as needed

**Example AskUserQuestion call (for this project):**
```
AskUserQuestion(
  questions: [{
    question: "Which backlog item do you want to work on?",
    header: "Backlog",
    options: [
      { label: "Building Placement (Recommended)", description: "⬜ Pending — Priority 2: Tap-to-place buildings on grid cells" },
      { label: "Mobile Build", description: "⬜ Pending — Priority 3: Test on actual iOS/Android device" },
      { label: "Zombie AI", description: "✅ Done — Priority 1: NavMesh pathfinding enemies" },
      { label: "Skip", description: "Don't select an item right now" }
    ],
    multiSelect: false
  }]
)
```

**Note**: Since AskUserQuestion only supports 2-4 options, if there are many backlog items, show only the pending ones plus 1-2 recently completed ones, always filling the last slot with "Skip".

## Implementation Steps (No Arguments)

```
1. Read .claude/BACKLOG.md
   - Parse ALL items (both [x] completed and [ ] pending)
   - Note which priority section each item belongs to

2. Build the selection UI:
   - Include ALL items — pending and completed
   - Pending items: label with "(Recommended)" on the top pending item
   - Completed items: prefix description with "✅ Done — "
   - Since AskUserQuestion caps at 4 options, show the top pending items + "Skip"
     (if items exceed 3, prioritize pending over completed)
   - Always include "Skip" as the last option

3. Present items via AskUserQuestion selection UI
   - DO NOT print a text summary — let the selection UI be the presentation

4. If user selects a pending item (not "Skip"):
   - Begin work on that item
   - Update BACKLOG.md to mark item as [x] when complete
```

## Implementation Steps (Subcommands)

For `/backlog verify`, `/backlog update`, `/backlog add`:

```
1. Check if .g/.claude/backlog/ directory exists using `test -d` or `ls`
   - If it exists (including as symlink), proceed
   - If it truly doesn't exist, create it with `mkdir -p`

2. Use Glob with ABSOLUTE path to find all .md files (excluding README.md and progress.md)
   - IMPORTANT: Glob doesn't work reliably with relative dot-paths
   - Use pattern: `*.md` with path: `/absolute/path/to/project/.g/.claude/backlog`

3. Read each file and extract metadata as needed for the subcommand

4. Perform subcommand-specific logic (verify/update/add)

5. ALWAYS update progress.md after any changes
```

## Example Session

**User**: /backlog

**Claude**: *Reads ONLY progress.md*
```
## Backlog Status

**Active**: (none)

**Pending**:
1. fix-mockserver - Fix Local Load Tests Mockserver (high)
2. complete-mocks - Complete Local Load Test Mocks (medium)

**Completed** (recent):
- setup-ci - CI pipeline working

### Next Up
**fix-mockserver** - High priority, blocking local development

Enter a number to start, or "skip":
```

**User**: 1

**Claude**: *NOW reads the specific backlog file, updates progress.md, begins work*
```
Loading 2026-01-13-fix-mockserver.md...

# Fix Local Load Tests Mockserver

**Status**: in-progress (was: pending)
**Priority**: high

## Problem
Local load tests fail because mockserver stubs are incomplete...

[Full backlog item content shown]
```

## Update Subcommand (`/backlog update`)

When invoked with `update`, scan the conversation and work to add outstanding items to the backlog.

### Update Steps

1. **Ensure backlog directory exists** at `.g/.claude/backlog/`

2. **Scan for outstanding work** by reviewing:
   - Current conversation history for mentioned but incomplete tasks
   - Git status for uncommitted changes that represent WIP
   - Any TODOs, FIXMEs, or "we should also..." mentioned
   - Features started but not finished
   - Known issues discovered during work

3. **For each item found**, check if it already exists in the backlog:
   - Search existing `.g/.claude/backlog/*.md` files for similar titles/problems
   - Skip if already tracked

4. **Create backlog items** for new outstanding work:
   - Filename format: `YYYY-MM-DD-short-description.md`
   - Use the backlog item template below
   - Set appropriate priority based on context

5. **Report what was added** to the user

### Backlog Item Template

```markdown
# [Title - brief description of the task]

**Status**: pending
**Priority**: [high|medium|low]
**Created**: [YYYY-MM-DD]

## Problem

[What needs to be done and why. Include context from the conversation.]

## Context

[Relevant files, related work, dependencies, or background information.]

## Acceptance Criteria

- [ ] [Specific, verifiable criterion]
- [ ] [Another criterion]
- [ ] Tests pass

## Session Log

[Updated by /update-claude-docs at end of each session. Tracks what's done, remaining, decisions, and uncertainties.]
```

### Priority Guidelines

- **high**: Blocking other work, broken functionality, or explicitly urgent
- **medium**: Should be done soon, affects user experience or code quality
- **low**: Nice to have, improvements, or can wait

### Update Notes

- Be specific in acceptance criteria so `/backlog verify` can auto-check them
- Include file paths and function names where relevant
- Link related backlog items if they exist
- Don't create items for trivial tasks that will be done immediately

## Verify Subcommand (`/backlog verify`)

When invoked with `verify`, check acceptance criteria for pending backlog items, report which can be auto-completed, and **reconcile progress.md with actual backlog state**.

### Verify Steps

1. **Scan the backlog directory** for all `.md` files (excluding README.md and progress.md)
2. **Reconcile progress.md** with actual file state:
   - Compare items listed in progress.md against actual files
   - Detect mismatches: files missing from progress.md, items in wrong section, deleted files still listed
   - Auto-fix progress.md to reflect reality
3. **For each pending item**, read its acceptance criteria
4. **Classify each criterion** as verifiable or manual
5. **Check verifiable criteria** against the codebase
6. **Report results** with clear pass/fail for each criterion
7. **Auto-complete items** where ALL criteria are verified (with user confirmation)

### Progress.md Reconciliation

Before verifying acceptance criteria, ensure progress.md accurately reflects the backlog:

| Situation | Action |
|-----------|--------|
| File exists but not in progress.md | Add to appropriate section based on status field |
| Item in progress.md but file deleted | Remove from progress.md |
| Item in wrong section (e.g., Pending but status=completed) | Move to correct section |
| Status field in file differs from progress.md section | Trust file's status field, update progress.md |

**Reconciliation output:**
```markdown
### Progress.md Reconciliation

| Change | Item | Details |
|--------|------|---------|
| Added | new-feature.md | Found file with status=pending, added to Pending |
| Moved | old-task.md | Status=completed but was in Pending, moved to Completed |
| Removed | deleted-item | Listed in progress.md but file no longer exists |

progress.md updated with 3 changes.
```

If no changes needed: "progress.md is in sync with backlog files."

### Verifiable Criteria Types

These criteria CAN be automatically verified by checking the codebase:

| Type | Pattern | How to Verify |
|------|---------|---------------|
| Environment variable | "Add ENV_VAR to config" | Grep for env var in config files |
| Function/class exists | "Create UserService class" | Grep for class/function definition |
| File exists | "Create config/settings.yaml" | Glob for file path |
| Config key | "Add retry.maxAttempts setting" | Grep for config key |
| Import/dependency | "Add axios dependency" | Check package.json/build.sbt |
| Route/endpoint | "Add GET /api/users endpoint" | Grep for route definition |
| Documentation | "Document in README" | Grep for section heading |

### Non-Verifiable Criteria Types

These criteria CANNOT be auto-verified (require manual testing or subjective review):

| Type | Pattern | Why Manual |
|------|---------|------------|
| Runtime behavior | "Service handles errors gracefully" | Requires execution |
| Test results | "All tests pass" | Requires running test suite |
| External systems | "Integrates with Slack API" | Requires live connection |
| Performance | "Response time < 100ms" | Requires benchmarking |
| UX/visual | "UI looks correct" | Subjective assessment |
| Security | "No SQL injection vulnerabilities" | Requires security audit |

### Verification Logic

For each acceptance criterion:

1. **Parse the criterion text** to identify what it's checking
2. **Determine verification method**:
   - File existence → Use Glob
   - Code pattern → Use Grep
   - Config value → Read config file and check
3. **Execute verification** and record result
4. **Handle ambiguity**: If unclear what to check, mark as "manual review needed"

### Output Format

```markdown
## Backlog Verification Report

### Progress.md Reconciliation

| Change | Item | Details |
|--------|------|---------|
| Moved | old-task.md | Status=completed but was in Pending |

progress.md updated with 1 change.

---

### Item: "Add user authentication"

| Criterion | Verifiable | Result |
|-----------|------------|--------|
| Create AuthService class | ✅ Yes | ✅ PASS - Found in src/services/auth.ts:15 |
| Add JWT_SECRET env var | ✅ Yes | ✅ PASS - Found in .env.example |
| Handle token expiry gracefully | ❌ No | ⏸️ Manual - Requires runtime testing |
| Tests pass | ❌ No | ⏸️ Manual - Requires test execution |

**Verifiable criteria**: 2/2 passed
**Manual criteria**: 2 remaining

### Item: "Fix login timeout"

| Criterion | Verifiable | Result |
|-----------|------------|--------|
| Increase timeout to 30s | ✅ Yes | ❌ FAIL - timeout still 10s in config/api.yaml |
| Add retry logic | ✅ Yes | ✅ PASS - Found in src/api/client.ts:42 |

**Verifiable criteria**: 1/2 passed
**Status**: Cannot auto-complete (criteria failing)
```

### Auto-Completion Rules

An item can be auto-completed ONLY when:
1. ALL verifiable criteria pass
2. There are NO failing criteria
3. Non-verifiable criteria are either:
   - Explicitly marked as "deferred" in the item
   - Or user confirms they've been manually verified

When auto-completing:
1. Update the item's status to `completed`
2. Add completion timestamp
3. Note which criteria were auto-verified vs manually confirmed
4. Update `progress.md` with the completion

## Add Subcommand (`/backlog add <jira_url>`)

When invoked with `add <jira_url>`, open the Jira ticket in Chrome using browser automation, extract the ticket details automatically, and create a local backlog item.

### Add Steps

1. **Validate the URL**
   - Must be a valid URL containing a Jira-like path (e.g., `/browse/`, `/jira/`)
   - Common patterns: `https://jira.example.com/browse/PROJ-123` or `https://example.atlassian.net/browse/PROJ-123`

2. **Extract Jira key from URL**
   - Use regex: `(?:browse|jira/browse)/([A-Z]+-\d+)` to extract the key
   - This will be used in the filename and metadata

3. **Open in Chrome using browser automation**
   - First, use `ToolSearch` to load Chrome MCP tools: `select:mcp__claude-in-chrome__navigate`
   - Create a new tab with `mcp__claude-in-chrome__tabs_create_mcp`
   - Navigate to the Jira URL with `mcp__claude-in-chrome__navigate`
   - Wait for page to load (Jira may require authentication - user should already be logged in)

4. **Extract ticket information from page**
   - Use `mcp__claude-in-chrome__get_page_text` to get the page content
   - Parse the following fields from the Jira page:
     - **Summary/Title**: Usually in an `h1` or element with `data-testid="issue.views.issue-base.foundation.summary.heading"`
     - **Description**: Content under the description section
     - **Priority**: Look for priority indicator (Highest, High, Medium, Low, Lowest)
     - **Acceptance Criteria**: Often in the description or a custom field
     - **Story Points**: If present
     - **Confluence Links**: Look for links to `confluence.tools.tax.service.gov.uk` in the description
   - If extraction fails, fall back to asking the user (see Fallback section)

5. **Follow Confluence links (if found)**
   - If the Jira ticket contains Confluence links (e.g., screen specs, business function docs):
     - Extract the document code from the URL (e.g., `R2`, `F14`, `A2.11`)
     - Check if `charities-wiki` skill has this document synced locally
     - If synced, invoke `/charities-wiki about [doc-code]` to get the spec details
     - Add the spec URL and key details to the backlog item's Context section
   - This enriches the backlog item with domain knowledge automatically

6. **Create the backlog item**
   - Ensure backlog directory exists at `.g/.claude/backlog/`
   - Generate filename: `YYYY-MM-DD-{jira-key}-{short-title}.md`
   - Use the backlog item template with extracted information
   - Include Confluence spec link and key details in Context section if found

7. **Confirm creation** to the user with a summary of extracted details

### Jira Field Extraction Patterns

Different Jira instances have different DOM structures. Try these patterns:

| Field | Selectors/Patterns to Try |
|-------|---------------------------|
| Summary | `h1`, `[data-testid*="summary"]`, `.issue-header-content` |
| Description | `[data-testid*="description"]`, `.user-content-block`, `#description-val` |
| Priority | `[data-testid*="priority"]`, `.priority-view`, text containing "Priority:" |
| Status | `[data-testid*="status"]`, `.status-view`, text containing "Status:" |
| Acceptance Criteria | Look in description for "AC:", "Acceptance Criteria", or custom fields |

### Fallback: Manual Input

If automatic extraction fails (page didn't load, auth required, DOM structure unknown):

1. Inform user: "I couldn't automatically extract the ticket details. The page may require authentication or has an unexpected structure."
2. Use AskUserQuestion to prompt for:
   - Ticket title/summary
   - Description/problem statement
   - Acceptance criteria (if listed)
   - Priority (default to medium)

### Jira Backlog Template

```markdown
# [Title from Jira ticket]

**Status**: pending
**Priority**: [high|medium|low - from Jira or default to medium]
**Created**: [YYYY-MM-DD]
**Jira**: [JIRA-KEY](jira_url)

## Problem

[Description from Jira ticket]

## Context

[Additional context, related tickets, or dependencies mentioned]

**Confluence Spec**: [If a Confluence link was found in the Jira ticket, include it here]
- URL: [confluence_url]
- Local doc: `/charities-wiki about [doc-code]` [if synced locally]
- Key details from spec: [brief summary from charities-wiki]

## Acceptance Criteria

- [ ] [Criterion from Jira]
- [ ] [Another criterion]
- [ ] Tests pass
```

### Example Session (Automatic Extraction)

**User**: /backlog add https://jira.example.com/browse/CHAR-456

**Claude**:
```
Opening Jira ticket CHAR-456 in Chrome...
```
*Uses Chrome MCP tools to navigate and extract*

```
Extracted ticket details from Jira:

- **Title**: Add validation for charity registration number
- **Priority**: Medium
- **Description**: The registration number field should validate format before submission
- **Acceptance Criteria**:
  - Format must be 7 digits
  - Show error message if invalid

Creating backlog item...
```

```
Created backlog item: `.g/.claude/backlog/2026-02-02-CHAR-456-registration-number-validation.md`

## Summary

- **Title**: Add validation for charity registration number
- **Priority**: medium
- **Jira**: [CHAR-456](https://jira.example.com/browse/CHAR-456)
- **Acceptance Criteria**: 2 items + tests pass
```

### Example Session (Fallback to Manual)

**User**: /backlog add https://jira.example.com/browse/CHAR-789

**Claude**:
```
Opening Jira ticket CHAR-789 in Chrome...
```
*Navigates but can't extract details*

```
I couldn't automatically extract the ticket details - the page may require you to log in first.

Please provide the following from the ticket:
1. What is the ticket title/summary?
2. What is the problem description?
3. Are there acceptance criteria listed?
4. What priority is it? (high/medium/low)
```

### URL Parsing

Extract the Jira key from common URL patterns:

| Pattern | Example | Key |
|---------|---------|-----|
| `/browse/KEY-123` | `jira.example.com/browse/CHAR-456` | `CHAR-456` |
| `/jira/browse/KEY-123` | `example.com/jira/browse/PROJ-789` | `PROJ-789` |
| Atlassian Cloud | `example.atlassian.net/browse/KEY-123` | `KEY-123` |

Use regex: `(?:browse|jira/browse)/([A-Z]+-\d+)` to extract the key.

### Chrome Browser Prerequisites

For automatic extraction to work:
- Chrome must be running with the Claude-in-Chrome extension connected
- User should be logged into Jira in Chrome (the extension uses the existing browser session)
- If Chrome is not available, fall back to manual input

### Notes

- The Jira link is preserved in the backlog item for reference
- Automatic extraction uses the Chrome MCP tools (`mcp__claude-in-chrome__*`)
- If extraction fails, always fall back gracefully to asking the user
- If no acceptance criteria found in Jira, suggest some based on the description
- Always include "Tests pass" as a default criterion

## DoD Verification

The backlog skill should check the Definition of Done (DoD) from the dev skill's Repo Map in addition to the backlog item's acceptance criteria.

### How to Find DoD

1. **Determine current repo name** from git remote or directory structure
2. **Read dev skill**: `~/.g/global/skills/dev/SKILL.md`
3. **Find repo in Repo Map section**: Look for `### {repo-name}` heading
4. **Extract DoD criteria**: Find `**Definition of Done:**` followed by checkboxes

### Example Repo Map Entry

```markdown
### charities-claims-frontend

**Definition of Done:**
- [ ] All tests pass (`sbt clean coverage test it/test`)
- [ ] UI validated locally via `/charities-claims-ui-agent`
```

### DoD Criteria Classification

| DoD Criterion | Verifiable | How to Check |
|---------------|------------|--------------|
| "All tests pass" | ⚠️ Partially | Can run test command, but requires execution |
| "UI validated via agent" | ❌ No | Requires running UI agent - manual check |
| "No linting errors" | ✅ Yes | Can run lint command |
| "Code reviewed" | ❌ No | Requires human review |

### Auto-Completion with DoD

Items can ONLY be auto-completed when:
1. ALL acceptance criteria pass verification
2. ALL verifiable DoD criteria pass
3. No manual DoD checks are required (or user confirms they've been done)

If the DoD includes manual checks like "UI validated":
- **DO NOT auto-complete** the item
- Report: "⚠️ DoD requires manual verification: UI validation"
- Ask user to confirm manual checks before completing

### Output Format with DoD

```markdown
## Verification Report

### Item: "Fix login bug"

**Acceptance Criteria:**
| Criterion | Result |
|-----------|--------|
| LoginController handles timeout | ✅ PASS |
| Tests added | ✅ PASS |

**Definition of Done (from dev skill):**
| Criterion | Result |
|-----------|--------|
| All tests pass | ⏸️ Manual - requires `sbt test` |
| UI validated via agent | ⏸️ Manual - run `/charities-claims-ui-agent` |

**Status:** Cannot auto-complete - DoD requires manual verification
```

### If No DoD Found

If the current repo is not in the dev skill's Repo Map:
- Use the "Default (no specific mapping)" section if present
- Otherwise, proceed with acceptance criteria only
- Note in output: "No repo-specific DoD found in dev skill"

## Notes

- Always preserve the original backlog file format when updating status
- If no pending items exist, inform user and suggest running `/backlog update` to capture any new work
- Completed/abandoned items can be shown with a `--all` flag if user asks

### CRITICAL: Keep progress.md Updated

**progress.md is the single source of truth** for quick backlog status checks. Failing to keep it updated defeats the purpose of the context-saving optimization.

**ALWAYS update progress.md when:**
1. Completing a backlog item → move to Completed list
2. Starting work on an item → move to Active list
3. Creating new items via `/backlog update` → add to Pending list
4. At the END of any `/backlog` skill execution
5. When `/update-claude-docs` detects backlog work was done

**If progress.md gets out of sync**, the default `/backlog` invocation will show stale data. Run `/backlog verify` to reconcile - it will scan all backlog files and update progress.md to match the actual state.

### Auto-verification Guidelines

See "Verify Subcommand" section for full verification logic and criteria types.

**Quick reference - Can auto-verify:**
- Environment variables, functions/classes, file existence, config keys, documentation

**Cannot auto-verify:**
- Runtime behavior, test results, external systems, subjective assessments

### Glob Tool Quirk

The Glob tool doesn't work with relative paths starting with `.` (dot directories):

```bash
# FAILS - returns "No files found"
Glob(pattern=".g/.claude/backlog/*.md")

# WORKS - use absolute path parameter
Glob(pattern="*.md", path="/absolute/path/to/project/.g/.claude/backlog")
```

Always use the absolute `path` parameter when scanning the backlog directory.

## Spike Backlog Items

Spikes are exploratory tasks that produce a **decision document** rather than production code. When creating or working with spike backlog items, follow these guidelines.

### Spike vs Feature Items

| Aspect | Feature Item | Spike Item |
|--------|--------------|------------|
| **Output** | Production code merged to main | Decision document (ADR) |
| **Code** | Must be production-ready | POC/throwaway code (can stay in branch) |
| **Acceptance** | Code works, tests pass | Decision made, options evaluated |
| **Verification** | Can auto-verify via codebase | Manual review of decision doc |

### Spike Output Section

Spike backlog items should include a **## Spike Output** section describing the expected deliverable:

```markdown
## Spike Output

**Primary deliverable:** Decision document (ADR) at `.g/.claude/decisions/` or Confluence

The decision document should contain:
1. **Summary of each approach** with working POC code (can be in a branch, not merged)
2. **Comparison matrix** - testability, maintainability, effort, risk
3. **Recommendation** - which approach to use and why
4. **Effort estimate** - story points or days for full implementation
5. **Next steps** - follow-up backlog items for the chosen approach

**Note:** POC code from this spike is throwaway/exploratory - it does NOT need to be production-ready or merged to main.
```

### Spike Backlog Template

When creating a spike backlog item, use this structure:

```markdown
# [Topic] Spike

**Status**: pending
**Priority**: medium
**Created**: YYYY-MM-DD
**Type**: spike

## Problem

[What question are we trying to answer? What decision needs to be made?]

## Options to Evaluate

### Option A: [Name]
[Brief description, pros, cons]

### Option B: [Name]
[Brief description, pros, cons]

## Spike Scope

[What specific investigation work will be done? Keep it time-boxed.]

## Spike Output

**Primary deliverable:** Decision document at [location]

The decision document should contain:
1. Summary of each approach with POC code
2. Comparison matrix
3. Recommendation with justification
4. Effort estimate for full implementation
5. Follow-up backlog items

**Note:** POC code is throwaway - does NOT need to be merged.

## Acceptance Criteria

- [ ] Option A evaluated with POC
- [ ] Option B evaluated with POC
- [ ] Comparison matrix created
- [ ] Recommendation documented with justification
- [ ] Follow-up backlog item(s) created for chosen approach
```

### Key Reminders for Spikes

- **POC code is throwaway** - It can live in a branch and doesn't need to be merged
- **Output is a decision** - The value is the analysis and recommendation, not the code
- **Time-box the exploration** - Spikes should have a defined scope to prevent endless investigation
- **Create follow-up items** - The spike should result in concrete backlog items for implementation
