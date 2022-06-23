using System;
using System.Collections.Generic;
using UnityEngine;

public class DevolverModifier : MonoBehaviour
{
	public float chanceToDevolve = 0.1f;

	public List<DevolverTier> DevolverHierarchy = new List<DevolverTier>();

	public List<string> EnemyGuidsToIgnore = new List<string>();

	private void Start()
	{
		Projectile component = GetComponent<Projectile>();
		if ((bool)component)
		{
			component.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(component.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
		}
	}

	private void HandleHitEnemy(Projectile sourceProjectile, SpeculativeRigidbody enemyRigidbody, bool killingBlow)
	{
		if (killingBlow || !enemyRigidbody || !enemyRigidbody.aiActor || UnityEngine.Random.value > chanceToDevolve)
		{
			return;
		}
		AIActor aiActor = enemyRigidbody.aiActor;
		if (!aiActor.IsNormalEnemy || aiActor.IsHarmlessEnemy || aiActor.healthHaver.IsBoss)
		{
			return;
		}
		string enemyGuid = aiActor.EnemyGuid;
		for (int i = 0; i < EnemyGuidsToIgnore.Count; i++)
		{
			if (EnemyGuidsToIgnore[i] == enemyGuid)
			{
				return;
			}
		}
		int num = DevolverHierarchy.Count - 1;
		for (int j = 0; j < DevolverHierarchy.Count; j++)
		{
			List<string> tierGuids = DevolverHierarchy[j].tierGuids;
			for (int k = 0; k < tierGuids.Count; k++)
			{
				if (tierGuids[k] == enemyGuid)
				{
					num = j - 1;
					break;
				}
			}
		}
		if (num >= 0 && num < DevolverHierarchy.Count)
		{
			List<string> tierGuids2 = DevolverHierarchy[num].tierGuids;
			string guid = tierGuids2[UnityEngine.Random.Range(0, tierGuids2.Count)];
			aiActor.Transmogrify(EnemyDatabase.GetOrLoadByGuid(guid), (GameObject)ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
			AkSoundEngine.PostEvent("Play_WPN_devolver_morph_01", base.gameObject);
		}
	}
}
