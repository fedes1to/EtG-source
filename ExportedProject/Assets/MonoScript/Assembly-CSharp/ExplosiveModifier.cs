using UnityEngine;

public class ExplosiveModifier : BraveBehaviour
{
	public bool doExplosion = true;

	[SerializeField]
	public ExplosionData explosionData;

	public bool doDistortionWave;

	[ShowInInspectorIf("doDistortionWave", true)]
	public float distortionIntensity = 1f;

	[ShowInInspectorIf("doDistortionWave", true)]
	public float distortionRadius = 1f;

	[ShowInInspectorIf("doDistortionWave", true)]
	public float maxDistortionRadius = 10f;

	[ShowInInspectorIf("doDistortionWave", true)]
	public float distortionDuration = 0.5f;

	public bool IgnoreQueues;

	public void Explode(Vector2 sourceNormal, bool ignoreDamageCaps = false, CollisionData cd = null)
	{
		if ((bool)base.projectile && (bool)base.projectile.Owner)
		{
			if (base.projectile.Owner is PlayerController)
			{
				for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
				{
					PlayerController playerController = GameManager.Instance.AllPlayers[i];
					if ((bool)playerController && (bool)playerController.specRigidbody)
					{
						explosionData.ignoreList.Add(playerController.specRigidbody);
					}
				}
			}
			else
			{
				explosionData.ignoreList.Add(base.projectile.Owner.specRigidbody);
			}
		}
		Vector3 vector = ((cd == null) ? base.specRigidbody.UnitCenter.ToVector3ZUp() : cd.Contact.ToVector3ZUp());
		if (doExplosion)
		{
			CoreDamageTypes coreDamageTypes = CoreDamageTypes.None;
			if (explosionData.doDamage && explosionData.damageRadius < 10f && (bool)base.projectile)
			{
				if (base.projectile.AppliesFreeze)
				{
					coreDamageTypes |= CoreDamageTypes.Ice;
				}
				if (base.projectile.AppliesFire)
				{
					coreDamageTypes |= CoreDamageTypes.Fire;
				}
				if (base.projectile.AppliesPoison)
				{
					coreDamageTypes |= CoreDamageTypes.Poison;
				}
				if (base.projectile.statusEffectsToApply != null)
				{
					for (int j = 0; j < base.projectile.statusEffectsToApply.Count; j++)
					{
						GameActorEffect gameActorEffect = base.projectile.statusEffectsToApply[j];
						if (gameActorEffect is GameActorFreezeEffect)
						{
							coreDamageTypes |= CoreDamageTypes.Ice;
						}
						else if (gameActorEffect is GameActorFireEffect)
						{
							coreDamageTypes |= CoreDamageTypes.Fire;
						}
						else if (gameActorEffect is GameActorHealthEffect)
						{
							coreDamageTypes |= CoreDamageTypes.Poison;
						}
					}
				}
			}
			Exploder.Explode(vector, explosionData, sourceNormal, null, IgnoreQueues, coreDamageTypes, ignoreDamageCaps);
		}
		if (doDistortionWave)
		{
			Exploder.DoDistortionWave(vector, distortionIntensity, distortionRadius, maxDistortionRadius, distortionDuration);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
