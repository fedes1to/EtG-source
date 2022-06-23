using System.Collections;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class ZoneControlChallengeModifier : ChallengeModifier
{
	public DungeonPlaceable BoxPlaceable;

	public float AuraRadius = 5f;

	public float WinTimer = 10f;

	public float DecayScale = 0.1875f;

	public int MinBoxes = 2;

	public int ExtraBoxAboveArea = 60;

	public int ExtraBoxEveryArea = 30;

	private FlippableCover[] m_instanceBox;

	private float m_timeElapsed;

	private void Start()
	{
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		int num = MinBoxes;
		int count = currentRoom.Cells.Count;
		for (count -= ExtraBoxAboveArea; count > 0; count -= ExtraBoxEveryArea)
		{
			num++;
		}
		num = Mathf.Clamp(num, MinBoxes, 11);
		m_instanceBox = new FlippableCover[num];
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			CellData cellData = GameManager.Instance.Dungeon.data[c];
			if (cellData == null || cellData.containsTrap || cellData.isOccupied)
			{
				return false;
			}
			for (int j = 0; j < m_instanceBox.Length; j++)
			{
				if (m_instanceBox[j] != null && Vector2.Distance(m_instanceBox[j].specRigidbody.UnitCenter, c.ToCenterVector2()) < 5f)
				{
					return false;
				}
			}
			return true;
		};
		for (int i = 0; i < num; i++)
		{
			IntVector2? randomAvailableCell = currentRoom.GetRandomAvailableCell(new IntVector2(4, 4), CellTypes.FLOOR, false, cellValidator);
			if (randomAvailableCell.HasValue)
			{
				GameObject gameObject = BoxPlaceable.InstantiateObject(currentRoom, randomAvailableCell.Value + IntVector2.One - currentRoom.area.basePosition);
				m_instanceBox[i] = gameObject.GetComponent<FlippableCover>();
				m_instanceBox[i].GetComponentInChildren<tk2dSpriteAnimator>().Play("moving_box_in");
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(m_instanceBox[i].specRigidbody);
			}
		}
	}

	private void UpdateAnimation(tk2dSpriteAnimator anim, bool playerRadius)
	{
		if (!anim.IsPlaying("moving_box_in") && !anim.IsPlaying("moving_box_out"))
		{
			if (playerRadius && !anim.IsPlaying("moving_box_open"))
			{
				anim.Play("moving_box_open");
			}
			if (!playerRadius && !anim.IsPlaying("moving_box_close"))
			{
				anim.Play("moving_box_close");
			}
		}
	}

	private void Update()
	{
		bool flag = false;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].IsGunLocked = true;
		}
		for (int j = 0; j < m_instanceBox.Length; j++)
		{
			if (!m_instanceBox[j])
			{
				continue;
			}
			flag = true;
			bool playerRadius = false;
			for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[k];
				if (Vector2.Distance(m_instanceBox[j].specRigidbody.UnitCenter, playerController.CenterPosition) < AuraRadius)
				{
					playerController.IsGunLocked = false;
					m_timeElapsed = Mathf.Clamp(m_timeElapsed + BraveTime.DeltaTime, 0f, WinTimer + 1f);
					playerRadius = true;
				}
				else
				{
					m_timeElapsed = Mathf.Clamp(m_timeElapsed - BraveTime.DeltaTime * DecayScale, 0f, WinTimer + 1f);
				}
			}
			UpdateAnimation(m_instanceBox[j].spriteAnimator, playerRadius);
		}
		if (!flag)
		{
			for (int l = 0; l < GameManager.Instance.AllPlayers.Length; l++)
			{
				GameManager.Instance.AllPlayers[l].IsGunLocked = false;
			}
		}
		float num = Mathf.Lerp(0.01f, 1f, m_timeElapsed / WinTimer);
		for (int m = 0; m < m_instanceBox.Length; m++)
		{
			if ((bool)m_instanceBox[m])
			{
				m_instanceBox[m].outlineEast.GetComponent<tk2dSprite>().scale = new Vector3(num, num, num);
			}
		}
		if (m_timeElapsed >= WinTimer)
		{
			PopBox();
		}
	}

	private void PopBox()
	{
		if (!GameManager.HasInstance || !GameManager.Instance.Dungeon)
		{
			return;
		}
		for (int i = 0; i < m_instanceBox.Length; i++)
		{
			if ((bool)m_instanceBox[i])
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleBoxPop(m_instanceBox[i]));
				m_instanceBox[i] = null;
			}
		}
	}

	private IEnumerator HandleBoxPop(FlippableCover box)
	{
		float elapsed = 0f;
		float duration = box.spriteAnimator.GetClipByName("moving_box_out").BaseClipLength;
		box.spriteAnimator.Play("moving_box_out");
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			if ((bool)box.outlineNorth)
			{
				box.outlineNorth.GetComponent<tk2dSprite>().scale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
			}
			if ((bool)box.outlineEast)
			{
				box.outlineEast.GetComponent<tk2dSprite>().scale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
			}
			yield return null;
		}
		LootEngine.DoDefaultPurplePoof(box.specRigidbody.UnitBottomCenter);
		Object.Destroy(box.gameObject);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].IsGunLocked = false;
		}
		PopBox();
	}
}
