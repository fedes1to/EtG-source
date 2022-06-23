using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/SpinBeams1")]
public class InfinilichSpinBeams1 : Script
{
	public class BeamBullet : Bullet
	{
		private int m_spawnDelay;

		public BeamBullet(int spawnDelay)
		{
			m_spawnDelay = spawnDelay;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(12 - m_spawnDelay);
			Vanish();
		}
	}

	private const int TurnSpeed = 3;

	private const int TurnDelay = 3;

	private const int BeamSetupTime = 7;

	private const int BeamLifeTime = 12;

	protected override IEnumerator Top()
	{
		while (true)
		{
			yield return Wait(3);
			StartTask(FireBeam(base.Position + new Vector2(1f, 0f), 0f));
			yield return Wait(9);
			StartTask(FireBeam(base.Position + new Vector2(-1f, 0f), 180f));
			yield return Wait(6);
		}
	}

	private IEnumerator FireBeam(Vector2 pos, float direction)
	{
		AkSoundEngine.PostEvent("Play_BOSS_lichC_zap_01", base.BulletBank.gameObject);
		for (int i = 0; i < 7; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				float x = (i * 3 + j) * ((!(direction > 90f)) ? 1 : (-1));
				Fire(Offset.OverridePosition(pos + new Vector2(x, 0.0625f)), new Direction(direction), new Speed(), new BeamBullet(i));
				Fire(Offset.OverridePosition(pos + new Vector2(x, -0.375f)), new Direction(direction), new Speed(), new BeamBullet(i));
			}
			yield return Wait(1);
		}
	}
}
