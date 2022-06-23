using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("BulletManMagic/Astral1")]
public class BulletManMagicAstral1 : Script
{
	public class AstralBullet : Bullet
	{
		private Script m_parentScript;

		public AstralBullet(Script parentScript)
			: base("astral")
		{
			m_parentScript = parentScript;
		}

		protected override IEnumerator Top()
		{
			HealthHaver owner = base.BulletBank.healthHaver;
			for (int i = 0; i < 180; i++)
			{
				ChangeDirection(new Direction(0f, DirectionType.Aim, 3f));
				if (!owner || owner.IsDead)
				{
					Vanish();
					yield break;
				}
				yield return Wait(1);
			}
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (m_parentScript != null)
			{
				m_parentScript.ForceEnd();
			}
		}
	}

	private const int AirTime = 180;

	protected override IEnumerator Top()
	{
		Fire(new Direction(0f, DirectionType.Aim), new Speed(7f), new AstralBullet(this));
		yield return Wait(180);
	}
}
