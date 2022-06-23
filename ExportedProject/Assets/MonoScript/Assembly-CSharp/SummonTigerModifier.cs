using System;
using Dungeonator;
using UnityEngine;

public class SummonTigerModifier : BraveBehaviour
{
	public Projectile TigerProjectilePrefab;

	private bool m_hasSummonedTiger;

	private void Start()
	{
		Projectile obj = base.projectile;
		obj.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(obj.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
		base.projectile.OnDestruction += HandleDestruction;
	}

	private void HandleDestruction(Projectile source)
	{
		if (!m_hasSummonedTiger)
		{
			SummonTiger(null);
		}
	}

	private void HandleHitEnemy(Projectile arg1, SpeculativeRigidbody arg2, bool arg3)
	{
		if (!m_hasSummonedTiger)
		{
			SummonTiger(arg2);
		}
	}

	private void SummonTiger(SpeculativeRigidbody optionalTarget)
	{
		m_hasSummonedTiger = true;
		RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
		Vector2? idealPosition = null;
		if (optionalTarget != null)
		{
			idealPosition = optionalTarget.UnitCenter;
		}
		IntVector2 value = new IntVector2(4, 2);
		if ((bool)base.sprite)
		{
			value = Vector2.Scale(new Vector2(4f, 2f), base.sprite.scale.XY()).ToIntVector2(VectorConversions.Ceil);
		}
		IntVector2? intVector = absoluteRoomFromPosition.GetOffscreenCell(value, CellTypes.FLOOR, false, idealPosition);
		if (!intVector.HasValue)
		{
			intVector = absoluteRoomFromPosition.GetRandomAvailableCell(value, CellTypes.FLOOR);
		}
		if (intVector.HasValue)
		{
			if (optionalTarget != null)
			{
				ShootSingleProjectile(intVector.Value.ToVector2(), BraveMathCollege.Atan2Degrees(optionalTarget.UnitCenter - intVector.Value.ToVector2()));
			}
			else
			{
				ShootSingleProjectile(intVector.Value.ToVector2(), BraveMathCollege.Atan2Degrees(absoluteRoomFromPosition.GetCenterCell().ToVector2() - intVector.Value.ToVector2()));
			}
		}
	}

	private void ShootSingleProjectile(Vector2 spawnPosition, float angle)
	{
		GameObject gameObject = SpawnManager.SpawnProjectile(TigerProjectilePrefab.gameObject, spawnPosition.ToVector3ZUp(spawnPosition.y), Quaternion.Euler(0f, 0f, angle));
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = base.projectile.Owner;
		component.Shooter = component.Owner.specRigidbody;
		if (component.Owner is PlayerController)
		{
			PlayerStats stats = (component.Owner as PlayerController).stats;
			component.baseData.damage *= stats.GetStatValue(PlayerStats.StatType.Damage);
			component.baseData.speed *= stats.GetStatValue(PlayerStats.StatType.ProjectileSpeed);
			component.baseData.force *= stats.GetStatValue(PlayerStats.StatType.KnockbackMultiplier);
			(component.Owner as PlayerController).DoPostProcessProjectile(component);
		}
	}
}
