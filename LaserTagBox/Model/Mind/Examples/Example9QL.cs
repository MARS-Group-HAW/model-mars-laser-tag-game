using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using LaserTagBox.Model.Shared;
using Mars.Common.Core.Random;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using ServiceStack;

namespace LaserTagBox.Model.Mind;

public class Example9QL : AbstractPlayerMind
{
    #region Fields
    
    private const String QtableSrc = "../../../Model/Mind/Examples/Example9_qtable.bin";
    private bool _isTraining = false;

    private PlayerMindLayer _mindLayer;

    private double _discount = 0.9;
    private double _epsilon = 0.2;
    private double _learningRate = 0.1;

    public enum Location
    {
        None,
        Hill,
        Ditch
    }

    // individual fields (for each agent)
    private EnemySnapshot _target; // target to shoot at
    private EnemySnapshot _sharedEnemy;

    private State _state = new State();

    // "shared" fields for our team
    private static List<IPlayerBody> _teammates = new();
    private static Dictionary<int, EnemySnapshot> _sharedEnemies = new();
    private static HashSet<Position> _sharedHills = new();
    private static HashSet<Position> _sharedDitches = new();
    private static HashSet<Position> _sharedBarriers = new();

    // mutexes for accessing and changing static fields
    private static Mutex _mutexTeammates = new Mutex();
    private static Mutex _mutexEnemies = new Mutex();
    private static Mutex _mutexHills = new Mutex();
    private static Mutex _mutexDitches = new Mutex();
    private static Mutex _mutexBarriers = new Mutex();
    //private static Mutex _mutexQTable = new Mutex();

    //private int _id;

    /// <summary>
    /// EnergyLevel: 6 (0=0, 1=1-20, 2=21-40, 3=41-60, 4=61-80, 5=81-100) <br></br>
    /// HasSharedEnemy: 2 <br></br>
    /// HasEnemy: 2 <br></br>
    /// HasMoved: 2 <br></br>
    /// LocationType: 3 <br></br>
    /// CurrentStance: 3 <br></br>
    /// ActionPoints: 11 <br></br>
    /// WasTaggedLastTick: 2
    /// </summary>
    private double[,,,,,,,,]
        _qTable = new double[6, 2, 2, 2, 3, 3, 11, 2, Enum.GetValues(typeof(Action)).Length];

    private Random _random = new();
    private bool _endTick;
    
    #endregion

    #region Init

    public override void Init(PlayerMindLayer mindLayer)
    {
        UpdateTeamMates();
        _mindLayer = mindLayer;
        
        if (!_isTraining)
        {
            _learningRate = 0;
            _epsilon = 0.05;
        }
        
        _state = new State();
        LoadQTable();
    }

    #endregion

    #region Tick

    public override void Tick()
    {
        if (Body.ActionPoints < 10)
        {
            return;
        }

        _endTick = false;

        UpdateTeamMates();

        while (!_endTick || Body.ActionPoints > 1)
        {
            _state = TakeAction(_state);
        }

        if (_isTraining)
        {
            SaveQTable();
        }

        _state.SetHasMoved(false);
    }

    #endregion

    #region Learning

    public enum Action
    {
        Reload3, // higher reward, if RemainingShots == 0 
        ChangeStanceToStanding2,
        ChangeStanceToKneeling2,
        ChangeStanceToLying2,
        LookForDitches1,
        LookForHills1,
        LookForBarriers1,
        LookForEnemies1,

        // closest hill, ditch, enemy,etc
        MoveToHill,
        MoveToDitch,
        MoveToSharedEnemy,
        MoveToRandom,
        MoveToAlly,
        MoveToTarget,
        TagTarget5,
        EndTick
    }

    public class State
    {
        public int EnergyLevel; // 6 levels: (0=0, 1=1-20, 2=21-40, 3=41-60, 4=61-80, 5=81-100)
        public bool HasSharedEnemy;
        public bool HasTarget;
        public bool HasMoved;
        public Location LocationType;
        public Stance CurrentStance;
        public int ActionPoints { get; set; } // 0-10
        public bool WasTaggedLastTick { get; set; }

