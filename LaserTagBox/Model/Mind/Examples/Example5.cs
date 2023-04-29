using System.Linq;
using LaserTagBox.Model.Shared;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;

namespace LaserTagBox.Model.Mind.Examples;

public class Example5 : AbstractPlayerMind
{
    private Position _goal;
    public static int Counter;

    public override void Init(PlayerMindLayer mindLayer)
    {
        //do something
        //Body.ChangeStance2(Stance.Lying);
        Counter = 0;
    }

    public override void Tick() // 10P
    {
        if (Body.Alive)
        {
            var exploreEnemies1 = Body.ExploreEnemies1(); // 1P
            if (exploreEnemies1 != null && exploreEnemies1.Any())
            {
                var enemyPosition = exploreEnemies1[0].Position;
                if (Body.RemainingShots == 0)
                {
                    Body.Reload3(); // 3P
                    Body.Tag5(enemyPosition); //5P
                    RandomMove();
                }
                else
                {
                    Body.Tag5(enemyPosition); //5P
                    //movePlayer(); // 3P
                    if (Body.WasTaggedLastTick)
                    {
                        MovePlayer(); // 3P
                    }
                    else
                    {
                        RandomMove();
                    }
                }
            }
            else // 1P
            {
                Body.Reload3(); // 3P
                if (Body.WasTaggedLastTick)
                {
                    Body.ChangeStance2(Stance.Standing); //P2
                    MovePlayer(); // 3P
                    //Body.ChangeStance2(Stance.Lying); //P2
                }
                else
                {
                    Body.ChangeStance2(Stance.Standing); //P2
                    RandomMove();
                    Body.ChangeStance2(Stance.Lying); //P2
                }
            }
        }
        //Console.WriteLine(Body.Position);
    }

    private void MovePlayer()
    {
        var exploreBarriers1 = Body.ExploreBarriers1(); // 1
        var exploreDitches1 = Body.ExploreDitches1(); // 1
        var exploreHills1 = Body.ExploreHills1(); // 1
        if (exploreBarriers1 != null && exploreBarriers1.Any())
        {
            Body.GoTo(exploreBarriers1[0]);
        }
        else if (exploreDitches1 != null && exploreDitches1.Any())
        {
            Body.GoTo(exploreDitches1[0]);
        }
        else if (exploreHills1 != null && exploreHills1.Any())
        {
            Body.GoTo(exploreHills1[0]);
        }
        else //if (_goal == null || Body.GetDistance(_goal) < 4)
        {
            RandomMove();
        }
    }

    private void RandomMove()
    {
        var x = RandomHelper.Random.Next(48);
        var y = RandomHelper.Random.Next(48);

        var goal = Position.CreatePosition(x, y);
        //_goal = Position.CreatePosition(48, 48);
        // Console.WriteLine("rand");
        Body.GoTo(goal);
        // Console.WriteLine("new goal " + goal);
    }
}