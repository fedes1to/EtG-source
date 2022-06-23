using System;
using System.Collections.Generic;
using FullInspector;
using Pathfinding;
using UnityEngine;

[InspectorDropdownName("Bosses/MineFlayer/ShellGameBehavior")]
public class MineFlayerShellGameBehavior : BasicAttackBehavior
{
	public enum ShadowSupport
	{
		None,
		Fade,
		Animate
	}

	private enum ShellGameState
	{
		None,
		Disappear,
		Gone,
		Reappear
	}

	public float MaxGoneTime = 5f;

	[InspectorCategory("Attack")]
	public BulletScriptSelector disappearBulletScript;

	[InspectorCategory("Attack")]
	public BulletScriptSelector reappearInBulletScript;

	[InspectorCategory("Visuals")]
	public string disappearAnim = "teleport_out";

	[InspectorCategory("Visuals")]
	public string reappearAnim = "teleport_in";

	[InspectorCategory("Visuals")]
	public bool requiresTransparency;

	[InspectorCategory("Visuals")]
	public ShadowSupport shadowSupport;

	[InspectorCategory("Visuals")]
	[InspectorShowIf("ShowShadowAnimationNames")]
	public string shadowDisappearAnim;

	[InspectorShowIf("ShowShadowAnimationNames")]
	[InspectorCategory("Visuals")]
	public string shadowReappearAnim;

	public int enemiesToSpawn;

	[EnemyIdentifier]
	public string enemyGuid;

	private tk2dBaseSprite m_shadowSprite;

	private Shader m_cachedShader;

	private float m_timer;

	private bool m_shouldFire;

	private List<AIActor> m_spawnedActors = new List<AIActor>();

	private AIActor m_myBell;

	private bool m_correctBellHit;

	private Vector2? m_reappearPosition;

	private ShellGameState m_state;

	private ShellGameState State
	{
		get
		{
			return m_state;
		}
		set
		{
			EndState(m_state);
			m_state = value;
			BeginState(m_state);
		}
	}

