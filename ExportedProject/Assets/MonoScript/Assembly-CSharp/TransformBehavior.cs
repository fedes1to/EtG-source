using System;
using FullInspector;
using UnityEngine;

public class TransformBehavior : BasicAttackBehavior
{
	public enum ShadowSupport
	{
		None = 10,
		Fade = 20,
		Animate = 30
	}

	public enum Invulnerability
	{
		None = 10,
		WhileTransformed = 20,
		WhileNotTransformed = 30
	}

	private enum TransformState
	{
		None,
		InTrans,
		Transformed,
		OutTrans
	}

	public Invulnerability invulnerabilityMode = Invulnerability.None;

	public bool reflectBullets;

	public float transformedTime = 1f;

	public bool goneWhileTransformed;

	public bool Uninterruptible;

	[InspectorCategory("Attack")]
	public GameObject shootPoint;

	[InspectorCategory("Attack")]
	public BulletScriptSelector inBulletScript;

	[InspectorCategory("Attack")]
	public BulletScriptSelector transformedBulletScript;

	[InspectorCategory("Attack")]
	public bool transformFireImmediately;

	[InspectorCategory("Attack")]
	public BulletScriptSelector outBulletScript;

	[InspectorCategory("Visuals")]
	public string inAnim;

	[InspectorCategory("Visuals")]
	public string transformedAnim;

	[InspectorCategory("Visuals")]
	public bool setTransformAnimAsBaseState;

	[InspectorCategory("Visuals")]
	public string outAnim;

	[InspectorCategory("Visuals")]
	public bool requiresTransparency;

	[InspectorCategory("Visuals")]
	public ShadowSupport shadowSupport = ShadowSupport.None;

	[InspectorCategory("Visuals")]
	[InspectorShowIf("ShowShadowAnimationNames")]
	public string shadowInAnim;

	[InspectorShowIf("ShowShadowAnimationNames")]
	[InspectorCategory("Visuals")]
	public string shadowOutAnim;

	private tk2dBaseSprite m_shadowSprite;

	private Shader m_cachedShader;

	private BulletScriptSource m_bulletSource;

	private PixelCollider m_enemyHitbox;

	private PixelCollider m_bulletBlocker;

	private float m_timer;

	private bool m_shouldFire;

	private bool m_isInvulnerable;

	private bool m_hasTransitioned;

	private TransformState m_state;

	private TransformState State
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

	private bool Invulnerable
	{
		get
		{
			return m_isInvulnerable;
		}
		set
		{
			if (value == m_isInvulnerable)
			{
				return;
			}
			m_enemyHitbox.Enabled = !value;
			m_aiActor.healthHaver.IsVulnerable = !value;
			if (m_bulletBlocker != null)
			{
				m_bulletBlocker.Enabled = value;
			}
			if (reflectBullets && m_aiActor.healthHaver.IsAlive)
			{
				m_aiActor.specRigidbody.ReflectProjectiles = value;
				m_aiActor.specRigidbody.ReflectBeams = value;
				if (value)
				{
					m_aiActor.specRigidbody.ReflectProjectilesNormalGenerator = GetNormal;
					m_aiActor.specRigidbody.ReflectBeamsNormalGenerator = GetNormal;
				}
			}
			m_isInvulnerable = value;
		}
	}

	private bool ShowShadowAnimationNames()
	{
		return shadowSupport == ShadowSupport.Animate;
	}

