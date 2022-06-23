using System.Collections;
using UnityEngine;

public class SubprojectileSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public Projectile Subprojectile;

	public bool DoesOrbit = true;

	public float OrbitMinRadius = 1f;

	public float OrbitMaxRadius = 1f;

	private Projectile m_projectile;

	private void Start()
	{
		m_projectile = GetComponent<Projectile>();
		if ((bool)m_projectile && (bool)m_projectile.PossibleSourceGun && m_projectile.PossibleSourceGun.OwnerHasSynergy(RequiredSynergy))
		{
			m_projectile.StartCoroutine(CreateSubprojectile());
		}
	}

	private IEnumerator CreateSubprojectile()
	{
		Projectile instanceSubprojectile = VolleyUtility.ShootSingleProjectile(Subprojectile, m_projectile.transform.position.XY(), 0f, false, m_projectile.Owner);
		yield return null;
		if (DoesOrbit)
		{
			OrbitProjectileMotionModule orbitProjectileMotionModule = new OrbitProjectileMotionModule();
			orbitProjectileMotionModule.MinRadius = OrbitMinRadius;
			orbitProjectileMotionModule.MaxRadius = OrbitMaxRadius;
			orbitProjectileMotionModule.usesAlternateOrbitTarget = true;
			orbitProjectileMotionModule.alternateOrbitTarget = m_projectile.specRigidbody;
			instanceSubprojectile.OverrideMotionModule = orbitProjectileMotionModule;
		}
	}
}
