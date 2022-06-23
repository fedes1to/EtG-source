using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ForgeHammerController : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	private enum HammerState
	{
		InitialDelay,
		PreSwing,
		Swing,
		Grounded,
		UpSwing,
		Gone
	}

	[DwarfConfigurable]
	public bool TracksPlayer = true;

	[DwarfConfigurable]
	public bool DeactivateOnEnemiesCleared = true;

	[DwarfConfigurable]
	public bool ForceLeft;

	[DwarfConfigurable]
	public bool ForceRight;

	public float FlashDurationBeforeAttack = 0.5f;

	public float AdditionalTrackingTime = 0.25f;

	public float DamageToEnemies = 30f;

	public float KnockbackForcePlayers = 50f;

	public float KnockbackForceEnemies = 50f;

	[DwarfConfigurable]
	public float InitialDelay = 1f;

	[DwarfConfigurable]
	public float MinTimeBetweenAttacks = 2f;

	[DwarfConfigurable]
	public float MaxTimeBetweenAttacks = 4f;

	[DwarfConfigurable]
	public float MinTimeToRestOnGround = 1f;

	[DwarfConfigurable]
	public float MaxTimeToRestOnGround = 1f;

	public bool DoScreenShake;

	public ScreenShakeSettings ScreenShake;

	public string Hammer_Anim_In_Left;

	public string Hammer_Anim_Out_Left;

	public string Hammer_Anim_In_Right;

	public string Hammer_Anim_Out_Right;

	public tk2dSpriteAnimator HitEffectAnimator;

	public tk2dSpriteAnimator TargetAnimator;

	public tk2dSpriteAnimator ShadowAnimator;

	public bool DoGoopOnImpact;

	[ShowInInspectorIf("DoGoopOnImpact", false)]
	public GoopDefinition GoopToDo;

	[DwarfConfigurable]
	public bool DoesBulletsOnImpact;

	[ShowInInspectorIf("DoGoopOnImpact", false)]
	public BulletScriptSelector BulletScript;

	[ShowInInspectorIf("DoGoopOnImpact", false)]
	public Transform ShootPoint;

	private float m_localTimeScale = 1f;

	private HammerState m_state = HammerState.Gone;

	private float m_timer;

	private PlayerController m_targetPlayer;

	private Vector2 m_targetOffset;

	private string m_inAnim;

	private string m_outAnim;

	private float m_additionalTrackTimer;

	private RoomHandler m_room;

	private bool m_isActive;

	private BulletScriptSource m_bulletSource;

	private bool m_attackIsLeft;

	private float LocalDeltaTime
	{
		get
		{
			return BraveTime.DeltaTime * LocalTimeScale;
		}
	}

	public float LocalTimeScale
	{
		get
		{
			return m_localTimeScale;
		}
		set
		{
			base.spriteAnimator.OverrideTimeScale = value;
			m_localTimeScale = value;
		}
	}

	private HammerState State
	{
		get
		{
			return m_state;
		}
		set
		{
			if (value != m_state)
			{
				EndState(m_state);
				m_state = value;
				BeginState(m_state);
			}
		}
	}

	public void Start()
	{
		PhysicsEngine.Instance.OnPostRigidbodyMovement += OnPostRigidbodyMovement;
	}

	public void Update()
	{
		if (!m_isActive && State == HammerState.Gone)
		{
			return;
		}
		if (m_isActive && State == HammerState.Gone)
		{
			Vector2 unitBottomLeft = m_room.area.UnitBottomLeft;
			Vector2 unitTopRight = m_room.area.UnitTopRight;
			int num = 0;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				if ((bool)playerController && playerController.healthHaver.IsAlive)
				{
					Vector2 centerPosition = playerController.CenterPosition;
					if (BraveMathCollege.AABBContains(unitBottomLeft - Vector2.one, unitTopRight + Vector2.one, centerPosition))
					{
						num++;
					}
				}
			}
			if (num == 0)
			{
				Deactivate();
			}
		}
		m_timer = Mathf.Max(0f, m_timer - LocalDeltaTime);
		UpdateState(State);
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.AllForgeHammers.Remove(this);
		base.OnDestroy();
	}

	public void Activate()
	{
		if (m_isActive)
		{
			return;
		}
		if (DeactivateOnEnemiesCleared && !m_room.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
		{
			ForceStop();
			return;
		}
		m_isActive = true;
		if (State == HammerState.Gone)
		{
			State = HammerState.InitialDelay;
		}
	}

	public void Deactivate()
	{
		if (m_isActive)
		{
			if ((bool)base.encounterTrackable)
			{
				GameStatsManager.Instance.HandleEncounteredObject(base.encounterTrackable);
			}
			m_isActive = false;
		}
	}

	private void BeginState(HammerState state)
	{
		switch (state)
		{
		case HammerState.InitialDelay:
			TargetAnimator.renderer.enabled = false;
			HitEffectAnimator.renderer.enabled = false;
			base.sprite.renderer.enabled = false;
			m_timer = InitialDelay;
			break;
		case HammerState.PreSwing:
		{
			m_targetPlayer = GameManager.Instance.GetRandomActivePlayer();
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				List<PlayerController> list = new List<PlayerController>(2);
				Vector2 unitBottomLeft = m_room.area.UnitBottomLeft;
				Vector2 unitTopRight = m_room.area.UnitTopRight;
				for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
				{
					PlayerController playerController = GameManager.Instance.AllPlayers[i];
					if ((bool)playerController && playerController.healthHaver.IsAlive)
					{
						Vector2 centerPosition = playerController.CenterPosition;
						if (BraveMathCollege.AABBContains(unitBottomLeft - Vector2.one, unitTopRight + Vector2.one, centerPosition))
						{
							list.Add(playerController);
						}
					}
				}
				if (list.Count > 0)
				{
					m_targetPlayer = BraveUtility.RandomElement(list);
				}
			}
			IntVector2 intVector = m_targetPlayer.CenterPosition.ToIntVector2(VectorConversions.Floor);
			if (ForceLeft)
			{
				m_attackIsLeft = true;
			}
			else if (ForceRight)
			{
				m_attackIsLeft = false;
			}
			else
			{
				int j;
				for (j = 0; GameManager.Instance.Dungeon.data[intVector + IntVector2.Left * j].type != CellType.WALL; j++)
				{
				}
				int k;
				for (k = 0; GameManager.Instance.Dungeon.data[intVector + IntVector2.Right * k].type != CellType.WALL; k++)
				{
				}
				m_attackIsLeft = j < k;
			}
			m_inAnim = ((!m_attackIsLeft) ? Hammer_Anim_In_Right : Hammer_Anim_In_Left);
			m_outAnim = ((!m_attackIsLeft) ? Hammer_Anim_Out_Right : Hammer_Anim_Out_Left);
			TargetAnimator.StopAndResetFrame();
			TargetAnimator.renderer.enabled = TracksPlayer;
			TargetAnimator.PlayAndDisableRenderer((!m_attackIsLeft) ? "hammer_right_target" : "hammer_left_target");
			m_targetOffset = ((!m_attackIsLeft) ? new Vector2(4.625f, 1.9375f) : new Vector2(1.9375f, 1.9375f));
			m_timer = FlashDurationBeforeAttack;
			break;
		}
		case HammerState.Swing:
			base.sprite.renderer.enabled = true;
			base.spriteAnimator.Play(m_inAnim);
			ShadowAnimator.renderer.enabled = true;
			ShadowAnimator.Play((!m_attackIsLeft) ? "hammer_right_slam_shadow" : "hammer_left_slam_shadow");
			base.sprite.HeightOffGround = -2.5f;
			base.sprite.UpdateZDepth();
			m_additionalTrackTimer = AdditionalTrackingTime;
			break;
		case HammerState.Grounded:
		{
			if (DoScreenShake)
			{
				GameManager.Instance.MainCameraController.DoScreenShake(ScreenShake, base.specRigidbody.UnitCenter);
			}
			base.specRigidbody.enabled = true;
			base.specRigidbody.PixelColliders[0].ManualOffsetX = ((!m_attackIsLeft) ? 59 : 16);
			base.specRigidbody.PixelColliders[1].ManualOffsetX = ((!m_attackIsLeft) ? 59 : 16);
			base.specRigidbody.ForceRegenerate();
			base.specRigidbody.Reinitialize();
			Exploder.DoRadialMinorBreakableBreak(TargetAnimator.sprite.WorldCenter, 4f);
			HitEffectAnimator.renderer.enabled = true;
			HitEffectAnimator.PlayAndDisableRenderer((!m_attackIsLeft) ? "hammer_right_slam_vfx" : "hammer_left_slam_vfx");
			List<SpeculativeRigidbody> overlappingRigidbodies = PhysicsEngine.Instance.GetOverlappingRigidbodies(base.specRigidbody);
			for (int l = 0; l < overlappingRigidbodies.Count; l++)
			{
				if (!overlappingRigidbodies[l].gameActor)
				{
					continue;
				}
				Vector2 direction = overlappingRigidbodies[l].UnitCenter - base.specRigidbody.UnitCenter;
				if (overlappingRigidbodies[l].gameActor is PlayerController)
				{
					PlayerController playerController2 = overlappingRigidbodies[l].gameActor as PlayerController;
					if (overlappingRigidbodies[l].CollideWithOthers && (!playerController2.DodgeRollIsBlink || !playerController2.IsDodgeRolling))
					{
						if ((bool)overlappingRigidbodies[l].healthHaver)
						{
							overlappingRigidbodies[l].healthHaver.ApplyDamage(0.5f, direction, StringTableManager.GetEnemiesString("#FORGE_HAMMER"));
						}
						if ((bool)overlappingRigidbodies[l].knockbackDoer)
						{
							overlappingRigidbodies[l].knockbackDoer.ApplyKnockback(direction, KnockbackForcePlayers);
						}
					}
				}
				else
				{
					if ((bool)overlappingRigidbodies[l].healthHaver)
					{
						overlappingRigidbodies[l].healthHaver.ApplyDamage(DamageToEnemies, direction, StringTableManager.GetEnemiesString("#FORGE_HAMMER"));
					}
					if ((bool)overlappingRigidbodies[l].knockbackDoer)
					{
						overlappingRigidbodies[l].knockbackDoer.ApplyKnockback(direction, KnockbackForceEnemies);
					}
				}
			}
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
			if (DoGoopOnImpact)
			{
				DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(GoopToDo).AddGoopRect(base.specRigidbody.UnitCenter + new Vector2(-1f, -1.25f), base.specRigidbody.UnitCenter + new Vector2(1f, 0.75f));
			}
			if (DoesBulletsOnImpact && m_isActive)
			{
				ShootPoint.transform.position = base.specRigidbody.UnitCenter;
				CellData cell = ShootPoint.transform.position.GetCell();
				if (cell != null && cell.type != CellType.WALL)
				{
					if (!m_bulletSource)
					{
						m_bulletSource = ShootPoint.gameObject.GetOrAddComponent<BulletScriptSource>();
					}
					m_bulletSource.BulletManager = base.bulletBank;
					m_bulletSource.BulletScript = BulletScript;
					m_bulletSource.Initialize();
				}
			}
			m_timer = UnityEngine.Random.Range(MinTimeToRestOnGround, MaxTimeToRestOnGround);
			break;
		}
		case HammerState.UpSwing:
			base.spriteAnimator.Play(m_outAnim);
			ShadowAnimator.PlayAndDisableRenderer((!m_attackIsLeft) ? "hammer_right_out_shadow" : "hammer_left_out_shadow");
			break;
		case HammerState.Gone:
			base.sprite.renderer.enabled = false;
			m_timer = UnityEngine.Random.Range(MinTimeBetweenAttacks, MaxTimeBetweenAttacks);
			break;
		}
	}

	private void UpdateState(HammerState state)
	{
		switch (state)
		{
		case HammerState.InitialDelay:
			if (m_timer <= 0f)
			{
				State = HammerState.PreSwing;
			}
			break;
		case HammerState.PreSwing:
			if (m_timer <= 0f)
			{
				State = HammerState.Swing;
			}
			break;
		case HammerState.Swing:
			m_additionalTrackTimer -= LocalDeltaTime;
			if (!base.spriteAnimator.IsPlaying(m_inAnim))
			{
				State = HammerState.Grounded;
			}
			break;
		case HammerState.Grounded:
			if (m_timer <= 0f)
			{
				State = HammerState.UpSwing;
			}
			break;
		case HammerState.UpSwing:
			if (!base.spriteAnimator.IsPlaying(m_outAnim))
			{
				State = HammerState.Gone;
			}
			break;
		case HammerState.Gone:
			if (m_timer <= 0f)
			{
				State = HammerState.PreSwing;
			}
			break;
		}
	}

	private void EndState(HammerState state)
	{
		if (state == HammerState.Grounded)
		{
			base.specRigidbody.enabled = false;
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		StaticReferenceManager.AllForgeHammers.Add(this);
		m_room = room;
		if (room.visibility == RoomHandler.VisibilityStatus.CURRENT)
		{
			DoRealConfigure(true);
		}
		else
		{
			StartCoroutine(FrameDelayedConfigure());
		}
	}

	private IEnumerator FrameDelayedConfigure()
	{
		yield return null;
		DoRealConfigure(false);
	}

	private void DoRealConfigure(bool activateNow)
	{
		if (ForceLeft)
		{
			base.transform.position += new Vector3(-1f, -1f, 0f);
		}
		else if (ForceRight)
		{
			base.transform.position += new Vector3(-3.5625f, -1f, 0f);
		}
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		m_room.Entered += delegate
		{
			Activate();
		};
		if (activateNow)
		{
			Activate();
		}
		if (DeactivateOnEnemiesCleared)
		{
			RoomHandler room = m_room;
			room.OnEnemiesCleared = (Action)Delegate.Combine(room.OnEnemiesCleared, new Action(Deactivate));
		}
	}

	private void HandleAnimationEvent(tk2dSpriteAnimator sourceAnimator, tk2dSpriteAnimationClip sourceClip, int sourceFrame)
	{
		if (State == HammerState.Swing && sourceClip.frames[sourceFrame].eventInfo == "impact")
		{
			State = HammerState.Grounded;
		}
	}

	private void OnPostRigidbodyMovement()
	{
		if (TracksPlayer && (State == HammerState.PreSwing || (m_additionalTrackTimer > 0f && State == HammerState.Swing)))
		{
			UpdatePosition();
		}
	}

	private void UpdatePosition()
	{
		Vector2 unitBottomLeft = m_room.area.UnitBottomLeft;
		Vector2 unitTopRight = m_room.area.UnitTopRight;
		Vector2 centerPosition = m_targetPlayer.CenterPosition;
		centerPosition = BraveMathCollege.ClampToBounds(centerPosition, unitBottomLeft + Vector2.one, unitTopRight - Vector2.one);
		base.transform.position = (centerPosition - m_targetOffset).Quantize(0.0625f);
		TargetAnimator.sprite.UpdateZDepth();
		base.sprite.UpdateZDepth();
	}

	private void ForceStop()
	{
		if ((bool)TargetAnimator)
		{
			TargetAnimator.renderer.enabled = false;
		}
		if ((bool)HitEffectAnimator)
		{
			HitEffectAnimator.renderer.enabled = false;
		}
		if ((bool)base.sprite)
		{
			base.sprite.renderer.enabled = false;
		}
		if ((bool)ShadowAnimator)
		{
			ShadowAnimator.renderer.enabled = false;
		}
		base.specRigidbody.enabled = false;
		if ((bool)m_bulletSource)
		{
			m_bulletSource.ForceStop();
		}
		State = HammerState.Gone;
	}
}
