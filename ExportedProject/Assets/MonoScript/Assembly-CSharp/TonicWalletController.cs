using System.Collections;
using Dungeonator;
using UnityEngine;

public class TonicWalletController : MonoBehaviour
{
	private IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.TONIC_IS_LOADED))
		{
			AIAnimator component = GetComponent<AIAnimator>();
			component.IdleAnimation.AnimNames[0] = "super_idle_right";
			component.IdleAnimation.AnimNames[1] = "super_idle_left";
			component.TalkAnimation.AnimNames[0] = "super_talk_right";
			component.TalkAnimation.AnimNames[1] = "super_talk_left";
			component.OtherAnimations[0].anim.AnimNames[0] = "super_bless_right";
			component.OtherAnimations[0].anim.AnimNames[1] = "super_bless_left";
			component.OtherAnimations[1].anim.AnimNames[0] = "super_cool_right";
			component.OtherAnimations[1].anim.AnimNames[1] = "super_cool_left";
		}
	}
}
