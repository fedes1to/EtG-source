using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/NegativeSpacePie1")]
public class InfinilichNegativeSpacePie1 : Script
{
	public class RayBullet : Bullet
	{
		public bool ShouldDestroy;

		public int DestroyDirection;

		public float Angle;

		private RayBullet m_leadBullet;

		private int m_spawnDelay;

		private Vector2 m_origin;

		private bool m_doTell;

		public bool DoTell
		{
			get
			{
				return m_doTell;
			}
			set
			{
				if (m_doTell != value)
				{
					if (value)
					{
						Projectile.spriteAnimator.Play();
					}
					else
					{
						Projectile.spriteAnimator.StopAndResetFrameToDefault();
					}
					m_doTell = value;
				}
			}
		}

		public RayBullet(RayBullet leadBullet, float angle, int spawnDelay, Vector2 origin)
			: base("pieBullet")
		{
			m_leadBullet = leadBullet;
			Angle = angle;
			m_spawnDelay = spawnDelay;
			m_origin = origin;
		}

		protected override IEnumerator Top()
		{
			float radius = (base.Position - m_origin).magnitude;
			float Magnitude = Random.Range(1.1f, 1.4f);
			float Period = Random.Range(1.35f, 1.65f);
			if (m_spawnDelay < 80)
			{
				yield return Wait(80 - m_spawnDelay);
			}
			float startingOffset = Random.value;
			base.ManualControl = true;
			int i = 0;
			while (true)
			{
				if (ShouldDestroy || (m_leadBullet != null && m_leadBullet.ShouldDestroy))
				{
					radius += 0.25f;
				}
				if (m_leadBullet != null)
				{
					DoTell = m_leadBullet.DoTell;
					Angle = m_leadBullet.Angle;
				}
				float offsetMagnitude = Mathf.SmoothStep(0f - Magnitude, Magnitude, Mathf.PingPong(startingOffset + (float)i / 60f * Period, 1f));
				if (i < 60)
				{
					offsetMagnitude *= (float)i / 60f;
				}
				base.Position = m_origin + BraveMathCollege.DegreesToVector(Angle, radius + offsetMagnitude);
				i++;
				yield return Wait(1);
			}
		}
	}

	private const int NumRays = 56;

	private const float SafeDegrees = 45f;

	private const int NumBullets = 14;

	private const float RayLength = 22f;

	private const int SetupTime = 80;

	private const float SpinSpeed = 0.42f;

	private const int NumTransitions = 6;

	private const int MidTransitionTime = 35;

	private const int TransitionTellTime = 30;

	private const int ForwardCount = 4;

	private const int ForwardTransitionTime = 150;

	private const int BackwardCount = 1;

	private const int BackwardTransitionTime = 90;

	private static float DeltaRay = 5.72727251f;

	private static float SafeEndAngle;

	private PooledLinkedList<RayBullet> m_leadBullets = new PooledLinkedList<RayBullet>();

