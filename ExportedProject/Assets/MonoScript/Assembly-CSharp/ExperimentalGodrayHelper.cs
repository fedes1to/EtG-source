using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class ExperimentalGodrayHelper : MonoBehaviour
{
	private IEnumerator Start()
	{
		Bounds bs = GetComponent<tk2dBaseSprite>().GetBounds();
		GetComponent<Renderer>().sharedMaterial.SetVector("_MeshBoundsCenter", bs.center);
		GetComponent<Renderer>().sharedMaterial.SetVector("_MeshBoundsExtents", bs.extents);
		yield return null;
		if (Application.isPlaying)
		{
			MeshFilter component = GetComponent<MeshFilter>();
			Bounds bounds = component.mesh.bounds;
			bounds.Expand(new Vector3(0f, bounds.extents.y * 12f, 0f));
			component.mesh.bounds = bounds;
		}
	}
}
