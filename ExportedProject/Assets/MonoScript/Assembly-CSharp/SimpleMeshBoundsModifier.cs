using UnityEngine;

public class SimpleMeshBoundsModifier : MonoBehaviour
{
	public Vector3 expansionVector = new Vector3(0f, 20f, 0f);

	private void Start()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		Bounds bounds = component.sharedMesh.bounds;
		bounds.Expand(expansionVector);
		component.mesh.bounds = bounds;
	}
}
