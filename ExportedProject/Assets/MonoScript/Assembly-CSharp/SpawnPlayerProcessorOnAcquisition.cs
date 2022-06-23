using System;
using UnityEngine;

public class SpawnPlayerProcessorOnAcquisition : MonoBehaviour
{
	public GameObject PrefabToSpawn;

	public string Identifier;

	private PassiveItem m_passiveItem;

	private PlayerItem m_playerItem;

	public void Awake()
	{
		m_passiveItem = GetComponent<PassiveItem>();
		m_playerItem = GetComponent<PlayerItem>();
		if ((bool)m_passiveItem)
		{
			PassiveItem passiveItem = m_passiveItem;
			passiveItem.OnPickedUp = (Action<PlayerController>)Delegate.Combine(passiveItem.OnPickedUp, new Action<PlayerController>(HandlePickedUp));
		}
		if ((bool)m_playerItem)
		{
			PlayerItem playerItem = m_playerItem;
			playerItem.OnPickedUp = (Action<PlayerController>)Delegate.Combine(playerItem.OnPickedUp, new Action<PlayerController>(HandlePickedUp));
		}
	}

	private void HandlePickedUp(PlayerController p)
	{
		if ((bool)p && !p.SpawnedSubobjects.ContainsKey(Identifier))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(PrefabToSpawn);
			gameObject.transform.parent = p.transform;
			gameObject.transform.localPosition = Vector3.zero;
			p.SpawnedSubobjects.Add(Identifier, gameObject);
		}
	}
}
