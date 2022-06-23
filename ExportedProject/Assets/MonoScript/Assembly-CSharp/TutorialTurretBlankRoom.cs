using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("TutorialTurret/BlankRoom")]
public class TutorialTurretBlankRoom : Script
{
	public class WarpBullet : Bullet
	{
		private bool m_doWarp;

		public WarpBullet(bool doWarp)
		{
			m_doWarp = doWarp;
		}

		protected override IEnumerator Top()
		{
			if (m_doWarp)
			{
				base.Position += new Vector2(-0.75f, 0f);
			}
			base.Position = base.Position.WithY(BraveMathCollege.QuantizeFloat(base.Position.y, 0.0625f));
			return null;
		}
	}

	public int CircleBullets = 20;

	public int LineBullets = 12;

	protected override IEnumerator Top()
	{
		bool doWarp = Mathf.Abs(base.Position.y % 1f) > 0.5f;
		yield return Wait(20);
		while (base.BulletBank.aiActor.enabled)
		{
			Fire(new Direction(180f), new Speed(6f), new WarpBullet(doWarp));
			yield return Wait(15);
		}
	}
}