        public State()
        {
            EnergyLevel = 100;
            HasSharedEnemy = false;
            HasTarget = false;
            HasMoved = false;
            LocationType = Location.None;
            CurrentStance = Stance.Standing;
            ActionPoints = 10;
            WasTaggedLastTick = false;
        }

        public State(int energyLevel, bool hasSharedEnemy, bool hasTarget, bool hasMoved,
            Location locationType, Stance currentStance, int actionPoints, bool wasTaggedLastTick)
        {
            EnergyLevel = energyLevel;
            HasSharedEnemy = hasSharedEnemy;
            HasTarget = hasTarget;
            HasMoved = hasMoved;
            LocationType = locationType;
            CurrentStance = currentStance;
            ActionPoints = actionPoints;
            WasTaggedLastTick = wasTaggedLastTick;
        }

        public int GetEnergyLevel()
        {
            return EnergyLevel switch
            {
                < 1 => 0,
                <= 20 => 1,
                <= 40 => 2,
                <= 60 => 3,
                <= 80 => 4,
                <= 100 => 5,
                _ => -1
            };
        }

        public int GetHasSharedEnemy()
        {
            return HasSharedEnemy ? 1 : 0;
        }

        public int GetHasTarget()
        {
            return HasTarget ? 1 : 0;
        }

        public int GetHasMoved()
        {
            return HasMoved ? 1 : 0;
        }

        public int GetLocationType()
        {
            return (int)LocationType;
        }

        public int GetCurrentStance()
        {
            return (int)CurrentStance;
        }

        public int GetWasTaggedLastTick()
        {
            return WasTaggedLastTick ? 1 : 0;
        }

        public void SetHasMoved(bool newHasMoved)
        {
            HasMoved = newHasMoved;
        }
    }


    /// <summary>
    ///  
    /// </summary>
    private double GetReward(State currentState, Action action)
    {
        return _qTable[currentState.GetEnergyLevel(), currentState.GetHasSharedEnemy(),
            currentState.GetHasTarget(), currentState.GetHasMoved(),
            currentState.GetLocationType(), currentState.GetCurrentStance(),
            currentState.ActionPoints, currentState.GetWasTaggedLastTick(), (int)action];
    }

