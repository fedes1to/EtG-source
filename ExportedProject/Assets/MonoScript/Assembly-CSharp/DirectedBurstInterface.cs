using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class DirectedBurstInterface
{
	public PlayerItemProjectileInterface ProjectileInterface;

	public int MinToSpawnPerWave = 10;

	public int MaxToSpawnPerWave = 10;

	public int NumberWaves = 1;

	public float TimeBetweenWaves = 1f;

	public bool SpiralWaves;

	public float AngleSubtended = 30f;

	public bool UseShotgunStyleVelocityModifier;

	public bool ForceAllowGoop;

	public void DoBurst(PlayerController source, float aimAngle)
	{
		if (NumberWaves == 1 && !SpiralWaves)
		{
			ImmediateBurst(ProjectileInterface.GetProjectile(source), source, aimAngle);
		}
		else
		{
			source.StartCoroutine(HandleBurst(ProjectileInterface.GetProjectile(source), source, aimAngle));
		}
	}

	private void ImmediateBurst(Projectile projectileToSpawn, PlayerController source, float aimAngle)
	{
		if (projectileToSpawn == null)
		{
			return;
		}
		int num = UnityEngine.Random.Range(MinToSpawnPerWave, MaxToSpawnPerWave);
		float num2 = AngleSubtended / (float)num;
		float num3 = 0f - AngleSubtended / 2f;
		num3 += aimAngle;
		bool flag = projectileToSpawn.GetComponent<BeamController>() != null;
		for (int i = 0; i < num; i++)
		{
			float targetAngle = num3 + num2 * (float)i;
			if (flag)
			{
				source.StartCoroutine(HandleFireShortBeam(projectileToSpawn, source, targetAngle, 1f * (float)NumberWaves));
			}
			else
			{
				DoSingleProjectile(projectileToSpawn, source, targetAngle);
			}
		}
	}

	private IEnumerator HandleBurst(Projectile projectileToSpawn, PlayerController source, float aimAngle)
	{
		if (projectileToSpawn == null)
		{
			yield break;
		}
		bool projectileIsBeam = projectileToSpawn.GetComponent<BeamController>() != null;
		bool projectileExplodes = projectileToSpawn.GetComponent<ExplosiveModifier>() != null;
		bool projectileSpawns = projectileToSpawn.GetComponent<SpawnProjModifier>() != null;
		bool reducedCountProjectile = projectileToSpawn.GetComponent<BlackHoleDoer>() != null;
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
		for (int w = 0; w < modWaves; w++)
		{
			int numToSpawn = UnityEngine.Random.Range(MinToSpawnPerWave, MaxToSpawnPerWave);
			if (reducedCountProjectile)
			{
				numToSpawn = 3;
			}
			float angleStep = AngleSubtended / (float)numToSpawn;
			float angleBase2 = 0f - AngleSubtended / 2f;
			float spiralDelay = TimeBetweenWaves / (float)numToSpawn;
			angleBase2 += aimAngle;
			for (int i = 0; i < numToSpawn; i++)
			{
				float targetAngle = angleBase2 + angleStep * (float)i;
				if (projectileIsBeam)
				{
					source.StartCoroutine(HandleFireShortBeam(projectileToSpawn, source, targetAngle, 1f * (float)NumberWaves));
				}
				else
				{
					DoSingleProjectile(projectileToSpawn, source, targetAngle);
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

	private IEnumerator HandleFireShortBeam(Projectile projectileToSpawn, PlayerController source, float targetAngle, float duration)
	{
		float elapsed = 0f;
		BeamController beam = BeginFiringBeam(projectileToSpawn, source, targetAngle);
		yield return null;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			ContinueFiringBeam(beam, source);
			yield return null;
		}
		CeaseBeam(beam);
	}

	private void DoSingleProjectile(Projectile projectileToSpawn, PlayerController source, float targetAngle)
	{
		Vector2 vector = ((!source.CurrentGun || !source.CurrentGun.barrelOffset) ? source.specRigidbody.UnitCenter : source.CurrentGun.barrelOffset.position.XY());
		GameObject gameObject = SpawnManager.SpawnProjectile(projectileToSpawn.gameObject, vector, Quaternion.Euler(0f, 0f, targetAngle));
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = source;
		component.Shooter = source.specRigidbody;
		if (MinToSpawnPerWave == 1 && MaxToSpawnPerWave == 1 && NumberWaves == 1 && !SpiralWaves && ProjectileInterface.UseCurrentGunProjectile && source.HasActiveBonusSynergy(CustomSynergyType.DOUBLE_HOLSTER))
		{
			HomingModifier homingModifier = component.gameObject.GetComponent<HomingModifier>();
			if (homingModifier == null)
			{
				homingModifier = component.gameObject.AddComponent<HomingModifier>();
				homingModifier.HomingRadius = 0f;
				homingModifier.AngularVelocity = 0f;
			}
			homingModifier.HomingRadius += 20f;
			homingModifier.AngularVelocity += 1080f;
		}
		if (UseShotgunStyleVelocityModifier)
		{
			component.baseData.speed = component.baseData.speed * (1f + UnityEngine.Random.Range(-15f, 15f) / 100f);
		}
		source.DoPostProcessProjectile(component);
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
	}

	private BeamController BeginFiringBeam(Projectile projectileToSpawn, PlayerController source, float targetAngle)
	{
		GameObject gameObject = SpawnManager.SpawnProjectile(projectileToSpawn.gameObject, source.CenterPosition, Quaternion.identity);
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = source;
		BeamController component2 = gameObject.GetComponent<BeamController>();
		component2.Owner = source;
		component2.HitsPlayers = false;
		component2.HitsEnemies = true;
		Vector3 vector = BraveMathCollege.DegreesToVector(targetAngle);
		component2.Direction = vector;
		component2.Origin = source.CenterPosition;
		InternalPostProcessProjectile(component);
		return component2;
	}

	private void ContinueFiringBeam(BeamController beam, PlayerController source)
	{
		beam.Origin = source.CenterPosition;
		beam.LateUpdatePosition(source.CenterPosition);
	}

	private void CeaseBeam(BeamController beam)
	{
		beam.CeaseAttack();
	}
}
