using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailgunChargeEffectController : BraveBehaviour
{
	public enum LineChargeMode
	{
		SEQUENTIAL_PARALLEL,
		TRIANGULAR_CONVERGE,
		PYRAMIDAL_CONVERGE,
		VERTICAL_CONVERGE,
		SCALING_PARALLEL
	}

	public LineChargeMode lineMode;

	public GameObject childLinePrefab;

	public float Width = 1f;

	public float NewLineFrequency = 0.5f;

	public float LineTraversalTime = 0.5f;

	public float StopCreatingLinesTime = 3f;

	public bool SequentialLinesReduceTraversalTime;

	[ShowInInspectorIf("lineMode", 0, false)]
	public float DistanceStart = 1f;

	[ShowInInspectorIf("lineMode", 1, false)]
	public float AngleStart = 90f;

	[ShowInInspectorIf("lineMode", 2, false)]
	public float SolidAngleStart = 60f;

	[ShowInInspectorIf("lineMode", 2, false)]
	public float SolidRotationSpeed = 180f;

	[ShowInInspectorIf("lineMode", 4, false)]
	public float ScalingDistanceDepth = 0.25f;

	[ShowInInspectorIf("lineMode", 4, false)]
	public float ScalingDistanceStart = 1f;

	[ShowInInspectorIf("lineMode", 4, false)]
	public float ScalingPower = 3f;

	public bool SmoothLerpIn;

	public bool SmoothLerpOut;

	public bool UseRaycast;

	public bool DestroyedOnCompletion;

	public float TargetHeightOffGround = -0.5f;

	public Gradient ColorGradient;

	public ParticleSystem ImpactParticles;

	private Gun m_ownerGun;

	private tk2dTiledSprite m_sprite;

	private List<tk2dTiledSprite> m_childLines;

	private float m_lineTimer;

	private float m_totalTimer;

	private float m_modTraversalTime;

	private bool m_hasConverged;

	public float? overrideBeamLength;

	[NonSerialized]
	public bool IsManuallyControlled;

	[NonSerialized]
	public float ManualCompletionPercentage;

	private Transform m_cachedParentTransform;

	private Dictionary<tk2dTiledSprite, float> CompletionMap;

	private void Start()
	{
		m_sprite = GetComponent<tk2dTiledSprite>();
		m_modTraversalTime = LineTraversalTime;
		m_sprite.color = ColorGradient.Evaluate(1f);
		m_ownerGun = base.gameObject.GetComponentInParent<Gun>();
		m_childLines = new List<tk2dTiledSprite>();
		UpdateAngleAndLength();
		if (lineMode == LineChargeMode.VERTICAL_CONVERGE)
		{
			AkSoundEngine.PostEvent("Play_WPN_dawnhammer_charge_01", base.gameObject);
		}
	}

	private void OnEnable()
	{
		m_cachedParentTransform = base.transform.parent;
	}

	private tk2dTiledSprite CreateDuplicate(bool forceVisible = false)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(childLinePrefab);
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.GetComponent<Renderer>().enabled = forceVisible || base.renderer.enabled;
		return gameObject.GetComponent<tk2dTiledSprite>();
	}

	private void UpdateAngleAndLength()
	{
		if (m_cachedParentTransform != base.transform.parent)
		{
			m_ownerGun = base.gameObject.GetComponentInParent<Gun>();
			m_cachedParentTransform = base.transform.parent;
		}
		if (lineMode == LineChargeMode.VERTICAL_CONVERGE)
		{
			m_sprite.dimensions = new Vector2(270f, Width);
			if ((bool)ImpactParticles && ImpactParticles.isPlaying)
			{
				ImpactParticles.Stop();
			}
		}
		else
		{
			if (lineMode == LineChargeMode.SCALING_PARALLEL && (bool)m_ownerGun && !m_ownerGun.IsFiring)
			{
				SpawnManager.Despawn(base.gameObject);
				return;
			}
			if ((bool)m_ownerGun)
			{
				base.transform.rotation = Quaternion.Euler(0f, 0f, m_ownerGun.CurrentAngle);
			}
			int rayMask = CollisionMask.LayerToMask(CollisionLayer.HighObstacle);
			bool flag = false;
			RaycastResult result;
			if (overrideBeamLength.HasValue)
			{
				result = null;
			}
			else
			{
				flag = PhysicsEngine.Instance.Raycast(base.transform.position.XY(), base.transform.right, 30f, out result, true, false, rayMask);
				if (UseRaycast)
				{
					RaycastResult.Pool.Free(ref result);
					flag |= PhysicsEngine.Instance.Raycast(base.transform.position.XY(), base.transform.right, 30f, out result, false, true, rayMask);
				}
			}
			if (flag && result != null)
			{
				if ((bool)m_sprite)
				{
					m_sprite.dimensions = new Vector2(result.Distance / 0.0625f, Width);
				}
				if ((bool)ImpactParticles)
				{
					ImpactParticles.transform.position = result.Contact.ToVector3ZUp(result.Contact.y - TargetHeightOffGround);
					if (m_hasConverged)
					{
						if (!ImpactParticles.isPlaying)
						{
							ImpactParticles.Play();
						}
					}
					else if (ImpactParticles.isPlaying)
					{
						ImpactParticles.Stop();
					}
				}
			}
			else if (overrideBeamLength.HasValue)
			{
				if ((bool)m_sprite)
				{
					m_sprite.dimensions = new Vector2(overrideBeamLength.Value * 16f, Width);
				}
				if ((bool)ImpactParticles && (bool)m_sprite)
				{
					ImpactParticles.transform.position = m_sprite.transform.position + new Vector3(0f, 0f - overrideBeamLength.Value, 0f - overrideBeamLength.Value);
					if (!ImpactParticles.isPlaying)
					{
						ImpactParticles.Play();
					}
				}
			}
			else
			{
				if ((bool)m_sprite)
				{
					m_sprite.dimensions = new Vector2(480f, Width);
				}
				if ((bool)ImpactParticles && ImpactParticles.isPlaying)
				{
					ImpactParticles.Stop();
				}
			}
			RaycastResult.Pool.Free(ref result);
		}
		if (!m_sprite)
		{
			return;
		}
		m_sprite.IsPerpendicular = false;
		m_sprite.HeightOffGround = TargetHeightOffGround;
		m_sprite.UpdateZDepth();
		for (int i = 0; i < m_childLines.Count; i++)
		{
			m_childLines[i].dimensions = m_sprite.dimensions;
			if (lineMode == LineChargeMode.SCALING_PARALLEL && CompletionMap != null && CompletionMap.ContainsKey(m_childLines[i]))
			{
				float f = CompletionMap[m_childLines[i]];
				f = Mathf.Pow(f, ScalingPower);
				m_childLines[i].dimensions = Vector2.Lerp(new Vector2(Width * 2f, Width * 2f), m_childLines[i].dimensions, f);
			}
			m_childLines[i].IsPerpendicular = false;
			m_childLines[i].HeightOffGround = TargetHeightOffGround;
			m_childLines[i].UpdateZDepth();
		}
	}

	public void OnSpawned()
	{
		m_totalTimer = 0f;
		m_modTraversalTime = LineTraversalTime;
		m_lineTimer = 0f;
		UpdateAngleAndLength();
		m_hasConverged = false;
	}

	public void OnDespawned()
	{
		StopAllCoroutines();
		for (int i = 0; i < m_childLines.Count; i++)
		{
			UnityEngine.Object.Destroy(m_childLines[i].gameObject);
		}
		m_childLines.Clear();
	}

	private IEnumerator HandleLine_ScalingParallel(float modTraversalTime)
	{
		if (CompletionMap == null)
		{
			CompletionMap = new Dictionary<tk2dTiledSprite, float>();
		}
		tk2dTiledSprite duplicate1 = CreateDuplicate(true);
		tk2dTiledSprite duplicate2 = CreateDuplicate(true);
		CompletionMap.Add(duplicate1, 0f);
		CompletionMap.Add(duplicate2, 0f);
		m_childLines.Add(duplicate1);
		m_childLines.Add(duplicate2);
		duplicate1.transform.localPosition = new Vector3(0f - ScalingDistanceDepth, ScalingDistanceStart, 0f);
		duplicate2.transform.localPosition = new Vector3(0f - ScalingDistanceDepth, 0f - ScalingDistanceStart, 0f);
		duplicate1.color = ColorGradient.Evaluate(0f);
		duplicate2.color = ColorGradient.Evaluate(0f);
		float elapsed = 0f;
		while (elapsed < modTraversalTime)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / modTraversalTime;
			if (SmoothLerpIn && SmoothLerpOut)
			{
				t = Mathf.SmoothStep(0f, 1f, t);
			}
			else if (SmoothLerpIn)
			{
				t = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t);
			}
			else if (SmoothLerpOut)
			{
				t = BraveMathCollege.LinearToSmoothStepInterpolate(0f, 1f, t);
			}
			duplicate1.transform.localPosition = Vector3.Lerp(new Vector3(0f - ScalingDistanceDepth, ScalingDistanceStart, 0f), Vector3.zero, t);
			duplicate2.transform.localPosition = Vector3.Lerp(new Vector3(0f - ScalingDistanceDepth, 0f - ScalingDistanceStart, 0f), Vector3.zero, t);
			if (CompletionMap.ContainsKey(duplicate1))
			{
				CompletionMap[duplicate1] = t;
			}
			if (CompletionMap.ContainsKey(duplicate2))
			{
				CompletionMap[duplicate2] = t;
			}
			duplicate1.color = ColorGradient.Evaluate(t);
			duplicate2.color = ColorGradient.Evaluate(t);
			yield return null;
		}
	}

	private IEnumerator HandleLine_SequentialParallel(float modTraversalTime)
	{
		tk2dTiledSprite duplicate1 = CreateDuplicate();
		tk2dTiledSprite duplicate2 = CreateDuplicate();
		m_childLines.Add(duplicate1);
		m_childLines.Add(duplicate2);
		duplicate1.transform.localPosition = new Vector3(0f, DistanceStart, 0f);
		duplicate2.transform.localPosition = new Vector3(0f, 0f - DistanceStart, 0f);
		duplicate1.color = ColorGradient.Evaluate(0f);
		duplicate2.color = ColorGradient.Evaluate(0f);
		float elapsed = 0f;
		while (elapsed < modTraversalTime)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / modTraversalTime;
			if (SmoothLerpIn && SmoothLerpOut)
			{
				t = Mathf.SmoothStep(0f, 1f, t);
			}
			else if (SmoothLerpIn)
			{
				t = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t);
			}
			else if (SmoothLerpOut)
			{
				t = BraveMathCollege.LinearToSmoothStepInterpolate(0f, 1f, t);
			}
			duplicate1.transform.localPosition = Vector3.Lerp(new Vector3(0f, DistanceStart, 0f), Vector3.zero, t);
			duplicate2.transform.localPosition = Vector3.Lerp(new Vector3(0f, 0f - DistanceStart, 0f), Vector3.zero, t);
			duplicate1.color = ColorGradient.Evaluate(t);
			duplicate2.color = ColorGradient.Evaluate(t);
			yield return null;
		}
	}

	private IEnumerator HandleLine_PyramidalConverge(float modTraversalTime)
	{
		tk2dTiledSprite duplicate1 = CreateDuplicate(true);
		tk2dTiledSprite duplicate2 = CreateDuplicate(true);
		tk2dTiledSprite duplicate3 = CreateDuplicate(true);
		duplicate1.ShouldDoTilt = false;
		duplicate2.ShouldDoTilt = false;
		duplicate3.ShouldDoTilt = false;
		m_childLines.Add(duplicate1);
		m_childLines.Add(duplicate2);
		m_childLines.Add(duplicate3);
		duplicate1.transform.localRotation = Quaternion.Euler(0f, 0f, SolidAngleStart);
		duplicate2.transform.localRotation = Quaternion.Euler(120f, 0f, 0f) * Quaternion.Euler(0f, 0f, SolidAngleStart);
		duplicate3.transform.localRotation = Quaternion.Euler(240f, 0f, 0f) * Quaternion.Euler(0f, 0f, SolidAngleStart);
		duplicate1.color = ColorGradient.Evaluate(0f);
		duplicate2.color = ColorGradient.Evaluate(0f);
		duplicate3.color = ColorGradient.Evaluate(0f);
		float elapsed = 0f;
		while (elapsed < modTraversalTime)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / modTraversalTime;
			if (SmoothLerpIn && SmoothLerpOut)
			{
				t = Mathf.SmoothStep(0f, 1f, t);
			}
			else if (SmoothLerpIn)
			{
				t = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t);
			}
			else if (SmoothLerpOut)
			{
				t = BraveMathCollege.LinearToSmoothStepInterpolate(0f, 1f, t);
			}
			float baseAngle = elapsed * SolidRotationSpeed;
			float solidAngle = Mathf.Lerp(SolidAngleStart, 0f, t);
			duplicate1.transform.localRotation = Quaternion.Euler(baseAngle, 0f, 0f) * Quaternion.Euler(0f, 0f, solidAngle);
			duplicate2.transform.localRotation = Quaternion.Euler(baseAngle + 120f, 0f, 0f) * Quaternion.Euler(0f, 0f, solidAngle);
			duplicate3.transform.localRotation = Quaternion.Euler(baseAngle + 240f, 0f, 0f) * Quaternion.Euler(0f, 0f, solidAngle);
			duplicate1.color = ColorGradient.Evaluate(t);
			duplicate2.color = ColorGradient.Evaluate(t);
			duplicate3.color = ColorGradient.Evaluate(t);
			duplicate1.UpdateZDepth();
			duplicate2.UpdateZDepth();
			duplicate3.UpdateZDepth();
			yield return null;
		}
	}

	private IEnumerator HandleLine_VerticalConverge(float modTraversalTime)
	{
		tk2dTiledSprite duplicate1 = CreateDuplicate(true);
		tk2dTiledSprite duplicate2 = CreateDuplicate(true);
		tk2dTiledSprite duplicate3 = CreateDuplicate(true);
		m_childLines.Add(duplicate1);
		m_childLines.Add(duplicate2);
		m_childLines.Add(duplicate3);
		duplicate1.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
		duplicate2.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
		duplicate3.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
		duplicate1.color = ColorGradient.Evaluate(0f);
		duplicate2.color = ColorGradient.Evaluate(0f);
		duplicate3.color = ColorGradient.Evaluate(0f);
		float elapsed = 0f;
		while (elapsed < modTraversalTime)
		{
			elapsed += BraveTime.DeltaTime;
			if (IsManuallyControlled)
			{
				elapsed = Mathf.Clamp01(ManualCompletionPercentage) * modTraversalTime;
			}
			float t = elapsed / modTraversalTime;
			if (SmoothLerpIn && SmoothLerpOut)
			{
				t = Mathf.SmoothStep(0f, 1f, t);
			}
			else if (SmoothLerpIn)
			{
				t = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t);
			}
			else if (SmoothLerpOut)
			{
				t = BraveMathCollege.LinearToSmoothStepInterpolate(0f, 1f, t);
			}
			float baseAngle = elapsed * SolidRotationSpeed;
			float maxDistance = 2f;
			duplicate1.transform.localPosition = Quaternion.Euler(0f, 0f, baseAngle) * new Vector3(maxDistance * (1f - t), 0f, 0f);
			duplicate2.transform.localPosition = Quaternion.Euler(0f, 0f, baseAngle + 120f) * new Vector3(maxDistance * (1f - t), 0f, 0f);
			duplicate3.transform.localPosition = Quaternion.Euler(0f, 0f, baseAngle + 240f) * new Vector3(maxDistance * (1f - t), 0f, 0f);
			duplicate1.color = ColorGradient.Evaluate(t);
			duplicate2.color = ColorGradient.Evaluate(t);
			duplicate3.color = ColorGradient.Evaluate(t);
			duplicate1.UpdateZDepth();
			duplicate2.UpdateZDepth();
			duplicate3.UpdateZDepth();
			yield return null;
		}
	}

	private IEnumerator HandleLine_TriangularConverge(float modTraversalTime)
	{
		tk2dTiledSprite duplicate1 = CreateDuplicate();
		tk2dTiledSprite duplicate2 = CreateDuplicate();
		m_childLines.Add(duplicate1);
		m_childLines.Add(duplicate2);
		duplicate1.transform.localRotation = Quaternion.Euler(0f, 0f, AngleStart);
		duplicate2.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - AngleStart);
		duplicate1.color = ColorGradient.Evaluate(0f);
		duplicate2.color = ColorGradient.Evaluate(0f);
		float elapsed = 0f;
		while (elapsed < modTraversalTime)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / modTraversalTime;
			if (SmoothLerpIn && SmoothLerpOut)
			{
				t = Mathf.SmoothStep(0f, 1f, t);
			}
			else if (SmoothLerpIn)
			{
				t = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, t);
			}
			else if (SmoothLerpOut)
			{
				t = BraveMathCollege.LinearToSmoothStepInterpolate(0f, 1f, t);
			}
			duplicate1.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(AngleStart, 0f, t));
			duplicate2.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f - AngleStart, 0f, t));
			duplicate1.color = ColorGradient.Evaluate(t);
			duplicate2.color = ColorGradient.Evaluate(t);
			yield return null;
		}
	}

	private void Update()
	{
		m_lineTimer -= BraveTime.DeltaTime;
		m_totalTimer += BraveTime.DeltaTime;
		if (m_totalTimer < StopCreatingLinesTime)
		{
			if (lineMode == LineChargeMode.SEQUENTIAL_PARALLEL && m_lineTimer <= 0f)
			{
				StartCoroutine(HandleLine_SequentialParallel((!SequentialLinesReduceTraversalTime) ? LineTraversalTime : m_modTraversalTime));
				m_lineTimer += NewLineFrequency;
				m_modTraversalTime -= NewLineFrequency;
			}
			else if (lineMode == LineChargeMode.TRIANGULAR_CONVERGE && m_lineTimer <= 0f)
			{
				StartCoroutine(HandleLine_TriangularConverge((!SequentialLinesReduceTraversalTime) ? LineTraversalTime : m_modTraversalTime));
				m_lineTimer += NewLineFrequency;
				m_modTraversalTime -= NewLineFrequency;
			}
			else if (lineMode == LineChargeMode.PYRAMIDAL_CONVERGE && m_lineTimer <= 0f)
			{
				StartCoroutine(HandleLine_PyramidalConverge((!SequentialLinesReduceTraversalTime) ? LineTraversalTime : m_modTraversalTime));
				m_lineTimer += NewLineFrequency;
				m_modTraversalTime -= NewLineFrequency;
			}
			else if (lineMode == LineChargeMode.VERTICAL_CONVERGE && m_lineTimer <= 0f)
			{
				StartCoroutine(HandleLine_VerticalConverge((!SequentialLinesReduceTraversalTime) ? LineTraversalTime : m_modTraversalTime));
				m_lineTimer += NewLineFrequency;
				m_modTraversalTime -= NewLineFrequency;
			}
			else if (lineMode == LineChargeMode.SCALING_PARALLEL && m_lineTimer <= 0f)
			{
				StartCoroutine(HandleLine_ScalingParallel((!SequentialLinesReduceTraversalTime) ? LineTraversalTime : m_modTraversalTime));
				m_lineTimer += NewLineFrequency;
				m_modTraversalTime -= NewLineFrequency;
			}
		}
		else
		{
			m_hasConverged = true;
			if (DestroyedOnCompletion)
			{
				SpawnManager.Despawn(base.gameObject);
			}
		}
	}

	private void LateUpdate()
	{
		UpdateAngleAndLength();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
