using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Blobulord/MoveSpray1")]
public class BlobulordMoveSpray1 : Script
{
	private const float NumBullets = 30f;

	private const float ArcDegrees = 150f;

	protected override IEnumerator Top()
	{
		for (int i = 0; (float)i < 30f; i++)
		{
			Fire(new Direction(Random.Range(-75f, 75f), DirectionType.Aim), new Speed(12f), new Bullet("spew"));
			yield return Wait(3);
		}
	}
}
