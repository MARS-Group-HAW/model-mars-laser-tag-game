using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LaserTagBox.Model.Body;
using LaserTagBox.Model.Mind.States;
using LaserTagBox.Model.Shared;
using Mars.Common;
using Mars.Common.Core.Collections;
using Mars.Components.Common;
using Mars.Components.Environments;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Numerics.Statistics;
using ServiceStack;
using Distance = Mars.Numerics.Distance;

namespace LaserTagBox.Model.Mind {
    public class MasterOfDisasterMind : AbstractPlayerMind, IOurPlayerMind {
        private static HashSet<Position> BarrierList { get; set; }
        private static Dictionary<Guid, EnemySnapshot> EnemyList { get; set; }
        private static Dictionary<int, FriendSnapshot> FriendList { get; set; }

        private static int _idTicker;

        private static SensedMap _EnvMap { get; set; }

        private static readonly string enemyType = "enemy";
        private static readonly string exploredType = "explored";
        private static readonly string barrierType = "barrier";
        private static readonly string enemyLastSeenType = "eLastSeen";
        private static readonly string barrierPath = "BarrierOut.csv";
        private static int OFFSET_CIRCLE = 10;
        private bool death = false;
        private Position passiveGoal;
        private bool wrongOrder = true;


        private int AgentId { get; set; }


        private int CurrTick { get; set; }

        private static Position _goal;
        private static double EXPLORE_RANGE = 20;
        private ExploreState _exploreState;

        private int leaderID = 1;

        private static StreamWriter _csvStreamWriter = null;


        public override void Init(PlayerMindLayer mindLayer) {
            //do something
            BarrierList = new HashSet<Position>();
            EnemyList = new Dictionary<Guid, EnemySnapshot>();
            FriendList = new Dictionary<int, FriendSnapshot>();
            CurrTick = 0;
            AgentId = GenNewId();

            if (_csvStreamWriter == null) {
                if (File.Exists(barrierPath)) {
                    File.Delete(barrierPath);
                }

                _csvStreamWriter = File.CreateText(barrierPath);
                _csvStreamWriter.AutoFlush = false;
                _csvStreamWriter.Write("tick;type;posX;posY");
                
                if (_EnvMap == null) {
                    _EnvMap = new SensedMap();
                    var boundingBarriers = new List<Position>();
                    for (int i = 0; i < 51; i++) {
                        boundingBarriers.Add(new Position(i,51));
                        boundingBarriers.Add(new Position(51,i));
                    }
                    _EnvMap.addBarriers(boundingBarriers);
                    SaveNewBarriers(boundingBarriers);
                    
                }
            }

            _exploreState = new ExploreState(this, _EnvMap);
            passiveGoal = (Position) Body.Position.Clone();
        }

        ~MasterOfDisasterMind() {
            _csvStreamWriter.Dispose();
        }


        public override void Tick() {
            // TODO measure time
            CurrTick++;
            if (IsLeader()) {
                if (!wrongOrder) {
                    UpdateAndExploreLastSeenEnemies(); //Uses 1 Point
                }

                UpdateAndExploreBarriers(); // Uses 1 Point
                if (Body.Energy < 20) {
                    setLeaderID(GetAgentWithHighestEnergy());
                }
            }

            if (wrongOrder) {
                UpdateAndExploreLastSeenEnemies(); //Uses 1 Point
            }

            if (MyHealthLow()) {
                Passive();
            }
            else if (EnemyDetected()) {
                Aggressive();
            }
            else {
                Explore();
            }

            RefreshFriendList();
            UseUpSparePoints();

            foreach (var pos in _EnvMap.getNewExploredAreas()) {
                WritePosToCsv(CurrTick, pos, exploredType);
            }

            if (Body.Energy < 0 && !death) {
                //Console.WriteLine("Agent: " + AgentId + " died");
                death = true;
            }
            else if (Body.WasTaggedLastTick) {
                //Console.WriteLine("Agent: " + AgentId + " has " + Body.Energy + " Energy left");
            }

            if (CurrTick >= 1800) {
                _csvStreamWriter.Flush();
                //Console.WriteLine("FLUSHED the Toilet");
                //Console.WriteLine("**** SCOREPOINTS *****");
                //Console.WriteLine(Body.GamePoints);
            }
        }

        public enum Movement {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            UPLEFT,
            UPRIGHT,
            DOWNLEFT,
            DOWNRIGHT,
            NOTHING
        }

