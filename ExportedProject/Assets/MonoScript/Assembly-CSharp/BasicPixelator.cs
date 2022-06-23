using UnityEngine;
using UnityEngine.Rendering;

public class BasicPixelator : MonoBehaviour
{
	private Vector3 FINAL_CAMERA_POSITION_OFFSET;

	private Camera m_camera;

	private void CheckSize()
	{
		if (m_camera == null)
		{
			m_camera = GetComponent<Camera>();
		}
		BraveCameraUtility.MaintainCameraAspect(m_camera);
		m_camera.orthographicSize = 8.4375f;
	}

	private void OnEnable()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11)
		{
			FINAL_CAMERA_POSITION_OFFSET = Vector3.zero;
		}
		else
		{
			FINAL_CAMERA_POSITION_OFFSET = new Vector3(1f / 32f, 1f / 32f, 0f);
		}
		base.transform.position += FINAL_CAMERA_POSITION_OFFSET;
	}

	private void OnDisable()
	{
		base.transform.position -= FINAL_CAMERA_POSITION_OFFSET;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture target)
	{
		CheckSize();
		RenderTexture temporary = RenderTexture.GetTemporary(BraveCameraUtility.H_PIXELS, BraveCameraUtility.V_PIXELS, 0, RenderTextureFormat.Default);
		if (!temporary.IsCreated())
		{
			temporary.Create();
		}
		Graphics.Blit(Pixelator.SmallBlackTexture, temporary);
		source.filterMode = FilterMode.Point;
		temporary.filterMode = FilterMode.Point;
		Graphics.Blit(source, temporary);
		Graphics.Blit(temporary, target);
		RenderTexture.ReleaseTemporary(temporary);
	}
}
