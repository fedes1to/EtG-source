using System;
using UnityEngine;

public class GunVolleyModificationItem : PassiveItem
{
	public bool AddsModule;

	[ShowInInspectorIf("AddsModule", false)]
	public ProjectileModule ModuleToAdd;

	public int DuplicatesOfBaseModule;

	public float DuplicateAngleOffset = 10f;

	public float DuplicateAngleBaseOffset;

	public int DuplicatesOfEachModule;

	public float EachModuleOffsetAngle;

	public float EachSingleModuleMinAngleVariance = 15f;

	[Header("For Helix Bullets")]
	public bool AddsHelixModifier;

	[Header("For Orbit Bullets")]
	public bool AddsOrbitModifier;

	private bool UpOrDown;

	private const int MAX_ORBIT_PROJECTILES = 20;

	private const float ORBIT_LIFESPAN = 15f;

	public void ModifyVolley(ProjectileVolleyData volleyToModify)
	{
		if (AddsModule)
		{
			bool flag = true;
			if (volleyToModify != null && volleyToModify.projectiles.Count > 0 && volleyToModify.projectiles[0].projectiles != null && volleyToModify.projectiles[0].projectiles.Count > 0)
			{
				Projectile projectile = volleyToModify.projectiles[0].projectiles[0];
				if ((bool)projectile && (bool)projectile.GetComponent<ArtfulDodgerProjectileController>())
				{
					flag = false;
				}
			}
			if (flag)
			{
				ModuleToAdd.isExternalAddedModule = true;
				volleyToModify.projectiles.Add(ModuleToAdd);
			}
		}
		if (DuplicatesOfEachModule > 0)
		{
			int count = volleyToModify.projectiles.Count;
			for (int i = 0; i < count; i++)
			{
				ProjectileModule projectileModule = volleyToModify.projectiles[i];
				for (int j = 0; j < DuplicatesOfEachModule; j++)
				{
					int sourceIndex = i;
					if (projectileModule.CloneSourceIndex >= 0)
					{
						sourceIndex = projectileModule.CloneSourceIndex;
					}
					ProjectileModule projectileModule2 = ProjectileModule.CreateClone(projectileModule, false, sourceIndex);
					if (projectileModule2.projectiles != null && projectileModule2.projectiles.Count > 0 && projectileModule2.projectiles[0] is InputGuidedProjectile)
					{
						projectileModule2.positionOffset = UnityEngine.Random.insideUnitCircle.normalized;
					}
					projectileModule2.angleVariance = Mathf.Max(projectileModule2.angleVariance * 2f, EachSingleModuleMinAngleVariance);
					projectileModule2.ignoredForReloadPurposes = true;
					projectileModule2.angleFromAim = projectileModule.angleFromAim + EachModuleOffsetAngle;
					projectileModule2.ammoCost = 0;
					volleyToModify.projectiles.Add(projectileModule2);
				}
				projectileModule.angleVariance = Mathf.Max(projectileModule.angleVariance, 5f);
			}
			if (!volleyToModify.UsesShotgunStyleVelocityRandomizer)
			{
				volleyToModify.UsesShotgunStyleVelocityRandomizer = true;
				volleyToModify.DecreaseFinalSpeedPercentMin = -10f;
				volleyToModify.IncreaseFinalSpeedPercentMax = 10f;
			}
		}
		if (DuplicatesOfBaseModule > 0)
		{
			AddDuplicateOfBaseModule(volleyToModify, base.Owner, DuplicatesOfBaseModule, DuplicateAngleOffset, DuplicateAngleBaseOffset);
		}
	}

