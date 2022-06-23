using Dungeonator;

public class SackKnightController : CompanionController
{
	public enum SackKnightPhase
	{
		PEASANT,
		SQUIRE,
		HEDGE_KNIGHT,
		KNIGHT,
		KNIGHT_LIEUTENANT,
		KNIGHT_COMMANDER,
		HOLY_KNIGHT,
		ANGELIC_KNIGHT,
		MECHAJUNKAN
	}

	public const bool c_usesJunkNotArmor = true;

	public SackKnightPhase CurrentForm;

	public static bool HasJunkan()
	{
		if (GameManager.HasInstance && (bool)GameManager.Instance.PrimaryPlayer)
		{
			for (int i = 0; i < GameManager.Instance.PrimaryPlayer.passiveItems.Count; i++)
			{
				PassiveItem passiveItem = GameManager.Instance.PrimaryPlayer.passiveItems[i];
				if (passiveItem is CompanionItem && (bool)(passiveItem as CompanionItem).ExtantCompanion && (bool)(passiveItem as CompanionItem).ExtantCompanion.GetComponent<SackKnightController>())
				{
					return true;
				}
			}
		}
		if (GameManager.HasInstance && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && (bool)GameManager.Instance.SecondaryPlayer)
		{
			for (int j = 0; j < GameManager.Instance.SecondaryPlayer.passiveItems.Count; j++)
			{
				PassiveItem passiveItem2 = GameManager.Instance.SecondaryPlayer.passiveItems[j];
				if (passiveItem2 is CompanionItem && (bool)(passiveItem2 as CompanionItem).ExtantCompanion && (bool)(passiveItem2 as CompanionItem).ExtantCompanion.GetComponent<SackKnightController>())
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void HandleRoomCleared(PlayerController callingPlayer)
	{
		if (CurrentForm >= SackKnightPhase.KNIGHT && callingPlayer.CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && callingPlayer.CurrentRoom.area.PrototypeRoomBossSubcategory != PrototypeDungeonRoom.RoomBossSubCategory.MINI_BOSS)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_SER_JUNKAN_UNLOCKED, true);
		}
	}

	public override void Update()
	{
		if (!GameManager.Instance || GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		if ((bool)m_owner && !Chest.HasDroppedSerJunkanThisSession)
		{
			Chest.HasDroppedSerJunkanThisSession = true;
		}
		UpdateAnimationNamesBasedOnSacks();
		if (HasStealthMode && (bool)m_owner)
		{
			if (m_owner.IsStealthed && !m_isStealthed)
			{
				m_isStealthed = true;
				base.aiAnimator.IdleAnimation.AnimNames[0] = "sst_dis_idle_right";
				base.aiAnimator.IdleAnimation.AnimNames[1] = "sst_dis_idle_left";
				base.aiAnimator.MoveAnimation.AnimNames[0] = "sst_dis_move_right";
				base.aiAnimator.MoveAnimation.AnimNames[1] = "sst_dis_move_left";
			}
			else if (!m_owner.IsStealthed && m_isStealthed)
			{
				m_isStealthed = false;
				base.aiAnimator.IdleAnimation.AnimNames[0] = "sst_idle_right";
				base.aiAnimator.IdleAnimation.AnimNames[1] = "sst_idle_left";
				base.aiAnimator.MoveAnimation.AnimNames[0] = "sst_move_right";
				base.aiAnimator.MoveAnimation.AnimNames[1] = "sst_move_left";
			}
		}
		IntVector2 intVector = base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
		if (GameManager.Instance.Dungeon.data.CheckInBounds(intVector))
		{
			CellData cellData = GameManager.Instance.Dungeon.data[intVector];
			if (cellData != null)
			{
				m_lastCellType = cellData.type;
			}
		}
	}

	private void UpdateAnimationNamesBasedOnSacks()
	{
		if (!m_owner)
		{
			return;
		}
		int num = 0;
		bool flag = false;
		for (int i = 0; i < m_owner.passiveItems.Count; i++)
		{
			if (m_owner.passiveItems[i] is BasicStatPickup)
			{
				BasicStatPickup basicStatPickup = m_owner.passiveItems[i] as BasicStatPickup;
				if (basicStatPickup.IsJunk)
				{
					num++;
				}
				if (basicStatPickup.IsJunk && basicStatPickup.GivesCurrency)
				{
					flag = true;
				}
			}
		}
		AIAnimator aIAnimator = base.aiAnimator;
		if (flag)
		{
			if (CurrentForm != SackKnightPhase.MECHAJUNKAN)
			{
				base.specRigidbody.PixelColliders[0].ManualOffsetX = 30;
				base.specRigidbody.PixelColliders[0].ManualOffsetY = 3;
				base.specRigidbody.PixelColliders[0].ManualWidth = 17;
				base.specRigidbody.PixelColliders[0].ManualHeight = 16;
				base.specRigidbody.PixelColliders[1].ManualOffsetX = 30;
				base.specRigidbody.PixelColliders[1].ManualOffsetY = 3;
				base.specRigidbody.PixelColliders[1].ManualWidth = 17;
				base.specRigidbody.PixelColliders[1].ManualHeight = 28;
				base.specRigidbody.PixelColliders[0].Regenerate(base.transform);
				base.specRigidbody.PixelColliders[1].Regenerate(base.transform);
				base.specRigidbody.Reinitialize();
				base.aiActor.ShadowObject.transform.position = base.specRigidbody.UnitBottomCenter;
			}
			CurrentForm = SackKnightPhase.MECHAJUNKAN;
			aIAnimator.IdleAnimation.AnimNames[0] = "junk_g_idle_right";
			aIAnimator.IdleAnimation.AnimNames[1] = "junk_g_idle_left";
			aIAnimator.MoveAnimation.AnimNames[0] = "junk_g_move_right";
			aIAnimator.MoveAnimation.AnimNames[1] = "junk_g_move_left";
			aIAnimator.TalkAnimation.AnimNames[0] = "junk_g_talk_right";
			aIAnimator.TalkAnimation.AnimNames[1] = "junk_g_talk_left";
			aIAnimator.OtherAnimations[0].anim.AnimNames[0] = "junk_g_sword_right";
			aIAnimator.OtherAnimations[0].anim.AnimNames[1] = "junk_g_sword_left";
		}
		else if (num < 1)
		{
			CurrentForm = SackKnightPhase.PEASANT;
			aIAnimator.IdleAnimation.AnimNames[0] = "junk_idle_right";
			aIAnimator.IdleAnimation.AnimNames[1] = "junk_idle_left";
			aIAnimator.MoveAnimation.AnimNames[0] = "junk_move_right";
			aIAnimator.MoveAnimation.AnimNames[1] = "junk_move_left";
			aIAnimator.TalkAnimation.AnimNames[0] = "junk_talk_right";
			aIAnimator.TalkAnimation.AnimNames[1] = "junk_talk_left";
			aIAnimator.OtherAnimations[0].anim.AnimNames[0] = "junk_attack_right";
			aIAnimator.OtherAnimations[0].anim.AnimNames[1] = "junk_attack_left";
		}
		else if (num == 1)
		{
			CurrentForm = SackKnightPhase.SQUIRE;
			aIAnimator.IdleAnimation.AnimNames[0] = "junk_h_idle_right";
			aIAnimator.IdleAnimation.AnimNames[1] = "junk_h_idle_left";
			aIAnimator.MoveAnimation.AnimNames[0] = "junk_h_move_right";
			aIAnimator.MoveAnimation.AnimNames[1] = "junk_h_move_left";
			aIAnimator.TalkAnimation.AnimNames[0] = "junk_h_talk_right";
			aIAnimator.TalkAnimation.AnimNames[1] = "junk_h_talk_left";
			aIAnimator.OtherAnimations[0].anim.AnimNames[0] = "junk_h_attack_right";
			aIAnimator.OtherAnimations[0].anim.AnimNames[1] = "junk_h_attack_left";
		}
		else if (num == 2)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_GOLD_JUNK, true);
			CurrentForm = SackKnightPhase.HEDGE_KNIGHT;
			aIAnimator.IdleAnimation.AnimNames[0] = "junk_sh_idle_right";
			aIAnimator.IdleAnimation.AnimNames[1] = "junk_sh_idle_left";
			aIAnimator.MoveAnimation.AnimNames[0] = "junk_sh_move_right";
			aIAnimator.MoveAnimation.AnimNames[1] = "junk_sh_move_left";
			aIAnimator.TalkAnimation.AnimNames[0] = "junk_sh_talk_right";
			aIAnimator.TalkAnimation.AnimNames[1] = "junk_sh_talk_left";
			aIAnimator.OtherAnimations[0].anim.AnimNames[0] = "junk_sh_attack_right";
			aIAnimator.OtherAnimations[0].anim.AnimNames[1] = "junk_sh_attack_left";
		}
		else if (num == 3)
		{
			CurrentForm = SackKnightPhase.KNIGHT;
			aIAnimator.IdleAnimation.AnimNames[0] = "junk_shs_idle_right";
			aIAnimator.IdleAnimation.AnimNames[1] = "junk_shs_idle_left";
			aIAnimator.MoveAnimation.AnimNames[0] = "junk_shs_move_right";
			aIAnimator.MoveAnimation.AnimNames[1] = "junk_shs_move_left";
			aIAnimator.TalkAnimation.AnimNames[0] = "junk_shs_talk_right";
			aIAnimator.TalkAnimation.AnimNames[1] = "junk_shs_talk_left";
			aIAnimator.OtherAnimations[0].anim.AnimNames[0] = "junk_shs_attack_right";
			aIAnimator.OtherAnimations[0].anim.AnimNames[1] = "junk_shs_attack_left";
		}
		else if (num == 4)
		{
			CurrentForm = SackKnightPhase.KNIGHT_LIEUTENANT;
			aIAnimator.IdleAnimation.AnimNames[0] = "junk_shsp_idle_right";
			aIAnimator.IdleAnimation.AnimNames[1] = "junk_shsp_idle_left";
			aIAnimator.MoveAnimation.AnimNames[0] = "junk_shsp_move_right";
			aIAnimator.MoveAnimation.AnimNames[1] = "junk_shsp_move_left";
			aIAnimator.TalkAnimation.AnimNames[0] = "junk_shsp_talk_right";
			aIAnimator.TalkAnimation.AnimNames[1] = "junk_shsp_talk_left";
			aIAnimator.OtherAnimations[0].anim.AnimNames[0] = "junk_shsp_attack_right";
			aIAnimator.OtherAnimations[0].anim.AnimNames[1] = "junk_shsp_attack_left";
		}
		else if (num == 5)
		{
			CurrentForm = SackKnightPhase.KNIGHT_COMMANDER;
			aIAnimator.IdleAnimation.AnimNames[0] = "junk_shspc_idle_right";
			aIAnimator.IdleAnimation.AnimNames[1] = "junk_shspc_idle_left";
			aIAnimator.MoveAnimation.AnimNames[0] = "junk_shspc_move_right";
			aIAnimator.MoveAnimation.AnimNames[1] = "junk_shspc_move_left";
			aIAnimator.TalkAnimation.AnimNames[0] = "junk_shspc_talk_right";
			aIAnimator.TalkAnimation.AnimNames[1] = "junk_shspc_talk_left";
			aIAnimator.OtherAnimations[0].anim.AnimNames[0] = "junk_shspc_attack_right";
			aIAnimator.OtherAnimations[0].anim.AnimNames[1] = "junk_shspc_attack_left";
		}
		else if (num == 6)
		{
			CurrentForm = SackKnightPhase.HOLY_KNIGHT;
			aIAnimator.IdleAnimation.AnimNames[0] = "junk_shspcg_idle_right";
			aIAnimator.IdleAnimation.AnimNames[1] = "junk_shspcg_idle_left";
			aIAnimator.MoveAnimation.AnimNames[0] = "junk_shspcg_move_right";
			aIAnimator.MoveAnimation.AnimNames[1] = "junk_shspcg_move_left";
			aIAnimator.TalkAnimation.AnimNames[0] = "junk_shspcg_talk_right";
			aIAnimator.TalkAnimation.AnimNames[1] = "junk_shspcg_talk_left";
			aIAnimator.OtherAnimations[0].anim.AnimNames[0] = "junk_shspcg_attack_right";
			aIAnimator.OtherAnimations[0].anim.AnimNames[1] = "junk_shspcg_attack_left";
		}
		else if (num > 6)
		{
			CurrentForm = SackKnightPhase.ANGELIC_KNIGHT;
			GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_SER_JUNKAN_MAXLVL, true);
			aIAnimator.IdleAnimation.AnimNames[0] = "junk_a_idle_right";
			aIAnimator.IdleAnimation.AnimNames[1] = "junk_a_idle_left";
			aIAnimator.MoveAnimation.AnimNames[0] = "junk_a_idle_right";
			aIAnimator.MoveAnimation.AnimNames[1] = "junk_a_idle_left";
			aIAnimator.TalkAnimation.AnimNames[0] = "junk_a_talk_right";
			aIAnimator.TalkAnimation.AnimNames[1] = "junk_a_talk_left";
			aIAnimator.OtherAnimations[0].anim.AnimNames[0] = "junk_a_attack_right";
			aIAnimator.OtherAnimations[0].anim.AnimNames[1] = "junk_a_attack_left";
			if (!base.aiActor.IsFlying)
			{
				base.aiActor.SetIsFlying(true, "angel", true, true);
			}
		}
		if (CurrentForm != SackKnightPhase.ANGELIC_KNIGHT && base.aiActor.IsFlying)
		{
			base.aiActor.SetIsFlying(false, "angel", true, true);
		}
	}
}
