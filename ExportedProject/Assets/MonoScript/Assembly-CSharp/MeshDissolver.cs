using System.Collections;
using UnityEngine;

public class MeshDissolver : MonoBehaviour
{
	public void DissolveMesh(Vector2 startPosition, float duration)
	{
		StartCoroutine(Dissolve(startPosition, duration));
	}

	private IEnumerator Dissolve(Vector2 startPosition, float duration)
	{
		Vector2 adjTestPosition = startPosition - base.transform.position.XY();
		MeshFilter mf = GetComponent<MeshFilter>();
		MeshRenderer mr = GetComponent<MeshRenderer>();
		for (int j = 0; j < mr.materials.Length; j++)
		{
			mr.materials[j].shader = Shader.Find("tk2d/CutoutVertexColor");
		}
		Mesh i = mf.mesh;
		float maxDistance = float.MinValue;
		float[] distances = new float[i.vertexCount];
		for (int k = 0; k < i.vertexCount; k++)
		{
			maxDistance = Mathf.Max(distances[k] = Vector2.Distance(adjTestPosition, i.vertices[k].XY()), maxDistance);
		}
		float elapsed = 0f;
		Color32[] colors = i.colors32;
		if (colors.Length != i.vertexCount)
		{
			colors = new Color32[i.vertexCount];
		}
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			for (int l = 0; l < i.vertexCount; l++)
			{
				float num = distances[l] / maxDistance;
				float num2 = Mathf.Lerp(1f, 0f, t / num);
				colors[l] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(num2 * 255f));
			}
			i.colors32 = colors;
			yield return null;
		}
		yield return null;
	}
}
