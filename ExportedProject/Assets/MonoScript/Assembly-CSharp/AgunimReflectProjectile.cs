using System;
using UnityEngine;

[RequireComponent(typeof(Projectile))]
public class AgunimReflectProjectile : BraveBehaviour
{
	public int[] NumBounces;

	public float[] SpeedIncreases;

	public float[] AnimSpeedMultipliers;

	public float[] BossReflectSpreads;

	public float[] PlayerReflectFriction;

	public float[] BossReflectFriction;

	public VFXPool PlayerReflectVfx;

	private int m_playerReflects;

	public void Awake()
	{
		base.projectile.OnReflected += OnReflected;
		SpeculativeRigidbody speculativeRigidbody = base.projectile.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.projectile.specRigidbody;
		speculativeRigidbody2.OnPreTileCollision = (SpeculativeRigidbody.OnPreTileCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnPreTileCollision, new SpeculativeRigidbody.OnPreTileCollisionDelegate(OnPreTileCollision));
	}

	public void OnSpawned()
	{
		m_playerReflects = 0;
	}

	public void OnDespawned()
	{
		base.projectile.spriteAnimator.OverrideTimeScale = -1f;
	}

	protected override void OnDestroy()
	{
		if ((bool)base.projectile)
		{
			base.projectile.OnReflected -= OnReflected;
			if ((bool)base.projectile.specRigidbody)
			{
				SpeculativeRigidbody speculativeRigidbody = base.projectile.specRigidbody;
				speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreRigidbodyCollision));
				SpeculativeRigidbody speculativeRigidbody2 = base.projectile.specRigidbody;
				speculativeRigidbody2.OnPreTileCollision = (SpeculativeRigidbody.OnPreTileCollisionDelegate)Delegate.Remove(speculativeRigidbody2.OnPreTileCollision, new SpeculativeRigidbody.OnPreTileCollisionDelegate(OnPreTileCollision));
			}
		}
		base.OnDestroy();
	}

	private void OnReflected(Projectile p)
	{
		if (p.Owner is PlayerController)
		{
			if (p.spriteAnimator.OverrideTimeScale < 0f)
			{
				p.spriteAnimator.OverrideTimeScale = 1f;
			}
			p.Speed += SpeedIncreases[m_playerReflects];
			p.spriteAnimator.OverrideTimeScale *= AnimSpeedMultipliers[m_playerReflects];
			PlayerReflectVfx.SpawnAtPosition(base.transform.position);
			StickyFrictionManager.Instance.RegisterCustomStickyFriction(PlayerReflectFriction[m_playerReflects], 0f, false, true);
			m_playerReflects++;
			AkSoundEngine.PostEvent("Play_BOSS_agunim_deflect_01", base.gameObject);
		}
	}

	private void OnPreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (!(base.projectile.Owner is PlayerController) || !otherRigidbody.healthHaver || !otherRigidbody.healthHaver.IsBoss)
		{
			return;
		}
		int num = NumBounces[3 - Mathf.RoundToInt(otherRigidbody.healthHaver.GetCurrentHealth())];
		if (m_playerReflects < num)
		{
			AkSoundEngine.PostEvent("Play_BOSS_agunim_deflect_01", base.gameObject);
			Projectile p = base.projectile;
			bool retargetReflectedBullet = true;
			AIActor newOwner = otherRigidbody.aiActor;
			float minReflectedBulletSpeed = 2f;
			float spread = BossReflectSpreads[m_playerReflects - 1];
			PassiveReflectItem.ReflectBullet(p, retargetReflectedBullet, newOwner, minReflectedBulletSpeed, 1f, 1f, spread);
			StickyFrictionManager.Instance.RegisterCustomStickyFriction(BossReflectFriction[m_playerReflects - 1], 0f, false, true);
			AIActor aIActor = otherRigidbody.aiActor;
			if ((bool)aIActor)
			{
				aIActor.aiAnimator.PlayUntilFinished("deflect", true);
				AIAnimator aIAnimator = aIActor.aiAnimator;
				string text = "deflect";
				Vector2? position = (aIActor.specRigidbody.GetUnitCenter(ColliderType.HitBox) + base.projectile.specRigidbody.UnitCenter) / 2f;
				aIAnimator.PlayVfx(text, null, null, position);
			}
			PhysicsEngine.SkipCollision = true;
			PhysicsEngine.PostSliceVelocity = base.projectile.specRigidbody.Velocity;
		}
		else
		{
			AIActor aIActor2 = otherRigidbody.aiActor;
			if ((bool)aIActor2)
			{
				aIActor2.aiAnimator.PlayUntilFinished("big_hit", true);
				AIAnimator aIAnimator2 = aIActor2.aiAnimator;
				string text = "big_hit";
				Vector2? position = (aIActor2.specRigidbody.GetUnitCenter(ColliderType.HitBox) + base.projectile.specRigidbody.UnitCenter) / 2f;
				aIAnimator2.PlayVfx(text, null, null, position);
			}
		}
	}

	private void OnPreTileCollision(SpeculativeRigidbody myrigidbody, PixelCollider mypixelcollider, PhysicsEngine.Tile tile, PixelCollider tilepixelcollider)
	{
		if (tile != null && (GameManager.Instance.Dungeon.data.isFaceWallHigher(tile.X, tile.Y) || GameManager.Instance.Dungeon.data.isFaceWallLower(tile.X, tile.Y)))
		{
			PhysicsEngine.SkipCollision = true;
		}
	}
}
