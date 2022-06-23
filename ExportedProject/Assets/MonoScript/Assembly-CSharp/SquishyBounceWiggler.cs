using System.Collections;
using UnityEngine;

public class SquishyBounceWiggler : BraveBehaviour
{
	private bool m_wiggleHold;

	protected tk2dBaseSprite m_sprite;

	protected IntVector2 m_spriteDimensions;

	public bool WiggleHold
	{
		get
		{
			return m_wiggleHold;
		}
		set
		{
			if (value && !m_wiggleHold)
			{
				if ((bool)this)
				{
					base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Critical"));
				}
				ResetWiggle();
			}
			else if (!value && m_wiggleHold && (bool)this)
			{
				base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unpixelated"));
			}
			m_wiggleHold = value;
		}
	}

	private void Awake()
	{
		m_sprite = GetComponent<tk2dBaseSprite>();
	}

	private void Start()
	{
		if (!m_sprite)
		{
			base.enabled = false;
		}
		Bounds bounds = m_sprite.GetBounds();
		m_spriteDimensions = new IntVector2(Mathf.RoundToInt(bounds.size.x / 0.0625f), Mathf.RoundToInt(bounds.size.y / 0.0625f));
		base.transform.position = base.transform.position.Quantize(0.0625f);
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.Reinitialize();
		}
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unpixelated"));
		StartCoroutine(DoSquishyBounceWiggle());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void ResetWiggle()
	{
		if (!(m_sprite == null))
		{
			MeshFilter component = GetComponent<MeshFilter>();
			Mesh mesh = component.mesh;
			Vector3[] vertices = mesh.vertices;
			Vector2[] uv = mesh.uv;
			Vector2 zero = Vector2.zero;
			Vector2 one = Vector2.one;
			Vector3 one2 = Vector3.one;
			Vector3 zero2 = Vector3.zero;
			SetClippedGeometry(m_sprite.GetCurrentSpriteDef(), vertices, uv, zero2, 0, one2, zero, one);
			Vector3[] normals = mesh.normals;
			Color[] colors = mesh.colors;
			mesh.Clear();
			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.normals = normals;
			mesh.colors = colors;
			int[] array = new int[6];
			tk2dSpriteGeomGen.SetClippedSpriteIndices(array, 0, 0, m_sprite.GetCurrentSpriteDef());
			mesh.triangles = array;
			mesh.RecalculateBounds();
			component.mesh = mesh;
		}
	}

	private IEnumerator DoSquishyBounceWiggle()
	{
		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh sourceMesh = mf.mesh;
		Vector3[] vertices = sourceMesh.vertices;
		Vector2[] uvs = sourceMesh.uv;
		float horizontalPercentagePixel = 1f / (float)m_spriteDimensions.x;
		float verticalPercentagePixel = 1f / (float)m_spriteDimensions.y;
		int[] bottomOffsets = new int[5];
		int[] upTranslations = new int[5] { 0, -3, 1, 2, -1 };
		float[] obj = new float[5] { 1f, 1f, 0f, 1f, 1f };
		obj[2] = 1f - horizontalPercentagePixel * 2f;
		float[] horizontalScales = obj;
		float[] obj2 = new float[5] { 1f, 0f, 1f, 1f, 1f };
		obj2[1] = 1f - verticalPercentagePixel * 2f;
		float[] verticalScales = obj2;
		float[] delays = new float[5] { 0.8f, 0.1f, 0.1f, 0.1f, 0.1f };
		while (true)
		{
			for (int i = 0; i < 5; i++)
			{
				if (WiggleHold)
				{
					i = 0;
				}
				bool hasOutlines = SpriteOutlineManager.HasOutline(m_sprite);
				tk2dBaseSprite[] outlineSprites = ((!hasOutlines) ? null : SpriteOutlineManager.GetOutlineSprites<tk2dBaseSprite>(m_sprite));
				Vector2 clipBottomLeft = new Vector2(0f, (float)bottomOffsets[i] * verticalPercentagePixel);
				Vector2 clipTopRight = new Vector2(1f, 1f);
				Vector3 scale = new Vector3(horizontalScales[i], verticalScales[i], 1f);
				Vector3 translation = new Vector3(0.0625f * ((1f - horizontalScales[i]) / 2f / horizontalPercentagePixel), 0.0625f * (float)upTranslations[i], 0f);
				SetClippedGeometry(m_sprite.GetCurrentSpriteDef(), vertices, uvs, translation, 0, scale, clipBottomLeft, clipTopRight);
				Vector3[] normals = sourceMesh.normals;
				Color[] colors = sourceMesh.colors;
				sourceMesh.Clear();
				sourceMesh.vertices = vertices;
				sourceMesh.uv = uvs;
				sourceMesh.normals = normals;
				sourceMesh.colors = colors;
				int[] indices = new int[6];
				tk2dSpriteGeomGen.SetClippedSpriteIndices(indices, 0, 0, m_sprite.GetCurrentSpriteDef());
				sourceMesh.triangles = indices;
				sourceMesh.RecalculateBounds();
				mf.mesh = sourceMesh;
				if (hasOutlines)
				{
					if (outlineSprites.Length == 1)
					{
						outlineSprites[0].scale = scale;
						outlineSprites[0].transform.localPosition = Vector3.Scale(translation, scale).WithZ(outlineSprites[0].transform.localPosition.z);
						SpriteOutlineManager.HandleSpriteChanged(outlineSprites[0]);
					}
					else
					{
						for (int j = 0; j < outlineSprites.Length; j++)
						{
							outlineSprites[j].scale = scale;
							outlineSprites[j].transform.localPosition = Vector3.Scale(IntVector2.Cardinals[j].ToVector3() * 0.0625f + translation, scale).WithZ(outlineSprites[j].transform.localPosition.z);
							SpriteOutlineManager.HandleSpriteChanged(outlineSprites[j]);
						}
					}
					m_sprite.UpdateZDepth();
				}
				float targetDelay = delays[i];
				float delayElapsed = 0f;
				while (delayElapsed < targetDelay)
				{
					delayElapsed += BraveTime.DeltaTime;
					if (i != 0)
					{
						base.transform.position = base.transform.position.Quantize(0.0625f);
					}
					yield return null;
				}
				if (i != 0)
				{
					continue;
				}
				while (WiggleHold)
				{
					if (i != 0)
					{
						base.transform.position = base.transform.position.Quantize(0.0625f);
					}
					yield return null;
				}
			}
		}
	}

