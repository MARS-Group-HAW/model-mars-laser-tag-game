using System.Linq;
using LaserTag.Core.Model.Body;

namespace LaserTag.Core.Model.Shared;

public class TeamDeathmatchScoring(PlayerBodyLayer layer) : ScoringStrategy(layer)
{
    public override void CalculateScore()
    {
        base.CalculateScore();

        foreach (var team in _layer.Bodies.Values.GroupBy(b => b.Color))
        {
            var color = team.Key;
            var points = team.Sum(b => b.GamePoints);
            _layer.Score[color].GamePoints += points;
        }
    }
}