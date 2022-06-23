using System;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Daikon Forge/User Interface/GUI Camera")]
public class dfGUICamera : MonoBehaviour
{
	public Vector3 cameraPositionOffset;

	public bool MaintainCameraAspect = true;

	public bool ForceToSixteenNine;

	public bool ForceNoHalfPixelOffset;

	private Camera m_camera;

	public void Awake()
	{
		if (Application.isPlaying && MaintainCameraAspect)
		{
			if (ForceToSixteenNine)
			{
				BraveCameraUtility.MaintainCameraAspectForceAspect(GetComponent<Camera>(), 1.77777779f);
			}
			else
			{
				BraveCameraUtility.MaintainCameraAspect(GetComponent<Camera>());
			}
			base.transform.parent.GetComponent<dfGUIManager>().ResolutionChanged();
		}
	}

	public void OnEnable()
	{
	}

	public void Start()
	{
		m_camera = GetComponent<Camera>();
		m_camera.transparencySortMode = TransparencySortMode.Orthographic;
		m_camera.useOcclusionCulling = false;
		m_camera.eventMask &= ~GetComponent<Camera>().cullingMask;
	}

	private void Update()
	{
		if (Application.isPlaying && MaintainCameraAspect)
		{
			if (ForceToSixteenNine)
			{
				BraveCameraUtility.MaintainCameraAspectForceAspect(m_camera, 1.77777779f);
			}
			else
			{
				BraveCameraUtility.MaintainCameraAspect(m_camera);
			}
		}
	}
}
