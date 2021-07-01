using System;
using System.Collections.Generic;
using System.Linq;
using DnsClient.Internal;
using LaserTagBox.Model.Body;
using LaserTagBox.Model.Shared;
using Mars.Common.Core.Collections;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using Mars.Numerics.Statistics;
using Microsoft.CodeAnalysis.Text;
using MongoDB.Driver;
using ServiceStack;

namespace LaserTagBox.Model.Mind
{
    public class SuperMind : AbstractPlayerMind
    {
        private Position _goal = null;
        private EnemySnapshot _targetEnemy = new EnemySnapshot(Guid.Empty, Color.Blue,Stance.Kneeling,Position.CreatePosition(0,0));
        public Position shootingTarget;
        private Queue<Position> pathToDestination;
        private int shotsFired = 0;
        public Position PreviousPosition;
        public string Role;
        private Random _random = new Random();
        private List<Position> randomPosList = new();

        public override void Init(PlayerMindLayer mindLayer)
        {
            PreviousPosition = Body.Position;
            SuperMindCollection.Add(this);
            pathToDestination = new Queue<Position>();
            if (randomPosList.Count <= 0) FillRandomPosList();
        }

        public void FillRandomPosList()
        {
            randomPosList.Add(Position.CreatePosition(12, 6));
            randomPosList.Add(Position.CreatePosition(30, 5));
            randomPosList.Add(Position.CreatePosition(42, 10));
            randomPosList.Add(Position.CreatePosition(21, 14));
            randomPosList.Add(Position.CreatePosition(31, 14));
            randomPosList.Add(Position.CreatePosition(5, 14));
            randomPosList.Add(Position.CreatePosition(17, 23));
            randomPosList.Add(Position.CreatePosition(29, 23));
            randomPosList.Add(Position.CreatePosition(41, 22));
            randomPosList.Add(Position.CreatePosition(8, 28));
            randomPosList.Add(Position.CreatePosition(37, 29));
            randomPosList.Add(Position.CreatePosition(25, 35));
            randomPosList.Add(Position.CreatePosition(9, 38));
            randomPosList.Add(Position.CreatePosition(43, 39));
            randomPosList.Add(Position.CreatePosition(13, 44));
            randomPosList.Add(Position.CreatePosition(24, 45));
            randomPosList.Add(Position.CreatePosition(44, 46));
            randomPosList.Add(Position.CreatePosition(49, 1));
            randomPosList.Add(Position.CreatePosition(46, 2));
            randomPosList.Add(Position.CreatePosition(29, 1));
            randomPosList.Add(Position.CreatePosition(15, 1));
            randomPosList.Add(Position.CreatePosition(1, 1));
            randomPosList.Add(Position.CreatePosition(4, 2));
            randomPosList.Add(Position.CreatePosition(3, 7));
            randomPosList.Add(Position.CreatePosition(10, 7));
            randomPosList.Add(Position.CreatePosition(18, 7));
            randomPosList.Add(Position.CreatePosition(24, 7));
            randomPosList.Add(Position.CreatePosition(9, 13));
            randomPosList.Add(Position.CreatePosition(21, 13));
            randomPosList.Add(Position.CreatePosition(32, 13));
            randomPosList.Add(Position.CreatePosition(44, 13));
            randomPosList.Add(Position.CreatePosition(50, 13));
            randomPosList.Add(Position.CreatePosition(39, 17));
            randomPosList.Add(Position.CreatePosition(18, 17));
            randomPosList.Add(Position.CreatePosition(5, 17));
            randomPosList.Add(Position.CreatePosition(38, 24));
            randomPosList.Add(Position.CreatePosition(30, 24));
            randomPosList.Add(Position.CreatePosition(18, 24));
            randomPosList.Add(Position.CreatePosition(6, 24));
            randomPosList.Add(Position.CreatePosition(47, 31));
            randomPosList.Add(Position.CreatePosition(38, 31));
            randomPosList.Add(Position.CreatePosition(29, 31));
            randomPosList.Add(Position.CreatePosition(15, 31));
            randomPosList.Add(Position.CreatePosition(6, 31));
            randomPosList.Add(Position.CreatePosition(45, 40));
            randomPosList.Add(Position.CreatePosition(34, 40));
            randomPosList.Add(Position.CreatePosition(25, 40));
            randomPosList.Add(Position.CreatePosition(15, 40));
            randomPosList.Add(Position.CreatePosition(6, 40));
            randomPosList.Add(Position.CreatePosition(49, 49));
            randomPosList.Add(Position.CreatePosition(5, 49));
            randomPosList.Add(Position.CreatePosition(46, 50));
            randomPosList.Add(Position.CreatePosition(1, 50));
        }

        public override void Tick()
        {
            if (Body.ActionPoints == 0) return;

            if (Role == "leader" && Body.Energy == 0)
            {
                SuperMindCollection.SetNewLeader();
            }
            
            if (shotsFired >= 4 && Body.ActionPoints >= 3) Reload3();

            switch (Role)
            {
                case "leader":
                    Leader();
                    break;
                case "mate":
                    Mate();
                    break;
            }
        }

        public void Mate()
        {
            if (SuperMindCollection.enemiesPositionList.Count > 0)
            {
                Shooting();
            }

            MateExploration();
            var leader = GetLeader("leader");
            if (GetDistance(leader.PreviousPosition) > RandomHelper.Random.Next(1,3))
            {
                GoTo(leader.PreviousPosition);
            }
        }
        
