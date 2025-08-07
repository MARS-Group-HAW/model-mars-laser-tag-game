using System.Linq;
using LaserTag.Core.Model.Mind;
using LaserTag.Core.Model.Shared;
using Mars.Interfaces.Environments;

namespace CaptureTheFlag.Mind.Examples
{
    public class Example1 : AbstractPlayerMind
    {
        private Position _ownFlagStand;
        private Position _enemyFlagStand;

        public override void Init(PlayerMindLayer mindLayer)
        {
            // cache your own and the enemy's flag stands once at startup
            _ownFlagStand = Body.ExploreOwnFlagStand();
            var enemyStands = Body.ExploreEnemyFlagStands1();
            if (enemyStands != null && enemyStands.Count > 0)
                _enemyFlagStand = enemyStands[0];
        }

        public override void Tick()
        {
            if (Body.ActionPoints < 1) return;

            // 1) If you're carrying the flag, go home
            if (Body.CarryingFlag)
            {
                if (Body.Stance != Stance.Standing && Body.ActionPoints >= 2)
                    Body.ChangeStance2(Stance.Standing);
                Body.GoTo(_ownFlagStand);
                return;
            }

            // 2) If any teammate holds the flag, support them
            var flags = Body.ExploreFlags2();
            if (flags != null)
            {
                // struct, so check PickedUp rather than null
                var carrier = flags.FirstOrDefault(f => f.Team != Body.Color && f.PickedUp);
                if (!carrier.Equals(default(FlagSnapshot)))
                {
                    Body.GoTo(carrier.Position);
                    return;
                }
            }

            // 3) If you see enemies, engage
            var enemies = Body.ExploreEnemies1();
            if (enemies != null && enemies.Count > 0)
            {
                HandleCombat(enemies);
                return;
            }

            // 4) If the enemy flag is on the ground (visible & not picked up), grab it
            if (flags != null)
            {
                var dropped = flags.FirstOrDefault(f => f.Team != Body.Color && !f.PickedUp);
                if (!dropped.Equals(default(FlagSnapshot)))
                {
                    if (Body.Stance != Stance.Standing && Body.ActionPoints >= 2)
                        Body.ChangeStance2(Stance.Standing);
                    Body.GoTo(dropped.Position);
                    return;
                }
            }

            // 5) Otherwise pressure the enemy base
            if (!_enemyFlagStand.Equals(default(Position)))
            {
                Body.GoTo(_enemyFlagStand);
                return;
            }

            // 6) Fallback: defend your own stand
            Body.GoTo(_ownFlagStand);
        }

        private void HandleCombat(System.Collections.Generic.List<EnemySnapshot> foes)
        {
            var target = foes.OrderBy(e => Body.GetDistance(e.Position)).First();
            int dist = Body.GetDistance(target.Position);

            // reload if empty
            if (Body.RemainingShots == 0 && Body.ActionPoints >= 3)
            {
                Body.Reload3();
                return;
            }

            // kneel+shoot if in line of sight
            if (Body.RemainingShots > 0 && Body.HasBeeline1(target.Position))
            {
                if (dist <= 5 && Body.Stance != Stance.Kneeling && Body.ActionPoints >= 2)
                {
                    Body.ChangeStance2(Stance.Kneeling);
                    return;
                }
                Body.Tag5(target.Position);
                return;
            }

            // if too close, back up
            if (dist < 3 && Body.ActionPoints >= 1)
            {
                var dx = Body.Position.X - target.Position.X;
                var dy = Body.Position.Y - target.Position.Y;
                var away = Position.CreatePosition(
                    Body.Position.X + dx,
                    Body.Position.Y + dy);
                Body.GoTo(away);
                return;
            }

            // otherwise advance
            if (Body.ActionPoints >= 1)
            {
                Body.GoTo(target.Position);
            }
        }
    }
}

