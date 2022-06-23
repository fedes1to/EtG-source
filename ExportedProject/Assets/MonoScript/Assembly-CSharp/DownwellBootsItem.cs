using System.Collections;
using UnityEngine;

public class DownwellBootsItem : PassiveItem
{
	public enum Condition
	{
		WhileDodgeRolling
	}

	public int NumProjectilesToFire = 5;

	public float ProjectileArcAngle = 45f;

	public float FireCooldown = 2f;

	private float m_cooldown;

	private AfterImageTrailController downwellAfterimage;

	[Header("Synergues")]
	public ExplosionData BlastBootsExplosion;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			downwellAfterimage = player.gameObject.AddComponent<AfterImageTrailController>();
			downwellAfterimage.spawnShadows = false;
			downwellAfterimage.shadowTimeDelay = 0.05f;
			downwellAfterimage.shadowLifetime = 0.3f;
			downwellAfterimage.minTranslation = 0.05f;
			downwellAfterimage.dashColor = Color.red;
			downwellAfterimage.OverrideImageShader = ShaderCache.Acquire("Brave/Internal/DownwellAfterImage");
			player.OnRollStarted += OnRollStarted;
		}
	}

	private void OnRollStarted(PlayerController sourcePlayer, Vector2 dirVec)
	{
		if ((bool)sourcePlayer && sourcePlayer.HasActiveBonusSynergy(CustomSynergyType.DOWNERWELL))
		{
			m_cooldown = 0f;
		}
		if (m_cooldown <= 0f)
		{
			if ((bool)sourcePlayer && sourcePlayer.HasActiveBonusSynergy(CustomSynergyType.BLASTBOOTS))
			{
				Exploder.Explode(sourcePlayer.CenterPosition + -dirVec.normalized, BlastBootsExplosion, dirVec, null, true);
			}
			else
			{
				for (int i = 0; i < NumProjectilesToFire; i++)
				{
					float num = 0f;
					if (NumProjectilesToFire > 1)
					{
						num = ProjectileArcAngle / -2f + ProjectileArcAngle / (float)(NumProjectilesToFire - 1) * (float)i;
					}
					GameObject gameObject = base.bulletBank.CreateProjectileFromBank(sourcePlayer.CenterPosition, BraveMathCollege.Atan2Degrees(dirVec * -1f) + num, "default");
					Projectile component = gameObject.GetComponent<Projectile>();
					if ((bool)component)
					{
						component.Shooter = sourcePlayer.specRigidbody;
						component.Owner = sourcePlayer;
						component.SpawnedFromNonChallengeItem = true;
						if ((bool)component.specRigidbody)
						{
							component.specRigidbody.PrimaryPixelCollider.CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.EnemyBulletBlocker);
						}
					}
					sourcePlayer.DoPostProcessProjectile(component);
				}
			}
			m_cooldown = FireCooldown;
		}
		sourcePlayer.StartCoroutine(HandleAfterImageStop(sourcePlayer));
	}

	private IEnumerator HandleAfterImageStop(PlayerController player)
	{
		downwellAfterimage.spawnShadows = true;
		while (player.IsDodgeRolling)
		{
			yield return null;
		}
		if ((bool)downwellAfterimage)
		{
			downwellAfterimage.spawnShadows = false;
		}
	}

	protected override void Update()
	{
		base.Update();
		m_cooldown -= BraveTime.DeltaTime;
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<DownwellBootsItem>().m_pickedUpThisRun = true;
		player.OnRollStarted -= OnRollStarted;
		if ((bool)downwellAfterimage)
		{
			Object.Destroy(downwellAfterimage);
		}
		downwellAfterimage = null;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_owner != null)
		{
			m_owner.OnRollStarted -= OnRollStarted;
			if ((bool)downwellAfterimage)
			{
				Object.Destroy(downwellAfterimage);
			}
			downwellAfterimage = null;
		}
		base.OnDestroy();
	}
}
