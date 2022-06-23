using System.Collections;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("ManfredsRival/ShieldThrow1")]
public class ManfredsRivalShieldThrow1 : Script
{
	public class ShieldBullet : Bullet
	{
		private Vector2 m_endOffset;

		public ShieldBullet(Vector2 endOffset)
			: base("shield")
		{
			m_endOffset = endOffset;
			base.SuppressVfx = true;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			yield return Wait(70);
			Vector2 start = base.Position;
			Vector2 end = base.Position + m_endOffset;
			base.ManualControl = true;
			for (int i = 0; i < 90; i++)
			{
				float t = (float)(i + 1) / 90f;
				base.Position = new Vector2(Mathf.SmoothStep(start.x, end.x, t), Mathf.SmoothStep(start.y, end.y, t));
				yield return Wait(1);
			}
			yield return Wait(480);
			Vanish();
		}
	}

	private const int WaitTime = 70;

	private const int TravelTime = 90;

	private const int HoldTime = 480;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		RoomHandler room = base.BulletBank.aiActor.ParentRoom;
		Vector2 leftPos = BraveUtility.RandomVector2(new Vector2(room.area.UnitLeft + 2f, room.area.UnitBottom + 2f), new Vector2(room.area.UnitCenter.x - 2f, room.area.UnitTop - 2f));
		Vector2 rightPos = BraveUtility.RandomVector2(new Vector2(room.area.UnitCenter.x + 2f, room.area.UnitBottom + 2f), new Vector2(room.area.UnitRight - 2f, room.area.UnitTop - 2f));
		FireShield(leftPos - base.Position);
		FireShield(new Vector2(0f, 0f));
		FireShield(rightPos - base.Position);
		FireShield(BulletManager.PlayerPosition() - base.Position);
		yield return Wait(160);
	}

	private void FireShield(Vector2 endOffset)
	{
		FireExpandingLine(new Vector2(-0.5f, -1f), new Vector2(0.5f, -1f), 4, endOffset);
		FireExpandingLine(new Vector2(-0.8f, -0.7f), new Vector2(-0.8f, 0.2f), 4, endOffset);
		FireExpandingLine(new Vector2(0.8f, -0.7f), new Vector2(0.8f, 0.2f), 4, endOffset);
		FireExpandingLine(new Vector2(-0.8f, 0.2f), new Vector2(-0.15f, 1f), 4, endOffset);
		FireExpandingLine(new Vector2(0.8f, 0.2f), new Vector2(0.15f, 1f), 4, endOffset);
	}

	private void FireExpandingLine(Vector2 start, Vector2 end, int numBullets, Vector2 endOffset)
	{
		start *= 0.5f;
		end *= 0.5f;
		for (int i = 0; i < numBullets; i++)
		{
			float t = ((numBullets > 1) ? ((float)i / ((float)numBullets - 1f)) : 0.5f);
			Vector2 vector = Vector2.Lerp(start, end, t);
			vector.y *= -1f;
			Fire(new Offset(vector * 4f, 0f, string.Empty), new Direction(vector.ToAngle()), new ShieldBullet(endOffset));
		}
	}
}
