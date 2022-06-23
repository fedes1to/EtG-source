using System.Collections;
using Dungeonator;
using UnityEngine;

public class ActiveSummonItem : PlayerItem
{
	[EnemyIdentifier]
	public string CompanionGuid;

	public bool HasDoubleSynergy;

	[LongNumericEnum]
	public CustomSynergyType DoubleSynergy;

	public IntVector2 CustomClearance;

	public Vector2 CustomOffset;

	public bool IsTimed;

	public float Lifespan = 60f;

	public string IntroDirectionalAnimation;

	public string OutroDirectionalAnimation;

	public GameObject DepartureVFXPrefab;

	private GameObject m_extantCompanion;

	private GameObject m_extantSecondCompanion;

	private bool m_synergyActive;

	public override bool CanBeUsed(PlayerController user)
	{
		if (user.CurrentRoom == null)
		{
			return false;
		}
		if (!user.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
		{
			return false;
		}
		return base.CanBeUsed(user);
	}

	private void CreateCompanion(PlayerController owner)
	{
		AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(CompanionGuid);
		IntVector2 value = IntVector2.Max(CustomClearance, orLoadByGuid.Clearance);
		IntVector2? nearestAvailableCell = owner.CurrentRoom.GetNearestAvailableCell(owner.transform.position.XY(), value, CellTypes.FLOOR);
		if (!nearestAvailableCell.HasValue)
		{
			return;
		}
		GameObject targetCompanion = (m_extantCompanion = Object.Instantiate(orLoadByGuid.gameObject, (nearestAvailableCell.Value.ToVector2() + CustomOffset).ToVector3ZUp(), Quaternion.identity));
		CompanionController orAddComponent = m_extantCompanion.GetOrAddComponent<CompanionController>();
		orAddComponent.companionID = CompanionController.CompanionIdentifier.GATLING_GULL;
		orAddComponent.Initialize(owner);
		if (IsTimed)
		{
			owner.StartCoroutine(HandleLifespan(targetCompanion, owner));
		}
		if (!string.IsNullOrEmpty(IntroDirectionalAnimation))
		{
			AIAnimator component = orAddComponent.GetComponent<AIAnimator>();
			component.PlayUntilFinished(IntroDirectionalAnimation, true);
		}
		if (!HasDoubleSynergy || !owner.HasActiveBonusSynergy(DoubleSynergy))
		{
			return;
		}
		nearestAvailableCell = owner.CurrentRoom.GetNearestAvailableCell(owner.transform.position.XY() + new Vector2(-1f, -1f), value, CellTypes.FLOOR);
		if (nearestAvailableCell.HasValue)
		{
			m_extantSecondCompanion = Object.Instantiate(orLoadByGuid.gameObject, (nearestAvailableCell.Value.ToVector2() + CustomOffset).ToVector3ZUp(), Quaternion.identity);
			CompanionController orAddComponent2 = m_extantSecondCompanion.GetOrAddComponent<CompanionController>();
			orAddComponent2.Initialize(owner);
			if (!string.IsNullOrEmpty(IntroDirectionalAnimation))
			{
				AIAnimator component2 = orAddComponent2.GetComponent<AIAnimator>();
				component2.PlayUntilFinished(IntroDirectionalAnimation, true);
			}
		}
	}

	private void DestroyCompanion()
	{
		if ((bool)m_extantCompanion)
		{
			if (!string.IsNullOrEmpty(OutroDirectionalAnimation))
			{
				AIAnimator component = m_extantCompanion.GetComponent<AIAnimator>();
				GameManager.Instance.Dungeon.StartCoroutine(HandleDeparture(true, component));
			}
			else
			{
				Object.Destroy(m_extantCompanion);
				m_extantCompanion = null;
			}
		}
		if ((bool)m_extantSecondCompanion)
		{
			if (!string.IsNullOrEmpty(OutroDirectionalAnimation))
			{
				AIAnimator component2 = m_extantSecondCompanion.GetComponent<AIAnimator>();
				GameManager.Instance.Dungeon.StartCoroutine(HandleDeparture(false, component2));
			}
			else
			{
				Object.Destroy(m_extantSecondCompanion);
				m_extantSecondCompanion = null;
			}
		}
	}

	private IEnumerator HandleDeparture(bool isPrimary, AIAnimator anim)
	{
		anim.behaviorSpeculator.enabled = false;
		anim.specRigidbody.Velocity = Vector2.zero;
		anim.aiActor.ClearPath();
		anim.PlayForDuration(OutroDirectionalAnimation, 3f, true);
		float animLength = anim.GetDirectionalAnimationLength(OutroDirectionalAnimation);
		GameObject extantCompanion;
		if (isPrimary)
		{
			extantCompanion = m_extantCompanion;
			m_extantCompanion = null;
		}
		else
		{
			extantCompanion = m_extantSecondCompanion;
			m_extantSecondCompanion = null;
		}
		float elapsed2 = 0f;
		while (elapsed2 < animLength)
		{
			elapsed2 += BraveTime.DeltaTime;
			yield return null;
		}
		GameObject instanceVFXObject = null;
		if ((bool)anim)
		{
			instanceVFXObject = Object.Instantiate(DepartureVFXPrefab);
			tk2dBaseSprite component = instanceVFXObject.GetComponent<tk2dBaseSprite>();
			component.transform.position = anim.sprite.transform.position;
		}
		Object.Destroy(extantCompanion);
		if ((bool)instanceVFXObject)
		{
			Vector3 startPosition = instanceVFXObject.transform.position;
			elapsed2 = 0f;
			while (elapsed2 < 1.5f)
			{
				elapsed2 += BraveTime.DeltaTime;
				instanceVFXObject.transform.position = Vector3.Lerp(startPosition, startPosition + new Vector3(0f, 75f, 0f), elapsed2 / 1.5f);
				yield return null;
			}
			Object.Destroy(instanceVFXObject);
		}
	}

	protected override void DoEffect(PlayerController user)
	{
		DestroyCompanion();
		CreateCompanion(user);
	}

	protected override void OnPreDrop(PlayerController user)
	{
		base.OnPreDrop(user);
		if (base.IsCurrentlyActive)
		{
			base.IsCurrentlyActive = false;
			if ((bool)m_extantCompanion)
			{
				DestroyCompanion();
			}
		}
	}

	private IEnumerator HandleLifespan(GameObject targetCompanion, PlayerController owner)
	{
		base.IsCurrentlyActive = true;
		float elapsed = 0f;
		m_activeDuration = Lifespan;
		m_activeElapsed = 0f;
		while (elapsed < Lifespan)
		{
			elapsed = (m_activeElapsed = elapsed + BraveTime.DeltaTime);
			if (!owner || owner.CurrentRoom == null || !owner.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
			{
				break;
			}
			yield return null;
		}
		base.IsCurrentlyActive = false;
		if (m_extantCompanion == targetCompanion)
		{
			DestroyCompanion();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
