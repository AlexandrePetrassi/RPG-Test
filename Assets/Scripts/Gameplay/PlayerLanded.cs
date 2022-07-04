using Platformer.Core;
using Platformer.Mechanics;
using Fireblizzard;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player character lands after being airborne.
    /// </summary>
    /// <typeparam name="PlayerLanded"></typeparam>
    public class PlayerLanded : Simulation.Event<PlayerLanded>
    {
        public IPlayer player;

        public override void Execute()
        {

        }
    }
}