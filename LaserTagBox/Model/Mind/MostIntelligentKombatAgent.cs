using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using Mars.Common.Core.Collections;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;
using ServiceStack;

namespace LaserTagBox.Model.Mind
{
    // MostIntelligentKombatAgent - MIKA
    public class MostIntelligentKombatAgent : AbstractPlayerMind
    {
        private Position _goal;
        private static List<IPlayerBody> TeamMates = new List<IPlayerBody>();
        private int _id;
        private const int CampingTime = 50;
        private int _counter = 0;
        private static bool _isCamping = false;
        private static HashSet<EnemySnapshot> _targets = new HashSet<EnemySnapshot>();
        private static bool _enemiesCleared = false;
        private Position _targteHill = null;
        private Position _lastHill = null;
        private static HashSet<Position> _hills = new HashSet<Position>();
        private static HashSet<Position> _barriers = new HashSet<Position>();
        private static HashSet<Position> _ditches = new HashSet<Position>();
        private bool _newGoalNeeded = false;
        private int[,] _sectors = new int[3, 3];
        private int[] _currentSector = new int[2];
        private Position _oldPosition = Position.CreatePosition(0, 0);
        private static PlayerMindLayer _mindLayer;

        /*
         * Follower Vars
         */
        private bool _campingPosSet = false;
        private static Position _campingPos = null;
        private Position _campingPosPrivate = null;
        private int _resetHillDitchTagCounter = 0;
        private HashSet<Position> _alreadyTagged = new HashSet<Position>();

        public override void Init(PlayerMindLayer mindLayer)
        {
            TeamMates.Add(Body);
            _id = TeamMates.Count;
            _mindLayer = mindLayer;
            _goal = Position.CreatePosition(25, 25);

            //Set the value of the starting sector to one
            if (Body.Position.X >= 0 && Body.Position.X <= 15)
            {
                if (Body.Position.Y >= 0 && Body.Position.Y <= 15)
                {
                    _sectors[0, 0] = 1;
                    _currentSector[0] = 0;
                    _currentSector[1] = 0;
                }
                else
                {
                    _sectors[0, 2] = 1;
                    _currentSector[0] = 0;
                    _currentSector[1] = 2;
                }
            }
            else if (Body.Position.X >= 34 && Body.Position.X <= 49)
            {
                if (Body.Position.Y >= 0 && Body.Position.Y <= 15)
                {
                    _sectors[2, 0] = 1;
                    _currentSector[0] = 2;
                    _currentSector[1] = 0;
                }
                else
                {
                    _sectors[2, 2] = 1;
                    _currentSector[0] = 2;
                    _currentSector[1] = 2;
                }
            }
        }

        public override void Tick()
        {
            //Reset enemy list
            if (!_enemiesCleared)
            {
                _targets = new HashSet<EnemySnapshot>();
                _enemiesCleared = true;
            }

            // Remove dead teammates from list
            if (!Body.Alive && TeamMates.Contains(Body) && TeamMates.Count > 1)
            {
                TeamMates.Remove(Body);
            }

            // Lead-Functions
            if (!TeamMates.IsEmpty() && Body == TeamMates.ElementAt(0))
            {
                if (!_isCamping)
                {
                    LeadNotCamping();
                }
                else
                {
                    LeadCamping();
                }
            }
            // Follower-Functions
            else
            {
                if (!_isCamping)
                {
                    FollowerNotCamping();
                }
                else
                {
                    FollowerCamping();
                }
            }

            _enemiesCleared = false;
        }

        /*
         * Describes the procedure of a Lead who is camping
         */
        public void LeadCamping()
        {
            CheckForReload3();
            CheckForEnemies7();
            CheckForReload3();
            ToggleCamping2();
        }

        /*
         * Describes the procedure of a Lead who is not camping
         */
        public void LeadNotCamping()
        {
            CheckForReload3();
            CheckForEnemies7();
            CheckForHills1();

            // Movement when more or equal than 3 hills
            if (_hills.Count >= 3 && _newGoalNeeded)
            {
                DetermineTargetHill();
                _goal = _targteHill;
                _lastHill = _targteHill;
                _newGoalNeeded = false;
            }

            // Is a new goal needed?
            if (Body.Position.Equals(_goal) || _oldPosition.Equals(Body.Position))
            {
                _newGoalNeeded = true;
            }

            // Movement when fewer than 3 hills
            if (_hills.Count < 3 && _newGoalNeeded)
            {
                _goal = GetNextGoal();
                _newGoalNeeded = false;
            }

            _oldPosition = new Position(Body.Position.X, Body.Position.Y);
            Body.GoTo(_goal);
            if (Body.Position.Equals(_targteHill))
            {
                ToggleCamping2();
            }

            CheckForReload3();
            CheckForBarriers1();
            CheckForDitches1();
        }

