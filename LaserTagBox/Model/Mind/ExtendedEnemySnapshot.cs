using LaserTagBox.Model.Shared;

namespace LaserTagBox.Model.Mind
{
    public struct ExtendedEnemySnapshot
    {
        public EnemySnapshot Snapshot { get; set; }
        public long Tick { get; set; }
    }
}