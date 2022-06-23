using System;
using System.Collections.Generic;
using UnityEngine;

public class BossFinalRogueController : BraveBehaviour
{
	public GameObject cameraPoint;

	public List<BossFinalRogueGunController> BaseGuns;

	public float minPlayerDist = -10f;

	public float maxPlayerDist = 14f;

	public float playerDistOffset = 7f;

	public Vector2 worldCenter;

	public float worldRadius;

	[Header("Background Scrolling")]
	public float minScrollDist = 8f;

	public float maxScrollDist = 20f;

	public float scrollMultiplier = 0.05f;

	private float? m_cameraX;

	private IntVector2 m_cachedCameraLowerLeftPixels;

	private IntVector2 m_cachedCameraUpperRightPixels;

	private PilotPastController m_pastController;

	private CameraController m_camera;

	private bool m_lockCamera;

	private bool m_suppressBaseGuns;

	public bool SuppressBaseGuns
	{
		get
		{
			return m_suppressBaseGuns;
		}
		set
		{
			if (m_suppressBaseGuns != value)
			{
				for (int i = 0; i < BaseGuns.Count; i++)
				{
					BaseGuns[i].fireType = ((!value) ? BossFinalRogueGunController.FireType.Timed : BossFinalRogueGunController.FireType.Triggered);
				}
			}
			m_suppressBaseGuns = value;
		}
	}

	public Vector2 CameraPos
	{
		get
		{
			CameraController cameraController = m_camera ?? GameManager.Instance.MainCameraController;
			float value = base.specRigidbody.HitboxPixelCollider.UnitBottom - GameManager.Instance.PrimaryPlayer.specRigidbody.UnitBottom;
			float t = Mathf.InverseLerp(minPlayerDist, maxPlayerDist, value);
			float num = Mathf.SmoothStep(playerDistOffset, 0f, t);
			Vector2 result = cameraPoint.transform.position.XY() + new Vector2(0f, 0f - cameraController.Camera.orthographicSize + num);
			if (m_cameraX.HasValue)
			{
				result.x = m_cameraX.Value;
			}
			return result;
		}
	}

	public void Start()
	{
		PhysicsEngine.Instance.OnPostRigidbodyMovement += PostRigidbodyMovement;
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("BG_Critical"));
		m_pastController = UnityEngine.Object.FindObjectOfType<PilotPastController>();
	}

	public void Update()
	{
		if ((bool)m_camera && m_cameraX.HasValue)
		{
			float x = m_camera.GetCoreCurrentBasePosition().x;
			float num = Mathf.InverseLerp(minScrollDist, maxScrollDist, Mathf.Abs(x - m_cameraX.Value));
			m_pastController.BackgroundScrollSpeed.x = Mathf.Sign(x - m_cameraX.Value) * num * scrollMultiplier;
		}
		m_cachedCameraLowerLeftPixels = PhysicsEngine.UnitToPixel(BraveUtility.ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay));
		m_cachedCameraUpperRightPixels = PhysicsEngine.UnitToPixel(BraveUtility.ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay));
	}

	protected override void OnDestroy()
	{
		if (PhysicsEngine.HasInstance)
		{
			PhysicsEngine.Instance.OnPostRigidbodyMovement -= PostRigidbodyMovement;
		}
		base.OnDestroy();
	}

	public void InitCamera()
	{
		if (!m_camera)
		{
			m_camera = GameManager.Instance.MainCameraController;
			m_lockCamera = true;
			m_camera.SetManualControl(true);
			m_camera.OverridePosition = CameraPos;
			m_cameraX = m_camera.OverridePosition.x;
			SpeculativeRigidbody speculativeRigidbody = GameManager.Instance.PrimaryPlayer.specRigidbody;
			speculativeRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(CameraBoundsMovementRestrictor));
		}
	}

	public void EndCameraLock()
	{
		m_lockCamera = false;
	}

	private void PostRigidbodyMovement()
	{
		if (m_lockCamera)
		{
			m_camera.OverridePosition = CameraPos;
		}
	}

	private void CameraBoundsMovementRestrictor(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (validLocation)
		{
			if (specRigidbody.PixelColliders[0].LowerLeft.x < m_cachedCameraLowerLeftPixels.x && pixelOffset.x < prevPixelOffset.x)
			{
				validLocation = false;
			}
			else if (specRigidbody.PixelColliders[0].UpperRight.x > m_cachedCameraUpperRightPixels.x && pixelOffset.x > prevPixelOffset.x)
			{
				validLocation = false;
			}
			else if (specRigidbody.PixelColliders[0].LowerLeft.y < m_cachedCameraLowerLeftPixels.y && pixelOffset.y < prevPixelOffset.y)
			{
				validLocation = false;
			}
			else if (specRigidbody.PixelColliders[1].UpperRight.y > m_cachedCameraUpperRightPixels.y && pixelOffset.y > prevPixelOffset.y)
			{
				validLocation = false;
			}
		}
	}
}