        private Position GetMovedPosition(Movement mv) {
            int maxMovement = 5;
            Position pos = (Position) Body.Position.Clone();
            switch (mv) {
                case Movement.UP:
                    pos.Y -= maxMovement;
                    break;
                case Movement.DOWN:
                    pos.Y += maxMovement;
                    break;
                case Movement.LEFT:
                    pos.X -= maxMovement;
                    break;
                case Movement.RIGHT:
                    pos.X += maxMovement;
                    break;
                case Movement.UPLEFT:
                    pos.X -= maxMovement;
                    pos.Y -= maxMovement;
                    break;
                case Movement.UPRIGHT:
                    pos.X += maxMovement;
                    pos.Y -= maxMovement;
                    break;
                case Movement.DOWNLEFT:
                    pos.Y += maxMovement;
                    pos.X -= maxMovement;
                    break;
                case Movement.DOWNRIGHT:
                    pos.Y += maxMovement;
                    pos.X += maxMovement;
                    break;
            }

            var obj = _EnvMap.get(pos);
            if (obj == SensedMap.SensedObjects.Explored) {
                return pos;
            }

            return (Position) Body.Position.Clone();
        }

        private double CalculateDistanceToEnemys(Position pos) {
            double distance = 0;
            foreach (var val in EnemyList.Values) {
                distance += pos.DistanceInMTo(val.Position);
            }

            return distance;
        }

        private void Passive() {
            //Stand up if lying
            if (Body.Stance != Stance.Standing) {
                Body.ChangeStance2(Stance.Standing);
            }

            if (EnemyList.Count > 0) {
                passiveGoal = GetNewPassiveGoalPosition();
            }

            EnemySnapshot enemySnapshot = GetBestEnemy(0); //TODO eval. Closest und den mit den niedrigsten Leben
            if (enemySnapshot.Position != null) {
                while (Body.ActionPoints >= 5 && Body.RemainingShots >= 1) {
                    var result = Body.Tag5(enemySnapshot.Position);
                    if (result) {
                        //Console.WriteLine(AgentId + " Enemy Hit.. scorePoint: " + Body.GamePoints); //TODO print how many lives left
                    }
                    else {
                        //Console.WriteLine(AgentId + " Enemy Missed");
                    }
                }
            }

            Body.GoTo(passiveGoal); // TODO return value handle
            //Lie down
            if (Body.Stance != Stance.Lying) {
                Body.ChangeStance2(Stance.Lying);
            }

            if (!(Body.Energy < 0)) {
                //Console.WriteLine("Agent: " + AgentId + " is passive");
            }
        }

        private Position GetNewPassiveGoalPosition() {
            //Check where to run away
            var bestMovement = Movement.NOTHING;
            var bestDistance = 0.0;
            foreach (Movement move in Enum.GetValues(typeof(Movement))) {
                //getPosition
                var distance = CalculateDistanceToEnemys(GetMovedPosition(move));
                if (bestDistance < distance) {
                    bestMovement = move;
                    bestDistance = distance;
                }
            }

            //Move to position
            return GetMovedPosition(bestMovement);
        }

        private void UseUpSparePoints() {
            if (Body.Energy >= 3 && Body.RemainingShots < 3) {
                Body.Reload3();
            }

            switch (Body.Energy) {
                case var _ when Body.Energy > 2:
                    UpdateAndExploreBarriers();
                    Body.ExploreDitches1();
                    Body.ExploreHills1();
                    break;
                case 2:
                    UpdateAndExploreBarriers();
                    Body.ExploreDitches1();
                    break;
                case 1:
                    UpdateAndExploreBarriers();
                    break;
            }
        }

        private void Aggressive() {
            // mb explore enemies here
            if (Body.RemainingShots == 0) {
                Body.Reload3();
            }
            else if (Body.Stance != Stance.Lying) {
                // would cost to much points
                Body.ChangeStance2(Stance.Lying);
            }

            EnemySnapshot enemySnapshot = GetBestEnemy(0); //TODO eval. Closest und den mit den niedrigsten Leben

            while (Body.ActionPoints >= 5 && Body.RemainingShots >= 1) {
                var result = Body.Tag5(enemySnapshot.Position);
                if (result) {
                    //Console.WriteLine(AgentId + " Enemy Hit.. scorePoint: " + Body.GamePoints); //TODO print how many lives left
                }
                else {
                    //Console.WriteLine(AgentId + " Enemy Missed");
                }
            }

            if (Body.WasTaggedLastTick && EnemyList.Count > 0) {
                _goal = GetNewPassiveGoalPosition();
            }
            else if (Body.GetDistance(enemySnapshot.Position) > 4) {
                _goal = enemySnapshot.Position; //! Hunt down the Enemy
            }
            else {
                _goal = Body.Position;
            }

            _goal ??= Position.CreatePosition(RandomHelper.Random.Next(50), RandomHelper.Random.Next(50)); // TODO
            // check if go towards enemy or hold position
            //Console.WriteLine("Agent: " + AgentId + " is aggressive");

            if (!Body.GoTo(_goal)) {
                //Console.WriteLine("something went wrong while moving in aggressive");
            }
        }

