using System.Collections.Generic;
using UnityEngine;

public class SpriteShadow
{
	public bool hasChanged;

	public List<Vector3> vertices;

	public tk2dSprite shadowedSprite;

	private SpriteShadowCaster m_caster;

	private Transform m_casterTransform;

	private tk2dSpriteAnimator spriteAnimator;

	private Transform spriteTransform;

	private Vector3 cachedPosition;

	private tk2dSpriteAnimationClip cachedClip;

	private int cachedFrame;

	private float shadowDepth;

	private GameObject m_shadowObject;

	private MeshFilter m_shadowFilter;

	private MeshRenderer m_shadowRenderer;

	private Mesh m_shadowMesh;

	private Mesh m_spriteMesh;

	public SpriteShadow(tk2dSprite sprite, SpriteShadowCaster caster)
	{
		shadowedSprite = sprite;
		m_caster = caster;
		m_casterTransform = caster.transform;
		shadowDepth = caster.shadowDepth;
		vertices = new List<Vector3>();
		m_spriteMesh = sprite.GetComponent<MeshFilter>().sharedMesh;
		spriteTransform = sprite.transform;
		spriteAnimator = sprite.GetComponent<tk2dSpriteAnimator>();
		cachedPosition = spriteTransform.position;
		if (spriteAnimator != null && spriteAnimator.CurrentClip != null)
		{
			cachedClip = spriteAnimator.CurrentClip;
			cachedFrame = spriteAnimator.CurrentFrame;
		}
		m_shadowObject = new GameObject("Shadow");
		m_shadowFilter = m_shadowObject.AddComponent<MeshFilter>();
		m_shadowRenderer = m_shadowObject.AddComponent<MeshRenderer>();
		m_shadowRenderer.sharedMaterial = m_caster.GetMaterialInstance();
		Texture mainTexture = shadowedSprite.GetComponent<MeshRenderer>().sharedMaterial.mainTexture;
		m_shadowRenderer.sharedMaterial.mainTexture = mainTexture;
		m_shadowMesh = new Mesh();
		m_shadowMesh.vertices = new Vector3[10];
		m_shadowMesh.triangles = new int[24]
		{
			0, 3, 1, 2, 3, 0, 2, 5, 3, 4,
			5, 2, 4, 7, 5, 6, 7, 4, 6, 9,
			7, 8, 9, 6
		};
		m_shadowMesh.uv = new Vector2[10];
		UpdateShadow(true);
	}

	public void Destroy()
	{
		Object.Destroy(m_shadowObject);
	}

	private Vector3 CollapseDepth(Vector3 input)
	{
		return new Vector3(input.x, input.y, shadowDepth);
	}

	public void UpdateShadow(bool force = false)
	{
		if (!force)
		{
			bool flag = cachedPosition != spriteTransform.position;
			bool flag2 = false;
			if (spriteAnimator != null && spriteAnimator.CurrentClip != null)
			{
				if (cachedClip != spriteAnimator.CurrentClip)
				{
					flag2 = true;
				}
				if (cachedFrame != spriteAnimator.CurrentFrame)
				{
					flag2 = true;
				}
			}
			if (!flag && !flag2)
			{
				return;
			}
		}
		float x = shadowedSprite.GetBounds().size.x;
		float y = shadowedSprite.GetBounds().size.y;
		Vector3 vector = CollapseDepth(m_casterTransform.position);
		Vector3 vector2 = CollapseDepth(spriteTransform.position);
		Vector3 vector3 = vector2 + new Vector3(x / 2f, 0f, 0f);
		float magnitude = (vector3 - vector).magnitude;
		Vector3 vector4 = vector2;
		Vector3 direction = vector4 - vector;
		Vector3 vector5 = vector2 + new Vector3(x, 0f, 0f);
		Vector3 direction2 = vector5 - vector;
		Ray ray = new Ray(vector, direction);
		Ray ray2 = new Ray(vector, direction2);
		Vector3 point = ray.GetPoint(magnitude);
		Vector3 point2 = ray2.GetPoint(magnitude);
		Vector3 point3 = ray.GetPoint(magnitude + y * (magnitude / m_caster.radius * 4f));
		Vector3 point4 = ray2.GetPoint(magnitude + y * (magnitude / m_caster.radius * 4f));
		vertices.Clear();
		vertices.Add(point);
		vertices.Add(point2);
		vertices.Add(point + (point3 - point) / 4f);
		vertices.Add(point2 + (point4 - point2) / 4f);
		vertices.Add((point + point3) / 2f);
		vertices.Add((point2 + point4) / 2f);
		vertices.Add(point * 0.25f + point3 * 0.75f);
		vertices.Add(point2 * 0.25f + point4 * 0.75f);
		vertices.Add(point3);
		vertices.Add(point4);
		hasChanged = true;
		if (vector.y > vector2.y)
		{
			m_shadowMesh.triangles = new int[24]
			{
				0, 1, 3, 2, 0, 3, 2, 3, 5, 4,
				2, 5, 4, 5, 7, 6, 4, 7, 6, 7,
				9, 8, 6, 9
			};
		}
		else
		{
			m_shadowMesh.triangles = new int[24]
			{
				0, 3, 1, 2, 3, 0, 2, 5, 3, 4,
				5, 2, 4, 7, 5, 6, 7, 4, 6, 9,
				7, 8, 9, 6
			};
		}
		RebuildMesh();
		cachedPosition = spriteTransform.position;
		if (spriteAnimator != null && spriteAnimator.CurrentClip != null)
		{
			cachedClip = spriteAnimator.CurrentClip;
			cachedFrame = spriteAnimator.CurrentFrame;
		}
	}

	private void RebuildMesh()
	{
		m_shadowMesh.vertices = vertices.ToArray();
		Vector2[] uv = m_spriteMesh.uv;
		Vector2[] array = new Vector2[10];
		array[0] = uv[0];
		array[1] = uv[1];
		array[4] = (uv[0] + uv[2]) / 2f;
		array[5] = (uv[1] + uv[3]) / 2f;
		array[2] = (uv[0] + array[4]) / 2f;
		array[3] = (uv[1] + array[5]) / 2f;
		array[6] = uv[0] + (uv[2] - uv[0]) * 0.75f;
		array[7] = uv[1] + (uv[3] - uv[1]) * 0.75f;
		array[8] = uv[2];
		array[9] = uv[3];
		m_shadowMesh.uv = array;
		m_shadowMesh.RecalculateBounds();
		m_shadowFilter.sharedMesh = m_shadowMesh;
	}
}
