using System;
using System.Collections.Generic;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model
{
    public interface IPlayerBody : IPositionable
    {
        //costly
        List<Position> ExploreHills1(); //within sight
        List<Position> ExploreBarriers1();
        List<Position> ExploreDitches1();
        List<EnemyFighter> ExploreEnemies1();

        void ChangeStance2(Stance newStance);

        void Tag5(Position position); //treffe den ersten, wenn mehrere auf dem selbem Feld

        void Reload3();

        // free
        List<IPlayerBody> ExploreTeam();

        bool GoTo(Position position); //pro Tick 1 Feld bewegen (liegend nur alle 3 Ticks 1 Feld)

        bool HasBeeline(Position position);

        int GetDistance(Position position); //chebyshef diganoal = 1, manhatten diagonal = 2

        int RemainingShots { get; }
        Stance Stance { get; }
        Position Position { get; }
    }
}