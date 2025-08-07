using System.Linq;
using LaserTag.Core.Model.Body;
using LaserTag.Core.Model.Items;
using LaserTag.Core.Model.Spots;

namespace LaserTag.Core.Model.Shared;

public class CaptureTheFlagScoring(PlayerBodyLayer layer) : ScoringStrategy(layer)
{
    public override void CalculateScore()
    {
        base.CalculateScore();

        var flags = _layer.Items.Values.OfType<Flag>().ToList();
        var flagStands = _layer.SpotEnv.Entities.OfType<FlagStand>().ToList();

        foreach (var flag in flags)
        {
            var player = _layer.FighterEnv.Explore(flag.Position, 0, -1, body => body.Alive).FirstOrDefault();
            if (player != null)
            {
                var flagStand = _layer.SpotEnv.Explore(flag.Position, 0, 1)
                    .OfType<FlagStand>()
                    .FirstOrDefault();

                bool isNotPickedUp = !flag.PickedUp;

                if ((flagStand == null && isNotPickedUp) ||
                    (flagStand != null && isNotPickedUp && flagStand.Color != player.Color))
                {
                    if (flag.Color != player.Color)
                    {
                        flag.PickUp(player);
                        player.CarryingFlag = true;
                    }
                    else
                    {
                        var homeStand = flagStands.First(fs => fs.Color == player.Color).Position;
                        _layer.ItemEnv.PosAt(flag, homeStand.X, homeStand.Y);
                    }
                }

                if (flagStand != null && flagStand.Color == player.Color && flag.PickedUp)
                {
                    flag.Owner.CarryingFlag = false;
                    flag.Drop();
                }
            }
        }

        foreach (var flagStand in flagStands)
        {
            int flagsAtStand = _layer.ItemEnv
                .Explore(flagStand.Position, 0, -1, item => item is Flag)
                .Count();

            if (flagsAtStand == _layer.Score.Keys.Count)
            {
                _layer.Score[flagStand.Color].GamePoints += 1;

                foreach (var flag in flags)
                {
                    var homeStand = flagStands.First(fs => fs.Color == flag.Color).Position;
                    _layer.ItemEnv.PosAt(flag, homeStand.X, homeStand.Y);
                }

                break;
            }
        }
    }
}
