using System;
using System.Collections.Generic;
using UnityEngine;

public class CorpseExplodeActiveItem : PlayerItem
{
	public ScreenShakeSettings ScreenShake;

	public ExplosionData CorpseExplosionData;

	public bool UsesCrisisStoneSynergy;

	public GameObject ShieldForCrisisStoneSynergy;

	private AIBulletBank m_bulletBank;

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		m_bulletBank = GetComponent<AIBulletBank>();
		if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
		{
			PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
		}
		if (!PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player].Add(GetType(), 1);
		}
		else
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = PassiveItem.ActiveFlagItems[player][GetType()] + 1;
		}
	}

	protected override void OnPreDrop(PlayerController player)
	{
		base.OnPreDrop(player);
		if (PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
	}

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_dead_again_01", base.gameObject);
		bool flag = false;
		for (int i = 0; i < StaticReferenceManager.AllCorpses.Count; i++)
		{
			GameObject gameObject = StaticReferenceManager.AllCorpses[i];
			if (!gameObject || !gameObject.GetComponent<tk2dBaseSprite>() || gameObject.transform.position.GetAbsoluteRoom() != user.CurrentRoom)
			{
				continue;
			}
			flag = true;
			Vector2 worldCenter = gameObject.GetComponent<tk2dBaseSprite>().WorldCenter;
			Exploder.Explode(worldCenter, CorpseExplosionData, Vector2.zero, null, true);
			if (user.HasActiveBonusSynergy(CustomSynergyType.CORPSE_EXPLOSHOOT))
			{
				float nearestDistance = -1f;
				AIActor nearestEnemy = user.CurrentRoom.GetNearestEnemy(worldCenter, out nearestDistance);
				if ((bool)nearestEnemy)
				{
					FireBullet(worldCenter, nearestEnemy.CenterPosition - worldCenter);
				}
			}
			if (user.HasActiveBonusSynergy(CustomSynergyType.CRISIS_ROCK))
			{
				UnityEngine.Object.Instantiate(ShieldForCrisisStoneSynergy, worldCenter, Quaternion.identity);
			}
			UnityEngine.Object.Destroy(gameObject.gameObject);
		}
		if (flag)
		{
			GameManager.Instance.MainCameraController.DoScreenShake(ScreenShake, user.CenterPosition);
		}
		else
		{
			ClearCooldowns();
		}
	}

	protected override void OnDestroy()
	{
		if (m_pickedUp && (bool)LastOwner && PassiveItem.ActiveFlagItems != null && PassiveItem.ActiveFlagItems.ContainsKey(LastOwner) && PassiveItem.ActiveFlagItems[LastOwner].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[LastOwner][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[LastOwner][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[LastOwner][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[LastOwner].Remove(GetType());
			}
		}
		base.OnDestroy();
	}

	private void FireBullet(Vector3 shootPoint, Vector2 direction)
	{
		GameObject gameObject = m_bulletBank.CreateProjectileFromBank(shootPoint, BraveMathCollege.Atan2Degrees(direction), "default");
		Projectile component = gameObject.GetComponent<Projectile>();
		if ((bool)component && (bool)LastOwner)
		{
			component.collidesWithPlayer = false;
			component.collidesWithEnemies = true;
			component.SetOwnerSafe(LastOwner, LastOwner.ActorName);
			component.SetNewShooter(LastOwner.specRigidbody);
			component.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyBulletBlocker));
		}
	}
}
