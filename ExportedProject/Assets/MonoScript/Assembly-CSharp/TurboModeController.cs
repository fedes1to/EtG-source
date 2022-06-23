using UnityEngine;

public class TurboModeController : MonoBehaviour
{
	public static float sPlayerSpeedMultiplier = 1.4f;

	public static float sPlayerRollSpeedMultiplier = 1.4f;

	public static float sEnemyBulletSpeedMultiplier = 1.3f;

	public static float sEnemyMovementSpeedMultiplier = 1.5f;

	public static float sEnemyCooldownMultiplier = 0.5f;

	public static float sEnemyWakeTimeMultiplier = 4f;

	public static float sEnemyAnimSpeed = 1f;

	public float PlayerSpeedMultiplier = 1.4f;

	public float PlayerRollSpeedMultiplier = 1.4f;

	public float EnemyBulletSpeedMultiplier = 1.3f;

	public float EnemyMovementSpeedMultiplier = 1.5f;

	public float EnemyCooldownMultiplier = 0.5f;

	public float EnemyWakeTimeMultiplier = 4f;

	public float EnemyAnimSpeed = 1f;

	public static bool IsActive
	{
		get
		{
			return GameManager.IsTurboMode;
		}
	}

	public void Update()
	{
		sPlayerSpeedMultiplier = PlayerSpeedMultiplier;
		sPlayerRollSpeedMultiplier = PlayerRollSpeedMultiplier;
		sEnemyBulletSpeedMultiplier = EnemyBulletSpeedMultiplier;
		sEnemyMovementSpeedMultiplier = EnemyMovementSpeedMultiplier;
		sEnemyCooldownMultiplier = EnemyCooldownMultiplier;
		sEnemyWakeTimeMultiplier = EnemyWakeTimeMultiplier;
		sEnemyAnimSpeed = EnemyAnimSpeed;
	}

	public static float MaybeModifyEnemyBulletSpeed(float speed)
	{
		if (GameManager.IsTurboMode)
		{
			return speed * sEnemyBulletSpeedMultiplier;
		}
		return speed;
	}

	public static float MaybeModifyEnemyMovementSpeed(float speed)
	{
		if (GameManager.IsTurboMode)
		{
			return speed * sEnemyMovementSpeedMultiplier;
		}
		return speed;
	}
}
