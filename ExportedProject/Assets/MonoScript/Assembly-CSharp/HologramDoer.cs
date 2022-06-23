using System.Collections;
using UnityEngine;

public class HologramDoer : BraveBehaviour
{
	public Transform Holopoint;

	public tk2dSprite Glower;

	public MeshRenderer ArcRenderer;

	private Material m_arcMaterial;

	public bool Automatic;

	public tk2dSprite TargetAutomaticSprite;

	public AdditionalBraveLight AttachedBraveLight;

	public bool parentHologram;

	public bool NotAHologram;

	private GameObject m_lastSource;

	private tk2dSprite m_hologramSprite;

	private void Start()
	{
		ArcRenderer.enabled = false;
		m_arcMaterial = ArcRenderer.material;
		if (Automatic)
		{
			StartCoroutine(HandleAutomaticTrigger());
		}
	}

	private IEnumerator HandleAutomaticTrigger()
	{
		yield return new WaitForSeconds(0.25f);
		while (base.spriteAnimator.IsPlaying("hbux_base_intro"))
		{
			yield return null;
		}
		base.spriteAnimator.Play("hbux_base_on");
		ChangeToSprite(base.gameObject, null, -1);
	}

	private void Update()
	{
	}

	private IEnumerator ToggleAdditionalLight(bool lightEnabled)
	{
		float elapsed = 0f;
		float duration = 0.25f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			if (!lightEnabled)
			{
				t = 1f - t;
			}
			AttachedBraveLight.LightIntensity = Mathf.Lerp(0f, 2.09f, t);
			yield return null;
		}
	}

	public void HideSprite(GameObject source, bool instant = false)
	{
		if (!(m_lastSource == source) && !instant)
		{
			return;
		}
		if ((bool)m_hologramSprite)
		{
			if (NotAHologram)
			{
				SpriteOutlineManager.ToggleOutlineRenderers(m_hologramSprite, false);
			}
			m_hologramSprite.renderer.enabled = false;
		}
		if ((bool)AttachedBraveLight)
		{
			StartCoroutine(ToggleAdditionalLight(false));
		}
		if (instant)
		{
			ArcRenderer.enabled = false;
		}
		else
		{
			StartCoroutine(HandleArcLerp(true));
		}
		if ((bool)Glower)
		{
			Glower.renderer.material.SetFloat("_EmissivePower", 0f);
		}
		m_lastSource = null;
	}

	private IEnumerator HandleArcLerp(bool invert)
	{
		float ela = 0f;
		if (Automatic)
		{
			m_arcMaterial.SetColor("_OverrideColor", new Color(0f, 0.4f, 0f));
		}
		while (ela < 0.2f)
		{
			ela += BraveTime.DeltaTime;
			float t = ela / 0.2f;
			if (invert)
			{
				t = Mathf.Clamp01(1f - t);
			}
			float smoothT = Mathf.SmoothStep(0f, 1f, t);
			m_arcMaterial.SetFloat("_RevealAmount", smoothT);
			ArcRenderer.enabled = !NotAHologram;
			yield return null;
		}
		if (!invert)
		{
			while (m_lastSource != null)
			{
				m_arcMaterial.SetFloat("_BrightnessWarble", Mathf.PingPong(Time.realtimeSinceStartup, 1f) / 10f + 1f);
				yield return null;
			}
		}
	}

	public void ChangeToSprite(GameObject source, tk2dSpriteCollectionData collection, int spriteId)
	{
		m_lastSource = source;
		if (Automatic)
		{
			m_hologramSprite = TargetAutomaticSprite;
		}
		else if (m_hologramSprite == null)
		{
			GameObject go = new GameObject("hologram");
			m_hologramSprite = tk2dSprite.AddComponent(go, collection, spriteId);
			if (parentHologram)
			{
				m_hologramSprite.transform.parent = base.transform;
			}
			if (NotAHologram)
			{
				SpriteOutlineManager.AddOutlineToSprite(m_hologramSprite, Color.white);
			}
			if ((bool)Glower && !NotAHologram)
			{
				Glower.usesOverrideMaterial = true;
				Glower.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
			}
		}
		else
		{
			m_hologramSprite.SetSprite(collection, spriteId);
			m_hologramSprite.ForceUpdateMaterial();
		}
		if (NotAHologram)
		{
			SpriteOutlineManager.ToggleOutlineRenderers(m_hologramSprite, true);
		}
		m_hologramSprite.renderer.enabled = true;
		m_hologramSprite.usesOverrideMaterial = true;
		if (!NotAHologram)
		{
			m_hologramSprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/HologramShader");
		}
		if (Automatic)
		{
			m_hologramSprite.renderer.material.SetFloat("_IsGreen", 1f);
			m_hologramSprite.spriteAnimator.PlayForDuration(m_hologramSprite.spriteAnimator.DefaultClip.name, m_hologramSprite.spriteAnimator.DefaultClip.BaseClipLength, "hbux_symbol_idle");
		}
		else
		{
			m_hologramSprite.PlaceAtPositionByAnchor(Holopoint.position, tk2dBaseSprite.Anchor.LowerCenter);
			m_hologramSprite.transform.localPosition = m_hologramSprite.transform.localPosition.Quantize(0.0625f);
		}
		if ((bool)Glower && !NotAHologram)
		{
			Glower.renderer.material.SetFloat("_EmissivePower", 20f);
			Glower.renderer.material.SetFloat("_EmissiveColorPower", 3f);
		}
		if ((bool)AttachedBraveLight)
		{
			StartCoroutine(ToggleAdditionalLight(true));
		}
		StartCoroutine(HandleArcLerp(false));
	}
}
