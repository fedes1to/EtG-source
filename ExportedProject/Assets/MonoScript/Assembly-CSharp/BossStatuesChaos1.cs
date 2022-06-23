using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossStatues/Chaos1")]
public class BossStatuesChaos1 : Script
{
	public class EggBullet : Bullet
	{
		public EggBullet()
			: base("egg")
		{
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(7.5f), 120);
			yield return Wait(600);
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		Fire(new Offset("top 1"), new Direction(90f), new Speed(7.5f), new EggBullet());
		Fire(new Offset("top 1"), new Direction(90f), new Speed(6f), new EggBullet());
		Fire(new Offset("right 1"), new Direction(), new Speed(7.5f), new EggBullet());
		Fire(new Offset("right 1"), new Direction(), new Speed(6f), new EggBullet());
		Fire(new Offset("bottom 1"), new Direction(-90f), new Speed(7.5f), new EggBullet());
		Fire(new Offset("bottom 1"), new Direction(-90f), new Speed(6f), new EggBullet());
		Fire(new Offset("left 1"), new Direction(180f), new Speed(7.5f), new EggBullet());
		Fire(new Offset("left 1"), new Direction(180f), new Speed(6f), new EggBullet());
		AntiCornerShot(this);
		return null;
	}

	public static void AntiCornerShot(Script parentScript)
	{
		if (!(Random.value > 0.33f))
		{
			float aimDirection = parentScript.AimDirection;
			string transform = "top 1";
			switch (BraveMathCollege.AngleToQuadrant(aimDirection))
			{
			case 0:
				transform = "top 1";
				break;
			case 1:
				transform = "right 1";
				break;
			case 2:
				transform = "bottom 1";
				break;
			case 3:
				transform = "left 1";
				break;
			}
			parentScript.Fire(new Offset(transform), new Direction(aimDirection), new Speed(7.5f), new Bullet("egg"));
		}
	}
}
