using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class WeightedGameObject
{
	[FormerlySerializedAs("gameObject")]
	public GameObject rawGameObject;

	[PickupIdentifier]
	public int pickupId = -1;

	public float weight;

	public bool forceDuplicatesPossible;

	public DungeonPrerequisite[] additionalPrerequisites;

	[NonSerialized]
	private bool m_hasCachedGameObject;

	[NonSerialized]
	private GameObject m_cachedGameObject;

	public GameObject gameObject
	{
		get
		{
			if (!m_hasCachedGameObject)
			{
				if (pickupId >= 0)
				{
					PickupObject byId = PickupObjectDatabase.GetById(pickupId);
					if ((bool)byId)
					{
						m_cachedGameObject = byId.gameObject;
					}
				}
				if (!m_cachedGameObject)
				{
					m_cachedGameObject = rawGameObject;
				}
				m_hasCachedGameObject = true;
			}
			return m_cachedGameObject;
		}
	}

	public void SetGameObject(GameObject gameObject)
	{
		m_cachedGameObject = gameObject;
		m_hasCachedGameObject = true;
	}

	public void SetGameObjectEditor(GameObject gameObject)
	{
		if ((bool)gameObject)
		{
			PickupObject component = gameObject.GetComponent<PickupObject>();
			if ((bool)component)
			{
				pickupId = component.PickupObjectId;
				rawGameObject = null;
				return;
			}
		}
		rawGameObject = gameObject;
	}
}
