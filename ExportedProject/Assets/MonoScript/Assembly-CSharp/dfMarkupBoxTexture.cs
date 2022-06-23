using System;
using UnityEngine;

public class dfMarkupBoxTexture : dfMarkupBox
{
	private static int[] TRIANGLE_INDICES = new int[6] { 0, 1, 2, 0, 2, 3 };

	private dfRenderData renderData = new dfRenderData();

	private Material material;

	public Texture Texture { get; set; }

	public dfMarkupBoxTexture(dfMarkupElement element, dfMarkupDisplayType display, dfMarkupStyle style)
		: base(element, display, style)
	{
	}

	internal void LoadTexture(Texture texture)
	{
		if (texture == null)
		{
			throw new InvalidOperationException();
		}
		Texture = texture;
		Size = new Vector2(texture.width, texture.height);
		Baseline = (int)Size.y;
	}

	protected override dfRenderData OnRebuildRenderData()
	{
		renderData.Clear();
		ensureMaterial();
		renderData.Material = material;
		renderData.Material.mainTexture = Texture;
		Vector3 zero = Vector3.zero;
		Vector3 vector = zero + Vector3.right * Size.x;
		Vector3 item = vector + Vector3.down * Size.y;
		Vector3 item2 = zero + Vector3.down * Size.y;
		renderData.Vertices.Add(zero);
		renderData.Vertices.Add(vector);
		renderData.Vertices.Add(item);
		renderData.Vertices.Add(item2);
		renderData.Triangles.AddRange(TRIANGLE_INDICES);
		renderData.UV.Add(new Vector2(0f, 1f));
		renderData.UV.Add(new Vector2(1f, 1f));
		renderData.UV.Add(new Vector2(1f, 0f));
		renderData.UV.Add(new Vector2(0f, 0f));
		Color color = Style.Color;
		renderData.Colors.Add(color);
		renderData.Colors.Add(color);
		renderData.Colors.Add(color);
		renderData.Colors.Add(color);
		return renderData;
	}

	private void ensureMaterial()
	{
		if (!(material != null) && !(Texture == null))
		{
			Shader shader = Shader.Find("Daikon Forge/Default UI Shader");
			if (shader == null)
			{
				Debug.LogError("Failed to find default shader");
				return;
			}
			material = new Material(shader)
			{
				name = "Default Texture Shader",
				hideFlags = HideFlags.DontSave,
				mainTexture = Texture
			};
		}
	}

	private static void addTriangleIndices(dfList<Vector3> verts, dfList<int> triangles)
	{
		int count = verts.Count;
		int[] tRIANGLE_INDICES = TRIANGLE_INDICES;
		for (int i = 0; i < tRIANGLE_INDICES.Length; i++)
		{
			triangles.Add(count + tRIANGLE_INDICES[i]);
		}
	}
}
