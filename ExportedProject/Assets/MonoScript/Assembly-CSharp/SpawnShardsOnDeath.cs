using UnityEngine;

public class SpawnShardsOnDeath : OnDeathBehavior
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

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnTrigger(Vector2 deathVelocity)
	{
		HandleShardSpawns(deathVelocity);
	}

	public void HandleShardSpawns(Vector2 sourceVelocity, Vector2? spawnPos = null)
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
			SpawnShards(Vector2.right, -180f, 180f, num, 1f, 2f);
			break;
		case MinorBreakable.BreakStyle.CONE:
			SpawnShards(sourceVelocity, -45f, 45f, num, sourceVelocity.magnitude * 0.5f, sourceVelocity.magnitude * 1.5f);
			break;
		case MinorBreakable.BreakStyle.JET:
			SpawnShards(sourceVelocity, -15f, 15f, num, sourceVelocity.magnitude * 0.5f, sourceVelocity.magnitude * 1.5f);
			break;
		case MinorBreakable.BreakStyle.CUSTOM:
			SpawnShards(direction, minAngle, maxAngle, verticalSpeed, minMagnitude, maxMagnitude, spawnPos);
			break;
		}
	}

	public void SpawnShards(Vector2 direction, float minAngle, float maxAngle, float verticalSpeed, float minMagnitude, float maxMagnitude, Vector2? spawnPos = null)
	{
		Vector3 position = ((!spawnPos.HasValue) ? base.specRigidbody.GetUnitCenter(ColliderType.HitBox) : spawnPos.Value);
		if (shardClusters == null || shardClusters.Length <= 0)
		{
			return;
		}
		int num = Random.Range(0, 10);
		for (int i = 0; i < shardClusters.Length; i++)
		{
			ShardCluster shardCluster = shardClusters[i];
			int num2 = Random.Range(shardCluster.minFromCluster, shardCluster.maxFromCluster + 1);
			int num3 = Random.Range(0, shardCluster.clusterObjects.Length);
			for (int j = 0; j < num2; j++)
			{
				float lowDiscrepancyRandom = BraveMathCollege.GetLowDiscrepancyRandom(num);
				num++;
				float z = Mathf.Lerp(minAngle, maxAngle, lowDiscrepancyRandom);
				Vector3 a = Quaternion.Euler(0f, 0f, z) * (direction.normalized * Random.Range(minMagnitude, maxMagnitude)).ToVector3ZUp(verticalSpeed);
				int num4 = (num3 + j) % shardCluster.clusterObjects.Length;
				GameObject gameObject = SpawnManager.SpawnDebris(shardCluster.clusterObjects[num4].gameObject, position, Quaternion.identity);
				tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
				if (base.sprite.attachParent != null && component != null)
				{
					component.attachParent = base.sprite.attachParent;
					component.HeightOffGround = base.sprite.HeightOffGround;
				}
				DebrisObject component2 = gameObject.GetComponent<DebrisObject>();
				a = Vector3.Scale(a, shardCluster.forceAxialMultiplier) * shardCluster.forceMultiplier;
				component2.Trigger(a, heightOffGround, shardCluster.rotationMultiplier);
			}
		}
	}
}
