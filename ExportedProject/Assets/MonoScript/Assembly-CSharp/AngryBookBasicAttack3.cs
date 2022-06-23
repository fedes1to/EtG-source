using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("AngryBook/BasicAttack3")]
public class AngryBookBasicAttack3 : Script
{
	public class DefaultBullet : Bullet
	{
		public int spawnTime;

		public DefaultBullet(int spawnTime)
		{
			this.spawnTime = spawnTime;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(45 + spawnTime);
			ChangeDirection(new Direction(Mathf.Sin((float)spawnTime / 10f * (float)Math.PI) * 10f, DirectionType.Aim));
			ChangeSpeed(new Speed(12f));
		}
	}

	public int LineBullets = 6;

	public int EdgeBullets = 4;

	public int CircleBullets = 6;

	public int StemBullets = 6;

	public const float Height = 2f;

	public const float Width = 1.5f;

	public const float CircleRadius = 0.5f;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		int count = 0;
		for (int i = 0; i < LineBullets; i++)
		{
			Fire(new Offset(-0.75f, SubdivideRange(-1f, 1f, LineBullets, i), 0f, string.Empty), new DefaultBullet(count++));
			yield return Wait(1);
		}
		for (int j = 0; j < LineBullets; j++)
		{
			Fire(new Offset(SubdivideRange(-0.75f, 0.25f, EdgeBullets, j), 1f, 0f, string.Empty), new DefaultBullet(count++));
			yield return Wait(1);
		}
		for (int k = 0; k < CircleBullets; k++)
		{
			Fire(new Offset(new Vector2(0.25f, 0.5f) + new Vector2(0.5f, 0f).Rotate(SubdivideArc(90f, -180f, CircleBullets, k)), 0f, string.Empty), new DefaultBullet(count++));
			yield return Wait(1);
		}
		for (int l = 0; l < LineBullets; l++)
		{
			Fire(new Offset(SubdivideRange(0.25f, -0.75f, EdgeBullets, l), 0f, 0f, string.Empty), new DefaultBullet(count++));
			yield return Wait(1);
		}
		for (int m = 0; m < StemBullets; m++)
		{
			Fire(new Offset(SubdivideRange(0f, 0.75f, StemBullets, m), SubdivideRange(0f, -1f, StemBullets, m), 0f, string.Empty), new DefaultBullet(count++));
			yield return Wait(1);
		}
	}
}
