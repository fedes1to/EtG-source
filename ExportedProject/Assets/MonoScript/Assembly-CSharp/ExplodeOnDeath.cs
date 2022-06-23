using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ExplodeOnDeath : OnDeathBehavior
{
	public ExplosionData explosionData;

	public bool immuneToIBombApp;

	public bool LinearChainExplosion;

	public float ChainDuration = 1f;

	public float ChainDistance = 10f;

	public int ChainNumExplosions = 5;

	public bool ChainIsReversed;

	public GameObject ChainTargetSprite;

	public ExplosionData LinearChainExplosionData;

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnTrigger(Vector2 dirVec)
	{
		if (base.enabled)
		{
			Exploder.Explode(base.specRigidbody.GetUnitCenter(ColliderType.HitBox), explosionData, Vector2.zero);
			if (LinearChainExplosion)
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleChainExplosion());
			}
		}
	}

	public IEnumerator HandleChainExplosion()
	{
		Vector2 startPosition = base.behaviorSpeculator.aiActor.CenterPosition;
		Vector2 endPosition2 = ((!base.behaviorSpeculator.aiActor.TargetRigidbody) ? base.behaviorSpeculator.aiActor.AimCenter : base.behaviorSpeculator.aiActor.TargetRigidbody.UnitCenter);
		Vector2 dir = (endPosition2 - startPosition).normalized;
		if (ChainIsReversed)
		{
			dir = dir.Rotate(180f);
		}
		endPosition2 = startPosition + dir * ChainDistance;
		float perExplosionTime = ChainDuration / (float)(ChainNumExplosions + 3);
		float[] explosionTimes = new float[ChainNumExplosions];
		explosionTimes[0] = perExplosionTime * 3f;
		explosionTimes[1] = perExplosionTime * 5f;
		for (int i = 2; i < ChainNumExplosions; i++)
		{
			explosionTimes[i] = explosionTimes[i - 1] + perExplosionTime;
		}
		Vector2 lastValidPosition2 = startPosition;
		bool hitWall2 = false;
		List<GameObject> landingTargets = null;
		if ((bool)ChainTargetSprite)
		{
			landingTargets = new List<GameObject>(ChainNumExplosions);
			for (int j = 0; j < ChainNumExplosions; j++)
			{
				Vector2 vector = Vector2.Lerp(startPosition, endPosition2, (float)(j + 1) / (float)ChainNumExplosions);
				if (!ValidExplosionPosition(vector))
				{
					hitWall2 = true;
				}
				if (!hitWall2)
				{
					lastValidPosition2 = vector;
				}
				GameObject gameObject = SpawnManager.SpawnVFX(ChainTargetSprite, lastValidPosition2, Quaternion.identity);
				gameObject.GetComponentInChildren<tk2dSprite>().UpdateZDepth();
				tk2dSpriteAnimator componentInChildren = gameObject.GetComponentInChildren<tk2dSpriteAnimator>();
				float num = explosionTimes[j];
				componentInChildren.Play(componentInChildren.DefaultClip, 0f, (float)componentInChildren.DefaultClip.frames.Length / num);
				landingTargets.Add(gameObject);
			}
		}
		int index = 0;
		float elapsed = 0f;
		lastValidPosition2 = startPosition;
		hitWall2 = false;
		while (elapsed < ChainDuration)
		{
			for (elapsed += BraveTime.DeltaTime; index < ChainNumExplosions && elapsed >= explosionTimes[index]; index++)
			{
				Vector2 vector2 = Vector2.Lerp(startPosition, endPosition2, ((float)index + 1f) / (float)ChainNumExplosions);
				if (!ValidExplosionPosition(vector2))
				{
					hitWall2 = true;
				}
				if (!hitWall2)
				{
					lastValidPosition2 = vector2;
				}
				Exploder.Explode(lastValidPosition2, LinearChainExplosionData, Vector2.zero);
				if (landingTargets != null && landingTargets.Count > 0)
				{
					SpawnManager.Despawn(landingTargets[0]);
					landingTargets.RemoveAt(0);
				}
			}
			yield return null;
		}
		if (landingTargets != null && landingTargets.Count > 0)
		{
			for (int k = 0; k < landingTargets.Count; k++)
			{
				SpawnManager.Despawn(landingTargets[k]);
			}
		}
	}

	private bool ValidExplosionPosition(Vector2 pos)
	{
		IntVector2 intVector = pos.ToIntVector2(VectorConversions.Floor);
		return GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector) && GameManager.Instance.Dungeon.data[intVector].type != CellType.WALL;
	}
}
