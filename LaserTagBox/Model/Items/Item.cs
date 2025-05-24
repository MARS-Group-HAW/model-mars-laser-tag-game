using System;
using LaserTagBox.Model.Body;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Items;

public abstract class Item : IAgent<PlayerBodyLayer>, IPositionable
{
    #region Properties
    /// <summary>
    ///     A reference to the environment in which the object is situated.
    /// </summary>
    protected PlayerBodyLayer Battleground { get; private set; }

    /// <summary>
    ///     The position of the object in the environment.
    /// </summary>
    public Position Position
    {
        get => Owner != null ? Owner.Position : _groundPosition;
        set => _groundPosition = value;
    }
    
    /// <summary>
    ///     Indicates whether the object is picked up by an agent.
    /// </summary>
    public bool PickedUp => Owner != null;


    /// <summary>
    ///     The unique identifier of the object.
    /// </summary>
    public Guid ID { get; set; }

    public Guid OwnerID
    {
        get => Owner != null ? Owner.ID : Guid.Empty;
    }

    public PlayerBody Owner;
    
    private Position _groundPosition;
    #endregion
    
    #region Initialization
    /// <summary>
    ///     Initialization routine of the object.
    /// </summary>
    /// <param name="battleground">A reference to the environment in which the object is to be situated.</param>
    public virtual void Init(PlayerBodyLayer battleground)
    {
        Battleground = battleground;
        Battleground.ItemEnv.Insert(this);
    }
    #endregion

    #region Tick
    /// <summary>
    ///     The behavior routine of the object. (This is an abstract object without behavior).
    /// </summary>
    public virtual void Tick()
    {
        //do nothing
    }
    #endregion
    
    #region Methods
    public virtual void PickUp(PlayerBody agent)
    {
        Owner = agent;
        Battleground.ItemEnv.Remove(this);
    }
    
    public virtual void Drop()
    {
        _groundPosition = new Position(Owner.Position.X, Owner.Position.Y);
        if (Owner != null)
        {
            // Console.WriteLine($"{Owner.ID} dropped flag at {Position}");
            Owner.CarryingFlag = false;
            Owner = null;
            Battleground.ItemEnv.Insert(this);
        }
    }
    #endregion
}