using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LaserTagBox.Model.Shared;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;
using Stance = LaserTagBox.Model.Shared.Stance;

namespace LaserTagBox.Model.Mind;

/// <summary>
/// A learning agent using Q-Learning and having a state, which consists of multiple components
/// constituted by different physical attributes, like energy and ammunition level.
/// </summary>
public class Example7QL : AbstractPlayerMind
{
    private PlayerMindLayer _mindLayer;
    private Position _goal;
    private QTableManager _qTableManager;
    private AgentState _state;
    public bool MovedWithinTick { get; private set; }
    public int AlertedCounter { get; private set; }
    public int CloseFriends { get; private set; }
    public HashSet<EnemySnapshot> Enemies = new();
    public HashSet<Position> Hills = new();
    public HashSet<Position> Ditches = new();
    private int _actionCount = 0;
    
    public Intel Intel { get; private set; }

    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
        Intel = Intel.GetInstance(_mindLayer.Width, _mindLayer.Height);
        _qTableManager = new QTableManager();
        _qTableManager.Read();
        _goal = default;
        _state = new AgentState(this);
    }

    /// <summary>
    /// Executes the Q-Learning algorithm as long as action points are left. After each action being
    /// selected, the agent's state will be executed accordingly and the agent will be given a reward,
    /// which leads to an update of the q table.
    /// At the end of the game the q table will be written to a file.
    /// </summary>
    public override void Tick()
    {
        Enemies = new();
        MovedWithinTick = false;
        if (_goal == default) _goal = Intel.GetGoal(Body.Position);
        AlertedCounter = Math.Max(--AlertedCounter, 0);
        CloseFriends = Body.ExploreTeam().Where(e => Body.GetDistance(e.Position) < 9).ToList().Count;

        _actionCount = 0;
        while (Body.ActionPoints > 0 && _actionCount < 5)
        {
            var action = _qTableManager.GetActionIndexByMaxQValue(_state);
            PerformAction(action);
            UpdateSight();
            _state.UpdateComponents();
        }
    }

    /// <summary>
    /// Maps int values in range 0-8 to action-methods.
    /// </summary>
    /// <param name="action"></param>
    /// <returns>reward</returns>
    /// <exception cref="Exception">if action > 8</exception>
    private void PerformAction(int action)
    {
        _actionCount++;
        switch (action)
        {
            case 0:
                Scout(); 
                break;
            case 1:
                ChargeEnemy();
                break;
            case 2: 
                GoToHill();
                break;
            case 3: 
                GoToDitch();
                break;
            case 4:
                ExploreDitches();
                break;
            case 5:
                ExploreEnemies();
                break;
            case 6:
                ExploreHills();
                break;
            case 7:
                LayDown();
                break;
            case 8:
                StandUp();
                break;
            case 9:
                CrouchDown();
                break;
            case 10:
                Reload();
                break;
            case 11:
                Tag();
                break;
            default: 
                throw new Exception($"action with index {action} is not defined!");
        }
    }

    /// <summary>
    /// Updates the things the team currently sees
    /// </summary>
    private void UpdateSight()
    {
        if (Intel.GetEnemies(_mindLayer.GetCurrentTick()).Count > 0) AlertedCounter = 10;
        Hills = Intel.GetHills();
        Ditches = Intel.GetDitches();
    }

    /// <summary>
    /// The Agent moves to the closest Hill
    /// </summary>
    /// <returns>reward</returns>
    private void GoToHill()
    { 
        GoTo(Hills.MinBy(e => Body.GetDistance(e)));
    }

    /// <summary>
    /// The Agent moves to the closest Ditch
    /// </summary>
    /// <returns>reward</returns>
    private void GoToDitch()
    { 
        GoTo(Ditches.MinBy(e => Body.GetDistance(e)));
    }

    /// <summary>
    /// The Agent charges to the enemy
    /// </summary>
    /// <returns>reward</returns>
    private void ChargeEnemy()
    {
        if (Enemies.Count > 0)
          GoTo(FindClosestEnemy(Enemies).Position);

        if (_state.Components["alerted"] == 0)
            GoTo(Intel.GetLastKnownPosition());

        GoTo(Intel.GetLastKnownPosition());
    }

    /// <summary>
    /// The Agent scouts the map
    /// </summary>
    /// <returns>reward</returns>
    private void Scout()
    {
        GoTo(_goal);
    }

    /// <summary>
    /// Changes the agent's stance to lying and rewards action according to whether the agent
    /// is in combat and the old stance differs or not
    /// </summary>
    /// <returns>reward</returns>
    private void LayDown()
    {
        Body.ChangeStance2(Stance.Lying);
    }

    /// <summary>
    /// Changes the agent's stance to standing and rewards action according to whether the agent
    /// is not in combat and the old stance differs or not
    /// </summary>
    /// <returns>reward</returns>
    private void StandUp()
    {
        Body.ChangeStance2(Stance.Standing);
    }

    /// <summary>
    /// Changes the agent's stance to kneeling  and rewards action according to whether the agent
    /// is in combat and the old stance differs or not
    /// </summary>
    /// <returns>reward</returns>
    private void CrouchDown()
    {
        Body.ChangeStance2(Stance.Kneeling);
    }

    /// <summary>
    /// Executes the agent's ExploreHills1() method and adds found hills to the shared Intel object. The action
    /// will be rewarded depending on whether the agent is on a hill already or not.
    /// </summary>
    /// <returns>reward</returns>
    private void ExploreHills()
    {
        Intel.AddHills(_mindLayer.GetCurrentTick(), Body.ExploreHills1());
    }

    /// <summary>
    /// Executes the agent's ExploreDitches1() method and adds found ditches to the shared Intel object. The action
    /// will be rewarded depending on whether the agent is in a ditch already or not.
    /// </summary>
    /// <returns>reward</returns>
    private void ExploreDitches()
    {
        Intel.AddDitches(_mindLayer.GetCurrentTick(), Body.ExploreDitches1());
    }

    /// <summary>
    /// Executes the agent's ExploreEnemies() method and adds found enemies to the shared Intel object. The action
    /// will be rewarded depending on whether the agent has found enemies or not.
    /// </summary>
    /// <returns>reward</returns>
    private void ExploreEnemies()
    {
        var enemies = Body.ExploreEnemies1();
        if (enemies != null && enemies.Count != 0)
        {
            Enemies.UnionWith(enemies);
            Intel.AddEnemies(_mindLayer.GetCurrentTick(), Enemies);
        }
    }

    /// <summary>
    /// Before going to _goal, it is checked whether an agent has seen an enemy, if yes, the _goal is set to the last
    /// known position of the enemy.
    /// </summary>
    /// <returns>reward</returns>
    private void GoTo(Position position)
    {
        var validGoal = Body.GoTo(position);
        MovedWithinTick = true;

        if (!validGoal)
        {
            _goal = Intel.GetGoal(_goal);
        }
    }

    /// <summary>
    /// Chooses the closest Enemy (if any was found) and executes the agent's Tag5() method. If no enemy was found
    /// the agent executes Tag5() on a random position.
    /// </summary>
    /// <returns>reward</returns>
    private void Tag()
    {
        if (Enemies.Count > 0)
        {
            Body.Tag5(FindClosestEnemy(Enemies).Position);
            return;
        }
        if (AlertedCounter == 10)
        {
            Body.Tag5(FindClosestEnemy(Intel.GetEnemies(_mindLayer.GetCurrentTick())).Position);
            return;
        }
        Body.Tag5(Position.CreatePosition(0, 0));
    }

    /// <summary>
    /// Executes the agent's Reload3() method in order to refresh the ammunition level. The reward depends on
    /// how many shots were reloaded. If remaining shots is equal to max amount of shots the reward is negative.
    /// </summary>
    /// <returns>reward</returns>
    private void Reload()
    {
        Body.Reload3();
    }

    /// <summary>
    /// Attack the closest Enemy. If remaining shots is 0, reload, in order to not waste any action points.
    /// </summary>
    private EnemySnapshot FindClosestEnemy(HashSet<EnemySnapshot> enemies)
    {

        return enemies.Where(e => e.Position != null)
            .OrderBy(e => Body.GetDistance(e.Position))
            .FirstOrDefault();
    }
}