        private void Explore() {
            // Console.WriteLine("Agent: " + AgentId + " is exploring");
            if (Body.Stance != Stance.Standing) {
                Body.ChangeStance2(Stance.Standing);
            }

            _exploreState.tick();

            var pos = _EnvMap.Explore(Body.Position, 10);
            var anzahl = pos.Count();
            EnemySnapshot
                enemySnapshot = GetBestEnemy(Body.Energy - 5); //TODO eval. Closest und den mit den niedrigsten Leben
            if (enemySnapshot.Position != null) {
                while (Body.ActionPoints >= 5 && Body.RemainingShots >= 1) {
                    var result = Body.Tag5(enemySnapshot.Position);
                    if (result) {
                        //Console.WriteLine(AgentId + " Enemy Hit.. scorePoint: " + Body.GamePoints); //TODO print how many lives left
                    }
                    else {
                        //Console.WriteLine(AgentId + " Enemy Missed");
                    }
                }
            }
        }

        private bool MyHealthLow() {
            return Body.Energy <= 31;
        }

        public void setLeaderID(int id) {
            leaderID = id;
        }

        public bool IsLeader() {
            return AgentId == leaderID;
        }

        public IPlayerBody getBody() {
            return Body;
        }

        private void UpdateAndExploreBarriers() {
            var barrierList = Body.ExploreBarriers1();
            if (barrierList == null) {
                barrierList = new List<Position>();
            }

            // if (barrierList != null && barrierList.Count > 0) {
            SaveNewBarriers(barrierList); // Saves them to CSV
            _EnvMap.addBarriers(barrierList);
            // }

            // UpdateSensedMap(BarrierList);
            //LookForBarriersInMyEnvironment();
            _EnvMap.senseEnvironment(Body.Position, barrierList); // calls own beeline
        }


        private bool EnemyDetected() {
            double minX = Double.MaxValue;
            double maxX = Double.MinValue;
            double minY = Double.MaxValue;
            double maxY = Double.MinValue;
            foreach (var item in FriendList.Values) {
                minX = Math.Min(minX, item.Position.X);
                maxX = Math.Max(maxX, item.Position.X);
                minY = Math.Min(minY, item.Position.Y);
                maxY = Math.Max(maxY, item.Position.Y);
            }

            foreach (var enemy in EnemyList.Values) {
                if ((enemy.Position.X < maxX + OFFSET_CIRCLE && enemy.Position.X > minX - OFFSET_CIRCLE) &&
                    (enemy.Position.Y < maxY + OFFSET_CIRCLE && enemy.Position.Y > minY - OFFSET_CIRCLE)) {
                    return true;
                }
            }

            return false;
        }

        private void RefreshFriendList() {
            if (FriendList.ContainsKey(AgentId)) {
                FriendList.Remove(AgentId);
            }

            if (Body.Energy > 0) {
                FriendList.Add(AgentId,
                    new FriendSnapshot(new Guid(), Body.Color, Body.Stance, Body.Position, Body.Energy,
                        Body.VisualRange, Body.VisibilityRange));
            }
        }

        private int GetAgentWithHighestEnergy() {
            int agentId = 0;
            int maxAgentHealth = Int32.MinValue;
            foreach (var agent in FriendList) {
                if (agent.Value.Energy > maxAgentHealth) {
                    agentId = agent.Key;
                    maxAgentHealth = agent.Value.Energy;
                }
            }

            return agentId;
        }

        private void UpdateSensedMap(HashSet<Position> barrierList) {
            foreach (var barr in barrierList) {
                _EnvMap[(int) Math.Round(barr.X), (int) Math.Round(barr.Y)] = SensedMap.SensedObjects.Barrier;
            }

            foreach (var barrier in barrierList) {
                for (int i = 0; i <= Body.VisualRange; i++) {
                    //if(Distance.SquareEuclidean())
                    PositionHelper.CalculatePosition(Body.Position.X, Body.Position.Y, barrier.X, barrier.Y, i);
                }
            }


            //TODO Add explored Area to map
        }

