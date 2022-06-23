using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchoutPlayerController : PunchoutGameActor
{
	public class PlayerBlockState : BlockState
	{
		public string HitAnimName = "block_hit";

		private bool m_hasBonk;

		public override string AnimName
		{
			get
			{
				return "block";
			}
		}

		public override void Bonk()
		{
			base.ActorPlayer.VfxIsAboveCharacter = false;
			base.Actor.Play(HitAnimName);
			StickyFrictionManager.Instance.RegisterCustomStickyFriction(base.ActorPlayer.BlockStickyFriction, 0f, false);
			base.Actor.aiAnimator.PlayVfx("block_ss");
			GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Light);
			if (base.Actor.Opponent.state != null)
			{
				base.Actor.Opponent.state.WasBlocked = true;
			}
		}

		public override void OnHit(ref bool preventDamage, bool isLeft, int starsUsed)
		{
			base.OnHit(ref preventDamage, isLeft, starsUsed);
			base.ActorPlayer.VfxIsAboveCharacter = false;
			base.Actor.aiAnimator.ChildAnimator.ChildAnimator.PlayUntilFinished("block_break");
		}
	}

	public class PlayerDuckState : DuckState
	{
		public override void Start()
		{
			base.Start();
			base.Actor.MoveCamera(new Vector2(0f, 0f - base.ActorPlayer.DuckCameraSway), 0.2f);
		}

		public override void Stop()
		{
			base.Stop();
			base.Actor.MoveCamera(new Vector2(0f, 0f), 0.2f);
		}
	}

	public class PlayerDodgeState : DodgeState
	{
		public PlayerDodgeState(bool isLeft)
			: base(isLeft)
		{
		}

		public override void Start()
		{
			base.Start();
			base.Actor.MoveCamera(new Vector2(base.ActorPlayer.DodgeCameraSway * (float)((!IsLeft) ? 1 : (-1)), 0f), 0.15f);
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (currentFrame == 3)
			{
				base.Actor.MoveCamera(new Vector2(0f, 0f), 0.25f);
			}
		}

		public override void Stop()
		{
			base.Stop();
			base.Actor.MoveCamera(new Vector2(0f, 0f), 0.15f);
		}
	}

	public class PlayerPunchState : BasicAttackState
	{
		private bool m_missed;

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
				return 1;
			}
		}

		public override float Damage
		{
			get
			{
				return 5f;
			}
		}

		public int RealFrame
		{
			get
			{
				if (m_missed)
				{
					return base.Actor.CurrentFrame + 1;
				}
				return base.Actor.CurrentFrame;
			}
		}

		public PlayerPunchState(bool isLeft)
			: base(isLeft)
		{
		}

		public override void Start()
		{
			base.Start();
			base.Actor.MoveCamera(new Vector2(0f, base.ActorPlayer.PunchCameraSway), 0.04f);
		}

		public override bool CanHitOpponent(State state)
		{
			if (m_missed)
			{
				return false;
			}
			bool flag = !(state is BlockState) && (state == null || state.CanBeHit(IsLeft));
			if (PunchoutController.InTutorial)
			{
				base.ActorPlayer.CurrentExhaust = 0f;
				m_missed = true;
			}
			else if (!flag)
			{
				m_missed = true;
				base.ActorPlayer.CurrentExhaust += 1f;
				if (base.Actor.Opponent.IsFarAway)
				{
					base.ActorPlayer.Play("punch_miss_far", IsLeft);
					AIAnimator aiAnimator = base.Actor.aiAnimator;
					string name = "miss_alert";
					Vector2? position = base.Actor.transform.position.XY() + new Vector2(0.0625f, 4.25f);
					aiAnimator.PlayVfx(name, null, null, position);
				}
				else
				{
					base.ActorPlayer.Play("punch_miss", IsLeft);
					Vector2 vector = new Vector2((!IsLeft) ? 0.3125f : (-0.1875f), 4.375f);
					if (base.ActorPlayer.PlayerID == 4)
					{
						vector.x = 0.0625f;
					}
					else if (base.ActorPlayer.PlayerID == 5)
					{
						vector.y += 0.3125f;
					}
					AIAnimator aiAnimator2 = base.Actor.Opponent.aiAnimator;
					string name = "block_poof";
					Vector2? position = base.Actor.transform.position.XY() + vector;
					aiAnimator2.PlayVfx(name, null, null, position);
					GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Quick, Vibration.Strength.Light);
					if (base.ActorPlayer.IsSlinger)
					{
						base.ActorPlayer.aiAnimator.PlayVfx((!IsLeft) ? "shoot_right_miss" : "shoot_left_miss");
					}
				}
			}
			else
			{
				base.ActorPlayer.CurrentExhaust = 0f;
				if (base.Actor.Opponent.state is PunchoutAIActor.ThrowAmmoState)
				{
					m_missed = true;
					base.ActorPlayer.Play("punch_miss_far", IsLeft);
					base.Actor.Opponent.aiAnimator.PlayVfx("normal_hit");
				}
				if (base.ActorPlayer.IsSlinger)
				{
					base.ActorPlayer.aiAnimator.PlayVfx((!IsLeft) ? "shoot_right" : "shoot_left");
				}
			}
			return flag;
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if ((!m_missed && currentFrame == 2) || (m_missed && currentFrame == 1))
			{
				base.Actor.MoveCamera(new Vector2(0f, 0f), 0.12f);
			}
		}

		public override void Stop()
		{
			base.Stop();
			base.Actor.MoveCamera(new Vector2(0f, 0f), 0.08f);
		}
	}

	public class PlayerSuperState : BasicAttackState
	{
		private int m_starsUsed;

		private bool m_isFinal;

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
				return 6;
			}
		}

		public override float Damage
		{
			get
			{
				return 15 * m_starsUsed;
			}
		}

		public PlayerSuperState(int starsUsed)
			: base(false)
		{
			m_starsUsed = starsUsed;
		}

		public override void Start()
		{
			base.Start();
			float currentExhaust = base.ActorPlayer.CurrentExhaust;
			PunchoutAIActor punchoutAIActor = base.Actor.Opponent as PunchoutAIActor;
			if (punchoutAIActor.Phase == 2 && CanHitOpponent(punchoutAIActor.state) && (base.Actor.Opponent.Health <= Damage || punchoutAIActor.ShouldInstantKO(m_starsUsed)))
			{
				if (AnimName != null)
				{
					base.Actor.Play("super_final", IsLeft);
				}
				m_isFinal = true;
			}
			base.ActorPlayer.CurrentExhaust = currentExhaust;
			base.Actor.Opponent.spriteAnimator.Pause();
			base.Actor.Opponent.aiAnimator.ChildAnimator.spriteAnimator.Pause();
			base.Actor.Opponent.aiAnimator.ChildAnimator.ChildAnimator.spriteAnimator.Pause();
			base.Actor.MoveCamera(new Vector2(0f, 0f - base.ActorPlayer.SuperBackCameraSway), 0.5f);
		}

		public override bool CanHitOpponent(State state)
		{
			if (m_isFinal)
			{
				return true;
			}
			bool flag = !base.Actor.Opponent.IsFarAway;
			if (!flag)
			{
				base.ActorPlayer.CurrentExhaust += 1f;
			}
			else
			{
				base.ActorPlayer.CurrentExhaust = 0f;
			}
			return flag;
		}

		public override void OnFrame(int currentFrame)
		{
			if (currentFrame == DamageFrame)
			{
				base.Actor.Opponent.spriteAnimator.Resume();
				base.Actor.Opponent.aiAnimator.ChildAnimator.spriteAnimator.Resume();
				base.Actor.Opponent.aiAnimator.ChildAnimator.ChildAnimator.spriteAnimator.Resume();
				if (CanHitOpponent(base.Actor.Opponent.state))
				{
					base.Actor.Opponent.Hit(IsLeft, Damage, m_starsUsed);
				}
			}
			switch (currentFrame)
			{
			case 6:
				base.Actor.MoveCamera(new Vector2(0f, base.ActorPlayer.SuperForwardCameraSway), 0.08f);
				break;
			case 7:
				base.Actor.MoveCamera(new Vector2(0f, 0f), 0.5f);
				break;
			}
		}

		public override void Stop()
		{
			base.Stop();
			base.Actor.MoveCamera(new Vector2(0f, 0f), 0.16f);
			if (base.Actor.Opponent.spriteAnimator.Paused)
			{
				base.Actor.Opponent.spriteAnimator.Resume();
				base.Actor.Opponent.aiAnimator.ChildAnimator.spriteAnimator.Resume();
				base.Actor.Opponent.aiAnimator.ChildAnimator.ChildAnimator.spriteAnimator.Resume();
			}
		}
	}

	public class ExhaustState : State
	{
		private int m_cycles;

		private bool m_usesExhaustTime;

		private float? m_overrideExhaustTime;

		public override string AnimName
		{
			get
			{
				return "exhaust";
			}
		}

		public int ExhaustCycles
		{
			get
			{
				return 3;
			}
		}

		public ExhaustState(float? overrideExhaustTime = null)
		{
			m_overrideExhaustTime = overrideExhaustTime;
		}

		public override void Start()
		{
			base.Start();
			float? overrideExhaustTime = m_overrideExhaustTime;
			if (overrideExhaustTime.HasValue)
			{
				base.Actor.aiAnimator.PlayForDurationOrUntilFinished(AnimName, m_overrideExhaustTime.Value);
				m_usesExhaustTime = true;
			}
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (currentFrame == base.Actor.spriteAnimator.CurrentClip.frames.Length - 1)
			{
				m_cycles++;
				if (!m_usesExhaustTime && m_cycles >= ExhaustCycles)
				{
					base.Actor.aiAnimator.EndAnimationIf(AnimName);
					base.IsDone = true;
				}
			}
		}

		public override void Stop()
		{
			base.Stop();
			base.ActorPlayer.CurrentExhaust = 0f;
		}
	}

	public class DeathState : State
	{
		private float m_timer;

		public DeathState(bool isLeft)
			: base(isLeft)
		{
		}

		public override void Start()
		{
			base.Start();
			base.Actor.aiAnimator.FacingDirection = (IsLeft ? 180 : 0);
			base.ActorPlayer.VfxIsAboveCharacter = true;
			base.Actor.aiAnimator.PlayUntilCancelled("die");
		}

		public override void Update()
		{
			if (m_timer < 3f)
			{
				m_timer += Time.unscaledDeltaTime;
				if (m_timer > 2f)
				{
					BraveTime.SetTimeScaleMultiplier(Mathf.Lerp(0.25f, 1f, m_timer - 2f), base.Actor.gameObject);
				}
				if (m_timer >= 3f)
				{
					BraveTime.ClearMultiplier(base.Actor.gameObject);
					UnityEngine.Object.FindObjectOfType<PunchoutController>().DoLoseFade(false);
				}
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			return false;
		}
	}

	public class WinState : State
	{
		public override void Start()
		{
			base.Start();
			base.Actor.aiAnimator.PlayUntilCancelled("win");
			base.ActorPlayer.CoopAnimator.PlayUntilCancelled("win");
		}

		public override bool CanBeHit(bool isLeft)
		{
			return false;
		}
	}

	private enum Action
	{
		DodgeLeft,
		DodgeRight,
		Block,
		Duck,
		PunchLeft,
		PunchRight,
		Super
	}

	public dfSprite PlayerUiSprite;

	public AIAnimator CoopAnimator;

	public dfSprite starRemovedAnimationPrefab;

	public Vector3 starRemovedOffset;

	[Header("Constants")]
	public float InputBufferTime = 0.1f;

	public float MaxExhaust = 2.9f;

	public float ExhauseRecoveryRate = 0.2f;

	public float BlockStickyFriction = 0.1f;

	public float DuckCameraSway = 0.4f;

	public float DodgeCameraSway = 0.7f;

	public float PunchCameraSway = 0.4f;

	public float SuperBackCameraSway = 0.4f;

	public float SuperForwardCameraSway = 0.4f;

	[Header("Visuals")]
	public Texture2D CosmicTex;

	private Action[] m_actions;

	private float[] m_inputLastPressed;

	private int m_playerId;

	private static readonly string[] PlayerNames = new string[7] { "convict", "hunter", "marine", "pilot", "bullet", "robot", "slinger" };

	private static readonly string[] PlayerUiNames = new string[7] { "punch_player_health_convict_00", "punch_player_health_hunter_00", "punch_player_health_marine_00", "punch_player_health_pilot_00", "punch_player_health_bullet_00", "punch_player_health_robot_00", "punch_player_health_slinger_00" };

	public float CurrentExhaust { get; set; }

	public int PlayerID
	{
		get
		{
			return m_playerId;
		}
	}

	public override bool IsDead
	{
		get
		{
			return base.state is DeathState;
		}
	}

	public bool IsSlinger
	{
		get
		{
			return m_playerId == 6;
		}
	}

	public bool IsEevee { get; private set; }

	public bool VfxIsAboveCharacter
	{
		set
		{
			tk2dBaseSprite tk2dBaseSprite2 = base.aiAnimator.ChildAnimator.ChildAnimator.sprite;
			tk2dBaseSprite2.HeightOffGround = ((!value) ? (-0.05f) : 0.05f);
			tk2dBaseSprite2.UpdateZDepth();
		}
	}

	public override void Start()
	{
		base.Start();
		m_actions = (Action[])Enum.GetValues(typeof(Action));
		m_inputLastPressed = new float[m_actions.Length];
		for (int i = 0; i < m_actions.Length; i++)
		{
			m_inputLastPressed[i] = 100f;
		}
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(HandleAnimationCompletedSwap));
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		UpdateInput();
		CurrentExhaust = Mathf.Max(0f, CurrentExhaust - ExhauseRecoveryRate * BraveTime.DeltaTime);
		if (base.state != null)
		{
			base.state.Update();
			if (base.state.IsDone)
			{
				base.state = null;
			}
		}
		UpdateState();
	}

	public void UpdateState()
	{
		if (base.state == null && CurrentExhaust >= MaxExhaust)
		{
			base.state = new ExhaustState();
			return;
		}
		if (base.state == null || base.state is BlockState)
		{
			if (WasPressed(Action.PunchLeft))
			{
				base.state = new PlayerPunchState(true);
				return;
			}
			if (WasPressed(Action.PunchRight))
			{
				base.state = new PlayerPunchState(false);
				return;
			}
			if (WasPressed(Action.Super) && base.Stars > 0)
			{
				base.state = new PlayerSuperState(base.Stars);
				base.Stars = 0;
				return;
			}
		}
		if (base.state is DuckState && base.CurrentFrame >= 6)
		{
			if (WasPressed(Action.PunchLeft))
			{
				base.state = new PlayerPunchState(true);
				return;
			}
			if (WasPressed(Action.PunchRight))
			{
				base.state = new PlayerPunchState(false);
				return;
			}
		}
		if (base.state == null)
		{
			if (WasPressed(Action.DodgeLeft))
			{
				base.state = new PlayerDodgeState(true);
			}
			else if (WasPressed(Action.DodgeRight))
			{
				base.state = new PlayerDodgeState(false);
			}
			else if (WasPressed(Action.Block))
			{
				base.state = new PlayerBlockState();
			}
			else if (WasPressed(Action.Duck))
			{
				base.state = new PlayerDuckState();
			}
		}
	}

	public override void Hit(bool isLeft, float damage, int starsUsed = 0, bool skipProcessing = false)
	{
		if (!(base.state is DeathState))
		{
			if (base.Stars > 0 && damage >= 4f)
			{
				RemoveStars();
			}
			bool preventDamage = false;
			if (base.state != null)
			{
				base.state.OnHit(ref preventDamage, isLeft, starsUsed);
			}
			AkSoundEngine.PostEvent("Play_CHR_general_hurt_01", base.gameObject);
			if (!CoopAnimator.IsPlaying("alarm"))
			{
				CoopAnimator.PlayUntilFinished("alarm");
			}
			if (Health - damage <= 0f)
			{
				Health = 0f;
				UpdateUI();
				BraveTime.RegisterTimeScaleMultiplier(0.25f, base.gameObject);
				FlashDamage((m_playerId != 5) ? 0.66f : 0.25f);
				base.aiAnimator.PlayVfx("death");
				base.state = new DeathState(isLeft);
			}
			else
			{
				base.state = new HitState(isLeft);
				base.aiAnimator.PlayVfx("normal_hit");
				FlashDamage();
				Health -= damage;
				GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Normal, Vibration.Strength.Hard);
				UpdateUI();
				CurrentExhaust = 0f;
			}
		}
	}

	public void SwapPlayer(int? newPlayerIndex = null, bool keepEevee = false)
	{
		if (!newPlayerIndex.HasValue)
		{
			newPlayerIndex = ((!IsEevee || keepEevee) ? new int?((m_playerId + 1) % (PlayerNames.Length + 1)) : new int?(0));
		}
		if (!keepEevee)
		{
			bool flag = newPlayerIndex.Value == 7;
			if (flag && !IsEevee)
			{
				IsEevee = true;
				base.sprite.usesOverrideMaterial = true;
				base.sprite.renderer.material.shader = Shader.Find("Brave/PlayerShaderEevee");
				base.sprite.renderer.sharedMaterial.SetTexture("_EeveeTex", CosmicTex);
				base.sprite.renderer.material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
				base.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
			}
			else if (!flag && IsEevee)
			{
				IsEevee = false;
				base.sprite.usesOverrideMaterial = false;
			}
		}
		if (IsEevee)
		{
			newPlayerIndex = UnityEngine.Random.Range(0, PlayerNames.Length);
		}
		string oldName = PlayerNames[m_playerId];
		string newName = PlayerNames[newPlayerIndex.Value];
		m_playerId = newPlayerIndex.Value;
		SwapAnim(base.aiAnimator.IdleAnimation, oldName, newName);
		SwapAnim(base.aiAnimator.HitAnimation, oldName, newName);
		for (int i = 0; i < base.aiAnimator.OtherAnimations.Count; i++)
		{
			SwapAnim(base.aiAnimator.OtherAnimations[i].anim, oldName, newName);
		}
		UpdateUI();
		List<AIAnimator.NamedDirectionalAnimation> otherAnimations = base.aiAnimator.ChildAnimator.OtherAnimations;
		otherAnimations[0].anim.Type = DirectionalAnimation.DirectionType.None;
		otherAnimations[1].anim.Type = DirectionalAnimation.DirectionType.None;
		otherAnimations[2].anim.Type = DirectionalAnimation.DirectionType.None;
		if (m_playerId == 4)
		{
			otherAnimations[0].anim.Type = DirectionalAnimation.DirectionType.Single;
			otherAnimations[0].anim.Prefix = "bullet_super_vfx";
			otherAnimations[1].anim.Type = DirectionalAnimation.DirectionType.Single;
			otherAnimations[1].anim.Prefix = "bullet_super_final_vfx";
		}
		else if (m_playerId == 5)
		{
			otherAnimations[0].anim.Type = DirectionalAnimation.DirectionType.Single;
			otherAnimations[0].anim.Prefix = "robot_super_vfx";
			otherAnimations[1].anim.Type = DirectionalAnimation.DirectionType.Single;
			otherAnimations[1].anim.Prefix = "robot_super_final_vfx";
			otherAnimations[2].anim.Type = DirectionalAnimation.DirectionType.Single;
			otherAnimations[2].anim.Prefix = "robot_knockout_vfx";
		}
		else if (m_playerId == 6)
		{
			otherAnimations[0].anim.Type = DirectionalAnimation.DirectionType.Single;
			otherAnimations[0].anim.Prefix = "slinger_super_vfx";
			otherAnimations[1].anim.Type = DirectionalAnimation.DirectionType.Single;
			otherAnimations[1].anim.Prefix = "slinger_super_final_vfx";
		}
	}

	private void SwapAnim(DirectionalAnimation directionalAnim, string oldName, string newName)
	{
		directionalAnim.Prefix = directionalAnim.Prefix.Replace(oldName, newName);
		for (int i = 0; i < directionalAnim.AnimNames.Length; i++)
		{
			directionalAnim.AnimNames[i] = directionalAnim.AnimNames[i].Replace(oldName, newName);
		}
	}

	public void Win()
	{
		base.state = new WinState();
		UnityEngine.Object.FindObjectOfType<PunchoutController>().DoWinFade(false);
	}

	public void AddStar()
	{
		base.Stars = Mathf.Min(base.Stars + 1, 3);
		AIAnimator childAnimator = base.aiAnimator.ChildAnimator.ChildAnimator;
		VfxIsAboveCharacter = true;
		if (base.Stars == 3)
		{
			childAnimator.PlayUntilFinished("get_star_three");
		}
		else if (base.Stars == 2)
		{
			childAnimator.PlayUntilFinished("get_star_two");
		}
		else
		{
			childAnimator.PlayUntilFinished("get_star_one");
		}
	}

	public void RemoveStars()
	{
		for (int i = 0; i < base.Stars; i++)
		{
			dfSprite dfSprite2 = StarsUI[i];
			GameObject gameObject = UnityEngine.Object.Instantiate(starRemovedAnimationPrefab.gameObject);
			gameObject.transform.parent = dfSprite2.transform.parent;
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.layer = dfSprite2.gameObject.layer;
			dfSprite component = gameObject.GetComponent<dfSprite>();
			component.BringToFront();
			dfSprite2.Parent.AddControl(component);
			dfSprite2.Parent.BringToFront();
			component.ZOrder = dfSprite2.ZOrder - 1;
			component.RelativePosition = dfSprite2.RelativePosition + starRemovedOffset;
		}
		base.Stars = 0;
		AIAnimator childAnimator = base.aiAnimator.ChildAnimator.ChildAnimator;
		VfxIsAboveCharacter = true;
		childAnimator.PlayUntilFinished("lose_stars");
	}

	public void UpdateUI()
	{
		string text = PlayerUiNames[m_playerId];
		HealthBarUI.SpriteName = "punch_health_bar_001";
		if (Health > 66f)
		{
			PlayerUiSprite.SpriteName = text + "1";
		}
		else if (Health > 33f)
		{
			PlayerUiSprite.SpriteName = text + "2";
		}
		else
		{
			PlayerUiSprite.SpriteName = text + "3";
		}
		if (IsEevee && PlayerUiSprite.OverrideMaterial == null)
		{
			Material material = UnityEngine.Object.Instantiate(PlayerUiSprite.Atlas.Material);
			material.shader = Shader.Find("Brave/Internal/GlitchEevee");
			material.SetTexture("_EeveeTex", CosmicTex);
			material.SetFloat("_WaveIntensity", 0.1f);
			material.SetFloat("_ColorIntensity", 0.015f);
			PlayerUiSprite.OverrideMaterial = material;
		}
		else if (!IsEevee && PlayerUiSprite.OverrideMaterial != null)
		{
			PlayerUiSprite.OverrideMaterial = null;
		}
	}

	public void Exhaust(float? time = null)
	{
		base.state = new ExhaustState(time);
	}

	private void HandleAnimationCompletedSwap(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2)
	{
		if (IsEevee)
		{
			SwapPlayer(UnityEngine.Random.Range(0, PlayerNames.Length), true);
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.shader = Shader.Find("Brave/PlayerShaderEevee");
			base.sprite.renderer.sharedMaterial.SetTexture("_EeveeTex", CosmicTex);
			base.sprite.renderer.material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
			base.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
		}
	}

	private void UpdateInput()
	{
		if (m_inputLastPressed == null)
		{
			return;
		}
		IEnumerator enumerator = Enum.GetValues(typeof(Action)).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Action action = (Action)enumerator.Current;
				if (GameManager.HasInstance && !GameManager.Instance.IsPaused && WasPressedRaw(action))
				{
					m_inputLastPressed[(int)action] = 0f;
				}
				else
				{
					m_inputLastPressed[(int)action] += BraveTime.DeltaTime;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
	}

	private bool WasPressed(Action action)
	{
		bool flag = m_inputLastPressed[(int)action] < InputBufferTime;
		if (flag)
		{
			m_inputLastPressed[(int)action] = 100f;
			if (PunchoutController.InTutorial)
			{
				PunchoutController.InputWasPressed((int)action);
			}
		}
		return flag;
	}

	private bool WasPressedRaw(Action action)
	{
		BraveInput braveInput = ((!BraveInput.HasInstanceForPlayer(0)) ? null : BraveInput.GetInstanceForPlayer(0));
		if ((bool)braveInput)
		{
			switch (action)
			{
			case Action.DodgeLeft:
				return braveInput.ActiveActions.PunchoutDodgeLeft.WasPressed;
			case Action.DodgeRight:
				return braveInput.ActiveActions.PunchoutDodgeRight.WasPressed;
			case Action.Block:
				return braveInput.ActiveActions.PunchoutBlock.WasPressed;
			case Action.Duck:
				return braveInput.ActiveActions.PunchoutDuck.WasPressed;
			case Action.PunchLeft:
				return braveInput.ActiveActions.PunchoutPunchLeft.WasPressed;
			case Action.PunchRight:
				return braveInput.ActiveActions.PunchoutPunchRight.WasPressed;
			case Action.Super:
				return braveInput.ActiveActions.PunchoutSuper.WasPressed;
			}
		}
		return false;
	}
}
