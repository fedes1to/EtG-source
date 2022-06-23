using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;

public class BraveInput : MonoBehaviour
{
	public enum AutoAim
	{
		AutoAim,
		SuperAutoAim
	}

	[Serializable]
	public class BufferedInput
	{
		public GungeonActions.GungeonActionType Control;

		public float BufferTime = 0.3f;
	}

	public class PressAction
	{
		public float Timer;

		private BufferedInput m_bufferedInput;

		public static ObjectPool<PressAction> Pool = new ObjectPool<PressAction>(() => new PressAction(), 10, Cleanup);

		public float Buffer
		{
			get
			{
				return m_bufferedInput.BufferTime;
			}
		}

		public GungeonActions.GungeonActionType Control
		{
			get
			{
				return m_bufferedInput.Control;
			}
		}

		private PressAction()
		{
		}

		public void SetAll(BufferedInput bufferedInput)
		{
			m_bufferedInput = bufferedInput;
			Timer = 0f;
		}

		public static void Cleanup(PressAction pressAction)
		{
			pressAction.m_bufferedInput = null;
		}
	}

	public class HoldAction
	{
		public float DownTimer;

		public float UpTimer;

		public bool Held = true;

		public bool ConsumedDown;

		public bool ConsumedUp;

		private BufferedInput m_bufferedInput;

		public static ObjectPool<HoldAction> Pool = new ObjectPool<HoldAction>(() => new HoldAction(), 10, Cleanup);

		public float Buffer
		{
			get
			{
				return m_bufferedInput.BufferTime;
			}
		}

		public GungeonActions.GungeonActionType Control
		{
			get
			{
				return m_bufferedInput.Control;
			}
		}

		private HoldAction()
		{
		}

		public void SetAll(BufferedInput bufferedInput)
		{
			m_bufferedInput = bufferedInput;
			DownTimer = 0f;
			UpTimer = 0f;
			Held = true;
			ConsumedDown = false;
			ConsumedUp = false;
		}

		public static void Cleanup(HoldAction holdAction)
		{
			holdAction.m_bufferedInput = null;
		}
	}

	private class TimedVibration
	{
		public float timer;

		public float largeMotor;

		public float smallMotor;

		public TimedVibration(float timer, float intensity)
		{
			this.timer = timer;
			largeMotor = intensity;
			smallMotor = intensity;
		}

		public TimedVibration(float timer, float largeMotor, float smallMotor)
		{
			this.timer = timer;
			this.largeMotor = largeMotor;
			this.smallMotor = smallMotor;
		}
	}

	public static bool AllowPausedRumble = false;

	public AutoAim autoAimMode;

	public bool showCursor;

	public MagnetAngles magnetAngles;

	public float controllerAutoAimDegrees = 15f;

	public float controllerSuperAutoAimDegrees = 25f;

	public float controllerFakeSemiAutoCooldown = 0.25f;

	[BetterList]
	public BufferedInput[] PressActions;

	[BetterList]
	public BufferedInput[] HoldActions;

	private GungeonActions m_activeGungeonActions;

	[NonSerialized]
	private int m_playerID;

	private PooledLinkedList<PressAction> m_pressActions = new PooledLinkedList<PressAction>();

	private PooledLinkedList<HoldAction> m_holdActions = new PooledLinkedList<HoldAction>();

	private List<TimedVibration> m_currentVibrations = new List<TimedVibration>();

	private float m_sustainedLargeVibration;

	private float m_sustainedSmallVibration;

	private static Dictionary<int, BraveInput> m_instances = new Dictionary<int, BraveInput>();

	public static BraveInput PlayerlessInstance
	{
		get
		{
			if (m_instances == null || m_instances.Count < 1)
			{
				return null;
			}
			return m_instances[0];
		}
	}

	public static BraveInput PrimaryPlayerInstance
	{
		get
		{
			if (m_instances == null || m_instances.Count < 1)
			{
				return null;
			}
			if (GameManager.Instance.PrimaryPlayer == null)
			{
				return m_instances[0];
			}
			return GetInstanceForPlayer(GameManager.Instance.PrimaryPlayer.PlayerIDX);
		}
	}

