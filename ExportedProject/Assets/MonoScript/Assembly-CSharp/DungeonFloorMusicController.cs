using Dungeonator;
using UnityEngine;

public class DungeonFloorMusicController : MonoBehaviour
{
	public enum DungeonMusicState
	{
		FLOOR_INTRO = 0,
		ACTIVE_SIDE_A = 10,
		ACTIVE_SIDE_B = 20,
		ACTIVE_SIDE_C = 23,
		ACTIVE_SIDE_D = 25,
		CALM = 30,
		SHOP = 40,
		SECRET = 50,
		ARCADE = 60,
		FOYER_ELEVATOR = 70,
		FOYER_SORCERESS = 75
	}

	private DungeonMusicState m_currentState;

	private float m_cooldownTimerRemaining = -1f;

	private float COOLDOWN_TIMER = 22.5f;

	private float MUSIC_CHANGE_TIMER = 40f;

	private float m_lastMusicChangeTime;

	private string m_cachedMusicEventCore = string.Empty;

	private bool m_overrideMusic;

	private bool m_isVictoryState;

	private float m_changedToArcadeTimer = -1f;

	private uint m_coreMusicEventID;

	public DungeonMusicState CurrentState
	{
		get
		{
			return m_currentState;
		}
	}

	public bool MusicOverridden
	{
		get
		{
			return m_overrideMusic;
		}
	}

	public bool ShouldPulseLightFX
	{
		get
		{
			return m_overrideMusic && !m_isVictoryState;
		}
	}

	private void LateUpdate()
	{
		if (m_cooldownTimerRemaining > 0f)
		{
			m_cooldownTimerRemaining -= BraveTime.DeltaTime;
			if (m_cooldownTimerRemaining <= 0f)
			{
				SwitchToState(DungeonMusicState.CALM);
				m_cooldownTimerRemaining = -1f;
			}
		}
		if (m_changedToArcadeTimer > 0f)
		{
			m_changedToArcadeTimer -= BraveTime.DeltaTime;
		}
	}

	public void ClearCoreMusicEventID()
	{
		Debug.Log("Clearing Core Music ID!");
		m_lastMusicChangeTime = -1000f;
		m_overrideMusic = false;
		m_isVictoryState = false;
		m_coreMusicEventID = 0u;
	}

	public void OnAkMusicEvent(object cookie, AkCallbackType eventType, object info)
	{
		switch (eventType)
		{
		case AkCallbackType.AK_Starvation:
			Debug.Log("Core music event: " + m_cachedMusicEventCore + " STARVING with playing ID: " + m_coreMusicEventID);
			break;
		case AkCallbackType.AK_EndOfEvent:
			Debug.Log("Core music event: " + m_cachedMusicEventCore + " ENDING with playing ID: " + m_coreMusicEventID);
			break;
		}
	}

