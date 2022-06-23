using System.Collections;
using UnityEngine;

public class PlayerIdentityPortraitDoer : MonoBehaviour
{
	public bool IsPlayerTwo;

	public static string GetPortraitSpriteName(PlayableCharacters character)
	{
		switch (character)
		{
		case PlayableCharacters.Convict:
			return "Player_Convict_001";
		case PlayableCharacters.Pilot:
			return "Player_Rogue_001";
		case PlayableCharacters.Guide:
			return "Player_Guide_001";
		case PlayableCharacters.Soldier:
			return "Player_Marine_001";
		case PlayableCharacters.Ninja:
			return "Player_Ninja_001";
		case PlayableCharacters.Cosmonaut:
			return "Player_Cosmonaut_001";
		case PlayableCharacters.Robot:
			return "Player_Robot_001";
		case PlayableCharacters.CoopCultist:
			return "Player_Coop_Pink_001";
		case PlayableCharacters.Bullet:
			return "Player_Bullet_001";
		case PlayableCharacters.Eevee:
			return "Player_Eevee_minimap_001";
		case PlayableCharacters.Gunslinger:
			return "Player_Slinger_001";
		default:
			return "Player_Rogue_001";
		}
	}

	private IEnumerator Start()
	{
		while (IsPlayerTwo && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.SecondaryPlayer == null)
		{
			yield return null;
		}
		while ((!IsPlayerTwo || GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) && GameManager.Instance.PrimaryPlayer == null)
		{
			yield return null;
		}
		dfSprite sprite = GetComponent<dfSprite>();
		if (IsPlayerTwo && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			sprite.SpriteName = GetPortraitSpriteName(GameManager.Instance.SecondaryPlayer.characterIdentity);
		}
		else
		{
			sprite.SpriteName = GetPortraitSpriteName(GameManager.Instance.PrimaryPlayer.characterIdentity);
		}
	}
}
