using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LaserTag.Core.Model.Mind;
using LaserTag.Core.Model.Shared;
using Mars.Interfaces.Environments;
using ServiceStack;
using Mars.Numerics;

namespace CaptureTheFlag.Mind.Examples;

/*## NeuralOps Lab05 Competition LaserTag CTF Agents:

Der NeuralOps Agent nutzt einen vollstaendig Rule Based Algorithmus.
Die Agenten kommunizieren untereinander und teilen Informationen ueber Flaggen und Gegner.
Die Agenten weisen sich dynamisch die Rollen Teamleader und Follower zu.
Das Team bewegt sich wenn sinnvoll komplett zusammen und moeglichst dicht formiert.
Friendly Fire wird vermieden.
Beim kaempfen sammelt sich das Team an einer Position fuer maximale Kampfstaerke.
Der Agent Kann manchmal doppelt in einem Tick schiessen.
Waehrend des Kaempfens liegt der Agent, sonst steht er.
Barrels werden beachtet. 
Beim Tod mehrerer Agenten sammelt sich das Team am eigenen Flaggenstand.
Agenten nutzen im Kampf bei wenig Leben eine Art Ausfallschrittmechanik, um sich hiter Teammates zu verstecken.


### NeuralOps LaserTag CTF Agents Rule Based Verhalten:

#### Erkunden:
- Gegner: Jeder Agent einmal pro Tick
    - Liste wird mit anderen Agenten geteilt und gegebenenfalls erweitert
    - falls Doppelschuss moeglich geteilte Liste laden und nicht erkunden
- Flaggen: Ein einziger Agent einmal pro Tick und Agent teilt Liste mit Team
- Team: Jeder Agent einmal pro Tick
- Faesser: Wenn letzter ueberlebender Agent und in der Unterzahl
- feindlicher Flaggenstand: Nur einmal zu Beginn der Partie
- Eigener Flaggenstand: Nur einmal zu Beginn der Partie

#### Schiessen:
- Agent legt sich hin wenn Gegner gesichtet und steht wieder auf wenn keiner mehr sichtbar
- Wenn Gegner auf der eigenen Position steht oder Agent nicht im Kampf -> erst bewegen und dann schiessen, sonst umgekehrt
- Nachladen falls 0 Munition und immer Nachladen wenn am Ende des Ticks noch Punkte uebrig
- wenn Doppelschuss moeglich werden alle 10 Actionpoints fuer 2 mal schiessen verwendet
- Zielauswahl nach Prioritaet:
  1. Barrel wurde sinnvollerweise gesichtet
  2. Gegner der auf einem Feld ohne Teammate steht
  3. Mehr Gegner als Teammates befinden aich auf der Position
  4. Gegner hat Flagge und Teammates sind in der Ueberzahl
  
#### Bewegen:
- Agent traegt Flagge:
    - aus der Formation ausbrechen und so schnell wie moegliech zum eigenen Flaggenstand laufen
- Agent traegt keine Flagge:
    - Teamleader: Gegner hat eigene Flagge -> eigene Flagge, sonst -> feindliche Flagge
    - Follower: zu Teamleader bewegen
    - Beide Rollen: falls Gegner gesichtet -> an einem Punkt sammeln und Ausfallschritte nutzen
- Alle Agenten:
    - Wenn im Startgebiet und min ein Teammate tot -> Am Flaggenstand sammeln*/
public class Example6 : AbstractPlayerMind
{
    private PlayerMindLayer _mindLayer;
    private Position _goal;
    private Position flagStand;
    private Position flagStandEnemy;
    private bool moved;
    private double distanceEnemyFlag;
    private int stunLock;
    private bool sidestep;
    private bool sidestepDone;
    private List<FriendSnapshot> team;
    private List<EnemySnapshot> enemies;
    private Position flagEnemy;
    private List<FlagSnapshot> flags;
    
