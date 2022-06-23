using UnityEngine;

public class PlayerHandController : BraveBehaviour
{
	public bool ForceRenderersOff;

	public Transform attachPoint;

	public float handHeightFromGun = 0.05f;

	protected float OUTLINE_DEPTH = 0.1f;

	protected PlayerController m_ownerPlayer;

	private bool IsPlayerPrimary;

	protected Shader m_cachedShader;

	private bool m_hasAlteredHeight;

	private Vector3 m_cachedStartPosition;

	private tk2dSprite[] outlineSprites;

	public void InitializeWithPlayer(PlayerController p, bool isPrimary)
	{
		m_ownerPlayer = p;
		IsPlayerPrimary = isPrimary;
	}

	private void Start()
	{
		m_cachedStartPosition = base.transform.localPosition;
		base.sprite.HeightOffGround = handHeightFromGun;
		DepthLookupManager.ProcessRenderer(base.renderer);
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, OUTLINE_DEPTH);
		m_cachedShader = base.sprite.renderer.material.shader;
	}

	private void ToggleRenderers(bool e)
	{
		if (outlineSprites == null || outlineSprites.Length == 0)
		{
			outlineSprites = SpriteOutlineManager.GetOutlineSprites(base.sprite);
		}
		base.renderer.enabled = e;
		for (int i = 0; i < outlineSprites.Length; i++)
		{
			outlineSprites[i].renderer.enabled = e;
		}
	}

	private void LateUpdate()
	{
		if (!attachPoint || !attachPoint.gameObject.activeSelf)
		{
			ToggleRenderers(false);
			base.transform.localPosition = m_cachedStartPosition;
		}
		else
		{
			ToggleRenderers(!ForceRenderersOff);
			base.transform.position = BraveUtility.QuantizeVector(attachPoint.position, 16f);
		}
		if ((bool)m_ownerPlayer && (bool)m_ownerPlayer.CurrentGun && m_ownerPlayer.CurrentGun.OnlyUsesIdleInWeaponBox)
		{
			float num = 0f;
			float currentAngle = m_ownerPlayer.CurrentGun.CurrentAngle;
			if (m_ownerPlayer.CurrentGun.IsFiring)
			{
				if (currentAngle <= 155f && currentAngle >= 25f)
				{
					num = 0f;
				}
				else
				{
					m_hasAlteredHeight = true;
					num = ((!IsPlayerPrimary) ? 1.5f : 0.5f);
				}
			}
			base.sprite.HeightOffGround = handHeightFromGun + num;
		}
		else if (m_hasAlteredHeight)
		{
			base.sprite.HeightOffGround = handHeightFromGun;
			m_hasAlteredHeight = false;
		}
		base.sprite.UpdateZDepth();
	}

	public Material SetOverrideShader(Shader overrideShader)
	{
		Debug.Log("overriding hand shader");
		base.sprite.renderer.material.shader = overrideShader;
		return base.sprite.renderer.material;
	}

	public void ClearOverrideShader()
	{
		base.sprite.renderer.material.shader = m_cachedShader;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
