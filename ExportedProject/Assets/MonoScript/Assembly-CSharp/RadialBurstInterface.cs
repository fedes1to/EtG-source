using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

[Serializable]
public class RadialBurstInterface
{
	public PlayerItemProjectileInterface ProjectileInterface;

	public int MinToSpawnPerWave = 10;

	public int MaxToSpawnPerWave = 10;

	public int NumberWaves = 1;

	public int NumberSubwaves = 1;

	public float TimeBetweenWaves = 1f;

	public bool SpiralWaves;

	public bool AlignFirstShot;

	public float AlignOffset;

	public bool SweepBeams;

	public float BeamSweepDegrees = 360f;

	public bool AimFirstAtNearestEnemy;

	public bool FixOverlapCollision;

	public bool ForceAllowGoop;

	public Action<Projectile> CustomPostProcessProjectile;

	public void DoBurst(PlayerController source, Vector2? overrideSpawnPoint = null, Vector2? spawnPointOffset = null)
	{
		if (NumberWaves == 1 && !SpiralWaves)
		{
			ImmediateBurst(ProjectileInterface.GetProjectile(source), source, overrideSpawnPoint, spawnPointOffset);
		}
		else
		{
			source.StartCoroutine(HandleBurst(ProjectileInterface.GetProjectile(source), source, overrideSpawnPoint, spawnPointOffset));
		}
	}

	private AIActor GetNearestEnemy(Vector2 sourcePoint)
	{
		RoomHandler absoluteRoom = sourcePoint.GetAbsoluteRoom();
		float nearestDistance = 0f;
		return absoluteRoom.GetNearestEnemy(sourcePoint, out nearestDistance, true, true);
	}

	private void ImmediateBurst(Projectile projectileToSpawn, PlayerController source, Vector2? overrideSpawnPoint, Vector2? spawnPointOffset = null)
	{
		if (projectileToSpawn == null)
		{
			return;
		}
		int num = UnityEngine.Random.Range(MinToSpawnPerWave, MaxToSpawnPerWave);
		int radialBurstLimit = projectileToSpawn.GetRadialBurstLimit(source);
		if (radialBurstLimit < num)
		{
			num = radialBurstLimit;
		}
		float num2 = 360f / (float)num;
		float num3 = UnityEngine.Random.Range(0f, num2);
		if (AlignFirstShot && (bool)source && (bool)source.CurrentGun)
		{
			num3 = source.CurrentGun.CurrentAngle + AlignOffset;
		}
		if (AimFirstAtNearestEnemy)
		{
			Vector2 vector = ((!overrideSpawnPoint.HasValue) ? source.CenterPosition : overrideSpawnPoint.Value);
			vector = ((!spawnPointOffset.HasValue) ? vector : (vector + spawnPointOffset.Value));
			AIActor nearestEnemy = GetNearestEnemy(vector);
			if ((bool)nearestEnemy)
			{
				num3 = Vector2.Angle(Vector2.right, nearestEnemy.CenterPosition - vector);
			}
		}
		bool flag = projectileToSpawn.GetComponent<BeamController>() != null;
		for (int i = 0; i < num; i++)
		{
			float targetAngle = num3 + num2 * (float)i;
			if (flag)
			{
				source.StartCoroutine(HandleFireShortBeam(projectileToSpawn, source, targetAngle, 1f * (float)NumberWaves, overrideSpawnPoint, spawnPointOffset));
			}
			else
			{
				DoSingleProjectile(projectileToSpawn, source, targetAngle, overrideSpawnPoint, spawnPointOffset);
			}
		}
	}

	private IEnumerator HandleBurst(Projectile projectileToSpawn, PlayerController source, Vector2? overrideSpawnPoint, Vector2? spawnPointOffset = null)
	{
		if (projectileToSpawn == null)
		{
			yield break;
		}
		bool projectileIsBeam = projectileToSpawn.GetComponent<BeamController>() != null;
		bool projectileExplodes = projectileToSpawn.GetComponent<ExplosiveModifier>() != null;
		bool projectileSpawns = projectileToSpawn.GetComponent<SpawnProjModifier>() != null;
		bool reducedCountProjectile = projectileToSpawn.GetComponent<BlackHoleDoer>() != null;
		int limit = projectileToSpawn.GetRadialBurstLimit(source);
		int modWaves = NumberWaves;
		if (projectileIsBeam)
		{
			modWaves = 1;
		}
		if (projectileExplodes)
		{
			modWaves = 1;
		}
		if (projectileSpawns)
		{
			modWaves = 1;
		}
		if (reducedCountProjectile)
		{
			modWaves = 1;
		}
		if (limit > 0 && limit < 1000)
		{
			modWaves = 1;
		}
		int modSubwaves = Mathf.Max(1, NumberSubwaves);
		for (int w = 0; w < modWaves; w++)
		{
			int numToSpawn = UnityEngine.Random.Range(MinToSpawnPerWave, MaxToSpawnPerWave);
			if (limit < numToSpawn)
			{
				numToSpawn = limit;
			}
			if (reducedCountProjectile)
			{
				numToSpawn = 3;
			}
			float angleStep = 360f / (float)numToSpawn;
			float angleBase = UnityEngine.Random.Range(0f, angleStep);
			float spiralDelay = TimeBetweenWaves / (float)numToSpawn;
			if (AlignFirstShot && (bool)source && (bool)source.CurrentGun)
			{
				angleBase = source.CurrentGun.CurrentAngle;
			}
			for (int i = 0; i < numToSpawn; i++)
			{
				for (int j = 0; j < modSubwaves; j++)
				{
					float targetAngle = angleBase + angleStep * (float)i + (float)j * (360f / (float)modSubwaves);
					if (projectileIsBeam)
					{
						source.StartCoroutine(HandleFireShortBeam(projectileToSpawn, source, targetAngle, 1f * (float)NumberWaves, overrideSpawnPoint, spawnPointOffset));
					}
					else
					{
						DoSingleProjectile(projectileToSpawn, source, targetAngle, overrideSpawnPoint, spawnPointOffset);
					}
				}
				if (SpiralWaves)
				{
					yield return new WaitForSeconds(spiralDelay);
				}
			}
			if (!SpiralWaves)
			{
				yield return new WaitForSeconds(TimeBetweenWaves);
			}
		}
	}

