using System.Collections;
using UnityEngine;

public class BigShotgunController : MonoBehaviour
{
	[EnemyIdentifier]
	public string[] TargetEnemies;

	public float SuckRadius = 8f;

	private Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	private void LateUpdate()
	{
		if ((bool)m_gun && m_gun.IsReloading && m_gun.CurrentOwner is PlayerController)
		{
			PlayerController playerController = m_gun.CurrentOwner as PlayerController;
			if (playerController.CurrentRoom != null)
			{
				playerController.CurrentRoom.ApplyActionToNearbyEnemies(playerController.CenterPosition, SuckRadius, ProcessEnemy);
			}
		}
	}

	private void ProcessEnemy(AIActor target, float distance)
	{
		for (int i = 0; i < TargetEnemies.Length; i++)
		{
			if (target.EnemyGuid == TargetEnemies[i])
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleEnemySuck(target));
				target.EraseFromExistence(true);
				break;
			}
		}
	}

	private IEnumerator HandleEnemySuck(AIActor target)
	{
		Transform copySprite = CreateEmptySprite(target);
		Vector3 startPosition = copySprite.transform.position;
		float elapsed = 0f;
		float duration = 0.5f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			if ((bool)m_gun && (bool)copySprite)
			{
				Vector3 position = m_gun.PrimaryHandAttachPoint.position;
				float t = elapsed / duration * (elapsed / duration);
				copySprite.position = Vector3.Lerp(startPosition, position, t);
				copySprite.rotation = Quaternion.Euler(0f, 0f, 360f * BraveTime.DeltaTime) * copySprite.rotation;
				copySprite.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.1f, 0.1f, 0.1f), t);
			}
			yield return null;
		}
		if ((bool)copySprite)
		{
			Object.Destroy(copySprite.gameObject);
		}
		if ((bool)m_gun)
		{
			m_gun.GainAmmo(1);
		}
	}

	private Transform CreateEmptySprite(AIActor target)
	{
		GameObject gameObject = new GameObject("suck image");
		gameObject.layer = target.gameObject.layer;
		tk2dSprite tk2dSprite2 = gameObject.AddComponent<tk2dSprite>();
		gameObject.transform.parent = SpawnManager.Instance.VFX;
		tk2dSprite2.SetSprite(target.sprite.Collection, target.sprite.spriteId);
		tk2dSprite2.transform.position = target.sprite.transform.position;
		GameObject gameObject2 = new GameObject("image parent");
		gameObject2.transform.position = tk2dSprite2.WorldCenter;
		tk2dSprite2.transform.parent = gameObject2.transform;
		if (target.optionalPalette != null)
		{
			tk2dSprite2.renderer.material.SetTexture("_PaletteTex", target.optionalPalette);
		}
		return gameObject2.transform;
	}
}
