using UnityEngine;

public class SpawnShardsArbitrarily : BraveBehaviour
{
	public MinorBreakable.BreakStyle breakStyle;

	public ShardCluster[] shardClusters;

	public float heightOffGround = 0.1f;

	public bool TriggerOnDestroy;

	public bool TriggerOnDamaged;

	private void Start()
	{
		if (TriggerOnDamaged)
		{
			base.healthHaver.OnDamaged += HandleDamaged;
		}
	}

	private void HandleDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		HandleShardSpawns(damageDirection.normalized);
	}

	protected override void OnDestroy()
	{
		if (TriggerOnDestroy)
		{
			HandleShardSpawns(Vector2.zero);
		}
		base.OnDestroy();
	}

	private void HandleShardSpawns(Vector2 sourceVelocity)
	{
		MinorBreakable.BreakStyle breakStyle = this.breakStyle;
		if (sourceVelocity == Vector2.zero)
		{
			breakStyle = MinorBreakable.BreakStyle.BURST;
		}
		float verticalSpeed = 1.5f;
		switch (breakStyle)
		{
		case MinorBreakable.BreakStyle.BURST:
			SpawnShards(Vector2.right, -180f, 180f, verticalSpeed, 1f, 2f);
			break;
		case MinorBreakable.BreakStyle.CONE:
			SpawnShards(sourceVelocity, -45f, 45f, verticalSpeed, sourceVelocity.magnitude * 0.5f, sourceVelocity.magnitude * 1.5f);
			break;
		case MinorBreakable.BreakStyle.JET:
			SpawnShards(sourceVelocity, -15f, 15f, verticalSpeed, sourceVelocity.magnitude * 0.5f, sourceVelocity.magnitude * 1.5f);
			break;
		}
	}

	public void SpawnShards(Vector2 direction, float minAngle, float maxAngle, float verticalSpeed, float minMagnitude, float maxMagnitude)
	{
		Vector3 position = (base.specRigidbody ? base.specRigidbody.GetUnitCenter(ColliderType.HitBox).ToVector3ZisY() : ((!base.sprite) ? base.transform.position : base.sprite.WorldCenter.ToVector3ZisY()));
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
				if ((bool)gameObject)
				{
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
}
