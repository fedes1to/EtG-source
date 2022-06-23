using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTimesNebulaController : MonoBehaviour
{
	public Camera NebulaCamera;

	public SlicedVolume NebulaClouds;

	public float CloudParallaxFactor = 0.5f;

	public Transform BGQuad;

	private bool m_isActive;

	private Material m_nebulaMaterial;

	private RenderTexture m_partiallyActiveRenderTarget;

	private Material m_portalMaterial;

	public List<Renderer> NebulaRegisteredVisuals = new List<Renderer>();

	private int m_playerPosID = -1;

	private IEnumerator Start()
	{
		yield return null;
		BecomePartiallyActive();
		yield return new WaitForSeconds(0.5f);
		NebulaCamera.enabled = false;
	}

	public void BecomePartiallyActive()
	{
		m_partiallyActiveRenderTarget = RenderTexture.GetTemporary(Pixelator.Instance.CurrentMacroResolutionX, Pixelator.Instance.CurrentMacroResolutionY, 0, RenderTextureFormat.Default);
		NebulaCamera.enabled = true;
		NebulaCamera.targetTexture = m_partiallyActiveRenderTarget;
		m_portalMaterial = BraveResources.Load("Shaders/DarkPortalMaterial", ".mat") as Material;
		if ((bool)m_portalMaterial)
		{
			m_portalMaterial.SetTexture("_PortalTex", m_partiallyActiveRenderTarget);
		}
		Shader.SetGlobalTexture("_EndTimesVortex", m_partiallyActiveRenderTarget);
		if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			NebulaClouds.generateNewSlices = true;
			m_nebulaMaterial = NebulaClouds.cloudMaterial;
		}
		else
		{
			Object.Destroy(NebulaClouds.gameObject);
			m_nebulaMaterial = null;
		}
	}

	private void ClearRT()
	{
		if (m_partiallyActiveRenderTarget != null)
		{
			RenderTexture.ReleaseTemporary(m_partiallyActiveRenderTarget);
			m_partiallyActiveRenderTarget = null;
		}
	}

	public void BecomeActive()
	{
		m_isActive = true;
		NebulaCamera.enabled = true;
		ClearRT();
		Pixelator.Instance.AdditionalBGCamera = NebulaCamera;
	}

	private void Update()
	{
		if (!m_isActive && !GameManager.Instance.IsLoadingLevel)
		{
			if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.LOW || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW)
			{
				NebulaCamera.enabled = false;
			}
			else if (!NebulaCamera.enabled)
			{
				if (GameManager.Instance.AllPlayers != null)
				{
					for (int i = 0; i < NebulaRegisteredVisuals.Count; i++)
					{
						if (NebulaRegisteredVisuals[i].isVisible)
						{
							NebulaCamera.enabled = true;
						}
					}
				}
			}
			else if (NebulaCamera.enabled && GameManager.Instance.AllPlayers != null)
			{
				bool flag = false;
				for (int j = 0; j < NebulaRegisteredVisuals.Count; j++)
				{
					if (NebulaRegisteredVisuals[j].isVisible)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					NebulaCamera.enabled = false;
				}
			}
		}
		if (m_isActive && m_nebulaMaterial != null)
		{
			float y = GameManager.Instance.MainCameraController.transform.position.y;
			m_nebulaMaterial.SetFloat("_ZOffset", y * CloudParallaxFactor);
		}
		if (m_isActive && (bool)BGQuad)
		{
			float aSPECT = BraveCameraUtility.ASPECT;
			float num = aSPECT / 1.77777779f;
			if (num > 1f)
			{
				BGQuad.transform.localScale = new Vector3(16f * num, 9f, 1f);
			}
			else
			{
				BGQuad.transform.localScale = new Vector3(16f * num, 9f, 1f);
			}
		}
		if ((bool)m_portalMaterial)
		{
			if (m_playerPosID == -1)
			{
				m_playerPosID = Shader.PropertyToID("_PlayerPos");
			}
			Vector2 centerPosition = GameManager.Instance.PrimaryPlayer.CenterPosition;
			Vector2 vector = ((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? Vector2.zero : GameManager.Instance.SecondaryPlayer.CenterPosition);
			m_portalMaterial.SetVector(m_playerPosID, new Vector4(centerPosition.x, centerPosition.y, vector.x, vector.y));
		}
	}

	private void OnDestroy()
	{
		ClearRT();
	}

	public void BecomeInactive(bool destroy = true)
	{
		m_isActive = false;
		if (Pixelator.HasInstance && Pixelator.Instance.AdditionalBGCamera == NebulaCamera)
		{
			Pixelator.Instance.AdditionalBGCamera = null;
		}
		if (destroy)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
