using System.Collections;
using UnityEngine;

public class ParadoxPortalController : DungeonPlaceableBehaviour, IPlayerInteractable
{
	public Texture2D CosmicTex;

	private bool m_used;

	public float GetDistanceToPoint(Vector2 point)
	{
		return Vector2.Distance(point, base.transform.position.XY()) / 1.5f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
	}

	public void OnExitRange(PlayerController interactor)
	{
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public void Interact(PlayerController interactor)
	{
		if (!m_used && interactor.IsPrimaryPlayer)
		{
			m_used = true;
			interactor.portalEeveeTex = CosmicTex;
			interactor.IsTemporaryEeveeForUnlock = true;
			base.transform.position.GetAbsoluteRoom().DeregisterInteractable(this);
			StartCoroutine(HandleDestroy());
		}
	}

	private IEnumerator HandleDestroy()
	{
		float elapsed = 0f;
		float duration = 1f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			GetComponent<MeshRenderer>().material.SetFloat("_UVDistCutoff", Mathf.Lerp(0.2f, 0f, elapsed / duration));
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}
}
