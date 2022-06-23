using System.Collections;
using UnityEngine;

public class SimpleFlagDisabler : MonoBehaviour
{
	[LongEnum]
	public GungeonFlags FlagToCheckFor;

	public bool DisableOnThisFlagValue;

	public bool UsesStatComparisonInstead;

	public TrackedStats RelevantStat = TrackedStats.NUMBER_ATTEMPTS;

	public int minStatValue = 1;

	public string ChangeSpriteInstead;

	public bool EnableOnGunGameMode;

	public bool DisableIfNotFoyer;

	private IEnumerator Start()
	{
		while (Foyer.DoIntroSequence || Foyer.DoMainMenu)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(ChangeSpriteInstead))
		{
			UsesStatComparisonInstead = true;
		}
		if (DisableIfNotFoyer)
		{
			if (!GameManager.Instance.IsFoyer)
			{
				Disable();
			}
		}
		else
		{
			if (UsesStatComparisonInstead && base.transform.parent != null && base.transform.parent.name.Contains("Livery") && GameStatsManager.Instance.AnyPastBeaten() && RelevantStat == TrackedStats.NUMBER_ATTEMPTS)
			{
				yield break;
			}
			if (UsesStatComparisonInstead)
			{
				if (GameStatsManager.Instance.GetPlayerStatValue(RelevantStat) < (float)minStatValue)
				{
					Disable();
				}
			}
			else if (FlagToCheckFor != 0 && GameStatsManager.Instance.GetFlag(FlagToCheckFor) == DisableOnThisFlagValue)
			{
				Disable();
			}
		}
	}

	private void Update()
	{
		if (EnableOnGunGameMode && !GameManager.Instance.IsSelectingCharacter && GameManager.Instance.PrimaryPlayer != null && (GameManager.Instance.PrimaryPlayer.CharacterUsesRandomGuns || ChallengeManager.CHALLENGE_MODE_ACTIVE))
		{
			SpeculativeRigidbody component = GetComponent<SpeculativeRigidbody>();
			if (!component.enabled)
			{
				component.enabled = true;
				component.Reinitialize();
				GetComponent<MeshRenderer>().enabled = true;
			}
		}
	}

	private void Disable()
	{
		SpeculativeRigidbody component = GetComponent<SpeculativeRigidbody>();
		if (!string.IsNullOrEmpty(ChangeSpriteInstead))
		{
			GetComponent<tk2dBaseSprite>().SetSprite(ChangeSpriteInstead);
			if ((bool)component)
			{
				component.Reinitialize();
			}
			return;
		}
		if ((bool)component)
		{
			component.enabled = false;
		}
		if (!EnableOnGunGameMode)
		{
			base.gameObject.SetActive(false);
		}
		else
		{
			GetComponent<MeshRenderer>().enabled = false;
		}
	}
}
