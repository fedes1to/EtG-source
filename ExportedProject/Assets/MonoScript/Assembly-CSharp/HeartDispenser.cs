using System.Collections;
using UnityEngine;

public class HeartDispenser : DungeonPlaceableBehaviour, IPlayerInteractable
{
	[PickupIdentifier]
	public int halfHeartId = -1;

	public GameObject dustVFX;

	private bool m_isVisible = true;

	private bool m_hasEverBeenRevealed;

	public static bool DispenserOnFloor;

	private static int m_currentHalfHeartsStored;

	private int m_cachedStored;

	private string m_currentBaseAnimation = "heart_dispenser_idle_empty";

	public static int CurrentHalfHeartsStored
	{
		get
		{
			return m_currentHalfHeartsStored;
		}
		set
		{
			m_currentHalfHeartsStored = value;
		}
	}

	public static void ClearPerLevelData()
	{
		CurrentHalfHeartsStored = 0;
		DispenserOnFloor = false;
	}

	private void UpdateVisuals()
	{
		if (CurrentHalfHeartsStored > 0)
		{
			m_currentBaseAnimation = "heart_dispenser_idle_full";
		}
		else
		{
			m_currentBaseAnimation = "heart_dispenser_idle_empty";
		}
	}

	public void Awake()
	{
		DispenserOnFloor = true;
	}

	private void Start()
	{
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
	}

	public void Update()
	{
		if (m_cachedStored != CurrentHalfHeartsStored)
		{
			m_cachedStored = CurrentHalfHeartsStored;
			UpdateVisuals();
		}
		if ((base.spriteAnimator.IsPlaying("heart_dispenser_idle_empty") || base.spriteAnimator.IsPlaying("heart_dispenser_idle_full")) && base.spriteAnimator.CurrentClip.name != m_currentBaseAnimation)
		{
			base.spriteAnimator.Play(m_currentBaseAnimation);
		}
		if (m_isVisible && !m_hasEverBeenRevealed && CurrentHalfHeartsStored == 0)
		{
			m_isVisible = false;
			ToggleRenderers(false);
		}
		else if (!m_isVisible && (m_hasEverBeenRevealed || CurrentHalfHeartsStored > 0))
		{
			m_hasEverBeenRevealed = true;
			m_isVisible = true;
			ToggleRenderers(true);
		}
	}

	private void ToggleRenderers(bool state)
	{
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, state);
		base.renderer.enabled = state;
		base.transform.Find("shadow").GetComponent<MeshRenderer>().enabled = state;
		base.specRigidbody.enabled = state;
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		return Vector2.Distance(base.specRigidbody.UnitBottomCenter, point);
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (!(interactor.healthHaver.GetCurrentHealthPercentage() >= 1f) && CurrentHalfHeartsStored > 0)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
	}

	public void Interact(PlayerController interactor)
	{
		if (CurrentHalfHeartsStored > 0 && interactor.healthHaver.GetCurrentHealthPercentage() >= 1f)
		{
			base.spriteAnimator.PlayForDuration("heart_dispenser_no", -1f, m_currentBaseAnimation);
		}
		else if (CurrentHalfHeartsStored > 0)
		{
			CurrentHalfHeartsStored--;
			base.spriteAnimator.PlayForDuration("heart_dispenser_dispense", -1f, m_currentBaseAnimation);
			Object.Instantiate(dustVFX, base.transform.position, Quaternion.identity);
			StartCoroutine(DelayedSpawnHalfHeart());
		}
		else
		{
			base.spriteAnimator.PlayForDuration("heart_dispenser_empty", -1f, m_currentBaseAnimation);
		}
	}

	private IEnumerator DelayedSpawnHalfHeart()
	{
		yield return new WaitForSeconds(1.125f);
		PickupObject halfHeart = PickupObjectDatabase.GetById(halfHeartId);
		LootEngine.SpawnItem(halfHeart.gameObject, base.specRigidbody.PrimaryPixelCollider.UnitCenter - halfHeart.sprite.GetBounds().extents.XY(), Vector2.down, 1f);
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}
}
