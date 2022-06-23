using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class KageBunshinController : BraveBehaviour
{
	public float Duration = -1f;

	[NonSerialized]
	public PlayerController Owner;

	[NonSerialized]
	public bool UsesRotationInsteadOfInversion;

	[NonSerialized]
	public float RotationAngle = 90f;

	public void InitializeOwner(PlayerController p)
	{
		Owner = p;
		base.sprite = GetComponentInChildren<tk2dSprite>();
		GetComponentInChildren<Renderer>().material.SetColor("_FlatColor", new Color(0.25f, 0.25f, 0.25f, 1f));
		base.sprite.usesOverrideMaterial = true;
		Owner.PostProcessProjectile += HandleProjectile;
		Owner.PostProcessBeam += HandleBeam;
		if (Duration > 0f)
		{
			UnityEngine.Object.Destroy(base.gameObject, Duration);
		}
	}

	private void HandleBeam(BeamController obj)
	{
		if ((bool)obj && (bool)obj.projectile)
		{
			GameObject gameObject = SpawnManager.SpawnProjectile(obj.projectile.gameObject, base.sprite.WorldCenter, Quaternion.identity);
			Projectile component = gameObject.GetComponent<Projectile>();
			component.Owner = Owner;
			BeamController component2 = gameObject.GetComponent<BeamController>();
			BasicBeamController basicBeamController = component2 as BasicBeamController;
			if ((bool)basicBeamController)
			{
				basicBeamController.SkipPostProcessing = true;
			}
			component2.Owner = Owner;
			component2.HitsPlayers = false;
			component2.HitsEnemies = true;
			Vector3 vector = BraveMathCollege.DegreesToVector(Owner.CurrentGun.CurrentAngle);
			component2.Direction = vector;
			component2.Origin = base.sprite.WorldCenter;
			GameManager.Instance.Dungeon.StartCoroutine(HandleFiringBeam(component2));
		}
	}

	private IEnumerator HandleFiringBeam(BeamController beam)
	{
		float elapsed = 0f;
		yield return null;
		while ((bool)Owner && Owner.IsFiring && (bool)this && (bool)base.sprite)
		{
			elapsed += BraveTime.DeltaTime;
			beam.Origin = base.sprite.WorldCenter;
			beam.LateUpdatePosition(base.sprite.WorldCenter);
			if ((bool)Owner)
			{
				Vector2 vector = Owner.unadjustedAimPoint.XY();
				if (!BraveInput.GetInstanceForPlayer(Owner.PlayerIDX).IsKeyboardAndMouse() && (bool)Owner.CurrentGun)
				{
					vector = Owner.CenterPosition + BraveMathCollege.DegreesToVector(Owner.CurrentGun.CurrentAngle, 10f);
				}
				float angle = (vector - base.specRigidbody.UnitCenter).ToAngle();
				beam.Direction = BraveMathCollege.DegreesToVector(angle);
			}
			yield return null;
		}
		beam.CeaseAttack();
		beam.DestroyBeam();
	}

	private void Disconnect()
	{
		if ((bool)Owner)
		{
			Owner.PostProcessProjectile -= HandleProjectile;
			Owner.PostProcessBeam -= HandleBeam;
		}
	}

	private void HandleProjectile(Projectile sourceProjectile, float arg2)
	{
		Quaternion rotation = sourceProjectile.transform.rotation;
		if ((bool)Owner && (bool)Owner.CurrentGun)
		{
			Vector2 vector = Owner.unadjustedAimPoint.XY();
			float target = (vector - Owner.CenterPosition).ToAngle();
			float num = Mathf.DeltaAngle(rotation.eulerAngles.z, target);
			if (!BraveInput.GetInstanceForPlayer(Owner.PlayerIDX).IsKeyboardAndMouse())
			{
				vector = Owner.CenterPosition + BraveMathCollege.DegreesToVector(Owner.CurrentGun.CurrentAngle, 10f);
			}
			float z = (vector - base.specRigidbody.UnitCenter).ToAngle() + num;
			rotation = Quaternion.Euler(0f, 0f, z);
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(sourceProjectile.gameObject, base.sprite.WorldCenter, rotation);
		Projectile component = gameObject.GetComponent<Projectile>();
		component.specRigidbody.RegisterSpecificCollisionException(base.specRigidbody);
		component.SetOwnerSafe(sourceProjectile.Owner, sourceProjectile.Owner.ActorName);
		component.SetNewShooter(sourceProjectile.Shooter);
	}

	private void LateUpdate()
	{
		if (!Owner)
		{
			return;
		}
		if (Owner.IsGhost)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		base.sprite.SetSprite(Owner.sprite.Collection, Owner.sprite.spriteId);
		base.sprite.FlipX = Owner.sprite.FlipX;
		base.sprite.transform.localPosition = Owner.sprite.transform.localPosition;
		if (UsesRotationInsteadOfInversion)
		{
			base.specRigidbody.Velocity = (Quaternion.Euler(0f, 0f, RotationAngle) * Owner.specRigidbody.Velocity).XY();
		}
		else
		{
			base.specRigidbody.Velocity = Owner.specRigidbody.Velocity * -1f;
		}
	}

	private void AttractEnemies(RoomHandler room)
	{
		List<AIActor> activeEnemies = room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null)
		{
			return;
		}
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (activeEnemies[i].OverrideTarget == null)
			{
				activeEnemies[i].OverrideTarget = base.specRigidbody;
			}
		}
	}

	protected override void OnDestroy()
	{
		Disconnect();
		base.OnDestroy();
	}
}
