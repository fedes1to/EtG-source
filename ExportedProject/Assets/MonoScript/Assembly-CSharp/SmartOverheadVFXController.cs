using UnityEngine;

public class SmartOverheadVFXController : BraveBehaviour
{
	public Vector2 offset;

	private PlayerController m_attachedPlayer;

	private Vector3 m_originalOffset;

	private bool m_playerInitialized;

	public void Initialize(PlayerController attachTarget, Vector3 offset)
	{
		m_playerInitialized = true;
		m_attachedPlayer = attachTarget;
		m_originalOffset = base.transform.localPosition.Quantize(0.0625f, VectorConversions.Floor);
	}

	public void OnDespawned()
	{
		m_playerInitialized = false;
		m_attachedPlayer = null;
		m_originalOffset = Vector3.zero;
	}

	private void Update()
	{
		if (m_playerInitialized)
		{
			if (m_attachedPlayer.healthHaver.IsDead)
			{
				SpawnManager.Despawn(base.gameObject);
			}
			Vector3 originalOffset = m_originalOffset;
			if (GameUIRoot.Instance.GetReloadBarForPlayer(m_attachedPlayer).AnyStatusBarVisible())
			{
				originalOffset += new Vector3(0f, 1.25f, 0f);
			}
			base.transform.localPosition = originalOffset;
		}
		if (offset != Vector2.zero)
		{
			base.transform.localPosition += (Vector3)offset;
		}
	}
}
