using System;
using UnityEngine;

[Serializable]
public class ShardCluster
{
	public int minFromCluster = 1;

	public int maxFromCluster = 3;

	public float forceMultiplier = 1f;

	public Vector3 forceAxialMultiplier = Vector3.one;

	public float rotationMultiplier = 1f;

	public DebrisObject[] clusterObjects;

	public void SpawnShards(Vector2 position, Vector2 direction, float minAngle, float maxAngle, float verticalSpeed, float minMagnitude, float maxMagnitude, tk2dSprite sourceSprite)
	{
		int num = UnityEngine.Random.Range(minFromCluster, maxFromCluster + 1);
		int num2 = UnityEngine.Random.Range(0, clusterObjects.Length);
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			float lowDiscrepancyRandom = BraveMathCollege.GetLowDiscrepancyRandom(num3);
			num3++;
			float z = Mathf.Lerp(minAngle, maxAngle, lowDiscrepancyRandom);
			Vector3 a = Quaternion.Euler(0f, 0f, z) * (direction.normalized * UnityEngine.Random.Range(minMagnitude, maxMagnitude)).ToVector3ZUp(verticalSpeed);
			int num4 = (num2 + i) % clusterObjects.Length;
			GameObject gameObject = SpawnManager.SpawnDebris(clusterObjects[num4].gameObject, position, Quaternion.identity);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			if (sourceSprite != null && sourceSprite.attachParent != null && component != null)
			{
				component.attachParent = sourceSprite.attachParent;
				component.HeightOffGround = sourceSprite.HeightOffGround;
			}
			DebrisObject component2 = gameObject.GetComponent<DebrisObject>();
			a = Vector3.Scale(a, forceAxialMultiplier) * forceMultiplier;
			component2.Trigger(a, 0.5f, rotationMultiplier);
		}
	}
}
