using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class LichIntroDoer : SpecificIntroDoer
{
	public static bool DoubleLich;

	public tk2dSprite HandSprite;

	public Texture2D CosmicTex;

	private AIActor m_otherLich;

	public override string OverrideBossMusicEvent
	{
		get
		{
			return (!GameManager.IsGunslingerPast) ? null : "Play_MUS_Lich_Double_01";
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		DoubleLich = GameManager.IsGunslingerPast;
		if (DoubleLich)
		{
			base.aiActor.PreventBlackPhantom = true;
			m_otherLich = AIActor.Spawn(EnemyDatabase.GetOrLoadByGuid(base.aiActor.EnemyGuid), base.specRigidbody.UnitBottomLeft, base.aiActor.ParentRoom, false, AIActor.AwakenAnimationType.Default, false);
			m_otherLich.transform.position = base.transform.position + new Vector3(0.25f, 0.25f, 0f);
			m_otherLich.specRigidbody.Reinitialize();
			m_otherLich.OverrideBlackPhantomShader = ShaderCache.Acquire("Brave/PlayerShaderEevee");
			m_otherLich.ForceBlackPhantom = true;
			m_otherLich.sprite.renderer.material.SetTexture("_EeveeTex", CosmicTex);
			m_otherLich.sprite.renderer.material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
			m_otherLich.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
			animators.Add(m_otherLich.spriteAnimator);
			m_otherLich.aiAnimator.PlayUntilCancelled("preintro");
			StartCoroutine(HandleDelayedTextureCR());
		}
	}

	private IEnumerator HandleDelayedTextureCR()
	{
		yield return null;
		m_otherLich.sprite.renderer.material.SetTexture("_EeveeTex", CosmicTex);
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		if (DoubleLich)
		{
			m_otherLich.aiAnimator.PlayUntilCancelled("intro");
		}
	}

	public override void OnCameraOutro()
	{
		if (DoubleLich)
		{
			base.aiAnimator.FacingDirection = -90f;
			base.aiAnimator.PlayUntilCancelled("idle");
			m_otherLich.aiAnimator.FacingDirection = -90f;
			m_otherLich.aiAnimator.PlayUntilCancelled("idle");
		}
	}
}
