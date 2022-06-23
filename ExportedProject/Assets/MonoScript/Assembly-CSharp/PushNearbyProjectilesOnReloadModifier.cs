using System;
using UnityEngine;

public class PushNearbyProjectilesOnReloadModifier : MonoBehaviour
{
	public float DistanceCutoff = 5f;

	public float AngleCutoff = 45f;

	public float SpeedMultiplier = 10f;

	public AnimationCurve NewSlowdownCurve;

	public float CurveTime = 1f;

	public bool IsSynergyContingent;

	[ShowInInspectorIf("IsSynergyContingent", false)]
	public CustomSynergyType RequiredSynergy = CustomSynergyType.BUBBLE_BUSTER;

	[ShowInInspectorIf("IsSynergyContingent", false)]
	public bool OnlyInSpecificForm;

	[ShowInInspectorIf("OnlyInSpecificForm", false)]
	public ProjectileVolleyData RequiredVolley;

	private Gun m_gun;

	public void Awake()
	{
		m_gun = GetComponent<Gun>();
		m_gun.CanReloadNoMatterAmmo = true;
		Gun gun = m_gun;
		gun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReload));
	}

	private void HandleReload(PlayerController ownerPlayer, Gun ownerGun, bool someBool)
	{
		if (!ownerGun || !ownerPlayer || !ownerGun.IsReloading || (IsSynergyContingent && !ownerPlayer.HasActiveBonusSynergy(RequiredSynergy)) || (OnlyInSpecificForm && ownerGun.RawSourceVolley != RequiredVolley))
		{
			return;
		}
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile = StaticReferenceManager.AllProjectiles[i];
			if ((bool)projectile && projectile.Owner == ownerPlayer && (bool)projectile.specRigidbody && projectile.PossibleSourceGun == ownerGun)
			{
				Vector2 unitCenter = projectile.specRigidbody.UnitCenter;
				Vector2 centerPosition = ownerPlayer.CenterPosition;
				Vector2 vector = unitCenter - centerPosition;
				float magnitude = vector.magnitude;
				float f = Mathf.DeltaAngle(ownerGun.CurrentAngle, vector.ToAngle());
				if (Mathf.Abs(f) < AngleCutoff && magnitude < DistanceCutoff)
				{
					projectile.baseData.speed *= SpeedMultiplier;
					projectile.baseData.AccelerationCurve = NewSlowdownCurve;
					projectile.baseData.IgnoreAccelCurveTime = projectile.ElapsedTime;
					projectile.baseData.CustomAccelerationCurveDuration = CurveTime;
					projectile.UpdateSpeed();
				}
			}
		}
	}
}
