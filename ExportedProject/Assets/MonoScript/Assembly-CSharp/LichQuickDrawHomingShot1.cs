using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Lich/QuickDrawHomingShot1")]
public class LichQuickDrawHomingShot1 : Script
{
	public class FastHomingShot : Bullet
	{
		public FastHomingShot()
			: base("quickHoming")
		{
		}

		protected override IEnumerator Top()
		{
			for (int i = 0; i < 180; i++)
			{
				float aim = GetAimDirection(1f, 16f);
				float delta = BraveMathCollege.ClampAngle180(aim - Direction);
				if (Mathf.Abs(delta) > 100f)
				{
					break;
				}
				Direction += Mathf.MoveTowards(0f, delta, 3f);
				yield return Wait(1);
			}
		}
	}

	protected override IEnumerator Top()
	{
		float aimDirection = GetAimDirection(1f, 16f);
		Fire(new Direction(aimDirection), new Speed(16f), new FastHomingShot());
		return null;
	}
}
