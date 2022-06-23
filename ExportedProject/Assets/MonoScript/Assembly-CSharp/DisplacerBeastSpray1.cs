using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("DisplacerBeastSpray1")]
public class DisplacerBeastSpray1 : Script
{
	public class DisplacerBullet : Bullet
	{
		protected override IEnumerator Top()
		{
			if ((bool)Projectile)
			{
				Projectile.IgnoreTileCollisionsFor(0.25f);
			}
			return null;
		}
	}

	private const int NumBullets = 20;

	private const float BulletSpread = 27f;

	protected override IEnumerator Top()
	{
		BulletLimbController[] limbs = base.BulletBank.aiAnimator.GetComponentsInChildren<BulletLimbController>();
		for (int j = 0; j < limbs.Length; j++)
		{
			limbs[j].DoingTell = true;
		}
		yield return Wait(54);
		base.BulletBank.aiAnimator.LockFacingDirection = true;
		yield return Wait(6);
		for (int k = 0; k < limbs.Length; k++)
		{
			limbs[k].DoingTell = false;
		}
		string[] transformNames = GetTransformNames();
		for (int i = 0; i < 20; i++)
		{
			string transformName = transformNames[i % 2];
			Fire(new Offset(transformName), new Direction(GetAimDirection(transformName) + Random.RandomRange(-27f, 27f)), new Speed(14f), new DisplacerBullet());
			yield return Wait(3);
		}
		base.BulletBank.aiAnimator.LockFacingDirection = false;
	}

	private string[] GetTransformNames()
	{
		Transform transform = base.BulletBank.transform.Find("bullet limbs").Find("back tip 1");
		if ((bool)transform && transform.gameObject.activeSelf)
		{
			return new string[2] { "bullet tip 1", "back tip 1" };
		}
		return new string[2] { "bullet tip 1", "bullet tip 2" };
	}
}
