using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Bashellisk/NibblesBullets1")]
public class BashelliskNibblesBullets1 : Script
{
	public class NibblesBullet : Bullet
	{
		private int delay;

		private NibblesBullet parent;

		private NibblesBullet child;

		private float prevDirection;

		private Vector2 prevPosition;

		private int turnCooldown;

		public NibblesBullet(int delay, NibblesBullet parent)
			: base("nibblesBullet")
		{
			this.delay = delay;
			this.parent = parent;
			if (parent != null)
			{
				parent.child = this;
			}
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			if (parent != null)
			{
				while (parent != null && !parent.Destroyed)
				{
					yield return Wait(1);
				}
				Vanish();
				yield break;
			}
			while (child == null)
			{
				yield return Wait(1);
			}
			if (delay > 0)
			{
				yield return Wait(delay * 3);
			}
			int preTurnTime = -1;
			turnCooldown = 8 - delay;
			for (int i = 0; i < 120; i++)
			{
				if (turnCooldown == 0 && preTurnTime < 0 && Random.value < 0.07f)
				{
					preTurnTime = 20;
					Projectile.spriteAnimator.Play();
				}
				if (preTurnTime >= 0)
				{
					preTurnTime--;
					if (preTurnTime <= 0)
					{
						prevDirection = Direction;
						Direction += BraveUtility.RandomSign() * 90f;
						turnCooldown = 201;
						preTurnTime = -1;
						Projectile.spriteAnimator.StopAndResetFrameToDefault();
					}
				}
				prevPosition = base.Position;
				base.Position += BraveMathCollege.DegreesToVector(Direction, Speed / 60f * 3f);
				NibblesBullet ptr = this;
				while (ptr.child != null && ptr.delay <= i)
				{
					ptr.child.prevDirection = ptr.child.Direction;
					ptr.child.Direction = ptr.prevDirection;
					ptr.child.prevPosition = ptr.child.Position;
					ptr.child.Position = ptr.prevPosition;
					ptr = ptr.child;
				}
				if (turnCooldown > 0)
				{
					turnCooldown--;
				}
				yield return Wait(3);
			}
			Vanish();
		}
	}

	private const int NumBullets = 8;

	private const int BulletSpeed = 12;

	private const int NibblesTickTime = 3;

	private const int NibblesTurnCooldown = 200;

	private const float NibblesTurnChance = 0.07f;

	protected override IEnumerator Top()
	{
		float input = BraveMathCollege.QuantizeFloat(GetAimDirection(1f, 12f), 90f);
		NibblesBullet parent = null;
		for (int i = 0; i < 8; i++)
		{
			NibblesBullet nibblesBullet = new NibblesBullet(i, parent);
			Fire(new Direction(BraveMathCollege.QuantizeFloat(input, 90f)), new Speed(12f), nibblesBullet);
			parent = nibblesBullet;
		}
		return null;
	}
}
