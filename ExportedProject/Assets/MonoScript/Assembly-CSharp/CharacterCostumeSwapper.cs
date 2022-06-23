using UnityEngine;

public class CharacterCostumeSwapper : MonoBehaviour, IPlayerInteractable
{
	public PlayableCharacters TargetCharacter;

	public tk2dSprite CostumeSprite;

	public tk2dSprite AlternateCostumeSprite;

	public tk2dSpriteAnimation TargetLibrary;

	public bool HasCustomTrigger;

	public bool CustomTriggerIsFlag;

	public GungeonFlags TriggerFlag;

	public bool CustomTriggerIsSpecialReserve;

	private bool m_active;

	private void Start()
	{
		bool flag = GameStatsManager.Instance.GetCharacterSpecificFlag(TargetCharacter, CharacterSpecificGungeonFlags.KILLED_PAST);
		if (HasCustomTrigger)
		{
			if (CustomTriggerIsFlag)
			{
				flag = GameStatsManager.Instance.GetFlag(TriggerFlag);
			}
			else if (CustomTriggerIsSpecialReserve)
			{
				flag = !GameStatsManager.Instance.GetFlag(GungeonFlags.SECRET_BULLETMAN_SEEN_05) && false;
			}
		}
		if (flag)
		{
			m_active = true;
			if (TargetCharacter == PlayableCharacters.Guide)
			{
				CostumeSprite.HeightOffGround = 0.25f;
				AlternateCostumeSprite.HeightOffGround = 0.25f;
				CostumeSprite.UpdateZDepth();
				AlternateCostumeSprite.UpdateZDepth();
			}
			AlternateCostumeSprite.renderer.enabled = true;
			CostumeSprite.renderer.enabled = false;
		}
		else
		{
			m_active = false;
			AlternateCostumeSprite.renderer.enabled = false;
			CostumeSprite.renderer.enabled = false;
		}
	}

	private void Update()
	{
		if (m_active && !GameManager.IsReturningToBreach && !GameManager.Instance.IsSelectingCharacter && !GameManager.Instance.IsLoadingLevel && !(GameManager.Instance.PrimaryPlayer == null) && TargetCharacter != PlayableCharacters.CoopCultist && GameManager.Instance.PrimaryPlayer.characterIdentity != TargetCharacter)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(AlternateCostumeSprite);
			SpriteOutlineManager.RemoveOutlineFromSprite(CostumeSprite);
			AlternateCostumeSprite.renderer.enabled = true;
			CostumeSprite.renderer.enabled = false;
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!m_active)
		{
			return 1000f;
		}
		if (AlternateCostumeSprite.renderer.enabled)
		{
			return Vector2.Distance(point, AlternateCostumeSprite.WorldCenter);
		}
		return Vector2.Distance(point, CostumeSprite.WorldCenter);
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (interactor.characterIdentity == TargetCharacter)
		{
			if (AlternateCostumeSprite.renderer.enabled)
			{
				SpriteOutlineManager.AddOutlineToSprite(AlternateCostumeSprite, Color.white);
			}
			else
			{
				SpriteOutlineManager.AddOutlineToSprite(CostumeSprite, Color.white);
			}
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if (interactor.characterIdentity == TargetCharacter)
		{
			if (AlternateCostumeSprite.renderer.enabled)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(AlternateCostumeSprite);
			}
			else
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(CostumeSprite);
			}
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (interactor.characterIdentity != TargetCharacter || !m_active)
		{
			return;
		}
		if (interactor.IsUsingAlternateCostume)
		{
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
		SpriteOutlineManager.RemoveOutlineFromSprite(AlternateCostumeSprite);
		SpriteOutlineManager.RemoveOutlineFromSprite(CostumeSprite);
		AlternateCostumeSprite.renderer.enabled = !AlternateCostumeSprite.renderer.enabled;
		CostumeSprite.renderer.enabled = !CostumeSprite.renderer.enabled;
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
