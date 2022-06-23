using UnityEngine;

public class BulletBroDeathController : BraveBehaviour
{
	private void Start()
	{
		base.healthHaver.OnDeath += OnDeath;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnDeath(Vector2 finalDeathDir)
	{
		BroController otherBro = BroController.GetOtherBro(base.gameObject);
		if ((bool)otherBro)
		{
			otherBro.Enrage();
		}
		else
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_BULLET_BROS, true);
		}
	}
}
