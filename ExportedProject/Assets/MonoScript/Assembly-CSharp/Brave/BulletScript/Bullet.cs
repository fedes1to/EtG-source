using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Brave.BulletScript
{
	public class Bullet
	{
		public enum DestroyType
		{
			DieInAir,
			HitRigidbody,
			HitTile
		}

		protected interface ITask
		{
			void Tick(out bool isFinished);
		}

		protected class Task : ITask
		{
			private int m_wait;

			private IEnumerator m_currentEnum;

			private List<IEnumerator> m_enumStack;

			public Task(IEnumerator enumerator)
			{
				m_currentEnum = enumerator;
			}

			public void Tick(out bool isFinished)
			{
				if (m_wait > 0)
				{
					m_wait--;
					isFinished = false;
					return;
				}
				if (!m_currentEnum.MoveNext())
				{
					if (m_enumStack == null || m_enumStack.Count == 1)
					{
						isFinished = true;
						return;
					}
					m_enumStack.RemoveAt(m_enumStack.Count - 1);
					m_currentEnum = m_enumStack[m_enumStack.Count - 1];
					Tick(out isFinished);
					return;
				}
				isFinished = false;
				object current = m_currentEnum.Current;
				if (current is int)
				{
					m_wait = (int)current - 1;
				}
				else
				{
					if (current == null)
					{
						return;
					}
					if (current is IEnumerator)
					{
						if (m_enumStack == null)
						{
							m_enumStack = new List<IEnumerator>();
							m_enumStack.Add(m_currentEnum);
						}
						m_enumStack.Add(current as IEnumerator);
						m_currentEnum = current as IEnumerator;
						Tick(out isFinished);
					}
					else
					{
						Debug.LogError("Unknown return type from BulletScript: " + current);
					}
				}
			}
		}

		protected class TaskLite : ITask
		{
			private int m_wait;

			private BulletLite m_currentBullet;

			private int m_state;

			public TaskLite(BulletLite bullet)
			{
				m_currentBullet = bullet;
			}

			public void Tick(out bool isFinished)
			{
				if (m_wait > 0)
				{
					m_wait--;
					isFinished = false;
					return;
				}
				if (m_currentBullet.Tick == 0)
				{
					m_currentBullet.Start();
				}
				int num = m_currentBullet.Update(ref m_state);
				if (num == -1)
				{
					isFinished = true;
					return;
				}
				isFinished = false;
				m_wait = num - 1;
			}
		}

		public string BankName;

		public string SpawnTransform;

		public float Direction;

		public float Speed;

		public Vector2 Velocity;

		public bool AutoRotation;

		public float TimeScale = 1f;

		public IBulletManager BulletManager;

		public GameObject Parent;

		public Projectile Projectile;

		public bool DontDestroyGameObject;

		protected readonly List<ITask> m_tasks = new List<ITask>();

		private float m_timer;

		private bool m_hasFiredBullet;

		private const float c_idealFrameTime = 1f / 60f;

		public Transform RootTransform { get; set; }

		public Vector2 Position { get; set; }

		public Vector2 PredictedPosition
		{
			get
			{
				return new Vector2(Position.x + m_timer * Velocity.x, Position.y + m_timer * Velocity.y);
			}
		}

		public AIBulletBank BulletBank
		{
			get
			{
				return BulletManager as AIBulletBank;
			}
		}

		public bool ManualControl { get; set; }

		public bool DisableMotion { get; set; }

		public bool Destroyed { get; set; }

		public bool SuppressVfx { get; set; }

		public bool FirstBulletOfAttack { get; set; }

		public bool ForceBlackBullet { get; set; }

		public bool EndOnBlank { get; set; }

		public int Tick { get; set; }

		public float AimDirection
		{
			get
			{
				return (BulletManager.PlayerPosition() - Position).ToAngle();
			}
		}

		public bool IsOwnerAlive
		{
			get
			{
				AIActor aIActor = null;
				if ((bool)BulletBank)
				{
					aIActor = BulletBank.aiActor;
				}
				if (!aIActor && (bool)Projectile)
				{
					aIActor = Projectile.Owner as AIActor;
				}
				return (bool)aIActor && (bool)aIActor.healthHaver && aIActor.healthHaver.IsAlive;
			}
		}

		private float LocalDeltaTime
		{
			get
			{
				if ((bool)Projectile)
				{
					return Projectile.LocalDeltaTime;
				}
				if ((bool)BulletBank && (bool)BulletBank.aiActor)
				{
					return BulletBank.aiActor.LocalDeltaTime;
				}
				return BraveTime.DeltaTime;
			}
		}

		public bool IsEnded
		{
			get
			{
				for (int i = 0; i < m_tasks.Count; i++)
				{
					if (m_tasks[i] != null)
					{
						return false;
					}
				}
				return true;
			}
		}

		public Bullet(string bankName = null, bool suppressVfx = false, bool firstBulletOfAttack = false, bool forceBlackBullet = false)
		{
			BankName = bankName;
			SuppressVfx = suppressVfx;
			FirstBulletOfAttack = firstBulletOfAttack;
			ForceBlackBullet = forceBlackBullet;
		}

		public virtual void Initialize()
		{
			IEnumerator enumerator = Top();
			if (enumerator != null)
			{
				m_tasks.Add(new Task(enumerator));
			}
		}

		public void FrameUpdate()
		{
			m_timer += LocalDeltaTime * TimeScale * Projectile.EnemyBulletSpeedMultiplier;
			while (m_timer > 1f / 60f)
			{
				m_timer -= 1f / 60f;
				DoTick();
			}
		}

		public void DoTick()
		{
			for (int i = 0; i < m_tasks.Count; i++)
			{
				if (m_tasks[i] != null)
				{
					bool isFinished;
					m_tasks[i].Tick(out isFinished);
					if (isFinished && i < m_tasks.Count)
					{
						m_tasks[i] = null;
					}
				}
			}
			Tick++;
			if (!ManualControl)
			{
				UpdateVelocity();
				UpdatePosition();
			}
		}

		public void HandleBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool allowProjectileSpawns)
		{
			Destroyed = true;
			bool flag = !allowProjectileSpawns;
			flag |= destroyType == DestroyType.HitRigidbody && (bool)hitRigidbody && (bool)hitRigidbody.GetComponent<PlayerOrbital>();
			OnBulletDestruction(destroyType, hitRigidbody, flag);
		}

		public virtual void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
		}

		public void ForceEnd()
		{
			OnForceEnded();
			m_tasks.Clear();
		}

		public virtual void OnForceEnded()
		{
		}

		public virtual void OnForceRemoved()
		{
		}

		public float GetAimDirection(string transform)
		{
			Vector2 vector = BulletManager.TransformOffset(Position, transform);
			Vector2 vector2 = BulletManager.PlayerPosition();
			return (vector2 - vector).ToAngle();
		}

		public float GetAimDirection(float leadAmount, float speed)
		{
			return GetAimDirection(Position, leadAmount, speed);
		}

		public float GetAimDirection(string transform, float leadAmount, float speed)
		{
			return GetAimDirection(BulletManager.TransformOffset(Position, transform), leadAmount, speed);
		}

		public float GetAimDirection(Vector2 position, float leadAmount, float speed)
		{
			Vector2 targetOrigin = BulletManager.PlayerPosition();
			Vector2 predictedPosition = BraveMathCollege.GetPredictedPosition(targetOrigin, BulletManager.PlayerVelocity(), position, speed);
			targetOrigin = new Vector2(targetOrigin.x + (predictedPosition.x - targetOrigin.x) * leadAmount, targetOrigin.y + (predictedPosition.y - targetOrigin.y) * leadAmount);
			return (targetOrigin - position).ToAngle();
		}

		public Vector2 GetPredictedTargetPosition(float leadAmount, float speed)
		{
			Vector2 position = Position;
			Vector2 targetOrigin = BulletManager.PlayerPosition();
			Vector2 predictedPosition = BraveMathCollege.GetPredictedPosition(targetOrigin, BulletManager.PlayerVelocity(), position, speed);
			return new Vector2(targetOrigin.x + (predictedPosition.x - targetOrigin.x) * leadAmount, targetOrigin.y + (predictedPosition.y - targetOrigin.y) * leadAmount);
		}

		public Vector2 GetPredictedTargetPositionExact(float leadAmount, float speed)
		{
			BulletBank.SuppressPlayerVelocityAveraging = true;
			Vector2 position = Position;
			Vector2 targetOrigin = BulletManager.PlayerPosition();
			Vector2 predictedPosition = BraveMathCollege.GetPredictedPosition(targetOrigin, BulletManager.PlayerVelocity(), position, speed);
			targetOrigin = new Vector2(targetOrigin.x + (predictedPosition.x - targetOrigin.x) * leadAmount, targetOrigin.y + (predictedPosition.y - targetOrigin.y) * leadAmount);
			BulletBank.SuppressPlayerVelocityAveraging = false;
			return targetOrigin;
		}

		public void PostWwiseEvent(string AudioEvent, string SwitchName = null)
		{
			if ((bool)BulletBank)
			{
				BulletBank.PostWwiseEvent(AudioEvent, SwitchName);
			}
		}

		public void Fire(Bullet bullet = null)
		{
			Fire(null, null, null, bullet);
		}

		public void Fire(Offset offset = null, Bullet bullet = null)
		{
			Fire(offset, null, null, bullet);
		}

		public void Fire(Offset offset = null, Speed speed = null, Bullet bullet = null)
		{
			Fire(offset, null, speed, bullet);
		}

		public void Fire(Offset offset = null, Direction direction = null, Bullet bullet = null)
		{
			Fire(offset, direction, null, bullet);
		}

		public void Fire(Direction direction = null, Bullet bullet = null)
		{
			Fire(null, direction, null, bullet);
		}

		public void Fire(Direction direction = null, Speed speed = null, Bullet bullet = null)
		{
			Fire(null, direction, speed, bullet);
		}

		public void Fire(Offset offset = null, Direction direction = null, Speed speed = null, Bullet bullet = null)
		{
			if (bullet == null)
			{
				bullet = new Bullet();
			}
			if (!m_hasFiredBullet)
			{
				bullet.FirstBulletOfAttack = true;
			}
			bullet.BulletManager = BulletManager;
			if (this is Script)
			{
				bullet.RootTransform = RootTransform;
			}
			bullet.Position = Position;
			bullet.Direction = Direction;
			bullet.Speed = Speed;
			bullet.m_timer = m_timer - LocalDeltaTime;
			bullet.EndOnBlank = EndOnBlank;
			float? overrideBaseDirection = null;
			if (offset != null)
			{
				overrideBaseDirection = offset.GetDirection(this);
				if (!string.IsNullOrEmpty(offset.transform))
				{
					bullet.SpawnTransform = offset.transform;
					Transform transform = BulletBank.GetTransform(offset.transform);
					if ((bool)transform)
					{
						bullet.RootTransform = transform;
					}
				}
			}
			bullet.Position = ((offset == null) ? Position : offset.GetPosition(this));
			bullet.Direction = ((direction == null) ? 0f : direction.GetDirection(this, overrideBaseDirection));
			bullet.Speed = ((speed == null) ? 0f : speed.GetSpeed(this));
			BulletManager.BulletSpawnedHandler(bullet);
			if ((bool)Projectile && Projectile.IsBlackBullet && (bool)bullet.Projectile)
			{
				bullet.Projectile.ForceBlackBullet = true;
				bullet.Projectile.BecomeBlackBullet();
			}
			m_hasFiredBullet = true;
		}

		protected void ChangeSpeed(Speed speed, int term = 1)
		{
			if (term <= 1)
			{
				Speed = speed.GetSpeed(this);
			}
			else
			{
				m_tasks.Add(new Task(ChangeSpeedTask(speed, term)));
			}
		}

		protected void ChangeDirection(Direction direction, int term = 1)
		{
			if (term <= 1)
			{
				Direction = direction.GetDirection(this);
			}
			else
			{
				m_tasks.Add(new Task(ChangeDirectionTask(direction, term)));
			}
		}

		protected void StartTask(IEnumerator enumerator)
		{
			m_tasks.Add(new Task(enumerator));
		}

		protected int Wait(int frames)
		{
			return frames;
		}

		protected int Wait(float frames)
		{
			return Mathf.CeilToInt(frames);
		}

		public void Vanish(bool suppressInAirEffects = false)
		{
			Destroyed = true;
			BulletManager.DestroyBullet(this, suppressInAirEffects);
		}

		protected virtual IEnumerator Top()
		{
			return null;
		}

		protected void UpdateVelocity()
		{
			float f = Direction * ((float)Math.PI / 180f);
			Velocity.x = Mathf.Cos(f) * Speed;
			Velocity.y = Mathf.Sin(f) * Speed;
		}

		protected void UpdatePosition()
		{
			Vector2 position = Position;
			position.x += Velocity.x / 60f;
			position.y += Velocity.y / 60f;
			Position = position;
		}

		protected float RandomAngle()
		{
			return UnityEngine.Random.Range(0, 360);
		}

		protected float SubdivideRange(float startValue, float endValue, int numDivisions, int i, bool offset = false)
		{
			return Mathf.Lerp(startValue, endValue, ((float)i + ((!offset) ? 0f : 0.5f)) / (float)(numDivisions - 1));
		}

		protected float SubdivideArc(float startAngle, float sweepAngle, int numBullets, int i, bool offset = false)
		{
			return startAngle + Mathf.Lerp(0f, sweepAngle, ((float)i + ((!offset) ? 0f : 0.5f)) / (float)(numBullets - 1));
		}

		protected float SubdivideCircle(float startAngle, int numBullets, int i, float direction = 1f, bool offset = false)
		{
			return startAngle + direction * Mathf.Lerp(0f, 360f, ((float)i + ((!offset) ? 0f : 0.5f)) / (float)numBullets);
		}

		protected bool IsPointInTile(Vector2 point)
		{
			if (!GameManager.Instance.Dungeon.data.CheckInBoundsAndValid((int)point.x, (int)point.y))
			{
				return true;
			}
			int rayMask = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker);
			SpeculativeRigidbody result;
			return PhysicsEngine.Instance.Pointcast(point, out result, true, false, rayMask, CollisionLayer.Projectile, false);
		}

		private IEnumerator ChangeSpeedTask(Speed speed, int term)
		{
			float delta = ((speed.type != SpeedType.Sequence) ? ((speed.GetSpeed(this) - Speed) / (float)term) : speed.speed);
			for (int i = 0; i < term; i++)
			{
				Speed += delta;
				yield return Wait(1);
			}
		}

		private IEnumerator ChangeDirectionTask(Direction direction, int term)
		{
			float delta = ((direction.type != DirectionType.Sequence) ? (BraveMathCollege.ClampAngle180(direction.GetDirection(this) - Direction) / (float)term) : direction.direction);
			if (direction.maxFrameDelta >= 0f)
			{
				delta = Mathf.Clamp(delta, 0f - direction.maxFrameDelta, direction.maxFrameDelta);
			}
			for (int i = 0; i < term; i++)
			{
				Direction += delta;
				yield return Wait(1);
			}
		}
	}
}
