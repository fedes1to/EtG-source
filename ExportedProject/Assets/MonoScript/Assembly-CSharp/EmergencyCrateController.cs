using System.Collections;
using Dungeonator;
using UnityEngine;

public class EmergencyCrateController : BraveBehaviour
{
	public string driftAnimationName;

	public string driftSucksAnimationName;

	public string landedAnimationName;

	public string landedSucksAnimationName;

	public string chuteLandedAnimationName;

	public string crateDisappearAnimationName;

	public string shittyCrateDisappearAnimationName;

	public tk2dSpriteAnimator chuteAnimator;

	public GameObject landingTargetSprite;

	public bool usesLootData;

	public LootData lootData;

	public GenericLootTable gunTable;

	public float ChanceToSpawnEnemy;

	public DungeonPlaceable EnemyPlaceable;

	public float ChanceToExplode;

	public ExplosionData ExplosionData;

	private bool m_hasBeenTriggered;

	private Vector3 m_currentPosition;

	private Vector3 m_currentVelocity;

	private RoomHandler m_parentRoom;

	private bool m_crateSucks;

	private GameObject m_landingTarget;

	public void Trigger(Vector3 startingVelocity, Vector3 startingPosition, RoomHandler room, bool crateSucks = true)
	{
		m_parentRoom = room;
		m_currentPosition = startingPosition;
		m_currentVelocity = startingVelocity;
		m_hasBeenTriggered = true;
		base.spriteAnimator.Play((!crateSucks) ? driftAnimationName : driftSucksAnimationName);
		m_crateSucks = crateSucks;
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unoccluded"));
		float num = startingPosition.z / (0f - startingVelocity.z);
		Vector3 position = startingPosition + num * startingVelocity;
		m_landingTarget = SpawnManager.SpawnVFX(landingTargetSprite, position, Quaternion.identity);
		m_landingTarget.GetComponentInChildren<tk2dSprite>().UpdateZDepth();
	}

