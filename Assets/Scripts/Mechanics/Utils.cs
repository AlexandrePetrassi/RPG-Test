using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using static Platformer.Mechanics.PlayerController;
using Platformer.Model;
using Platformer.Core;
using Platformer.Mechanics;
using Fireblizzard;

namespace Fireblizzard
{
    public static class Utils
    {
        public static PlayerJumpedEvent UpdateState(IPlayer player)
        {
            PlayerJumpedEvent evt = player.Jumped;
            if (!player.ControlEnabled)
            {
                return evt.Copy(move: evt.Move.Copy(x: 0, y: player.CurrentVelocity.y));
            }
            if (evt.JumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
            {
                return evt.Copy(jumpState: JumpState.PrepareToJump, move: evt.Move.Copy(x: Input.GetAxis("Horizontal")));
            }
            if (Input.GetButtonUp("Jump"))
            {
                Schedule<PlayerStopJump>().player = player;
                float factor = (evt.Move.y > 0) ? player.Model.jumpDeceleration : 1;
                return evt.Copy(move: evt.Move.Copy(x: Input.GetAxis("Horizontal"), y: player.CurrentVelocity.y * factor));
            }
            if (evt.JumpState == JumpState.PrepareToJump)
            {
                return evt.Copy(move: evt.Move.Copy(x: Input.GetAxis("Horizontal"), y: player.JumpTakeOffSpeed * player.Model.jumpModifier), jumpState: JumpState.Jumping);
            }
            if (evt.JumpState == JumpState.Jumping && !player.IsGrounded)
            {
                Schedule<PlayerJumped>().player = player;
                return evt.Copy(jumpState: JumpState.InFlight, move: evt.Move.Copy(x: Input.GetAxis("Horizontal")));
            }
            if (evt.JumpState == JumpState.InFlight && player.IsGrounded)
            {
                Schedule<PlayerLanded>().player = player;
                return evt.Copy(jumpState: JumpState.Landed, move: evt.Move.Copy(x: Input.GetAxis("Horizontal")));
            }
            if (evt.JumpState == JumpState.Landed)
            {
                return evt.Copy(jumpState: JumpState.Grounded, move: evt.Move.Copy(x: Input.GetAxis("Horizontal")));
            }
            else
            {
                return evt.Copy(move: evt.Move.Copy(x: Input.GetAxis("Horizontal"), y: player.CurrentVelocity.y));
            }
        }
        public static PlayerJumpedEvent Copy(
            this PlayerJumpedEvent value,
            JumpState? jumpState = null,
            Vector2? move = null)
        {
            return new PlayerJumpedEvent(
                jumpState: jumpState ?? value.JumpState,
                move: move ?? value.Move);
        }
        public static Vector2 Copy(
            this Vector2 value,
            float? x = null,
            float? y = null)
        {
            return new Vector2(x ?? value.x, y ?? value.y);
        }
    }

    public struct PlayerJumpedEvent
    {
        readonly JumpState jumpState;
        readonly Vector2 move;

        public JumpState JumpState => jumpState;
        public Vector2 Move => move;

        public PlayerJumpedEvent(
            JumpState jumpState = JumpState.Grounded,
            Vector2 move = new Vector2())
        {
            this.jumpState = jumpState;
            this.move = move;
        }
    }

    public interface IPlayer
    {
        AudioSource AudioSource { get; }
        AudioClip JumpAudio { get; }
        PlayerJumpedEvent Jumped { get; }
        bool IsGrounded { get; }
        bool ControlEnabled { get; }
        float JumpTakeOffSpeed { get; }
        PlatformerModel Model { get; }
        Vector2 CurrentVelocity { get; }
    }
}

namespace Platformer.Mechanics
{
    public partial class PlayerController : IPlayer
    {
        public AudioClip JumpAudio => jumpAudio;
        public bool ControlEnabled => controlEnabled;
        public float JumpTakeOffSpeed => jumpTakeOffSpeed;
        public PlatformerModel Model => model;
        public Vector2 CurrentVelocity => velocity;
        public PlayerJumpedEvent Jumped { get; set; } = new PlayerJumpedEvent();
        public void UpdateSpriteRenderer()
        {
            if (Jumped.Move.x > 0.01f)
                SpriteRenderer.flipX = false;
            else if (Jumped.Move.x < -0.01f)
                SpriteRenderer.flipX = true;
        }
        public void UpdateAnimator()
        {
            Animator.SetBool("grounded", IsGrounded);
            Animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
        }
        public void SetKinematicVelocity(Vector2 newVelocity)
        {
            velocity.y = newVelocity.y;
            targetVelocity = newVelocity * maxSpeed;
        }
    }
}
