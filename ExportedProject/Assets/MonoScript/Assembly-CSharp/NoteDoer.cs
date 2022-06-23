using System;
using Dungeonator;
using UnityEngine;

public class NoteDoer : DungeonPlaceableBehaviour, IPlayerInteractable
{
	public enum NoteBackgroundType
	{
		LETTER,
		STONE,
		WOOD,
		NOTE
	}

	public NoteBackgroundType noteBackgroundType;

	public string stringKey;

	public bool useAdditionalStrings;

	public string[] additionalStrings;

	public bool isNormalNote;

	public bool useItemsTable;

	[NonSerialized]
	public bool alreadyLocalized;

	public Transform textboxSpawnPoint;

	private bool m_boxIsExtant;

	public bool DestroyedOnFinish;

	private string m_selectedDisplayString;

	public void Start()
	{
		if (base.majorBreakable != null)
		{
			MajorBreakable obj = base.majorBreakable;
			obj.OnBreak = (Action)Delegate.Combine(obj.OnBreak, new Action(OnBroken));
		}
	}

	private void OnBroken()
	{
		GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor)).DeregisterInteractable(this);
	}

	private void OnDisable()
	{
		if (m_boxIsExtant)
		{
			GameUIRoot.Instance.ShowCoreUI(string.Empty);
			m_boxIsExtant = false;
			TextBoxManager.ClearTextBoxImmediate(textboxSpawnPoint);
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!base.sprite)
		{
			if (m_boxIsExtant)
			{
				ClearBox();
			}
			return 1000f;
		}
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			if (m_boxIsExtant)
			{
				ClearBox();
			}
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	private void ClearBox()
	{
		GameUIRoot.Instance.ShowCoreUI(string.Empty);
		m_boxIsExtant = false;
		TextBoxManager.ClearTextBox(textboxSpawnPoint);
		if (DestroyedOnFinish)
		{
			GetAbsoluteParentRoom().DeregisterInteractable(this);
			RoomHandler.unassignedInteractableObjects.Remove(this);
			LootEngine.DoDefaultItemPoof(base.sprite.WorldCenter);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (m_boxIsExtant)
		{
			ClearBox();
			return;
		}
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		m_boxIsExtant = true;
		string text = m_selectedDisplayString;
		if (m_selectedDisplayString == null)
		{
			text = (alreadyLocalized ? stringKey : ((!useItemsTable) ? StringTableManager.GetLongString(stringKey) : StringTableManager.GetItemsLongDescription(stringKey)));
			if (useAdditionalStrings)
			{
				if (isNormalNote)
				{
					text = (alreadyLocalized ? additionalStrings[UnityEngine.Random.Range(0, additionalStrings.Length)] : ((!useItemsTable) ? StringTableManager.GetLongString(additionalStrings[UnityEngine.Random.Range(0, additionalStrings.Length)]) : StringTableManager.GetItemsLongDescription(additionalStrings[UnityEngine.Random.Range(0, additionalStrings.Length)])));
				}
				else if (GameStatsManager.Instance.GetFlag(GungeonFlags.SECRET_NOTE_ENCOUNTERED))
				{
					text = (alreadyLocalized ? additionalStrings[UnityEngine.Random.Range(0, additionalStrings.Length)] : ((!useItemsTable) ? StringTableManager.GetLongString(additionalStrings[UnityEngine.Random.Range(0, additionalStrings.Length)]) : StringTableManager.GetItemsLongDescription(additionalStrings[UnityEngine.Random.Range(0, additionalStrings.Length)])));
				}
			}
			if (stringKey == "#IRONCOIN_SHORTDESC")
			{
				text = " \n" + text + "\n ";
			}
			m_selectedDisplayString = text;
		}
		switch (noteBackgroundType)
		{
		case NoteBackgroundType.LETTER:
			TextBoxManager.ShowLetterBox(textboxSpawnPoint.position, textboxSpawnPoint, -1f, text);
			break;
		case NoteBackgroundType.STONE:
			TextBoxManager.ShowStoneTablet(textboxSpawnPoint.position, textboxSpawnPoint, -1f, text);
			break;
		case NoteBackgroundType.WOOD:
			TextBoxManager.ShowWoodPanel(textboxSpawnPoint.position, textboxSpawnPoint, -1f, text);
			break;
		case NoteBackgroundType.NOTE:
			TextBoxManager.ShowNote(textboxSpawnPoint.position, textboxSpawnPoint, -1f, text);
			break;
		}
		if (useAdditionalStrings && !isNormalNote)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.SECRET_NOTE_ENCOUNTERED, true);
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
