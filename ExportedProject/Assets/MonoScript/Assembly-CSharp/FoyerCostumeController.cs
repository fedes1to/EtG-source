using System.Collections;
using UnityEngine;

public class FoyerCostumeController : BraveBehaviour, IPlayerInteractable
{
	[LongEnum]
	public GungeonFlags RequiredFlag;

	public tk2dSpriteAnimation TargetLibrary;

	private bool m_active;

	private IEnumerator Start()
	{
		while (Foyer.DoIntroSequence || Foyer.DoMainMenu)
		{
			yield return null;
		}
		if (!GameStatsManager.Instance.GetFlag(RequiredFlag))
		{
			m_active = false;
			base.gameObject.SetActive(false);
		}
		else
		{
			m_active = true;
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!m_active)
		{
			return 1000f;
		}
		return Vector2.Distance(point, base.sprite.WorldCenter);
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void Interact(PlayerController interactor)
	{
		if (!m_active)
		{
			return;
		}
		if (interactor.IsUsingAlternateCostume)
		{
			if (interactor.AlternateCostumeLibrary == TargetLibrary)
			{
				interactor.SwapToAlternateCostume();
				return;
			}
			interactor.SwapToAlternateCostume();
			interactor.AlternateCostumeLibrary = TargetLibrary;
			interactor.SwapToAlternateCostume();
		}
		else
		{
			if ((bool)TargetLibrary)
			{
				interactor.AlternateCostumeLibrary = TargetLibrary;
			}
			interactor.SwapToAlternateCostume();
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (m_active)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if (m_active)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		}
	}
}
