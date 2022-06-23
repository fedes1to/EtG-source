namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sets the IsTriger property of a PixelCollider.")]
	[ActionCategory(".Brave")]
	public class SetPixelColliderIsTrigger : FsmStateAction
	{
		[Tooltip("If null, use self.")]
		public FsmGameObject targetObject;

		[Tooltip("PixelCollider index to set (0 indexed).")]
		public FsmInt colliderIndex;

		[Tooltip("The new value of the IsTrigger flag on the PixelCollider.")]
		public FsmBool isTriggerValue;

		public override void Reset()
		{
			colliderIndex = 0;
			isTriggerValue = false;
		}

		public override void OnEnter()
		{
			if (targetObject.Value == null)
			{
				TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
				component.specRigidbody.PixelColliders[colliderIndex.Value].IsTrigger = isTriggerValue.Value;
			}
			else
			{
				TalkDoerLite component2 = targetObject.Value.GetComponent<TalkDoerLite>();
				component2.specRigidbody.PixelColliders[colliderIndex.Value].IsTrigger = isTriggerValue.Value;
			}
			Finish();
		}
	}
}
