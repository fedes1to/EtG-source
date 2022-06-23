using System;
using Dungeonator;
using UnityEngine;

public class ModifyProjectileOnEnemyImpact : PassiveItem
{
	public bool ApplyRandomBounceOffEnemy = true;

	[ShowInInspectorIf("ApplyRandomBounceOffEnemy", false)]
	public float ChanceToSeekEnemyOnBounce = 0.5f;

	public bool NormalizeAcrossFireRate;

	[ShowInInspectorIf("NormalizeAcrossFireRate", false)]
	public float ActivationsPerSecond = 1f;

	[ShowInInspectorIf("NormalizeAcrossFireRate", false)]
	public float MinActivationChance = 0.05f;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
		}
	}

	private void PostProcessProjectile(Projectile p, float effectChanceScalar)
	{
		p.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(p.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleProjectileHitEnemy));
	}

	private void HandleProjectileHitEnemy(Projectile obj, SpeculativeRigidbody enemy, bool killed)
	{
		if (!ApplyRandomBounceOffEnemy)
		{
			return;
		}
		PierceProjModifier orAddComponent = obj.gameObject.GetOrAddComponent<PierceProjModifier>();
		orAddComponent.penetratesBreakables = true;
		orAddComponent.penetration++;
		HomingModifier component = obj.gameObject.GetComponent<HomingModifier>();
		if ((bool)component)
		{
			component.AngularVelocity *= 0.75f;
		}
		Vector2 dirVec = UnityEngine.Random.insideUnitCircle;
		float num = ChanceToSeekEnemyOnBounce;
		Gun possibleSourceGun = obj.PossibleSourceGun;
		if (NormalizeAcrossFireRate && (bool)possibleSourceGun)
		{
			float num2 = 1f / possibleSourceGun.DefaultModule.cooldownTime;
			if (possibleSourceGun.Volley != null && possibleSourceGun.Volley.UsesShotgunStyleVelocityRandomizer)
			{
				num2 *= (float)Mathf.Max(1, possibleSourceGun.Volley.projectiles.Count);
			}
			num = Mathf.Clamp01(ActivationsPerSecond / num2);
			num = Mathf.Max(MinActivationChance, num);
		}
		if (UnityEngine.Random.value < num && (bool)enemy.aiActor)
		{
			Func<AIActor, bool> isValid = (AIActor a) => (bool)a && a.HasBeenEngaged && (bool)a.healthHaver && a.healthHaver.IsVulnerable;
			AIActor closestToPosition = BraveUtility.GetClosestToPosition(enemy.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All), enemy.UnitCenter, isValid, enemy.aiActor);
			if ((bool)closestToPosition)
			{
				dirVec = closestToPosition.CenterPosition - obj.transform.position.XY();
			}
		}
		obj.SendInDirection(dirVec, false);
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<ModifyProjectileOnEnemyImpact>().m_pickedUpThisRun = true;
		player.PostProcessProjectile -= PostProcessProjectile;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
		}
	}
}
