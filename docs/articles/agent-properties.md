# Agent Properties

The `IPlayerBody` interface contains a set of properties and methods that define an agent's capabilities. Agents can access these by inheriting from `AbstractAgentMind`.

## Properties

Below are the key properties of the `IPlayerBody` interface, grouped by category.

---

### General Properties

- **`ActionPoints`**  
  The number of points available to perform actions during the current tick. Each action costs a specific number of `ActionPoints`. Reset to `10` at the end of each tick.

- **`Color`**  
  The team color of the agent.

- **`Energy`**  
  The agent's health. Maximum is `100`. It decreases when the agent is tagged. If it reaches `0`, the agent is removed from the simulation.

- **`GamePoints`**  
  Score that tracks an agent’s performance.
  Its usage depends on the selected game mode:

  In Team Deathmatch:
  - Tagging an opponent: +10 points
  - Eliminating an opponent (enemy energy ≤ 0): +10 bonus points
  - Being eliminated: –10 points

  In Capture the Flag:
  - No points are awarded for tagging or eliminating.
  - +1 point is awarded only when a team successfully captures the enemy flag and returns it to their own flag stand.


- **`CarryingFlag`**  
  Indicates if the agent is currently carrying an enemy flag (only relevant in *Capture the Flag* mode).

---

### Movement Properties

- **`Position`**  
  The current `(x, y)` coordinates of the agent on the map.

- **`Stance`**  
  Enum value describing posture: `Standing`, `Kneeling`, or `Lying`.  
  Affects movement speed, `VisualRange`, and `VisibilityRange`.

---

### Exploration Properties

- **`VisualRange`**  
  Number of visible grid cells, based on `Stance`:  
  - `Standing` → 10  
  - `Kneeling` → 8  
  - `Lying` → 5

- **`VisibilityRange`**  
  Distance at which the agent can be seen by others, based on `Stance`:  
  - `Standing` → 10  
  - `Kneeling` → 8  
  - `Lying` → 5

---

### Tagging Properties

- **`RemainingShots`**  
  Current number of available shots. If `0`, the agent must call `Reload()` to shoot again.

- **`WasTaggedLastTick`**  
  `true` if the agent was tagged in the previous tick; otherwise `false`.

---

If you need help with the corresponding methods for these properties, see the [Agent Methods](agent-methods.md) section.


