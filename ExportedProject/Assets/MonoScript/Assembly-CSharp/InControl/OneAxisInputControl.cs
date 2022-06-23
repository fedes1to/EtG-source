using System;
using UnityEngine;

namespace InControl
{
	public class OneAxisInputControl : IInputControl
	{
		private float sensitivity = 1f;

		private float lowerDeadZone;

		private float upperDeadZone = 1f;

		private float stateThreshold;

		public float FirstRepeatDelay = 0.8f;

		public float RepeatDelay = 0.1f;

		public bool Raw;

		internal bool Enabled = true;

		internal bool Suppressed;

		private ulong pendingTick;

		private bool pendingCommit;

		private float nextRepeatTime;

		private float lastPressedTime;

		private float lastReleasedTime;

		private bool wasRepeated;

		private bool clearInputState;

		private InputControlState lastState;

		private InputControlState nextState;

		private InputControlState thisState;

		private float startRepeatTime;

		private float nextTimeForRepeat;

		public ulong UpdateTick { get; protected set; }

		public bool State
		{
			get
			{
				return Enabled && !Suppressed && thisState.State;
			}
		}

		public bool LastState
		{
			get
			{
				return Enabled && !Suppressed && lastState.State;
			}
			set
			{
				lastState.State = value;
			}
		}

		public float Value
		{
			get
			{
				return (!Enabled || Suppressed) ? 0f : thisState.Value;
			}
		}

		public float LastValue
		{
			get
			{
				return (!Enabled || Suppressed) ? 0f : lastState.Value;
			}
		}

		public float RawValue
		{
			get
			{
				return (!Enabled || Suppressed) ? 0f : thisState.RawValue;
			}
		}

		internal float NextRawValue
		{
			get
			{
				return (!Enabled || Suppressed) ? 0f : nextState.RawValue;
			}
		}

		public bool HasChanged
		{
			get
			{
				return Enabled && !Suppressed && thisState != lastState;
			}
		}

		public bool IsPressed
		{
			get
			{
				return Enabled && !Suppressed && thisState.State;
			}
		}

		public bool WasPressed
		{
			get
			{
				return Enabled && !Suppressed && (bool)thisState && !lastState;
			}
		}

		public bool WasPressedRepeating
		{
			get
			{
				if ((bool)thisState)
				{
					if (!lastState)
					{
						nextTimeForRepeat = Time.realtimeSinceStartup + 0.5f;
						startRepeatTime = Time.realtimeSinceStartup;
						return true;
					}
					if (Time.realtimeSinceStartup > startRepeatTime + 5f)
					{
						return false;
					}
					if (Time.realtimeSinceStartup >= nextTimeForRepeat)
					{
						nextTimeForRepeat = Time.realtimeSinceStartup + 0.1f;
						return true;
					}
				}
				return false;
			}
		}

		public bool WasPressedAsDpad
		{
			get
			{
				return Enabled && !Suppressed && thisState.RawValue >= 0.5f && lastState.RawValue < 0.5f;
			}
		}

		public bool WasPressedAsDpadRepeating
		{
			get
			{
				if (thisState.RawValue >= 0.5f)
				{
					if (lastState.RawValue < 0.5f)
					{
						nextTimeForRepeat = Time.realtimeSinceStartup + 0.5f;
						startRepeatTime = Time.realtimeSinceStartup;
						return true;
					}
					if (Time.realtimeSinceStartup > startRepeatTime + 5f)
					{
						return false;
					}
					if (Time.realtimeSinceStartup >= nextTimeForRepeat)
					{
						nextTimeForRepeat = Time.realtimeSinceStartup + 0.1f;
						return true;
					}
				}
				return false;
			}
		}

		public float PressedDuration
		{
			get
			{
				if (lastReleasedTime > lastPressedTime)
				{
					return lastReleasedTime - lastPressedTime;
				}
				return Time.realtimeSinceStartup - lastPressedTime;
			}
		}

		public bool WasReleased
		{
			get
			{
				return Enabled && !Suppressed && !thisState && (bool)lastState;
			}
		}

		public bool WasRepeated
		{
			get
			{
				return Enabled && !Suppressed && wasRepeated;
			}
		}

		public float Sensitivity
		{
			get
			{
				return sensitivity;
			}
			set
			{
				sensitivity = Mathf.Clamp01(value);
			}
		}

		public float LowerDeadZone
		{
			get
			{
				return lowerDeadZone;
			}
			set
			{
				lowerDeadZone = Mathf.Clamp01(value);
			}
		}

		public float UpperDeadZone
		{
			get
			{
				return upperDeadZone;
			}
			set
			{
				upperDeadZone = Mathf.Clamp01(value);
			}
		}

		public float StateThreshold
		{
			get
			{
				return stateThreshold;
			}
			set
			{
				stateThreshold = Mathf.Clamp01(value);
			}
		}

