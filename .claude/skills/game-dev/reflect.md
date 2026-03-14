# Game Dev — Reflection Prompts

## Look For
- Were there new Unity gotchas discovered (runtime errors, silent failures, editor quirks)? Add to references/unity-gotchas.md
- Did a feature get implemented without tests? This violates the workflow — check why and reinforce
- Were there compile errors that only showed up in batch mode but not in the editor? Capture the pattern
- Did a new game architecture pattern emerge (beyond grid/match-3) that should be documented?
- Were the bundled scripts (unity-compile.sh, unity-test.sh) used? Did they work correctly or need updates?

## Update SKILL.md When
- A new debugging pattern is discovered (add to Debugging section)
- A new game architecture pattern is implemented (add to Game Architecture Patterns)
- The Unity version changes (update CLI paths in examples)
- New asset templates are needed (add to assets/ and document in Resources)

## Examples to Capture
- New Unity gotchas with reproduction steps and fixes
- Successful test patterns for MonoBehaviour logic
- Game systems that worked well as plain C# classes vs MonoBehaviours