        public SuperMind GetLeader(string Role)
        {
            return SuperMindCollection.GetSuperMindList().FirstOrDefault(SuperMind => SuperMind.Role == "leader");
        }


        
        public void MateExploration()
        {
            foreach (var hill in Explore1("hills"))
            {
                SuperMindCollection.hillsList.Add(hill);
            }

            foreach (var ditche in Explore1("ditches"))
            {
                SuperMindCollection.ditchesList.Add(ditche);
            }
        }




        public void Leader()
        {
            SuperMindCollection.ClearAllLists();
            LeaderExploreEnemies();
            LeaderExploreObstacles();
            Shooting();
            SetRandomPos();
            GoTo(_goal);
        }

        public void LeaderExploreEnemies()
        {
            foreach (var enemy in ExploreEnemies1())
            {
                Position enemyPos = enemy.Position;
                SuperMindCollection.enemiesList.Add(enemy);
                SuperMindCollection.enemiesPositionList.Enqueue(enemyPos);
                SuperMindCollection.occupiedPositions.Add(enemyPos);
            }
        }

        public void LeaderExploreObstacles()
        {
            foreach (var obstacle in Explore1("barrier"))
            {
                SuperMindCollection.barriesList.Add(obstacle);
                SuperMindCollection.occupiedPositions.Add(obstacle);

            }
        }

        public void SetRandomPos()
        {
            if (_goal == null || GetDistance(_goal) < 2 || Equals(_goal, Body.Position))
            {
                _goal = randomPosList[_random.Next(randomPosList.Count)];
            }
        }

        public void Shooting()
        {
            if (SuperMindCollection.enemiesPositionList.Count <= 0) return;
            Position pos = SuperMindCollection.enemiesPositionList.Dequeue();
            if (SuperMindCollection.enemiesPositionList.Count < 3) SuperMindCollection.enemiesPositionList.Enqueue(pos);
            TagEnemy5(pos);
            shotsFired += 1;
            _goal = pos;
        }

        /**
         * Zeigt alle Teammitglieder in der Umgebung an
         * Radius: Stehend = 10, Kniend = 8, Liegend = 5 
         */
        public List<FriendSnapshot> ExploreTeammates()
        {
            return Body.ExploreTeam();
        }

        /**
         * Zeigt alle Gegner in der Umgebung an
         * Radius: Stehend = 10, Kniend = 8, Liegend = 5 
         * ActionPoints -= 1
         */ 
        public List<EnemySnapshot> ExploreEnemies1()
        {
            return Body.ExploreEnemies1();
        }

        /**
         * Ändert die Position des Spielers
         * Möglichkeiten: Stehend, Kniend, Liegend
         */
        public void ChangeStance(Stance stance)
        {
            Body.ChangeStance2(stance);
        }

        /**
         * Gegner wird angeschossen und verliert 5 Energie
         * ActionPoints -= 5
         */ 
        public void TagEnemy5(Position position)
        {
            Body.Tag5(position);
        }

        /**
         * Magazine wird voll aufgefüllt
         * ActionPoints -= 3
         */ 
        public void Reload3()
        {
            Body.Reload3();
        }

        /**
         * Hindernisse in der Nähe werde erkannt,
         * es muss aber übergeben werden, nach welchem Hindernis gesucht werden soll
         * 3 = Ditch, 2 = Hill, 1 = Barrier (siehe map.csv)
         * ActionPoints -= 1
         */ 
        public List<Position> Explore1(string obj)
        {
            List<Position> exploreList = new List<Position>();
            switch (obj)
            {
                case "ditches":
                    exploreList = Body.ExploreDitches1();
                    break;
                case "hills":
                    exploreList = Body.ExploreHills1();
                    break;
                default:
                    exploreList = Body.ExploreBarriers1();
                    break;
            }

            return exploreList ?? (exploreList = new List<Position>());
        }

        /**
         * Spieler bewegt sich um ein Feld zu der Position
         * Es kann immer nur EIN Feld gegangen werden
         */
        public bool GoTo(Position position)
        {
            return Body.GoTo(position);
        }
        
        /**
         * Berechnet die Distanz zu der Position
         */
        public int GetDistance(Position position)
        {
            return Body.GetDistance(position);
        }
        
    }
    
    class SuperMindCollection
    {
        public static List<SuperMind> _superMindList = new();
        public static List<Position> barriesList = new();
        public static List<Position> ditchesList = new();
        public static List<Position> hillsList = new();
        public static List<EnemySnapshot> enemiesList = new ();
        public static Queue<Position> enemiesPositionList = new();
        public static List<FriendSnapshot> teammatesList = new();
        public static List<Position> teammatesPositionList = new();
        public static List<Position> obstaclesList = new();
        public static HashSet<Position> occupiedPositions = new();


        public static void Add(SuperMind superMind)
        {
            superMind.Role = _superMindList.IsEmpty() ? "leader" : "mate";
            _superMindList.Add(superMind);
        }

        public static void Delete(SuperMind superMind)
        {
            _superMindList.Remove(superMind);
        }

        public static void SetNewLeader()
        {
            foreach (var superMind in _superMindList.Where(superMind => superMind.Role == "mate" && superMind.Body.Energy > 0))
            {
                superMind.Role = "leader";
                break;
            }
        }

        public static List<SuperMind> GetSuperMindList()
        {
            return _superMindList;
        }

        public static void DeleteSuperMindList()
        {
            _superMindList.Clear();
        }

        public static void ClearAllLists()
        {
            barriesList = new();
            ditchesList = new();
            hillsList = new();
            enemiesList = new();
            enemiesPositionList = new();
            teammatesList = new();
            teammatesPositionList = new();
            obstaclesList = new();
            occupiedPositions = new();
        }
    }
}
