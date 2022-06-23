using UnityEngine;

public class ReticleRiserEffect : MonoBehaviour
{
	public int NumRisers = 4;

	public float RiserHeight = 1f;

	public float RiseTime = 1.5f;

	private tk2dSlicedSprite m_sprite;

	private tk2dSlicedSprite[] m_risers;

	private Shader m_shader;

	private float m_localElapsed;

	private void Start()
	{
		m_sprite = GetComponent<tk2dSlicedSprite>();
		m_sprite.usesOverrideMaterial = true;
		m_shader = ShaderCache.Acquire("tk2d/BlendVertexColorUnlitTilted");
		m_sprite.renderer.material.shader = m_shader;
		GameObject gameObject = Object.Instantiate(base.gameObject);
		Object.Destroy(gameObject.GetComponent<ReticleRiserEffect>());
		m_risers = new tk2dSlicedSprite[NumRisers];
		m_risers[0] = gameObject.GetComponent<tk2dSlicedSprite>();
		for (int i = 0; i < NumRisers - 1; i++)
		{
			m_risers[i + 1] = Object.Instantiate(gameObject).GetComponent<tk2dSlicedSprite>();
		}
		OnSpawned();
	}

	private void OnSpawned()
	{
		m_localElapsed = 0f;
		if (m_risers != null)
		{
			for (int i = 0; i < m_risers.Length; i++)
			{
				m_risers[i].transform.parent = base.transform;
				m_risers[i].transform.localPosition = Vector3.zero;
				m_risers[i].transform.localRotation = Quaternion.identity;
				m_risers[i].dimensions = m_sprite.dimensions;
				m_risers[i].usesOverrideMaterial = true;
				m_risers[i].renderer.material.shader = m_shader;
				m_risers[i].gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Critical"));
			}
		}
	}

	private void Update()
	{
		if (!m_sprite)
		{
			return;
		}
		m_localElapsed += BraveTime.DeltaTime;
		m_sprite.ForceRotationRebuild();
		m_sprite.UpdateZDepth();
		m_sprite.renderer.material.shader = m_shader;
		if (m_risers != null)
		{
			for (int i = 0; i < m_risers.Length; i++)
			{
				float num = 0f;
				float num2 = Mathf.Max(0f, m_localElapsed - RiseTime / (float)NumRisers * (float)i);
				float t = num2 % RiseTime / RiseTime;
				m_risers[i].color = Color.Lerp(new Color(1f, 1f, 1f, 0.75f), new Color(1f, 1f, 1f, 0f), t);
				num = Mathf.Lerp(0f, RiserHeight, t);
				m_risers[i].transform.localPosition = Vector3.zero;
				m_risers[i].transform.position += Vector3.zero.WithY(num);
				m_risers[i].ForceRotationRebuild();
				m_risers[i].UpdateZDepth();
				m_risers[i].renderer.material.shader = m_shader;
			}
		}
	}
}
