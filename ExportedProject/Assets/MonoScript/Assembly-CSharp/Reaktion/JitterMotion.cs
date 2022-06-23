using System;
using UnityEngine;

namespace Reaktion
{
	public class JitterMotion : MonoBehaviour
	{
		public bool AllowCameraInfluence;

		public float InfluenceAxialX = 20f;

		public float InfluenceAxialY = 20f;

		public float positionFrequency = 0.2f;

		public float rotationFrequency = 0.2f;

		public float positionAmount = 1f;

		public float rotationAmount = 30f;

		public Vector3 positionComponents = Vector3.one;

		public Vector3 rotationComponents = new Vector3(1f, 1f, 0f);

		public int positionOctave = 3;

		public int rotationOctave = 3;

		public bool UseMainCameraShakeAmount = true;

		private float timePosition;

		private float timeRotation;

		private Vector2[] noiseVectors;

		private Vector3 initialPosition;

		private Quaternion initialRotation;

		private Vector2 m_currentInfluenceVec = Vector2.zero;

		public Vector3 GetInitialPosition()
		{
			return initialPosition;
		}

		public Quaternion GetInitialRotation()
		{
			return initialRotation;
		}

		private void Awake()
		{
			timePosition = UnityEngine.Random.value * 10f;
			timeRotation = UnityEngine.Random.value * 10f;
			noiseVectors = new Vector2[6];
			for (int i = 0; i < 6; i++)
			{
				float f = UnityEngine.Random.value * (float)Math.PI * 2f;
				noiseVectors[i].Set(Mathf.Cos(f), Mathf.Sin(f));
			}
			initialPosition = base.transform.localPosition;
			initialRotation = base.transform.localRotation;
		}

		private void Update()
		{
			timePosition += BraveTime.DeltaTime * positionFrequency;
			timeRotation += BraveTime.DeltaTime * rotationFrequency;
			if (positionAmount != 0f)
			{
				Vector3 a = new Vector3(Fbm(noiseVectors[0] * timePosition, positionOctave), Fbm(noiseVectors[1] * timePosition, positionOctave), Fbm(noiseVectors[2] * timePosition, positionOctave));
				a = Vector3.Scale(a, positionComponents) * positionAmount * 2f;
				base.transform.localPosition = initialPosition + a + GameManager.Instance.MainCameraController.ScreenShakeVector * 5f;
			}
			if (rotationAmount != 0f)
			{
				Vector3 a2 = new Vector3(Fbm(noiseVectors[3] * timeRotation, rotationOctave), Fbm(noiseVectors[4] * timeRotation, rotationOctave), Fbm(noiseVectors[5] * timeRotation, rotationOctave));
				a2 = Vector3.Scale(a2, rotationComponents) * rotationAmount * 2f;
				base.transform.localRotation = Quaternion.Euler(a2) * initialRotation;
			}
			if (!AllowCameraInfluence)
			{
				return;
			}
			Vector2 target = Vector2.zero;
			float num = 0f;
			float num2 = 0f;
			if (BraveInput.PrimaryPlayerInstance != null)
			{
				if (BraveInput.PrimaryPlayerInstance.IsKeyboardAndMouse())
				{
					target = GameManager.Instance.MainCameraController.Camera.ScreenToViewportPoint(Input.mousePosition);
					target = (target + new Vector2(-0.5f, -0.5f)) * 2f;
				}
				else
				{
					target = BraveInput.PrimaryPlayerInstance.ActiveActions.Aim.Vector;
				}
			}
			m_currentInfluenceVec = Vector2.MoveTowards(m_currentInfluenceVec, target, 1.25f * GameManager.INVARIANT_DELTA_TIME);
			num = m_currentInfluenceVec.x * InfluenceAxialX;
			num2 = m_currentInfluenceVec.y * InfluenceAxialY * -1f;
			base.transform.RotateAround(base.transform.position + base.transform.forward * 10f, Vector3.up, num);
			base.transform.RotateAround(base.transform.position + base.transform.forward * 10f, Vector3.right, num2);
		}

		public static float Fbm(Vector2 coord, int octave)
		{
			float num = 0f;
			float num2 = 1f;
			for (int i = 0; i < octave; i++)
			{
				num += num2 * (Mathf.PerlinNoise(coord.x, coord.y) - 0.5f);
				coord *= 2f;
				num2 *= 0.5f;
			}
			return num;
		}
	}
}
