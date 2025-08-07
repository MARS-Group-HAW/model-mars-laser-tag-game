using System.Linq;
using LaserTag.Core.Model.Body;

namespace LaserTag.Core.Model.Shared;

public abstract class ScoringStrategy(PlayerBodyLayer layer)
{
    protected readonly PlayerBodyLayer _layer = layer;

    public virtual void CalculateScore()
    {
        if (_layer.Score.Count > 0) return;

        foreach (var team in _layer.Bodies.Values.GroupBy(body => body.Color))
        {
            var teamColor = team.Key;
            var teamName = team.First().TeamName;
            _layer.Score[teamColor] = new TeamScore(teamName, teamColor, 0);
        }
    }
}