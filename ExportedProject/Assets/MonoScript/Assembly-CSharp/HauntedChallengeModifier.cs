using System;
using Dungeonator;
using UnityEngine;

public class HauntedChallengeModifier : ChallengeModifier
{
	[EnemyIdentifier]
	public string GhostGuid;

	public float Chance = 0.5f;

	public string GhostOverrideSpawnAnimation;

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
		if (!enemyHealth || enemyHealth.IsBoss || !fatal || !(UnityEngine.Random.value < Chance) || !enemyHealth.aiActor || !enemyHealth.aiActor.IsNormalEnemy || !(enemyHealth.aiActor.ActorName != "Hollow Point") || (bool)enemyHealth.GetComponent<SpawnEnemyOnDeath>())
		{
			return;
		}
		string actorName = enemyHealth.aiActor.ActorName;
		if (actorName == "Blobulin" || actorName == "Bombshee" || actorName.Contains("Bullat") || (actorName.StartsWith("Mine Flayer ") && UnityEngine.Random.value > 0.25f))
		{
			return;
		}
		PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
		if (enemyHealth.aiActor.ParentRoom != bestActivePlayer.CurrentRoom || enemyHealth.aiActor.ParentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.RoomClear) <= 0)
		{
			return;
		}
		Vector2 centerPosition = enemyHealth.aiActor.CenterPosition;
		IntVector2 intVector = centerPosition.ToIntVector2(VectorConversions.Floor);
		if (!GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
		{
			return;
		}
		CellData cellData = GameManager.Instance.Dungeon.data[intVector];
		if (cellData.isExitCell || cellData.parentRoom != bestActivePlayer.CurrentRoom || centerPosition.GetAbsoluteRoom() != bestActivePlayer.CurrentRoom)
		{
			return;
		}
		AIActor aIActor = AIActor.Spawn(EnemyDatabase.GetOrLoadByGuid(GhostGuid), centerPosition, bestActivePlayer.CurrentRoom, true);
		if ((bool)aIActor && !string.IsNullOrEmpty(GhostOverrideSpawnAnimation))
		{
			AIAnimator.NamedDirectionalAnimation namedDirectionalAnimation = aIActor.aiAnimator.OtherAnimations.Find((AIAnimator.NamedDirectionalAnimation vfx) => vfx.name == "awaken");
			if (namedDirectionalAnimation != null)
			{
				namedDirectionalAnimation.anim.Prefix = GhostOverrideSpawnAnimation;
			}
		}
		aIActor.HasBeenEngaged = true;
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
