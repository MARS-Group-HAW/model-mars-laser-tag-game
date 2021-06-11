using System;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Shared
{
    /// <summary>
    ///     Provides information about other team members. 
    /// </summary>
    public  struct FriendSnapshot
    {
        public FriendSnapshot(Guid id, Color team, Stance stance, Position position, int energy, double visualRange, double visibilityRange)
        {
            Id = id;
            Team = team;
            Stance = stance;
            Position = position;
            Energy = energy;
            VisualRange = visualRange;
            VisibilityRange = visibilityRange;
        }
        
        /// <summary>
        ///     Identifies the player.
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        ///     Indicates to witch team the agent belongs.
        /// </summary>
        public Color Team { get; }
        
        /// <summary>
        ///     The stance has influence on visibility (explorable by others) and visual range (extend of on exploration).
        /// </summary>
        public Stance Stance { get; }
        
        /// <summary>
        ///     Current position of the agent on the grid.
        /// </summary>
        public Position Position { get; }
        
        /// <summary>
        ///     Remaining energy of the agent.
        /// </summary>
        public int Energy { get; }
        
        /// <summary>
        ///     Provides a metric on the distance for exploring.
        /// </summary>
        public double VisualRange { get; }

        /// <summary>
        ///     Provides a metric on how visible the agent is for enemies.
        /// </summary>
        public double VisibilityRange { get; }
    }
}