	public static BraveInput SecondaryPlayerInstance
	{
		get
		{
			if (m_instances == null || m_instances.Count < 2)
			{
				return null;
			}
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER)
			{
				return null;
			}
			if (GameManager.Instance.SecondaryPlayer == null)
			{
				return null;
			}
			return GetInstanceForPlayer(GameManager.Instance.SecondaryPlayer.PlayerIDX);
		}
	}

	public static AutoAim AutoAimMode
	{
		get
		{
			return m_instances[0].autoAimMode;
		}
		set
		{
			m_instances[0].autoAimMode = value;
		}
	}

	public static bool ShowCursor
	{
		get
		{
			return m_instances[0].showCursor;
		}
		set
		{
			m_instances[0].showCursor = value;
		}
	}

	public static MagnetAngles MagnetAngles
	{
		get
		{
			return m_instances[0].magnetAngles;
		}
	}

	public static float ControllerAutoAimDegrees
	{
		get
		{
			float num = m_instances[0].controllerAutoAimDegrees;
			if (GameManager.Options != null)
			{
				num *= GameManager.Options.controllerAimAssistMultiplier;
			}
			return num;
		}
	}

	public static float ControllerSuperAutoAimDegrees
	{
		get
		{
			float num = m_instances[0].controllerSuperAutoAimDegrees;
			if (GameManager.Options != null)
			{
				num *= GameManager.Options.controllerAimAssistMultiplier;
			}
			return num;
		}
	}

	public static float ControllerFakeSemiAutoCooldown
	{
		get
		{
			return m_instances[0].controllerFakeSemiAutoCooldown;
		}
	}

	public GungeonActions ActiveActions
	{
		get
		{
			return m_activeGungeonActions;
		}
	}

	public Vector2 MousePosition
	{
		get
		{
			return Input.mousePosition.XY();
		}
	}

	public static GameOptions.ControllerSymbology PlayerOneCurrentSymbology
	{
		get
		{
			return GetCurrentSymbology(0);
		}
	}

	public static GameOptions.ControllerSymbology PlayerTwoCurrentSymbology
	{
		get
		{
			return GetCurrentSymbology(1);
		}
	}

	public bool MenuInteractPressed
	{
		get
		{
			return ActiveActions != null && (ActiveActions.InteractAction.WasPressed || ActiveActions.MenuSelectAction.WasPressed);
		}
	}

	private static void DoStartupAssignmentOfControllers(int lastActiveDeviceIndex = -1)
	{
		if (GameManager.PreventGameManagerExistence || GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER)
		{
			GameManager.Options.PlayerIDtoDeviceIndexMap.Clear();
		}
		else
		{
			if (GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER)
			{
				return;
			}
			GameManager.Options.PlayerIDtoDeviceIndexMap.Clear();
			if (Application.platform == RuntimePlatform.PS4)
			{
				GameManager.Options.PlayerIDtoDeviceIndexMap.Add(0, 0);
				GameManager.Options.PlayerIDtoDeviceIndexMap.Add(1, 1);
			}
			else if (InputManager.Devices.Count == 1)
			{
				if (lastActiveDeviceIndex != 0)
				{
					GameManager.Options.PlayerIDtoDeviceIndexMap.Add(1, 0);
					GameManager.Options.PlayerIDtoDeviceIndexMap.Add(0, 1);
				}
				else
				{
					GameManager.Options.PlayerIDtoDeviceIndexMap.Add(0, 0);
					GameManager.Options.PlayerIDtoDeviceIndexMap.Add(1, 1);
				}
			}
			else if (InputManager.Devices.Count == 2)
			{
				if (lastActiveDeviceIndex >= 1)
				{
					GameManager.Options.PlayerIDtoDeviceIndexMap.Add(0, lastActiveDeviceIndex);
					GameManager.Options.PlayerIDtoDeviceIndexMap.Add(1, 0);
				}
				else
				{
					GameManager.Options.PlayerIDtoDeviceIndexMap.Add(0, 0);
					GameManager.Options.PlayerIDtoDeviceIndexMap.Add(1, 1);
				}
			}
			else if (lastActiveDeviceIndex >= 1)
			{
				GameManager.Options.PlayerIDtoDeviceIndexMap.Add(0, lastActiveDeviceIndex);
				GameManager.Options.PlayerIDtoDeviceIndexMap.Add(1, -1);
				GameManager.Instance.StartCoroutine(AssignPlayerTwoToNextActiveDevice());
			}
			else
			{
				GameManager.Options.PlayerIDtoDeviceIndexMap.Add(0, 0);
				GameManager.Options.PlayerIDtoDeviceIndexMap.Add(1, 1);
			}
		}
	}

	private static IEnumerator AssignPlayerTwoToNextActiveDevice()
	{
		int lastActiveDeviceIndex;
		while (true)
		{
			InputDevice lastActiveDevice = InputManager.ActiveDevice;
			lastActiveDeviceIndex = -1;
			for (int i = 0; i < InputManager.Devices.Count; i++)
			{
				if (InputManager.Devices[i] == lastActiveDevice)
				{
					lastActiveDeviceIndex = i;
				}
			}
			if (GameManager.Options.PlayerIDtoDeviceIndexMap.ContainsKey(0) && GameManager.Options.PlayerIDtoDeviceIndexMap.ContainsKey(1) && GameManager.Options.PlayerIDtoDeviceIndexMap[0] != lastActiveDeviceIndex)
			{
				break;
			}
			yield return null;
		}
		ReassignPlayerPort(1, lastActiveDeviceIndex);
	}

	public static void ReassignAllControllers(InputDevice overrideLastActiveDevice = null)
	{
		Debug.LogWarning("Reassigning all controllers.");
		InputDevice inputDevice = overrideLastActiveDevice ?? InputManager.ActiveDevice;
		int lastActiveDeviceIndex = -1;
		for (int i = 0; i < InputManager.Devices.Count; i++)
		{
			if (InputManager.Devices[i] == inputDevice)
			{
				lastActiveDeviceIndex = i;
			}
		}
		for (int j = 0; j < m_instances.Count; j++)
		{
			if (m_instances[j].m_activeGungeonActions != null)
			{
				m_instances[j].m_activeGungeonActions.Destroy();
			}
			m_instances[j].m_activeGungeonActions = null;
		}
		DoStartupAssignmentOfControllers(lastActiveDeviceIndex);
		for (int k = 0; k < m_instances.Count; k++)
		{
			if (m_instances[k].m_activeGungeonActions == null)
			{
				m_instances[k].m_activeGungeonActions = new GungeonActions();
				m_instances[k].AssignActionsDevice();
				m_instances[k].m_activeGungeonActions.InitializeDefaults();
				if ((GameManager.Instance.PrimaryPlayer == null && m_instances[k].m_playerID == 0) || m_instances[k].m_playerID == GameManager.Instance.PrimaryPlayer.PlayerIDX)
				{
					TryLoadBindings(0, m_instances[k].ActiveActions);
				}
				else
				{
					TryLoadBindings(1, m_instances[k].ActiveActions);
				}
			}
			m_instances[k].AssignActionsDevice();
		}
		for (int l = 0; l < m_instances.Count; l++)
		{
			if (GameManager.Instance.AllPlayers.Length <= 1)
			{
				continue;
			}
			if (m_instances[l].m_activeGungeonActions.Device == null)
			{
				m_instances[l].m_activeGungeonActions.IgnoreBindingsOfType(BindingSourceType.DeviceBindingSource);
			}
			else if (m_instances[l].m_playerID == GameManager.Instance.PrimaryPlayer.PlayerIDX)
			{
				if (GetInstanceForPlayer(GameManager.Instance.SecondaryPlayer.PlayerIDX).m_activeGungeonActions.Device == null)
				{
					m_instances[l].m_activeGungeonActions.IgnoreBindingsOfType(BindingSourceType.KeyBindingSource);
					m_instances[l].m_activeGungeonActions.IgnoreBindingsOfType(BindingSourceType.MouseBindingSource);
				}
			}
			else
			{
				m_instances[l].m_activeGungeonActions.IgnoreBindingsOfType(BindingSourceType.KeyBindingSource);
				m_instances[l].m_activeGungeonActions.IgnoreBindingsOfType(BindingSourceType.MouseBindingSource);
			}
		}
	}

	public static void ForceLoadBindingInfoFromOptions()
	{
		if (GameManager.Options == null)
		{
			return;
		}
		for (int i = 0; i < m_instances.Count; i++)
		{
			if (GameManager.PreventGameManagerExistence || GameManager.Instance.PrimaryPlayer == null)
			{
				if (m_instances[i].m_playerID == 0)
				{
					TryLoadBindings(0, m_instances[i].ActiveActions);
				}
			}
			else if (m_instances[i].m_playerID == GameManager.Instance.PrimaryPlayer.PlayerIDX)
			{
				TryLoadBindings(0, m_instances[i].ActiveActions);
			}
			else
			{
				TryLoadBindings(1, m_instances[i].ActiveActions);
			}
		}
	}

	public static void SavePlayerlessBindingsToOptions()
	{
		if (GameManager.Options != null && !(GameManager.Instance.PrimaryPlayer != null) && !(PlayerlessInstance == null))
		{
			GameManager.Options.playerOneBindingDataV2 = PlayerlessInstance.ActiveActions.Save();
		}
	}

	public static void SaveBindingInfoToOptions()
	{
		if (GameManager.Options == null || GameManager.Instance.PrimaryPlayer == null)
		{
			return;
		}
		Debug.Log("Saving Binding Info To Options");
		for (int i = 0; i < m_instances.Count; i++)
		{
			if (m_instances[i].m_playerID == GameManager.Instance.PrimaryPlayer.PlayerIDX)
			{
				GameManager.Options.playerOneBindingDataV2 = m_instances[i].ActiveActions.Save();
			}
			else
			{
				GameManager.Options.playerTwoBindingDataV2 = m_instances[i].ActiveActions.Save();
			}
		}
	}

	public static void OnLanguageChanged()
	{
		for (int i = 0; i < m_instances.Count; i++)
		{
			if ((bool)m_instances[i] && m_instances[i].ActiveActions != null)
			{
				m_instances[i].ActiveActions.ReinitializeMenuDefaults();
			}
		}
	}

	public static void ResetBindingsToDefaults()
	{
		GameManager.Options.playerOneBindingData = string.Empty;
		GameManager.Options.playerOneBindingDataV2 = string.Empty;
		GameManager.Options.playerTwoBindingData = string.Empty;
		GameManager.Options.playerTwoBindingDataV2 = string.Empty;
		DoStartupAssignmentOfControllers();
		for (int i = 0; i < m_instances.Count; i++)
		{
			if (m_instances[i].m_activeGungeonActions != null)
			{
				m_instances[i].m_activeGungeonActions.Destroy();
			}
			m_instances[i].m_activeGungeonActions = null;
			m_instances[i].CheckForActionInitialization();
		}
		SaveBindingInfoToOptions();
	}

	public static int GetDeviceIndex(InputDevice device)
	{
		int result = -1;
		for (int i = 0; i < InputManager.Devices.Count; i++)
		{
			if (InputManager.Devices[i] == device)
			{
				result = i;
			}
		}
		return result;
	}

	public static XInputDevice GetXInputDeviceInSlot(int xInputSlot)
	{
		for (int i = 0; i < InputManager.Devices.Count; i++)
		{
			if (InputManager.Devices[i] is XInputDevice)
			{
				XInputDevice xInputDevice = InputManager.Devices[i] as XInputDevice;
				if (xInputDevice.DeviceIndex == xInputSlot)
				{
					return xInputDevice;
				}
			}
		}
		return null;
	}

	public static void ReassignPlayerPort(int playerID, int portNum)
	{
		GameManager.Options.PlayerIDtoDeviceIndexMap.Remove(playerID);
		GameManager.Options.PlayerIDtoDeviceIndexMap.Add(playerID, portNum);
		for (int i = 0; i < m_instances.Count; i++)
		{
			if (m_instances[i].m_activeGungeonActions != null)
			{
				m_instances[i].m_activeGungeonActions.Destroy();
			}
			m_instances[i].m_activeGungeonActions = null;
		}
		InControlInputAdapter.SkipInputForRestOfFrame = true;
	}

	public static bool HasInstanceForPlayer(int id)
	{
		return m_instances.ContainsKey(id) && m_instances[id] != null && (bool)m_instances[id];
	}

	public static BraveInput GetInstanceForPlayer(int id)
	{
		if (m_instances.ContainsKey(id) && (m_instances[id] == null || !m_instances[id]))
		{
			m_instances.Remove(id);
		}
		if (!m_instances.ContainsKey(id))
		{
			if (m_instances.ContainsKey(0))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_instances[0].gameObject);
				BraveInput component = gameObject.GetComponent<BraveInput>();
				component.m_playerID = id;
				m_instances.Add(id, component);
			}
			else
			{
				Debug.LogError("Player " + id + " is attempting to get a BraveInput instance, and player 0's doesn't exist.");
			}
		}
		if (!m_instances.ContainsKey(id))
		{
			return null;
		}
		return m_instances[id];
	}

	public void Awake()
	{
		if (!m_instances.ContainsKey(0))
		{
			m_instances.Add(0, this);
		}
	}

	public void OnDestroy()
	{
		if (m_activeGungeonActions != null)
		{
			m_activeGungeonActions.Destroy();
			m_activeGungeonActions = null;
		}
		if (m_instances.ContainsValue(this))
		{
			m_instances.Remove(m_playerID);
		}
	}

	private void AssignActionsDevice()
	{
		if (GameManager.PreventGameManagerExistence || GameManager.Instance.AllPlayers.Length < 2)
		{
			m_activeGungeonActions.Device = InputManager.ActiveDevice;
			return;
		}
		m_activeGungeonActions.Device = InputManager.GetActiveDeviceForPlayer(m_playerID);
		if (m_playerID != 0 && m_activeGungeonActions.Device == InputManager.GetActiveDeviceForPlayer(0))
		{
			m_activeGungeonActions.ForceDisable = true;
		}
	}

	private static void TryLoadBindings(int playerNum, GungeonActions actions)
	{
		string text;
		string text2;
		switch (playerNum)
		{
		case 0:
			text = GameManager.Options.playerOneBindingData;
			text2 = GameManager.Options.playerOneBindingDataV2;
			break;
		case 1:
			text = GameManager.Options.playerTwoBindingData;
			text2 = GameManager.Options.playerTwoBindingDataV2;
			break;
		default:
			return;
		}
		if (!string.IsNullOrEmpty(text2))
		{
			actions.Load(text2);
		}
		else if (!string.IsNullOrEmpty(text))
		{
			actions.Load(text, true);
		}
		actions.PostProcessAdditionalBlankControls(playerNum);
	}

	public void CheckForActionInitialization()
	{
		if (m_activeGungeonActions == null)
		{
			m_activeGungeonActions = new GungeonActions();
			AssignActionsDevice();
			m_activeGungeonActions.InitializeDefaults();
			if (GameManager.PreventGameManagerExistence || (GameManager.Instance.PrimaryPlayer == null && m_playerID == 0) || m_playerID == GameManager.Instance.PrimaryPlayer.PlayerIDX)
			{
				TryLoadBindings(0, ActiveActions);
			}
			else
			{
				TryLoadBindings(1, ActiveActions);
			}
			if (!GameManager.PreventGameManagerExistence && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				if (m_playerID == 0 && GetInstanceForPlayer(GameManager.Instance.SecondaryPlayer.PlayerIDX).m_activeGungeonActions == null)
				{
					GetInstanceForPlayer(GameManager.Instance.SecondaryPlayer.PlayerIDX).CheckForActionInitialization();
				}
				if (m_activeGungeonActions.Device == null)
				{
					m_activeGungeonActions.IgnoreBindingsOfType(BindingSourceType.DeviceBindingSource);
				}
				else if (m_playerID == GameManager.Instance.PrimaryPlayer.PlayerIDX)
				{
					if (GetInstanceForPlayer(GameManager.Instance.SecondaryPlayer.PlayerIDX).m_activeGungeonActions.Device == null)
					{
						m_activeGungeonActions.IgnoreBindingsOfType(BindingSourceType.KeyBindingSource);
						m_activeGungeonActions.IgnoreBindingsOfType(BindingSourceType.MouseBindingSource);
					}
				}
				else
				{
					m_activeGungeonActions.IgnoreBindingsOfType(BindingSourceType.KeyBindingSource);
					m_activeGungeonActions.IgnoreBindingsOfType(BindingSourceType.MouseBindingSource);
				}
			}
		}
		AssignActionsDevice();
	}

	public void Update()
	{
		if (GameManager.Options.PlayerIDtoDeviceIndexMap == null || GameManager.Options.PlayerIDtoDeviceIndexMap.Count == 0)
		{
			DoStartupAssignmentOfControllers();
		}
		CheckForActionInitialization();
		LinkedListNode<PressAction> linkedListNode = m_pressActions.First;
		while (linkedListNode != null)
		{
			PressAction obj = linkedListNode.Value;
			obj.Timer += GameManager.INVARIANT_DELTA_TIME;
			if (obj.Timer >= obj.Buffer)
			{
				LinkedListNode<PressAction> next = linkedListNode.Next;
				PressAction.Pool.Free(ref obj);
				m_pressActions.Remove(linkedListNode, true);
				linkedListNode = next;
			}
			else
			{
				linkedListNode = linkedListNode.Next;
			}
		}
		for (int i = 0; i < PressActions.Length; i++)
		{
			if (m_activeGungeonActions.GetActionFromType(PressActions[i].Control).WasPressed)
			{
				PressAction pressAction = PressAction.Pool.Allocate();
				pressAction.SetAll(PressActions[i]);
				m_pressActions.AddLast(pressAction);
			}
		}
		LinkedListNode<HoldAction> linkedListNode2 = m_holdActions.First;
		while (linkedListNode2 != null)
		{
			HoldAction value = linkedListNode2.Value;
			value.DownTimer += GameManager.INVARIANT_DELTA_TIME;
			if (!value.Held)
			{
				value.UpTimer += GameManager.INVARIANT_DELTA_TIME;
			}
			else if (!m_activeGungeonActions.GetActionFromType(value.Control).IsPressed)
			{
				value.Held = false;
			}
			LinkedListNode<HoldAction> linkedListNode3 = linkedListNode2;
			linkedListNode2 = linkedListNode2.Next;
			if (!value.Held)
			{
				if (value.ConsumedDown && value.ConsumedUp)
				{
					HoldAction obj2 = linkedListNode3.Value;
					m_holdActions.Remove(linkedListNode3, true);
					HoldAction.Pool.Free(ref obj2);
				}
				else if (!value.ConsumedDown && value.UpTimer >= value.Buffer)
				{
					HoldAction obj3 = linkedListNode3.Value;
					m_holdActions.Remove(linkedListNode3, true);
					HoldAction.Pool.Free(ref obj3);
				}
			}
		}
		for (int j = 0; j < HoldActions.Length; j++)
		{
			if (m_activeGungeonActions.GetActionFromType(HoldActions[j].Control).WasPressed)
			{
				HoldAction holdAction = HoldAction.Pool.Allocate();
				holdAction.SetAll(HoldActions[j]);
				m_holdActions.AddLast(holdAction);
			}
		}
	}

	public void LateUpdate()
	{
		if (!GameManager.Options.RumbleEnabled || GameManager.Instance.IsLoadingLevel)
		{
			SetVibration(0f, 0f);
			m_currentVibrations.Clear();
		}
		else if (GameManager.Instance.IsPaused && !AllowPausedRumble)
		{
			SetVibration(0f, 0f);
		}
		else
		{
			float b = 0f;
			float b2 = 0f;
			float a = Vibration.ConvertFromShakeMagnitude(GameManager.Instance.MainCameraController.ScreenShakeVibration);
			b = Mathf.Max(a, b);
			b2 = Mathf.Max(a, b2);
			b = Mathf.Max(m_sustainedLargeVibration, b);
			b2 = Mathf.Max(m_sustainedSmallVibration, b2);
			for (int num = m_currentVibrations.Count - 1; num >= 0; num--)
			{
				TimedVibration timedVibration = m_currentVibrations[num];
				b = Mathf.Max(timedVibration.largeMotor, b);
				b2 = Mathf.Max(timedVibration.smallMotor, b2);
				if (GameManager.Instance.IsPaused && AllowPausedRumble)
				{
					timedVibration.timer -= GameManager.INVARIANT_DELTA_TIME;
				}
				else
				{
					timedVibration.timer -= BraveTime.DeltaTime;
				}
				if (timedVibration.timer < 0f)
				{
					m_currentVibrations.RemoveAt(num);
				}
			}
			SetVibration(b, b2);
		}
		GameManager.Instance.MainCameraController.MarkScreenShakeVibrationDirty();
		m_sustainedLargeVibration = 0f;
		m_sustainedSmallVibration = 0f;
	}

	public BindingSourceType GetLastInputType()
	{
		if (m_activeGungeonActions == null)
		{
			return BindingSourceType.None;
		}
		return m_activeGungeonActions.LastInputType;
	}

	public bool IsKeyboardAndMouse(bool includeNone = false)
	{
		if (m_activeGungeonActions == null)
		{
			return true;
		}
		return (includeNone && m_activeGungeonActions.LastInputType == BindingSourceType.None) || m_activeGungeonActions.LastInputType == BindingSourceType.KeyBindingSource || m_activeGungeonActions.LastInputType == BindingSourceType.MouseBindingSource;
	}

	public bool HasMouse()
	{
		if (m_activeGungeonActions == null)
		{
			return true;
		}
		return m_activeGungeonActions.LastInputType == BindingSourceType.KeyBindingSource || m_activeGungeonActions.LastInputType == BindingSourceType.MouseBindingSource;
	}

	public static void FlushAll()
	{
		for (int i = 0; i < m_instances.Count; i++)
		{
			m_instances[i].Flush();
		}
	}

	public void Flush()
	{
		while (m_pressActions.Count > 0)
		{
			PressAction obj = m_pressActions.First.Value;
			PressAction.Pool.Free(ref obj);
			m_pressActions.RemoveFirst();
		}
		while (m_holdActions.Count > 0)
		{
			HoldAction obj2 = m_holdActions.First.Value;
			HoldAction.Pool.Free(ref obj2);
			m_holdActions.RemoveFirst();
		}
	}

	public void DoVibration(Vibration.Time time, Vibration.Strength strength)
	{
		m_currentVibrations.Add(new TimedVibration(Vibration.ConvertTime(time), Vibration.ConvertStrength(strength)));
	}

	public void DoVibration(float time, Vibration.Strength strength)
	{
		m_currentVibrations.Add(new TimedVibration(time, Vibration.ConvertStrength(strength)));
	}

	public void DoVibration(Vibration.Time time, Vibration.Strength largeMotor, Vibration.Strength smallMotor)
	{
		m_currentVibrations.Add(new TimedVibration(Vibration.ConvertTime(time), Vibration.ConvertStrength(largeMotor), Vibration.ConvertStrength(smallMotor)));
	}

	public void DoScreenShakeVibration(float time, float magnitude)
	{
		m_currentVibrations.Add(new TimedVibration(time, Vibration.ConvertFromShakeMagnitude(magnitude)));
	}

	public void DoSustainedVibration(Vibration.Strength strength)
	{
		m_sustainedLargeVibration = Mathf.Max(m_sustainedLargeVibration, Vibration.ConvertStrength(strength));
	}

	public void DoSustainedVibration(Vibration.Strength largeMotor, Vibration.Strength smallMotor)
	{
		m_sustainedLargeVibration = Mathf.Max(m_sustainedLargeVibration, Vibration.ConvertStrength(largeMotor));
		m_sustainedSmallVibration = Mathf.Max(m_sustainedSmallVibration, Vibration.ConvertStrength(smallMotor));
	}

	public static void DoVibrationForAllPlayers(Vibration.Time time, Vibration.Strength strength)
	{
		for (int i = 0; i < m_instances.Count; i++)
		{
			if (m_instances[i] != null)
			{
				m_instances[i].DoVibration(time, strength);
			}
		}
	}

	public static void DoVibrationForAllPlayers(Vibration.Time time, Vibration.Strength largeMotor, Vibration.Strength smallMotor)
	{
		for (int i = 0; i < m_instances.Count; i++)
		{
			if (m_instances[i] != null)
			{
				m_instances[i].DoVibration(time, largeMotor, smallMotor);
			}
		}
	}

	public static void DoSustainedScreenShakeVibration(float magnitude)
	{
		for (int i = 0; i < m_instances.Count; i++)
		{
			if (m_instances[i] != null)
			{
				m_instances[i].m_sustainedLargeVibration = Mathf.Max(m_instances[i].m_sustainedLargeVibration, Vibration.ConvertFromShakeMagnitude(magnitude));
				m_instances[i].m_sustainedSmallVibration = Mathf.Max(m_instances[i].m_sustainedSmallVibration, Vibration.ConvertFromShakeMagnitude(magnitude));
			}
		}
	}

	private void SetVibration(float largeMotor, float smallMotor)
	{
		if (m_activeGungeonActions != null && m_activeGungeonActions.Device != null)
		{
			m_activeGungeonActions.Device.Vibrate(largeMotor, smallMotor);
		}
	}

	private bool CheckBufferedActionsForControlType(BufferedInput[] bufferedInputs, GungeonActions.GungeonActionType controlType)
	{
		for (int i = 0; i < bufferedInputs.Length; i++)
		{
			if (bufferedInputs[i].Control == controlType)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckPressActionsForControlType(GungeonActions.GungeonActionType controlType)
	{
		for (LinkedListNode<PressAction> linkedListNode = m_pressActions.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			if (linkedListNode.Value.Control == controlType)
			{
				return true;
			}
		}
		return false;
	}

	private PressAction GetPressActionForControlType(GungeonActions.GungeonActionType controlType)
	{
		for (LinkedListNode<PressAction> linkedListNode = m_pressActions.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			if (linkedListNode.Value.Control == controlType)
			{
				return linkedListNode.Value;
			}
		}
		return null;
	}

	private HoldAction GetHoldActionForControlType(GungeonActions.GungeonActionType controlType)
	{
		for (LinkedListNode<HoldAction> linkedListNode = m_holdActions.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			if (linkedListNode.Value.Control == controlType)
			{
				return linkedListNode.Value;
			}
		}
		return null;
	}

	public bool GetButtonDown(GungeonActions.GungeonActionType controlType)
	{
		if (CheckBufferedActionsForControlType(PressActions, controlType))
		{
			return CheckPressActionsForControlType(controlType);
		}
		if (CheckBufferedActionsForControlType(HoldActions, controlType))
		{
			HoldAction holdActionForControlType = GetHoldActionForControlType(controlType);
			if (holdActionForControlType != null)
			{
				return !holdActionForControlType.ConsumedDown;
			}
			return false;
		}
		Debug.LogError(string.Format("BraveInput.GetButtonDown(): {0} isn't registered with the BraveInput object", controlType));
		return false;
	}

	public void ConsumeButtonDown(GungeonActions.GungeonActionType controlType)
	{
		if (CheckBufferedActionsForControlType(PressActions, controlType))
		{
			PressAction obj = GetPressActionForControlType(controlType);
			if (obj != null)
			{
				m_pressActions.Remove(obj);
				PressAction.Pool.Free(ref obj);
			}
			else
			{
				Debug.LogError(string.Format("BraveInput.ConsumeButtonDown(): No action for {0} was found", controlType.ToString()));
			}
		}
		else if (CheckBufferedActionsForControlType(HoldActions, controlType))
		{
			HoldAction holdActionForControlType = GetHoldActionForControlType(controlType);
			if (holdActionForControlType != null)
			{
				holdActionForControlType.ConsumedDown = true;
			}
			else if (!MemoryTester.HasInstance)
			{
				Debug.LogError(string.Format("BraveInput.ConsumeButtonDown(): No action for {0} was found", controlType.ToString()));
			}
		}
		else
		{
			Debug.LogError(string.Format("BraveInput.ConsumeButtonDown(): {0} isn't registered with the BraveInput object", controlType.ToString()));
		}
	}

	public bool GetButton(GungeonActions.GungeonActionType controlType)
	{
		if (CheckBufferedActionsForControlType(HoldActions, controlType))
		{
			HoldAction holdActionForControlType = GetHoldActionForControlType(controlType);
			if (holdActionForControlType != null)
			{
				return holdActionForControlType.ConsumedDown && holdActionForControlType.Held;
			}
			return false;
		}
		if (!MemoryTester.HasInstance)
		{
			Debug.LogError(string.Format("BraveInput.GetButtonDown(): {0} isn't a registered hold action with the BraveInput object", controlType.ToString()));
		}
		return false;
	}

	public bool GetButtonUp(GungeonActions.GungeonActionType controlType)
	{
		if (CheckBufferedActionsForControlType(HoldActions, controlType))
		{
			HoldAction holdActionForControlType = GetHoldActionForControlType(controlType);
			if (holdActionForControlType != null)
			{
				return !holdActionForControlType.Held && holdActionForControlType.ConsumedDown && !holdActionForControlType.ConsumedUp;
			}
			return false;
		}
		if (!MemoryTester.HasInstance)
		{
			Debug.LogError(string.Format("BraveInput.GetButtonDown(): {0} isn't a registered hold action with the BraveInput object", controlType.ToString()));
		}
		return false;
	}

	public void ConsumeButtonUp(GungeonActions.GungeonActionType controlType)
	{
		if (CheckBufferedActionsForControlType(HoldActions, controlType))
		{
			HoldAction holdActionForControlType = GetHoldActionForControlType(controlType);
			if (holdActionForControlType != null)
			{
				holdActionForControlType.ConsumedUp = true;
			}
			else if (!MemoryTester.HasInstance)
			{
				Debug.LogError(string.Format("BraveInput.ConsumeButtonUp(): No action for {0} was found", controlType.ToString()));
			}
		}
		else
		{
			Debug.LogError(string.Format("BraveInput.ConsumeButtonUp(): {0} isn't registered with the BraveInput object", controlType.ToString()));
		}
	}

	public static void ConsumeAllAcrossInstances(GungeonActions.GungeonActionType controlType)
	{
		for (int i = 0; i < m_instances.Count; i++)
		{
			m_instances[i].ConsumeAll(controlType);
		}
	}

	public void ConsumeAll(GungeonActions.GungeonActionType controlType)
	{
		LinkedListNode<PressAction> linkedListNode = m_pressActions.First;
		while (linkedListNode != null)
		{
			LinkedListNode<PressAction> linkedListNode2 = linkedListNode;
			linkedListNode = linkedListNode.Next;
			if (linkedListNode2.Value.Control == controlType)
			{
				PressAction obj = linkedListNode2.Value;
				m_pressActions.Remove(linkedListNode2, true);
				PressAction.Pool.Free(ref obj);
			}
		}
		LinkedListNode<HoldAction> linkedListNode3 = m_holdActions.First;
		while (linkedListNode3 != null)
		{
			LinkedListNode<HoldAction> linkedListNode4 = linkedListNode3;
			linkedListNode3 = linkedListNode3.Next;
			if (linkedListNode4.Value.Control == controlType && !linkedListNode4.Value.ConsumedDown)
			{
				HoldAction obj2 = linkedListNode4.Value;
				m_holdActions.Remove(linkedListNode4, true);
				HoldAction.Pool.Free(ref obj2);
			}
		}
	}

	public bool WasAdvanceDialoguePressed(out bool suppressThisClick)
	{
		suppressThisClick = false;
		if (MenuInteractPressed)
		{
			return true;
		}
		if (IsKeyboardAndMouse())
		{
			if (Input.GetMouseButtonDown(0))
			{
				suppressThisClick = true;
				return true;
			}
			return Input.GetKeyDown(KeyCode.Return);
		}
		return false;
	}

	public bool WasAdvanceDialoguePressed()
	{
		if (MenuInteractPressed)
		{
			return true;
		}
		if (IsKeyboardAndMouse())
		{
			return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return);
		}
		return false;
	}

	public static GameOptions.ControllerSymbology GetCurrentSymbology(int id)
	{
		GameOptions.ControllerSymbology controllerSymbology = ((id != 0) ? GameManager.Options.PlayerTwoPreferredSymbology : GameManager.Options.PlayerOnePreferredSymbology);
		if (controllerSymbology == GameOptions.ControllerSymbology.AutoDetect)
		{
			BraveInput instanceForPlayer = GetInstanceForPlayer(id);
			if (instanceForPlayer != null && !instanceForPlayer.IsKeyboardAndMouse())
			{
				InputDevice device = instanceForPlayer.ActiveActions.Device;
				if (device != null)
				{
					controllerSymbology = device.ControllerSymbology;
				}
			}
		}
		if (controllerSymbology == GameOptions.ControllerSymbology.AutoDetect)
		{
			controllerSymbology = GameOptions.ControllerSymbology.Xbox;
		}
		return controllerSymbology;
	}

	public static bool WasSelectPressed(InputDevice device = null)
	{
		if (device == null)
		{
			device = InputManager.ActiveDevice;
		}
		if (device.Action1.WasPressed)
		{
			return true;
		}
		if (GameManager.HasInstance && GameManager.Options.allowUnknownControllers && GetInstanceForPlayer(0).ActiveActions.MenuSelectAction.WasPressed)
		{
			return true;
		}
		return false;
	}

	public static bool WasCancelPressed(InputDevice device = null)
	{
		if (device == null)
		{
			device = InputManager.ActiveDevice;
		}
		if (device.Action2.WasPressed)
		{
			return true;
		}
		if (GameManager.HasInstance && GameManager.Options.allowUnknownControllers && GetInstanceForPlayer(0).ActiveActions.CancelAction.WasPressed)
		{
			return true;
		}
		return false;
	}
}
