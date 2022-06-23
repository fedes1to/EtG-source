using System;
using System.Collections;
using UnityEngine;

public class CigaretteItem : MonoBehaviour
{
	public GameObject inAirVFX;

	private bool m_inAir = true;

	public GameObject smokeSystem;

	public GameObject sparkVFX;

	public bool DestroyOnGrounded;

	private void Start()
	{
		DebrisObject component = GetComponent<DebrisObject>();
		AkSoundEngine.PostEvent("Play_OBJ_cigarette_throw_01", base.gameObject);
		component.killTranslationOnBounce = false;
		if ((bool)component)
		{
			component.OnBounced = (Action<DebrisObject>)Delegate.Combine(component.OnBounced, new Action<DebrisObject>(OnBounced));
			component.OnGrounded = (Action<DebrisObject>)Delegate.Combine(component.OnGrounded, new Action<DebrisObject>(OnHitGround));
		}
		if (inAirVFX != null)
		{
			StartCoroutine(SpawnVFX());
		}
	}

	private IEnumerator SpawnVFX()
	{
		while (m_inAir)
		{
			SpawnManager.SpawnVFX(inAirVFX, base.transform.position, Quaternion.identity, false);
			yield return new WaitForSeconds(0.33f);
		}
	}

	private void OnBounced(DebrisObject obj)
	{
		DeadlyDeadlyGoopManager.IgniteGoopsCircle(base.transform.position.XY(), 1f);
	}

	private void OnHitGround(DebrisObject obj)
	{
		OnBounced(obj);
		if ((bool)smokeSystem)
		{
			BraveUtility.EnableEmission(smokeSystem.GetComponent<ParticleSystem>(), false);
		}
		GetComponent<tk2dSpriteAnimator>().Stop();
		if (DestroyOnGrounded)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
