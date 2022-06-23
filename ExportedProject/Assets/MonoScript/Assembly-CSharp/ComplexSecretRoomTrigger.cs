using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using InControl;
using UnityEngine;

public class ComplexSecretRoomTrigger : BraveBehaviour, IPlayerInteractable
{
	public string introStringKey;

	[PickupIdentifier]
	public List<int> RequiredObjectIds;

	public List<string> RequiredItemSupplyKeys;

	public Transform speakPoint;

	private List<bool> SuppliedObjects = new List<bool>();

	public SecretRoomManager referencedSecretRoom;

	public RoomHandler parentRoom;

	private bool m_isInteracting;

	public void Initialize(RoomHandler room)
	{
		parentRoom = room;
		parentRoom.RegisterInteractable(this);
		List<PickupObject> list = new List<PickupObject>();
		for (int i = 0; i < RequiredObjectIds.Count; i++)
		{
			SuppliedObjects.Add(false);
			list.Add(PickupObjectDatabase.GetById(RequiredObjectIds[i]));
		}
		GameManager.Instance.Dungeon.data.DistributeComplexSecretPuzzleItems(list, parentRoom);
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		return Vector2.Distance(point, base.sprite.WorldCenter);
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (!referencedSecretRoom.IsOpen && (bool)this)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (!m_isInteracting)
		{
			m_isInteracting = true;
			StartCoroutine(HandleDialog(interactor));
		}
	}

	private IEnumerator HandleDialog(PlayerController player)
	{
		TextBoxManager.ShowTextBox(text: StringTableManager.GetString(introStringKey), worldPosition: speakPoint.position + new Vector3(0f, 0f, -5f), parent: speakPoint, duration: -1f, audioTag: string.Empty);
		yield return StartCoroutine(WaitForPlayer());
		TextBoxManager.ClearTextBox(speakPoint);
		for (int i = 0; i < RequiredObjectIds.Count; i++)
		{
			if (SuppliedObjects[i])
			{
				continue;
			}
			for (int j = 0; j < player.additionalItems.Count; j++)
			{
				yield return null;
				PickupObject requiredObject = PickupObjectDatabase.GetById(RequiredObjectIds[i]);
				if (player.additionalItems[j].DisplayName == requiredObject.DisplayName)
				{
					TextBoxManager.ShowTextBox(speakPoint.position + new Vector3(0f, 0f, -5f), speakPoint, -1f, StringTableManager.GetString(RequiredItemSupplyKeys[i]), string.Empty);
					yield return StartCoroutine(WaitForPlayerYesNo(player, i, j));
					break;
				}
			}
		}
		bool allComplete = true;
		for (int k = 0; k < SuppliedObjects.Count; k++)
		{
			if (!SuppliedObjects[k])
			{
				allComplete = false;
				break;
			}
		}
		if (allComplete)
		{
			parentRoom.DeregisterInteractable(this);
			if (!referencedSecretRoom.IsOpen)
			{
				referencedSecretRoom.OpenDoor();
			}
		}
		m_isInteracting = false;
	}

	private IEnumerator WaitForPlayerYesNo(PlayerController player, int i, int j)
	{
		yield return null;
		int selectedResponse = -1;
		TalkModule yesNoModule = new TalkModule
		{
			responses = new List<TalkResponse>()
		};
		TalkResponse yesResponse = new TalkResponse
		{
			response = "#YES"
		};
		TalkResponse noResponse = new TalkResponse
		{
			response = "#NO"
		};
		yesNoModule.responses.Add(yesResponse);
		yesNoModule.responses.Add(noResponse);
		GameUIRoot.Instance.DisplayPlayerConversationOptions(player, yesNoModule, string.Empty, string.Empty);
		while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
		{
			yield return null;
		}
		TextBoxManager.ClearTextBox(speakPoint);
		if (selectedResponse == 0)
		{
			SuppliedObjects[i] = true;
			player.UsePuzzleItem(player.additionalItems[j]);
		}
	}

	private IEnumerator WaitForPlayer()
	{
		yield return null;
		while (!BraveInput.WasSelectPressed(InputManager.ActiveDevice))
		{
			yield return null;
		}
	}

	protected void AttemptSupplyObjects(PlayerController player)
	{
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
