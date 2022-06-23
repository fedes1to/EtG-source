using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchoutAIActor : PunchoutGameActor
{
	public class PunchState : BasicAttackState
	{
		private bool m_missed;

		private bool m_canWhiff;

		public override string AnimName
		{
			get
			{
				return "punch";
			}
		}

		public override int DamageFrame
		{
			get
			{
				return 7;
			}
		}

		public override float Damage
		{
			get
			{
				return 10f;
			}
		}

		public PunchState(bool isLeft, bool canWhiff)
			: base(isLeft)
		{
			m_canWhiff = canWhiff;
		}

		public override bool CanHitOpponent(State state)
		{
			if (m_missed)
			{
				return false;
			}
			bool flag = !(state is DuckState);
			if (state is BlockState)
			{
				(state as BlockState).Bonk();
				flag = false;
			}
			DodgeState dodgeState = state as DodgeState;
			if (dodgeState != null && dodgeState.IsLeft != IsLeft)
			{
				flag = false;
			}
			if (!flag && m_canWhiff)
			{
				base.Actor.Play("punch_miss", IsLeft);
				m_missed = true;
				return false;
			}
			return flag;
		}

		public override bool CanBeHit(bool isLeft)
		{
			return !base.WasBlocked && (m_missed || (base.Actor.CurrentFrame >= 3 && base.Actor.CurrentFrameFloat < 5.5f));
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (!m_missed && currentFrame == 3)
			{
				base.Actor.FlashWarn(2.5f);
			}
		}

		public override void OnHit(ref bool preventDamage, bool isLeft, int starsUsed)
		{
			base.OnHit(ref preventDamage, isLeft, starsUsed);
			if (!m_missed && (base.Actor.CurrentFrame == 4 || base.Actor.CurrentFrame == 5))
			{
				base.Actor.state = new DazeState();
			}
		}
	}

	public class UppercutState : BasicAttackState
	{
		public override string AnimName
		{
			get
			{
				return "uppercut";
			}
		}

		public override int DamageFrame
		{
			get
			{
				return 8;
			}
		}

		public override float Damage
		{
			get
			{
				return 20f;
			}
		}

		public override float PunishTime
		{
			get
			{
				return 0.3f;
			}
		}

		public UppercutState(bool isLeft)
			: base(isLeft)
		{
		}

		public override bool CanHitOpponent(State state)
		{
			if (state is BlockState)
			{
				(state as BlockState).Bonk();
				return false;
			}
			if (state is DuckState)
			{
				return false;
			}
			DodgeState dodgeState = state as DodgeState;
			if (dodgeState != null && dodgeState.IsLeft != IsLeft)
			{
				return false;
			}
			return true;
		}

		public override bool CanBeHit(bool isLeft)
		{
			return !base.WasBlocked && base.Actor.CurrentFrame > DamageFrame;
		}
	}

	public class SuperAttackState : BasicAttackState
	{
		public override string AnimName
		{
			get
			{
				return "super";
			}
		}

		public override int DamageFrame
		{
			get
			{
				return 16;
			}
		}

		public override float Damage
		{
			get
			{
				return 35f;
			}
		}

		public override float PunishTime
		{
			get
			{
				return 0.3f;
			}
		}

		public override bool CanHitOpponent(State state)
		{
			return !(state is DuckState);
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (currentFrame == 15)
			{
				base.Actor.FlashWarn(1f);
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			return !base.WasBlocked && (base.Actor.CurrentFrame == 15 || base.Actor.CurrentFrame >= 17);
		}

		public override bool IsFarAway()
		{
			return base.Actor.CurrentFrame >= 2 && base.Actor.CurrentFrame <= 15;
		}

		public override void OnHit(ref bool preventDamage, bool isLeft, int starsUsed)
		{
			base.OnHit(ref preventDamage, isLeft, starsUsed);
			if (base.Actor.CurrentFrame == 15)
			{
				base.Actor.state = new DazeState();
			}
		}
	}

	public class TailWhipState : BasicAttackState
	{
		public override string AnimName
		{
			get
			{
				return "tail_whip";
			}
		}

		public override int DamageFrame
		{
			get
			{
				return 11;
			}
		}

		public override float Damage
		{
			get
			{
				return 20f;
			}
		}

		public override float PunishTime
		{
			get
			{
				return 0.3f;
			}
		}

		public override void Update()
		{
			base.Update();
			base.WasBlocked = false;
		}

		public override bool CanHitOpponent(State state)
		{
			if (state is BlockState)
			{
				(state as BlockState).Bonk();
				return false;
			}
			return true;
		}

		public override bool CanBeHit(bool isLeft)
		{
			return base.Actor.CurrentFrameFloat >= 8.5f && base.Actor.CurrentFrame <= 10;
		}

		public override void OnHit(ref bool preventDamage, bool isLeft, int starsUsed)
		{
			base.OnHit(ref preventDamage, isLeft, starsUsed);
			base.Actor.state = new TailDazeState();
		}

		public override bool ShouldInstantKO(int starsUsed)
		{
			return starsUsed >= 1;
		}
	}

	public class ThrowAmmoState : State
	{
		private enum ThrowState
		{
			None,
			Intro,
			Switch,
			Throw,
			ThrowMiss,
			ThrowLaugh,
			Outro,
			Return,
			Hit
		}

		public float SwitchChance = 0.33f;

		public float Damage = 20f;

		public float ReturnDamage = 20f;

		private ThrowState m_state;

		private bool m_hasThrown;

		private Vector2 m_boxStartPos;

		private Vector2 m_boxEndPos;

		private float m_boxThrowTime;

		private float m_boxThrowTimer;

		public ThrowAmmoState(bool isLeft)
			: base(isLeft)
		{
		}

		public override void Start()
		{
			base.Actor.Play("throw_intro", IsLeft);
			m_state = ThrowState.Intro;
			base.Actor.MoveCamera(new Vector2(0f, 1f), 0.5f);
			base.Start();
		}

		public override void Update()
		{
			base.Update();
			if (m_state == ThrowState.Throw && m_hasThrown)
			{
				m_boxThrowTimer += BraveTime.DeltaTime;
				float num = m_boxThrowTimer / m_boxThrowTime;
				Vector2 vector = Vector2.Lerp(m_boxStartPos, m_boxEndPos, num);
				vector.y += Mathf.Sin(num * (float)Math.PI) * 0.5f;
				base.ActorEnemy.BoxAnimator.transform.localPosition = vector.ToVector3ZisY();
				base.ActorEnemy.BoxAnimator.sprite.HeightOffGround = Mathf.Lerp(16f, 5f, num);
				base.ActorEnemy.BoxAnimator.sprite.UpdateZDepth();
				if (!(m_boxThrowTimer >= m_boxThrowTime))
				{
					return;
				}
				if (CanHitOpponent())
				{
					base.ActorEnemy.DoBoxShells(m_boxEndPos + new Vector2(0f, 1f));
					PunchoutPlayerController.PlayerPunchState playerPunchState = base.Actor.Opponent.state as PunchoutPlayerController.PlayerPunchState;
					if (playerPunchState != null && playerPunchState.RealFrame == 0)
					{
						m_state = ThrowState.Return;
						BoxReturn();
						return;
					}
					m_state = ThrowState.ThrowLaugh;
					base.Actor.Play("throw_laugh", IsLeft);
					base.Actor.Opponent.Hit(!IsLeft, Damage);
					base.Actor.Opponent.aiAnimator.PlayVfx("box_hit");
					base.ActorEnemy.BoxAnimator.gameObject.SetActive(false);
				}
				else
				{
					m_state = ThrowState.ThrowMiss;
					base.Actor.Play("throw_miss", IsLeft);
					BoxMiss();
				}
			}
			else if (m_state == ThrowState.ThrowMiss)
			{
				m_boxThrowTimer += BraveTime.DeltaTime;
				float t = m_boxThrowTimer / m_boxThrowTime;
				base.ActorEnemy.BoxAnimator.transform.localPosition = Vector2.Lerp(m_boxStartPos, m_boxEndPos, t);
				base.ActorEnemy.BoxAnimator.sprite.HeightOffGround = 5f;
				base.ActorEnemy.BoxAnimator.sprite.UpdateZDepth();
				if (m_boxThrowTimer >= m_boxThrowTime)
				{
					base.ActorEnemy.BoxAnimator.gameObject.SetActive(false);
				}
			}
			else if (m_state == ThrowState.Return)
			{
				m_boxThrowTimer += BraveTime.DeltaTime;
				float t2 = m_boxThrowTimer / m_boxThrowTime;
				base.ActorEnemy.BoxAnimator.transform.localPosition = Vector2.Lerp(m_boxStartPos, m_boxEndPos, t2);
				base.ActorEnemy.BoxAnimator.sprite.HeightOffGround = Mathf.Lerp(6f, 16f, t2);
				base.ActorEnemy.BoxAnimator.sprite.UpdateZDepth();
				if (m_boxThrowTimer >= m_boxThrowTime)
				{
					base.Actor.Play("throw_hit", IsLeft);
					m_state = ThrowState.Hit;
					base.Actor.Hit(IsLeft, ReturnDamage, 0, true);
					base.Actor.aiAnimator.PlayVfx((!IsLeft) ? "box_hit_right" : "box_hit_left");
					base.ActorEnemy.BoxAnimator.gameObject.SetActive(false);
					base.ActorEnemy.DoBoxShellsBack(m_boxEndPos, IsLeft);
				}
			}
		}

		public bool CanHitOpponent()
		{
			State state = base.Actor.Opponent.state;
			if (m_state != ThrowState.Throw)
			{
				return false;
			}
			if (state is DuckState)
			{
				return false;
			}
			DodgeState dodgeState = state as DodgeState;
			if (dodgeState != null && dodgeState.IsLeft == IsLeft)
			{
				return false;
			}
			return true;
		}

		public override void OnAnimationCompleted()
		{
			base.OnAnimationCompleted();
			if (m_state == ThrowState.Intro)
			{
				if (UnityEngine.Random.value < SwitchChance)
				{
					base.Actor.Play("throw_switch", IsLeft);
					m_state = ThrowState.Switch;
				}
				else
				{
					base.Actor.Play("throw", IsLeft);
					m_state = ThrowState.Throw;
				}
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Light);
			}
			else if (m_state == ThrowState.Switch)
			{
				IsLeft = !IsLeft;
				base.Actor.Play("throw", IsLeft);
				m_state = ThrowState.Throw;
			}
			else if (m_state == ThrowState.Throw && base.Actor.aiAnimator.IsPlaying("throw"))
			{
				base.Actor.Play("throw_outro", IsLeft);
				m_state = ThrowState.Outro;
				base.Actor.MoveCamera(new Vector2(0f, 0f), 0.5f);
			}
			else if (m_state == ThrowState.ThrowLaugh || m_state == ThrowState.ThrowMiss || m_state == ThrowState.Hit)
			{
				base.Actor.Play("throw_outro", IsLeft);
				m_state = ThrowState.Outro;
				base.Actor.MoveCamera(new Vector2(0f, 0f), 0.5f);
			}
			else if (m_state == ThrowState.Outro)
			{
				base.IsDone = true;
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Light);
			}
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (m_state == ThrowState.Switch && currentFrame == 6)
			{
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Light);
			}
			else if (base.Actor.aiAnimator.IsPlaying("throw_miss") && currentFrame % 3 == 2)
			{
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Light);
			}
			else if (m_state == ThrowState.Throw && currentFrame == 15)
			{
				BoxThrow();
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			return (m_state == ThrowState.Throw && m_boxThrowTimer > base.ActorEnemy.BoxCounterStartTime) || (m_state == ThrowState.Return && (double)m_boxThrowTimer < 0.33);
		}

		public override bool IsFarAway()
		{
			return true;
		}

		public override void OnHit(ref bool preventDamage, bool isLeft, int starsUsed)
		{
			base.OnHit(ref preventDamage, isLeft, starsUsed);
			if (m_state == ThrowState.Throw)
			{
				preventDamage = true;
				m_state = ThrowState.Return;
				BoxReturn();
			}
			else if (m_state == ThrowState.Return)
			{
				preventDamage = true;
			}
		}

		private void BoxThrow()
		{
			m_boxStartPos = base.ActorEnemy.BoxStart;
			if (IsLeft)
			{
				m_boxStartPos.x *= -1f;
			}
			m_boxEndPos = base.ActorEnemy.BoxEnd;
			m_boxThrowTime = base.ActorEnemy.BoxThrowTime;
			m_boxThrowTimer = 0f;
			base.ActorEnemy.BoxAnimator.gameObject.SetActive(true);
			base.ActorEnemy.BoxAnimator.Play(IsLeft ? "rat_box_left" : "rat_box_right");
			base.ActorEnemy.BoxAnimator.transform.localPosition = m_boxStartPos;
			base.ActorEnemy.BoxAnimator.sprite.HeightOffGround = 16f;
			base.ActorEnemy.BoxAnimator.sprite.UpdateZDepth();
			m_hasThrown = true;
		}

		private void BoxMiss()
		{
			base.ActorEnemy.BoxAnimator.Play(IsLeft ? "rat_box_left_fall" : "rat_box_right_fall");
			Vector2 vector = m_boxEndPos - m_boxStartPos;
			m_boxStartPos = m_boxEndPos;
			m_boxEndPos = m_boxStartPos + vector;
			m_boxThrowTime = base.ActorEnemy.BoxAnimator.CurrentClip.BaseClipLength;
			m_boxThrowTimer = 0f;
		}

		private void BoxReturn()
		{
			m_boxEndPos = m_boxStartPos;
			m_boxStartPos = base.ActorEnemy.BoxAnimator.transform.localPosition.XY();
			if (m_boxStartPos.y < 2f)
			{
				m_boxStartPos.y = 2f;
			}
			m_boxThrowTime = base.ActorEnemy.BoxCounterReturnTime;
			m_boxThrowTimer = 0f;
			base.ActorEnemy.BoxAnimator.Play(IsLeft ? "rat_box_left_return" : "rat_box_right_return");
			base.ActorEnemy.BoxAnimator.transform.localPosition = m_boxStartPos;
			base.ActorEnemy.BoxAnimator.sprite.HeightOffGround = 6f;
			base.ActorEnemy.BoxAnimator.sprite.UpdateZDepth();
			base.ActorEnemy.BoxAnimator.ignoreTimeScale = true;
			StickyFrictionManager.Instance.RegisterCustomStickyFriction(0.3125f, 0f, false);
			base.Actor.Opponent.aiAnimator.PlayVfx((!base.Actor.Opponent.state.IsLeft) ? "box_punch_right" : "box_punch_left");
		}
	}

	public class PunchBasicComboState : BasicComboState
	{
		public override float PunishTime
		{
			get
			{
				return 0.3f;
			}
		}

		public PunchBasicComboState(bool firstIsLeft)
			: base(new State[4]
			{
				new PunchState(firstIsLeft, false),
				new PunchState(!firstIsLeft, false),
				new PunchState(firstIsLeft, false),
				new UppercutState(!firstIsLeft)
			})
		{
		}
	}

	public class BrassKnucklesPunchState : BasicAttackState
	{
		public override string AnimName
		{
			get
			{
				return "brass_punch";
			}
		}

		public override int DamageFrame
		{
			get
			{
				return 26;
			}
		}

		public override float Damage
		{
			get
			{
				return 20f;
			}
		}

		public override float PunishTime
		{
			get
			{
				return 0.3f;
			}
		}

		public BrassKnucklesPunchState(bool isLeft)
			: base(isLeft)
		{
		}

		public override bool CanHitOpponent(State state)
		{
			return true;
		}

		public override bool CanBeHit(bool isLeft)
		{
			return !base.WasBlocked && (base.Actor.CurrentFrame == 24 || base.Actor.CurrentFrame == 25);
		}

		public override bool IsFarAway()
		{
			return base.Actor.CurrentFrame < 23 || !(base.Actor.CurrentFrameFloat < 25.5f);
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (currentFrame == 4 || currentFrame == 14 || currentFrame == 19 || currentFrame == 24)
			{
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Light);
			}
			if (currentFrame == 23)
			{
				base.Actor.FlashWarn(2.5f);
			}
		}

		public override void OnHit(ref bool preventDamage, bool isLeft, int starsUsed)
		{
			base.OnHit(ref preventDamage, isLeft, starsUsed);
			base.Actor.state = new DazeState();
		}

		public override bool ShouldInstantKO(int starsUsed)
		{
			return starsUsed >= 3;
		}
	}

	public class SuperTailWhipState : BasicAttackState
	{
		private int m_spins;

		private bool m_hitPlayer;

		public override string AnimName
		{
			get
			{
				return "super_tail_whip";
			}
		}

		public int FlashFrame
		{
			get
			{
				return 15;
			}
		}

		public override int DamageFrame
		{
			get
			{
				return 18;
			}
		}

		public override float Damage
		{
			get
			{
				return 10f;
			}
		}

		public override float PunishTime
		{
			get
			{
				return 0.3f;
			}
		}

		public override void Update()
		{
			base.Update();
			base.WasBlocked = false;
		}

		public override bool CanHitOpponent(State state)
		{
			if (state is BlockState)
			{
				(state as BlockState).Bonk();
				return false;
			}
			m_hitPlayer = true;
			return true;
		}

		public override bool CanBeHit(bool isLeft)
		{
			if (m_spins == 0 && base.Actor.CurrentFrame >= FlashFrame && base.Actor.CurrentFrame < DamageFrame)
			{
				return true;
			}
			return false;
		}

		public override bool IsFarAway()
		{
			return base.Actor.CurrentFrame >= 6 && base.Actor.CurrentFrame <= 14;
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (currentFrame == 5 || currentFrame == 7 || currentFrame == 9 || currentFrame == 11)
			{
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Light);
			}
			if (m_spins == 0 && currentFrame == FlashFrame)
			{
				base.Actor.FlashWarn(DamageFrame - FlashFrame);
			}
			if (currentFrame == DamageFrame)
			{
				m_spins++;
			}
			if (currentFrame == 22 && m_spins >= 4)
			{
				if (m_hitPlayer)
				{
					base.Actor.aiAnimator.EndAnimation();
				}
				else
				{
					base.Actor.state = new DazeState();
				}
			}
		}

		public override void OnHit(ref bool preventDamage, bool isLeft, int starsUsed)
		{
			base.OnHit(ref preventDamage, isLeft, starsUsed);
			base.Actor.state = new DazeState();
		}

		public override bool ShouldInstantKO(int starsUsed)
		{
			return starsUsed >= 3;
		}
	}

	public class LaughTauntState : State
	{
		public override string AnimName
		{
			get
			{
				return "laugh_taunt";
			}
		}

		public LaughTauntState()
			: base(false)
		{
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (currentFrame == 6)
			{
				base.Actor.FlashWarn(1.5f);
			}
			if (currentFrame % 3 == 1)
			{
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Light);
			}
		}

		public override bool ShouldInstantKO(int starsUsed)
		{
			return starsUsed >= 2;
		}

		public override void Stop()
		{
			base.Stop();
			base.ActorEnemy.TauntCooldownTimer = base.ActorEnemy.TauntCooldown;
		}
	}

	public class CheeseTauntState : State
	{
		public float HealAmount = 30f;

		private bool m_isCountering;

		private float m_startingHealth;

		private float? m_targetHealth;

		public override string AnimName
		{
			get
			{
				return "cheese_taunt";
			}
		}

		public CheeseTauntState()
			: base(false)
		{
		}

		public override void Start()
		{
			base.Start();
			m_startingHealth = base.Actor.Health;
			m_targetHealth = Mathf.Min(100f, m_startingHealth + HealAmount);
		}

		public override bool CanBeHit(bool isLeft)
		{
			if (!m_isCountering)
			{
				if (base.Actor.CurrentFrame < 9)
				{
					m_isCountering = true;
					base.Actor.Play("cheese_taunt_counter");
					return false;
				}
				return base.Actor.CurrentFrameFloat >= 8.5f && base.Actor.CurrentFrame <= 9 && !isLeft;
			}
			return false;
		}

		public override void OnFrame(int currentFrame)
		{
			if (!m_isCountering)
			{
				switch (currentFrame)
				{
				case 9:
					GameManager.Instance.PrimaryPlayer.DoVibration(0.545454562f, Vibration.Strength.Light);
					break;
				case 10:
					base.ActorEnemy.DoHealSuck(new Vector3(-0.125f, 2f, -2.5f));
					base.ActorEnemy.SuccessfulHeals++;
					break;
				case 11:
				case 15:
					base.Actor.PulseColor(base.ActorEnemy.RedPulseColor, 3f);
					break;
				}
				return;
			}
			switch (currentFrame)
			{
			case 8:
				base.Actor.Opponent.Hit(true, 3f);
				(base.Actor.Opponent as PunchoutPlayerController).Exhaust(1.45f);
				break;
			case 16:
				GameManager.Instance.PrimaryPlayer.DoVibration(0.727272749f, Vibration.Strength.Light);
				break;
			case 17:
				base.ActorEnemy.DoHealSuck(new Vector3(-0.125f, 2f, -2.5f));
				base.ActorEnemy.SuccessfulHeals++;
				break;
			case 18:
			case 22:
				base.Actor.PulseColor(base.ActorEnemy.RedPulseColor, 3f);
				break;
			}
		}

		public override void Update()
		{
			base.Update();
			if (!m_isCountering)
			{
				if (base.Actor.CurrentFrame >= 10)
				{
					float? targetHealth = m_targetHealth;
					if (targetHealth.HasValue)
					{
						base.Actor.Health = Mathf.Lerp(m_startingHealth, m_targetHealth.Value, Mathf.Clamp01((base.Actor.CurrentFrameFloat - 10f) / 7f));
					}
				}
			}
			else if (base.Actor.CurrentFrame >= 17)
			{
				float? targetHealth2 = m_targetHealth;
				if (targetHealth2.HasValue)
				{
					base.Actor.Health = Mathf.Lerp(m_startingHealth, m_targetHealth.Value, Mathf.Clamp01((base.Actor.CurrentFrameFloat - 17f) / 7f));
				}
			}
		}

		public override void OnHit(ref bool preventDamage, bool isLeft, int starsUsed)
		{
			base.OnHit(ref preventDamage, isLeft, starsUsed);
			m_targetHealth = null;
			if (starsUsed == 0 && !m_isCountering && base.Actor.CurrentFrame == 9)
			{
				preventDamage = true;
				base.Actor.state = new CheeseHitState();
				((PunchoutPlayerController)base.Actor.Opponent).AddStar();
			}
		}

		public override void Stop()
		{
			base.Stop();
			base.ActorEnemy.TauntCooldownTimer = base.ActorEnemy.TauntCooldown;
			float? targetHealth = m_targetHealth;
			if (targetHealth.HasValue)
			{
				base.Actor.Health = m_targetHealth.Value;
			}
		}
	}

	public class IntroState : State
	{
		private enum State
		{
			MaybeIntro,
			Tutorial,
			Transition,
			Intro
		}

		private State m_state;

		public override string AnimName
		{
			get
			{
				return null;
			}
		}

		public override void Start()
		{
			base.Start();
			GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_RatPunch_Intro_01", base.Actor.gameObject);
			base.Actor.Play("intro");
			m_state = State.MaybeIntro;
		}

		public override void Update()
		{
			base.Update();
			if (m_state == State.MaybeIntro)
			{
				if (PunchoutController.InTutorial)
				{
					base.Actor.Play("intro_tutorial");
					m_state = State.Tutorial;
				}
			}
			else if (m_state == State.Tutorial)
			{
				if (!PunchoutController.InTutorial)
				{
					base.Actor.Play("intro_transition");
					m_state = State.Transition;
				}
			}
			else if (m_state == State.Transition && base.Actor.aiAnimator.IsIdle())
			{
				base.Actor.Play("intro");
				m_state = State.Intro;
			}
		}

		public override void Stop()
		{
			base.Stop();
			GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_RatPunch_Theme_01", base.Actor.gameObject);
		}

		public override bool IsFarAway()
		{
			if (!PunchoutController.IsActive)
			{
				return true;
			}
			return m_state != 0 && m_state != State.Intro;
		}

		public override bool CanBeHit(bool isLeft)
		{
			if (!PunchoutController.IsActive)
			{
				return false;
			}
			return m_state == State.MaybeIntro || m_state == State.Intro;
		}
	}

	public class InstantKnockdownState : State
	{
		private enum KnockdownState
		{
			None,
			Fall,
			Attack
		}

		public int DamageFrame = 11;

		public float Damage = 10f;

		private KnockdownState m_state;

		public InstantKnockdownState(bool isLeft)
			: base(isLeft)
		{
		}

		public override void Start()
		{
			base.Actor.Play("knockdown", IsLeft);
			m_state = KnockdownState.Fall;
			base.ActorEnemy.UpdateUI(base.ActorEnemy.Phase + 1);
			base.ActorEnemy.DropKey(IsLeft);
			base.Start();
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (m_state == KnockdownState.Fall && currentFrame == 10)
			{
				m_state = KnockdownState.Attack;
				base.Actor.Play("knockdown_cheat", !IsLeft);
			}
			else if (m_state == KnockdownState.Attack && currentFrame == DamageFrame && CanHitOpponent())
			{
				base.Actor.Opponent.Hit(!IsLeft, Damage);
			}
		}

		public bool CanHitOpponent()
		{
			State state = base.Actor.Opponent.state;
			if (state is BlockState)
			{
				(state as BlockState).Bonk();
				return false;
			}
			return !(state is DuckState);
		}

		public override void OnAnimationCompleted()
		{
			base.OnAnimationCompleted();
			if (m_state == KnockdownState.Attack)
			{
				base.ActorEnemy.GoToNextPhase(null, false);
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			return false;
		}

		public override bool IsFarAway()
		{
			return true;
		}
	}

	public class DeathState : State
	{
		private bool m_killedBySuper;

		public DeathState(bool isLeft, bool killedBySuper)
			: base(isLeft)
		{
			m_killedBySuper = killedBySuper;
		}

		public override void Start()
		{
			base.Start();
			base.Actor.aiAnimator.FacingDirection = (IsLeft ? 180 : 0);
			base.Actor.aiAnimator.PlayUntilCancelled((!m_killedBySuper) ? "die" : "die_super");
			base.ActorEnemy.DropKey(IsLeft);
			if (m_killedBySuper)
			{
				base.ActorEnemy.DropKey(IsLeft);
			}
			if (base.ActorEnemy.NumTimesTripleStarred >= 3)
			{
				base.ActorEnemy.DropKey(IsLeft);
			}
			base.ActorEnemy.DropReward(IsLeft, PickupObject.ItemQuality.A, PickupObject.ItemQuality.S);
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (m_killedBySuper)
			{
				switch (currentFrame)
				{
				case 10:
					SpriteOutlineManager.RemoveOutlineFromSprite(base.Actor.sprite);
					return;
				case 18:
					if (!(base.Actor.Opponent.state is PunchoutPlayerController.WinState))
					{
						((PunchoutPlayerController)base.Actor.Opponent).Win();
						return;
					}
					break;
				}
				if (currentFrame == 30)
				{
					base.Actor.transform.position += new Vector3(0f, -0.6875f);
				}
			}
			else if (currentFrame == 13 && !(base.Actor.Opponent.state is PunchoutPlayerController.WinState))
			{
				((PunchoutPlayerController)base.Actor.Opponent).Win();
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			return false;
		}

		public override bool IsFarAway()
		{
			return true;
		}
	}

	public class WinState : State
	{
		public override void Start()
		{
			base.Start();
			base.Actor.aiAnimator.PlayUntilCancelled("win");
		}

		public override bool CanBeHit(bool isLeft)
		{
			return false;
		}
	}

	public class EscapeState : State
	{
		public override void Start()
		{
			base.Start();
			base.Actor.aiAnimator.FacingDirection = -90f;
			base.Actor.aiAnimator.PlayUntilCancelled("escape");
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (currentFrame == 27)
			{
				base.ActorEnemy.DoFadeOut();
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			return false;
		}

		public override bool IsFarAway()
		{
			return true;
		}
	}

	public class PhaseTransitionState : State
	{
		private float m_startingHealth;

		public override string AnimName
		{
			get
			{
				return "transition";
			}
		}

		public PhaseTransitionState(bool isLeft, float startingHealth)
			: base(isLeft)
		{
			m_startingHealth = startingHealth;
		}

		public override void Start()
		{
			base.Start();
			base.ActorEnemy.UpdateUI(base.ActorEnemy.Phase + 1);
			if (base.ActorEnemy.Phase == 0)
			{
				GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_RatPunch_Transition_01", base.Actor.gameObject);
			}
			else
			{
				GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_RatPunch_Transition_02", base.Actor.gameObject);
			}
		}

		public override void Update()
		{
			base.Update();
			if (base.Actor.CurrentFrame >= 16)
			{
				base.Actor.Health = Mathf.Lerp(m_startingHealth, 100f, Mathf.Clamp01((base.Actor.CurrentFrameFloat - 16f) / 8f));
			}
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			switch (currentFrame)
			{
			case 16:
				base.ActorEnemy.DoHealSuck(new Vector3(IsLeft ? 1 : (-1), 3.625f, -4.3125f));
				break;
			case 17:
			case 21:
				base.Actor.PulseColor(base.ActorEnemy.RedPulseColor, 3f);
				break;
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			return false;
		}

		public override bool IsFarAway()
		{
			return true;
		}

		public override void Stop()
		{
			base.Stop();
			base.Actor.Health = 100f;
			base.ActorEnemy.Phase = (base.ActorEnemy.Phase + 1) % 3;
			base.ActorEnemy.SuccessfulHeals = 0;
			if (base.ActorEnemy.Phase == 1)
			{
				GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_RatPunch_Theme_02", base.Actor.gameObject);
			}
			else
			{
				GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_RatPunch_Theme_03", base.Actor.gameObject);
			}
		}
	}

	public new class HitState : State
	{
		private int m_maxHits;

		private int m_hits = 1;

		private bool m_isAlternating = true;

		public override string AnimName
		{
			get
			{
				return "hit";
			}
		}

		public HitState(bool isLeft, int remainingHits)
			: base(isLeft)
		{
			m_maxHits = remainingHits;
		}

		public override void Update()
		{
			base.Update();
			if (!(base.Actor.Opponent.state is BasicAttackState) || base.Actor.Opponent.state == base.Actor.LastHitBy)
			{
				return;
			}
			if (m_isAlternating && (base.Actor.Opponent.state as BasicAttackState).IsLeft != IsLeft)
			{
				if (m_hits + 1 > m_maxHits * 2)
				{
					base.Actor.state = new BlockState();
				}
			}
			else if (m_hits + 1 > m_maxHits)
			{
				base.Actor.state = new BlockState();
			}
		}

		public void HitAgain(bool newIsLeft)
		{
			m_hits++;
			if (newIsLeft == IsLeft)
			{
				m_isAlternating = false;
			}
			IsLeft = newIsLeft;
			base.Actor.Play("hit", IsLeft);
		}
	}

	public class SuperHitState : State
	{
		public override string AnimName
		{
			get
			{
				return "hit_super";
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			return false;
		}
	}

	public class CheeseHitState : State
	{
		public override string AnimName
		{
			get
			{
				return "cheese_taunt_hit";
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			return false;
		}
	}

	public class DazeState : State
	{
		public override string AnimName
		{
			get
			{
				return "daze";
			}
		}

		public override void Stop()
		{
			base.Stop();
			base.Actor.aiAnimator.EndAnimationIf("daze");
		}
	}

	public class TailDazeState : State
	{
		public override string AnimName
		{
			get
			{
				return "tail_whip_hit";
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			return false;
		}

		public override void OnAnimationCompleted()
		{
			base.OnAnimationCompleted();
			base.Actor.state = new DazeState();
		}
	}

	public dfSprite RatUiSprite;

	public tk2dSpriteAnimator BoxAnimator;

	public GameObject HealParticleVfx;

	public GameObject BoxShellVfx;

	public GameObject BoxShellBackVfx;

	public GameObject DroppedItemPrefab;

	[Header("Constants")]
	public float TauntCooldown = 5f;

	public int MaxHeals = 2;

	public Vector2 BoxStart;

	public Vector2 BoxEnd;

	public float BoxThrowTime;

	public float BoxCounterStartTime;

	public float BoxCounterReturnTime;

	public Color RedPulseColor;

	[Header("AI Phase 1")]
	public float p1AttackChance;

	public float p1TauntChance;

	public float p1PunchChance;

	public float p1UppercutChance;

	[Header("AI Phase 2")]
	public float p2AttackChance;

	public float p2TauntChance;

	public float p2SneakChance;

	public float p2ComboChance;

	public float p2TailwhipChance;

	public float p2ThrowAmmoChance;

	[Header("AI Phase 3")]
	public float p3AttackChance;

	public float p3TauntChance;

	public float p3SneakChance;

	public float p3ComboChance;

	public float p3TailwhipChance;

	public float p3ThrowAmmoChance;

	public float p3BrassKnucklesChance;

	public float p3TailTornadoChance;

	public List<int> DroppedRewardIds = new List<int>();

	private PunchoutController m_punchoutController;

	private float m_punishTimer;

	private Vector2 m_startPosition;

	private bool m_droppedFirstKey;

	private int m_hitUntilFirstDrop = 5;

	private bool m_hasDroppedStarDrop;

	private int m_glassGuonsDropped;

	public int Phase { get; set; }

	public float TauntCooldownTimer { get; set; }

	public int SuccessfulHeals { get; set; }

	public int NumKeysDropped { get; set; }

	public int NumTimesTripleStarred { get; set; }

	public override bool IsDead
	{
		get
		{
			return base.state is DeathState;
		}
	}

	public override void Start()
	{
		base.Start();
		m_punchoutController = UnityEngine.Object.FindObjectOfType<PunchoutController>();
		base.state = new IntroState();
		float num = p1TauntChance + p1PunchChance + p1UppercutChance;
		p1TauntChance /= num;
		p1PunchChance /= num;
		p1UppercutChance /= num;
		num = p2TauntChance + p2SneakChance + p2ComboChance + p2TailwhipChance + p2ThrowAmmoChance;
		p2TauntChance /= num;
		p2SneakChance /= num;
		p2ComboChance /= num;
		p2TailwhipChance /= num;
		p2ThrowAmmoChance /= num;
		num = p3TauntChance + p3SneakChance + p3ComboChance + p3TailwhipChance + p3ThrowAmmoChance + p3BrassKnucklesChance + p3TailTornadoChance;
		p3TauntChance /= num;
		p3SneakChance /= num;
		p3ComboChance /= num;
		p3TailwhipChance /= num;
		p3ThrowAmmoChance /= num;
		p3BrassKnucklesChance /= num;
		p3TailTornadoChance /= num;
		m_startPosition = base.transform.localPosition;
		m_hitUntilFirstDrop = UnityEngine.Random.Range(5, 9);
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		m_punishTimer = Mathf.Max(0f, m_punishTimer - BraveTime.DeltaTime);
		TauntCooldownTimer = Mathf.Max(0f, TauntCooldownTimer - BraveTime.DeltaTime);
		bool finishedThisFrame = false;
		if (base.state != null)
		{
			base.state.Update();
			if (base.state.IsDone)
			{
				if (base.state.PunishTime > 0f && !base.state.WasBlocked)
				{
					m_punishTimer = base.state.PunishTime;
				}
				base.state = null;
				finishedThisFrame = true;
			}
		}
		if (base.state == null)
		{
			base.state = GetNextState(finishedThisFrame);
		}
	}

	public State GetNextState(bool finishedThisFrame)
	{
		if (m_punishTimer > 0f)
		{
			return null;
		}
		if (Opponent.state is PunchoutPlayerController.DeathState && base.state == null)
		{
			return new WinState();
		}
		if (Opponent.state is BasicAttackState)
		{
			if (Opponent.aiAnimator.IsPlaying("super"))
			{
				return new PunchState(BraveUtility.RandomBool(), true);
			}
			return new BlockState();
		}
		if (finishedThisFrame)
		{
			return null;
		}
		if (Phase == 0)
		{
			if (UnityEngine.Random.value < BraveMathCollege.SliceProbability(p1AttackChance, BraveTime.DeltaTime))
			{
				float num = ((!(TauntCooldownTimer <= 0f)) ? UnityEngine.Random.RandomRange(0f, 1f - p1TauntChance) : UnityEngine.Random.value);
				if (num < p1PunchChance)
				{
					return new PunchState(BraveUtility.RandomBool(), true);
				}
				num -= p1PunchChance;
				if (num < p1UppercutChance)
				{
					return new UppercutState(BraveUtility.RandomBool());
				}
				num -= p1UppercutChance;
				if (num < p1TauntChance)
				{
					return new LaughTauntState();
				}
				num -= p1TauntChance;
			}
			return null;
		}
		if (Phase == 1)
		{
			if (UnityEngine.Random.value < BraveMathCollege.SliceProbability(p2AttackChance, BraveTime.DeltaTime))
			{
				float num2 = ((!(TauntCooldownTimer <= 0f) || !(Health < 100f) || SuccessfulHeals >= MaxHeals) ? UnityEngine.Random.RandomRange(0f, 1f - p2TauntChance) : UnityEngine.Random.value);
				if (num2 < p2SneakChance)
				{
					return new SuperAttackState();
				}
				num2 -= p2SneakChance;
				if (num2 < p2ComboChance)
				{
					return new PunchBasicComboState(BraveUtility.RandomBool());
				}
				num2 -= p2ComboChance;
				if (num2 < p2TailwhipChance)
				{
					return new TailWhipState();
				}
				num2 -= p2TailwhipChance;
				if (num2 < p2ThrowAmmoChance)
				{
					return new ThrowAmmoState(BraveUtility.RandomBool());
				}
				num2 -= p2ThrowAmmoChance;
				if (num2 < p2TauntChance)
				{
					return new CheeseTauntState();
				}
				num2 -= p2TauntChance;
			}
			return null;
		}
		if (Phase == 2)
		{
			if (UnityEngine.Random.value < BraveMathCollege.SliceProbability(p3AttackChance, BraveTime.DeltaTime))
			{
				float num3 = ((!(TauntCooldownTimer <= 0f) || !(Health < 100f) || SuccessfulHeals >= MaxHeals) ? UnityEngine.Random.RandomRange(0f, 1f - p3TauntChance) : UnityEngine.Random.value);
				if (num3 < p3SneakChance)
				{
					return new SuperAttackState();
				}
				num3 -= p3SneakChance;
				if (num3 < p3ComboChance)
				{
					return new PunchBasicComboState(BraveUtility.RandomBool());
				}
				num3 -= p3ComboChance;
				if (num3 < p3TailwhipChance)
				{
					return new TailWhipState();
				}
				num3 -= p3TailwhipChance;
				if (num3 < p3ThrowAmmoChance)
				{
					return new ThrowAmmoState(BraveUtility.RandomBool());
				}
				num3 -= p3ThrowAmmoChance;
				if (num3 < p3BrassKnucklesChance)
				{
					return new BrassKnucklesPunchState(BraveUtility.RandomBool());
				}
				num3 -= p3BrassKnucklesChance;
				if (num3 < p3TailTornadoChance)
				{
					return new SuperTailWhipState();
				}
				num3 -= p3TailTornadoChance;
				if (num3 < p3TauntChance)
				{
					return new CheeseTauntState();
				}
				num3 -= p3TauntChance;
			}
			return null;
		}
		return null;
	}

	public override void Hit(bool isLeft, float damage, int starsUsed = 0, bool skipProcessing = false)
	{
		State state = base.state;
		bool preventDamage = false;
		if (base.state != null && !skipProcessing)
		{
			if (base.state.ShouldInstantKO(starsUsed))
			{
				if (starsUsed > 0 && !m_hasDroppedStarDrop)
				{
					DropReward(isLeft, PickupObject.ItemQuality.A);
					m_hasDroppedStarDrop = true;
				}
				if (starsUsed >= 3)
				{
					NumTimesTripleStarred++;
					Debug.LogWarningFormat("Hit by 3 stars {0} times", NumTimesTripleStarred);
				}
				base.aiAnimator.PlayVfx("star_hit");
				AkSoundEngine.PostEvent("Play_BOSS_Punchout_Punch_Hit_01", base.gameObject);
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Slow, Vibration.Strength.Hard);
				Knockdown(isLeft, true);
				return;
			}
			base.state.OnHit(ref preventDamage, isLeft, starsUsed);
		}
		if (!preventDamage)
		{
			if (base.IsYellow)
			{
				((PunchoutPlayerController)Opponent).AddStar();
			}
			if (!m_droppedFirstKey)
			{
				DropKey(isLeft);
				m_droppedFirstKey = true;
			}
			if (m_hitUntilFirstDrop > 0)
			{
				m_hitUntilFirstDrop--;
				if (m_hitUntilFirstDrop == 0)
				{
					DropReward(isLeft, PickupObject.ItemQuality.COMMON, PickupObject.ItemQuality.D, PickupObject.ItemQuality.C);
				}
			}
			if (UnityEngine.Random.value < m_punchoutController.NormalHitRewardChance)
			{
				DropReward(isLeft);
			}
			base.aiAnimator.PlayVfx((starsUsed <= 0) ? "normal_hit" : "star_hit");
			AkSoundEngine.PostEvent("Play_BOSS_Punchout_Punch_Hit_01", base.gameObject);
			if (starsUsed > 0)
			{
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Slow, Vibration.Strength.Hard);
				if (!m_hasDroppedStarDrop)
				{
					DropReward(isLeft, PickupObject.ItemQuality.A);
					m_hasDroppedStarDrop = true;
				}
			}
			else
			{
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
			}
			if (starsUsed > 0 && !(base.state is InstantKnockdownState))
			{
				if (!(Health - damage > 0f))
				{
					if (starsUsed >= 3)
					{
						NumTimesTripleStarred++;
						Debug.LogWarningFormat("Hit by 3 stars {0} times", NumTimesTripleStarred);
					}
					Knockdown(isLeft, true);
					return;
				}
				base.state = new SuperHitState();
			}
			else if (base.state == state && !skipProcessing)
			{
				if (base.state is HitState)
				{
					(base.state as HitState).HitAgain(isLeft);
				}
				else
				{
					int remainingHits = ((!(base.state is DazeState)) ? 3 : 5);
					base.state = new HitState(isLeft, remainingHits);
				}
			}
			base.LastHitBy = Opponent.state;
			Health -= damage;
			FlashDamage();
			if (skipProcessing && Health < 1f)
			{
				Health = 1f;
			}
		}
		else
		{
			GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
		}
		if (Health <= 0f && !skipProcessing)
		{
			if (Phase == 0)
			{
				DropReward(isLeft, PickupObject.ItemQuality.C);
			}
			else
			{
				DropReward(isLeft, PickupObject.ItemQuality.B);
			}
			GoToNextPhase(isLeft, starsUsed > 0);
		}
	}

	private void DropKey(bool isLeft)
	{
		StartCoroutine(DropKeyCR(isLeft));
	}

	private IEnumerator DropKeyCR(bool isLeft)
	{
		NumKeysDropped++;
		while (base.state is ThrowAmmoState)
		{
			yield return null;
		}
		GameObject droppedItem = SpawnManager.SpawnVFX(DroppedItemPrefab, base.transform.position + new Vector3(-0.25f, 2.5f), Quaternion.identity);
		droppedItem.GetComponent<PunchoutDroppedItem>().Init(isLeft);
	}

	private void DropReward(bool isLeft, params PickupObject.ItemQuality[] targetQualities)
	{
		StartCoroutine(DropRewardCR(isLeft, targetQualities));
	}

	private IEnumerator DropRewardCR(bool isLeft, params PickupObject.ItemQuality[] targetQualities)
	{
		int rewardId = -1;
		if (targetQualities == null || targetQualities.Length == 0)
		{
			rewardId = BraveUtility.RandomElement(m_punchoutController.NormalHitRewards);
			if (rewardId == GlobalItemIds.GlassGuonStone)
			{
				if (m_glassGuonsDropped >= m_punchoutController.MaxGlassGuonStones)
				{
					rewardId = GlobalItemIds.SmallHeart;
				}
				else
				{
					m_glassGuonsDropped++;
				}
			}
		}
		else
		{
			if (targetQualities.Length > 1)
			{
				Debug.LogFormat("Dropping a {0}-{1} item.", targetQualities[0].ToString(), targetQualities[targetQualities.Length - 1].ToString());
			}
			else
			{
				Debug.LogFormat("Dropping a {0} item.", targetQualities[0].ToString());
			}
			RewardManager rewardManager = GameManager.Instance.RewardManager;
			GenericLootTable tableToUse = ((!BraveUtility.RandomBool()) ? rewardManager.ItemsLootTable : rewardManager.GunsLootTable);
			GameObject itemForPlayer = rewardManager.GetItemForPlayer(GameManager.Instance.BestActivePlayer, tableToUse, BraveUtility.RandomElement(targetQualities), null);
			if ((bool)itemForPlayer)
			{
				rewardId = itemForPlayer.GetComponent<PickupObject>().PickupObjectId;
			}
		}
		if (rewardId >= 0)
		{
			DroppedRewardIds.Add(rewardId);
			while (base.state is ThrowAmmoState)
			{
				yield return null;
			}
			GameObject droppedItem = SpawnManager.SpawnVFX(DroppedItemPrefab, base.transform.position + new Vector3(-0.25f, 2.5f), Quaternion.identity);
			tk2dSprite droppedItemSprite = droppedItem.GetComponent<tk2dSprite>();
			tk2dSprite rewardSprite = PickupObjectDatabase.GetById(rewardId).GetComponent<tk2dSprite>();
			droppedItemSprite.SetSprite(rewardSprite.Collection, rewardSprite.spriteId);
			droppedItem.GetComponent<PunchoutDroppedItem>().Init(isLeft);
		}
	}

	public void Knockdown(bool isLeft, bool triggeredBySuper)
	{
		Health = 0f;
		if (Phase < 2)
		{
			if (Phase == 0)
			{
				DropReward(isLeft, PickupObject.ItemQuality.C);
			}
			else
			{
				DropReward(isLeft, PickupObject.ItemQuality.B);
			}
			base.state = new InstantKnockdownState(isLeft);
		}
		else
		{
			base.state = new DeathState(isLeft, triggeredBySuper);
		}
	}

	public void GoToNextPhase(bool? isLeft, bool triggeredBySuper)
	{
		if (!isLeft.HasValue)
		{
			isLeft = false;
		}
		if (Phase < 2)
		{
			base.state = new PhaseTransitionState(isLeft.Value, Health);
		}
		else
		{
			base.state = new DeathState(isLeft.Value, triggeredBySuper);
		}
	}

	public void UpdateUI(int phase = -1)
	{
		if (phase < 0)
		{
			phase = Phase;
		}
		switch (phase)
		{
		case 0:
			HealthBarUI.SpriteName = "punch_health_bar_green_001";
			RatUiSprite.SpriteName = "punch_boss_health_rat_001";
			break;
		case 1:
			HealthBarUI.SpriteName = "punch_health_bar_yellow_001";
			RatUiSprite.SpriteName = "punch_boss_health_rat_002";
			break;
		default:
			HealthBarUI.SpriteName = "punch_health_bar_001";
			RatUiSprite.SpriteName = "punch_boss_health_rat_003";
			break;
		}
	}

	public bool ShouldInstantKO(int starsUsed)
	{
		if (base.state == null)
		{
			return false;
		}
		return base.state.ShouldInstantKO(starsUsed);
	}

	public void DoFadeOut()
	{
		base.aiAnimator.PlayVfx("bomb_explosion");
		m_punchoutController.DoBombFade();
	}

	public void DoHealSuck(Vector3 deltaPos)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(HealParticleVfx, base.transform.position + deltaPos, Quaternion.Euler(-45f, 0f, 0f));
		ParticleKiller component = gameObject.GetComponent<ParticleKiller>();
		component.ForceInit();
	}

	public void DoBoxShells(Vector3 deltaPos)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(BoxShellVfx, base.transform.position + deltaPos, Quaternion.Euler(325f, 0f, 0f));
		ParticleKiller component = gameObject.GetComponent<ParticleKiller>();
		component.ForceInit();
	}

	public void DoBoxShellsBack(Vector3 deltaPos, bool isLeft)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(BoxShellBackVfx, base.transform.position + deltaPos + new Vector3(0f, 0f, 1.75f), Quaternion.Euler(340f, (!isLeft) ? 225 : 135, 180f));
		ParticleKiller component = gameObject.GetComponent<ParticleKiller>();
		component.ForceInit();
	}

	public void Reset()
	{
		Phase = 0;
		Health = 100f;
		UpdateUI(0);
		NumKeysDropped = 0;
		DroppedRewardIds.Clear();
		SuccessfulHeals = 0;
		m_droppedFirstKey = false;
		m_hitUntilFirstDrop = UnityEngine.Random.Range(5, 9);
		m_hasDroppedStarDrop = false;
		m_glassGuonsDropped = 0;
		NumTimesTripleStarred = 0;
		if (base.state is DeathState)
		{
			base.state.IsDone = true;
			base.aiAnimator.EndAnimationIf("die");
		}
		base.aiAnimator.EndAnimation();
		base.state = new IntroState();
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
		base.transform.localPosition = m_startPosition.ToVector3ZUp(base.transform.localPosition.z);
		Opponent.state = null;
		Opponent.Health = 0f;
		Opponent.aiAnimator.EndAnimation();
		(Opponent as PunchoutPlayerController).CurrentExhaust = 0f;
		(Opponent as PunchoutPlayerController).Stars = 0;
	}
}