	public void ResetForNewFloor(Dungeon d)
	{
		m_overrideMusic = false;
		m_isVictoryState = false;
		m_lastMusicChangeTime = -1000f;
		GameManager.Instance.FlushMusicAudio();
		if (!string.IsNullOrEmpty(d.musicEventName))
		{
			m_cachedMusicEventCore = d.musicEventName;
		}
		else
		{
			m_cachedMusicEventCore = "Play_MUS_Dungeon_Theme_01";
		}
		m_coreMusicEventID = AkSoundEngine.PostEvent(m_cachedMusicEventCore, GameManager.Instance.gameObject, 33u, OnAkMusicEvent, null);
		Debug.LogWarning("Posting core music event: " + m_cachedMusicEventCore + " with playing ID: " + m_coreMusicEventID);
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST && GameManager.Instance.PrimaryPlayer.characterIdentity != PlayableCharacters.Bullet)
		{
			m_overrideMusic = true;
			AkSoundEngine.PostEvent("Play_MUS_Ending_State_01", GameManager.Instance.gameObject);
		}
		else
		{
			SwitchToState(DungeonMusicState.FLOOR_INTRO);
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER && GameStatsManager.Instance.AnyPastBeaten())
		{
			AkSoundEngine.PostEvent("Play_MUS_State_Winner", GameManager.Instance.gameObject);
		}
	}

	public void UpdateCoreMusicEvent()
	{
		if (m_coreMusicEventID == 0)
		{
			ResetForNewFloor(GameManager.Instance.Dungeon);
		}
	}

	public void SwitchToArcadeMusic()
	{
		m_changedToArcadeTimer = 0.1f;
		SwitchToState(DungeonMusicState.ARCADE);
	}

	public void StartArcadeGame()
	{
		AkSoundEngine.PostEvent("Play_MUS_Winchester_state_Game", base.gameObject);
	}

	public void SwitchToCustomMusic(string customMusicEvent, GameObject source, bool useSwitch, string switchEvent)
	{
		m_cooldownTimerRemaining = -1f;
		AkSoundEngine.PostEvent(customMusicEvent, source);
		if (useSwitch)
		{
			AkSoundEngine.PostEvent(switchEvent, source);
		}
		m_currentState = (DungeonMusicState)(-1);
	}

	public void SwitchToVictoryMusic()
	{
		m_cooldownTimerRemaining = -1f;
		AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
		AkSoundEngine.PostEvent("Play_MUS_Victory_Theme_01", base.gameObject);
		m_overrideMusic = true;
		m_isVictoryState = true;
	}

	public void SwitchToEndTimesMusic()
	{
		m_cooldownTimerRemaining = -1f;
		AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
		AkSoundEngine.PostEvent("Play_MUS_Space_Intro_01", base.gameObject);
		m_overrideMusic = true;
		m_isVictoryState = false;
	}

	public void SwitchToDragunTwo()
	{
		m_cooldownTimerRemaining = -1f;
		AkSoundEngine.PostEvent("Stop_MUS_All", GameManager.Instance.gameObject);
		AkSoundEngine.PostEvent("Play_MUS_Boss_Theme_Dragun_02", GameManager.Instance.gameObject);
		m_overrideMusic = true;
	}

	public void SwitchToBossMusic(string bossMusicString, GameObject source)
	{
		bool flag = GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH || GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH;
		flag |= GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON;
		flag |= GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST && GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Bullet;
		if (true && m_isVictoryState)
		{
			EndVictoryMusic();
		}
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.CHARACTER_PAST || GameManager.Instance.PrimaryPlayer.characterIdentity != PlayableCharacters.Convict)
		{
			m_cooldownTimerRemaining = -1f;
			AkSoundEngine.PostEvent("Stop_MUS_All", source);
			AkSoundEngine.PostEvent(bossMusicString, source);
			m_overrideMusic = true;
		}
	}

	public void EndBossMusic()
	{
		AkSoundEngine.PostEvent("Stop_MUS_Boss_Theme", base.gameObject);
		m_overrideMusic = false;
		AkSoundEngine.PostEvent("Play_MUS_Victory_Theme_01", base.gameObject);
		m_isVictoryState = true;
	}

	public void EndBossMusicNoVictory()
	{
		AkSoundEngine.PostEvent("Stop_MUS_Boss_Theme", base.gameObject);
		AkSoundEngine.PostEvent(m_cachedMusicEventCore, base.gameObject);
		m_overrideMusic = false;
		SwitchToState(DungeonMusicState.CALM);
	}

	public void EndVictoryMusic()
	{
		m_overrideMusic = false;
		m_isVictoryState = false;
		AkSoundEngine.PostEvent("Stop_MUS_All", GameManager.Instance.gameObject);
		AkSoundEngine.PostEvent(m_cachedMusicEventCore, GameManager.Instance.gameObject);
	}

	private void SwitchToState(DungeonMusicState targetState)
	{
		if (m_changedToArcadeTimer > 0f && targetState == DungeonMusicState.CALM && m_currentState == DungeonMusicState.ARCADE)
		{
			return;
		}
		Debug.Log("Attemping to switch to state: " + targetState.ToString() + " with core ID: " + m_coreMusicEventID);
		if (m_overrideMusic)
		{
			return;
		}
		switch (targetState)
		{
		case DungeonMusicState.FLOOR_INTRO:
			m_cooldownTimerRemaining = -1f;
			AkSoundEngine.PostEvent("Play_MUS_Dungeon_State_Intro", GameManager.Instance.gameObject);
			break;
		case DungeonMusicState.ACTIVE_SIDE_A:
			AkSoundEngine.PostEvent("Play_MUS_Dungeon_State_LoopA", GameManager.Instance.gameObject);
			break;
		case DungeonMusicState.ACTIVE_SIDE_B:
			AkSoundEngine.PostEvent("Play_MUS_Dungeon_State_LoopB", GameManager.Instance.gameObject);
			break;
		case DungeonMusicState.ACTIVE_SIDE_C:
			AkSoundEngine.PostEvent("Play_MUS_Dungeon_State_LoopC", GameManager.Instance.gameObject);
			break;
		case DungeonMusicState.ACTIVE_SIDE_D:
			AkSoundEngine.PostEvent("Play_MUS_Dungeon_State_LoopD", GameManager.Instance.gameObject);
			break;
		case DungeonMusicState.CALM:
			m_cooldownTimerRemaining = -1f;
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER && GameStatsManager.Instance.AnyPastBeaten())
			{
				AkSoundEngine.PostEvent("Play_MUS_Dungeon_State_Winner", GameManager.Instance.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Play_MUS_Dungeon_State_Drone", GameManager.Instance.gameObject);
			}
			break;
		case DungeonMusicState.SHOP:
			m_cooldownTimerRemaining = -1f;
			AkSoundEngine.PostEvent("Play_MUS_Dungeon_State_Shop", GameManager.Instance.gameObject);
			break;
		case DungeonMusicState.SECRET:
			m_cooldownTimerRemaining = -1f;
			AkSoundEngine.PostEvent("Play_MUS_Dungeon_State_Secret", GameManager.Instance.gameObject);
			break;
		case DungeonMusicState.ARCADE:
			m_cooldownTimerRemaining = -1f;
			AkSoundEngine.PostEvent("Play_MUS_Dungeon_State_Winchester", GameManager.Instance.gameObject);
			AkSoundEngine.PostEvent("Play_MUS_Winchester_State_Drone", base.gameObject);
			break;
		case DungeonMusicState.FOYER_SORCERESS:
			m_cooldownTimerRemaining = -1f;
			AkSoundEngine.PostEvent("Play_MUS_State_Sorceress", GameManager.Instance.gameObject);
			break;
		case DungeonMusicState.FOYER_ELEVATOR:
			m_cooldownTimerRemaining = -1f;
			AkSoundEngine.PostEvent("Play_MUS_State_Elevator", GameManager.Instance.gameObject);
			break;
		}
		Debug.Log("Successfully switched to state: " + targetState);
		m_currentState = targetState;
	}

	public void NotifyRoomSuddenlyHasEnemies(RoomHandler newRoom)
	{
		if (newRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
		{
			m_cooldownTimerRemaining = -1f;
			if (m_currentState == DungeonMusicState.FLOOR_INTRO || m_currentState == DungeonMusicState.CALM || m_currentState == DungeonMusicState.SHOP)
			{
				SwitchToActiveMusic(null);
			}
		}
	}

	public void SwitchToActiveMusic(DungeonMusicState? excludedState)
	{
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.TUTORIAL)
		{
			if (GameManager.Instance.RandomIntForCurrentRun % 4 == 1)
			{
				if (excludedState.HasValue)
				{
					if (excludedState.Value == DungeonMusicState.ACTIVE_SIDE_C)
					{
						SwitchToState(DungeonMusicState.ACTIVE_SIDE_D);
					}
					else if (excludedState.Value == DungeonMusicState.ACTIVE_SIDE_D)
					{
						SwitchToState(DungeonMusicState.ACTIVE_SIDE_C);
					}
					else
					{
						SwitchToState((!(Random.value < 0.5f)) ? DungeonMusicState.ACTIVE_SIDE_D : DungeonMusicState.ACTIVE_SIDE_C);
					}
				}
				else
				{
					SwitchToState((!(Random.value < 0.5f)) ? DungeonMusicState.ACTIVE_SIDE_D : DungeonMusicState.ACTIVE_SIDE_C);
				}
			}
			else if (excludedState.HasValue)
			{
				if (excludedState.Value == DungeonMusicState.ACTIVE_SIDE_A)
				{
					SwitchToState(DungeonMusicState.ACTIVE_SIDE_B);
				}
				else if (excludedState.Value == DungeonMusicState.ACTIVE_SIDE_B)
				{
					SwitchToState(DungeonMusicState.ACTIVE_SIDE_A);
				}
				else
				{
					SwitchToState((!(Random.value < 0.5f)) ? DungeonMusicState.ACTIVE_SIDE_B : DungeonMusicState.ACTIVE_SIDE_A);
				}
			}
			else
			{
				SwitchToState((!(Random.value < 0.5f)) ? DungeonMusicState.ACTIVE_SIDE_B : DungeonMusicState.ACTIVE_SIDE_A);
			}
		}
		else
		{
			m_lastMusicChangeTime = Time.realtimeSinceStartup;
			if (excludedState.HasValue && excludedState.Value == DungeonMusicState.ACTIVE_SIDE_A)
			{
				SwitchToState(DungeonMusicState.ACTIVE_SIDE_B);
			}
			else if (excludedState.HasValue && excludedState.Value == DungeonMusicState.ACTIVE_SIDE_B)
			{
				SwitchToState(DungeonMusicState.ACTIVE_SIDE_A);
			}
			else
			{
				SwitchToState((!(Random.value > 0.5f)) ? DungeonMusicState.ACTIVE_SIDE_B : DungeonMusicState.ACTIVE_SIDE_A);
			}
		}
	}

	public void NotifyEnteredNewRoom(RoomHandler newRoom)
	{
		UpdateCoreMusicEvent();
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON)
		{
			if (newRoom != null && (newRoom.RoomVisualSubtype == 7 || newRoom.RoomVisualSubtype == 8))
			{
				if (m_cachedMusicEventCore != "Play_MUS_Space_Theme_01")
				{
					AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
					m_currentState = DungeonMusicState.FLOOR_INTRO;
					m_cachedMusicEventCore = "Play_MUS_Space_Theme_01";
					AkSoundEngine.PostEvent("Play_MUS_Space_Theme_01", GameManager.Instance.gameObject);
				}
			}
			else if (m_cachedMusicEventCore != "Play_MUS_Office_Theme_01")
			{
				AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
				m_currentState = DungeonMusicState.FLOOR_INTRO;
				m_cachedMusicEventCore = "Play_MUS_Office_Theme_01";
				AkSoundEngine.PostEvent("Play_MUS_Office_Theme_01", GameManager.Instance.gameObject);
			}
		}
		if (newRoom.area != null && newRoom.area.runtimePrototypeData != null && newRoom.area.runtimePrototypeData.UsesCustomMusic && !string.IsNullOrEmpty(newRoom.area.runtimePrototypeData.CustomMusicEvent))
		{
			SwitchToCustomMusic(newRoom.area.runtimePrototypeData.CustomMusicEvent, base.gameObject, newRoom.area.runtimePrototypeData.UsesCustomSwitch, newRoom.area.runtimePrototypeData.CustomMusicSwitch);
		}
		else if (newRoom.area != null && newRoom.area.runtimePrototypeData != null && newRoom.area.runtimePrototypeData.UsesCustomMusicState)
		{
			m_cooldownTimerRemaining = -1f;
			SwitchToState(newRoom.area.runtimePrototypeData.CustomMusicState);
		}
		else if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			m_cooldownTimerRemaining = -1f;
			SwitchToState(DungeonMusicState.CALM);
		}
		else if (newRoom.IsShop && GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.FORGEGEON)
		{
			m_lastMusicChangeTime = Time.realtimeSinceStartup;
			SwitchToState(DungeonMusicState.SHOP);
		}
		else if (newRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
		{
			m_cooldownTimerRemaining = -1f;
			if (m_isVictoryState || m_currentState == DungeonMusicState.FLOOR_INTRO || m_currentState == DungeonMusicState.CALM || m_currentState == DungeonMusicState.SHOP || m_currentState == DungeonMusicState.SECRET || m_currentState == DungeonMusicState.ARCADE || m_currentState == (DungeonMusicState)(-1))
			{
				if (m_isVictoryState)
				{
					EndVictoryMusic();
				}
				SwitchToActiveMusic(null);
			}
			else if (m_currentState == DungeonMusicState.ACTIVE_SIDE_A)
			{
				if (Random.value > 0.5f && Time.realtimeSinceStartup - m_lastMusicChangeTime > MUSIC_CHANGE_TIMER)
				{
					m_lastMusicChangeTime = Time.realtimeSinceStartup;
					SwitchToActiveMusic(DungeonMusicState.ACTIVE_SIDE_A);
				}
			}
			else if (m_currentState == DungeonMusicState.ACTIVE_SIDE_B && Random.value > 0.5f && Time.realtimeSinceStartup - m_lastMusicChangeTime > MUSIC_CHANGE_TIMER)
			{
				m_lastMusicChangeTime = Time.realtimeSinceStartup;
				SwitchToActiveMusic(DungeonMusicState.ACTIVE_SIDE_B);
			}
		}
		else if (newRoom.WasEverSecretRoom)
		{
			SwitchToState(DungeonMusicState.SECRET);
		}
		else if (m_currentState == DungeonMusicState.SHOP || m_currentState == DungeonMusicState.ARCADE || m_currentState == DungeonMusicState.SECRET || m_currentState == DungeonMusicState.FOYER_ELEVATOR || m_currentState == DungeonMusicState.FOYER_SORCERESS)
		{
			SwitchToState(DungeonMusicState.CALM);
		}
	}

	public void NotifyCurrentRoomEnemiesCleared()
	{
		m_cooldownTimerRemaining = COOLDOWN_TIMER;
	}
}
