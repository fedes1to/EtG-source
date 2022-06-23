using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class DecalObject : EphemeralObject
{
	private static Dictionary<RoomHandler, List<DecalObject>> m_roomMap = new Dictionary<RoomHandler, List<DecalObject>>();

	public bool IsRoomLimited;

	[ShowInInspectorIf("IsRoomLimited", false)]
	public int MaxNumberInRoom = 5;

	private RoomHandler m_parent;

	public static void ClearPerLevelData()
	{
		m_roomMap.Clear();
	}

	public override void Start()
	{
		base.Start();
		if (IsRoomLimited)
		{
			m_parent = base.transform.position.GetAbsoluteRoom();
			if (!m_roomMap.ContainsKey(m_parent))
			{
				m_roomMap.Add(m_parent, new List<DecalObject>());
			}
			m_roomMap[m_parent].Add(this);
			if (m_roomMap[m_parent].Count > MaxNumberInRoom)
			{
				DecalObject decalObject = m_roomMap[m_parent][0];
				m_roomMap[m_parent].RemoveAt(0);
				decalObject.StartCoroutine(decalObject.FadeAndDestroy(decalObject));
			}
		}
	}

	public IEnumerator FadeAndDestroy(DecalObject decal)
	{
		float elapsed = 0f;
		float duration = 0.5f;
		tk2dBaseSprite spr = decal.sprite;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			if ((bool)spr)
			{
				spr.color = spr.color.WithAlpha(Mathf.Lerp(1f, 0f, elapsed / duration));
			}
			yield return null;
		}
		Object.Destroy(decal.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (IsRoomLimited && m_roomMap.ContainsKey(m_parent))
		{
			m_roomMap[m_parent].Remove(this);
		}
	}
}
