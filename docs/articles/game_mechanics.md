# Game Mechanics

This section outlines the core rules and systems that define how agents perceive, move, and interact in the LaserTag simulation. Understanding these mechanics is crucial to developing effective strategies.

---

## Enemy Vision

Agent visibility is determined using `ExploreEnemies1()`, which consumes **1 ActionPoint** and returns a list of visible opponents.  
The method processes visibility in a strict step-by-step sequence:

### Step 1: Is the Target Within Visual Range?

Before anything else, the target must be within the calling agent’s `VisualRange`. If the target lies outside this range, no further checks are made.

```csharp
public double VisualRange =>
    VisualRangePenalty + Stance switch
    {
        Standing => 10,
        Kneeling => 8,
        Lying    => 5
    };
```

#### VisualRange Modifiers

```csharp
private double VisualRangePenalty =>
    CurrentSpot switch
    {
        null      => 0,
        Hill _    => 3,
        Ditch _   => -3,
        _         => 0
    };
```

- **Standing on a Hill**: `+3` range bonus
- **Inside a Ditch**: `-3` range penalty
- **Flat terrain**: no change

Only if the enemy lies within this effective `VisualRange`, the agent proceeds with further visibility checks.

### Step 2: Team Check

Only agents from opposing teams are considered visible. Friendly agents are ignored even if all other conditions are met.

### Step 3: Beeline (Line-of-Sight)

The agent must have an unobstructed straight line to the target without any vision-blocking structures (e.g. Barriers, Hills).  
This is verified using `HasBeeline(enemy)`, which uses **Bresenham’s Line Algorithm** to trace the path.

### Step 4: Visibility Check

The enemy must be visible according to:

```csharp
Battleground.GetIntValue(Position) is 2 or 3 || enemy.VisibilityRange >= GetDistance(enemy.Position);
```

This means:
- If **the caller is on a Hill or in a Ditch** (`GetIntValue == 2 or 3`), visibility is automatically granted.
- Otherwise, the enemy’s `VisibilityRange` must be greater than or equal to the actual distance.

### How `VisibilityRange` is Calculated

Each agent has a dynamic visibility value depending on their stance and a possible penalty:

```csharp
public double VisibilityRange =>
    VisibilityRangePenalty + Stance switch
    {
        Standing => 10,
        Kneeling => 8,
        Lying    => 5
    };
```

- **Standing**: 10 units visibility
- **Kneeling**: 8 units visibility
- **Lying**: 5 units visibility  
- **VisibilityRangePenalty**: a dynamic modifier that reduces visibility due to the environment effects.

### Terrain Effects on Visibility

The `VisibilityRange` is affected by the terrain the agent is currently occupying via a **penalty or bonus**:

```csharp
private double VisibilityRangePenalty =>
    CurrentSpot switch
    {
        null      => 0,
        Hill _    => 3,
        Ditch _   => -3,
        _         => 0
    };
```

- **Hill**: adds +3 to `VisibilityRange` (more exposed)
- **Ditch**: subtracts 3 from `VisibilityRange` (better cover)
- **Flat ground or unknown**: no modifier

This means:
- Agents on **Hills** are easier to spot.
- Agents in **Ditches** are harder to spot.

---

## Movement

Agents navigate the map using a modified [D* Lite Algorithm](http://idm-lab.org/bib/abstracts/papers/aaai02b.pdf), which plans efficient routes and adapts when new obstacles appear.

Movement is affected by **stance**, introducing a delay per action:

- `Standing` → no delay (fastest)
- `Kneeling` → 2-tick delay
- `Lying` → 3-tick delay

Delays are tracked using an internal `MovementDelayCounter`, which resets when a new goal is selected or the stance changes.

> [!TIP]  
> Use `Standing` for fast mobility, and `Lying` for stealth or accuracy — depending on your tactical needs.

---

## Tagging

Tagging simulates laser interactions and is implemented probabilistically in `Tag5(Position)`.  
Refer to the [`Tag5(Position)` method](agent-methods.md#tag5position) for full technical details.

Tagging success is influenced by:

1. **Attacker’s Stance (X)**  
   - `Lying` → highest accuracy  
   - `Standing` → lowest accuracy

2. **Target’s Stance (Y)**  
   - `Standing` → easiest to tag  
   - `Lying` → hardest to tag

3. **Target Terrain**  
   - On `Hill`: Y becomes **easier** to tag due to exposure  
   - In `Ditch`: Y becomes **harder** to tag due to cover

4. **Randomness**  
   Even ideal conditions don’t guarantee a hit — randomness ensures unpredictability.
---

