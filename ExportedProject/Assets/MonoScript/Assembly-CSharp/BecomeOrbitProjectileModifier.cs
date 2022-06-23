using System;
using UnityEngine;

public class BecomeOrbitProjectileModifier : BraveBehaviour
{
	public float MinRadius = 2f;

	public float MaxRadius = 5f;

	public int OrbitGroup = -1;

	public float SpawnVFXElapsedTimer = -1f;

	public VFXPool RespawnVFX;

	public bool TriggerOnBounce = true;

	public void Start()
	{
		Projectile projectile = base.projectile;
		if (TriggerOnBounce)
		{
			BounceProjModifier orAddComponent = projectile.gameObject.GetOrAddComponent<BounceProjModifier>();
			orAddComponent.numberOfBounces = Mathf.Max(orAddComponent.numberOfBounces, 1);
			orAddComponent.onlyBounceOffTiles = true;
			orAddComponent.OnBounceContext = (Action<BounceProjModifier, SpeculativeRigidbody>)Delegate.Combine(orAddComponent.OnBounceContext, new Action<BounceProjModifier, SpeculativeRigidbody>(HandleStartOrbit));
		}
		else
		{
			StartOrbit();
		}
	}

	private void StartOrbit()
	{
		OrbitProjectileMotionModule orbitProjectileMotionModule = new OrbitProjectileMotionModule();
		orbitProjectileMotionModule.MinRadius = MinRadius;
		orbitProjectileMotionModule.MaxRadius = MaxRadius;
		orbitProjectileMotionModule.OrbitGroup = OrbitGroup;
		base.projectile.OverrideMotionModule = orbitProjectileMotionModule;
	}

	private void HandleStartOrbit(BounceProjModifier bouncer, SpeculativeRigidbody srb)
	{
		bouncer.projectile.specRigidbody.CollideWithTileMap = false;
		OrbitProjectileMotionModule orbitProjectileMotionModule = new OrbitProjectileMotionModule();
		orbitProjectileMotionModule.MinRadius = MinRadius;
		orbitProjectileMotionModule.MaxRadius = MaxRadius;
		orbitProjectileMotionModule.OrbitGroup = OrbitGroup;
		orbitProjectileMotionModule.HasSpawnVFX = true;
		orbitProjectileMotionModule.SpawnVFX = RespawnVFX.effects[0].effects[0].effect;
		orbitProjectileMotionModule.CustomSpawnVFXElapsed = SpawnVFXElapsedTimer;
		bouncer.projectile.OverrideMotionModule = orbitProjectileMotionModule;
	}
}
