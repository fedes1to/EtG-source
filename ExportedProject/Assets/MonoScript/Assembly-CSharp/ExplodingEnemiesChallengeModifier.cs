using System;
using UnityEngine;

public class ExplodingEnemiesChallengeModifier : ChallengeModifier
{
	public ExplosionData explosion;

	private void Start()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController obj = GameManager.Instance.AllPlayers[i];
			obj.OnAnyEnemyReceivedDamage = (Action<float, bool, HealthHaver>)Delegate.Combine(obj.OnAnyEnemyReceivedDamage, new Action<float, bool, HealthHaver>(OnEnemyDamaged));
		}
	}

	private void OnEnemyDamaged(float damage, bool fatal, HealthHaver enemyHealth)
	{
		if ((bool)enemyHealth && !enemyHealth.IsBoss && fatal && (bool)enemyHealth.aiActor && enemyHealth.aiActor.IsNormalEnemy)
		{
			string text = enemyHealth.name;
			if (!text.StartsWith("Bashellisk") && !text.StartsWith("Blobulin") && !text.StartsWith("Poisbulin"))
			{
				Exploder.Explode(enemyHealth.aiActor.CenterPosition, explosion, Vector2.zero);
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController obj = GameManager.Instance.AllPlayers[i];
			obj.OnAnyEnemyReceivedDamage = (Action<float, bool, HealthHaver>)Delegate.Remove(obj.OnAnyEnemyReceivedDamage, new Action<float, bool, HealthHaver>(OnEnemyDamaged));
		}
	}
}