public class Intel
{
    private static readonly object LockEnemyObject = new();
    private static readonly object LockHillObject = new();
    private static readonly object LockDitchObject = new();
    private static readonly object LockGoalObject = new();
    private Position _lastKnownEnemyPosition;
    private HashSet<EnemySnapshot> _enemies;
    private readonly HashSet<Position> _hills;
    private readonly HashSet<Position> _ditches;
    private long _enemyTick = -1;
    private Position _goal;
    private readonly int _envWidth;
    private readonly int _envHeight;
    private static Intel _instance;
    public Intel(int width, int height)
    {
        _envWidth = width;
        _envHeight = height;
        _lastKnownEnemyPosition = Position.CreatePosition(width / 2, height / 2);
        _enemies = new HashSet<EnemySnapshot>();
        _hills = new HashSet<Position>();
        _ditches = new HashSet<Position>();
    }

    /// <summary>
    /// Returns the last known position of an enemy.
    /// </summary>
    /// <returns></returns>
    public Position GetLastKnownPosition()
    {
        return _lastKnownEnemyPosition;
    }

    public Position GetGoal(Position currentPosition)
    {
        lock (LockGoalObject)
        {
            if (_goal == default || currentPosition.Equals(_goal))
            {
                var x = RandomHelper.Random.Next(10, _envWidth -10);
                var y = RandomHelper.Random.Next(10, _envHeight -10);
                _goal = Position.CreatePosition(x, y);
            }
            return _goal;
        }
    }

