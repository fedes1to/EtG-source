using System.Collections;
using Dungeonator;
using UnityEngine;

public class EnemyFactorySpawnPoint : DungeonPlaceableBehaviour
{
	public tk2dSpriteAnimator animator;

	public string spawnAnimationOpen = string.Empty;

	public string spawnAnimationClose = string.Empty;

	public float preSpawnDelay = 1f;

	public float postSpawnDelay = 0.5f;

	public GameObject spawnVFX;

	public void OnSpawn(AIActor actorToSpawn, IntVector2 spawnPosition, RoomHandler room)
	{
		StartCoroutine(HandleSpawnAnimations(actorToSpawn, spawnPosition, room));
	}

	private IEnumerator HandleSpawnAnimations(AIActor actorToSpawn, IntVector2 spawnPosition, RoomHandler room)
	{
		if (!string.IsNullOrEmpty(spawnAnimationOpen))
		{
			animator.Play(spawnAnimationOpen);
		}
		yield return new WaitForSeconds(preSpawnDelay);
		if (spawnVFX != null)
		{
			GameObject gameObject = SpawnManager.SpawnVFX(spawnVFX, spawnPosition.ToVector3(), Quaternion.identity);
			gameObject.GetComponent<tk2dSprite>().PlaceAtPositionByAnchor(spawnPosition.ToVector3(), tk2dBaseSprite.Anchor.LowerCenter);
		}
		AIActor.Spawn(actorToSpawn, spawnPosition, room);
		yield return new WaitForSeconds(postSpawnDelay);
		if (!string.IsNullOrEmpty(spawnAnimationClose))
		{
			animator.Play(spawnAnimationClose);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
