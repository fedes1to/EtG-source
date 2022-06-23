using UnityEngine;

[ExecuteInEditMode]
public class QuadUVSetter : MonoBehaviour
{
	public Vector2 uv0;

	public Vector2 uv1;

	public Vector2 uv2;

	public Vector2 uv3;

	private void OnEnable()
	{
		Mesh sharedMesh = GetComponent<MeshFilter>().sharedMesh;
		Vector2[] uv = sharedMesh.uv;
		uv[0] = uv0;
		uv[1] = uv1;
		uv[2] = uv2;
		uv[3] = uv3;
		sharedMesh.uv = uv;
		sharedMesh.uv2 = uv;
	}
}