	private bool ShowReflectBullets()
	{
		return invulnerabilityMode != Invulnerability.None;
	}

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		if (invulnerabilityMode == Invulnerability.None)
		{
			return;
		}
		for (int i = 0; i < m_aiActor.specRigidbody.PixelColliders.Count; i++)
		{
			PixelCollider pixelCollider = m_aiActor.specRigidbody.PixelColliders[i];
			if (pixelCollider.CollisionLayer == CollisionLayer.EnemyHitBox)
			{
				m_enemyHitbox = pixelCollider;
			}
			if (pixelCollider.CollisionLayer == CollisionLayer.BulletBlocker)
			{
				m_bulletBlocker = pixelCollider;
			}
		}
		if (invulnerabilityMode == Invulnerability.WhileTransformed)
		{
			Invulnerable = false;
		}
		else if (invulnerabilityMode == Invulnerability.WhileNotTransformed)
		{
			Invulnerable = true;
		}
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
		State = TransformState.InTrans;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == TransformState.InTrans)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f - m_aiAnimator.CurrentClipProgress);
			}
			if (!m_hasTransitioned && m_aiAnimator.CurrentClipProgress > 0.5f)
			{
				if (invulnerabilityMode == Invulnerability.WhileTransformed)
				{
					Invulnerable = true;
				}
				else if (invulnerabilityMode == Invulnerability.WhileNotTransformed)
				{
					Invulnerable = false;
				}
				m_hasTransitioned = true;
			}
			if (!m_aiAnimator.IsPlaying(inAnim))
			{
				State = ((!(transformedTime > 0f)) ? TransformState.OutTrans : TransformState.Transformed);
			}
		}
		else if (State == TransformState.Transformed)
		{
			if (m_timer <= 0f)
			{
				State = TransformState.OutTrans;
			}
		}
		else if (State == TransformState.OutTrans)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(m_aiAnimator.CurrentClipProgress);
			}
			if (!m_hasTransitioned && m_aiAnimator.CurrentClipProgress > 0.5f)
			{
				if (invulnerabilityMode == Invulnerability.WhileTransformed)
				{
					Invulnerable = false;
				}
				else if (invulnerabilityMode == Invulnerability.WhileNotTransformed)
				{
					Invulnerable = true;
				}
				m_hasTransitioned = true;
			}
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "TransformBehavior");
			}
			if (!m_aiAnimator.IsPlaying(outAnim))
			{
				State = TransformState.None;
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (invulnerabilityMode != Invulnerability.None)
		{
			if (invulnerabilityMode == Invulnerability.WhileTransformed)
			{
				Invulnerable = false;
			}
			else if (invulnerabilityMode == Invulnerability.WhileNotTransformed)
			{
				Invulnerable = true;
			}
		}
		if ((bool)m_aiShooter)
		{
			m_aiShooter.ToggleGunAndHandRenderers(true, "TransformBehavior");
		}
		if ((bool)m_bulletSource && !m_bulletSource.IsEnded)
		{
			m_bulletSource.ForceStop();
		}
		if (setTransformAnimAsBaseState && m_state == TransformState.Transformed)
		{
			m_aiAnimator.ClearBaseAnim();
		}
		m_aiAnimator.EndAnimationIf(inAnim);
		m_aiAnimator.EndAnimationIf(transformedAnim);
		m_aiAnimator.EndAnimationIf(outAnim);
		if (requiresTransparency && (bool)m_cachedShader)
		{
			m_aiActor.sprite.usesOverrideMaterial = false;
			m_aiActor.renderer.material.shader = m_cachedShader;
			m_cachedShader = null;
		}
		if (shadowSupport == ShadowSupport.Fade)
		{
			m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
		}
		else if (shadowSupport == ShadowSupport.Animate)
		{
			tk2dSpriteAnimationClip clipByName = m_shadowSprite.spriteAnimator.GetClipByName(shadowOutAnim);
			m_shadowSprite.spriteAnimator.Play(clipByName, clipByName.frames.Length - 1, clipByName.fps);
		}
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return !Uninterruptible;
	}

	public void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		string eventInfo = clip.GetFrame(frame).eventInfo;
		if (m_shouldFire && eventInfo == "fire")
		{
			if (State == TransformState.InTrans)
			{
				ShootBulletScript(inBulletScript);
			}
			else if (State == TransformState.Transformed)
			{
				ShootBulletScript(transformedBulletScript);
			}
			else if (State == TransformState.OutTrans)
			{
				ShootBulletScript(outBulletScript);
			}
			m_shouldFire = false;
		}
		if (m_state == TransformState.OutTrans || m_state == TransformState.InTrans)
		{
			if (eventInfo == "collider_on")
			{
				m_aiActor.IsGone = false;
				m_aiActor.specRigidbody.CollideWithOthers = true;
			}
			else if (eventInfo == "collider_off")
			{
				m_aiActor.IsGone = true;
				m_aiActor.specRigidbody.CollideWithOthers = false;
			}
		}
	}

	private void ShootBulletScript(BulletScriptSelector script)
	{
		if (!m_bulletSource)
		{
			m_bulletSource = shootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_aiActor.bulletBank;
		m_bulletSource.BulletScript = script;
		m_bulletSource.Initialize();
	}

	private void BeginState(TransformState state)
	{
		m_hasTransitioned = false;
		switch (state)
		{
		case TransformState.InTrans:
			if (inBulletScript != null && !inBulletScript.IsNull)
			{
				m_shouldFire = true;
			}
			if (requiresTransparency)
			{
				m_cachedShader = m_aiActor.renderer.material.shader;
				m_aiActor.sprite.usesOverrideMaterial = true;
				m_aiActor.renderer.material.shader = ShaderCache.Acquire("Brave/LitBlendUber");
			}
			m_aiAnimator.PlayUntilCancelled(inAnim, true);
			if (shadowSupport == ShadowSupport.Animate)
			{
				m_shadowSprite.spriteAnimator.PlayAndForceTime(shadowInAnim, m_aiAnimator.CurrentClipLength);
			}
			m_aiActor.ClearPath();
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "TransformBehavior");
			}
			break;
		case TransformState.Transformed:
			m_timer = transformedTime;
			if (transformedBulletScript != null && !transformedBulletScript.IsNull)
			{
				m_shouldFire = true;
			}
			if (!string.IsNullOrEmpty(transformedAnim))
			{
				m_aiAnimator.PlayUntilCancelled(transformedAnim);
			}
			if (setTransformAnimAsBaseState)
			{
				m_aiAnimator.SetBaseAnim(transformedAnim);
			}
			if (goneWhileTransformed)
			{
				m_aiActor.IsGone = true;
				m_aiActor.specRigidbody.CollideWithOthers = false;
			}
			if (transformFireImmediately && transformedBulletScript != null && !transformedBulletScript.IsNull)
			{
				ShootBulletScript(transformedBulletScript);
				m_shouldFire = false;
			}
			break;
		case TransformState.OutTrans:
			if (outBulletScript != null && !outBulletScript.IsNull)
			{
				m_shouldFire = true;
			}
			m_aiAnimator.PlayUntilFinished(outAnim, true);
			if (shadowSupport == ShadowSupport.Animate)
			{
				m_shadowSprite.spriteAnimator.PlayAndForceTime(shadowOutAnim, m_aiAnimator.CurrentClipLength);
			}
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "TransformBehavior");
			}
			break;
		}
	}

	private void EndState(TransformState state)
	{
		switch (state)
		{
		case TransformState.InTrans:
			if (inBulletScript != null && !inBulletScript.IsNull && m_shouldFire)
			{
				ShootBulletScript(inBulletScript);
				m_shouldFire = false;
			}
			if (invulnerabilityMode == Invulnerability.WhileTransformed)
			{
				Invulnerable = true;
			}
			else if (invulnerabilityMode == Invulnerability.WhileNotTransformed)
			{
				Invulnerable = false;
			}
			break;
		case TransformState.Transformed:
			if (setTransformAnimAsBaseState)
			{
				m_aiAnimator.ClearBaseAnim();
			}
			if (!string.IsNullOrEmpty(transformedAnim))
			{
				m_aiAnimator.EndAnimationIf(transformedAnim);
			}
			if (transformedBulletScript != null && !transformedBulletScript.IsNull && m_shouldFire)
			{
				ShootBulletScript(transformedBulletScript);
				m_shouldFire = false;
			}
			break;
		case TransformState.OutTrans:
			if (requiresTransparency && (bool)m_cachedShader)
			{
				m_aiActor.sprite.usesOverrideMaterial = false;
				m_aiActor.renderer.material.shader = m_cachedShader;
				m_cachedShader = null;
			}
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
			}
			if (goneWhileTransformed)
			{
				m_aiActor.IsGone = false;
				m_aiActor.specRigidbody.CollideWithOthers = true;
			}
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(true, "TransformBehavior");
			}
			if (outBulletScript != null && !outBulletScript.IsNull && m_shouldFire)
			{
				ShootBulletScript(outBulletScript);
				m_shouldFire = false;
			}
			if (invulnerabilityMode == Invulnerability.WhileTransformed)
			{
				Invulnerable = false;
			}
			else if (invulnerabilityMode == Invulnerability.WhileNotTransformed)
			{
				Invulnerable = true;
			}
			break;
		}
	}

	private Vector2 GetNormal(Vector2 contact, Vector2 normal)
	{
		return (contact - m_bulletBlocker.UnitCenter).normalized;
	}
}
