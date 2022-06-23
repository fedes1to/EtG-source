using System;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Meduzi/UnderwaterBehavior")]
public class MeduziUnderwaterBehavior : BasicAttackBehavior
{
	public enum ShadowSupport
	{
		None,
		Fade,
		Animate
	}

	public enum StartingDirection
	{
		Player,
		RandomAwayFromWalls
	}

	private enum UnderwaterState
	{
		None,
		Disappear,
		Gone,
		Reappear
	}

	public bool AttackableDuringAnimation;

	public bool AvoidWalls;

	public StartingDirection startingDirection;

	public float GoneTime = 1f;

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

	[InspectorCategory("Visuals")]
	[InspectorShowIf("ShowShadowAnimationNames")]
	public string shadowReappearAnim;

	public GameObject crawlSprite;

	public tk2dSpriteAnimator crawlAnimator;

	public float crawlSpeed = 8f;

	public float crawlTurnTime = 1f;

	private tk2dBaseSprite m_shadowSprite;

	private Shader m_cachedShader;

	private GoopDoer m_goopDoer;

	private float m_timer;

	private bool m_shouldFire;

	private float m_direction;

	private float m_angularVelocity;

	private UnderwaterState m_state;

	private UnderwaterState State
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
		if ((disappearBulletScript != null && !disappearBulletScript.IsNull) || (reappearInBulletScript != null && !reappearInBulletScript.IsNull))
		{
			tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		}
		crawlAnimator.sprite.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
		crawlAnimator.renderer.material.SetFloat("_ReflectionYOffset", 1000f);
		m_goopDoer = m_aiActor.GetComponent<GoopDoer>();
		SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
		specRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(specRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(RoomMovementRestrictor));
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
		State = UnderwaterState.Disappear;
		m_aiActor.healthHaver.minimumHealth = 1f;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == UnderwaterState.Disappear)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f - m_aiAnimator.CurrentClipProgress);
			}
			if (!m_aiAnimator.IsPlaying(disappearAnim))
			{
				State = ((!(GoneTime > 0f)) ? UnderwaterState.Reappear : UnderwaterState.Gone);
			}
		}
		else if (State == UnderwaterState.Gone)
		{
			float target = ((m_aiActor.BehaviorVelocity.magnitude != 0f) ? m_aiActor.BehaviorVelocity.ToAngle() : 0f);
			if ((bool)m_aiActor.TargetRigidbody)
			{
				target = (m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.Ground) - m_aiActor.specRigidbody.UnitCenter).ToAngle();
			}
			m_direction = Mathf.SmoothDampAngle(m_direction, target, ref m_angularVelocity, crawlTurnTime);
			crawlSprite.transform.rotation = Quaternion.Euler(0f, 0f, BraveMathCollege.QuantizeFloat(m_direction, 11.25f));
			m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_direction, crawlSpeed);
			if (m_timer <= 0f)
			{
				State = UnderwaterState.Reappear;
			}
		}
		else if (State == UnderwaterState.Reappear)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(m_aiAnimator.CurrentClipProgress);
			}
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "MeduziUnderwaterBehavior");
			}
			if (!m_aiAnimator.IsPlaying(reappearAnim))
			{
				State = UnderwaterState.None;
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
		crawlSprite.SetActive(false);
		m_aiActor.BehaviorOverridesVelocity = false;
		SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
		m_goopDoer.enabled = false;
		m_state = UnderwaterState.None;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override void OnActorPreDeath()
	{
		SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
		specRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Remove(specRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(RoomMovementRestrictor));
		base.OnActorPreDeath();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (m_shouldFire && clip.GetFrame(frame).eventInfo == "fire")
		{
			if (State == UnderwaterState.Reappear)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, reappearInBulletScript);
			}
			else if (State == UnderwaterState.Disappear)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, disappearBulletScript);
			}
			m_shouldFire = false;
		}
	}

	private void RoomMovementRestrictor(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (!validLocation)
		{
			return;
		}
		IntVector2 intVector = pixelOffset - prevPixelOffset;
		CellArea area = m_aiActor.ParentRoom.area;
		if (intVector.x < 0)
		{
			int num = specRigidbody.PixelColliders[0].MinX + pixelOffset.x;
			int num2 = area.basePosition.x * 16;
			if (num < num2)
			{
				validLocation = false;
			}
		}
		else if (intVector.x > 0)
		{
			int num3 = specRigidbody.PixelColliders[0].MaxX + pixelOffset.x;
			int num4 = (area.basePosition.x + area.dimensions.x) * 16 - 1;
			if (num3 > num4)
			{
				validLocation = false;
			}
		}
		else if (intVector.y < 0)
		{
			int num5 = specRigidbody.PixelColliders[0].MinY + pixelOffset.y;
			int num6 = area.basePosition.y * 16;
			if (num5 < num6)
			{
				validLocation = false;
			}
		}
		else if (intVector.y > 0)
		{
			int num7 = specRigidbody.PixelColliders[0].MaxY + pixelOffset.y;
			int num8 = (area.basePosition.y + area.dimensions.y) * 16 - 1;
			if (num7 > num8)
			{
				validLocation = false;
			}
		}
	}

	private void BeginState(UnderwaterState state)
	{
		switch (state)
		{
		case UnderwaterState.Disappear:
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
			if (!AttackableDuringAnimation)
			{
				m_aiActor.specRigidbody.CollideWithOthers = false;
				m_aiActor.IsGone = true;
			}
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "MeduziUnderwaterBehavior");
			}
			break;
		case UnderwaterState.Gone:
			m_timer = GoneTime;
			m_aiActor.specRigidbody.CollideWithOthers = false;
			m_aiActor.IsGone = true;
			m_aiActor.sprite.renderer.enabled = false;
			crawlSprite.transform.rotation = Quaternion.identity;
			crawlSprite.SetActive(true);
			crawlAnimator.Play(crawlAnimator.DefaultClip, 0f, crawlAnimator.DefaultClip.fps);
			if (startingDirection == StartingDirection.Player && (bool)m_aiActor.TargetRigidbody)
			{
				m_direction = (m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.Ground) - m_aiActor.specRigidbody.UnitCenter).ToAngle();
			}
			else
			{
				m_direction = UnityEngine.Random.Range(0f, 360f);
			}
			m_angularVelocity = 0f;
			crawlSprite.transform.rotation = Quaternion.Euler(0f, 0f, BraveMathCollege.QuantizeFloat(m_direction, 11.25f));
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_direction, crawlSpeed);
			m_goopDoer.enabled = true;
			break;
		case UnderwaterState.Reappear:
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
			if (AttackableDuringAnimation)
			{
				m_aiActor.specRigidbody.CollideWithOthers = true;
				m_aiActor.IsGone = false;
			}
			m_aiActor.sprite.renderer.enabled = true;
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "MeduziUnderwaterBehavior");
			}
			SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
			break;
		}
	}

	private void EndState(UnderwaterState state)
	{
		switch (state)
		{
		case UnderwaterState.Disappear:
			m_shadowSprite.renderer.enabled = false;
			SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, false);
			if (disappearBulletScript != null && !disappearBulletScript.IsNull && m_shouldFire)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, disappearBulletScript);
				m_shouldFire = false;
			}
			break;
		case UnderwaterState.Gone:
			crawlSprite.SetActive(false);
			m_aiActor.BehaviorOverridesVelocity = false;
			m_goopDoer.enabled = false;
			break;
		case UnderwaterState.Reappear:
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
			break;
		}
	}
}
