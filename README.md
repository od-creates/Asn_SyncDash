# Sync Dash

**Sync Dash** is a local “simulated multiplayer” endless-runner built in Unity. On the right you control your player cube; on the left a ghost mirrors your moves with configurable lag.

---

#Game Concept

- **Player World (Right Half)**  
  - A player cube runs forward automatically.  
  - Use Left/Right buttons (or arrow keys) to dodge lanes.  
  - Tap anywhere on the screen to jump/leap over obstacles.  
  - Collect orbs for points(5 points each orb); hitting an obstacle ends the run.

- **Ghost World (Left Half)**  
  - A clone of your cube replays your inputs & movement snapshots with a short delay(editable config on GhostCube prefab)  
  - Demonstrates how real-time network sync might look with latency.  
  - Uses local ring buffers to store position snapshots and event queues to replay actions.

---

#Controls

| Input Method  | Action           |
|---------------|------------------|
| Left Arrow    | Dodge left       |
| Right Arrow   | Dodge right      |
| Tap anywhere  | Jump             |

---

#Key Mechanics

1. **Infinite Forward Illusion**  
   - Player & ghost cubes stay at Z=0 while the objects moves toward them via a `PooledObject` script.

2. **Real-Time Sync Simulation**  
   - **Snapshots**: Each frame records `(time, position)` in a 32-slot ring buffer.  
   - **Events**: Jumps, orb collects, obstacle hits queued in a 32-slot ring buffer.  
   - Ghost queries `time–lag` snapshot and replays events once their timestamp+lag ≤ now.

3. **Object Pooling**  
   - Two pools (Obstacles & Orbs) pre-warm 10 instances each (configurable prefab via editor)  
   - `ObjectPool.Get()` re-activates or instantiates; `Release()` de-activates and queues it.

4. **Shaders & VFX**  
   - **Player Cube**: Textured shader/material. Green glow on orb collection and red glow on obstable hit.
   - **Obstacles**: Dissolve shader with particle system - animates on hit.  
   - **Orbs**: Glowing particle system.  

5. **Performance**  
   - Fixed‐size buffers avoid per-frame GC allocations.  
   - Physics‐driven movement in `FixedUpdate` with `RigidbodyInterpolation` for smoothness.  
   - Build size < 50 MB via lightweight assets.

---
