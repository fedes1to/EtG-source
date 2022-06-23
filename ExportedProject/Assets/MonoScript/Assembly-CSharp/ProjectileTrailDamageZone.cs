using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTrailDamageZone : MonoBehaviour
{
	public float delayTime = 0.5f;

	public float additionalDestroyTime = 0.5f;

	public float damageToDeal = 5f;

	public bool AppliesFire;

	public GameActorFireEffect FireEffect;

	public void OnSpawned()
	{
		StartCoroutine(HandleSpawnBehavior());
	}

	public IEnumerator HandleSpawnBehavior()
	{
		float elapsed = 0f;
		while (elapsed < delayTime)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		List<SpeculativeRigidbody> overlaps = PhysicsEngine.Instance.GetOverlappingRigidbodies(GetComponent<SpeculativeRigidbody>());
		for (int i = 0; i < overlaps.Count; i++)
		{
			if (!overlaps[i])
			{
				continue;
			}
			AIActor component = overlaps[i].GetComponent<AIActor>();
			if ((bool)component && (bool)component.healthHaver)
			{
				component.healthHaver.ApplyDamage(damageToDeal, Vector2.zero, string.Empty, CoreDamageTypes.Fire);
				if (AppliesFire)
				{
					component.ApplyEffect(FireEffect);
				}
			}
		}
		yield return new WaitForSeconds(additionalDestroyTime);
		SpawnManager.Despawn(base.gameObject);
	}
}
