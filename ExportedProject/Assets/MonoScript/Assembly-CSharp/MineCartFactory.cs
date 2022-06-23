using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class MineCartFactory : DungeonPlaceableBehaviour
{
	public MineCartController MineCartPrefab;

	[DwarfConfigurable]
	public float TargetPathIndex;

	[DwarfConfigurable]
	public float TargetPathNodeIndex;

	[DwarfConfigurable]
	public float MaxCarts = 5f;

	[DwarfConfigurable]
	public float DelayBetweenCarts = 3f;

	[DwarfConfigurable]
	public float DelayUponDestruction = 1f;

	public bool ForceCartActive;

	[NonSerialized]
	private List<MineCartController> m_spawnedCarts;

	[NonSerialized]
	private float m_delayTimer = 1f;

	private RoomHandler m_room;

	private void Start()
	{
		m_room = GetAbsoluteParentRoom();
		m_spawnedCarts = new List<MineCartController>();
	}

	private void Update()
	{
		if (!GameManager.Instance.IsAnyPlayerInRoom(m_room))
		{
			return;
		}
		for (int i = 0; i < m_spawnedCarts.Count; i++)
		{
			if (!m_spawnedCarts[i])
			{
				m_spawnedCarts.RemoveAt(i);
				i--;
				m_delayTimer = Mathf.Max(DelayUponDestruction, m_delayTimer);
			}
		}
		if (m_delayTimer <= 0f && (float)m_spawnedCarts.Count < MaxCarts)
		{
			m_delayTimer = DelayBetweenCarts;
			DoSpawnCart();
		}
		m_delayTimer = Mathf.Max(0f, m_delayTimer - BraveTime.DeltaTime);
	}

	private IEnumerator DelayedApplyVelocity(MineCartController mcc)
	{
		yield return null;
		mcc.ApplyVelocity(mcc.MaxSpeedEnemy);
	}

	protected void DoSpawnCart()
	{
		RoomHandler absoluteParentRoom = GetAbsoluteParentRoom();
		GameObject gameObject = UnityEngine.Object.Instantiate(MineCartPrefab.gameObject, base.transform.position, Quaternion.identity);
		MineCartController component = gameObject.GetComponent<MineCartController>();
		PathMover component2 = gameObject.GetComponent<PathMover>();
		if (ForceCartActive)
		{
			component.ForceActive = true;
		}
		absoluteParentRoom.RegisterInteractable(component);
		component2.Path = absoluteParentRoom.area.runtimePrototypeData.paths[Mathf.RoundToInt(TargetPathIndex)];
		component2.PathStartNode = Mathf.RoundToInt(TargetPathNodeIndex);
		component2.RoomHandler = absoluteParentRoom;
		m_spawnedCarts.Add(component);
		if (component.occupation == MineCartController.CartOccupationState.EMPTY && (float)m_spawnedCarts.Count < MaxCarts)
		{
			StartCoroutine(DelayedApplyVelocity(component));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
