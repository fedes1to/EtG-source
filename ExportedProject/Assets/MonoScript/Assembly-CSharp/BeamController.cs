using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BeamController : BraveBehaviour
{
	public const float c_chanceTick = 1f;

	[TogglesProperty("knockbackStrength", "Knockback")]
	public bool knocksShooterBack;

	[HideInInspector]
	public float knockbackStrength = 10f;

	[TogglesProperty("chargeDelay", "Charge Delay")]
	public bool usesChargeDelay;

	[HideInInspector]
	public float chargeDelay;

	public float statusEffectChance = 1f;

	public float statusEffectAccumulateMultiplier = 1f;

	[NonSerialized]
	public List<SpeculativeRigidbody> IgnoreRigidbodes = new List<SpeculativeRigidbody>();

	[NonSerialized]
	public List<Tuple<SpeculativeRigidbody, float>> TimedIgnoreRigidbodies = new List<Tuple<SpeculativeRigidbody, float>>();

	private SpeculativeRigidbody[] m_ignoreRigidbodiesList;

	protected float m_chanceTick = -1000f;

	public GameActor Owner { get; set; }

	public Gun Gun { get; set; }

	public bool HitsPlayers { get; set; }

	public bool HitsEnemies { get; set; }

	public Vector2 Origin { get; set; }

	public Vector2 Direction { get; set; }

	public float DamageModifier { get; set; }

	public abstract bool ShouldUseAmmo { get; }

	public float ChanceBasedHomingRadius { get; set; }

	public float ChanceBasedHomingAngularVelocity { get; set; }

	public bool ChanceBasedShadowBullet { get; set; }

	public bool IsReflectedBeam { get; set; }

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public abstract void LateUpdatePosition(Vector3 origin);

	public abstract void CeaseAttack();

	public abstract void DestroyBeam();

	public abstract void AdjustPlayerBeamTint(Color targetTintColor, int priority, float lerpTime = 0f);

	protected bool HandleChanceTick()
	{
		bool result = false;
		if (m_chanceTick <= 0f)
		{
			ChanceBasedHomingRadius = 0f;
			ChanceBasedHomingAngularVelocity = 0f;
			ChanceBasedShadowBullet = false;
			(Owner as PlayerController).DoPostProcessBeamChanceTick(this);
			m_chanceTick += 1f;
			result = true;
		}
		m_chanceTick -= BraveTime.DeltaTime;
		return result;
	}

	protected SpeculativeRigidbody[] GetIgnoreRigidbodies()
	{
		PlayerController playerController = Owner as PlayerController;
		int num = IgnoreRigidbodes.Count + TimedIgnoreRigidbodies.Count;
		if ((bool)Owner && (bool)Owner.specRigidbody)
		{
			num++;
		}
		if ((bool)playerController && playerController.IsInMinecart)
		{
			num++;
		}
		if (m_ignoreRigidbodiesList == null || m_ignoreRigidbodiesList.Length != num)
		{
			m_ignoreRigidbodiesList = new SpeculativeRigidbody[num];
		}
		int num2 = 0;
		for (int i = 0; i < IgnoreRigidbodes.Count; i++)
		{
			m_ignoreRigidbodiesList[num2++] = IgnoreRigidbodes[i];
		}
		for (int j = 0; j < TimedIgnoreRigidbodies.Count; j++)
		{
			m_ignoreRigidbodiesList[num2++] = TimedIgnoreRigidbodies[j].First;
		}
		if ((bool)Owner && (bool)Owner.specRigidbody)
		{
			m_ignoreRigidbodiesList[num2++] = Owner.specRigidbody;
		}
		if ((bool)playerController && playerController.IsInMinecart)
		{
			m_ignoreRigidbodiesList[num2++] = playerController.currentMineCart.specRigidbody;
		}
		return m_ignoreRigidbodiesList;
	}

	public static BeamController FreeFireBeam(Projectile projectileToSpawn, PlayerController source, float targetAngle, float duration, bool skipChargeTime = false)
	{
		GameObject gameObject = SpawnManager.SpawnProjectile(projectileToSpawn.gameObject, source.CenterPosition, Quaternion.identity);
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = source;
		BeamController component2 = gameObject.GetComponent<BeamController>();
		if (skipChargeTime)
		{
			component2.chargeDelay = 0f;
			component2.usesChargeDelay = false;
		}
		component2.Owner = source;
		component2.HitsPlayers = false;
		component2.HitsEnemies = true;
		Vector3 vector = BraveMathCollege.DegreesToVector(targetAngle);
		component2.Direction = vector;
		component2.Origin = source.CenterPosition;
		GameManager.Instance.Dungeon.StartCoroutine(HandleFiringBeam(component2, source, targetAngle, duration));
		return component2;
	}

	private static IEnumerator HandleFiringBeam(BeamController beam, PlayerController source, float targetAngle, float duration)
	{
		float elapsed = 0f;
		yield return null;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			beam.Origin = source.CenterPosition;
			beam.LateUpdatePosition(source.CenterPosition);
			yield return null;
		}
		beam.CeaseAttack();
	}
}
