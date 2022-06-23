using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	public class CheckGunslingChallengeComplete : BraveFsmStateAction
	{
		public GunslingChallengeType ChallengeType;

		public Gun GunToUsePrefab;

		public Gun GunToUse;

		public FsmEvent SuccessEvent;

		public FsmEvent FailEvent;

		private RoomHandler m_challengeRoom;

		private TalkDoerLite m_talkDoer;

		private bool m_success = true;

		private GameObject m_extantIcon;

		private bool m_hasAlreadyRegisteredIcon;

		private bool m_hasSucceeded;

		private int gunId = -1;

		public RoomHandler ChallengeRoom
		{
			get
			{
				return m_challengeRoom;
			}
			set
			{
				m_challengeRoom = value;
			}
		}

		public override void Awake()
		{
			base.Awake();
			m_talkDoer = base.Owner.GetComponent<TalkDoerLite>();
		}

		public override void OnEnter()
		{
			base.OnEnter();
			ChallengeType = (GunslingChallengeType)base.Fsm.Variables.GetFsmInt("ChallengeType").Value;
			m_challengeRoom = m_talkDoer.GetAbsoluteParentRoom().injectionTarget;
			m_challengeRoom.IsGunslingKingChallengeRoom = true;
			if (!m_hasAlreadyRegisteredIcon)
			{
				m_hasAlreadyRegisteredIcon = true;
				m_extantIcon = Minimap.Instance.RegisterRoomIcon(m_challengeRoom, ResourceCache.Acquire("Global Prefabs/Minimap_King_Icon") as GameObject, true);
			}
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (ChallengeType == GunslingChallengeType.NO_DAMAGE)
				{
					GameManager.Instance.AllPlayers[i].healthHaver.OnDamaged += HandlePlayerDamagedFailed;
				}
				if (ChallengeType == GunslingChallengeType.SPECIFIC_GUN)
				{
					GameManager.Instance.AllPlayers[i].PostProcessProjectile += HandlePlayerFiredProjectile;
					GameManager.Instance.AllPlayers[i].PostProcessBeam += HandlePlayerFiredBeam;
				}
			}
			if (ChallengeType == GunslingChallengeType.DAISUKE_CHALLENGES)
			{
				ChallengeManager.ChallengeModeType = ChallengeModeType.GunslingKingTemporary;
				ChallengeManager.Instance.GunslingTargetRoom = m_challengeRoom;
			}
		}

		private void Succeed()
		{
			if (!m_hasSucceeded)
			{
				if ((bool)m_extantIcon)
				{
					Minimap.Instance.DeregisterRoomIcon(m_challengeRoom, m_extantIcon);
				}
				m_hasSucceeded = true;
				switch (ChallengeType)
				{
				case GunslingChallengeType.NO_DAMAGE:
					GameStatsManager.Instance.SetFlag(GungeonFlags.GUNSLING_KING_CHALLENGE_TYPE_ONE_COMPLETE, true);
					break;
				case GunslingChallengeType.NO_DODGE_ROLL:
					GameStatsManager.Instance.SetFlag(GungeonFlags.GUNSLING_KING_CHALLENGE_TYPE_TWO_COMPLETE, true);
					break;
				case GunslingChallengeType.SPECIFIC_GUN:
					GameStatsManager.Instance.SetFlag(GungeonFlags.GUNSLING_KING_CHALLENGE_TYPE_THREE_COMPLETE, true);
					break;
				case GunslingChallengeType.DAISUKE_CHALLENGES:
					GameStatsManager.Instance.SetFlag(GungeonFlags.GUNSLING_KING_CHALLENGE_TYPE_FOUR_COMPLETE, true);
					break;
				}
				GetRidOfSuppliedGun();
				int num = 0;
				if (GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLING_KING_CHALLENGE_TYPE_ONE_COMPLETE))
				{
					num++;
				}
				if (GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLING_KING_CHALLENGE_TYPE_TWO_COMPLETE))
				{
					num++;
				}
				if (GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLING_KING_CHALLENGE_TYPE_THREE_COMPLETE))
				{
					num++;
				}
				if (GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLING_KING_CHALLENGE_TYPE_FOUR_COMPLETE))
				{
					num++;
				}
				if (num >= 3)
				{
					GameStatsManager.Instance.SetFlag(GungeonFlags.GUNSLING_KING_ACTIVE_IN_FOYER, true);
				}
				InformManservantSuccess();
				base.Fsm.Event(SuccessEvent);
				tk2dSprite component = (ResourceCache.Acquire("Global VFX/GunslingKing_VictoryIcon") as GameObject).GetComponent<tk2dSprite>();
				GameUIRoot.Instance.notificationController.DoCustomNotification(StringTableManager.GetString("#GUNKING_SUCCESS_HEADER"), StringTableManager.GetString("#GUNKING_SUCCESS_BODY"), component.Collection, component.spriteId, UINotificationController.NotificationColor.GOLD);
				Finish();
			}
		}

		private void Fail()
		{
			m_success = false;
			if ((bool)m_extantIcon)
			{
				Minimap.Instance.DeregisterRoomIcon(m_challengeRoom, m_extantIcon);
			}
			GetRidOfSuppliedGun();
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.GUNSLING_KING_CHALLENGES_FAILED, 1f);
			base.Fsm.Event(FailEvent);
			tk2dSprite component = (ResourceCache.Acquire("Global VFX/GunslingKing_DefeatIcon") as GameObject).GetComponent<tk2dSprite>();
			GameUIRoot.Instance.notificationController.DoCustomNotification(StringTableManager.GetString("#GUNKING_FAIL_HEADER"), StringTableManager.GetString("#GUNKING_FAIL_BODY"), component.Collection, component.spriteId);
			Finish();
		}

		private void GetRidOfSuppliedGun()
		{
			if (!(GunToUsePrefab != null) || !GunToUsePrefab.encounterTrackable)
			{
				return;
			}
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				for (int j = 0; j < playerController.inventory.AllGuns.Count; j++)
				{
					Gun gun = playerController.inventory.AllGuns[j];
					if ((bool)gun && (bool)gun.encounterTrackable && gun.IsMinusOneGun && gun.encounterTrackable.journalData.GetPrimaryDisplayName() == GunToUsePrefab.encounterTrackable.journalData.GetPrimaryDisplayName())
					{
						playerController.inventory.DestroyGun(gun);
						break;
					}
				}
			}
			GunToUse = null;
			GunToUsePrefab = null;
		}

		private void HandlePlayerFiredBeam(BeamController obj)
		{
			if (gunId == -1)
			{
				gunId = FindActionOfType<SelectGunslingGun>().SelectedObject.GetComponent<PickupObject>().PickupObjectId;
			}
			if (!obj || !obj.Gun)
			{
				return;
			}
			if (obj.Gun.CurrentOwner is PlayerController)
			{
				PlayerController playerController = obj.Gun.CurrentOwner as PlayerController;
				if (playerController.CurrentRoom != ChallengeRoom)
				{
					return;
				}
			}
			if (obj.Gun.PickupObjectId != gunId)
			{
				Fail();
			}
		}

		private void HandlePlayerFiredProjectile(Projectile obj, float effectChanceScalar)
		{
			if (gunId == -1)
			{
				gunId = FindActionOfType<SelectGunslingGun>().SelectedObject.GetComponent<PickupObject>().PickupObjectId;
			}
			if (obj.Owner is PlayerController)
			{
				PlayerController playerController = obj.Owner as PlayerController;
				if (playerController.CurrentRoom != ChallengeRoom)
				{
					return;
				}
			}
			if (obj.Owner.CurrentGun.PickupObjectId != gunId)
			{
				Fail();
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (ChallengeType == GunslingChallengeType.NO_DAMAGE)
				{
					GameManager.Instance.AllPlayers[i].healthHaver.OnDamaged -= HandlePlayerDamagedFailed;
				}
				if (ChallengeType == GunslingChallengeType.SPECIFIC_GUN)
				{
					GameManager.Instance.AllPlayers[i].PostProcessProjectile -= HandlePlayerFiredProjectile;
					GameManager.Instance.AllPlayers[i].PostProcessBeam -= HandlePlayerFiredBeam;
				}
			}
		}

		private void HandlePlayerDamagedFailed(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
		{
			if (GameManager.HasInstance)
			{
				PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
				if ((bool)primaryPlayer && primaryPlayer.healthHaver.IsAlive && primaryPlayer.CurrentRoom == m_challengeRoom)
				{
					Fail();
				}
				primaryPlayer = GameManager.Instance.SecondaryPlayer;
				if ((bool)primaryPlayer && primaryPlayer.healthHaver.IsAlive && primaryPlayer.CurrentRoom == m_challengeRoom)
				{
					Fail();
				}
			}
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (ChallengeType != GunslingChallengeType.NO_DODGE_ROLL || !m_success)
			{
				return;
			}
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (GameManager.Instance.AllPlayers[i].CurrentRoom == m_challengeRoom && GameManager.Instance.AllPlayers[i].IsDodgeRolling)
				{
					Fail();
				}
			}
		}

		private void InformManservantSuccess()
		{
			List<TalkDoerLite> componentsAbsoluteInRoom = m_talkDoer.GetAbsoluteParentRoom().GetComponentsAbsoluteInRoom<TalkDoerLite>();
			for (int i = 0; i < componentsAbsoluteInRoom.Count; i++)
			{
				if (componentsAbsoluteInRoom[i] == m_talkDoer)
				{
					continue;
				}
				for (int j = 0; j < componentsAbsoluteInRoom[i].playmakerFsms.Length; j++)
				{
					if (componentsAbsoluteInRoom[i].playmakerFsms[j].FsmName.Contains("Dungeon"))
					{
						componentsAbsoluteInRoom[i].playmakerFsms[j].FsmVariables.FindFsmString("currentMode").Value = "modeQuest";
					}
				}
			}
		}

		public override void OnLateUpdate()
		{
			base.OnLateUpdate();
			if (!m_challengeRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
			{
				if (m_success)
				{
					Succeed();
				}
				else
				{
					Fail();
				}
				Finish();
			}
		}
	}
}