	public static void AddDuplicateOfBaseModule(ProjectileVolleyData volleyToModify, PlayerController Owner, int DuplicatesOfBaseModule, float DuplicateAngleOffset, float DuplicateAngleBaseOffset)
	{
		ProjectileModule projectileModule = volleyToModify.projectiles[0];
		int num = 0;
		if (volleyToModify.ModulesAreTiers && (bool)Owner && (bool)Owner.CurrentGun && Owner.CurrentGun.CurrentStrengthTier >= 0 && Owner.CurrentGun.CurrentStrengthTier < volleyToModify.projectiles.Count)
		{
			projectileModule = volleyToModify.projectiles[Owner.CurrentGun.CurrentStrengthTier];
			num = Owner.CurrentGun.CurrentStrengthTier;
		}
		if (projectileModule.mirror)
		{
			return;
		}
		float num2 = (float)DuplicatesOfBaseModule * DuplicateAngleOffset * -1f / 2f;
		num2 = (projectileModule.angleFromAim = num2 + DuplicateAngleBaseOffset);
		for (int i = 0; i < DuplicatesOfBaseModule; i++)
		{
			int sourceIndex = num;
			if (projectileModule.CloneSourceIndex >= 0)
			{
				sourceIndex = projectileModule.CloneSourceIndex;
			}
			ProjectileModule projectileModule2 = ProjectileModule.CreateClone(projectileModule, false, sourceIndex);
			float num3 = (projectileModule2.angleFromAim = num2 + DuplicateAngleOffset * (float)(i + 1));
			projectileModule2.ignoredForReloadPurposes = true;
			projectileModule2.ammoCost = 0;
			volleyToModify.projectiles.Add(projectileModule2);
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			if (AddsHelixModifier)
			{
				player.PostProcessProjectile += PostProcessProjectileHelix;
				player.PostProcessBeam += PostProcessProjectileHelixBeam;
			}
			if (AddsOrbitModifier)
			{
				player.PostProcessProjectile += PostProcessProjectileOrbit;
				player.PostProcessBeam += PostProcessProjectileOrbitBeam;
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<GunVolleyModificationItem>().m_pickedUpThisRun = true;
		if (AddsHelixModifier)
		{
			player.PostProcessProjectile -= PostProcessProjectileHelix;
			player.PostProcessBeam -= PostProcessProjectileHelixBeam;
		}
		if (AddsOrbitModifier)
		{
			player.PostProcessProjectile -= PostProcessProjectileOrbit;
			player.PostProcessBeam -= PostProcessProjectileOrbitBeam;
		}
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_owner)
		{
			if (AddsHelixModifier)
			{
				m_owner.PostProcessProjectile -= PostProcessProjectileHelix;
				m_owner.PostProcessBeam -= PostProcessProjectileHelixBeam;
			}
			if (AddsOrbitModifier)
			{
				m_owner.PostProcessProjectile -= PostProcessProjectileOrbit;
				m_owner.PostProcessBeam -= PostProcessProjectileOrbitBeam;
			}
		}
	}

	private void PostProcessProjectileHelix(Projectile obj, float effectChanceScalar)
	{
		if (obj is InstantDamageOneEnemyProjectile || obj is InstantlyDamageAllProjectile)
		{
			return;
		}
		if (obj is HelixProjectile)
		{
			if ((bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.DOUBLE_DOUBLE_HELIX))
			{
				HelixProjectile helixProjectile = obj as HelixProjectile;
				helixProjectile.helixAmplitude *= 0.5f;
				helixProjectile.helixWavelength *= 0.75f;
			}
			return;
		}
		if (obj.OverrideMotionModule != null && obj.OverrideMotionModule is OrbitProjectileMotionModule)
		{
			OrbitProjectileMotionModule orbitProjectileMotionModule = obj.OverrideMotionModule as OrbitProjectileMotionModule;
			orbitProjectileMotionModule.StackHelix = true;
			orbitProjectileMotionModule.ForceInvert = !UpOrDown;
		}
		else if (UpOrDown)
		{
			obj.OverrideMotionModule = new HelixProjectileMotionModule();
		}
		else
		{
			HelixProjectileMotionModule helixProjectileMotionModule = new HelixProjectileMotionModule();
			helixProjectileMotionModule.ForceInvert = true;
			obj.OverrideMotionModule = helixProjectileMotionModule;
		}
		UpOrDown = !UpOrDown;
	}

	private void PostProcessProjectileHelixBeam(BeamController beam)
	{
		if (!(beam.Owner is AIActor))
		{
			if (beam.projectile.OverrideMotionModule != null && beam.projectile.OverrideMotionModule is OrbitProjectileMotionModule)
			{
				OrbitProjectileMotionModule orbitProjectileMotionModule = beam.projectile.OverrideMotionModule as OrbitProjectileMotionModule;
				orbitProjectileMotionModule.StackHelix = true;
				orbitProjectileMotionModule.ForceInvert = !UpOrDown;
			}
			else if (UpOrDown)
			{
				beam.projectile.OverrideMotionModule = new HelixProjectileMotionModule();
			}
			else
			{
				HelixProjectileMotionModule helixProjectileMotionModule = new HelixProjectileMotionModule();
				helixProjectileMotionModule.ForceInvert = true;
				beam.projectile.OverrideMotionModule = helixProjectileMotionModule;
			}
			UpOrDown = !UpOrDown;
		}
	}

	private void PostProcessProjectileOrbit(Projectile obj, float effectChanceScalar)
	{
		if (!(obj is InstantDamageOneEnemyProjectile) && !(obj is InstantlyDamageAllProjectile) && !obj.GetComponent<ArtfulDodgerProjectileController>())
		{
			BounceProjModifier orAddComponent = obj.gameObject.GetOrAddComponent<BounceProjModifier>();
			orAddComponent.numberOfBounces = Mathf.Max(orAddComponent.numberOfBounces, 1);
			orAddComponent.onlyBounceOffTiles = true;
			orAddComponent.OnBounceContext = (Action<BounceProjModifier, SpeculativeRigidbody>)Delegate.Combine(orAddComponent.OnBounceContext, new Action<BounceProjModifier, SpeculativeRigidbody>(HandleStartOrbit));
		}
	}

	private void PostProcessProjectileOrbitBeam(BeamController beam)
	{
		if (!(beam.Owner is AIActor))
		{
			float num = 2.75f + (float)OrbitProjectileMotionModule.ActiveBeams;
			if (beam.projectile.baseData.range > 0f)
			{
				beam.projectile.baseData.range = beam.projectile.baseData.range + (float)Math.PI * 2f * num;
			}
			if (beam.projectile.baseData.speed > 0f)
			{
				beam.projectile.baseData.speed = Mathf.Max(beam.projectile.baseData.speed, 75f);
			}
			if (beam is BasicBeamController)
			{
				(beam as BasicBeamController).PenetratesCover = true;
				(beam as BasicBeamController).penetration += 10;
			}
			OrbitProjectileMotionModule orbitProjectileMotionModule = new OrbitProjectileMotionModule();
			orbitProjectileMotionModule.BeamOrbitRadius = num;
			orbitProjectileMotionModule.RegisterAsBeam(beam);
			if (beam.projectile.OverrideMotionModule != null && beam.projectile.OverrideMotionModule is HelixProjectileMotionModule)
			{
				orbitProjectileMotionModule.StackHelix = true;
				orbitProjectileMotionModule.ForceInvert = (beam.projectile.OverrideMotionModule as HelixProjectileMotionModule).ForceInvert;
			}
			beam.projectile.OverrideMotionModule = orbitProjectileMotionModule;
		}
	}

	private void HandleStartOrbit(BounceProjModifier bouncer, SpeculativeRigidbody srb)
	{
		int orbitersInGroup = OrbitProjectileMotionModule.GetOrbitersInGroup(-1);
		if (orbitersInGroup < 20)
		{
			bouncer.projectile.specRigidbody.CollideWithTileMap = false;
			bouncer.projectile.ResetDistance();
			bouncer.projectile.baseData.range = Mathf.Max(bouncer.projectile.baseData.range, 500f);
			if (bouncer.projectile.baseData.speed > 50f)
			{
				bouncer.projectile.baseData.speed = 20f;
				bouncer.projectile.UpdateSpeed();
			}
			OrbitProjectileMotionModule orbitProjectileMotionModule = new OrbitProjectileMotionModule();
			orbitProjectileMotionModule.lifespan = 15f;
			if (bouncer.projectile.OverrideMotionModule != null && bouncer.projectile.OverrideMotionModule is HelixProjectileMotionModule)
			{
				orbitProjectileMotionModule.StackHelix = true;
				orbitProjectileMotionModule.ForceInvert = (bouncer.projectile.OverrideMotionModule as HelixProjectileMotionModule).ForceInvert;
			}
			bouncer.projectile.OverrideMotionModule = orbitProjectileMotionModule;
		}
	}
}