    /// <summary>
    /// Updates the enemy list based on the current tick. If current tick is incremented, the list will be
    /// emptied.
    /// </summary>
    /// <param name="curTick">the current tick the agent is at</param>
    private void UpdateEnemyList(long curTick)
    {
        if (_enemyTick != curTick)
        {
            lock (LockEnemyObject)
            {
                _enemyTick = curTick;
                _enemies.Clear();
            }
        }
    }

    /// <summary>
    /// Adds all enemies an agents has explored in the current tick. 
    /// </summary>
    /// <param name="curTick">the current tick the agent is at</param>
    /// <param name="enemies">explored enemies</param>
    /// <returns>false, if method is called twice or no enemies found, otherwise true</returns>
    public bool AddEnemies(long curTick, HashSet<EnemySnapshot> enemies)
    {
        UpdateEnemyList(curTick);
        if (enemies == null || enemies.Count == 0) return false;
        var oldSize = enemies.Count;
        _lastKnownEnemyPosition = enemies.First().Position;
        lock (LockEnemyObject)
        {
            _enemies.UnionWith(enemies);
        }
        return _enemies.Count > oldSize;
    }
    
    /// <summary>
    /// Returns all enemies explored by all members at the current tick.
    /// </summary>
    /// <param name="curTick">the current tick the agent is at</param>
    /// <returns>set of enemies explored</returns>
    public HashSet<EnemySnapshot> GetEnemies(long curTick)
    {
        lock (LockEnemyObject)
        {
            UpdateEnemyList(curTick);
            return new HashSet<EnemySnapshot>(_enemies);   
        }
    }

    /// <summary>
    /// Adds all hills the agent has explored at the current tick.
    /// </summary>
    /// <param name="curTick">the current tick the agent is at</param>
    /// <param name="hills">explored hills</param>
    /// <returns>true, if hills were added - false, if no hills found</returns>
    public bool AddHills(long curTick, List<Position> hills)
    {
        lock (LockHillObject)
        {
            var hillCount = _hills.Count;
            if (hills != null && hills.Count == 0) return false;
            if (hills != null) _hills.UnionWith(hills);
            return _hills.Count > hillCount;
        }
    }

    /// <summary>
    /// Returns all explored hills for the current tick.
    /// </summary>
    /// <returns>list of explored hills</returns>
    public HashSet<Position> GetHills()
    {
        lock (LockHillObject)
        {
            return new HashSet<Position>(_hills);
        }
    }
    