        /*
         * Describes the procedure of a Follower who is camping
        */
        private void FollowerCamping()
        {
            DetermineCampingPos();
            _goal = _campingPosPrivate;
            Body.GoTo(_goal);
            ToggleCampingFollower2();
            CheckForReload3();
            if (!_targets.IsEmpty())
            {
                TagTarget6();
            }

            CheckForEnemies7();
            CheckForReload3();
        }

        /*
         * Describes the procedure of a Follower who is not camping
         */
        private void FollowerNotCamping()
        {
            CheckForReload3();
            CheckForEnemies7();
            TagHillOrDitch();
            MoveTowardsLead();
            CheckForReload3();
            ToggleCampingFollower2();
            CheckForBarriers1();
        }

        /*
         * Check if the player has to reload.
         * If there are less than two shoots left the player reloads
         *
         * Maximum ActionPoint costs: 3
         * Minimum ActionPoint costs: 0
         */
        public void CheckForReload3()
        {
            if (Body.ActionPoints < 3) return;
            if (Body.RemainingShots < 2)
            {
                Body.Reload3();
            }
        }

        /*
         * Check if there are enemies in Range.
         * If there are enemies the first one spotted will be the target of the team.
         *
         * If a target is set, the player will tag the targets position
         *
         * Maximum ActionPoint costs: 7
         * Minimum ActionPoint costs: 1
         *
         * This method calls: TagTarget6()
         */
        public void CheckForEnemies7()
        {
            if (Body.ActionPoints < 7) return;
            var enemies = Body.ExploreEnemies1();
            if (!enemies.IsEmpty())
            {
                _targets.AddRange(enemies);
                TagTarget6();
                // Stay camping when enemy encountered
                if (Body.Equals(TeamMates.ElementAt(0)))
                {
                    _counter = CampingTime;
                }
            }
        }

        /*
         * Check if the player can tag the target(check HasBeeline)
         *
         * If there is a BeeLine the player will tag the targets position
         * If there is no BeeLine the player will do nothing
         *
         * Maximum ActionPoint costs: 6
         * Minimum ActionPoint costs: 1
         */
        public void TagTarget6()
        {
            var targetsList = _targets.ToList();
            targetsList.Sort((x, y) => Body.GetDistance(x.Position).CompareTo(Body.GetDistance(y.Position)));
            var target = targetsList.ToArray()[0];
            if (Body.HasBeeline1(target.Position))
            {
                Body.Tag5(target.Position);
            }
        }

        /*
         * Change to Camping or to not Camping when called.
         * Also change the stance of the lead accordingly.
         *
         * If in camping mode check counter before changing mode,
         * only change mode if counter 0.
         */
        public void ToggleCamping2()
        {
            if (!_isCamping)
            {
                _isCamping = true;
                if (TeamMates.Count == 2) Body.ChangeStance2(Stance.Kneeling);
                if (TeamMates.Count == 1) Body.ChangeStance2(Stance.Lying);
                _counter = CampingTime;
            }
            else if (_counter > 0)
            {
                _counter--;
            }
            else
            {
                _isCamping = false;
                if (Body.Stance != Stance.Standing)
                {
                    Body.ChangeStance2(Stance.Standing);
                }

                _counter = 0;
            }
        }

        /*
         * If lead changes camping mode the followers have
         * to change the stance too.
         */
        public void ToggleCampingFollower2()
        {
            if (!_isCamping)
            {
                if (Body.Stance == Stance.Lying)
                {
                    if (Body.ActionPoints < 2) return;
                    Body.ChangeStance2(Stance.Standing);
                }
            }

            if (Body.Position.Equals(_goal) && _isCamping)
            {
                if (Body.Stance != Stance.Lying)
                {
                    if (Body.ActionPoints < 2) return;
                    Body.ChangeStance2(Stance.Lying);
                }
            }
        }

        /*
         * Checks for Hills in the surrounding and add the hills position to a Set.
         * That Set contains the positions of all Hills that have been found.
         *
         * Newly found hill will be the targetHill
         */
        public bool CheckForHills1()
        {
            if (Body.ActionPoints < 1) return false;
            var hillsTick = Body.ExploreHills1();
            if (!hillsTick.IsEmpty())
            {
                foreach (var hill in hillsTick)
                {
                    if (!_hills.Contains(hill))
                    {
                        _targteHill = hill;
                        _goal = _targteHill;
                        _lastHill = _targteHill;
                    }
                }

                _hills.AddRange(hillsTick);
                return true;
            }

            return false;
        }

        /*
         * Out of the hill HashSet, determine a new hill as a target.
         * This hill will not be the last targetHill
         */
        public void DetermineTargetHill()
        {
            var newTarget = _hills.ElementAt(RandomHelper.Random.Next(_hills.Count));
            while (newTarget.Equals(_lastHill))
            {
                newTarget = _hills.ElementAt(RandomHelper.Random.Next(_hills.Count));
            }

            _targteHill = newTarget;
        }

