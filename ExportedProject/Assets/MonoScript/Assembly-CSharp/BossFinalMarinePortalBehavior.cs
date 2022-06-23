using FullInspector;
using Pathfinding;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalMarine/PortalBehavior")]
public class BossFinalMarinePortalBehavior : BasicAttackBehavior
{
	public enum ShadowSupport
	{
		None,
		Fade,
		Animate
	}

	private enum TeleportState
	{
		None,
		TeleportOut,
		Gone,
		TeleportIn,
		PostTeleport
	}

	public bool AttackableDuringAnimation;

	public float GoneTime = 1f;

	public float PortalSize = 16f;

	[InspectorCategory("Visuals")]
	public string teleportOutAnim = "teleport_out";

	[InspectorCategory("Visuals")]
	public string teleportInAnim = "teleport_in";

	[InspectorCategory("Visuals")]
	public bool teleportRequiresTransparency;

	[InspectorCategory("Visuals")]
	public bool hasOutlinesDuringAnim = true;

	[InspectorCategory("Visuals")]
	public string portalAnim = "weak_attack_charge";

	[InspectorCategory("Visuals")]
	public ShadowSupport shadowSupport;

	[InspectorShowIf("ShowShadowAnimationNames")]
	[InspectorCategory("Visuals")]
	public string shadowOutAnim;

	[InspectorCategory("Visuals")]
	[InspectorShowIf("ShowShadowAnimationNames")]
	public string shadowInAnim;

	public bool ManuallyDefineRoom;

	[InspectorShowIf("ManuallyDefineRoom")]
	[InspectorIndent]
	public Vector2 roomMin;

	[InspectorIndent]
	[InspectorShowIf("ManuallyDefineRoom")]
	public Vector2 roomMax;

	private DimensionFogController m_portalController;

	private tk2dBaseSprite m_shadowSprite;

	private Shader m_cachedShader;

	private float m_timer;

	private TeleportState m_state;

	private TeleportState State
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
		m_portalController = Object.FindObjectOfType<DimensionFogController>();
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
		State = TeleportState.TeleportOut;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == TeleportState.TeleportOut)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f - m_aiAnimator.CurrentClipProgress);
			}
			if (!m_aiAnimator.IsPlaying(teleportOutAnim))
			{
				State = TeleportState.Gone;
			}
		}
		else if (State == TeleportState.Gone)
		{
			if (m_timer <= 0f)
			{
				State = TeleportState.TeleportIn;
			}
		}
		else if (State == TeleportState.TeleportIn)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(m_aiAnimator.CurrentClipProgress);
			}
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "BossFinalMarinePortalBehavior");
			}
			if (!m_aiAnimator.IsPlaying(teleportInAnim))
			{
				State = TeleportState.PostTeleport;
			}
		}
		else if (State == TeleportState.PostTeleport && !m_aiAnimator.IsPlaying(portalAnim))
		{
			State = TeleportState.None;
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	private void BeginState(TeleportState state)
	{
		switch (state)
		{
		case TeleportState.TeleportOut:
			if (teleportRequiresTransparency)
			{
				m_cachedShader = m_aiActor.renderer.material.shader;
				m_aiActor.sprite.usesOverrideMaterial = true;
				m_aiActor.renderer.material.shader = ShaderCache.Acquire("Brave/LitBlendUber");
			}
			m_aiAnimator.PlayUntilCancelled(teleportOutAnim, true);
			if (shadowSupport == ShadowSupport.Animate)
			{
				m_shadowSprite.spriteAnimator.PlayAndForceTime(shadowOutAnim, m_aiAnimator.CurrentClipLength);
			}
			m_aiActor.ClearPath();
			if (!AttackableDuringAnimation)
			{
				m_aiActor.specRigidbody.CollideWithOthers = false;
				m_aiActor.IsGone = true;
			}
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "BossFinalMarinePortalBehavior");
			}
			if (!hasOutlinesDuringAnim)
			{
				SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, false);
			}
			break;
		case TeleportState.Gone:
			if (GoneTime <= 0f)
			{
				State = TeleportState.TeleportIn;
				break;
			}
			m_timer = GoneTime;
			m_aiActor.specRigidbody.CollideWithOthers = false;
			m_aiActor.IsGone = true;
			m_aiActor.sprite.renderer.enabled = false;
			break;
		case TeleportState.TeleportIn:
			DoTeleport();
			m_aiAnimator.PlayUntilFinished(teleportInAnim, true);
			if (shadowSupport == ShadowSupport.Animate)
			{
				m_shadowSprite.spriteAnimator.PlayAndForceTime(shadowInAnim, m_aiAnimator.CurrentClipLength);
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
				m_aiShooter.ToggleGunAndHandRenderers(false, "BossFinalMarinePortalBehavior");
			}
			if (hasOutlinesDuringAnim)
			{
				SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
			}
			break;
		case TeleportState.PostTeleport:
			m_aiAnimator.PlayUntilFinished(portalAnim, true);
			m_portalController.targetRadius = PortalSize;
			break;
		}
	}

	private void EndState(TeleportState state)
	{
		switch (state)
		{
		case TeleportState.TeleportOut:
			m_shadowSprite.renderer.enabled = false;
			if (hasOutlinesDuringAnim)
			{
				SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, false);
			}
			break;
		case TeleportState.TeleportIn:
			if (teleportRequiresTransparency)
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
				m_aiShooter.ToggleGunAndHandRenderers(true, "BossFinalMarinePortalBehavior");
			}
			if (!hasOutlinesDuringAnim)
			{
				SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
			}
			break;
		}
	}

	private void DoTeleport()
	{
		Vector2 vector = m_aiActor.specRigidbody.UnitCenter - m_aiActor.transform.position.XY();
		IntVector2 pos = (roomMin + (roomMax - roomMin + Vector2.one) / 2f).ToIntVector2(VectorConversions.Floor);
		m_aiActor.transform.position = Pathfinder.GetClearanceOffset(pos, m_aiActor.Clearance) - vector;
		m_aiActor.specRigidbody.Reinitialize();
	}
}
