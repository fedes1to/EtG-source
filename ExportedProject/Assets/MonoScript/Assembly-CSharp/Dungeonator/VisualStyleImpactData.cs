using System;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class VisualStyleImpactData
	{
		[SerializeField]
		public string annotation;

		[SerializeField]
		public GameObject[] wallShards;

		[SerializeField]
		public VFXComplex[] fallbackVerticalTileMapEffects;

		[SerializeField]
		public VFXComplex[] fallbackHorizontalTileMapEffects;

		public void SpawnRandomVertical(Vector3 position, float rotation, Transform enemy, Vector2 sourceNormal, Vector2 sourceVelocity)
		{
			VFXComplex vFXComplex = fallbackVerticalTileMapEffects[UnityEngine.Random.Range(0, fallbackVerticalTileMapEffects.Length)];
			float num = Mathf.FloorToInt(position.y) - 1;
			vFXComplex.SpawnAtPosition(position.x, num, position.y - num, rotation, null, sourceNormal, sourceVelocity);
		}

		public void SpawnRandomHorizontal(Vector3 position, float rotation, Transform enemy, Vector2 sourceNormal, Vector2 sourceVelocity)
		{
			VFXComplex vFXComplex = fallbackHorizontalTileMapEffects[UnityEngine.Random.Range(0, fallbackHorizontalTileMapEffects.Length)];
			vFXComplex.SpawnAtPosition(position, rotation, enemy, sourceNormal, sourceVelocity);
		}

		public void SpawnRandomShard(Vector3 position, Vector2 collisionNormal)
		{
			GameObject gameObject = wallShards[UnityEngine.Random.Range(0, wallShards.Length)];
			if (gameObject != null)
			{
				GameObject gameObject2 = SpawnManager.SpawnDebris(gameObject, position, Quaternion.identity);
				DebrisObject component = gameObject2.GetComponent<DebrisObject>();
				component.angularVelocity = UnityEngine.Random.Range(0.5f, 1.5f) * component.angularVelocity;
				float num = ((!(Mathf.Abs(collisionNormal.y) > 0.1f)) ? 0f : 0.25f);
				component.Trigger(Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-30, 30)) * collisionNormal.ToVector3ZUp() * UnityEngine.Random.Range(0f, 4f), UnityEngine.Random.Range(0.1f, 0.5f) + num);
			}
		}
	}
}