	private bool m_done;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		float aim = base.AimDirection;
		int j = 0;
		SafeEndAngle = aim + 180f + 27.5f * DeltaRay;
		int halfRays = 28;
		for (int i = 0; i < halfRays; i++)
		{
			float angle2 = aim + 180f - ((float)i + 0.5f) * DeltaRay;
			RayBullet leadBullet2 = SpawnRay(angle2, j);
			m_leadBullets.AddFirst(leadBullet2);
			angle2 = aim + 180f + ((float)i + 0.5f) * DeltaRay;
			leadBullet2 = SpawnRay(angle2, j);
			m_leadBullets.AddLast(leadBullet2);
			j++;
			int remainingRays = halfRays - i;
			yield return Wait((remainingRays >= 5) ? 1 : (10 - remainingRays * 2));
		}
		yield return Wait(52);
		StartTask(HandleGaps());
		j = 0;
		while (!m_done)
		{
			float currentSpeed = 0.42f;
			if (BraveMathCollege.ClampAngle180(base.AimDirection - SafeEndAngle) < -3f)
			{
				currentSpeed *= 0.5f;
			}
			GameActor target = base.BulletBank.aiActor.PlayerTarget;
			if ((bool)target && target.IsFalling)
			{
				currentSpeed = 0f;
			}
			float deltaAngle = ((j >= 90) ? currentSpeed : Mathf.Lerp(0f, currentSpeed, (float)j / 90f));
			SafeEndAngle += deltaAngle;
			int destroyedCount = 0;
			for (LinkedListNode<RayBullet> node = m_leadBullets.First; node != null; node = node.Next)
			{
				node.Value.Angle += deltaAngle;
				if (node.Value.Destroyed)
				{
					destroyedCount++;
				}
			}
			j++;
			yield return Wait(1);
		}
		for (LinkedListNode<RayBullet> node = m_leadBullets.First; node != null; node = node.Next)
		{
			node.Value.ShouldDestroy = true;
		}
		m_leadBullets.Clear();
		ForceEnd();
	}

	private RayBullet SpawnRay(float angle, int spawnDelay)
	{
		RayBullet rayBullet = null;
		for (int i = 0; i < 14; i++)
		{
			RayBullet rayBullet2 = new RayBullet(rayBullet, angle, spawnDelay, base.Position);
			if (rayBullet == null)
			{
				rayBullet = rayBullet2;
			}
			Fire(new Offset(Mathf.Lerp(1.5f, 22f, (float)i / 13f), 0f, angle, string.Empty), new Speed(), rayBullet2);
		}
		return rayBullet;
	}

	private IEnumerator HandleGaps()
	{
		int lastRotationDirection = -1;
		for (int i = 0; i < 6; i++)
		{
			yield return Wait(5);
			int rotateDirection = ((lastRotationDirection <= 0) ? Mathf.RoundToInt(BraveUtility.RandomSign()) : (-1));
			lastRotationDirection = rotateDirection;
			if (lastRotationDirection > 0)
			{
				LinkedListNode<RayBullet> linkedListNode = m_leadBullets.Last;
				for (int l = 0; l < 4; l++)
				{
					linkedListNode.Value.DoTell = true;
					linkedListNode = linkedListNode.Previous;
				}
			}
			else
			{
				LinkedListNode<RayBullet> node3 = m_leadBullets.First;
				for (int m = 0; m < 1; m++)
				{
					node3.Value.DoTell = true;
					node3 = node3.Next;
				}
				yield return Wait(1);
			}
			yield return Wait(30);
			if (rotateDirection > 0)
			{
				SafeEndAngle -= DeltaRay * 4f;
				for (int k = 0; k < 150; k++)
				{
					LinkedListNode<RayBullet> node2 = m_leadBullets.Last;
					for (int n = 0; n < 4; n++)
					{
						node2.Value.Angle += (45f - DeltaRay) / 150f;
						node2 = node2.Previous;
					}
					yield return Wait(1);
				}
				for (int num = 0; num < 4; num++)
				{
					LinkedListNode<RayBullet> last = m_leadBullets.Last;
					m_leadBullets.Remove(last, false);
					m_leadBullets.AddFirst(last);
					last.Value.DoTell = false;
				}
				continue;
			}
			for (int j = 0; j < 90; j++)
			{
				LinkedListNode<RayBullet> node = m_leadBullets.First;
				for (int num2 = 0; num2 < 1; num2++)
				{
					node.Value.Angle -= (45f - DeltaRay) / 90f;
					node = node.Next;
				}
				yield return Wait(1);
			}
			for (int num3 = 0; num3 < 1; num3++)
			{
				LinkedListNode<RayBullet> first = m_leadBullets.First;
				m_leadBullets.Remove(first, false);
				m_leadBullets.AddLast(first);
				first.Value.DoTell = false;
			}
			SafeEndAngle += DeltaRay * 1f;
		}
		m_done = true;
	}
}