        private HashSet<Position> LookForBarriersInMyEnvironment() {
            HashSet<Position> surroundingBarriers = new HashSet<Position>();
            foreach (var pos in BarrierList) {
                int xDiff = (int) pos.X - (int) Body.Position.X;
                int yDiff = (int) pos.Y - (int) Body.Position.Y;
                if ((xDiff <= 1 && xDiff >= -1) && (yDiff <= 1 && yDiff >= -1)) {
                    surroundingBarriers.Add(pos);
                }
            }

            return surroundingBarriers;
        }

        private EnemySnapshot GetBestEnemy(int maximumEnergyInput) {
            //Console.WriteLine("Chance of tagging Left Value: " + leftValue + " Right Value: " + rightValue); */
            int energyInputLeft = maximumEnergyInput;
            EnemySnapshot closestEnemySnapshot = new EnemySnapshot();
            List<EnemySnapshot> closestEnemy = new List<EnemySnapshot>();
            closestEnemy = EnemyList.Values.ToList();
            closestEnemy = closestEnemy.OrderBy(o =>
                Distance.SquareEuclidean(o.Position.X, o.Position.Y, Body.Position.X, Body.Position.Y)).ToList();
            if (!closestEnemy.IsEmpty()) {
                closestEnemySnapshot = closestEnemy[0];
                int bestValue = Int32.MinValue;
                foreach (var enemy in closestEnemy) {
                    if (GetEnemySpotValue(enemy.Position) + GetEnemyStanceValue(enemy) > bestValue) {
                        if (_EnvMap.OwnHasBeeline(Body.Position, enemy.Position) > 90) {
                            bestValue = GetEnemySpotValue(enemy.Position) + GetEnemyStanceValue(enemy);
                            closestEnemySnapshot = enemy;
                        }
                        else if (energyInputLeft > 0) {
                            energyInputLeft--;
                            if (Body.HasBeeline1(enemy.Position)) {
                                bestValue = GetEnemySpotValue(enemy.Position) + GetEnemyStanceValue(enemy);
                                closestEnemySnapshot = enemy;
                            }
                        }
                    }
                }
            }

            return closestEnemySnapshot;
        }

        private int GetEnemySpotValue(Position pos) {
            var obj = _EnvMap.get(pos);
            var enemySpot = obj switch {
                SensedMap.SensedObjects.Ditch => 0, // in ditch
                SensedMap.SensedObjects.Hill => 2, // on hill
                _ => 1 // on normal ground
            };
            return enemySpot;
        }

        private int GetEnemyStanceValue(EnemySnapshot enemySnapshot) {
            var enemyStance = enemySnapshot.Stance switch {
                Stance.Standing => 2,
                Stance.Kneeling => 1,
                Stance.Lying => 0,
                _ => throw new ArgumentOutOfRangeException()
            };
            return enemyStance;
        }

        private Position LookForEnemies() {
            Position pos = null;
            foreach (var enemy in EnemyList) {
                pos = enemy.Value.Position;
            }

            return pos;
        }

        private void UpdateAndExploreLastSeenEnemies() {
            var lastSeenEnemies = new HashSet<Guid>();
            lastSeenEnemies.AddRange(EnemyList.Keys);

            var enemyList = Body.ExploreEnemies1();
            if (enemyList == null) {
                enemyList = new List<EnemySnapshot>();
            }


            if (!enemyList.IsEmpty()) {
                foreach (var enemy in enemyList) {
                    WritePosToCsv(CurrTick, enemy.Position, enemyType);
                    lastSeenEnemies.Remove(enemy.Id);
                    EnemyList.Remove(enemy.Id);
                    EnemyList.Add(enemy.Id, enemy);
                }
            }

            foreach (var enemy in lastSeenEnemies) {
                WritePosToCsv(CurrTick, EnemyList[enemy].Position, enemyLastSeenType);
            }

            EnemyList.Clear();
            foreach (var enemy in enemyList) {
                EnemyList.Add(enemy.Id, enemy);
            }
        }

        private void SaveNewBarriers(IEnumerable<Position> list) {
            var newEntrys = new HashSet<Position>(list);
            newEntrys.ExceptWith(BarrierList);

            foreach (var pos in newEntrys) {
                WritePosToCsv(CurrTick, pos, barrierType);
            }

            BarrierList.UnionWith(newEntrys);
        }

        private void WritePosToCsv(int tick, Position pos, string type) {
            var newLine = $"\n{tick};{type};{pos.X};{pos.Y}";
            // BufferedStream buff = new BufferedStream()
            _csvStreamWriter.Write(newLine);
            // File.AppendAllText(barrierPath, newLine);
        }

        private int GenNewId() {
            return Interlocked.Increment(ref _idTicker);
        }
    }
}