using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class SpectreGroupShot : Script
{
	private const int NumBullets = 4;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 4; i++)
		{
			yield return Wait(30);
			FireFrom("eyes " + Random.Range(1, 4));
		}
	}

	private void FireFrom(string transform)
	{
		float aimDirection = GetAimDirection(transform, Random.Range(0, 2), 8f);
		Vector2 vector = PhysicsEngine.PixelToUnit(new IntVector2(4, 0));
		Vector2 offset = vector;
		string transform2 = transform;
		Fire(new Offset(offset, 0f, transform2), new Direction(aimDirection), new Speed(8f), new Bullet("eyeBullet"));
		offset = -vector;
		transform2 = transform;
		Fire(new Offset(offset, 0f, transform2), new Direction(aimDirection), new Speed(8f), new Bullet("eyeBullet"));
	}
}
