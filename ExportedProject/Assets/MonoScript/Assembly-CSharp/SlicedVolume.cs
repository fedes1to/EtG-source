using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class SlicedVolume : MonoBehaviour
{
	public Material cloudMaterial;

	public Material shadowCasterMat;

	public int sliceAmount = 25;

	public int segmentCount = 3;

	public Vector3 dimensions = new Vector3(1000f, 50f, 1000f);

	public Vector3 normalDirection = new Vector3(1f, 1f, 1f);

	public bool shadowCaster;

	public bool transferVariables = true;

	public bool unityFive;

	public bool curved;

	public float roundness = 2f;

	public float intensity = 0.001f;

	public bool generateNewSlices;

	private bool updateCloudDirection = true;

	private int cameraCloudRelation = 1;

	private Color[] vertexColor;

	private Vector3[] vertices;

	private Vector2[] uvMap;

	private int[] triangleConstructor;

	private Vector3 posGainPerVertices;

	private float posGainPerUV;

	private GameObject meshSlices;

	private GameObject meshShadowCaster;

	private void OnDrawGizmos()
	{
		editorUpdate();
		if (!generateNewSlices)
		{
			return;
		}
		if ((bool)cloudMaterial)
		{
			integrityCheck();
			settingValuesUp(false);
			if (shadowCaster)
			{
				int num = sliceAmount;
				sliceAmount = 1;
				settingValuesUp(true);
				sliceAmount = num;
			}
		}
		generateNewSlices = false;
	}

	private void Update()
	{
		if (!Application.isPlaying || !generateNewSlices)
		{
			return;
		}
		if ((bool)cloudMaterial)
		{
			integrityCheck();
			settingValuesUp(false);
			if (shadowCaster)
			{
				int num = sliceAmount;
				sliceAmount = 1;
				settingValuesUp(true);
				sliceAmount = num;
			}
		}
		generateNewSlices = false;
	}

	private void syncCloudAndShadowCaster()
	{
		shadowCasterMat.CopyPropertiesFromMaterial(cloudMaterial);
	}

	private void editorUpdate()
	{
		sliceAmount = ((sliceAmount <= 1) ? 1 : sliceAmount);
		segmentCount = ((segmentCount <= 2) ? 2 : segmentCount);
		if (Camera.current.name != "PreRenderCamera" && (bool)cloudMaterial && !curved && (bool)meshSlices)
		{
			if (Camera.current.transform.position.y > base.transform.position.y && cameraCloudRelation == -1)
			{
				cameraCloudRelation = 1;
				updateCloudDirection = true;
			}
			else if (Camera.current.transform.position.y < base.transform.position.y && cameraCloudRelation == 1)
			{
				cameraCloudRelation = -1;
				updateCloudDirection = true;
			}
			if (updateCloudDirection)
			{
				meshSlices.transform.localScale = new Vector3(Mathf.Abs(meshSlices.transform.localScale.x), Mathf.Abs(meshSlices.transform.localScale.y) * (float)cameraCloudRelation, Mathf.Abs(meshSlices.transform.localScale.z));
				cloudMaterial.SetVector("_CloudNormalsDirection", new Vector4(normalDirection.x, normalDirection.y * (float)cameraCloudRelation, normalDirection.z * -1f, 0f));
				updateCloudDirection = false;
			}
		}
		else if (curved && (bool)cloudMaterial && (bool)meshSlices)
		{
			meshSlices.transform.localScale = new Vector3(Mathf.Abs(meshSlices.transform.localScale.x), Mathf.Abs(meshSlices.transform.localScale.y) * -1f, Mathf.Abs(meshSlices.transform.localScale.z));
			if ((bool)meshShadowCaster)
			{
				meshShadowCaster.transform.localScale = new Vector3(Mathf.Abs(meshSlices.transform.localScale.x), Mathf.Abs(meshSlices.transform.localScale.y) * -1f, Mathf.Abs(meshSlices.transform.localScale.z));
			}
			cloudMaterial.SetVector("_CloudNormalsDirection", new Vector4(normalDirection.x, normalDirection.y * -1f, normalDirection.z * -1f, 0f));
		}
		if (transferVariables && (bool)cloudMaterial && (bool)shadowCasterMat)
		{
			syncCloudAndShadowCaster();
		}
	}

	private void integrityCheck()
	{
		if (!meshSlices)
		{
			IEnumerator enumerator = base.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					if (transform.name == "Clouds")
					{
						meshSlices = transform.gameObject;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			if (!meshSlices)
			{
				meshSlices = new GameObject("Clouds");
				meshSlices.transform.parent = base.transform;
				meshSlices.transform.localPosition = Vector3.zero;
				meshSlices.AddComponent<MeshFilter>();
				meshSlices.AddComponent<MeshRenderer>();
				meshSlices.GetComponent<Renderer>().material = cloudMaterial;
			}
		}
		if (shadowCaster && !meshShadowCaster)
		{
			IEnumerator enumerator2 = base.transform.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					Transform transform2 = (Transform)enumerator2.Current;
					if (transform2.name == "Shadow Caster")
					{
						meshShadowCaster = transform2.gameObject;
					}
				}
			}
			finally
			{
				IDisposable disposable2;
				if ((disposable2 = enumerator2 as IDisposable) != null)
				{
					disposable2.Dispose();
				}
			}
			if (!meshShadowCaster)
			{
				meshShadowCaster = new GameObject("Shadow Caster");
				meshShadowCaster.transform.parent = base.transform;
				meshShadowCaster.transform.localPosition = Vector3.zero;
				meshShadowCaster.AddComponent<MeshFilter>();
				meshShadowCaster.AddComponent<MeshRenderer>();
				meshShadowCaster.GetComponent<Renderer>().material = shadowCasterMat;
			}
		}
		if (!shadowCaster)
		{
			if ((bool)meshShadowCaster)
			{
				UnityEngine.Object.DestroyImmediate(meshShadowCaster);
			}
			else
			{
				IEnumerator enumerator3 = base.transform.GetEnumerator();
				try
				{
					while (enumerator3.MoveNext())
					{
						Transform transform3 = (Transform)enumerator3.Current;
						if (transform3.name == "Shadow Caster")
						{
							UnityEngine.Object.DestroyImmediate(transform3.gameObject);
						}
					}
				}
				finally
				{
					IDisposable disposable3;
					if ((disposable3 = enumerator3 as IDisposable) != null)
					{
						disposable3.Dispose();
					}
				}
			}
		}
		if (meshShadowCaster != null)
		{
			meshShadowCaster.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
		}
		meshSlices.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
		if (meshShadowCaster != null)
		{
			meshShadowCaster.GetComponent<MeshRenderer>().receiveShadows = false;
		}
		meshSlices.GetComponent<MeshRenderer>().receiveShadows = false;
	}

	private void settingValuesUp(bool isShadowCaster)
	{
		vertices = new Vector3[segmentCount * segmentCount * sliceAmount];
		uvMap = new Vector2[vertices.Length];
		triangleConstructor = new int[(segmentCount - 1) * (segmentCount - 1) * sliceAmount * 2 * 3];
		vertexColor = new Color[vertices.Length];
		float num = 1f / ((float)segmentCount - 1f);
		posGainPerVertices = new Vector3(num * dimensions.x, 1f / (float)Mathf.Clamp(sliceAmount - 1, 1, 999999) * dimensions.y, num * dimensions.z);
		posGainPerUV = num;
		trianglesCreation(isShadowCaster);
	}

	private void trianglesCreation(bool isShadowCaster)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		float f = 0f;
		float num4 = -1f;
		float num5 = 0f;
		for (int i = 0; i < sliceAmount; i++)
		{
			num4 = -1f + (float)i * (2f / (float)sliceAmount);
			num5 = ((!((float)i < (float)sliceAmount * 0.5f)) ? (2f - 1f / ((float)sliceAmount * 0.5f) * (float)(i + 1)) : (1f / ((float)sliceAmount * 0.5f) * (float)i));
			if (sliceAmount == 1)
			{
				num5 = 1f;
			}
			for (int j = 0; j < segmentCount; j++)
			{
				int num6 = segmentCount * num;
				for (int k = 0; k < segmentCount; k++)
				{
					if (curved)
					{
						f = Vector3.Distance(new Vector3(posGainPerVertices.x * (float)k - dimensions.x / 2f, 0f, posGainPerVertices.z * (float)j - dimensions.z / 2f), Vector3.zero);
					}
					if (sliceAmount == 1)
					{
						vertices[k + num6] = new Vector3(posGainPerVertices.x * (float)k - dimensions.x / 2f, Mathf.Pow(f, roundness) * intensity, posGainPerVertices.z * (float)j - dimensions.z / 2f);
					}
					else
					{
						vertices[k + num6] = new Vector3(posGainPerVertices.x * (float)k - dimensions.x / 2f, posGainPerVertices.y * (float)i - dimensions.y / 2f + Mathf.Pow(f, roundness) * intensity, posGainPerVertices.z * (float)j - dimensions.z / 2f);
					}
					uvMap[k + num6] = new Vector2(posGainPerUV * (float)k, posGainPerUV * (float)j);
					vertexColor[k + num6] = new Vector4(num4, num4, num4, num5);
				}
				num++;
				if (j >= 1)
				{
					for (int l = 0; l < segmentCount - 1; l++)
					{
						triangleConstructor[num3] = l + num2 + i * segmentCount;
						triangleConstructor[1 + num3] = segmentCount + l + num2 + i * segmentCount;
						triangleConstructor[2 + num3] = 1 + l + num2 + i * segmentCount;
						triangleConstructor[3 + num3] = segmentCount + 1 + l + num2 + i * segmentCount;
						triangleConstructor[4 + num3] = 1 + l + num2 + i * segmentCount;
						triangleConstructor[5 + num3] = segmentCount + l + num2 + i * segmentCount;
						num3 += 6;
					}
					num2 += segmentCount;
				}
			}
		}
		if (!isShadowCaster)
		{
			Mesh mesh = new Mesh();
			mesh.Clear();
			mesh.name = "GeoSlices";
			mesh.vertices = vertices;
			mesh.triangles = triangleConstructor;
			mesh.uv = uvMap;
			mesh.colors = vertexColor;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			calculateMeshTangents(mesh);
			meshSlices.GetComponent<MeshFilter>().mesh = mesh;
		}
		else
		{
			Mesh mesh2 = new Mesh();
			mesh2.Clear();
			mesh2.name = "GeoSlices";
			mesh2.vertices = vertices;
			mesh2.triangles = triangleConstructor;
			mesh2.uv = uvMap;
			mesh2.colors = vertexColor;
			mesh2.RecalculateNormals();
			mesh2.RecalculateBounds();
			calculateMeshTangents(mesh2);
			meshShadowCaster.GetComponent<MeshFilter>().mesh = mesh2;
		}
	}

	public static void calculateMeshTangents(Mesh mesh)
	{
		int[] triangles = mesh.triangles;
		Vector3[] array = mesh.vertices;
		Vector2[] uv = mesh.uv;
		Vector3[] normals = mesh.normals;
		int num = triangles.Length;
		int num2 = array.Length;
		Vector3[] array2 = new Vector3[num2];
		Vector3[] array3 = new Vector3[num2];
		Vector4[] array4 = new Vector4[num2];
		for (long num3 = 0L; num3 < num; num3 += 3)
		{
			long num4 = triangles[num3];
			long num5 = triangles[num3 + 1];
			long num6 = triangles[num3 + 2];
			Vector3 vector = array[num4];
			Vector3 vector2 = array[num5];
			Vector3 vector3 = array[num6];
			Vector2 vector4 = uv[num4];
			Vector2 vector5 = uv[num5];
			Vector2 vector6 = uv[num6];
			float num7 = vector2.x - vector.x;
			float num8 = vector3.x - vector.x;
			float num9 = vector2.y - vector.y;
			float num10 = vector3.y - vector.y;
			float num11 = vector2.z - vector.z;
			float num12 = vector3.z - vector.z;
			float num13 = vector5.x - vector4.x;
			float num14 = vector6.x - vector4.x;
			float num15 = vector5.y - vector4.y;
			float num16 = vector6.y - vector4.y;
			float num17 = 1f / (num13 * num16 - num14 * num15);
			Vector3 vector7 = new Vector3((num16 * num7 - num15 * num8) * num17, (num16 * num9 - num15 * num10) * num17, (num16 * num11 - num15 * num12) * num17);
			Vector3 vector8 = new Vector3((num13 * num8 - num14 * num7) * num17, (num13 * num10 - num14 * num9) * num17, (num13 * num12 - num14 * num11) * num17);
			array2[num4] += vector7;
			array2[num5] += vector7;
			array2[num6] += vector7;
			array3[num4] += vector8;
			array3[num5] += vector8;
			array3[num6] += vector8;
		}
		for (long num18 = 0L; num18 < num2; num18++)
		{
			Vector3 normal = normals[num18];
			Vector3 tangent = array2[num18];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			array4[num18].x = tangent.x;
			array4[num18].y = tangent.y;
			array4[num18].z = tangent.z;
			array4[num18].w = ((!(Vector3.Dot(Vector3.Cross(normal, tangent), array3[num18]) < 0f)) ? 1f : (-1f));
		}
		mesh.tangents = array4;
	}
}
