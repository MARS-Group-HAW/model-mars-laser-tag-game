using LaserTagBox.Model.Shared;

namespace LaserTagBox.Model.Mind
{
    public interface IOurPlayerMind
    {
        bool IsLeader();
        IPlayerBody getBody();
    }
}