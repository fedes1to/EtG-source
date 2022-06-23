using UnityEngine;

public class Encircler : MonoBehaviour
{
	private MeshFilter m_filter;

	private Renderer m_renderer;

	private AIActor m_actor;

	private int m_centerUVID;

	private void Start()
	{
		m_centerUVID = Shader.PropertyToID("_CenterUV");
		m_filter = GetComponent<MeshFilter>();
		m_renderer = GetComponent<Renderer>();
		m_actor = GetComponent<AIActor>();
		if ((bool)m_actor && (bool)m_actor.sprite)
		{
			SpriteOutlineManager.ToggleOutlineRenderers(m_actor.sprite, false);
		}
	}

	private void LateUpdate()
	{
		Vector4 zero = Vector4.zero;
		Mesh sharedMesh = m_filter.sharedMesh;
		Vector2 rhs = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 rhs2 = new Vector2(float.MinValue, float.MinValue);
		for (int i = 0; i < sharedMesh.uv.Length; i++)
		{
			rhs = Vector2.Min(sharedMesh.uv[i], rhs);
			rhs2 = Vector2.Max(sharedMesh.uv[i], rhs2);
			zero += new Vector4(sharedMesh.uv[i].x, sharedMesh.uv[i].y, 0f, 0f);
		}
		zero /= (float)sharedMesh.uv.Length;
		zero.z = Mathf.Min(rhs2.x - rhs.x, rhs2.y - rhs.y);
		zero.w = (float)m_renderer.sharedMaterial.mainTexture.width / (float)m_renderer.sharedMaterial.mainTexture.height;
		m_renderer.material.SetVector(m_centerUVID, zero);
	}

	private void OnDestroy()
	{
	}
}
