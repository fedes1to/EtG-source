using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class ForgeFlamePipeController : BraveBehaviour, IPlaceConfigurable
{
	public float DamageToEnemies = 6f;

	public float TimeToSpew = 10f;

	public float ConeAngle = 10f;

	public float TimeBetweenBullets = 0.05f;

	public DungeonData.Direction DirectionToSpew = DungeonData.Direction.EAST;

	public Transform ShootPoint;

	public string EndSpriteName;

	public string LoopAnimationName;

	public string OutAnimationName;

	public tk2dSpriteAnimator[] vfxAnimators;

	private bool m_hasBurst;

	public void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnBeamCollision = (SpeculativeRigidbody.OnBeamCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnBeamCollision, new SpeculativeRigidbody.OnBeamCollisionDelegate(HandleBeamCollision));
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (!m_hasBurst && (bool)rigidbodyCollision.OtherRigidbody.projectile)
		{
			m_hasBurst = true;
			StartCoroutine(HandleBurst());
		}
	}

	private void HandleBeamCollision(BeamController beamController)
	{
		if (!m_hasBurst)
		{
			m_hasBurst = true;
			StartCoroutine(HandleBurst());
		}
	}

	private IEnumerator HandleBurst()
	{
		float elapsed = 0f;
		float bulletTimer = 0f;
		if (!string.IsNullOrEmpty(EndSpriteName))
		{
			base.sprite.SetSprite(EndSpriteName);
		}
		AkSoundEngine.PostEvent("Play_TRP_flame_torch_01", base.gameObject);
		while (elapsed < TimeToSpew)
		{
			elapsed += BraveTime.DeltaTime;
			bulletTimer -= BraveTime.DeltaTime;
			if (bulletTimer <= 0f)
			{
				for (int i = 0; i < vfxAnimators.Length; i++)
				{
					if (!vfxAnimators[i].renderer.enabled)
					{
						vfxAnimators[i].renderer.enabled = true;
						vfxAnimators[i].Play(LoopAnimationName);
					}
				}
				GlobalSparksDoer.DoLinearParticleBurst(Mathf.Max(4, (int)(BraveTime.DeltaTime * 80f)), ShootPoint.position, ShootPoint.position + DungeonData.GetIntVector2FromDirection(DirectionToSpew).ToVector3() * 2f, 120f, 1.5f, 0.75f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
				for (int j = 0; j < base.specRigidbody.PixelColliders[1].TriggerCollisions.Count; j++)
				{
					SpeculativeRigidbody speculativeRigidbody = base.specRigidbody.PixelColliders[1].TriggerCollisions[j].SpecRigidbody;
					if ((bool)speculativeRigidbody && (bool)speculativeRigidbody.gameActor && (bool)speculativeRigidbody.healthHaver)
					{
						if (speculativeRigidbody.gameActor is AIActor)
						{
							speculativeRigidbody.healthHaver.ApplyDamage(DamageToEnemies, Vector2.zero, StringTableManager.GetEnemiesString("#TRAP"), CoreDamageTypes.Fire, DamageCategory.Environment);
						}
						else
						{
							speculativeRigidbody.healthHaver.ApplyDamage(0.5f, Vector2.zero, StringTableManager.GetEnemiesString("#TRAP"), CoreDamageTypes.Fire, DamageCategory.Environment);
						}
					}
				}
				bulletTimer += TimeBetweenBullets;
			}
			yield return null;
		}
		for (int k = 0; k < vfxAnimators.Length; k++)
		{
			if (vfxAnimators[k].renderer.enabled)
			{
				vfxAnimators[k].PlayAndDisableRenderer(OutAnimationName);
			}
		}
	}

	private void FireBullet()
	{
		base.bulletBank.CreateProjectileFromBank(ShootPoint.position, BraveMathCollege.Atan2Degrees(DungeonData.GetIntVector2FromDirection(DirectionToSpew).ToVector2()) + UnityEngine.Random.Range(0f - ConeAngle, ConeAngle), "default");
	}

	protected override void OnDestroy()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnBeamCollision = (SpeculativeRigidbody.OnBeamCollisionDelegate)Delegate.Remove(speculativeRigidbody2.OnBeamCollision, new SpeculativeRigidbody.OnBeamCollisionDelegate(HandleBeamCollision));
		base.OnDestroy();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		for (int i = 0; i < 2; i++)
		{
			GameManager.Instance.Dungeon.data[intVector + new IntVector2(0, i)].cellVisualData.containsObjectSpaceStamp = true;
		}
	}
}
