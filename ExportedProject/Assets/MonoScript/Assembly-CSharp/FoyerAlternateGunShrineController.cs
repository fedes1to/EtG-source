using System.Collections;
using Dungeonator;
using UnityEngine;

public class FoyerAlternateGunShrineController : BraveBehaviour, IPlayerInteractable
{
	public Transform talkPoint;

	public string displayTextKey;

	public string acceptOptionKey;

	public string declineOptionKey;

	public tk2dSpriteAnimator Flame;

	public tk2dBaseSprite AlternativeOutlineTarget;

	private bool IsCurrentlyActive
	{
		get
		{
			if (GameManager.HasInstance && (bool)GameManager.Instance.PrimaryPlayer)
			{
				return GameManager.Instance.PrimaryPlayer.UsingAlternateStartingGuns;
			}
			return false;
		}
	}

	private IEnumerator Start()
	{
		while (Foyer.DoIntroSequence || Foyer.DoMainMenu)
		{
			yield return null;
		}
		GetComponent<tk2dBaseSprite>().HeightOffGround = -1f;
		GetComponent<tk2dBaseSprite>().UpdateZDepth();
		if (!GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_ALTERNATE_GUNS_UNLOCKED))
		{
			base.transform.position.GetAbsoluteRoom().DeregisterInteractable(this);
			base.specRigidbody.enabled = false;
			base.gameObject.SetActive(false);
		}
	}

	public void DoEffect(PlayerController interactor)
	{
		if (interactor.characterIdentity != PlayableCharacters.Eevee && interactor.characterIdentity != PlayableCharacters.Gunslinger)
		{
			interactor.UsingAlternateStartingGuns = !interactor.UsingAlternateStartingGuns;
			interactor.ReinitializeGuns();
			interactor.PlayEffectOnActor((GameObject)ResourceCache.Acquire("Global VFX/VFX_AltGunShrine"), Vector3.zero);
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (!TextBoxManager.HasTextBox(talkPoint) && interactor.characterIdentity != PlayableCharacters.Eevee && interactor.characterIdentity != PlayableCharacters.Gunslinger)
		{
			StartCoroutine(HandleShrineConversation(interactor));
		}
	}

	private IEnumerator HandleShrineConversation(PlayerController interactor)
	{
		TextBoxManager.ShowStoneTablet(text: StringTableManager.GetLongString(displayTextKey), worldPosition: talkPoint.position, parent: talkPoint, duration: -1f);
		int selectedResponse = -1;
		interactor.SetInputOverride("shrineConversation");
		bool canUse = GameStatsManager.Instance.GetCharacterSpecificFlag(interactor.characterIdentity, CharacterSpecificGungeonFlags.KILLED_PAST_ALTERNATE_COSTUME);
		yield return null;
		if (canUse)
		{
			string @string = StringTableManager.GetString(acceptOptionKey);
			GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, @string, StringTableManager.GetString(declineOptionKey));
		}
		else
		{
			GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, StringTableManager.GetString(declineOptionKey), string.Empty);
		}
		while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
		{
			yield return null;
		}
		interactor.ClearInputOverride("shrineConversation");
		TextBoxManager.ClearTextBox(talkPoint);
		if (canUse && selectedResponse == 0)
		{
			DoEffect(interactor);
		}
	}

	private void Update()
	{
		if (!Foyer.DoIntroSequence && !Foyer.DoMainMenu && GameManager.HasInstance && !Dungeon.IsGenerating && !GameManager.Instance.IsLoadingLevel && base.gameObject.activeSelf)
		{
			Flame.sprite.HeightOffGround = -0.5f;
			Flame.renderer.enabled = IsCurrentlyActive;
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (base.sprite == null)
		{
			return 100f;
		}
		Vector3 vector = BraveMathCollege.ClosestPointOnRectangle(point, base.specRigidbody.UnitBottomLeft, base.specRigidbody.UnitDimensions);
		return Vector2.Distance(point, vector) / 1.5f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (interactor.characterIdentity != PlayableCharacters.Eevee && interactor.characterIdentity != PlayableCharacters.Gunslinger)
		{
			if (AlternativeOutlineTarget != null)
			{
				SpriteOutlineManager.AddOutlineToSprite(AlternativeOutlineTarget, Color.white);
			}
			else
			{
				SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
			}
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if (interactor.characterIdentity != PlayableCharacters.Eevee && interactor.characterIdentity != PlayableCharacters.Gunslinger)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite((!(AlternativeOutlineTarget != null)) ? base.sprite : AlternativeOutlineTarget);
		}
	}
}
