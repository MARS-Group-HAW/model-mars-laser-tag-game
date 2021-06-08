using System;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model
{
    public readonly struct EnemyFighter
    {
        public EnemyFighter(Guid id, Color team, Stance stance, Position position)
        {
            Id = id;
            Team = team;
            Stance = stance;
            Position = position;
        }

        public Color Team { get; }
        public Guid Id { get; }
        public Stance Stance { get; }
        public Position Position { get; }

        //TODO scoring?
    }
}