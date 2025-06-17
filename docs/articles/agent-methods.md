# Agent Methods

This section describes the methods provided by the `IPlayerBody` interface.  
The digit at the end of each method name indicates the number of `ActionPoints` (AP) required to execute it.  
Methods without a digit require **0 AP**.


> [!NOTE]
> If the agent does not have enough `ActionPoints`, the method usually returns `false` or `null`.

---

## Movement Methods

### `ChangeStance2(Stance)`
**AP Cost:** 2  
**Returns:** *void*

Changes the agent's stance to `Standing`, `Kneeling`, or `Lying`.  
Stance affects movement speed, `VisualRange`, and `VisibilityRange`.

---

### `GoTo(Position) : bool`
**AP Cost:** 0  
**Returns:** `true` if move executed, `false` otherwise

Pathfinding and movement method. Moves the agent one step toward a specified `(x, y)` destination.  
If called again with a different position, the path is re-planned.

> [!NOTE]
> Returns `false` if the target is unreachable or outside the map.

---

## Exploration Methods

> [!NOTE]
> All exploration methods fail (return `null` or `false`) if the agent does not have enough `ActionPoints`.  
> By default, exploration methods cost **1 ActionPoint**, unless stated otherwise.



- `ExploreBarriers1() : List<Position>` – Detect visible `Barrier` tiles
- `ExploreDitches1() : List<Position>` – Detect visible `Ditch` tiles
- `ExploreHills1() : List<Position>` – Detect visible `Hill` tiles
- `ExploreEnemies1() : List<EnemySnapshot>` – Detect visible opponents
- `ExploreTeam() : List<IPlayerBody>` – Detect teammates
- `ExploreWater1() : List<Position>` – Detect visible `Water` tiles
- `ExploreBarrels1() : List<Position>` – All visible `ExplosiveBarrel` tiles
- `ExploreExplosiveBarrels1() : List<Position>` – Only unexploded barrels
- `ExploreEnemyFlagStands1() : List<Position>` – Detect your opponent’s flag stand
- `ExploreFlags2() : List<FlagSnapshot>` – All visible flags (**costs 2 AP**)
- `ExploreOwnFlagStand() : Position` – Detect your team’s flag stand 

---

### `GetDistance(Position) : int`
**AP Cost:** 0  
**Returns:** Number of grid cells (or `-1` if not visible)

Computes shortest distance to a position, if it's visible to the agent.

---

### `HasBeeline1(Position) : bool`
**AP Cost:** 1  
Checks for unobstructed line-of-sight to a grid position.


> [!NOTE]
> Returns `false` if vision is blocked or if insufficient AP.

---

## Tagging Methods

### `Reload3()`
**AP Cost:** 3  
**Returns:** *void*

Reloads the tagging weapon, restoring `RemainingShots` to 5.  
Must be used when `RemainingShots == 0`.

---

### `Tag5(Position)`

**AP Cost:** 5  
**Returns:** `true` if a tag or explosion was successful, `false` otherwise

Fires a shot at the specified grid cell. The action can result in:

- Triggering an **explosion** if an unexploded `ExplosiveBarrel` is present.
- Attempting to **tag** an enemy agent using a **probabilistic hit calculation**.

#### Conditions for Execution

The method will **immediately fail and return `false`** if:

- The caller has **less than 5 ActionPoints**
- The caller has **no RemainingShots**
- There is **no line of sight** (`!HasBeeline`) to the `aimedPosition`
- The target is a **friendly agent**

#### Tagging Logic

If the above checks pass and an **enemy** is present at the `aimedPosition`, the following probabilistic model determines if the tag is successful:

```plaintext
Random.Next(10) + targetStanceValue + targetTerrainValue > ownStanceThreshold
```

- `targetStanceValue`:
  - `Standing` → 2
  - `Kneeling` → 1
  - `Lying` → 0

- `targetTerrainValue` (based on field type):
  - Normal ground → 1
  - Ditch → 0
  - Hill → 2

- `ownStanceThreshold`:
  - `Standing` → 8
  - `Kneeling` → 6
  - `Lying` → 4

> This means: a tag is **more likely** when the attacker is in a low stance (e.g. `Lying`), and the defender is more exposed (e.g. `Standing` on a `Hill`).

If the **hit succeeds**:
- The enemy's `Energy` is reduced by **10**
- The caller gains **10 GamePoints**
- If the tag causes the enemy’s `Energy` to drop to **0 or less**, the caller gains an **additional 10 GamePoints**

#### Team Mode Dependency

> [!IMPORTANT]
> **GamePoints are only awarded in `Team Deathmatch` mode.**  
> In `Capture the Flag` mode, tags affect gameplay but do **not** yield points.

#### Special Case: Explosive Barrel

If the targeted cell contains a **live ExplosiveBarrel**:
- The barrel explodes **immediately**
- Explosion affects **all agents** in a **3-tile radius**
- Returns `true` regardless of whether an enemy is present

#### Side Effects

- Reduces `RemainingShots` by 1 if fired
- Friendly agents are **never** tagged
---