		public bool IsNull
		{
			get
			{
				return object.ReferenceEquals(this, InputControl.Null);
			}
		}

		private void PrepareForUpdate(ulong updateTick)
		{
			if (!IsNull)
			{
				if (updateTick < pendingTick)
				{
					throw new InvalidOperationException("Cannot be updated with an earlier tick.");
				}
				if (pendingCommit && updateTick != pendingTick)
				{
					throw new InvalidOperationException("Cannot be updated for a new tick until pending tick is committed.");
				}
				if (updateTick > pendingTick)
				{
					lastState = thisState;
					nextState.Reset();
					pendingTick = updateTick;
					pendingCommit = true;
				}
			}
		}

		public bool UpdateWithState(bool state, ulong updateTick, float deltaTime)
		{
			if (IsNull)
			{
				return false;
			}
			PrepareForUpdate(updateTick);
			nextState.Set(state || nextState.State);
			return state;
		}

		public bool UpdateWithValue(float value, ulong updateTick, float deltaTime)
		{
			if (IsNull)
			{
				return false;
			}
			PrepareForUpdate(updateTick);
			if (Utility.Abs(value) > Utility.Abs(nextState.RawValue))
			{
				nextState.RawValue = value;
				if (!Raw)
				{
					value = Utility.ApplyDeadZone(value, lowerDeadZone, upperDeadZone);
				}
				nextState.Set(value, stateThreshold);
				return true;
			}
			return false;
		}

		internal bool UpdateWithRawValue(float value, ulong updateTick, float deltaTime)
		{
			if (IsNull)
			{
				return false;
			}
			Raw = true;
			PrepareForUpdate(updateTick);
			if (Utility.Abs(value) > Utility.Abs(nextState.RawValue))
			{
				nextState.RawValue = value;
				nextState.Set(value, stateThreshold);
				return true;
			}
			return false;
		}

		internal void SetValue(float value, ulong updateTick)
		{
			if (!IsNull)
			{
				if (updateTick > pendingTick)
				{
					lastState = thisState;
					nextState.Reset();
					pendingTick = updateTick;
					pendingCommit = true;
				}
				nextState.RawValue = value;
				nextState.Set(value, StateThreshold);
			}
		}

		public void ClearInputState()
		{
			lastState.Reset();
			thisState.Reset();
			nextState.Reset();
			wasRepeated = false;
			clearInputState = true;
		}

		public void Commit()
		{
			if (IsNull)
			{
				return;
			}
			pendingCommit = false;
			thisState = nextState;
			if (Suppressed && !thisState.State)
			{
				ClearInputState();
				Suppressed = false;
			}
			if (clearInputState)
			{
				lastState = nextState;
				UpdateTick = pendingTick;
				clearInputState = false;
				return;
			}
			bool state = lastState.State;
			bool state2 = thisState.State;
			wasRepeated = false;
			if (state && !state2)
			{
				nextRepeatTime = 0f;
				lastReleasedTime = Time.realtimeSinceStartup;
			}
			else if (state2)
			{
				if (state != state2)
				{
					lastPressedTime = Time.realtimeSinceStartup;
					nextRepeatTime = Time.realtimeSinceStartup + FirstRepeatDelay;
				}
				else if (Time.realtimeSinceStartup >= nextRepeatTime)
				{
					wasRepeated = true;
					nextRepeatTime = Time.realtimeSinceStartup + RepeatDelay;
				}
			}
			if (thisState != lastState)
			{
				UpdateTick = pendingTick;
			}
		}

		public void CommitWithState(bool state, ulong updateTick, float deltaTime)
		{
			UpdateWithState(state, updateTick, deltaTime);
			Commit();
		}

		public void CommitWithValue(float value, ulong updateTick, float deltaTime)
		{
			UpdateWithValue(value, updateTick, deltaTime);
			Commit();
		}

		internal void CommitWithSides(InputControl negativeSide, InputControl positiveSide, ulong updateTick, float deltaTime)
		{
			LowerDeadZone = Mathf.Max(negativeSide.LowerDeadZone, positiveSide.LowerDeadZone);
			UpperDeadZone = Mathf.Min(negativeSide.UpperDeadZone, positiveSide.UpperDeadZone);
			Raw = negativeSide.Raw || positiveSide.Raw;
			float value = Utility.ValueFromSides(negativeSide.RawValue, positiveSide.RawValue);
			CommitWithValue(value, updateTick, deltaTime);
		}

		public void ResetRepeating()
		{
			nextTimeForRepeat = Time.realtimeSinceStartup + 0.5f;
		}

		public void Suppress()
		{
			Suppressed = true;
		}

		public static implicit operator bool(OneAxisInputControl instance)
		{
			return instance.State;
		}

		public static implicit operator float(OneAxisInputControl instance)
		{
			return instance.Value;
		}
	}
}
