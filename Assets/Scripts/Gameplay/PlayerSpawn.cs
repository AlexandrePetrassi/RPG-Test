using Fireblizzard;
using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : Simulation.Event<PlayerSpawn>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            var player = model.player;
            player.Collider2D.enabled = true;
            player.controlEnabled = false;
            if (player.AudioSource && player.respawnAudio)
                player.AudioSource.PlayOneShot(player.respawnAudio);
            player.Health.Increment();
            player.Teleport(model.spawnPoint.transform.position);
            player.Jumped = player.Jumped.Copy(jumpState: PlayerController.JumpState.Grounded);
            player.Animator.SetBool("dead", false);
            model.virtualCamera.m_Follow = player.transform;
            model.virtualCamera.m_LookAt = player.transform;
            Simulation.Schedule<EnablePlayerInput>(2f);
        }
    }
}