    private List<Action> GetValidActions(State currentState)
    {
        var validActions = new List<Action>();

        if (currentState.ActionPoints >= 1)
        {
            validActions.Add(Action.LookForDitches1);
            validActions.Add(Action.LookForHills1);
            validActions.Add(Action.LookForBarriers1);
            validActions.Add(Action.LookForEnemies1);

            if (!currentState.HasMoved)
            {
                validActions.Add(Action.MoveToRandom);
                validActions.Add(Action.MoveToAlly);
                if (_sharedHills.Any())
                {
                    validActions.Add(Action.MoveToHill);
                }

                if (_sharedDitches.Any())
                {
                    validActions.Add(Action.MoveToDitch);
                }

                if (_sharedEnemies.Any())
                {
                    validActions.Add(Action.MoveToSharedEnemy);
                }

                if (currentState.HasTarget)
                {
                    validActions.Add(Action.MoveToTarget);
                }
            }

            if (currentState.ActionPoints >= 2)
            {
                switch (currentState.CurrentStance)
                {
                    case Stance.Standing:
                        validActions.Add(Action.ChangeStanceToKneeling2);
                        validActions.Add(Action.ChangeStanceToLying2);
                        break;
                    case Stance.Kneeling:
                        validActions.Add(Action.ChangeStanceToStanding2);
                        validActions.Add(Action.ChangeStanceToLying2);
                        break;
                    case Stance.Lying:
                        validActions.Add(Action.ChangeStanceToStanding2);
                        validActions.Add(Action.ChangeStanceToKneeling2);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (currentState.ActionPoints >= 3)
                {
                    validActions.Add(Action.Reload3);
                    if (currentState.ActionPoints >= 5)
                    {
                        if (currentState.HasTarget)
                        {
                            validActions.Add(Action.TagTarget5);
                        }
                    }
                }
            }
        }

        validActions.Add(Action.EndTick);

        return validActions;
    }

    private State TakeAction(State currentState)
    {
        var validActions = GetValidActions(currentState);

        // Exploration
        if (_random.NextDouble() < _epsilon)
        {
            var randomIndexAction = _random.Next(0, validActions.Count);
            var action = validActions[randomIndexAction];
            var success = ExecuteAction(action);
            var nextState = GetNextState(currentState, action, success);
            var reward = CalculateReward(currentState, action, success);
            UpdateQValue(currentState, action, nextState, reward);


            return nextState;
        }

        // Exploitation
        double maxQValue = double.MinValue;
        Action? bestAction = null;

        foreach (var action in validActions)
        {
            double qValue = _qTable[currentState.GetEnergyLevel(), currentState.GetHasSharedEnemy(),
                currentState.GetHasTarget(), currentState.GetHasMoved(),
                currentState.GetLocationType(), currentState.GetCurrentStance(),
                currentState.ActionPoints, currentState.GetWasTaggedLastTick(), (int)action];
            if (qValue > maxQValue)
            {
                maxQValue = qValue;
                bestAction = action;
            }
        }

        if (bestAction != null)
        {
            var success = ExecuteAction((Action)bestAction);
            var nextState =
                GetNextState(currentState, (Action)bestAction,
                    success); // Replace with your own logic to determine the next state
            var reward = CalculateReward(currentState, (Action)bestAction, success);
            UpdateQValue(currentState, (Action)bestAction, nextState, reward);
            return nextState;
        }

        return currentState;
    }

    /// <summary>
    /// Determines the next state based on action success/failure
    /// </summary>
    /// <param name="currentState"></param>
    /// <param name="executedAction"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private State GetNextState(State currentState, Action executedAction, bool success)
    {
        var energyLevel = Body.Energy;
        var hasSharedEnemy = _sharedEnemy.Position != null;
        var hasTarget = _target.Position != null;

        var hasMoved = !currentState.HasMoved
            ? executedAction switch
            {
                Action.MoveToHill => success,
                Action.MoveToDitch => success,
                Action.MoveToSharedEnemy => success,
                Action.MoveToRandom => success,
                Action.MoveToAlly => success,
                Action.MoveToTarget => success,
                _ => currentState.HasMoved
            }
            : currentState.HasMoved;
        var locationType = CalcLocationType();
        var currentStance = Body.Stance;
        var actionPoints = Body.ActionPoints;
        var wasTaggedLastTick = Body.WasTaggedLastTick;

        var nextState = new State(energyLevel, hasSharedEnemy, hasTarget, hasMoved, locationType, currentStance,
            actionPoints, wasTaggedLastTick);

        return nextState;
    }

    /// <summary>
    /// Calculates LocationType based of agents stance and VisualRange
    /// </summary>
    /// <returns>LocationType the agent currently stands on</returns>
    private Location CalcLocationType()
    {
        return Body.Stance switch
        {
            Stance.Standing => Body.VisualRange switch
            {
                15 => Location.Hill,
                7 => Location.Ditch,
                _ => Location.None
            },
            Stance.Kneeling => Body.VisualRange switch
            {
                13 => Location.Hill,
                5 => Location.Ditch,
                _ => Location.None
            },
            _ => Body.VisualRange switch
            {
                10 => Location.Hill,
                2 => Location.Ditch,
                _ => Location.None
            }
        };
    }

    /// <summary>
    /// Calculating the reward based on action success/failure
    /// Reward:
    /// Individual-Reward:
    /// - tag enemy : 5
    /// - discover enemy : 2
    /// - discover hill : 1
    /// - discover ditch : 1
    /// - use same method more than once per tick: -2 (find sweetspot, so that exploring enemies multiple times doesn't reward with easy points)
    /// </summary>
    /// <param name="currentState"></param>
    /// <param name="action"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private double CalculateReward(State currentState, Action action, bool success)
    {
        double reward;

        // Reward for a successful action
        switch (action)
        {
            case Action.Reload3:
                reward = success ? 2 : -3;
                break;
            case Action.ChangeStanceToStanding2:
                reward = success ? 0.5 : -2;
                break;
            case Action.ChangeStanceToKneeling2:
                reward = success ? 0.5 : -2;
                break;
            case Action.ChangeStanceToLying2:
                reward = success ? 0.5 : -2;
                break;
            case Action.LookForDitches1:
                reward = success ? 1 : -1;
                break;
            case Action.LookForHills1:
                reward = success ? 1 : -1;
                break;
            case Action.LookForBarriers1:
                reward = success ? 1 : -1;
                break;
            case Action.LookForEnemies1:
                reward = success ? 2 : -1;
                break;
            case Action.MoveToHill:
                reward = success ? 0.2 : -1;
                break;
            case Action.MoveToDitch:
                reward = success ? 0.2 : -1;
                break;
            case Action.MoveToSharedEnemy:
                reward = success ? 1 : -1;
                break;
            case Action.MoveToRandom:
                reward = success ? 0.1 : -1;
                break;
            case Action.MoveToAlly:
                reward = success ? 0.5 : -1;
                break;
            case Action.MoveToTarget:
                reward = success ? 0.1 : -1;
                break;
            case Action.TagTarget5:
                reward = success ? 20 : -5;
                break;
            case Action.EndTick:
                reward = success ? 10 - (currentState.ActionPoints) : 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }

        return reward;
    }

    private void UpdateQValue(State currentState, Action selectedAction, State nextState, double reward)
    {
        double saReward = GetReward(currentState, selectedAction);

        double max = double.MinValue;
        for (int i = 0; i < Enum.GetValues(typeof(Action)).Length; i++)
        {
            var value = _qTable[nextState.GetEnergyLevel(), nextState.GetHasSharedEnemy(),
                nextState.GetHasTarget(), nextState.GetHasMoved(),
                nextState.GetLocationType(), nextState.GetCurrentStance(),
                nextState.ActionPoints, nextState.GetWasTaggedLastTick(), i];
            if (value > max)
            {
                max = value;
            }
        }

        double nsReward = max;

        double qCurrentState = saReward + (_discount * nsReward) + _learningRate * (reward - saReward);

        // Update Q-value in the Q-table
        _qTable[currentState.GetEnergyLevel(), currentState.GetHasSharedEnemy(),
                currentState.GetHasTarget(), currentState.GetHasMoved(),
                currentState.GetLocationType(), currentState.GetCurrentStance(),
                currentState.ActionPoints, currentState.GetWasTaggedLastTick(), (int)selectedAction] =
            (1 - _learningRate) * _qTable[currentState.GetEnergyLevel(), currentState.GetHasSharedEnemy(),
                currentState.GetHasTarget(), currentState.GetHasMoved(),
                currentState.GetLocationType(), currentState.GetCurrentStance(),
                currentState.ActionPoints, currentState.GetWasTaggedLastTick(), (int)selectedAction] +
            _learningRate * qCurrentState;
    }

    /// <summary>
    /// Execute the action in the environment
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private bool ExecuteAction(Action action)
    {
        return action switch
        {
            Action.Reload3 => Reload3(),
            Action.ChangeStanceToStanding2 => ChangeStanceToStanding2(),
            Action.ChangeStanceToKneeling2 => ChangeStanceToKneeling2(),
            Action.ChangeStanceToLying2 => ChangeStanceToLying2(),
            Action.LookForDitches1 => LookForDitches1(),
            Action.LookForHills1 => LookForHills1(),
            Action.LookForBarriers1 => LookForBarriers1(),
            Action.LookForEnemies1 => LookForEnemies1(),
            Action.MoveToHill => MoveToHill(),
            Action.MoveToDitch => MoveToDitch(),
            Action.MoveToSharedEnemy => MoveToSharedEnemy(),
            Action.MoveToRandom => MoveToRandom(),
            Action.MoveToAlly => MoveToAlly(),
            Action.MoveToTarget => MoveToTarget(),
            Action.TagTarget5 => TagTarget5(),
            Action.EndTick => EndTick(),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    /// <summary>
    /// Loading Q-Table from a file
    /// </summary>
    private void LoadQTable()
    {
        try
        {
            using (FileStream fileStream = File.Open(QtableSrc, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                // 1. Dimensionen lesen
                int[] lengths = new int[9];
                for (int i = 0; i < 9; i++)
                {
                    lengths[i] = reader.ReadInt32();
                }

                // 2. Array anlegen
                _qTable = new double[
                    lengths[0], lengths[1], lengths[2],
                    lengths[3], lengths[4], lengths[5],
                    lengths[6], lengths[7], lengths[8]];

                // 3. Werte lesen und einsetzen
                int[] indices = new int[9];
                foreach (var _ in _qTable)
                {
                    double value = reader.ReadDouble();

                    // Set value at current indices
                    _qTable.SetValue(value, indices);

                    // Indizes wie bei einem 9D-Zähler hochzählen
                    for (int d = 8; d >= 0; d--)
                    {
                        indices[d]++;
                        if (indices[d] < lengths[d]) break;
                        indices[d] = 0;
                    }
                }
            }

            //Console.WriteLine("Q-table loaded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading Q-table: " + ex.Message);
        }
    }


    /// <summary>
    /// Saving Q-Table to a file
    /// </summary>
    private void SaveQTable()
    {
        try
        {
            using (FileStream fileStream = File.Open(QtableSrc, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                // 1. Dimensionen speichern
                int rank = _qTable.Rank;
                for (int i = 0; i < rank; i++)
                {
                    writer.Write(_qTable.GetLength(i));
                }

                // 2. Werte speichern (flach durchiterieren)
                foreach (double value in _qTable)
                {
                    writer.Write(value);
                }
            }

            //Console.WriteLine("Q-table saved successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error saving Q-table: " + ex.Message);
        }
    }


    #endregion

    #region Actions

    /// <summary>
    /// Reloads magazine.
    /// </summary>
    /// <returns>False if not enough ActionPoints or if Magazine is already full</returns>
    private bool Reload3()
    {
        if (Body.ActionPoints < 3 || Body.RemainingShots == 5)
        {
            return false;
        }

        Body.Reload3();
        return true;
    }

    /// <summary>
    /// Change current stance to standing.
    /// </summary>
    /// <returns>False, if agent is already standing or does not have enough action points.</returns>
    private bool ChangeStanceToStanding2()
    {
        if (Body.ActionPoints < 2 || Body.Stance == Stance.Standing)
        {
            return false;
        }

        Body.ChangeStance2(Stance.Standing);
        return true;
    }

    /// <summary>
    /// Change current stance to kneeling.
    /// </summary>
    /// <returns>False, if agent is already kneeling or does not have enough action points.</returns>
    private bool ChangeStanceToKneeling2()
    {
        if (Body.ActionPoints < 2 || Body.Stance == Stance.Kneeling)
        {
            return false;
        }

        Body.ChangeStance2(Stance.Kneeling);
        return true;
    }

    /// <summary>
    /// Change current stance to lying.
    /// </summary>
    /// <returns>False, if agent is already lying or does not have enough action points.</returns>
    private bool ChangeStanceToLying2()
    {
        if (Body.ActionPoints < 2 || Body.Stance == Stance.Lying)
        {
            return false;
        }

        Body.ChangeStance2(Stance.Lying);
        return true;
    }

    /// <summary>
    /// Explores ditches and saves them to a shared HashSet.
    /// </summary>
    /// <returns>False if agent does not have enough action-points of if no new ditch has been found.</returns>
    private bool LookForDitches1()
    {
        if (Body.ActionPoints < 1)
        {
            return false;
        }

        var seenDitches = Body.ExploreDitches1();

        if (seenDitches.IsEmpty())
        {
            return false;
        }

        _mutexDitches.WaitOne();
        var oldSize = _sharedDitches.Count;
        foreach (var ditch in seenDitches)
        {
            _sharedDitches.Add(ditch);
        }

        var newSize = _sharedDitches.Count;
        _mutexDitches.ReleaseMutex();
        return newSize != oldSize;
    }

    /// <summary>
    /// Explores hills and saves them to a shared HashSet.
    /// </summary>
    /// <returns>False if agent does not have enough action-points of if no new hill has been found.</returns>
    private bool LookForHills1()
    {
        if (Body.ActionPoints < 1)
        {
            return false;
        }

        var seenHills = Body.ExploreHills1();

        if (seenHills.IsEmpty())
        {
            return false;
        }

        _mutexHills.WaitOne();
        var oldSize = _sharedHills.Count;
        foreach (var hill in seenHills)
        {
            _sharedHills.Add(hill);
        }

        var newSize = _sharedHills.Count;
        _mutexHills.ReleaseMutex();

        return newSize != oldSize;
    }

    /// <summary>
    /// Explores barriers and saves them to a shared HashSet.
    /// </summary>
    /// <returns>False if agent does not have enough action-points of if no new barrier has been found.</returns>
    private bool LookForBarriers1()
    {
        if (Body.ActionPoints < 1)
        {
            return false;
        }

        var seenBarriers = Body.ExploreBarriers1();

        if (seenBarriers.IsEmpty())
        {
            return false;
        }

        _mutexBarriers.WaitOne();
        var oldSize = _sharedBarriers.Count;
        foreach (var barrier in seenBarriers)
        {
            _sharedBarriers.Add(barrier);
        }

        var newSize = _sharedBarriers.Count;
        _mutexBarriers.ReleaseMutex();
        return newSize != oldSize;
    }

    /// <summary>
    /// Agent is exploring for and saves them to a shared dictionary.
    /// </summary>
    /// <returns>
    /// True if at least one new enemy has been found or if an enemy position has been updated.
    /// Otherwise false.
    /// </returns>
    private bool LookForEnemies1()
    {
        if (Body.ActionPoints < 1)
        {
            return false;
        }

        var seenEnemies = Body.ExploreEnemies1();

        if (seenEnemies.IsEmpty())
        {
            return false;
        }

        // set closest seen enemy as _target
        _target = seenEnemies.MinBy(enemy => Body.GetDistance(enemy.Position));

        _mutexEnemies.WaitOne();
        var oldSharedEnemies = _sharedEnemies.CreateCopy();
        foreach (var enemy in seenEnemies)
        {
            if (!_sharedEnemies.TryAdd(enemy.MemberId, enemy))
            {
                _sharedEnemies[enemy.MemberId] = enemy;
            }
        }

        var newSharedEnemies = _sharedEnemies.CreateCopy();
        _mutexEnemies.ReleaseMutex();

        _sharedEnemy = newSharedEnemies.First().Value;

        if (seenEnemies.Contains(_sharedEnemy))
        {
            // if the agent also sees the _sharedEnemy, target _sharedEnemy instead.
            _target = _sharedEnemy;
        }

        return !Equals(oldSharedEnemies, newSharedEnemies);
    }

    /// <summary>
    /// Moving to closest shared hill.
    /// </summary>
    /// <returns>True, if move-operation was successful. Otherwise false.</returns>
    private bool MoveToHill()
    {
        if (_state.HasMoved) return false;

        _mutexHills.WaitOne();
        if (_sharedHills.Any())
        {
            var hillPos = _sharedHills.MinBy(hill => Body.GetDistance(hill));
            _mutexHills.ReleaseMutex();
            return Body.GoTo(hillPos);
        }

        _mutexHills.ReleaseMutex();
        return false;
    }

    /// <summary>
    /// Moving to closest shared ditch.
    /// </summary>
    /// <returns>True, if move-operation was successful. Otherwise false.</returns>
    private bool MoveToDitch()
    {
        if (_state.HasMoved) return false;

        _mutexDitches.WaitOne();
        if (_sharedDitches.Any())
        {
            var ditchPos = _sharedDitches.MinBy(ditch => Body.GetDistance(ditch));
            _mutexDitches.ReleaseMutex();
            return Body.GoTo(ditchPos);
        }

        _mutexDitches.ReleaseMutex();

        return false;
    }

    /// <summary>
    /// Moving to closest shared enemy.
    /// </summary>
    /// <returns>True, if move-operation was successful. Otherwise false.</returns>
    private bool MoveToSharedEnemy()
    {
        if (_state.HasMoved) return false;

        _mutexEnemies.WaitOne();
        if (_sharedEnemies.Any())
        {
            var enemyPos = _sharedEnemies.MinBy(enemy =>
                Body.GetDistance(enemy.Value.Position)).Value.Position;
            _mutexEnemies.ReleaseMutex();
            return Body.GoTo(enemyPos);
        }

        _mutexEnemies.ReleaseMutex();
        return false;
    }

    /// <summary>
    /// Moving to a random position. Position will be randomized each time.
    /// </summary>
    /// <returns>True, if move-operation was successful. Otherwise false.</returns>
    private bool MoveToRandom()
    {
        if (_state.HasMoved) return false;

        var newX = RandomHelper.Random.Next(_mindLayer.Width);
        var newY = RandomHelper.Random.Next(_mindLayer.Height);
        return Body.GoTo(Position.CreatePosition(newX, newY));
    }

    /// <summary>
    /// Moving to closest teammate.
    /// </summary>
    /// <returns>True, if move-operation was successful. Otherwise false.</returns>
    private bool MoveToAlly()
    {
        if (_state.HasMoved) return false;

        _mutexTeammates.WaitOne();
        if (_teammates.Any())
        {
            var other = _teammates.Where(mate => !Equals(mate, Body)).ToList();
            if (other.Any())
            {
                var matePos = other.MinBy(mate => Body.GetDistance(mate.Position)).Position;
                _mutexTeammates.ReleaseMutex();
                return Body.GoTo(matePos);
            }
        }
        _mutexTeammates.ReleaseMutex();
        
        return false;
    }

    /// <summary>
    /// Moving to agents target position.
    /// </summary>
    /// <returns>True, if move-operation was successful. Otherwise false.</returns>
    private bool MoveToTarget()
    {
        if (_state.HasMoved) return false;
        if (_state.HasTarget)
        {
            return Body.GoTo(_target.Position);
        }

        return false;
    }

    /// <summary>
    /// Tagging target.
    /// </summary>
    /// <returns>True if tagging-operation was successful. Otherwise false.</returns>
    private bool TagTarget5()
    {
        if (Body.ActionPoints < 5 || !_state.HasTarget || _target.Position == null)
        {
            return false;
        }

        return Body.Tag5(_target.Position);
    }

    private bool EndTick()
    {
        _endTick = true;
        return true;
    }

    #endregion

    /// <summary>
    /// Chooses a team leader based on its body-id. Only team-members that are alive will be considered a leader.
    /// If the leader-body is equal to it's own, _isLeader will be set to true.
    /// </summary>
    private void UpdateTeamMates()
    {
        _mutexTeammates.WaitOne();
        var toBeRemoved = _teammates.Where(mate => !mate.Alive).ToList();

        if (toBeRemoved.Any())
        {
            foreach (var mate in toBeRemoved)
            {
                _teammates.Remove(mate);
            }
        }
        _mutexTeammates.ReleaseMutex();
    }
}