    /// <summary>
    /// Adds all ditches the agent has explored at the current tick.
    /// </summary>
    /// <param name="curTick">the current tick the agent is at</param>
    /// <returns>true, if hills were added - false, no ditches found</returns>
    public bool AddDitches(long curTick, List<Position> ditches)
    {
        lock (LockDitchObject)
        {
            var ditchCount = _ditches.Count;
            if (ditches != null && ditches.Count == 0) return false;
            if (ditches != null) _ditches.UnionWith(ditches);
            return _ditches.Count > ditchCount;
        }
    }
     
    /// <summary>
    /// Returns all explored ditches for the current tick.
    /// </summary>
    /// <returns>list of explored ditches</returns>
    public HashSet<Position> GetDitches()
    {
        lock (LockDitchObject)
        {
            return new HashSet<Position>(_ditches);
        }
    }

    public static Intel GetInstance(int width, int height)
    {
        if (_instance != null)
        {
            return _instance;
            
        }
        _instance = new Intel(width, height);
        return _instance;
    }
}

public class AgentState
{
    private readonly Example7QL _agent;
    public Dictionary<string, int> Components { get; }

    public AgentState(Example7QL agent)
    {
        _agent = agent;
        Components = ComponentLookUp.Table.Keys.ToDictionary(key => key, _ => 0);
        UpdateComponents();
    }

    /// <summary>
    /// Updates the agnet's state components, which will change as soon as the agent performs actions.
    /// </summary>
    public void UpdateComponents()
    {
        DiscretizeEnergyLevel();
        DiscretizeAmmunitionLevel();
        DiscretizeStance();
        DiscretizeActionPoints();
        DiscretizeLocation();
        DiscretizeHasMoved();
        DiscretizeCloseDitch();
        DiscretizeCloseHill();
        DiscretizeCloseEnemy();
        DiscretizeCloseTeamPartners();
        DiscretizeAlerted();
    }

    /// <summary>
    /// Discretizes the agent's energy to values Critical, Low, Medium or High and sets the corresponding
    /// variable based on the current value.
    /// </summary>
    private void DiscretizeEnergyLevel()
    {
        Components["energyLevel"] = _agent.Body.Energy switch
        {
            < 31 => 0,
            _    => 1
        };
    }

    /// <summary>
    /// Discretizes the agent's ammunition to values Empty, Medium or High and sets the corresponding
    /// variable based on the current value.
    /// </summary>
    private void DiscretizeAmmunitionLevel()
    {
        Components["ammunitionLevel"] = _agent.Body.RemainingShots switch
        {
            0   => 0,
            < 5 => 1,
            _   => 2
        };
    }

    /// <summary>
    /// Discretizes the agent's stance to values Standing, Kneeling or Lying and sets the corresponding
    /// variable based on the current value.
    /// </summary>
    private void DiscretizeStance()
    {
        Components["stance"] = _agent.Body.Stance switch
        {
            Stance.Standing => 0,
            Stance.Kneeling => 1,
            _                      => 2
        };
    }

    /// <summary>
    /// Discretizes the agent's action points to values in range 0-10 and sets the corresponding
    /// variable based on the current value.
    /// </summary>
    private void DiscretizeActionPoints()
    {
        Components["actionPoints"] = _agent.Body.ActionPoints switch
        {
            0    => 0, 
            1    => 1,
            2    => 2,
            <= 4 => 3,
            _    => 4
        };
    }

    /// <summary>
    /// Discretizes whether the agent has moved in the current tick.
    /// </summary>
    private void DiscretizeHasMoved()
    {
        Components["hasMoved"] = _agent.MovedWithinTick ? 0 : 1;
    }
    
    /// <summary>
    /// Discretizes whether the agent is alerted.
    /// </summary>
    private void DiscretizeAlerted()
    {
        Components["alerted"] = _agent.AlertedCounter > 0 ? 0 : 1;
    }
    
    /// <summary>
    /// Discretizes whether the agent is close to a hill.
    /// </summary>
    private void DiscretizeCloseHill()
    {
        Components["closeHill"] = _agent.Hills.Where(e => _agent.Body.GetDistance(e) < 11).Any() ? 
            0 : 1;
    }
    
