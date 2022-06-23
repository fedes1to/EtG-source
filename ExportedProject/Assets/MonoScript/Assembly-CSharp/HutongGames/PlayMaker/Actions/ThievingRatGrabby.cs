using System.Collections;
using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	public class ThievingRatGrabby : FsmStateAction
	{
		public PickupObject TargetObject;

		public NoteDoer notePrefab;

		private Vector2 m_lastPosition;

		private TalkDoerLite m_talkDoer;

		private bool m_grabby;

		public override void Awake()
		{
			base.Awake();
			m_talkDoer = base.Owner.GetComponent<TalkDoerLite>();
		}

		public override void OnEnter()
		{
			m_lastPosition = m_talkDoer.specRigidbody.UnitCenter;
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (m_talkDoer.CurrentPath != null)
			{
				if (!m_talkDoer.CurrentPath.WillReachFinalGoal)
				{
					m_talkDoer.transform.position = TargetObject.sprite.WorldCenter + new Vector2(0f, 1f);
					m_talkDoer.specRigidbody.Reinitialize();
					PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(m_talkDoer.specRigidbody, CollisionMask.LayerToMask(CollisionLayer.PlayerCollider));
					m_talkDoer.talkDoer.CurrentPath = null;
				}
				else
				{
					m_talkDoer.specRigidbody.Velocity = m_talkDoer.GetPathVelocityContribution(m_lastPosition, 32);
					m_lastPosition = m_talkDoer.specRigidbody.UnitCenter;
				}
			}
		}

		private IEnumerator HandleGrabby()
		{
			m_grabby = true;
			m_talkDoer.aiAnimator.PlayUntilFinished("laugh");
			string targetItemName = ((!TargetObject) ? string.Empty : ((!TargetObject.encounterTrackable) ? TargetObject.DisplayName : TargetObject.encounterTrackable.GetModifiedDisplayName()));
			yield return new WaitForSeconds(2f);
			m_talkDoer.aiAnimator.PlayUntilFinished("grabby", true);
			yield return new WaitForSeconds(0.55f);
			if (base.Fsm.ActiveState != base.State)
			{
				yield break;
			}
			base.Fsm.SuppressGlobalTransitions = true;
			if ((bool)TargetObject && (bool)TargetObject.GetComponentInParent<PlayerController>())
			{
				yield break;
			}
			if (TargetObject is IPlayerInteractable)
			{
				RoomHandler.unassignedInteractableObjects.Remove(TargetObject as IPlayerInteractable);
			}
			float elapsed = 0f;
			float duration = 0.25f;
			if ((bool)TargetObject && (bool)TargetObject.transform)
			{
				Vector3 startPosition = TargetObject.transform.position;
				while (elapsed < duration)
				{
					elapsed += BraveTime.DeltaTime;
					if ((bool)TargetObject && (bool)TargetObject.transform && TargetObject.sprite != null)
					{
						TargetObject.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.1f, 0.1f, 0.1f), elapsed / duration);
						TargetObject.transform.position = Vector3.Lerp(startPosition, m_talkDoer.transform.position + new Vector3(0.4375f, 0.375f, 0f), elapsed / duration);
					}
					yield return null;
				}
				if (PassiveItem.IsFlagSetAtAll(typeof(RingOfResourcefulRatItem)))
				{
					PickupObject.ItemQuality quality = TargetObject.quality;
					if (quality != 0 && quality != PickupObject.ItemQuality.SPECIAL && quality != PickupObject.ItemQuality.EXCLUDED)
					{
						PickupObject pickupObject = null;
						if (TargetObject is Gun)
						{
							pickupObject = LootEngine.GetItemOfTypeAndQuality<Gun>(quality, GameManager.Instance.RewardManager.GunsLootTable);
						}
						else if (TargetObject is PassiveItem)
						{
							pickupObject = LootEngine.GetItemOfTypeAndQuality<PassiveItem>(quality, GameManager.Instance.RewardManager.ItemsLootTable);
						}
						else if (TargetObject is PlayerItem)
						{
							pickupObject = LootEngine.GetItemOfTypeAndQuality<PlayerItem>(quality, GameManager.Instance.RewardManager.ItemsLootTable);
						}
						if ((bool)pickupObject)
						{
							DebrisObject debrisObject = LootEngine.SpawnItem(pickupObject.gameObject, startPosition, Vector2.up, 0f);
							PickupObject componentInChildren = debrisObject.GetComponentInChildren<PickupObject>();
							if ((bool)componentInChildren && !componentInChildren.IgnoredByRat)
							{
								componentInChildren.ClearIgnoredByRatFlagOnPickup = true;
								componentInChildren.IgnoredByRat = true;
							}
							for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
							{
								PassiveItem.DecrementFlag(GameManager.Instance.AllPlayers[i], typeof(RingOfResourcefulRatItem));
							}
						}
					}
				}
				if (TargetObject is Gun)
				{
					(TargetObject as Gun).GetRidOfMinimapIcon();
				}
				if (TargetObject is PlayerItem)
				{
					(TargetObject as PlayerItem).GetRidOfMinimapIcon();
				}
				if (TargetObject is PassiveItem)
				{
					(TargetObject as PassiveItem).GetRidOfMinimapIcon();
				}
				if (TargetObject is Gun && TargetObject.transform.parent != null)
				{
					Object.Destroy(TargetObject.transform.parent.gameObject);
				}
				else
				{
					Object.Destroy(TargetObject.gameObject);
				}
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.ITEMS_TAKEN_BY_RAT, 1f);
				yield return new WaitForSeconds(0.9f);
			}
			m_talkDoer.aiAnimator.PlayUntilFinished("grab_laugh", true);
			yield return new WaitForSeconds(1f);
			if ((bool)m_talkDoer)
			{
				NoteDoer component = notePrefab.InstantiateObject(m_talkDoer.GetAbsoluteParentRoom(), m_talkDoer.transform.position.IntXY(VectorConversions.Floor) - m_talkDoer.GetAbsoluteParentRoom().area.basePosition).GetComponent<NoteDoer>();
				component.stringKey = StringTableManager.GetLongString("#RESRAT_NOTE_ITEM").Replace("%ITEM", targetItemName);
				m_talkDoer.GetAbsoluteParentRoom().RegisterInteractable(component);
				component.alreadyLocalized = true;
			}
			base.Fsm.SuppressGlobalTransitions = false;
			Finish();
		}

		public override void OnLateUpdate()
		{
			if (m_talkDoer.CurrentPath == null && !m_grabby)
			{
				m_talkDoer.StartCoroutine(HandleGrabby());
			}
		}
	}
}
