using UnityEngine;

public class GatlingGullDeathController : BraveBehaviour
{
	public void Start()
	{
		base.healthHaver.OnPreDeath += OnBossDeath;
	}

	protected override void OnDestroy()
	{
		if (GameManager.HasInstance)
		{
			Cleanup();
		}
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		Cleanup();
	}

	private void Cleanup()
	{
		SkyRocket[] array = Object.FindObjectsOfType<SkyRocket>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DieInAir();
		}
	}
}
