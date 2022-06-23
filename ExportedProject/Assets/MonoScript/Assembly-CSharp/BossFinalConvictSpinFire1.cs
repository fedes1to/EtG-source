using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalConvict/SpinFire1")]
public class BossFinalConvictSpinFire1 : Script
{
	private const int NumBullets = 48;

	protected override IEnumerator Top()
	{
		Animation animation = BulletManager.GetUnityAnimation();
		AnimationClip clip = animation.GetClip("BossFinalConvictSpinAttack");
		for (int i = 0; i < 48; i++)
		{
			clip.SampleAnimation(animation.gameObject, (float)i / 60f);
			Fire(new Offset("left hand shoot point"), new Direction(Random.Range(-15, 15), DirectionType.Relative), new Speed(9f));
			Fire(new Offset("right hand shoot point"), new Direction(Random.Range(-15, 15), DirectionType.Relative), new Speed(9f));
			yield return Wait(1);
		}
	}
}
