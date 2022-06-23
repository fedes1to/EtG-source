using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public abstract class ChallengeModifier : MonoBehaviour
{
	[SerializeField]
	public string DisplayName;

	[SerializeField]
	public string AlternateLanguageDisplayName;

	[SerializeField]
	public string AtlasSpriteName;

	[SerializeField]
	public bool ValidInBossChambers = true;

	[SerializeField]
	public List<ChallengeModifier> MutuallyExclusive;

	[NonSerialized]
	public dfSprite IconSprite;

	[NonSerialized]
	public dfLabel IconLabel;

	[NonSerialized]
	public bool IconShattered;

	public void ShatterIcon(dfAnimationClip ChallengeBurstClip)
	{
		if (!IconShattered && (bool)IconSprite)
		{
			IconShattered = true;
			dfSpriteAnimation dfSpriteAnimation2 = IconSprite.gameObject.AddComponent<dfSpriteAnimation>();
			dfSpriteAnimation2.Target = new dfComponentMemberInfo();
			dfComponentMemberInfo target = dfSpriteAnimation2.Target;
			target.Component = IconSprite;
			target.MemberName = "SpriteName";
			dfSpriteAnimation2.Clip = ChallengeBurstClip;
			dfSpriteAnimation2.Length = 0.2f;
			dfSpriteAnimation2.LoopType = dfTweenLoopType.Once;
			dfSpriteAnimation2.Play();
			UnityEngine.Object.Destroy(IconLabel.gameObject, 0.2f);
			UnityEngine.Object.Destroy(IconSprite.gameObject, 0.2f);
		}
	}

	public virtual bool IsValid(RoomHandler room)
	{
		return true;
	}
}
