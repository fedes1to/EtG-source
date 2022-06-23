using UnityEngine;

public class BossFinalRobotDeathController : BraveBehaviour
{
	public void Start()
	{
		base.healthHaver.OnPreDeath += OnBossDeath;
		base.healthHaver.OverrideKillCamTime = 1f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnBossDeath(Vector2 dir)
	{
		Object.FindObjectOfType<RobotPastController>().OnBossKilled(base.transform);
	}
}
