using UnityEngine;

public class CutsceneMotion
{
	public Transform transform;

	public CameraController camera;

	public Vector2 lerpStart;

	public Vector2? lerpEnd;

	public float lerpProgress;

	public float speed;

	public float zOffset;

	public bool isSmoothStepped = true;

	public CutsceneMotion(Transform t, Vector2? targetPosition, float s, float z = 0f)
	{
		transform = t;
		lerpStart = t.position.XY();
		lerpEnd = targetPosition;
		lerpProgress = 0f;
		speed = s;
		zOffset = z;
	}
}
