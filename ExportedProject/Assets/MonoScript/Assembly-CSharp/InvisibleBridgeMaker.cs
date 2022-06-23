using System.Collections.Generic;
using UnityEngine;

public class InvisibleBridgeMaker : DungeonPlaceableBehaviour
{
	public MovingPlatform InvisiblePlatform2x2;

	private List<MovingPlatform> m_extantPlatforms = new List<MovingPlatform>();

	private void Start()
	{
		RegenerateBridge();
		GameManager.Instance.PrimaryPlayer.OnReceivedDamage += HandlePlayerDamaged;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.SecondaryPlayer.OnReceivedDamage += HandlePlayerDamaged;
		}
	}

	protected override void OnDestroy()
	{
		if (GameManager.HasInstance)
		{
			if ((bool)GameManager.Instance.PrimaryPlayer)
			{
				GameManager.Instance.PrimaryPlayer.OnReceivedDamage -= HandlePlayerDamaged;
			}
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && (bool)GameManager.Instance.SecondaryPlayer)
			{
				GameManager.Instance.SecondaryPlayer.OnReceivedDamage -= HandlePlayerDamaged;
			}
			base.OnDestroy();
		}
	}

	private void HandlePlayerDamaged(PlayerController obj)
	{
		if (obj.CurrentRoom == GetAbsoluteParentRoom())
		{
			RegenerateBridge();
		}
	}

	private void AddPlatformPosition(IntVector2 position, List<IntVector2> points, HashSet<IntVector2> positions)
	{
		positions.Add(position);
		positions.Add(position + IntVector2.Up);
		positions.Add(position + IntVector2.Right);
		positions.Add(position + IntVector2.One);
		points.Add(position);
	}

	private IntVector2 RotateDir(IntVector2 curDir)
	{
		if (curDir.x > 0)
		{
			if (Random.value < 0.5f)
			{
				return IntVector2.Up;
			}
			return IntVector2.Down;
		}
		if (curDir.y < 0)
		{
			if (Random.value < 0.5f)
			{
				return IntVector2.Right;
			}
			return IntVector2.Right;
		}
		if (curDir.y > 0)
		{
			if (Random.value < 0.5f)
			{
				return IntVector2.Right;
			}
			return IntVector2.Right;
		}
		if (curDir.x < 0)
		{
			if (Random.value < 0.5f)
			{
				return IntVector2.Up;
			}
			return IntVector2.Down;
		}
		return curDir;
	}

	private bool IsPositionValid(IntVector2 testPosition, IntVector2 minPosition, IntVector2 maxPosition, HashSet<IntVector2> usedPositions)
	{
		return !usedPositions.Contains(testPosition) && testPosition.y >= minPosition.y && testPosition.y < maxPosition.y;
	}

	private MovingPlatform CreateNewPlatform(IntVector2 position)
	{
		GameObject gameObject = Object.Instantiate(InvisiblePlatform2x2.gameObject, position.ToVector3(), Quaternion.identity);
		gameObject.GetComponent<SpeculativeRigidbody>().Reinitialize();
		return gameObject.GetComponent<MovingPlatform>();
	}

	private void RegenerateBridge()
	{
		for (int i = 0; i < StaticReferenceManager.AllDebris.Count; i++)
		{
			if (StaticReferenceManager.AllDebris[i].transform.position.GetAbsoluteRoom() == GetAbsoluteParentRoom())
			{
				StaticReferenceManager.AllDebris[i].ForceUpdatePitfall();
			}
		}
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		IntVector2 maxPosition = intVector + new IntVector2(GetWidth(), GetHeight());
		int y = Random.Range(intVector.y, maxPosition.y - 1);
		List<IntVector2> list = new List<IntVector2>();
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		IntVector2 intVector2 = new IntVector2(intVector.x, y);
		IntVector2 intVector3 = IntVector2.Right;
		AddPlatformPosition(intVector2, list, hashSet);
		int num = 0;
		while (maxPosition.x - intVector2.x > 3)
		{
			IntVector2 intVector4 = intVector2 + intVector3 + intVector3;
			if (!IsPositionValid(intVector4, intVector, maxPosition, hashSet))
			{
				intVector3 = IntVector2.Right;
				intVector4 = intVector2 + intVector3 + intVector3;
				num = 0;
			}
			AddPlatformPosition(intVector4, list, hashSet);
			num++;
			if (num > 2 && Random.value < 0.5f)
			{
				IntVector2 intVector5 = RotateDir(intVector3);
				if (!IsPositionValid(intVector4 + intVector5 + intVector5, intVector, maxPosition, hashSet))
				{
					intVector5 = -1 * intVector5;
				}
				if (!IsPositionValid(intVector4 + intVector5 + intVector5, intVector, maxPosition, hashSet))
				{
					intVector5 = intVector3;
				}
				if (intVector5 != intVector3)
				{
					num = 0;
				}
				intVector3 = intVector5;
			}
			intVector2 = intVector4;
		}
		for (int j = 0; j < m_extantPlatforms.Count; j++)
		{
			m_extantPlatforms[j].ClearCells();
			Object.Destroy(m_extantPlatforms[j].gameObject);
		}
		m_extantPlatforms.Clear();
		for (int k = 0; k < list.Count; k++)
		{
			if (k >= m_extantPlatforms.Count)
			{
				m_extantPlatforms.Add(CreateNewPlatform(list[k]));
				continue;
			}
			m_extantPlatforms[k].ClearCells();
			m_extantPlatforms[k].transform.position = list[k].ToVector3();
			m_extantPlatforms[k].specRigidbody.Reinitialize();
			m_extantPlatforms[k].MarkCells();
		}
		if (m_extantPlatforms.Count > list.Count)
		{
			int num2;
			for (num2 = list.Count; num2 < m_extantPlatforms.Count; num2++)
			{
				m_extantPlatforms[num2].ClearCells();
				Object.Destroy(m_extantPlatforms[num2].gameObject);
				m_extantPlatforms.RemoveAt(num2);
				num2--;
			}
		}
	}
}
