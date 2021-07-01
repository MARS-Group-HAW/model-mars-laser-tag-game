using System;
using System.Collections.Generic;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Mind
{
    public class RenamedPlayerMind : AbstractPlayerMind
    {
        private static readonly Team Team = new();
        private PlayerMindLayer _mind;

        public override void Init(PlayerMindLayer mindLayer)
        {
            _mind = mindLayer;

            // update team positions
            Team.UpdateFriends(Body.ExploreTeam());
        }

        public override void Tick()
        {
            // don't do anything if dead, obviously
            if (!Body.Alive)
            {
                //Console.WriteLine($"{_mind.GetCurrentTick()} {ID}: DEAD!");
                return;
            }

            var currentTick = _mind.GetCurrentTick();

            // check if we have a target
            var target = Team.GetEnemyTarget(currentTick);
            if (target == null)
            {
                // stand up if not standing
                if (Body.Stance != Stance.Standing)
                    ChangeStance(Stance.Standing);

                // -1 explore
                Team.UpdateSeenEnemies(ExploreEnemies(), currentTick);

                // check for target
                target = Team.GetEnemyTarget(currentTick);
                if (target == null)
                {
                    // reload if less than 5 shots
                    if (Body.RemainingShots < 5)
                    {
                        Reload();
                    }
                    else
                    {
                        // explore barriers, hills, ditches to avoid moving there
                        if (Body.ActionPoints >= 1)
                            Team.AddObstacles(ExploreBarriers());
                        if (Body.ActionPoints >= 1)
                            Team.AddObstacles(ExploreDitches());
                        if (Body.ActionPoints >= 1)
                            Team.AddObstacles(ExploreHills());
                    }

                    // no target: walk toward movement target and reload if necessary
                    var gotoResult = Body.GoTo(Team.GetMovementTarget(currentTick, Body.Position));
                    if (!gotoResult && currentTick > 5)
                        Team.SetMovementTargetFailed();

                    // lie down again
                    ChangeStance(Stance.Lying);

                    // reset combat strategy if moving
                    Team.ResetCombatStrategy();
                }
                else
                {
                    // target found: lay down & tag
                    if (Body.Stance != Stance.Lying)
                        ChangeStance(Stance.Lying);
                    ReloadAndTag(Team.GetEnemyTarget(currentTick));
                }
            }
            else
            {
                /* choose combat strategy
                    the combat strategy switches every tick during a fight for every agent
                    so for three agents it will switch between:
                    explorer: stand up -> explore -> lie down -> shoot
                    shooter: shoot -> shoot
                    
                    sweet spot: accurate enemy positions / dealing lots of damage
                    for example:
                        tick1:
                            agent1:
                                shoot -> shoot
                            agent2:
                                stand up -> explore -> lie down -> shoot
                            agent3:
                                shoot -> shoot
                        tick2:
                            agent1:
                                stand up -> explore -> lie down -> shoot
                            agent2:
                                shoot -> shoot
                            agent3:
                                stand up -> explore -> lie down -> shoot
                        ...
                    */

                // lie down to improve accuracy if not already lying
                if (Body.Stance != Stance.Lying)
                    ChangeStance(Stance.Lying);

                // select combat strategy, as explained above
                var combatStrategy = Team.GetCombatStrategy();
                switch (combatStrategy)
                {
                    case CombatStrategy.Explorer:
                        ChangeStance(Stance.Standing);
                        Team.UpdateSeenEnemies(ExploreEnemies(), currentTick);
                        ChangeStance(Stance.Lying);
                        ReloadAndTag(Team.GetEnemyTarget(currentTick));
                        break;
                    case CombatStrategy.Shooter:
                    {
                        while (Body.ActionPoints >= 5)
                            ReloadAndTag(Team.GetEnemyTarget(currentTick));
                        break;
                    }
                    default:
                        throw new NotImplementedException("weird...");
                }
            }

            // update team positions
            Team.UpdateFriends(Body.ExploreTeam());
        }

        /// <summary>
        ///     shoot & reload if required
        /// </summary>
        /// <param name="enemy">target enemy</param>
        private void ReloadAndTag(Position enemy)
        {
            // if player has no shots left, reload before shooting
            if (Body.RemainingShots == 0)
                Reload();
            Tag(enemy);
        }

        /// <summary>
        ///     wraps the "Reload3" function with additional console output
        /// </summary>
        private void Reload()
        {
            var actionPoints = Body.ActionPoints;
            Body.Reload3();
            //.WriteLine($"{_mind.GetCurrentTick()} {ID} Reload: {actionPoints} -> {Body.ActionPoints}");
        }

        /// <summary>
        ///     wraps the "ExploreDitches1" function with additional console output
        /// </summary>
        /// <returns>list of visible enemies</returns>
        private IEnumerable<Position> ExploreDitches()
        {
            var actionPoints = Body.ActionPoints;
            var result = Body.ExploreDitches1();
            //Console.WriteLine($"{_mind.GetCurrentTick()} {ID} ExploreDitches: {actionPoints} -> {Body.ActionPoints}");
            return result;
        }

        /// <summary>
        ///     wraps the "ExploreBarriers1" function with additional console output
        /// </summary>
        /// <returns>list of visible enemies</returns>
        private IEnumerable<Position> ExploreBarriers()
        {
            var actionPoints = Body.ActionPoints;
            var result = Body.ExploreBarriers1();
            //Console.WriteLine($"{_mind.GetCurrentTick()} {ID} ExploreBarriers: {actionPoints} -> {Body.ActionPoints}");
            return result;
        }

        /// <summary>
        ///     wraps the "ExploreHills1" function with additional console output
        /// </summary>
        /// <returns>list of visible enemies</returns>
        private IEnumerable<Position> ExploreHills()
        {
            var actionPoints = Body.ActionPoints;
            var result = Body.ExploreHills1();
            //Console.WriteLine($"{_mind.GetCurrentTick()} {ID} ExploreHills: {actionPoints} -> {Body.ActionPoints}");
            return result;
        }

        /// <summary>
        ///     wraps the "ExploreEnemies1" function with additional console output
        /// </summary>
        /// <returns>list of visible enemies</returns>
        private IEnumerable<EnemySnapshot> ExploreEnemies()
        {
            var actionPoints = Body.ActionPoints;
            var result = Body.ExploreEnemies1();
            //Console.WriteLine($"{_mind.GetCurrentTick()} {ID} ExploreEnemies: {actionPoints} -> {Body.ActionPoints}");
            return result;
        }

        /// <summary>
        ///     wraps the "ChangeStance2" function with additional console output
        /// </summary>
        /// <param name="stance">stance</param>
        private void ChangeStance(Stance stance)
        {
            var actionPoints = Body.ActionPoints;
            Body.ChangeStance2(stance);
            //Console.WriteLine($"{_mind.GetCurrentTick()} {ID} ChangeStance: {actionPoints} -> {Body.ActionPoints}");
        }

        /// <summary>
        ///     wraps the "Tag5" function with additional console output
        /// </summary>
        /// <param name="enemy">target enemy</param>
        private void Tag(Position enemy)
        {
            var actionPoints = Body.ActionPoints;
            Body.Tag5(enemy);
            //Console.WriteLine($"{_mind.GetCurrentTick()} {ID} Tag: {actionPoints} -> {Body.ActionPoints}");
        }
    }
}