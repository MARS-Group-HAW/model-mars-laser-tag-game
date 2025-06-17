## Rules

### Game Logic

Below is a list of some of the most important parts of the game's logic:

- If an agent's `Energy` is equal to or below 0, then the agent is taken out of the environment and does not respawn for the rest of the game. The agent's points, however, are maintained and added to the cumulative score of the team at the end of the match.
- An agent's `Energy` regenerates over time. At the end of each tick, an agent's `Energy` is increased by 1.
- In `Capture the Flag` mode, agents respawn at their own team's flag stand after a short delay when their energy reaches zero.

### Constraints for Developers

To ensure the game functions as intended, follow these implementation rules:

1. Only interact with the interface `IPlayerBody` to access the agent's physical representation.
2. When interacting with the `PlayerMindLayer`, only invoke the `GetCurrentTick()` method. Other calls are not allowed.
3. Your agent's constructor must be empty.
4. Loops that are known not to terminate (e.g., `while(true)`) are not allowed.
5. `PropertyDescription` tags for loading external info into agents are not allowed. Only learned behavior is permitted as external data.