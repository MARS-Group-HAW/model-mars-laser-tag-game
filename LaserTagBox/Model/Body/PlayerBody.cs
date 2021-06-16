using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using LaserTagBox.Model.Spots;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using Mars.Numerics.Statistics;

namespace LaserTagBox.Model.Body
{
    public class PlayerBody : MovingAgent, IPlayerBody
    {
        private long _currentTick = -1;

        public override void Tick()
        {
            if (!Alive) return;
            if (_currentTick == battleground.GetCurrentTick())
                throw new InvalidOperationException("Don't call the Tick method, it's done by the system.");
            _currentTick = battleground.GetCurrentTick();

            RefillPoints();
        }

        //	*********************** core attributes ***********************
        public int ActionPoints { get; private set; } = 1;
        public int Energy { get; private set; } = 100;
        public int GamePoints { get; private set; }


        //	*********************** tagging attributes ***********************
        public bool WasTaggedLastTick => _currentTick - 1 == _tickWhenLastTagged;

        private long _tickWhenLastTagged = -100;

        public List<Position> ExploreHills1() => ExploreSpots(typeof(Hill));

        public List<Position> ExploreBarriers1() => ExploreSpots(typeof(Barrier));

        public List<Position> ExploreDitches1() => ExploreSpots(typeof(Ditch));

        private List<Position> ExploreSpots(Type type)
        {
            if (ActionPoints < 1) return null;
            ActionPoints -= 1;
            return battleground.SpotEnv
                .Explore(Position, VisualRange, -1, spot => spot.GetType() == type && HasBeeline(spot))
                .Select(spot => Position.CreatePosition(spot.Position.X, spot.Position.Y)).ToList();
        }

        public List<EnemySnapshot> ExploreEnemies1()
        {
            if (ActionPoints < 1) return null;
            ActionPoints -= 1;
            return battleground.FigtherEnv
                .Explore(Position, VisualRange, -1,
                    player => IsEnemy(player) && HasBeeline(player) && IsVisible(player))
                .Select(player => new EnemySnapshot(player.ID, player.Color, player.Stance, player.Position))
                .ToList();
        }

        private bool IsEnemy(PlayerBody enemy) => enemy.Color != Color;

        private bool IsVisible(PlayerBody enemy) => enemy.VisibilityRange >= GetDistance(enemy.Position);

        public void ChangeStance2(Stance newStance)
        {
            if (ActionPoints < 2) return;
            ActionPoints -= 2;

            Stance = newStance;

            MovementDelay = Stance switch
            {
                Stance.Standing => 0,
                Stance.Kneeling => 2,
                Stance.Lying => 3,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool Tag5(Position aimedPosition)
        {
            if (ActionPoints < 5) return false;
            ActionPoints -= 5;

            if (RemainingShots < 1) return false;
            RemainingShots--;

            if (!HasBeeline(aimedPosition)) return false;

            var enemy = battleground.GetAgentOn(aimedPosition);
            if (enemy == null) return false;

            var fieldType = battleground.GetIntValue(aimedPosition);
            var enemySpot = fieldType switch
            {
                3 => 0, // in ditch
                2 => 2, // on hill
                _ => 1 // on normal ground
            };
            var enemyStance = enemy.Stance switch
            {
                Stance.Standing => 2,
                Stance.Kneeling => 1,
                Stance.Lying => 0,
                _ => throw new ArgumentOutOfRangeException()
            };

            var stanceValue = Stance switch
            {
                Stance.Standing => 8,
                Stance.Kneeling => 6,
                Stance.Lying => 4,
                _ => throw new ArgumentOutOfRangeException()
            };

            var success = RandomHelper.Random.Next(10) + enemyStance + enemySpot > stanceValue;
            if (success)
            {
                GamePoints += 10;
                if (enemy.Tagged()) GamePoints += 10; // bonus points
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Is called when a tag was successfully executed. Handles the consequences.
        /// </summary>
        /// <returns>true if the player has below 0 energy and therefore dies, false otherwise</returns>
        private bool Tagged()
        {
            _tickWhenLastTagged = _currentTick;
            Energy -= 10;
            if (Energy >= 0) return false;

            Die();
            return true;
        }

        public bool Alive { get; private set; } = true;
        
        private void Die()
        {
            battleground.FigtherEnv.Remove(this);
            //do not remove it from tick-cycle for evaluation purposes
            
            ActionPoints = 0;
            Alive = false;
        }

        public void Reload3()
        {
            if (ActionPoints < 3) return;
            ActionPoints -= 3;
            RemainingShots = 5;
        }

        public List<FriendSnapshot> ExploreTeam()
        {
            return new List<FriendSnapshot>(battleground.FigtherEnv
                .Entities.Where(body => body.Color == Color && body != this).Select(b =>
                    new FriendSnapshot(b.ID, b.Color, b.Stance, b.Position, b.Energy, b.VisualRange, b.VisibilityRange))
                .ToList());
        }

        protected override void InsertIntoEnv()
        {
            battleground.FigtherEnv.Insert(this);
        }

        protected override Position MoveToPosition(Position position)
        {
            return battleground.FigtherEnv.PosAt(this, position.PositionArray);
        }

        private bool HasBeeline(IPositionable other)
        {
            return battleground.HasBeeline(Position.X, Position.Y, other.Position.X, other.Position.Y);
        }

        public bool HasBeeline1(Position other)
        {
            if (ActionPoints < 1) return false;
            ActionPoints -= 1;

            return HasBeeline(other);
        }

        private bool HasBeeline(Position other)
        {
            return battleground.HasBeeline(Position.X, Position.Y, other.X, other.Y);
        }

        public int GetDistance(Position position) =>
            (int) Distance.Manhattan(Position.PositionArray, position.PositionArray);

        public int RemainingShots { get; private set; } = 5;


        private void RefillPoints()
        {
            ActionPoints = 10;
            if (MovementDelay > 0) MovementDelay--;
            HasMoved = false;
        }
    }
}