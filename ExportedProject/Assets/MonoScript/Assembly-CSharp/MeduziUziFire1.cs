using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Meduzi/UziFire1")]
public abstract class MeduziUziFire1 : Script
{
	private const int NumBullets = 60;

	protected abstract string UnityAnimationName { get; }

	protected override IEnumerator Top()
	{
		Animation animation = BulletManager.GetUnityAnimation();
		AnimationClip clip = animation.GetClip(UnityAnimationName);
		for (int i = 0; i < 60; i++)
		{
			clip.SampleAnimation(animation.gameObject, (float)i / 60f);
			Fire(new Offset("left hand shoot point"), new Direction(Random.Range(-15, 15), DirectionType.Relative), new Speed(12f));
			Fire(new Offset("right hand shoot point"), new Direction(Random.Range(-15, 15), DirectionType.Relative), new Speed(12f));
			yield return Wait(1);
		}
	}
}
