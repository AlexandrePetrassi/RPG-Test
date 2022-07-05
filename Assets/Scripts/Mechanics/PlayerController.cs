using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using Fireblizzard;
using System;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public partial class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;
        
        public bool controlEnabled = true;

        Vector2 move;
        
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Health Health => health != null ? health : (health = GetComponent<Health>()); private Health health;
        public AudioSource AudioSource => audioSource != null ? audioSource : (audioSource = GetComponent<AudioSource>()); private AudioSource audioSource;
        public SpriteRenderer SpriteRenderer => spriteRenderer != null ? spriteRenderer : (spriteRenderer = GetComponent<SpriteRenderer>()); private SpriteRenderer spriteRenderer;
        public Animator Animator => animator != null ? animator : (animator = GetComponent<Animator>()); private Animator animator;
        public Collider2D Collider2D => collider2d != null ? collider2d : (collider2d = GetComponent<Collider2D>()); private Collider2D collider2d;
        public Bounds Bounds => Collider2D.bounds;

        protected override void Update()
        {
            move.x = controlEnabled ? Input.GetAxis("Horizontal") : 0;
            Jumped = Utils.UpdateJumpState(this);
            base.Update();
            ComputeJump();
            UpdateSpriteRenderer();
            UpdateAnimator();
            SetKinematicVelocity(move);
        }

        protected override void ComputeVelocity() { }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}