	private IEnumerator HandleFireShortBeam(Projectile projectileToSpawn, PlayerController source, float targetAngle, float duration, Vector2? overrideSpawnPoint, Vector2? spawnPointOffset = null)
	{
		float elapsed = 0f;
		BeamController beam = BeginFiringBeam(projectileToSpawn, source, targetAngle, overrideSpawnPoint, spawnPointOffset);
		yield return null;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			if (SweepBeams)
			{
				beam.Direction = Quaternion.Euler(0f, 0f, BraveTime.DeltaTime / duration * BeamSweepDegrees) * beam.Direction;
			}
			ContinueFiringBeam(beam, source, overrideSpawnPoint, spawnPointOffset);
			yield return null;
		}
		CeaseBeam(beam);
	}

	private void DoSingleProjectile(Projectile projectileToSpawn, PlayerController source, float targetAngle, Vector2? overrideSpawnPoint, Vector2? spawnPointOffset = null)
	{
		Vector2 vector = ((!overrideSpawnPoint.HasValue) ? source.specRigidbody.UnitCenter : overrideSpawnPoint.Value);
		vector = ((!spawnPointOffset.HasValue) ? vector : (vector + spawnPointOffset.Value));
		GameObject gameObject = SpawnManager.SpawnProjectile(projectileToSpawn.gameObject, vector, Quaternion.Euler(0f, 0f, targetAngle));
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = source;
		component.Shooter = source.specRigidbody;
		source.DoPostProcessProjectile(component);
		if (CustomPostProcessProjectile != null)
		{
			CustomPostProcessProjectile(component);
		}
		InternalPostProcessProjectile(component);
	}

	private void InternalPostProcessProjectile(Projectile proj)
	{
		if ((bool)proj && !ForceAllowGoop)
		{
			GoopModifier component = proj.GetComponent<GoopModifier>();
			if ((bool)component)
			{
				UnityEngine.Object.Destroy(component);
			}
		}
		if (FixOverlapCollision && (bool)proj && (bool)proj.specRigidbody)
		{
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(proj.specRigidbody);
		}
	}

	private BeamController BeginFiringBeam(Projectile projectileToSpawn, PlayerController source, float targetAngle, Vector2? overrideSpawnPoint, Vector2? spawnPointOffset = null)
	{
		Vector2 vector = ((!overrideSpawnPoint.HasValue) ? source.CenterPosition : overrideSpawnPoint.Value);
		vector = ((!spawnPointOffset.HasValue) ? vector : (vector + spawnPointOffset.Value));
		GameObject gameObject = SpawnManager.SpawnProjectile(projectileToSpawn.gameObject, vector, Quaternion.identity);
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = source;
		BeamController component2 = gameObject.GetComponent<BeamController>();
		component2.Owner = source;
		component2.HitsPlayers = false;
		component2.HitsEnemies = true;
		Vector3 vector2 = BraveMathCollege.DegreesToVector(targetAngle);
		component2.Direction = vector2;
		component2.Origin = vector;
		InternalPostProcessProjectile(component);
		return component2;
	}

	private void ContinueFiringBeam(BeamController beam, PlayerController source, Vector2? overrideSpawnPoint, Vector2? spawnPointOffset = null)
	{
		Vector2 vector = ((!overrideSpawnPoint.HasValue) ? source.CenterPosition : overrideSpawnPoint.Value);
		vector = (beam.Origin = ((!spawnPointOffset.HasValue) ? vector : (vector + spawnPointOffset.Value)));
		beam.LateUpdatePosition(vector);
	}

	private void CeaseBeam(BeamController beam)
	{
		beam.CeaseAttack();
	}
}