    private static long _currentTick;
    private static  List<EnemySnapshot> _enemies;
    private static long _currentTickFlag;
    private static  List<FlagSnapshot> _flags;
    private static long _currentTickMove;
    private static Position _leaderMove;
    
    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
    }

    
    public override void Tick()
    {
        if (!Body.Alive) return;
        _goal = null;
        enemies = null;
        moved = false;
        flags ??= Body.ExploreFlags2();
        flagStand ??= Body.ExploreOwnFlagStand();
        flagStandEnemy ??= Body.ExploreEnemyFlagStands1().First();
        team = Body.ExploreTeam();
        if (Body.Stance == Stance.Standing) Move();
        
        //gegebenenfalls auf Scouten verzichten, um genug Actionpoints fuer 2 Schuesse zu haben
        if (_mindLayer.GetCurrentTick() == _currentTick && _enemies != null && _enemies.Count > 0 &&
                 Body.RemainingShots >= 2 && Body.Stance == Stance.Lying && Body.ActionPoints == 10)
        {
            enemies = _enemies.CreateCopy();
            flags = _flags.CreateCopy();
        }
        else
            Scout();
        Shoot();
        Move();
        if (Body.RemainingShots < 5) Body.Reload3();                          
    }

    
    private void Shoot()
    {
        if (Body.ActionPoints < 5) return;

        if (enemies != null && enemies.Count > 0)
        {
            Position moreEnemyThanTeam = null;
            if (Body.RemainingShots == 0) Body.Reload3();
            
            //Wenn Gegner auf der eigenen Position steht -> erst bewegen und dann schiessen, sonst umgekehrt
            foreach (var enemy in enemies) 
                if (enemy.Position.Equals(Body.Position))
                    Move();
            
            //hinlegen wenn der Kampf beginnt
            if ((!Body.CarryingFlag ||                                                                                  
                 Distance.Euclidean(Body.Position.X, Body.Position.Y, enemies.First().Position.X,
                     enemies.First().Position.Y) < 11) && Body.Stance != Stance.Lying)
            {
                Body.ChangeStance2(Stance.Lying);
                stunLock++;
            }
            
            //Fass abschiessen falls Sinnvoll
            if (Body.ActionPoints > 6 && team != null && team.Count < 1 && enemies.Count > 1)
            {
                var barrels = Body.ExploreExplosiveBarrels1();
                if (barrels != null && barrels.Count > 0 && Body.HasBeeline1(barrels.First()))
                    Body.Tag5(barrels.First());
            }
            
            //Passende Ziele suchen
            foreach (var enemy in enemies)
            {
                if (team != null && team.Count > 0)
                {
                    bool samePosition = false;
                    foreach (var member in team)
                    {
                        if (enemy.Position.Equals(member.Position))
                        {
                            samePosition = true;
                            if (moreEnemyThanTeam == null && CheckPositionTeamVsEnemy(enemy.Position))
                                moreEnemyThanTeam = enemy.Position;
                        }
                    }

                    if (enemy.Position.Equals(Body.Position))
                    {
                        samePosition = true;
                        if (moreEnemyThanTeam == null && CheckPositionTeamVsEnemy(enemy.Position))
                            moreEnemyThanTeam = enemy.Position;
                    }

                    if (!samePosition)
                    {
                        //kein Teammate auf dem ZielFeld, daher freie Schussbahn
                        Body.Tag5(enemy.Position);
                        Body.Tag5(enemy.Position);
                    }
                }
                else if (!enemy.Position.Equals(Body.Position))
                {
                    //Kein Teammate am leben, daher freie Schussbahn
                    Body.Tag5(enemy.Position);
                    Body.Tag5(enemy.Position);
                }
            }
            //Mehr Gegner als Teammates auf dem Feld, daher akzeptables Ziel
            if (moreEnemyThanTeam != null) Body.Tag5(moreEnemyThanTeam);
            if (moreEnemyThanTeam != null) Body.Tag5(moreEnemyThanTeam);
            
            //Notfallziel: nurnoch 1 gegner mit Flagge am leben, aber Teammates auf dem Feld
            if (enemies.Count == 1 && Body.ActionPoints > 4 && flags != null && flags.Count > 0)
            {
                foreach (var flag in flags)
                {
                    if (flag.Position.Equals(enemies.First().Position))
                    {
                        Body.Tag5(enemies.First().Position);
                    }
                }
            }
        }
        //Aufstehen, falls Kampf vorbei
        else if (Body.Stance != Stance.Standing) 
        {
            Body.ChangeStance2(Stance.Standing);
            stunLock = 0;
            sidestep = false;
        }
    }
    
    
    private void Move()
    {
        if (moved) return;
        
        flags = _flags;
        double distanceFlagStand = 1000;
        if (flagStand != null)
            distanceFlagStand = Distance.Euclidean(Body.Position.X, Body.Position.Y, flagStand.X, flagStand.Y);
        
        // passende Flagge als Ziel aussuchen
        if (Body.CarryingFlag)
        {
            if (flagStand != null) _goal = flagStand;
        }
        else
        {
            if (flags != null && flags.Count > 0)
            {
                foreach (var flag in flags)
                {
                    if (flag.Team != Body.Color)
                    {
                        distanceEnemyFlag = Distance.Euclidean(Body.Position.X, Body.Position.Y, flag.Position.X,
                            flag.Position.Y);
                        _goal ??= flag.Position;
                        flagEnemy = flag.Position;
                    }
                    else if (!flag.Position.Equals(flagStand) && flagEnemy != null && flagStandEnemy != null &&
                             (distanceEnemyFlag > 8 || flagEnemy.Equals(flagStand)))
                    {
                        if (Body.Position.Equals(flag.Position))
                            _goal = flagStandEnemy;
                        else
                            _goal = flag.Position;
                    }
                }
            }
        }
        
        //Team zusammenhalten
        if (team != null && team.Count > 0 && !Body.CarryingFlag)
        {
            Position maxDistanceMember = team.First().Position.Copy();
            Position minDistanceMember = team.First().Position.Copy();
            foreach (var member in team)
            {
                if (Distance.Euclidean(Body.Position.X, Body.Position.Y, maxDistanceMember.X, maxDistanceMember.Y) <
                    Distance.Euclidean(Body.Position.X, Body.Position.Y, member.Position.X, member.Position.Y)) 
                    maxDistanceMember = member.Position;
                if (Distance.Euclidean(Body.Position.X, Body.Position.Y, minDistanceMember.X, minDistanceMember.Y) >
                    Distance.Euclidean(Body.Position.X, Body.Position.Y, member.Position.X, member.Position.Y)) 
                    minDistanceMember = member.Position;
            }

            if (Body.Stance == Stance.Lying && stunLock < 40)
            {
                //Ausfallschritt Mechanik
                if (Body.Energy < 50 && !sidestep)
                {
                    sidestepDone = false;
                    sidestep = true;
                    foreach (var member in team)
                        if (!Body.Position.Equals(member.Position))
                            sidestep = false;
                }
                else
                {
                    _goal = minDistanceMember;
                }
            }
            else if (Distance.Euclidean(Body.Position.X, Body.Position.Y, maxDistanceMember.X, maxDistanceMember.Y) >
                     2 && distanceEnemyFlag > 8)
                _goal = maxDistanceMember;
        }

        //Team am eigenen Flaggenstand Sammeln
        if (team != null && team.Count < 2 && distanceFlagStand >= 2 && distanceFlagStand <= 5 &&
            !Body.CarryingFlag && distanceEnemyFlag > 3 ) _goal = null;
        
        //Teamleader bestimmen und Bewegung ausfuehren
        if (_goal != null)
        {
            if (sidestep && !sidestepDone)
            {
                Position oldPosition = Body.Position.Copy();
                Body.GoTo(_goal);
                if (!Body.Position.Equals(oldPosition)) sidestepDone = true;
            }
            else
            {
                if (_mindLayer.GetCurrentTick() > _currentTickMove || Body.CarryingFlag || distanceEnemyFlag < 3)
                {
                    Body.GoTo(_goal);
                    _leaderMove = Body.Position.Copy();
                    _currentTickMove = _mindLayer.GetCurrentTick().CreateCopy();
                }
                else
                {
                    _goal = _leaderMove;
                    Body.GoTo(_goal);
                }
            }
            moved = true;
        }
    }

    
    private void Scout()
    {
        //Gegner erkunden
        enemies = Body.ExploreEnemies1();
        if (enemies != null && enemies.Count > 0)
        {
            if (_mindLayer.GetCurrentTick() != _currentTick)
            {
                _enemies = enemies.CreateCopy();
                _currentTick = _mindLayer.GetCurrentTick().CreateCopy();
            }
            else
            {
                foreach (var _enemy in _enemies)
                {
                    bool add = true;
                    foreach (var enemy in enemies)
                    {
                        if(_enemy.Position.Equals(enemy.Position)) add = false;
                    }
                    if(add)enemies.Add(_enemy);
                }
                _enemies = enemies.CreateCopy();
                
            }
        }
        
        //Flaggen erkunden
        else if (_mindLayer.GetCurrentTick() == _currentTick)
        {
            enemies = _enemies.CreateCopy();
        }
        if (_mindLayer.GetCurrentTick() > _currentTickFlag)
        {
            flags = Body.ExploreFlags2();
            if (flags != null && flags.Count > 0)
            {
                _flags = flags;
                _currentTickFlag = _mindLayer.GetCurrentTick().CreateCopy();
            }
        }
        else
        {
            flags = _flags.CreateCopy();
        }
    }
    
    
    //Returns true wenn mehr Gegner als Teammates auf der Position position sind
    private bool CheckPositionTeamVsEnemy(Position position)
    {
        int teamCount = 0;
        int enemyCount = 0;

        if (position.Equals(Body.Position)) teamCount++;
        if (enemies != null && enemies.Count > 0)
        {
            foreach (var enemy in enemies)
            {
                if (position.Equals(enemy.Position)) enemyCount++;
            }

        }
        if (team != null && team.Count > 0)
        {
            foreach (var member in team)
            {
                if (position.Equals(member.Position)) teamCount++;
            }
        }
        return (teamCount < enemyCount)? true : false;
    }
}