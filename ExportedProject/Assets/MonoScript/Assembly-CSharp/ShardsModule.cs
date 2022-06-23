using System;
using UnityEngine;

[Serializable]
public class ShardsModule
{
	public MinorBreakable.BreakStyle breakStyle;

	[ShowInInspectorIf("breakStyle", 4, true)]
	public Vector2 direction;

	[ShowInInspectorIf("breakStyle", 4, true)]
	public float minAngle;

	[ShowInInspectorIf("breakStyle", 4, true)]
	public float maxAngle;

	[ShowInInspectorIf("breakStyle", 4, true)]
	public float verticalSpeed;

	[ShowInInspectorIf("breakStyle", 4, true)]
	public float minMagnitude;

	[ShowInInspectorIf("breakStyle", 4, true)]
	public float maxMagnitude;

	public ShardCluster[] shardClusters;

	public float heightOffGround = 0.1f;

	public void HandleShardSpawns(Vector2 spawnPos, Vector2 sourceVelocity)
	{
		MinorBreakable.BreakStyle breakStyle = this.breakStyle;
		if (sourceVelocity == Vector2.zero && this.breakStyle != MinorBreakable.BreakStyle.CUSTOM)
		{
			breakStyle = MinorBreakable.BreakStyle.BURST;
		}
		float num = 1.5f;
		switch (breakStyle)
		{
		case MinorBreakable.BreakStyle.BURST:
			SpawnShards(spawnPos, Vector2.right, -180f, 180f, num, 1f, 2f);
			break;
		case MinorBreakable.BreakStyle.CONE:
			SpawnShards(spawnPos, sourceVelocity, -45f, 45f, num, sourceVelocity.magnitude * 0.5f, sourceVelocity.magnitude * 1.5f);
			break;
		case MinorBreakable.BreakStyle.JET:
			SpawnShards(spawnPos, sourceVelocity, -15f, 15f, num, sourceVelocity.magnitude * 0.5f, sourceVelocity.magnitude * 1.5f);
			break;
		case MinorBreakable.BreakStyle.CUSTOM:
			SpawnShards(spawnPos, direction, minAngle, maxAngle, verticalSpeed, minMagnitude, maxMagnitude);
			break;
		}
	}

	private void SpawnShards(Vector2 spawnPos, Vector2 direction, float minAngle, float maxAngle, float verticalSpeed, float minMagnitude, float maxMagnitude)
	{
		Vector3 position = spawnPos;
		if (shardClusters == null || shardClusters.Length <= 0)
		{
			return;
		}
		int num = UnityEngine.Random.Range(0, 10);
		for (int i = 0; i < shardClusters.Length; i++)
		{
			ShardCluster shardCluster = shardClusters[i];
			int num2 = UnityEngine.Random.Range(shardCluster.minFromCluster, shardCluster.maxFromCluster + 1);
			int num3 = UnityEngine.Random.Range(0, shardCluster.clusterObjects.Length);
			for (int j = 0; j < num2; j++)
			{
				float lowDiscrepancyRandom = BraveMathCollege.GetLowDiscrepancyRandom(num);
				num++;
				float z = Mathf.Lerp(minAngle, maxAngle, lowDiscrepancyRandom);
				Vector3 a = Quaternion.Euler(0f, 0f, z) * (direction.normalized * UnityEngine.Random.Range(minMagnitude, maxMagnitude)).ToVector3ZUp(verticalSpeed);
				int num4 = (num3 + j) % shardCluster.clusterObjects.Length;
				GameObject gameObject = SpawnManager.SpawnDebris(shardCluster.clusterObjects[num4].gameObject, position, Quaternion.identity);
				DebrisObject component = gameObject.GetComponent<DebrisObject>();
				a = Vector3.Scale(a, shardCluster.forceAxialMultiplier) * shardCluster.forceMultiplier;
				component.Trigger(a, heightOffGround, shardCluster.rotationMultiplier);
			}
		}
	}
}
