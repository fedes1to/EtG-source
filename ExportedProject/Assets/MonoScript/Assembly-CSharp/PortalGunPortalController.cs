using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PortalGunPortalController : BraveBehaviour
{
	public bool IsAlternatePortal;

	public Vector2 PortalNormal;

	public PortalGunPortalController pairedPortal;

	public Camera FacewallCamera;

	public MeshRenderer PortalRenderer;

	public Texture2D FacewallMaskTexture;

	public int PixelWidth = 16;

	public int PixelHeight = 32;

	private RenderTexture m_renderTarget;

	private bool m_doRender;

	private int cm_bg;

	private int cm_fg;

	private static int m_portalNumber;

	private void Awake()
	{
		cm_bg = (1 << LayerMask.NameToLayer("BG_Nonsense")) | (1 << LayerMask.NameToLayer("BG_Critical"));
		cm_fg = (1 << LayerMask.NameToLayer("FG_Nonsense")) | (1 << LayerMask.NameToLayer("ShadowCaster")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("FG_Reflection")) | (1 << LayerMask.NameToLayer("FG_Critical"));
		if (m_renderTarget == null && (bool)FacewallCamera)
		{
			FacewallCamera.orthographicSize = 1f;
			FacewallCamera.opaqueSortMode = OpaqueSortMode.FrontToBack;
			FacewallCamera.transparencySortMode = TransparencySortMode.Orthographic;
			FacewallCamera.enabled = false;
			m_renderTarget = RenderTexture.GetTemporary(PixelWidth, PixelHeight, 24, RenderTextureFormat.Default);
			m_renderTarget.filterMode = FilterMode.Point;
			FacewallCamera.targetTexture = m_renderTarget;
			Material material = UnityEngine.Object.Instantiate(PortalRenderer.material);
			material.shader = Shader.Find("Brave/Effects/CutoutPortalInternalTilted");
			material.SetTexture("_MainTex", m_renderTarget);
			material.SetTexture("_MaskTex", FacewallMaskTexture);
			PortalRenderer.material = material;
		}
		m_portalNumber++;
	}

	private void LateUpdate()
	{
		if (m_doRender)
		{
			FacewallCamera.clearFlags = CameraClearFlags.Color;
			FacewallCamera.backgroundColor = Color.black;
			FacewallCamera.cullingMask = cm_bg;
			FacewallCamera.Render();
			FacewallCamera.clearFlags = CameraClearFlags.Depth;
			FacewallCamera.backgroundColor = Color.clear;
			FacewallCamera.cullingMask = cm_fg;
			FacewallCamera.Render();
		}
	}

	private IEnumerator Start()
	{
		base.transform.position += new Vector3(0f, -0.125f, 0f);
		if ((bool)FacewallCamera)
		{
			base.transform.position += new Vector3(0f, 0.5f, 0f);
			base.sprite.HeightOffGround += 0.75f;
		}
		base.specRigidbody.Reinitialize();
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerCollision));
		for (int i = 0; i < StaticReferenceManager.AllPortals.Count; i++)
		{
			PortalGunPortalController portalGunPortalController = StaticReferenceManager.AllPortals[i];
			if (portalGunPortalController.IsAlternatePortal != IsAlternatePortal)
			{
				portalGunPortalController.BecomeLinkedTo(this);
				BecomeLinkedTo(portalGunPortalController);
				break;
			}
		}
		for (int num = StaticReferenceManager.AllPortals.Count - 1; num >= 0; num--)
		{
			if (StaticReferenceManager.AllPortals[num] != this && StaticReferenceManager.AllPortals[num] != pairedPortal)
			{
				UnityEngine.Object.Destroy(StaticReferenceManager.AllPortals[num].gameObject);
			}
		}
		if (pairedPortal != null)
		{
			pairedPortal.BecomeLinkedTo(this);
		}
		StaticReferenceManager.AllPortals.Add(this);
		yield return null;
		if (base.sprite.FlipY)
		{
			PortalNormal = new Vector2(-1f, 0f);
			base.specRigidbody.Reinitialize();
		}
		if (base.transform.rotation != Quaternion.identity && PortalNormal.y < 0f)
		{
			PortalNormal = new Vector2(0f, 1f);
		}
	}

	private void BecomeLinkedTo(PortalGunPortalController otherPortal)
	{
		pairedPortal = otherPortal;
		if ((bool)FacewallCamera)
		{
			m_doRender = true;
			FacewallCamera.transform.position = otherPortal.transform.position + new Vector3(otherPortal.PortalNormal.x, otherPortal.PortalNormal.y * 2f + -0.375f, -10f) + CameraController.PLATFORM_CAMERA_OFFSET;
			PortalRenderer.enabled = true;
		}
	}

	private void BecomeUnlinked()
	{
		pairedPortal = null;
		m_doRender = false;
		if ((bool)FacewallCamera)
		{
			PortalRenderer.enabled = false;
		}
	}

	private void HandleTriggerCollision(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody myRigidbody, CollisionData collisionData)
	{
		if (!pairedPortal)
		{
			return;
		}
		if ((bool)otherRigidbody.projectile)
		{
			float z = Mathf.DeltaAngle(BraveMathCollege.Atan2Degrees(-PortalNormal), BraveMathCollege.Atan2Degrees(pairedPortal.PortalNormal));
			Vector2 unitCenter = pairedPortal.specRigidbody.UnitCenter;
			if (pairedPortal.PortalNormal.x != 0f)
			{
				unitCenter += pairedPortal.PortalNormal.normalized * 0.5f;
			}
			else
			{
				unitCenter += pairedPortal.PortalNormal.normalized;
			}
			otherRigidbody.transform.position = unitCenter.ToVector3ZisY();
			otherRigidbody.Reinitialize();
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(otherRigidbody);
			otherRigidbody.RegisterGhostCollisionException(pairedPortal.specRigidbody);
			otherRigidbody.RegisterGhostCollisionException(base.specRigidbody);
			otherRigidbody.projectile.SendInDirection((Quaternion.Euler(0f, 0f, z) * otherRigidbody.projectile.Direction).XY(), false);
			PhysicsEngine.SkipCollision = true;
			otherRigidbody.projectile.LastPosition = otherRigidbody.transform.position;
		}
		else if ((bool)otherRigidbody.gameActor)
		{
			Vector2 vector = otherRigidbody.gameActor.transform.position.XY() - otherRigidbody.gameActor.specRigidbody.UnitCenter;
			vector += pairedPortal.PortalNormal;
			if (pairedPortal.PortalNormal.y < 0f)
			{
				vector += pairedPortal.PortalNormal * 2f;
			}
			if (otherRigidbody.gameActor is PlayerController)
			{
				PlayerController playerController = otherRigidbody.gameActor as PlayerController;
				playerController.WarpToPoint(pairedPortal.specRigidbody.UnitCenter + vector);
				playerController.specRigidbody.RecheckTriggers = false;
			}
			else if (otherRigidbody.gameActor is AIActor)
			{
				AIActor aIActor = otherRigidbody.gameActor as AIActor;
				aIActor.transform.position = pairedPortal.specRigidbody.UnitCenter + vector;
				otherRigidbody.Reinitialize();
			}
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(otherRigidbody, null, true);
			otherRigidbody.RegisterTemporaryCollisionException(pairedPortal.specRigidbody, 0.5f);
			otherRigidbody.RegisterTemporaryCollisionException(base.specRigidbody, 0.5f);
			if ((bool)otherRigidbody.knockbackDoer)
			{
				otherRigidbody.knockbackDoer.ApplyKnockback(pairedPortal.PortalNormal, 10f);
			}
		}
	}

	protected override void OnDestroy()
	{
		if (pairedPortal != null)
		{
			if (pairedPortal.pairedPortal == this)
			{
				pairedPortal.BecomeUnlinked();
			}
			BecomeUnlinked();
		}
		StaticReferenceManager.AllPortals.Remove(this);
		if (m_renderTarget != null)
		{
			RenderTexture.ReleaseTemporary(m_renderTarget);
		}
		base.OnDestroy();
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		throw new NotImplementedException();
	}
}
