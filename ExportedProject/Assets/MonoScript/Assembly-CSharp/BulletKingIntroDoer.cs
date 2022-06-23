using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class BulletKingIntroDoer : SpecificIntroDoer
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void StartIntro(List<tk2dSpriteAnimator> animators)
	{
		BulletKingToadieController[] array = Object.FindObjectsOfType<BulletKingToadieController>();
		for (int i = 0; i < array.Length; i++)
		{
			animators.Add(array[i].spriteAnimator);
			if ((bool)array[i].scepterAnimator)
			{
				animators.Add(array[i].scepterAnimator);
			}
		}
	}
}
