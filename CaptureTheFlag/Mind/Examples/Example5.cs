using System.Collections.Generic;
using System.Linq;
using System.Timers;
using LaserTag.Core.Model.Mind;
using LaserTag.Core.Model.Shared;
using Mars.Interfaces.Environments;
using Mars.Numerics;

namespace CaptureTheFlag.Mind.Examples;

public class Example5 : AbstractPlayerMind
{
    private Position _enemyFlagStand;
    private PlayerMindLayer _mindLayer;
    private Position _goal;

    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
    }

    public override void Tick()
    {
        // Wenn das Magazin leer ist → nachladen
        if (Body.RemainingShots == 0)
        {
            Body.Reload3();
        }
        
        // Positionen der Gegner exploren
        var enemies = Body.ExploreEnemies1();
        if ((enemies == null || enemies.Count == 0) && !Body.Stance.Equals(Stance.Standing))
        {
            Body.ChangeStance2(Stance.Standing);
        }
        
        if (enemies != null && enemies.Count > 0)
        {
            Stance newStance = Stance.Standing;
            
            // Wenn der nächste Gegner weiter als 8 Felder entfernt ist → Stehen
            if (Body.GetDistance(enemies.FirstOrDefault().Position) > 8)
            {
                newStance = Stance.Standing;
            }
            // Wenn der nächste Gegner gleich oder weniger als 8, aber weiter als 5 Felder entfernt ist → Knien
            else if (Body.GetDistance(enemies.FirstOrDefault().Position) <= 8 &&
                     Body.GetDistance(enemies.FirstOrDefault().Position) > 5)
            {
                newStance = Stance.Kneeling;
            }
            // Wenn der nächste Gegner gleich oder weniger als 5 Felder entfernt ist → Liegen
            else if (Body.GetDistance(enemies.FirstOrDefault().Position) <= 5)
            {
                newStance = Stance.Lying;
            }

            // Wechsel in entsprechende Position
            if (newStance != Body.Stance)
            {
                Body.ChangeStance2(newStance);
            }

            // Positionen der explosiven Fässer exploren
            var explosiveBarrelPositions = Body.ExploreExplosiveBarrels1();

            var shotFired = false;
            if (explosiveBarrelPositions != null && explosiveBarrelPositions.Count > 0)
            {
                // Für jedes Fass prüfen, ob es mehr als 3 Felder von einem selbst entfernt ist, aber weniger als 3 vom Gegner. Wenn ja, schieß darauf.
                for (int i = 0; i < explosiveBarrelPositions.Count; i++)
                {
                    if (Body.GetDistance(explosiveBarrelPositions[i]) > 3 && Distance.Euclidean(
                            explosiveBarrelPositions[i].X, explosiveBarrelPositions[i].Y,
                            enemies.FirstOrDefault().Position.X, enemies.FirstOrDefault().Position.Y) <= 3)
                    {
                        Body.Tag5(explosiveBarrelPositions[i]);
                        shotFired = true;
                        break;
                    }
                }
            }
            
            // ansonsten schieß auf den Gegner
            if (!shotFired)
            {
                Body.Tag5(enemies.FirstOrDefault().Position); // Tag the first enemy
            }
            
            // Wenn 3 oder weniger Schüsse im Magazin sind → reload
            if (Body.RemainingShots <= 3)
            {
                Body.Reload3(); // Reload if shots are low
            }
            
            
        }

        FlagCollection(); // Richtung Flagge bewegen, mitnehmen und in Base bringen
    }

    private void FlagCollection()
    {
        List<FlagSnapshot> flags = null;
        _enemyFlagStand ??= Body.ExploreEnemyFlagStands1()[0];
        if (Body.ActionPoints > 2 && !Body.CarryingFlag)
        {
            flags = Body.ExploreFlags2();
            var ownFlag = flags.FirstOrDefault(f => f.Team == Body.Color);
            var ownFlagStand = Body.ExploreOwnFlagStand();
            if (Distance.Euclidean(ownFlagStand.X, ownFlagStand.Y, ownFlag.Position.X, ownFlag.Position.Y) > 2)
            {
                _goal = ownFlag.Position;
            }
            else
            { 
                _goal = flags.Where(f => f.Team != Body.Color && f.PickedUp == false).Select(f => f.Position).FirstOrDefault();
            }
        }
        if (Body.CarryingFlag)
        {
            var flagStand = Body.ExploreOwnFlagStand();
            _goal = flagStand;
        }

        if (_goal == null && !Body.CarryingFlag && flags != null)
        {
            _goal = flags.Where(f => f.Team != Body.Color && f.PickedUp).Select(f => f.Position).FirstOrDefault();

        }
        
        if (_goal == null)
        {
            _goal = Body.Position;
        }
        var moved = Body.GoTo(_goal);
    }
    
}