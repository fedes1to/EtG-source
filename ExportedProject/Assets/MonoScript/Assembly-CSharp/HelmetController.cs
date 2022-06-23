using UnityEngine;

public class HelmetController : BraveBehaviour
{
	public GameObject helmetEffect;

	public float helmetForce = 5f;

	public void Start()
	{
		base.healthHaver.OnPreDeath += OnPreDeath;
	}

	protected override void OnDestroy()
	{
		base.healthHaver.OnPreDeath -= OnPreDeath;
		base.OnDestroy();
	}

	public void OnPreDeath(Vector2 finalDamageDirection)
	{
		if (!base.aiActor.IsFalling && helmetEffect != null)
		{
			GameObject gameObject = SpawnManager.SpawnDebris(helmetEffect, base.specRigidbody.UnitTopLeft, Quaternion.identity);
			DebrisObject component = gameObject.GetComponent<DebrisObject>();
			if ((bool)component)
			{
				component.Trigger(finalDamageDirection.normalized * helmetForce, 1f);
			}
		}
	}
}
