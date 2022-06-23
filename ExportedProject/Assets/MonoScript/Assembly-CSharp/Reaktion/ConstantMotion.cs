using System;
using UnityEngine;

namespace Reaktion
{
	public class ConstantMotion : MonoBehaviour
	{
		public enum TransformMode
		{
			Off,
			XAxis,
			YAxis,
			ZAxis,
			Arbitrary,
			Random
		}

		[Serializable]
		public class TransformElement
		{
			public TransformMode mode;

			public float velocity = 1f;

			public Vector3 arbitraryVector = Vector3.up;

			public float randomness;

			private Vector3 randomVector;

			private float randomScalar;

			public Vector3 Vector
			{
				get
				{
					switch (mode)
					{
					case TransformMode.XAxis:
						return Vector3.right;
					case TransformMode.YAxis:
						return Vector3.up;
					case TransformMode.ZAxis:
						return Vector3.forward;
					case TransformMode.Arbitrary:
						return arbitraryVector;
					case TransformMode.Random:
						return randomVector;
					default:
						return Vector3.zero;
					}
				}
			}

			public float Delta
			{
				get
				{
					float num = 1f - randomness * randomScalar;
					return velocity * num * BraveTime.DeltaTime;
				}
			}

			public void Initialize()
			{
				randomVector = UnityEngine.Random.onUnitSphere;
				randomScalar = UnityEngine.Random.value;
			}
		}

		public TransformElement position = new TransformElement();

		public TransformElement rotation = new TransformElement
		{
			velocity = 30f
		};

		public bool useLocalCoordinate = true;

		private void Awake()
		{
			position.Initialize();
			rotation.Initialize();
		}

		private void Update()
		{
			if (position.mode != 0)
			{
				if (useLocalCoordinate)
				{
					base.transform.localPosition += position.Vector * position.Delta;
				}
				else
				{
					base.transform.position += position.Vector * position.Delta;
				}
			}
			if (rotation.mode != 0)
			{
				Quaternion quaternion = Quaternion.AngleAxis(rotation.Delta, rotation.Vector);
				if (useLocalCoordinate)
				{
					base.transform.localRotation = quaternion * base.transform.localRotation;
				}
				else
				{
					base.transform.rotation = quaternion * base.transform.rotation;
				}
			}
		}
	}
}