	private void SetClippedGeometry(tk2dSpriteDefinition spriteDef, Vector3[] pos, Vector2[] uv, Vector3 translation, int offset, Vector3 scale, Vector2 clipBottomLeft, Vector2 clipTopRight)
	{
		Vector2 vector = clipBottomLeft;
		Vector2 vector2 = clipTopRight;
		Vector3 position = spriteDef.position0;
		Vector3 position2 = spriteDef.position3;
		Vector3 vector3 = new Vector3(Mathf.Lerp(position.x, position2.x, vector.x) * scale.x, Mathf.Lerp(position.y, position2.y, vector.y) * scale.y, position.z * scale.z);
		Vector3 vector4 = new Vector3(Mathf.Lerp(position.x, position2.x, vector2.x) * scale.x, Mathf.Lerp(position.y, position2.y, vector2.y) * scale.y, position.z * scale.z);
		pos[offset] = new Vector3(vector3.x, vector3.y, vector3.z) + translation;
		pos[offset + 1] = new Vector3(vector4.x, vector3.y, vector3.z) + translation;
		pos[offset + 2] = new Vector3(vector3.x, vector4.y, vector3.z) + translation;
		pos[offset + 3] = new Vector3(vector4.x, vector4.y, vector3.z) + translation;
		if (m_sprite.ShouldDoTilt)
		{
			for (int i = offset; i < offset + 4; i++)
			{
				if (m_sprite.IsPerpendicular)
				{
					pos[i].z -= pos[i].y;
				}
				else
				{
					pos[i].z += pos[i].y;
				}
			}
		}
		if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
		{
			Vector2 vector5 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector.x));
			Vector2 vector6 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector2.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector2.x));
			uv[offset] = new Vector2(vector5.x, vector5.y);
			uv[offset + 1] = new Vector2(vector5.x, vector6.y);
			uv[offset + 2] = new Vector2(vector6.x, vector5.y);
			uv[offset + 3] = new Vector2(vector6.x, vector6.y);
		}
		else if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
		{
			Vector2 vector7 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector.x));
			Vector2 vector8 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector2.y), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector2.x));
			uv[offset] = new Vector2(vector7.x, vector7.y);
			uv[offset + 2] = new Vector2(vector8.x, vector7.y);
			uv[offset + 1] = new Vector2(vector7.x, vector8.y);
			uv[offset + 3] = new Vector2(vector8.x, vector8.y);
		}
		else
		{
			Vector2 vector9 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector.x), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector.y));
			Vector2 vector10 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, vector2.x), Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, vector2.y));
			uv[offset] = new Vector2(vector9.x, vector9.y);
			uv[offset + 1] = new Vector2(vector10.x, vector9.y);
			uv[offset + 2] = new Vector2(vector9.x, vector10.y);
			uv[offset + 3] = new Vector2(vector10.x, vector10.y);
		}
	}
}
