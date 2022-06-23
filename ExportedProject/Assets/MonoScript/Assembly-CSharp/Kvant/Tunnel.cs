using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kvant
{
	[ExecuteInEditMode]
	[AddComponentMenu("Kvant/Tunnel")]
	public class Tunnel : MonoBehaviour
	{
		[Serializable]
		private class BulkMesh
		{
			private Mesh _mesh;

			public Mesh mesh
			{
				get
				{
					return _mesh;
				}
			}

			public BulkMesh(int columns, int rowsPerSegment, int totalRows)
			{
				_mesh = BuildMesh(columns, rowsPerSegment, totalRows);
			}

			public void Rebuild(int columns, int rowsPerSegment, int totalRows)
			{
				Release();
				_mesh = BuildMesh(columns, rowsPerSegment, totalRows);
			}

			public void Release()
			{
				if (_mesh != null)
				{
					UnityEngine.Object.DestroyImmediate(_mesh);
					_mesh = null;
				}
			}

			private Mesh BuildMesh(int columns, int rows, int totalRows)
			{
				int num = rows + 1;
				float num2 = 0.5f / (float)columns;
				float num3 = 1f / (float)(totalRows + 1);
				float num4 = 0f;
				Vector2[] array = new Vector2[columns * (num - 1) * 6];
				Vector2[] array2 = new Vector2[columns * (num - 1) * 6];
				int num5 = 0;
				for (int i = 0; i < num - 1; i++)
				{
					int num6 = 0;
					while (num6 < columns)
					{
						int num7 = num6 * 2 + (i & 1);
						array[num5] = new Vector2(num2 * (float)num7, num4 + num3 * (float)i);
						array[num5 + 1] = new Vector2(num2 * (float)(num7 + 1), num4 + num3 * (float)(i + 1));
						array[num5 + 2] = new Vector2(num2 * (float)(num7 + 2), num4 + num3 * (float)i);
						array2[num5] = (array2[num5 + 1] = (array2[num5 + 2] = array[num5]));
						num6++;
						num5 += 3;
					}
				}
				for (int j = 0; j < num - 1; j++)
				{
					int num8 = 0;
					while (num8 < columns)
					{
						int num9 = num8 * 2 + 2 - (j & 1);
						array[num5] = new Vector2(num2 * (float)num9, num4 + num3 * (float)j);
						array[num5 + 1] = new Vector2(num2 * (float)(num9 - 1), num4 + num3 * (float)(j + 1));
						array[num5 + 2] = new Vector2(num2 * (float)(num9 + 1), num4 + num3 * (float)(j + 1));
						array2[num5] = (array2[num5 + 1] = (array2[num5 + 2] = array[num5]));
						num8++;
						num5 += 3;
					}
				}
				int[] array3 = new int[columns * (num - 1) * 3];
				int[] array4 = new int[columns * (num - 1) * 3];
				for (int k = 0; k < array3.Length; k++)
				{
					array3[k] = k;
					array4[k] = k + array3.Length;
				}
				int[] array5 = new int[columns * (num - 1) * 6];
				int num10 = 0;
				int num11 = 0;
				for (int l = 0; l < num - 1; l++)
				{
					int num12 = 0;
					while (num12 < columns)
					{
						array5[num10] = num11;
						array5[num10 + 1] = num11 + 1;
						array5[num10 + 2] = num11;
						array5[num10 + 3] = num11 + 2;
						array5[num10 + 4] = num11 + 1;
						array5[num10 + 5] = num11 + 2;
						num12++;
						num10 += 6;
						num11 += 3;
					}
				}
				Mesh mesh = new Mesh();
				mesh.subMeshCount = 3;
				mesh.vertices = new Vector3[array.Length];
				mesh.uv = array;
				mesh.uv2 = array2;
				mesh.SetIndices(array3, MeshTopology.Triangles, 0);
				mesh.SetIndices(array4, MeshTopology.Triangles, 1);
				mesh.SetIndices(array5, MeshTopology.Lines, 2);
				mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
				mesh.hideFlags = HideFlags.DontSave;
				return mesh;
			}
		}

		[SerializeField]
		private int _slices = 40;

		[SerializeField]
		private int _stacks = 100;

		[SerializeField]
		private float _radius = 5f;

		[SerializeField]
		private float _height = 50f;

		[SerializeField]
		private float _offset;

		[SerializeField]
		private int _noiseRepeat = 2;

		[SerializeField]
		private float _noiseFrequency = 0.05f;

		[SerializeField]
		[Range(1f, 5f)]
		private int _noiseDepth = 3;

		[SerializeField]
		private float _noiseClampMin = -1f;

		[SerializeField]
		private float _noiseClampMax = 1f;

		[SerializeField]
		private float _noiseElevation = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _noiseWarp;

		[SerializeField]
		private Material _material;

		private bool _owningMaterial;

		[SerializeField]
		private ShadowCastingMode _castShadows;

		[SerializeField]
		private bool _receiveShadows;

		[ColorUsage(true, true, 0f, 8f, 0.125f, 3f)]
		[SerializeField]
		private Color _lineColor = new Color(0f, 0f, 0f, 0.4f);

		[SerializeField]
		private Shader _kernelShader;

		[SerializeField]
		private Shader _lineShader;

		[SerializeField]
		private Shader _debugShader;

		private int _stacksPerSegment;

		private int _totalStacks;

		private RenderTexture _positionBuffer;

		private RenderTexture _normalBuffer1;

		private RenderTexture _normalBuffer2;

		private BulkMesh _bulkMesh;

		private Material _kernelMaterial;

		private Material _lineMaterial;

		private Material _debugMaterial;

		private bool _needsReset = true;

		public int slices
		{
			get
			{
				return _slices;
			}
		}

		public int stacks
		{
			get
			{
				return _totalStacks;
			}
		}

		public float radius
		{
			get
			{
				return _radius;
			}
			set
			{
				_radius = value;
			}
		}

		public float height
		{
			get
			{
				return _height;
			}
			set
			{
				_height = value;
			}
		}

		public float offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}

		public int noiseRepeat
		{
			get
			{
				return _noiseRepeat;
			}
			set
			{
				_noiseRepeat = value;
			}
		}

		public float noiseFrequency
		{
			get
			{
				return _noiseFrequency;
			}
			set
			{
				_noiseFrequency = value;
			}
		}

		public int noiseDepth
		{
			get
			{
				return _noiseDepth;
			}
			set
			{
				_noiseDepth = value;
			}
		}

		public float noiseClampMin
		{
			get
			{
				return _noiseClampMin;
			}
			set
			{
				_noiseClampMin = value;
			}
		}

		public float noiseClampMax
		{
			get
			{
				return _noiseClampMax;
			}
			set
			{
				_noiseClampMax = value;
			}
		}

		public float noiseElevation
		{
			get
			{
				return _noiseElevation;
			}
			set
			{
				_noiseElevation = value;
			}
		}

		public float noiseWarp
		{
			get
			{
				return _noiseWarp;
			}
			set
			{
				_noiseWarp = value;
			}
		}

		public Material sharedMaterial
		{
			get
			{
				return _material;
			}
			set
			{
				_material = value;
			}
		}

		public Material material
		{
			get
			{
				if (!_owningMaterial)
				{
					_material = UnityEngine.Object.Instantiate(_material);
					_owningMaterial = true;
				}
				return _material;
			}
			set
			{
				if (_owningMaterial)
				{
					UnityEngine.Object.Destroy(_material, 0.1f);
				}
				_material = value;
				_owningMaterial = false;
			}
		}

		public ShadowCastingMode shadowCastingMode
		{
			get
			{
				return _castShadows;
			}
			set
			{
				_castShadows = value;
			}
		}

		public bool receiveShadows
		{
			get
			{
				return _receiveShadows;
			}
			set
			{
				_receiveShadows = value;
			}
		}

		public Color lineColor
		{
			get
			{
				return _lineColor;
			}
			set
			{
				_lineColor = value;
			}
		}

		private float ZOffset
		{
			get
			{
				return Mathf.Repeat(_offset, _height / (float)_totalStacks * 2f);
			}
		}

		private float VOffset
		{
			get
			{
				return ZOffset - _offset;
			}
		}

		private void UpdateColumnAndRowCounts()
		{
			_slices = Mathf.Clamp(_slices, 4, 4096);
			_stacks = Mathf.Clamp(_stacks, 4, 4096);
			int num = _slices * (_stacks + 1) * 6;
			int num2 = num / 65000 + 1;
			_stacksPerSegment = ((num2 <= 1) ? _stacks : (_stacks / num2 / 2 * 2));
			_totalStacks = _stacksPerSegment * num2;
		}

		public void NotifyConfigChange()
		{
			_needsReset = true;
		}

		private Material CreateMaterial(Shader shader)
		{
			Material material = new Material(shader);
			material.hideFlags = HideFlags.DontSave;
			return material;
		}

		private RenderTexture CreateBuffer()
		{
			int width = _slices * 2;
			int num = _totalStacks + 1;
			RenderTexture renderTexture = new RenderTexture(width, num, 0, RenderTextureFormat.ARGBFloat);
			renderTexture.hideFlags = HideFlags.DontSave;
			renderTexture.filterMode = FilterMode.Point;
			renderTexture.wrapMode = TextureWrapMode.Repeat;
			return renderTexture;
		}

		private void UpdateKernelShader()
		{
			Material kernelMaterial = _kernelMaterial;
			kernelMaterial.SetVector("_Extent", new Vector2(_radius, _height));
			kernelMaterial.SetFloat("_Offset", VOffset);
			kernelMaterial.SetVector("_Frequency", new Vector2(_noiseRepeat, _noiseFrequency));
			kernelMaterial.SetVector("_Amplitude", new Vector3(1f, _noiseWarp, _noiseWarp) * _noiseElevation);
			kernelMaterial.SetVector("_ClampRange", new Vector2(_noiseClampMin, _noiseClampMax) * 1.415f);
			if (_noiseWarp > 0f)
			{
				kernelMaterial.EnableKeyword("ENABLE_WARP");
			}
			else
			{
				kernelMaterial.DisableKeyword("ENABLE_WARP");
			}
			for (int i = 1; i <= 5; i++)
			{
				if (i == _noiseDepth)
				{
					kernelMaterial.EnableKeyword("DEPTH" + i);
				}
				else
				{
					kernelMaterial.DisableKeyword("DEPTH" + i);
				}
			}
		}

		private void ResetResources()
		{
			UpdateColumnAndRowCounts();
			if (_bulkMesh == null)
			{
				_bulkMesh = new BulkMesh(_slices, _stacksPerSegment, _totalStacks);
			}
			else
			{
				_bulkMesh.Rebuild(_slices, _stacksPerSegment, _totalStacks);
			}
			if ((bool)_positionBuffer)
			{
				UnityEngine.Object.DestroyImmediate(_positionBuffer);
			}
			if ((bool)_normalBuffer1)
			{
				UnityEngine.Object.DestroyImmediate(_normalBuffer1);
			}
			if ((bool)_normalBuffer2)
			{
				UnityEngine.Object.DestroyImmediate(_normalBuffer2);
			}
			_positionBuffer = CreateBuffer();
			_normalBuffer1 = CreateBuffer();
			_normalBuffer2 = CreateBuffer();
			if (!_kernelMaterial)
			{
				_kernelMaterial = CreateMaterial(_kernelShader);
			}
			if (!_lineMaterial)
			{
				_lineMaterial = CreateMaterial(_lineShader);
			}
			if (!_debugMaterial)
			{
				_debugMaterial = CreateMaterial(_debugShader);
			}
			_lineMaterial.SetTexture("_PositionBuffer", _positionBuffer);
			_needsReset = false;
		}

		private void Reset()
		{
			_needsReset = true;
		}

		private void OnDestroy()
		{
			if (_bulkMesh != null)
			{
				_bulkMesh.Release();
			}
			if ((bool)_positionBuffer)
			{
				UnityEngine.Object.DestroyImmediate(_positionBuffer);
			}
			if ((bool)_normalBuffer1)
			{
				UnityEngine.Object.DestroyImmediate(_normalBuffer1);
			}
			if ((bool)_normalBuffer2)
			{
				UnityEngine.Object.DestroyImmediate(_normalBuffer2);
			}
			if ((bool)_kernelMaterial)
			{
				UnityEngine.Object.DestroyImmediate(_kernelMaterial);
			}
			if ((bool)_lineMaterial)
			{
				UnityEngine.Object.DestroyImmediate(_lineMaterial);
			}
			if ((bool)_debugMaterial)
			{
				UnityEngine.Object.DestroyImmediate(_debugMaterial);
			}
		}

		private void LateUpdate()
		{
			if (_needsReset)
			{
				ResetResources();
			}
			UpdateKernelShader();
			Graphics.Blit(null, _positionBuffer, _kernelMaterial, 0);
			Graphics.Blit(_positionBuffer, _normalBuffer1, _kernelMaterial, 1);
			Graphics.Blit(_positionBuffer, _normalBuffer2, _kernelMaterial, 2);
			_lineMaterial.SetColor("_Color", _lineColor);
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			MaterialPropertyBlock materialPropertyBlock2 = new MaterialPropertyBlock();
			materialPropertyBlock.SetTexture("_PositionBuffer", _positionBuffer);
			materialPropertyBlock2.SetTexture("_PositionBuffer", _positionBuffer);
			materialPropertyBlock.SetTexture("_NormalBuffer", _normalBuffer1);
			materialPropertyBlock2.SetTexture("_NormalBuffer", _normalBuffer2);
			Vector3 vector = new Vector3(0f, 0f, VOffset);
			materialPropertyBlock.SetVector("_MapOffset", vector);
			materialPropertyBlock2.SetVector("_MapOffset", vector);
			materialPropertyBlock.SetFloat("_UseBuffer", 1f);
			materialPropertyBlock2.SetFloat("_UseBuffer", 1f);
			Mesh mesh = _bulkMesh.mesh;
			Vector3 position = base.transform.position;
			Quaternion rotation = base.transform.rotation;
			Vector2 vector2 = new Vector2(0.5f / (float)_positionBuffer.width, 0f);
			position += base.transform.forward * ZOffset;
			for (int i = 0; i < _totalStacks; i += _stacksPerSegment)
			{
				vector2.y = (0.5f + (float)i) / (float)_positionBuffer.height;
				materialPropertyBlock.SetVector("_BufferOffset", vector2);
				materialPropertyBlock2.SetVector("_BufferOffset", vector2);
				if ((bool)_material)
				{
					Graphics.DrawMesh(mesh, position, rotation, _material, 0, null, 0, materialPropertyBlock, _castShadows, _receiveShadows);
					Graphics.DrawMesh(mesh, position, rotation, _material, 0, null, 1, materialPropertyBlock2, _castShadows, _receiveShadows);
				}
				if (_lineColor.a > 0f)
				{
					Graphics.DrawMesh(mesh, position, rotation, _lineMaterial, 0, null, 2, materialPropertyBlock, false, false);
				}
			}
		}
	}
}
