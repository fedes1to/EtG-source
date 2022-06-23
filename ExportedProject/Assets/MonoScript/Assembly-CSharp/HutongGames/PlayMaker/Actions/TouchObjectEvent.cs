using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionTarget(typeof(GameObject), "gameObject", false)]
	[ActionCategory(ActionCategory.Device)]
	[Tooltip("Sends events when an object is touched. Optionally filter by a fingerID. NOTE: Uses the MainCamera!")]
	public class TouchObjectEvent : FsmStateAction
	{
		[CheckForComponent(typeof(Collider))]
		[RequiredField]
		[Tooltip("The Game Object to detect touches on.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("How far from the camera is the Game Object pickable.")]
		[RequiredField]
		public FsmFloat pickDistance;

		[Tooltip("Only detect touches that match this fingerID, or set to None.")]
		public FsmInt fingerId;

		[Tooltip("Event to send on touch began.")]
		[ActionSection("Events")]
		public FsmEvent touchBegan;

		[Tooltip("Event to send on touch moved.")]
		public FsmEvent touchMoved;

		[Tooltip("Event to send on stationary touch.")]
		public FsmEvent touchStationary;

		[Tooltip("Event to send on touch ended.")]
		public FsmEvent touchEnded;

		[Tooltip("Event to send on touch cancel.")]
		public FsmEvent touchCanceled;

		[Tooltip("Store the fingerId of the touch.")]
		[UIHint(UIHint.Variable)]
		[ActionSection("Store Results")]
		public FsmInt storeFingerId;

		[Tooltip("Store the world position where the object was touched.")]
		[UIHint(UIHint.Variable)]
		public FsmVector3 storeHitPoint;

		[Tooltip("Store the surface normal vector where the object was touched.")]
		[UIHint(UIHint.Variable)]
		public FsmVector3 storeHitNormal;

		public override void Reset()
		{
			gameObject = null;
			pickDistance = 100f;
			fingerId = new FsmInt
			{
				UseVariable = true
			};
			touchBegan = null;
			touchMoved = null;
			touchStationary = null;
			touchEnded = null;
			touchCanceled = null;
			storeFingerId = null;
			storeHitPoint = null;
			storeHitNormal = null;
		}

		public override void OnUpdate()
		{
			if (Camera.main == null)
			{
				LogError("No MainCamera defined!");
				Finish();
			}
			else
			{
				if (Input.touchCount <= 0)
				{
					return;
				}
				GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
				if (ownerDefaultTarget == null)
				{
					return;
				}
				Touch[] touches = Input.touches;
				for (int i = 0; i < touches.Length; i++)
				{
					Touch touch = touches[i];
					if (!fingerId.IsNone && touch.fingerId != fingerId.Value)
					{
						continue;
					}
					Vector2 position = touch.position;
					RaycastHit hitInfo;
					Physics.Raycast(Camera.main.ScreenPointToRay(position), out hitInfo, pickDistance.Value);
					base.Fsm.RaycastHitInfo = hitInfo;
					if (hitInfo.transform != null && hitInfo.transform.gameObject == ownerDefaultTarget)
					{
						storeFingerId.Value = touch.fingerId;
						storeHitPoint.Value = hitInfo.point;
						storeHitNormal.Value = hitInfo.normal;
						switch (touch.phase)
						{
						case TouchPhase.Began:
							base.Fsm.Event(touchBegan);
							return;
						case TouchPhase.Moved:
							base.Fsm.Event(touchMoved);
							return;
						case TouchPhase.Stationary:
							base.Fsm.Event(touchStationary);
							return;
						case TouchPhase.Ended:
							base.Fsm.Event(touchEnded);
							return;
						case TouchPhase.Canceled:
							base.Fsm.Event(touchCanceled);
							return;
						}
					}
				}
			}
		}
	}
}
