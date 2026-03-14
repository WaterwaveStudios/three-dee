# Backlog

## Priority 1 — Foundation

- [x] **3D Rendering Pipeline** — Get URP rendering working with proper lighting, shadows, and camera setup for a top-down/isometric strategy view
- [x] **Camera System** — Isometric camera with pan (WASD/mouse) and zoom (scroll wheel)
- [x] **Base Grid System** — 10x10 grid with checkerboard ground and placeholder buildings
- [x] **AI 3D Asset Generation** — Meshy AI for unit models, texture import pipeline with quality settings
- [x] **Asset Import Pipeline** — Meshy FBX → Resources/Models/ with auto texture import, URP Lit materials
- [x] **Mobile Touch Controls** — Pinch-to-zoom, two-finger pan, rotate
- [x] **Migrate to New Input System** — Replace deprecated Input Manager with Unity Input System package
- [x] **Player Unit Control** — WASD moves unit with kinematic movement, camera follows, Meshy model rotation fixed
- [x] **Fix WASD movement** — Switched to `Rigidbody.MovePosition` in `FixedUpdate`; camera-relative movement (W=screen-up, D=screen-right); `applyRootMotion=false` to stop animation fighting movement
- [x] **Animation System** — AnimatorController auto-created via AssetPostprocessor; idle/walk wired to `IsMoving` bool; walk clip loops set in `OnPostprocessAnimation`; **requires Reimport of FBX in Unity to apply**
- [x] **Zombie AI** — 11 zombies chase the Explorer using NavMeshAgent pathfinding; `ZombieController` drives rotation and `IsMoving` animation param; staggered avoidance priority prevents clumping
- [x] **NavMesh Runtime Baking** — `NavMeshSetup.Build()` collects PhysicsColliders and bakes NavMesh at runtime; called after buildings spawn, before units spawn
- [x] **Border Walls** — `GridManager.CreateBorderWalls()` adds 4 invisible BoxCollider walls at map edges to keep units on the playable area

## Priority 2 — Core Systems

- [x] **Migrate .claude config** — Copy CLAUDE.md and game-dev skill from .g to .claude/ so collaborators get project config from the repo. Sync script at scripts/sync-claude.sh
- [x] **Health System & Game Over** — Explorer has 10 HP; each zombie contact deals 1 damage; at 0 HP show Game Over screen with Retry button that restarts the scene
- [ ] **Building Placement** — Tap-to-place buildings on grid cells

## Priority 3 — Polish

- [ ] **Mobile Build** — Test on actual iOS/Android device
