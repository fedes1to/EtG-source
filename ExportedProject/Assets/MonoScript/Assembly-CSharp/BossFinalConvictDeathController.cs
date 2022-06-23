using UnityEngine;

public class BossFinalConvictDeathController : BraveBehaviour
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
		ConvictPastController convictPastController = Object.FindObjectOfType<ConvictPastController>();
		convictPastController.OnBossKilled(base.transform);
	}
}
