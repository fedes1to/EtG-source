using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraController : BraveBehaviour
{
	[Serializable]
	public class ControllerCamSettings
	{
		public float VisibleBorder = 4f;

		public AnimationCurve BorderBumperCurve;

		public float ToHallwayTime = 1.5f;

		public float ToRoomTime = 1.5f;

		public float ToRoomLockTime = 1f;

		public float EndRoomLockTime = 2f;

		public float AimContribution = 5f;

		public float AimContributionTime = 0.5f;

		public float AimContributionFastTime = 0.25f;

		public float AimContributionSlowTime = 1f;

		[NonSerialized]
		public ControllerCameraState state;

		[NonSerialized]
		public bool isTransitioning;

		[NonSerialized]
		public float transitionTimer;

		[NonSerialized]
		public float transitionDuration;

		[NonSerialized]
		public Vector2 transitionStart;

		[NonSerialized]
		public float forceTimer;

		[NonSerialized]
		public RoomHandler exitRoomOne;

		[NonSerialized]
		public RoomHandler exitRoomTwo;

		public bool UseAimContribution
		{
			get
			{
				return GameManager.Options.controllerAimLookMultiplier > 0f && !GameManager.Instance.MainCameraController.PreventAimLook;
			}
		}

		public float ModifiedAimContribution
		{
			get
			{
				return AimContribution * GameManager.Options.controllerAimLookMultiplier;
			}
		}
	}

	public enum ControllerCameraState
	{
		FollowPlayer,
		RoomLock,
		Off
	}

	public ControllerCamSettings controllerCamera;

	private const float c_screenShakeClamp = 5f;

	public float screenShakeDist;

	[CurveRange(0f, 0f, 1f, 1f)]
	public AnimationCurve screenShakeCurve;

	private PlayerController m_player;

	[SerializeField]
	private float z_Offset = -10f;

	public bool IsPerspectiveMode;

	[HideInInspector]
	public float CurrentStickyFriction = 1f;

	[HideInInspector]
	public Vector3 OverridePosition;

	[HideInInspector]
	public bool UseOverridePlayerOnePosition;

	[HideInInspector]
	public Vector2 OverridePlayerOnePosition;

	[HideInInspector]
	public bool UseOverridePlayerTwoPosition;

	[HideInInspector]
	public Vector2 OverridePlayerTwoPosition;

	[NonSerialized]
	public float OverrideZoomScale = 1f;

	[NonSerialized]
	public float CurrentZoomScale = 1f;

	private float m_screenShakeVibration;

	private bool m_screenShakeVibrationDirty;

	private Vector3 screenShakeAmount = Vector3.zero;

	private Vector2 previousBasePosition;

	private Dictionary<Component, IEnumerator> continuousShakeMap = new Dictionary<Component, IEnumerator>();

	private List<IEnumerator> activeContinuousShakes = new List<IEnumerator>();

	private bool m_isTrackingPlayer = true;

	private bool m_manualControl;

	private bool m_isLerpingToManualControl;

	private bool m_isRecoveringFromManualControl;

	private Vector2 m_lastAimOffset = Vector2.zero;

	private Vector2 m_aimOffsetVelocity = Vector2.zero;

	private Vector3 m_currentVelocity;

	private Camera m_camera;

	[NonSerialized]
	public float OverrideRecoverySpeed = -1f;

	private const float RECOVERY_SPEED = 20f;

	[NonSerialized]
	public Vector3 FINAL_CAMERA_POSITION_OFFSET;

	public Action OnFinishedFrame;

	private List<UnityEngine.Object> m_focusObjects = new List<UnityEngine.Object>();

	private Vector2 m_cachedMinPos;

	private Vector2 m_cachedMaxPos;

	private Vector2 m_cachedSize;

	private static Vector2 m_cachedCameraMin;

	private static Vector2 m_cachedCameraMax;

	private const float COOP_REDUCTION = 0.3f;

	private const float c_newScreenShakeModeScalar = 0.5f;

	private bool m_terminateNextContinuousScreenShake;

	public float CurrentZOffset
	{
		get
		{
			if (IsPerspectiveMode)
			{
				return base.transform.position.y - 40f;
			}
			return z_Offset;
		}
	}

	public bool IsCurrentlyZoomIntermediate
	{
		get
		{
			return CurrentZoomScale != OverrideZoomScale;
		}
	}

	public Vector3 ScreenShakeVector
	{
		get
		{
			return screenShakeAmount;
		}
	}

	public float ScreenShakeVibration
	{
		get
		{
			return m_screenShakeVibration;
		}
	}

	public bool ManualControl
	{
		get
		{
			return m_manualControl;
		}
	}

	public bool PreventAimLook { get; set; }

	public Camera Camera
	{
		get
		{
			if (!m_camera && (bool)this)
			{
				m_camera = GetComponent<Camera>();
			}
			return m_camera;
		}
	}

	private float m_deltaTime
	{
		get
		{
			return GameManager.INVARIANT_DELTA_TIME;
		}
	}

	public bool LockX { get; set; }

	public bool LockY { get; set; }

	public bool LockToRoom { get; set; }

	public bool PreventFuseBombAimOffset { get; set; }

	public static Vector3 PLATFORM_CAMERA_OFFSET
	{
		get
		{
			if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11)
			{
				return Vector3.zero;
			}
			return new Vector3(1f / 32f, 1f / 32f, 0f);
		}
	}

	public static bool SuperSmoothCamera
	{
		get
		{
			return GameManager.Options.SuperSmoothCamera;
		}
	}

	private bool UseMouseAim
	{
		get
		{
			if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.XboxOne)
			{
				return false;
			}
			if (!GameManager.Options.mouseAimLook)
			{
				return false;
			}
			return BraveInput.GetInstanceForPlayer(0).IsKeyboardAndMouse() && !LockToRoom;
		}
	}

	public bool IsLerping
	{
		get
		{
			return (!m_manualControl) ? m_isRecoveringFromManualControl : m_isLerpingToManualControl;
		}
		set
		{
			if (m_manualControl)
			{
				m_isLerpingToManualControl = true;
			}
			else
			{
				m_isRecoveringFromManualControl = true;
			}
		}
	}

	public Vector2 MinVisiblePoint
	{
		get
		{
			return m_cachedMinPos;
		}
	}

	public Vector2 MaxVisiblePoint
	{
		get
		{
			return m_cachedMaxPos;
		}
	}

	public void ClearPlayerCache()
	{
		m_player = null;
	}

	public void SetZoomScaleImmediate(float zoomScale)
	{
		OverrideZoomScale = zoomScale;
		CurrentZoomScale = zoomScale;
		if (Pixelator.HasInstance)
		{
			Pixelator.Instance.NUM_MACRO_PIXELS_HORIZONTAL = (int)((float)BraveCameraUtility.H_PIXELS / CurrentZoomScale).Quantize(2f);
			Pixelator.Instance.NUM_MACRO_PIXELS_VERTICAL = (int)((float)BraveCameraUtility.V_PIXELS / CurrentZoomScale).Quantize(2f);
		}
	}

	public void UpdateScreenShakeVibration(float newVibration)
	{
		if (m_screenShakeVibrationDirty)
		{
			m_screenShakeVibration = 0f;
			m_screenShakeVibrationDirty = false;
		}
		m_screenShakeVibration = Mathf.Max(m_screenShakeVibration, newVibration);
	}

	public void MarkScreenShakeVibrationDirty()
	{
		m_screenShakeVibrationDirty = true;
	}

	private void Awake()
	{
		BraveTime.CacheDeltaTimeForFrame();
	}

	private void Start()
	{
		m_camera = GetComponent<Camera>();
		FINAL_CAMERA_POSITION_OFFSET = PLATFORM_CAMERA_OFFSET;
		if (m_player == null)
		{
			m_player = GameManager.Instance.PrimaryPlayer;
		}
		screenShakeAmount = new Vector3(0f, 0f, 0f);
	}

	public void AddFocusPoint(GameObject go)
	{
		if (!m_focusObjects.Contains(go))
		{
			m_focusObjects.Add(go);
		}
	}

	public void AddFocusPoint(SpeculativeRigidbody specRigidbody)
	{
		if (!m_focusObjects.Contains(specRigidbody))
		{
			m_focusObjects.Add(specRigidbody);
		}
	}

	public void RemoveFocusPoint(GameObject go)
	{
		m_focusObjects.Remove(go);
	}

	public void RemoveFocusPoint(SpeculativeRigidbody specRigidbody)
	{
		m_focusObjects.Remove(specRigidbody);
	}

	private Vector2 GetPlayerPosition(PlayerController targetPlayer)
	{
		if (targetPlayer.IsPrimaryPlayer)
		{
			return UseOverridePlayerOnePosition ? OverridePlayerOnePosition : ((!SuperSmoothCamera) ? targetPlayer.CenterPosition : targetPlayer.SmoothedCameraCenter);
		}
		return UseOverridePlayerTwoPosition ? OverridePlayerTwoPosition : ((!SuperSmoothCamera) ? targetPlayer.CenterPosition : targetPlayer.SmoothedCameraCenter);
	}

	public Vector2 GetCoreCurrentBasePosition()
	{
		if (m_player == null)
		{
			m_player = GameManager.Instance.PrimaryPlayer;
		}
		Vector2 prevAverage = Vector2.zero;
		int prevCount = 0;
		if (GameManager.Instance.AllPlayers.Length < 2)
		{
			if (m_player == null)
			{
				return Vector2.zero;
			}
			BraveMathCollege.WeightedAverage(GetPlayerPosition(m_player), ref prevAverage, ref prevCount);
		}
		else
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (GameManager.Instance.AllPlayers[i].gameObject.activeSelf && !GameManager.Instance.AllPlayers[i].IgnoredByCamera && !GameManager.Instance.AllPlayers[i].IsGhost)
				{
					BraveMathCollege.WeightedAverage(GetPlayerPosition(GameManager.Instance.AllPlayers[i]), ref prevAverage, ref prevCount);
				}
			}
			if (prevCount > 1)
			{
				prevCount = 1;
			}
		}
		for (int j = 0; j < m_focusObjects.Count; j++)
		{
			if (m_focusObjects[j] is GameObject)
			{
				BraveMathCollege.WeightedAverage((m_focusObjects[j] as GameObject).transform.position, ref prevAverage, ref prevCount);
			}
			else if (m_focusObjects[j] is SpeculativeRigidbody)
			{
				BraveMathCollege.WeightedAverage((m_focusObjects[j] as SpeculativeRigidbody).GetUnitCenter(ColliderType.HitBox), ref prevAverage, ref prevCount);
			}
		}
		return prevAverage;
	}

	public Vector2 GetIdealCameraPosition()
	{
		Vector2 coreCurrentBasePosition = GetCoreCurrentBasePosition();
		return coreCurrentBasePosition + GetCoreOffset(coreCurrentBasePosition, false, true);
	}

	private Vector2 GetCoreOffset(Vector2 currentBasePosition, bool isUpdate, bool allowAimOffset)
	{
		if (UseMouseAim)
		{
			Vector2 result = Vector2.zero;
			if (allowAimOffset && GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER)
			{
				Vector2 vector = m_camera.ScreenToWorldPoint(Input.mousePosition).XY();
				Vector2 vector2 = vector - currentBasePosition;
				vector2 = new Vector2(vector2.x / m_camera.aspect, vector2.y);
				vector2.x = Mathf.Clamp(vector2.x, m_camera.orthographicSize * -1.5f, m_camera.orthographicSize * 1.5f);
				vector2.y = Mathf.Clamp(vector2.y, m_camera.orthographicSize * -1.5f, m_camera.orthographicSize * 1.5f);
				float num = Mathf.Lerp(0f, 0.33333f, Mathf.Pow(Mathf.Clamp01((vector2.magnitude - 1f) / (m_camera.orthographicSize - 1f)), 0.5f));
				result = vector2 * num;
			}
			if (result.magnitude < 1f / 64f)
			{
				result = Vector2.zero;
			}
			return result;
		}
		Vector2 vector3 = Vector2.zero;
		GungeonActions activeActions = BraveInput.GetInstanceForPlayer(0).ActiveActions;
		if ((bool)Minimap.Instance && !Minimap.Instance.IsFullscreen && activeActions != null)
		{
			Vector2 vector4 = activeActions.Aim.Vector;
			vector3 = ((!(vector4.magnitude > 0.1f)) ? Vector2.zero : (vector4.normalized * controllerCamera.ModifiedAimContribution));
			if (vector3.y > 0f && PreventFuseBombAimOffset)
			{
				vector3.y = 0f;
			}
		}
		Vector2 aimOffset = Vector2.SmoothDamp(smoothTime: (vector3 == Vector2.zero) ? controllerCamera.AimContributionSlowTime : ((!(m_lastAimOffset != Vector2.zero) || !(Mathf.Abs(BraveMathCollege.ClampAngle180(vector3.ToAngle() - m_lastAimOffset.ToAngle())) > 135f)) ? controllerCamera.AimContributionTime : controllerCamera.AimContributionFastTime), current: m_lastAimOffset, target: vector3, currentVelocity: ref m_aimOffsetVelocity, maxSpeed: 20f, deltaTime: m_deltaTime);
		if (isUpdate)
		{
			m_lastAimOffset = aimOffset;
		}
		Vector2 vector5 = currentBasePosition;
		if (controllerCamera.state == ControllerCameraState.RoomLock)
		{
			Rect cameraBoundingRect = GameManager.Instance.PrimaryPlayer.CurrentRoom.cameraBoundingRect;
			cameraBoundingRect.yMin += 1f;
			cameraBoundingRect.height += 2f;
			vector5 = GetBoundedCameraPositionInRect(cameraBoundingRect, currentBasePosition, ref aimOffset);
		}
		if (controllerCamera.UseAimContribution && OverrideZoomScale == 1f && GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER)
		{
			vector5 += aimOffset;
		}
		Vector2 result2 = vector5 - currentBasePosition;
		if (result2.magnitude < 1f / 64f)
		{
			result2 = Vector2.zero;
		}
		return result2;
	}

	private void Update()
	{
		tk2dSpriteAnimator.CameraPositionThisFrame = base.transform.position.XY();
		if (m_screenShakeVibrationDirty)
		{
			m_screenShakeVibration = 0f;
			m_screenShakeVibrationDirty = false;
		}
	}

	private void AdjustRecoverySpeedFoyer()
	{
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER && OverrideRecoverySpeed > 0f)
		{
			if (OverrideRecoverySpeed >= 20f)
			{
				OverrideRecoverySpeed = -1f;
			}
			else
			{
				OverrideRecoverySpeed += BraveTime.DeltaTime * 3f;
			}
		}
	}

	private void LateUpdate()
	{
		controllerCamera.forceTimer = Mathf.Max(0f, controllerCamera.forceTimer - BraveTime.DeltaTime);
		m_terminateNextContinuousScreenShake = false;
		for (int i = 0; i < activeContinuousShakes.Count; i++)
		{
			activeContinuousShakes[i].MoveNext();
		}
		if (GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		if ((bool)Pixelator.Instance && (CurrentZoomScale != OverrideZoomScale || Pixelator.Instance.NUM_MACRO_PIXELS_HORIZONTAL != (int)((float)BraveCameraUtility.H_PIXELS / CurrentZoomScale).Quantize(2f)))
		{
			CurrentZoomScale = Mathf.MoveTowards(CurrentZoomScale, OverrideZoomScale, 0.5f * GameManager.INVARIANT_DELTA_TIME);
			float aspect = m_camera.aspect;
			int h_PIXELS = BraveCameraUtility.H_PIXELS;
			int v_PIXELS = BraveCameraUtility.V_PIXELS;
			Pixelator.Instance.NUM_MACRO_PIXELS_HORIZONTAL = (int)((float)h_PIXELS / CurrentZoomScale).Quantize(2f);
			Pixelator.Instance.NUM_MACRO_PIXELS_VERTICAL = (int)((float)v_PIXELS / CurrentZoomScale).Quantize(2f);
		}
		if (!m_manualControl)
		{
			Vector2 vector = ((!m_isTrackingPlayer) ? previousBasePosition : GetCoreCurrentBasePosition());
			if (!UseMouseAim && controllerCamera.forceTimer <= 0f)
			{
				bool flag = GameManager.Instance.PrimaryPlayer != null && GameManager.Instance.PrimaryPlayer.CurrentRoom != null && GameManager.Instance.PrimaryPlayer.CurrentRoom.IsSealed;
				if ((controllerCamera.state == ControllerCameraState.FollowPlayer || controllerCamera.state == ControllerCameraState.Off) && flag)
				{
					controllerCamera.state = ControllerCameraState.RoomLock;
					controllerCamera.isTransitioning = true;
					controllerCamera.transitionDuration = controllerCamera.ToRoomLockTime;
					controllerCamera.transitionStart = base.transform.position;
					controllerCamera.transitionTimer = 0f;
				}
				else if ((controllerCamera.state == ControllerCameraState.RoomLock || controllerCamera.state == ControllerCameraState.Off) && !flag)
				{
					controllerCamera.state = ControllerCameraState.FollowPlayer;
					controllerCamera.isTransitioning = true;
					controllerCamera.transitionDuration = controllerCamera.EndRoomLockTime;
					controllerCamera.transitionStart = base.transform.position;
					controllerCamera.transitionTimer = 0f;
				}
			}
			Vector2 coreOffset = GetCoreOffset(vector, true, m_isTrackingPlayer);
			vector += coreOffset;
			previousBasePosition = vector;
			Vector2 vector2 = vector;
			if (!UseMouseAim && controllerCamera.isTransitioning)
			{
				controllerCamera.transitionTimer += m_deltaTime;
				float t = Mathf.SmoothStep(0f, 1f, Mathf.Min(controllerCamera.transitionTimer / controllerCamera.transitionDuration, 1f));
				vector2 = Vector2.Lerp(controllerCamera.transitionStart, vector, t);
				if (controllerCamera.transitionTimer > controllerCamera.transitionDuration)
				{
					controllerCamera.isTransitioning = false;
				}
			}
			else if (m_isRecoveringFromManualControl)
			{
				Vector2 vector3 = base.transform.PositionVector2() - FINAL_CAMERA_POSITION_OFFSET.XY();
				float num = Vector2.Distance(vector2, vector3);
				AdjustRecoverySpeedFoyer();
				float num2 = ((!(OverrideRecoverySpeed > 0f)) ? 20f : OverrideRecoverySpeed);
				if (num > num2 * m_deltaTime)
				{
					vector2 = vector3 + (vector2 - vector3).normalized * num2 * m_deltaTime;
				}
				else
				{
					m_isRecoveringFromManualControl = false;
					OverrideRecoverySpeed = -1f;
				}
			}
			if (UseMouseAim)
			{
				controllerCamera.state = ControllerCameraState.Off;
				controllerCamera.isTransitioning = false;
			}
			Vector3 vector4 = screenShakeAmount * ScreenShakeSettings.GLOBAL_SHAKE_MULTIPLIER * GameManager.Options.ScreenShakeMultiplier;
			Vector3 vector5 = ((!(vector4.magnitude > 5f)) ? vector4 : (vector4.normalized * 5f));
			if (float.IsNaN(vector5.x) || float.IsInfinity(vector5.x))
			{
				vector5.x = 0f;
			}
			if (float.IsNaN(vector5.y) || float.IsInfinity(vector5.y))
			{
				vector5.y = 0f;
			}
			if (float.IsNaN(vector5.z) || float.IsInfinity(vector5.z))
			{
				vector5.z = 0f;
			}
			if (GameManager.Instance.IsPaused)
			{
				vector5 = Vector3.zero;
			}
			Vector3 position = vector2.ToVector3ZUp(CurrentZOffset) + vector5;
			position += FINAL_CAMERA_POSITION_OFFSET;
			if (LockX)
			{
				position.x = base.transform.position.x;
			}
			if (LockY)
			{
				position.y = base.transform.position.y;
			}
			base.transform.position = position;
		}
		else
		{
			if (controllerCamera != null)
			{
				controllerCamera.isTransitioning = false;
				controllerCamera.transitionStart = base.transform.position;
			}
			GetCoreOffset(GetCoreCurrentBasePosition(), true, true);
			Vector2 vector6 = OverridePosition.XY();
			if (m_isLerpingToManualControl)
			{
				Vector2 vector7 = base.transform.PositionVector2() - FINAL_CAMERA_POSITION_OFFSET.XY();
				float num3 = Vector2.Distance(vector6, vector7);
				AdjustRecoverySpeedFoyer();
				float num4 = ((!(OverrideRecoverySpeed > 0f)) ? 20f : OverrideRecoverySpeed);
				if (num3 > num4 * m_deltaTime)
				{
					vector6 = vector7 + (vector6 - vector7).normalized * num4 * m_deltaTime;
				}
				else
				{
					m_isLerpingToManualControl = false;
				}
			}
			Vector3 vector8 = ((!(screenShakeAmount.magnitude > 5f)) ? screenShakeAmount : (screenShakeAmount.normalized * 5f));
			float screenShakeMultiplier = GameManager.Options.ScreenShakeMultiplier;
			Vector3 position2 = vector6.ToVector3ZUp(CurrentZOffset) + vector8 * ScreenShakeSettings.GLOBAL_SHAKE_MULTIPLIER * screenShakeMultiplier + FINAL_CAMERA_POSITION_OFFSET;
			if (LockX)
			{
				position2.x = base.transform.position.x;
			}
			if (LockY)
			{
				position2.y = base.transform.position.y;
			}
			if (float.IsNaN(position2.x) || float.IsNaN(position2.y))
			{
				Debug.LogWarning("THERE'S NaNS IN THEM THAR HILLS");
				position2 = GetCoreCurrentBasePosition();
			}
			base.transform.position = position2;
		}
		if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW || GameManager.Options.CurrentPreferredScalingMode == GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST)
		{
			base.transform.position = base.transform.position.Quantize(0.0625f) + PLATFORM_CAMERA_OFFSET;
		}
		Ray ray = Camera.main.ViewportPointToRay(new Vector2(0f, 0f));
		Plane plane = new Plane(Vector3.back, Vector3.zero);
		float enter;
		plane.Raycast(ray, out enter);
		m_cachedMinPos = ray.GetPoint(enter);
		ray = Camera.main.ViewportPointToRay(new Vector2(1f, 1f));
		plane.Raycast(ray, out enter);
		m_cachedMaxPos = ray.GetPoint(enter);
		m_cachedSize = m_cachedMaxPos - m_cachedMinPos;
		m_cachedCameraMin = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f));
		m_cachedCameraMax = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f));
		if (OnFinishedFrame != null)
		{
			OnFinishedFrame();
		}
	}

	public void SetManualControl(bool manualControl, bool shouldLerp = true)
	{
		m_manualControl = manualControl;
		if (m_manualControl)
		{
			m_isLerpingToManualControl = shouldLerp;
		}
		else
		{
			m_isRecoveringFromManualControl = shouldLerp;
		}
	}

	public void ForceUpdateControllerCameraState(ControllerCameraState newState)
	{
		controllerCamera.state = newState;
		controllerCamera.isTransitioning = false;
		controllerCamera.forceTimer = 6f;
	}

	public void UpdateOverridePosition(Vector3 newOverridePosition, float duration)
	{
		StartCoroutine(UpdateOverridePosition_CR(newOverridePosition, duration));
	}

	public IEnumerator UpdateOverridePosition_CR(Vector3 newOverridePosition, float duration)
	{
		float ela = 0f;
		Vector3 startOverride = OverridePosition;
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			OverridePosition = Vector3.Lerp(startOverride, newOverridePosition, ela / duration);
			yield return null;
		}
	}

	public Vector2 GetAimContribution()
	{
		if (m_manualControl || BraveInput.GetInstanceForPlayer(0).IsKeyboardAndMouse())
		{
			return Vector2.zero;
		}
		if (controllerCamera.UseAimContribution && OverrideZoomScale == 1f && GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER)
		{
			return m_lastAimOffset;
		}
		return Vector2.zero;
	}

	public void ResetAimContribution()
	{
		m_lastAimOffset = Vector2.zero;
		m_aimOffsetVelocity = Vector2.zero;
	}

	public void ForceToPlayerPosition(PlayerController p)
	{
		base.transform.position = BraveUtility.QuantizeVector(p.transform.position.WithZ(CurrentZOffset), PhysicsEngine.Instance.PixelsPerUnit) + new Vector3(1f / 32f, 1f / 32f, 0f);
		if (controllerCamera != null)
		{
			controllerCamera.isTransitioning = false;
			controllerCamera.transitionStart = base.transform.position;
		}
	}

	public void ForceToPlayerPosition(PlayerController p, Vector3 prevPlayerPosition)
	{
		Vector3 vector = base.transform.position - prevPlayerPosition;
		Vector3 vector2 = p.transform.position + vector;
		base.transform.position = BraveUtility.QuantizeVector(vector2.WithZ(CurrentZOffset), PhysicsEngine.Instance.PixelsPerUnit) + new Vector3(1f / 32f, 1f / 32f, 0f);
		if (controllerCamera != null)
		{
			controllerCamera.isTransitioning = false;
			controllerCamera.transitionStart = base.transform.position;
		}
	}

	public void AssignBoundingPolygon(RoomHandlerBoundingPolygon p)
	{
	}

	public void StartTrackingPlayer()
	{
		m_isTrackingPlayer = true;
	}

	public void StopTrackingPlayer()
	{
		m_isRecoveringFromManualControl = true;
		m_isTrackingPlayer = false;
	}

	public void DoScreenShake(ScreenShakeSettings shakesettings, Vector2? shakeOrigin, bool isPlayerGun = false)
	{
		float num = shakesettings.magnitude;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Options.CoopScreenShakeReduction)
		{
			num *= 0.3f;
		}
		if (isPlayerGun)
		{
			num *= 0.75f;
		}
		bool useCameraVibration = shakesettings.vibrationType != ScreenShakeSettings.VibrationType.None;
		if (shakesettings.vibrationType == ScreenShakeSettings.VibrationType.Simple)
		{
			BraveInput.DoVibrationForAllPlayers(shakesettings.simpleVibrationTime, shakesettings.simpleVibrationStrength);
			useCameraVibration = false;
		}
		StartCoroutine(HandleScreenShake(num, shakesettings.speed, shakesettings.time, shakesettings.falloff, shakesettings.direction, shakeOrigin, useCameraVibration));
	}

	public void DoScreenShake(float magnitude, float shakeSpeed, float time, float falloffTime, Vector2? shakeOrigin)
	{
		float num = magnitude;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Options.CoopScreenShakeReduction)
		{
			num *= 0.3f;
		}
		StartCoroutine(HandleScreenShake(num, shakeSpeed, time, falloffTime, Vector2.zero, shakeOrigin, true));
	}

	public void DoGunScreenShake(ScreenShakeSettings shakesettings, Vector2 dir, Vector2? shakeOrigin, PlayerController playerOwner = null)
	{
		float num = shakesettings.magnitude;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Options.CoopScreenShakeReduction)
		{
			num *= 0.3f;
		}
		if ((bool)playerOwner)
		{
			num *= 0.75f;
		}
		bool useCameraVibration = shakesettings.vibrationType != ScreenShakeSettings.VibrationType.None;
		if ((bool)playerOwner)
		{
			if (shakesettings.vibrationType == ScreenShakeSettings.VibrationType.Auto)
			{
				playerOwner.DoScreenShakeVibration(shakesettings.time, shakesettings.magnitude);
				useCameraVibration = false;
			}
			else if (shakesettings.vibrationType == ScreenShakeSettings.VibrationType.Simple)
			{
				playerOwner.DoVibration(shakesettings.simpleVibrationTime, shakesettings.simpleVibrationStrength);
				useCameraVibration = false;
			}
		}
		StartCoroutine(HandleScreenShake(num, shakesettings.speed, shakesettings.time, shakesettings.falloff, dir, shakeOrigin, useCameraVibration));
	}

	public bool PointIsVisible(Vector2 flatPoint)
	{
		return flatPoint.x > m_cachedMinPos.x && flatPoint.x < m_cachedMaxPos.x && flatPoint.y > m_cachedMinPos.y && flatPoint.y < m_cachedMaxPos.y;
	}

	public bool PointIsVisible(Vector2 flatPoint, float percentBuffer)
	{
		Vector2 vector = m_cachedSize * percentBuffer;
		return flatPoint.x > m_cachedMinPos.x - vector.x && flatPoint.x < m_cachedMaxPos.x + vector.x && flatPoint.y > m_cachedMinPos.y - vector.y && flatPoint.y < m_cachedMaxPos.y + vector.y;
	}

	public void DoContinuousScreenShake(ScreenShakeSettings shakesettings, Component source, bool isPlayerGun = false)
	{
		float num = shakesettings.magnitude;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Options.CoopScreenShakeReduction)
		{
			num *= 0.3f;
		}
		if (isPlayerGun)
		{
			num *= 0.75f;
		}
		bool useCameraVibration = shakesettings.vibrationType != ScreenShakeSettings.VibrationType.None;
		if (shakesettings.vibrationType == ScreenShakeSettings.VibrationType.Simple)
		{
			BraveInput.DoVibrationForAllPlayers(shakesettings.simpleVibrationTime, shakesettings.simpleVibrationStrength);
			useCameraVibration = false;
		}
		IEnumerator enumerator = HandleContinuousScreenShake(num, shakesettings.speed, shakesettings.direction, source, useCameraVibration);
		if (continuousShakeMap.ContainsKey(source))
		{
			Debug.LogWarning("Overwriting previous screen shake for " + source, source);
			StopContinuousScreenShake(source);
		}
		continuousShakeMap.Add(source, enumerator);
		activeContinuousShakes.Add(enumerator);
	}

	public void DoDelayedScreenShake(ScreenShakeSettings s, float delay, Vector2? shakeOrigin)
	{
		StartCoroutine(HandleDelayedScreenShake(s, delay, shakeOrigin));
	}

	public void StopContinuousScreenShake(Component source)
	{
		if (continuousShakeMap.ContainsKey(source))
		{
			IEnumerator enumerator = continuousShakeMap[source];
			m_terminateNextContinuousScreenShake = true;
			enumerator.MoveNext();
			continuousShakeMap.Remove(source);
			activeContinuousShakes.Remove(enumerator);
		}
	}

	public Vector3 DoFrameScreenShake(float magnitude, float shakeSpeed, Vector2 direction, Vector3 lastShakeAmount, float elapsedTime)
	{
		screenShakeAmount -= lastShakeAmount;
		if (direction == Vector2.zero)
		{
			float x = Mathf.PerlinNoise(0.3141567f + elapsedTime * shakeSpeed / 1.073f, 0.1156832f + elapsedTime * shakeSpeed / 4.8127f) * 2f - 1f;
			float y = Mathf.PerlinNoise(0.7159354f + elapsedTime * shakeSpeed / 2.3727f, 0.9315825f + elapsedTime * shakeSpeed / 0.9812f) * 2f - 1f;
			Vector2 vector = new Vector2(x, y);
			float num = magnitude - Mathf.PingPong(elapsedTime * shakeSpeed, magnitude) / magnitude * magnitude;
			Vector2 vector2 = vector.normalized * num;
			screenShakeAmount += new Vector3(vector2.x, vector2.y, 0f);
			BraveInput.DoSustainedScreenShakeVibration(magnitude);
			return new Vector3(vector2.x, vector2.y, 0f);
		}
		float num2 = Mathf.PingPong(elapsedTime * shakeSpeed, magnitude);
		Vector2 vector3 = new Vector2(num2 * direction.x, num2 * direction.y);
		screenShakeAmount += new Vector3(vector3.x, vector3.y, 0f);
		BraveInput.DoSustainedScreenShakeVibration(magnitude);
		return new Vector3(vector3.x, vector3.y, 0f);
	}

	public void ClearFrameScreenShake(Vector3 lastShakeAmount)
	{
		screenShakeAmount -= lastShakeAmount;
	}

	private IEnumerator HandleContinuousScreenShake(float magnitude, float shakeSpeed, Vector2 direction, Component source, bool useCameraVibration)
	{
		float t = 0f;
		Vector3 lastScreenShakeAmount = Vector3.zero;
		Vector2 baseDirection = ((!(direction != Vector2.zero)) ? new Vector2(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value - 0.5f).normalized : direction.normalized);
		magnitude *= 0.5f;
		while (!m_terminateNextContinuousScreenShake)
		{
			screenShakeAmount -= lastScreenShakeAmount;
			t += m_deltaTime * CurrentStickyFriction;
			float currentMagnitude = Mathf.PingPong(t * shakeSpeed, magnitude);
			Vector2 contribution = new Vector2(currentMagnitude * baseDirection.x, currentMagnitude * baseDirection.y);
			screenShakeAmount += new Vector3(contribution.x, contribution.y, 0f);
			lastScreenShakeAmount = new Vector3(contribution.x, contribution.y, 0f);
			if (useCameraVibration)
			{
				UpdateScreenShakeVibration(magnitude);
			}
			yield return null;
		}
		screenShakeAmount -= lastScreenShakeAmount;
		m_terminateNextContinuousScreenShake = false;
	}

	private IEnumerator HandleDelayedScreenShake(ScreenShakeSettings sss, float delay, Vector2? origin)
	{
		yield return new WaitForSeconds(delay);
		DoScreenShake(sss, origin);
	}

	private IEnumerator HandleScreenShake(float magnitude, float shakeSpeed, float time, float falloffTime, Vector2 direction, Vector2? origin, bool useCameraVibration)
	{
		if (origin.HasValue)
		{
			Vector2 b = BraveUtility.ScreenCenterWorldPoint();
			float num = Vector2.Distance(origin.Value, b);
			if (num > screenShakeDist)
			{
				yield break;
			}
			float num2 = Mathf.Clamp01(screenShakeCurve.Evaluate(num / screenShakeDist));
			magnitude *= num2;
			shakeSpeed *= num2;
		}
		if (magnitude == 0f)
		{
			yield break;
		}
		float t = 0f;
		Vector3 lastScreenShakeAmount = Vector3.zero;
		if (direction == Vector2.zero)
		{
			Vector4 randoms = new Vector4(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
			magnitude *= 0.5f;
			shakeSpeed *= 3.75f;
			while (t < time + falloffTime)
			{
				if (!GameManager.Instance.IsPaused)
				{
					screenShakeAmount -= lastScreenShakeAmount;
					float num3 = magnitude;
					if (t > time)
					{
						num3 = ((!(falloffTime <= 0f)) ? (Mathf.Sqrt(1f - (t - time) / falloffTime) * magnitude) : 0f);
					}
					float x = Mathf.PerlinNoise(randoms.x + t * shakeSpeed / 1.073f, randoms.y + t * shakeSpeed / 4.8127f) * 2f - 1f;
					float y = Mathf.PerlinNoise(randoms.z + t * shakeSpeed / 2.3727f, randoms.w + t * shakeSpeed / 0.9812f) * 2f - 1f;
					Vector2 vector = new Vector2(x, y);
					float num4 = num3 - Mathf.PingPong(t * shakeSpeed, magnitude) / magnitude * num3;
					Vector2 vector2 = vector.normalized * num4;
					if (float.IsNaN(vector2.x) || float.IsNaN(vector2.y))
					{
						yield break;
					}
					screenShakeAmount += new Vector3(vector2.x, vector2.y, 0f);
					lastScreenShakeAmount = new Vector3(vector2.x, vector2.y, 0f);
					if (useCameraVibration)
					{
						UpdateScreenShakeVibration(num3);
					}
					t += m_deltaTime * CurrentStickyFriction;
				}
				yield return null;
			}
			screenShakeAmount -= lastScreenShakeAmount;
			yield break;
		}
		Vector2 baseDirection = direction.normalized;
		magnitude *= 0.5f;
		while (t < time + falloffTime)
		{
			if (!GameManager.Instance.IsPaused)
			{
				screenShakeAmount -= lastScreenShakeAmount;
				float num5 = magnitude;
				if (t > time)
				{
					num5 = ((!(falloffTime <= 0f)) ? (Mathf.Sqrt(1f - (t - time) / falloffTime) * magnitude) : 0f);
				}
				float t2 = Mathf.Clamp01(Mathf.PingPong(t * shakeSpeed, magnitude) / magnitude);
				float num6 = Mathf.Lerp(num5, 0f - num5, t2);
				Vector2 vector3 = baseDirection * num6;
				if (float.IsNaN(vector3.x) || float.IsNaN(vector3.y))
				{
					yield break;
				}
				screenShakeAmount += new Vector3(vector3.x, vector3.y, 0f);
				lastScreenShakeAmount = new Vector3(vector3.x, vector3.y, 0f);
				if (useCameraVibration)
				{
					UpdateScreenShakeVibration(num5);
				}
				t += m_deltaTime * CurrentStickyFriction;
			}
			yield return null;
		}
		screenShakeAmount -= lastScreenShakeAmount;
	}

	private Vector2 GetBoundedCameraPositionInRect(Rect rect, Vector2 focalPos, ref Vector2 aimOffset)
	{
		Vector2 result = focalPos;
		Vector2 vector = m_camera.ViewportToWorldPoint(Vector2.zero);
		Vector2 vector2 = m_camera.ViewportToWorldPoint(Vector2.one);
		Rect rect2 = new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
		rect2.center = focalPos;
		float num = controllerCamera.VisibleBorder / controllerCamera.ModifiedAimContribution;
		if (rect2.width > rect.width)
		{
			float num2 = Mathf.Max(1f, controllerCamera.VisibleBorder - (rect2.width - rect.width) / 2f);
			if (rect2.center.x < rect.center.x)
			{
				float num3 = (rect.center.x - rect2.center.x) / (rect.width / 2f);
				result.x = rect.center.x - controllerCamera.BorderBumperCurve.Evaluate(num3) * num2;
				aimOffset.x = (1f - num3) * aimOffset.x + num3 * aimOffset.x * num;
			}
			else if (rect2.center.x > rect.center.x)
			{
				float num4 = (rect2.center.x - rect.center.x) / (rect.width / 2f);
				result.x = rect.center.x + controllerCamera.BorderBumperCurve.Evaluate(num4) * num2;
				aimOffset.x = (1f - num4) * aimOffset.x + num4 * aimOffset.x * num;
			}
		}
		else if (rect2.xMin < rect.xMin)
		{
			float num5 = (rect.xMin - rect2.xMin) / (rect2.width / 2f);
			result.x = rect.xMin - controllerCamera.BorderBumperCurve.Evaluate(num5) * controllerCamera.VisibleBorder + rect2.width / 2f;
			aimOffset.x = (1f - num5) * aimOffset.x + num5 * aimOffset.x * num;
		}
		else if (rect2.xMax > rect.xMax)
		{
			float num6 = (rect2.xMax - rect.xMax) / (rect2.width / 2f);
			result.x = rect.xMax + controllerCamera.BorderBumperCurve.Evaluate(num6) * controllerCamera.VisibleBorder - rect2.width / 2f;
			aimOffset.x = (1f - num6) * aimOffset.x + num6 * aimOffset.x * num;
		}
		if (rect2.height > rect.height)
		{
			float num7 = Mathf.Max(1f, controllerCamera.VisibleBorder - (rect2.height - rect.height) / 2f);
			if (rect2.center.y < rect.center.y)
			{
				float num8 = (rect.center.y - rect2.center.y) / (rect.height / 2f);
				result.y = rect.center.y - controllerCamera.BorderBumperCurve.Evaluate(num8) * num7;
				aimOffset.y = (1f - num8) * aimOffset.y + num8 * aimOffset.y * num;
			}
			else if (rect2.center.y > rect.center.y)
			{
				float num9 = (rect2.center.y - rect.center.y) / (rect.height / 2f);
				result.y = rect.center.y + controllerCamera.BorderBumperCurve.Evaluate(num9) * num7;
				aimOffset.y = (1f - num9) * aimOffset.y + num9 * aimOffset.y * num;
			}
		}
		else if (rect2.yMin < rect.yMin)
		{
			float num10 = (rect.yMin - rect2.yMin) / (rect2.height / 2f);
			result.y = rect.yMin - controllerCamera.BorderBumperCurve.Evaluate(num10) * controllerCamera.VisibleBorder + rect2.height / 2f;
			aimOffset.y = (1f - num10) * aimOffset.y + num10 * aimOffset.y * num;
		}
		else if (rect2.yMax > rect.yMax)
		{
			float num11 = (rect2.yMax - rect.yMax) / (rect2.height / 2f);
			result.y = rect.yMax + controllerCamera.BorderBumperCurve.Evaluate(num11) * controllerCamera.VisibleBorder - rect2.height / 2f;
			aimOffset.y = (1f - num11) * aimOffset.y + num11 * aimOffset.y * num;
		}
		return result;
	}

	public static Vector2 CameraToWorld(float x, float y)
	{
		return new Vector2(Mathf.Lerp(m_cachedCameraMin.x, m_cachedCameraMax.x, x), Mathf.Lerp(m_cachedCameraMin.y, m_cachedCameraMax.y, y));
	}

	public static Vector2 CameraToWorld(Vector2 point)
	{
		return new Vector2(Mathf.Lerp(m_cachedCameraMin.x, m_cachedCameraMax.x, point.x), Mathf.Lerp(m_cachedCameraMin.y, m_cachedCameraMax.y, point.y));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
