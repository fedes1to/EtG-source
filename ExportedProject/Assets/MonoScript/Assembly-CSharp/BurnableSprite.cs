using System;
using System.Collections;
using UnityEngine;

public class BurnableSprite : MonoBehaviour
{
	public float burnDuration = 2f;

	private GameObject burnParticleSystem;

	private bool m_isBurning;

	public void Initialize()
	{
		SpeculativeRigidbody component = GetComponent<SpeculativeRigidbody>();
		component.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(component.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
	}

	public void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (!m_isBurning)
		{
			Projectile component = rigidbodyCollision.OtherRigidbody.GetComponent<Projectile>();
			if (component != null)
			{
				Burn();
			}
		}
	}

	public void Burn()
	{
		m_isBurning = true;
		burnParticleSystem = SpawnManager.SpawnParticleSystem(BraveResources.Load<GameObject>("BurningSpriteEffect"));
		burnParticleSystem.transform.parent = base.transform;
		burnParticleSystem.transform.localPosition = new Vector3(0.5f, 0f, 0f);
		StartCoroutine(HandleBurning());
	}

	private IEnumerator HandleBurning()
	{
		float elapsed = 0f;
		Material material = GetComponent<Renderer>().material;
		float spriteHeight = material.GetFloat("_PixelHeight") / 16f;
		while (elapsed < burnDuration)
		{
			elapsed += BraveTime.DeltaTime;
			float percentComplete = elapsed / burnDuration;
			material.SetFloat("_Threshold", percentComplete);
			burnParticleSystem.transform.localPosition = burnParticleSystem.transform.localPosition.WithY(percentComplete * spriteHeight);
			yield return null;
		}
		UnityEngine.Object.Destroy(burnParticleSystem);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