	private bool ShowShadowAnimationNames()
	{
		return shadowSupport == ShadowSupport.Animate;
	}

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (m_shadowSprite == null)
		{
			m_shadowSprite = m_aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		State = ShellGameState.Disappear;
		m_aiActor.healthHaver.minimumHealth = 1f;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == ShellGameState.Disappear)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f - m_aiAnimator.CurrentClipProgress);
			}
			if (!m_aiAnimator.IsPlaying(disappearAnim))
			{
				State = ((!(MaxGoneTime > 0f)) ? ShellGameState.Reappear : ShellGameState.Gone);
			}
		}
		else if (State == ShellGameState.Gone)
		{
			if (m_timer <= 0f)
			{
				State = ShellGameState.Reappear;
			}
		}
		else if (State == ShellGameState.Reappear)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(m_aiAnimator.CurrentClipProgress);
			}
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "MeduziUnderwaterBehavior");
			}
			Vector2? reappearPosition = m_reappearPosition;
			if (reappearPosition.HasValue)
			{
				m_aiActor.specRigidbody.CollideWithTileMap = false;
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(m_aiActor.specRigidbody);
				Vector2 vector = m_reappearPosition.Value - m_aiActor.specRigidbody.UnitBottomLeft;
				m_aiActor.BehaviorOverridesVelocity = true;
				m_aiActor.BehaviorVelocity = vector / (m_aiAnimator.CurrentClipLength * (1f - m_aiAnimator.CurrentClipProgress));
			}
			if (!m_aiAnimator.IsPlaying(reappearAnim))
			{
				State = ShellGameState.None;
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_aiActor.healthHaver.minimumHealth = 0f;
		if (requiresTransparency && (bool)m_cachedShader)
		{
			m_aiActor.sprite.usesOverrideMaterial = false;
			m_aiActor.renderer.material.shader = m_cachedShader;
			m_cachedShader = null;
		}
		m_aiActor.sprite.renderer.enabled = true;
		if ((bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(false, "teleport");
		}
		m_aiActor.specRigidbody.CollideWithOthers = true;
		m_aiActor.IsGone = false;
		if ((bool)m_aiShooter)
		{
			m_aiShooter.ToggleGunAndHandRenderers(true, "MeduziUnderwaterBehavior");
		}
		m_aiAnimator.EndAnimationIf(disappearAnim);
		m_aiAnimator.EndAnimationIf(reappearAnim);
		if (shadowSupport == ShadowSupport.Fade)
		{
			m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
		}
		else if (shadowSupport == ShadowSupport.Animate)
		{
			tk2dSpriteAnimationClip clipByName = m_shadowSprite.spriteAnimator.GetClipByName(shadowReappearAnim);
			m_shadowSprite.spriteAnimator.Play(clipByName, clipByName.frames.Length - 1, clipByName.fps);
		}
		m_spawnedActors.Clear();
		m_correctBellHit = false;
		SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
		Vector2? reappearPosition = m_reappearPosition;
		if (reappearPosition.HasValue)
		{
			m_aiActor.specRigidbody.CollideWithTileMap = true;
			m_aiActor.transform.position += (Vector3)(m_reappearPosition.Value - m_aiActor.specRigidbody.UnitBottomLeft);
			m_aiActor.specRigidbody.Reinitialize();
			m_aiActor.BehaviorOverridesVelocity = false;
			m_reappearPosition = null;
		}
		m_state = ShellGameState.None;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (m_shouldFire && clip.GetFrame(frame).eventInfo == "fire")
		{
			if (State == ShellGameState.Reappear)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, reappearInBulletScript);
			}
			else if (State == ShellGameState.Disappear)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, disappearBulletScript);
			}
			m_shouldFire = false;
		}
		else if (State == ShellGameState.Disappear && clip.GetFrame(frame).eventInfo == "collider_off")
		{
			m_aiActor.specRigidbody.CollideWithOthers = false;
			m_aiActor.IsGone = true;
		}
		else if (State == ShellGameState.Reappear && clip.GetFrame(frame).eventInfo == "collider_on")
		{
			m_aiActor.specRigidbody.CollideWithOthers = true;
			m_aiActor.IsGone = false;
		}
	}

	private void OnMyBellDeath(Vector2 obj)
	{
		if (State != ShellGameState.Reappear)
		{
			m_correctBellHit = true;
			State = ShellGameState.Reappear;
		}
	}

	private void BeginState(ShellGameState state)
	{
		switch (state)
		{
		case ShellGameState.Disappear:
			if (disappearBulletScript != null && !disappearBulletScript.IsNull)
			{
				m_shouldFire = true;
			}
			if (requiresTransparency)
			{
				m_cachedShader = m_aiActor.renderer.material.shader;
				m_aiActor.sprite.usesOverrideMaterial = true;
				m_aiActor.renderer.material.shader = ShaderCache.Acquire("Brave/LitBlendUber");
			}
			m_aiAnimator.PlayUntilCancelled(disappearAnim, true);
			if (shadowSupport == ShadowSupport.Animate)
			{
				m_shadowSprite.spriteAnimator.PlayAndForceTime(shadowDisappearAnim, m_aiAnimator.CurrentClipLength);
			}
			m_aiActor.ClearPath();
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "MeduziUnderwaterBehavior");
			}
			break;
		case ShellGameState.Gone:
		{
			m_timer = MaxGoneTime;
			m_aiActor.specRigidbody.CollideWithOthers = false;
			m_aiActor.IsGone = true;
			m_aiActor.sprite.renderer.enabled = false;
			Vector2 position = m_aiActor.specRigidbody.UnitCenter + Vector2.right;
			for (int j = 0; j < enemiesToSpawn; j++)
			{
				AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(enemyGuid);
				AIActor item = AIActor.Spawn(orLoadByGuid, position, m_aiActor.ParentRoom, true);
				m_spawnedActors.Add(item);
			}
			m_myBell = BraveUtility.RandomElement(m_spawnedActors);
			m_myBell.healthHaver.OnPreDeath += OnMyBellDeath;
			m_myBell.OnCorpseVFX.type = VFXPoolType.None;
			m_myBell.healthHaver.spawnBulletScript = false;
			break;
		}
		case ShellGameState.Reappear:
		{
			if ((bool)m_myBell)
			{
				m_aiActor.specRigidbody.AlignWithRigidbodyBottomCenter(m_myBell.specRigidbody, new IntVector2(-6, -2));
				if (PhysicsEngine.Instance.OverlapCast(m_aiActor.specRigidbody, null, true, true, null, null, false, null, null))
				{
					DoReposition();
				}
				else
				{
					m_reappearPosition = null;
				}
			}
			for (int i = 0; i < m_spawnedActors.Count; i++)
			{
				AIActor aIActor = m_spawnedActors[i];
				if ((bool)aIActor && (bool)aIActor.healthHaver && aIActor.healthHaver.IsAlive)
				{
					if (m_correctBellHit)
					{
						aIActor.healthHaver.spawnBulletScript = false;
					}
					aIActor.healthHaver.ApplyDamage(1E+10f, Vector2.zero, "Bell Death", CoreDamageTypes.None, DamageCategory.Unstoppable);
				}
			}
			if (reappearInBulletScript != null && !reappearInBulletScript.IsNull)
			{
				m_shouldFire = true;
			}
			m_aiAnimator.PlayUntilFinished(reappearAnim, true);
			if (shadowSupport == ShadowSupport.Animate)
			{
				m_shadowSprite.spriteAnimator.PlayAndForceTime(shadowReappearAnim, m_aiAnimator.CurrentClipLength);
			}
			m_shadowSprite.renderer.enabled = true;
			m_aiActor.sprite.renderer.enabled = true;
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "MeduziUnderwaterBehavior");
			}
			SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
			break;
		}
		}
	}

	private void EndState(ShellGameState state)
	{
		switch (state)
		{
		case ShellGameState.Disappear:
			m_shadowSprite.renderer.enabled = false;
			SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, false);
			if (disappearBulletScript != null && !disappearBulletScript.IsNull && m_shouldFire)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, disappearBulletScript);
				m_shouldFire = false;
			}
			break;
		case ShellGameState.Gone:
			m_aiActor.BehaviorOverridesVelocity = false;
			break;
		case ShellGameState.Reappear:
		{
			if (requiresTransparency)
			{
				m_aiActor.sprite.usesOverrideMaterial = false;
				m_aiActor.renderer.material.shader = m_cachedShader;
			}
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
			}
			m_aiActor.specRigidbody.CollideWithOthers = true;
			m_aiActor.IsGone = false;
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(true, "MeduziUnderwaterBehavior");
			}
			if (reappearInBulletScript != null && !reappearInBulletScript.IsNull && m_shouldFire)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, reappearInBulletScript);
				m_shouldFire = false;
			}
			Vector2? reappearPosition = m_reappearPosition;
			if (reappearPosition.HasValue)
			{
				m_aiActor.specRigidbody.CollideWithTileMap = true;
				m_aiActor.transform.position += (Vector3)(m_reappearPosition.Value - m_aiActor.specRigidbody.UnitBottomLeft);
				m_aiActor.specRigidbody.Reinitialize();
				m_aiActor.BehaviorOverridesVelocity = false;
				m_reappearPosition = null;
			}
			break;
		}
		}
	}

	private void DoReposition()
	{
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int i = 0; i < m_aiActor.Clearance.x; i++)
			{
				for (int j = 0; j < m_aiActor.Clearance.y; j++)
				{
					if (GameManager.Instance.Dungeon.data.isWall(c.x + i, c.y + j))
					{
						return false;
					}
					if (GameManager.Instance.Dungeon.data.isTopWall(c.x + i, c.y + j))
					{
						return false;
					}
				}
			}
			return true;
		};
		IntVector2? nearestAvailableCell = m_aiActor.ParentRoom.GetNearestAvailableCell(m_aiActor.specRigidbody.UnitBottomLeft, m_aiActor.Clearance, m_aiActor.PathableTiles, false, cellValidator);
		if (nearestAvailableCell.HasValue)
		{
			Vector2 value = Pathfinder.GetClearanceOffset(nearestAvailableCell.Value, m_aiActor.Clearance).WithY(nearestAvailableCell.Value.y);
			value -= new Vector2(m_aiActor.specRigidbody.UnitDimensions.x / 2f, 0f);
			m_reappearPosition = value;
		}
		else
		{
			m_reappearPosition = null;
		}
	}
}
