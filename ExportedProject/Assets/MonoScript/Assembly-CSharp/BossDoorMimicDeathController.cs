using UnityEngine;

public class BossDoorMimicDeathController : BraveBehaviour
{
	public void Start()
	{
		base.healthHaver.OnPreDeath += OnBossDeath;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		BossDoorMimicIntroDoer component = GetComponent<BossDoorMimicIntroDoer>();
		if ((bool)component)
		{
			component.PhantomDoorBlocker.Unseal();
		}
	}
}