    /// <summary>
    /// Discretizes whether the agent is close to a ditch.
    /// </summary>
    private void DiscretizeCloseDitch()
    {
        Components["closeDitch"] = _agent.Ditches.Where(e => _agent.Body.GetDistance(e) < 11).Any() ? 
            0 : 1;
    }
    
    /// <summary>
    /// Discretizes whether an enemy is close.
    /// </summary>
    private void DiscretizeCloseEnemy()
    {
        Components["closeEnemy"] = _agent.Enemies.Count > 0 ? 0 : 1;
    }
    
    /// <summary>
    /// Discretizes how many Teampartners are in close range.
    /// </summary>
    private void DiscretizeCloseTeamPartners()
    {
        Components["closeFriends"] = _agent.CloseFriends > 0 ? 0 : 1;
    }

    /// <summary>
    /// Discretizes the agent's location to the values InDitch, OnHill or OnGround based on the agent's
    /// current position and the position of known ditches and hills.
    /// </summary>
    private void DiscretizeLocation()
    {
        if (_agent.Intel.GetDitches().Contains(_agent.Body.Position)) Components["location"] = 2;
        else if (_agent.Intel.GetHills().Contains(_agent.Body.Position)) Components["location"] = 1;
        else Components["location"] = 0;
    }

    /// <summary>
    /// Calculates the state's q table index by using the component's bin length (Binning).
    /// </summary>
    /// <returns></returns>
    public int GetQTableIndex()
    {
        return Components.Select(pair =>
        {
            var key = pair.Key;
            var value1 = pair.Value;
            var value2 = ComponentLookUp.BinLengths[key];
            return value1 * value2;
        }).Sum();
    }
    
    private static class ComponentLookUp
    {
        public static readonly Dictionary<string, int[]> Table = new()
        {
            { "energyLevel", new[] { 0, 1 } },
            { "ammunitionLevel", new[] { 0, 1, 2 } },
            { "hasMoved", new[] { 0, 1 } },
            { "closeHill", new[] { 0, 1 } },
            { "closeDitch", new[] { 0, 1 } },
            { "closeEnemy", new[] { 0, 1 } },
            { "closeFriends", new[] { 0, 1 } },
            { "alerted", new[] { 0, 1 } },
            { "stance", new[] { 0, 1, 2 } },
            { "actionPoints", new[] { 0, 1, 2, 3, 4 } },
            { "location", new[] { 0, 1, 2 } },
        };

        public static readonly Dictionary<string, int> BinLengths = Table.Keys.ToDictionary(
            key => key,
            key => Table.Values
                .Where((_, i) => i > Table.Keys.ToList().IndexOf(key))
                .Aggregate(1, (acc, values) => acc * values.Length)
        );
    }
}

public class QTableManager
{
    private static readonly object ReadLocker = new();
    private double[,] _qTable;

    public void Read()
    {
        lock (ReadLocker)
        {
            var filePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, 
                "Model", "Mind", "Examples", "Example7_qtable.json");
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);

                var options = new JsonSerializerOptions { WriteIndented = true };

                var jaggedArray = JsonSerializer.Deserialize<double[][]>(json, options);
                _qTable = ToMultiDimensionalArray(jaggedArray);
            } else throw new Exception("Unable to load QTable for HeWasNumberOne!");
        }
    }
    
    private static double[,] ToMultiDimensionalArray(double[][] jaggedArray)
    {
        var rows = jaggedArray.Length;
        var cols = jaggedArray[0].Length;

        var array = new double[rows, cols];

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                array[i, j] = jaggedArray[i][j];
            }
        }

        return array;
    }
    
    public int GetActionIndexByMaxQValue(AgentState state)
    {
        var maxValueAction = 0;
        var maxQValue = _qTable[state.GetQTableIndex(), 0];
        for (var i = 1; i < 12; i++)
        {
            var next = _qTable[state.GetQTableIndex(), i];
            if (next > maxQValue)
            {
                maxQValue = next;
                maxValueAction = i;
            }
        }
        return maxValueAction;
    }
}
