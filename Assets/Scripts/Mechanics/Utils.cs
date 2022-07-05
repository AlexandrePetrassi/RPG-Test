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
        public static PlayerJumpedEvent UpdateJumpState(IPlayer player)
        {
            PlayerJumpedEvent evt = player.Jumped;
            if (!player.ControlEnabled)
            {
                return evt;
            }
            else if (evt.JumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
            {
                return evt.Copy(jumpState: JumpState.PrepareToJump);
            }
            else if (Input.GetButtonUp("Jump"))
            {
                Schedule<PlayerStopJump>().player = player;
                return evt.Copy(stopJump: true);
            }
            else if (evt.JumpState == JumpState.PrepareToJump)
            {
                return evt.Copy(jump: true, stopJump: false, jumpState: JumpState.Jumping);
            }
            else if (evt.JumpState == JumpState.Jumping && !player.IsGrounded)
            {
                Schedule<PlayerJumped>().player = player;
                return evt.Copy(jump: false, jumpState: JumpState.InFlight);
            }
            else if (evt.JumpState == JumpState.InFlight && player.IsGrounded)
            {
                Schedule<PlayerLanded>().player = player;
                return evt.Copy(jump: false, jumpState: JumpState.Landed);
            }
            else if (evt.JumpState == JumpState.Landed)
            {
                return evt.Copy(jump: false, jumpState: JumpState.Grounded);
            }
            else
            {
                return evt;
            }
        }
        public static PlayerJumpedEvent ComputeVerticalVelocity(IPlayer player)
        {
            PlayerJumpedEvent evt = player.Jumped;
            if (evt.Jump && player.IsGrounded)
            {
                return evt.Copy(jump: false, move: evt.Move.NewY(player.JumpTakeOffSpeed * player.Model.jumpModifier));
            }
            else if (evt.StopJump)
            {
                float factor = (evt.Move.y > 0) ? player.Model.jumpDeceleration : 1;
                return evt.Copy(jump: false, move: evt.Move.NewY(player.CurrentVelocity.y * factor));
            }
            else
            {
                return evt.Copy(move: evt.Move.NewY(player.CurrentVelocity.y));
            }
        }
        public static PlayerJumpedEvent ComputeHorizontalVelocity(IPlayer player)
        {
            PlayerJumpedEvent evt = player.Jumped;
            float newSpeed = player.ControlEnabled ? Input.GetAxis("Horizontal") : 0;
            return evt.Copy(move: evt.Move.NewX(newSpeed));
        }
        public static PlayerJumpedEvent Copy(
            this PlayerJumpedEvent value,
            bool? jump = null,
            bool? stopJump = null,
            JumpState? jumpState = null,
            Vector2? move = null)
        {
            return new PlayerJumpedEvent(
                jump: jump ?? value.Jump,
                stopJump: stopJump ?? value.StopJump,
                jumpState: jumpState ?? value.JumpState,
                move: move ?? value.Move);
        }
        public static Vector2 NewX(this Vector2 vector, float x)
        {
            return new Vector2(x, vector.y);
        }
        public static Vector2 NewY(this Vector2 vector, float y)
        {
            return new Vector2(vector.x, y);
        }
    }

    public struct PlayerJumpedEvent
    {
        readonly bool jump;
        readonly bool stopJump;
        readonly JumpState jumpState;
        readonly Vector2 move;

        public bool Jump => jump;
        public bool StopJump => stopJump;
        public JumpState JumpState => jumpState;
        public Vector2 Move => move;

        public PlayerJumpedEvent(
            bool jump = false,
            bool stopJump = false,
            JumpState jumpState = JumpState.Grounded,
            Vector2 move = new Vector2())
        {
            this.jump = jump;
            this.stopJump = stopJump;
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
        public void ComputeJump()
        {
            Jumped = Utils.ComputeVerticalVelocity(this);
            Jumped = Utils.ComputeHorizontalVelocity(this);
            velocity = Jumped.Move;
        }
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
