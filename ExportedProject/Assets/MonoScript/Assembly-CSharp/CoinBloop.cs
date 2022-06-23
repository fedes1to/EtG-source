using System.Collections;
using UnityEngine;

public class CoinBloop : BraveBehaviour
{
	private static int bloopCounter;

	public float bloopWait = 0.7f;

	private int m_bloopIndex = -1;

	private Vector3 m_cachedLocalPosition;

	private Vector3 m_cachedParentLocalPosition;

	private int m_cachedBloopCounter = -1;

	private float elapsed;

	private PlayerController m_player;

	private Bounds? m_sprBounds;

	private void Start()
	{
		if (m_bloopIndex == -1)
		{
			base.gameObject.layer = LayerMask.NameToLayer("Unpixelated");
			base.transform.localPosition = BraveUtility.QuantizeVector(base.transform.localPosition, 16f);
			m_cachedLocalPosition = base.transform.localPosition;
			base.renderer.enabled = false;
		}
	}

	private void Update()
	{
		if (m_bloopIndex <= -1)
		{
			return;
		}
		if (bloopCounter > m_cachedBloopCounter)
		{
			m_cachedBloopCounter = bloopCounter;
			if (elapsed / bloopWait > 0.75f)
			{
				elapsed = bloopWait;
			}
		}
		if (!m_sprBounds.HasValue)
		{
			m_sprBounds = base.sprite.GetBounds();
		}
		float num = Mathf.Max(0.625f, m_sprBounds.Value.extents.y * 2f + 0.0625f);
		float num2 = (float)(bloopCounter - m_bloopIndex) * num;
		float num3 = 0f;
		if ((bool)GameUIRoot.Instance && (bool)m_player && (bool)GameUIRoot.Instance.GetReloadBarForPlayer(m_player) && GameUIRoot.Instance.GetReloadBarForPlayer(m_player).ReloadIsActive)
		{
			num3 = 0.5f;
		}
		base.transform.parent.localPosition = m_cachedParentLocalPosition + new Vector3(0f, num2 + num3, 0f);
	}

	protected void DoBloopInternal(tk2dBaseSprite targetSprite, string overrideSprite, Color tintColor, bool addOutline = false)
	{
		m_bloopIndex = bloopCounter;
		m_cachedBloopCounter = bloopCounter;
		if (string.IsNullOrEmpty(overrideSprite))
		{
			base.sprite.SetSprite(targetSprite.Collection, targetSprite.spriteId);
		}
		else
		{
			base.sprite.SetSprite(targetSprite.Collection, overrideSprite);
		}
		base.sprite.color = tintColor;
		if (addOutline)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
		}
		Bounds bounds = base.sprite.GetBounds();
		float num = bounds.min.x + bounds.extents.x;
		base.transform.parent.position = base.transform.parent.position.WithX(BraveMathCollege.QuantizeFloat(base.transform.parent.parent.GetComponent<PlayerController>().LockedApproximateSpriteCenter.x - num, 0.0625f));
		base.transform.parent.localPosition = base.transform.parent.localPosition.WithZ(-5f);
		m_cachedParentLocalPosition = base.transform.parent.localPosition;
		StartCoroutine(Bloop());
	}

	public void DoBloop(tk2dBaseSprite targetSprite, string overrideSprite, Color tintColor, bool addOutline = false)
	{
		bloopCounter++;
		if (m_player == null)
		{
			m_player = GetComponentInParent<PlayerController>();
		}
		GameObject gameObject = Object.Instantiate(base.transform.parent.gameObject);
		gameObject.transform.parent = base.transform.parent.parent;
		gameObject.transform.position = base.transform.parent.position;
		CoinBloop componentInParent = gameObject.transform.GetChild(0).GetComponentInParent<CoinBloop>();
		componentInParent.m_player = m_player;
		componentInParent.DoBloopInternal(targetSprite, overrideSprite, tintColor, addOutline);
	}

	private IEnumerator Bloop()
	{
		Vector3 localPosition = base.transform.localPosition;
		GetComponent<Animation>().Play();
		base.renderer.enabled = true;
		elapsed = 0f;
		while (elapsed < bloopWait)
		{
			float yOffset = 0f;
			if ((bool)GameUIRoot.Instance && GameUIRoot.Instance.GetReloadBarForPlayer(m_player).ReloadIsActive)
			{
				yOffset = 0.5f;
			}
			base.transform.localPosition = BraveUtility.QuantizeVector(localPosition + new Vector3(0f, yOffset, 0f), 16f);
			if ((bool)base.sprite)
			{
				base.sprite.UpdateZDepth();
			}
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		GetComponent<Animation>().Stop();
		base.renderer.enabled = false;
		base.transform.localPosition = m_cachedLocalPosition;
		Object.Destroy(base.transform.parent.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
