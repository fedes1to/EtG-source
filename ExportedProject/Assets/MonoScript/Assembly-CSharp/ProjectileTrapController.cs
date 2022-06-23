using Dungeonator;
using UnityEngine;

public class ProjectileTrapController : BasicTrapController
{
	public ProjectileModule projectileModule;

	public ProjectileData overrideProjectileData;

	public DungeonData.Direction shootDirection;

	public VFXPool shootVfx;

	public Transform shootPoint;

	public override void Start()
	{
		base.Start();
		StaticReferenceManager.AllProjectileTraps.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		StaticReferenceManager.AllProjectileTraps.Remove(this);
	}

	public override GameObject InstantiateObject(RoomHandler targetRoom, IntVector2 loc, bool deferConfiguration)
	{
		return base.InstantiateObject(targetRoom, loc, deferConfiguration);
	}

	protected override void TriggerTrap(SpeculativeRigidbody target)
	{
		base.TriggerTrap(target);
		if (projectileModule.shootStyle == ProjectileModule.ShootStyle.Beam)
		{
			Debug.LogWarning("Unsupported shootstyle Beam.");
			return;
		}
		Vector2 vector = DungeonData.GetIntVector2FromDirection(shootDirection).ToVector2();
		ShootProjectileInDirection(shootPoint.position, vector);
		shootVfx.SpawnAtLocalPosition(Vector3.zero, vector.ToAngle(), shootPoint);
	}

	private void ShootProjectileInDirection(Vector3 spawnPosition, Vector2 direction)
	{
		AkSoundEngine.PostEvent("Play_TRP_bullet_shot_01", base.gameObject);
		float z = Mathf.Atan2(direction.y, direction.x) * 57.29578f;
		GameObject gameObject = SpawnManager.SpawnProjectile(projectileModule.GetCurrentProjectile().gameObject, spawnPosition, Quaternion.Euler(0f, 0f, z));
		Projectile component = gameObject.GetComponent<Projectile>();
		if (overrideProjectileData != null)
		{
			component.baseData.SetAll(overrideProjectileData);
		}
		component.Shooter = base.specRigidbody;
		component.TrapOwner = this;
	}
}