        /*
         * Check for barriers and write them into a HashSet
         */
        public void CheckForBarriers1()
        {
            if (Body.ActionPoints < 1) return;
            _barriers.AddRange(Body.ExploreBarriers1());
        }

        /*
         * Check for ditches and write them into a HashSet
         */
        public void CheckForDitches1()
        {
            if (Body.ActionPoints < 1) return;
            _ditches.AddRange(Body.ExploreDitches1());
        }

        /*
         * Move towards the Lead-Position but keep at least one tile distance
         */
        public void MoveTowardsLead()
        {
            if (Body.GetDistance(TeamMates.ElementAt(0).Position) > 1)
            {
                _goal = TeamMates.ElementAt(0).Position;
            }
            else
            {
                _goal = Body.Position;
            }

            if (TeamMates.Count == 3 && Body.Equals(TeamMates.ElementAt(2)))
            {
                if (Body.GetDistance(TeamMates.ElementAt(1).Position) > 1)
                {
                    _goal = TeamMates.ElementAt(1).Position;
                }
                else
                {
                    _goal = Body.Position;
                }
            }

            Body.GoTo(_goal);
        }

        /*
         * Followers have to position themselfes beside the Lead-
         * The choose a Position bewside the Lead where the other follower is not and
         * where no barrier is.
         */
        public void DetermineCampingPos()
        {
            if (!_campingPosSet)
            {
                for (int i = 1; i <= 4; i++)
                {
                    var posPos = getPossiblePos(i);
                    if (!_barriers.Contains(posPos))
                    {
                        if (_campingPos != null)
                        {
                            if (!_campingPos.Equals(posPos))
                            {
                                _campingPosPrivate = posPos;
                                _campingPos = null;
                                return;
                            }
                        }
                        else
                        {
                            _campingPos = posPos;
                            _campingPosPrivate = posPos;
                            return;
                        }
                    }
                }
            }
        }

        /*
         * Helper function for DetermineCampingPos
         */
        private Position getPossiblePos(int dir)
        {
            if (dir == 1) //N
            {
                return new Position(TeamMates.ElementAt(0).Position.X, TeamMates.ElementAt(0).Position.Y + 1);
            }

            if (dir == 2) //O
            {
                return new Position(TeamMates.ElementAt(0).Position.X + 1, TeamMates.ElementAt(0).Position.Y);
            }

            if (dir == 3) //S
            {
                return new Position(TeamMates.ElementAt(0).Position.X, TeamMates.ElementAt(0).Position.Y - 1);
            }

            if (dir == 4) //W
            {
                return new Position(TeamMates.ElementAt(0).Position.X - 1, TeamMates.ElementAt(0).Position.Y);
            }

            return null;
        }

        private Position GetNextGoal()
        {
            int value = _sectors[0, 0];
            List<int[]> posSectors = new List<int[]>();
            posSectors.Add(new int[] {0, 0});
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (_sectors[i, j] < value)
                    {
                        posSectors = new List<int[]>();
                        posSectors.Add(new[] {i, j});
                        value = _sectors[i, j];
                    }
                    else if (value == _sectors[i, j])
                    {
                        posSectors.Add(new[] {i, j});
                    }
                }
            }

            Random random = new Random();
            int index = random.Next(0, posSectors.Count);
            int[] newSector = posSectors[index];
            _sectors[newSector[0], newSector[1]] += 1;
            int x = random.Next(1, 16) + newSector[0] * 16;
            int y = random.Next(1, 16) + newSector[1] * 16;
            return Position.CreatePosition(x, y);
        }

        private void TagHillOrDitch()
        {
            if (_resetHillDitchTagCounter >= 50)
            {
                _alreadyTagged = new HashSet<Position>();
                _resetHillDitchTagCounter = 0;
            }

            _resetHillDitchTagCounter++;
            foreach (var ditch in _ditches)
            {
                if (Body.GetDistance(ditch) < 10 && Body.GetDistance(ditch) > 5 && !_alreadyTagged.Contains(ditch))
                {
                    _alreadyTagged.Add(ditch);
                    if (Body.ActionPoints >= 5)
                    {
                        Body.Tag5(ditch);
                    }

                    return;
                }
            }

            foreach (var hill in _hills)
            {
                if (Body.GetDistance(hill) < 15 && Body.GetDistance(hill) > 8 && !_alreadyTagged.Contains(hill))
                {
                    _alreadyTagged.Add(hill);
                    if (Body.ActionPoints >= 5)
                    {
                        Body.Tag5(hill);
                    }

                    return;
                }
            }
        }
    }
}
