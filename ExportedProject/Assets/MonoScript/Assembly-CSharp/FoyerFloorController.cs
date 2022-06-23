using System.Collections;
using UnityEngine;

public class FoyerFloorController : MonoBehaviour
{
	public string FinalFormSpriteName;

	public string IntermediateFormSpriteName;

	public string BaseSpriteName;

	public tk2dSprite PitSprite;

	public string FinalPitName;

	public string IntermediatePitName;

	public string BasePitName;

	private IEnumerator Start()
	{
		while (Foyer.DoIntroSequence || Foyer.DoMainMenu)
		{
			yield return null;
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_ALTERNATE_GUNS_UNLOCKED) || GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLINGER_UNLOCKED))
		{
			GetComponent<tk2dBaseSprite>().SetSprite(FinalFormSpriteName);
			PitSprite.SetSprite(FinalPitName);
		}
		else if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.NUMBER_ATTEMPTS) >= 4f)
		{
			GetComponent<tk2dBaseSprite>().SetSprite(IntermediateFormSpriteName);
			PitSprite.SetSprite(IntermediatePitName);
		}
		else
		{
			GetComponent<tk2dBaseSprite>().SetSprite(BaseSpriteName);
			PitSprite.SetSprite(BasePitName);
		}
		GetComponent<SpeculativeRigidbody>().Reinitialize();
		PitSprite.GetComponent<SpeculativeRigidbody>().Reinitialize();
	}
}