	private void Update()
	{
		if (m_hasBeenTriggered)
		{
			m_currentPosition += m_currentVelocity * BraveTime.DeltaTime;
			if (m_currentPosition.z <= 0f)
			{
				m_currentPosition.z = 0f;
				OnLanded();
			}
			base.transform.position = BraveUtility.QuantizeVector(m_currentPosition.WithZ(m_currentPosition.y - m_currentPosition.z), PhysicsEngine.Instance.PixelsPerUnit);
			base.sprite.HeightOffGround = m_currentPosition.z;
			base.sprite.UpdateZDepth();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnLanded()
	{
		m_hasBeenTriggered = false;
		base.sprite.gameObject.layer = LayerMask.NameToLayer("FG_Critical");
		base.sprite.renderer.sortingLayerName = "Background";
		base.sprite.IsPerpendicular = false;
		base.sprite.HeightOffGround = -1f;
		m_currentPosition.z = -1f;
		base.spriteAnimator.Play((!m_crateSucks) ? landedAnimationName : landedSucksAnimationName);
		chuteAnimator.PlayAndDestroyObject(chuteLandedAnimationName);
		if ((bool)m_landingTarget)
		{
			SpawnManager.Despawn(m_landingTarget);
		}
		m_landingTarget = null;
		if (Random.value < ChanceToExplode)
		{
			Exploder.Explode(base.sprite.WorldCenter, ExplosionData, Vector2.zero, null, true);
			StartCoroutine(DestroyCrateDelayed());
			return;
		}
		if (Random.value < ChanceToSpawnEnemy)
		{
			DungeonPlaceableVariant dungeonPlaceableVariant = EnemyPlaceable.SelectFromTiersFull();
			if (dungeonPlaceableVariant != null && dungeonPlaceableVariant.GetOrLoadPlaceableObject != null)
			{
				AIActor.Spawn(dungeonPlaceableVariant.GetOrLoadPlaceableObject.GetComponent<AIActor>(), base.sprite.WorldCenter.ToIntVector2(), GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.sprite.WorldCenter.ToIntVector2()), true);
			}
			StartCoroutine(DestroyCrateDelayed());
			return;
		}
		GameObject gameObject = null;
		gameObject = ((!usesLootData) ? gunTable.SelectByWeight() : lootData.GetItemsForPlayer(GameManager.Instance.PrimaryPlayer)[0].gameObject);
		if (gameObject.GetComponent<AmmoPickup>() != null)
		{
			AmmoPickup component = LootEngine.SpawnItem(gameObject, base.sprite.WorldCenter.ToVector3ZUp() + new Vector3(-0.5f, 0.5f, 0f), Vector2.zero, 0f, false).GetComponent<AmmoPickup>();
			StartCoroutine(DestroyCrateWhenPickedUp(component));
		}
		else if (gameObject.GetComponent<Gun>() != null)
		{
			GameObject gameObject2 = Object.Instantiate(gameObject);
			gameObject2.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(base.sprite.WorldCenter.ToVector3ZUp() + new Vector3(0f, 0.5f, 0f), tk2dBaseSprite.Anchor.MiddleCenter);
			Gun component2 = gameObject2.GetComponent<Gun>();
			component2.Initialize(null);
			component2.DropGun();
			StartCoroutine(DestroyCrateWhenPickedUp(component2));
		}
		else
		{
			DebrisObject spawned = LootEngine.SpawnItem(gameObject, base.sprite.WorldCenter.ToVector3ZUp() + new Vector3(-0.5f, 0.5f, 0f), Vector2.zero, 0f, false);
			StartCoroutine(DestroyCrateWhenPickedUp(spawned));
		}
	}

	private IEnumerator DestroyCrateDelayed()
	{
		yield return new WaitForSeconds(1.5f);
		if ((bool)m_landingTarget)
		{
			SpawnManager.Despawn(m_landingTarget);
		}
		m_landingTarget = null;
		if (m_parentRoom.ExtantEmergencyCrate == base.gameObject)
		{
			m_parentRoom.ExtantEmergencyCrate = null;
		}
		base.spriteAnimator.Play((!m_crateSucks) ? crateDisappearAnimationName : shittyCrateDisappearAnimationName);
	}

	private IEnumerator DestroyCrateWhenPickedUp(DebrisObject spawned)
	{
		while ((bool)spawned)
		{
			yield return new WaitForSeconds(0.25f);
		}
		if ((bool)m_landingTarget)
		{
			SpawnManager.Despawn(m_landingTarget);
		}
		m_landingTarget = null;
		if (m_parentRoom.ExtantEmergencyCrate == base.gameObject)
		{
			m_parentRoom.ExtantEmergencyCrate = null;
		}
		base.spriteAnimator.Play((!m_crateSucks) ? crateDisappearAnimationName : shittyCrateDisappearAnimationName);
	}

	private IEnumerator DestroyCrateWhenPickedUp(AmmoPickup spawnedAmmo)
	{
		while ((bool)spawnedAmmo && !spawnedAmmo.pickedUp)
		{
			yield return new WaitForSeconds(0.25f);
		}
		if ((bool)m_landingTarget)
		{
			SpawnManager.Despawn(m_landingTarget);
		}
		m_landingTarget = null;
		if (m_parentRoom.ExtantEmergencyCrate == base.gameObject)
		{
			m_parentRoom.ExtantEmergencyCrate = null;
		}
		base.spriteAnimator.Play((!m_crateSucks) ? crateDisappearAnimationName : shittyCrateDisappearAnimationName);
	}

	private IEnumerator DestroyCrateWhenPickedUp(Gun spawnedGun)
	{
		while ((bool)spawnedGun && spawnedGun.IsInWorld)
		{
			yield return new WaitForSeconds(0.25f);
		}
		if ((bool)m_landingTarget)
		{
			SpawnManager.Despawn(m_landingTarget);
		}
		m_landingTarget = null;
		if (m_parentRoom.ExtantEmergencyCrate == base.gameObject)
		{
			m_parentRoom.ExtantEmergencyCrate = null;
		}
		base.spriteAnimator.Play((!m_crateSucks) ? crateDisappearAnimationName : shittyCrateDisappearAnimationName);
	}

	public void ClearLandingTarget()
	{
		if ((bool)m_landingTarget)
		{
			SpawnManager.Despawn(m_landingTarget);
		}
		m_landingTarget = null;
	}
}
