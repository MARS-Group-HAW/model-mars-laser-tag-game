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

        public override void Tick()
        {
            //do nothing, the mind handles everything
        }

        //	*********************** core attributes ***********************
        public int ActionPoints { get; private set; } = 1;
        public int Energy { get; private set; } = 100;
        public int GamePoints { get; private set; }


        //	*********************** tagging attributes ***********************
        public int magazineCount = 5;
        public bool WasTagged { get; set; }


        public List<Position> ExploreHills1() => ActionPoints-- < 1 ? null : ExploreSpots(typeof(Hill));

        public List<Position> ExploreBarriers1() => ActionPoints-- < 1 ? null : ExploreSpots(typeof(Barrier));

        public List<Position> ExploreDitches1() => ActionPoints-- < 1 ? null : ExploreSpots(typeof(Ditch));

        private List<Position> ExploreSpots(Type type)
        {
            return battleground.SpotEnv
                .Explore(Position, VisualRange, -1, spot => spot.GetType() == type && HasBeeline(spot))
                .Select(spot => Position.CreatePosition(spot.Position.X, spot.Position.Y)).ToList();
        }

        public List<EnemySnapshot> ExploreEnemies1()
        {
            if (ActionPoints-- < 1) return null;
            return battleground.FigtherEnv
                .Explore(Position, VisualRange, -1, player => player.Color != Color && HasBeeline(player))
                .Select(player => new EnemySnapshot(player.ID, player.Color, player.Stance, player.Position))
                .ToList();
        }

        public void ChangeStance2(Stance newStance)
        {
            if (ActionPoints < 2) return;
            ActionPoints -= 2;
            Stance = newStance;
        }

        public void Tag5(Position position)
        {
            if (magazineCount < 1) Reload3();

            if (ActionPoints < 5) return;
            ActionPoints -= 5;

            var enemyStanceVal = 2;

            var enemy = battleground.GetAgentOn(Position);

            var fieldType = battleground.GetIntValue(position);
            enemyStanceVal = fieldType switch
            {
                2 => 2,
                3 => 0,
                _ => enemy.Stance switch
                {
                    Stance.Kneeling => 1,
                    Stance.Lying => 0,
                    _ => enemyStanceVal
                }
            };
            var stanceValue = Stance switch
            {
                Stance.Standing => 8,
                Stance.Kneeling => 6,
                Stance.Lying => 4,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (RandomHelper.Random.Next(10) + 1 + enemyStanceVal > stanceValue)
            {
                enemy.WasTagged = true;
                GamePoints += 10;
                if (enemy.Energy <= 0) GamePoints += 10;
            }

            magazineCount--;
        }

        public void Reload3()
        {
            if (ActionPoints < 3) return;
            ActionPoints -= 3;
            magazineCount = 5;
        }

        public List<IPlayerBody> ExploreTeam()
        {
            return new List<IPlayerBody>(battleground.FigtherEnv
                .Entities.Where(body => body.Color == Color)
                .ToList());
        }

        protected override Position MoveToPosition(Position position)
        {
            return battleground.FigtherEnv.PosAt(this, position.PositionArray);
        }

        private bool HasBeeline(IPositionable other)
        {
            return battleground.HasBeeline(Position.X, Position.Y, other.Position.X, other.Position.Y);
        }

        public bool HasBeeline(Position other)
        {
            return battleground.HasBeeline(Position.X, Position.Y, other.X, other.Y);
        }

        public int GetDistance(Position position) =>
            (int) Distance.Chebyshev(Position.PositionArray, position.PositionArray);

        public int RemainingShots { get; private set; }
    }
}