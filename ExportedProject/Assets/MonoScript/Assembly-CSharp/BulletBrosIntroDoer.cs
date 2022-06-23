using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class BulletBrosIntroDoer : SpecificIntroDoer
{
	public tk2dSpriteAnimator shadowDummy;

	private bool m_initialized;

	private bool m_finished;

	private AIAnimator m_smiley;

	private tk2dBaseSprite m_smileyShadow;

	private AIAnimator m_shades;

	private tk2dBaseSprite m_shadesShadow;

	public override Vector2? OverrideIntroPosition
	{
		get
		{
			return 0.5f * (m_smiley.specRigidbody.GetUnitCenter(ColliderType.HitBox) + m_shades.specRigidbody.GetUnitCenter(ColliderType.HitBox));
		}
	}

	public override bool IsIntroFinished
	{
		get
		{
			return m_finished;
		}
	}

	public void Update()
	{
		if (m_initialized || !base.aiActor.ShadowObject)
		{
			return;
		}
		m_smiley = base.aiAnimator;
		for (int i = 0; i < StaticReferenceManager.AllBros.Count; i++)
		{
			if (StaticReferenceManager.AllBros[i].gameObject != base.gameObject)
			{
				m_shades = StaticReferenceManager.AllBros[i].aiAnimator;
				break;
			}
		}
		m_smiley.aiActor.ToggleRenderers(false);
		m_smiley.aiShooter.ToggleGunAndHandRenderers(false, "BulletBrosIntroDoer");
		m_smiley.transform.position += PhysicsEngine.PixelToUnit(new IntVector2(11, 0)).ToVector3ZUp();
		m_smiley.specRigidbody.Reinitialize();
		m_shades.aiActor.ToggleRenderers(false);
		m_shades.aiShooter.ToggleGunAndHandRenderers(false, "BulletBrosIntroDoer");
		m_shades.transform.position += PhysicsEngine.PixelToUnit(new IntVector2(-11, 0)).ToVector3ZUp();
		m_shades.specRigidbody.Reinitialize();
		m_smileyShadow = m_smiley.aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
		m_smileyShadow.renderer.enabled = false;
		m_shadesShadow = m_shades.aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
		m_shadesShadow.renderer.enabled = false;
		m_initialized = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = -90f;
		if ((bool)m_smiley && (bool)m_shades)
		{
			m_smiley.aiActor.ToggleRenderers(false);
			SpriteOutlineManager.ToggleOutlineRenderers(m_smiley.sprite, false);
			m_smiley.aiShooter.ToggleGunAndHandRenderers(false, "BulletBrosIntroDoer");
			m_smileyShadow.renderer.enabled = false;
			m_shades.aiActor.ToggleRenderers(false);
			SpriteOutlineManager.ToggleOutlineRenderers(m_shades.sprite, false);
			m_shades.aiShooter.ToggleGunAndHandRenderers(false, "BulletBrosIntroDoer");
			m_shadesShadow.renderer.enabled = false;
		}
		StartCoroutine(FuckOutlines());
	}

	private IEnumerator FuckOutlines()
	{
		yield return null;
		SpriteOutlineManager.ToggleOutlineRenderers(m_smiley.sprite, false);
		SpriteOutlineManager.ToggleOutlineRenderers(m_shades.sprite, false);
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		StartCoroutine(DoIntro());
		animators.Add(m_shades.spriteAnimator);
		animators.Add(shadowDummy);
	}

	public override void OnBossCard()
	{
		m_smileyShadow.renderer.enabled = true;
		m_shadesShadow.renderer.enabled = true;
		m_smiley.aiShooter.ToggleGunAndHandRenderers(true, "BulletBrosIntroDoer");
		m_shades.aiShooter.ToggleGunAndHandRenderers(true, "BulletBrosIntroDoer");
	}

	public override void EndIntro()
	{
		m_finished = true;
		StopAllCoroutines();
		m_smiley.aiActor.ToggleRenderers(true);
		SpriteOutlineManager.ToggleOutlineRenderers(m_smiley.sprite, true);
		m_smiley.sprite.renderer.enabled = true;
		m_smiley.EndAnimation();
		m_smiley.aiShooter.ToggleGunAndHandRenderers(true, "BulletBrosIntroDoer");
		m_smiley.specRigidbody.CollideWithOthers = true;
		m_smiley.aiActor.IsGone = false;
		m_smiley.aiActor.State = AIActor.ActorState.Normal;
		m_smiley.aiShooter.AimAtPoint(m_smiley.aiActor.CenterPosition + new Vector2(10f, -2f));
		m_smiley.FacingDirection = -90f;
		m_shades.aiActor.ToggleRenderers(true);
		SpriteOutlineManager.ToggleOutlineRenderers(m_shades.sprite, true);
		m_shades.sprite.renderer.enabled = true;
		m_shades.EndAnimation();
		m_shades.aiShooter.ToggleGunAndHandRenderers(true, "BulletBrosIntroDoer");
		m_shades.specRigidbody.CollideWithOthers = true;
		m_shades.aiActor.IsGone = false;
		m_shades.aiActor.State = AIActor.ActorState.Normal;
		m_shades.aiShooter.AimAtPoint(m_shades.aiActor.CenterPosition + new Vector2(-10f, -2f));
		m_shades.FacingDirection = -90f;
		shadowDummy.renderer.enabled = false;
	}

	private IEnumerator DoIntro()
	{
		SpriteOutlineManager.ToggleOutlineRenderers(m_smiley.sprite, false);
		SpriteOutlineManager.ToggleOutlineRenderers(m_shades.sprite, false);
		float elapsed2 = 0f;
		for (float duration2 = 1f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			m_smiley.aiShooter.ToggleGunAndHandRenderers(false, "BulletBrosIntroDoer");
			m_shades.aiShooter.ToggleGunAndHandRenderers(false, "BulletBrosIntroDoer");
			m_smileyShadow.renderer.enabled = false;
			m_shadesShadow.renderer.enabled = false;
			yield return null;
		}
		m_smiley.aiActor.ToggleRenderers(true);
		m_smiley.PlayUntilFinished("intro");
		SpriteOutlineManager.ToggleOutlineRenderers(m_smiley.sprite, true);
		m_shades.aiActor.ToggleRenderers(true);
		m_shades.PlayUntilFinished("intro");
		SpriteOutlineManager.ToggleOutlineRenderers(m_shades.sprite, true);
		shadowDummy.Play();
		while (m_smiley.IsPlaying("intro"))
		{
			m_smiley.aiShooter.ToggleGunAndHandRenderers(false, "BulletBrosIntroDoer");
			m_shades.aiShooter.ToggleGunAndHandRenderers(false, "BulletBrosIntroDoer");
			m_smileyShadow.renderer.enabled = false;
			m_shadesShadow.renderer.enabled = false;
			yield return null;
		}
		shadowDummy.renderer.enabled = false;
		m_smileyShadow.renderer.enabled = true;
		m_smiley.PlayUntilFinished("idle");
		m_smiley.aiShooter.ToggleGunAndHandRenderers(true, "BulletBrosIntroDoer");
		m_smiley.aiShooter.AimAtPoint(m_smiley.aiActor.CenterPosition + new Vector2(10f, -2f));
		m_shadesShadow.renderer.enabled = true;
		m_shades.PlayUntilFinished("idle");
		m_shades.aiShooter.ToggleGunAndHandRenderers(true, "BulletBrosIntroDoer");
		m_shades.aiShooter.AimAtPoint(m_shades.aiActor.CenterPosition + new Vector2(-10f, -2f));
		elapsed2 = 0f;
		for (float duration2 = 1f; elapsed2 < duration2; elapsed2 += GameManager.INVARIANT_DELTA_TIME)
		{
			yield return null;
		}
		m_finished = true;
	}
}
