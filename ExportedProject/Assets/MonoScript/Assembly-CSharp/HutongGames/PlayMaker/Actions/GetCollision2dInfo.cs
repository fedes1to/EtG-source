namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Gets info on the last collision 2D event and store in variables. See Unity and PlayMaker docs on Unity 2D physics.")]
	[ActionCategory(ActionCategory.Physics2D)]
	public class GetCollision2dInfo : FsmStateAction
	{
		[Tooltip("Get the GameObject hit.")]
		[UIHint(UIHint.Variable)]
		public FsmGameObject gameObjectHit;

		[Tooltip("Get the relative velocity of the collision.")]
		[UIHint(UIHint.Variable)]
		public FsmVector3 relativeVelocity;

		[Tooltip("Get the relative speed of the collision. Useful for controlling reactions. E.g., selecting an appropriate sound fx.")]
		[UIHint(UIHint.Variable)]
		public FsmFloat relativeSpeed;

		[Tooltip("Get the world position of the collision contact. Useful for spawning effects etc.")]
		[UIHint(UIHint.Variable)]
		public FsmVector3 contactPoint;

		[Tooltip("Get the collision normal vector. Useful for aligning spawned effects etc.")]
		[UIHint(UIHint.Variable)]
		public FsmVector3 contactNormal;

		[Tooltip("The number of separate shaped regions in the collider.")]
		[UIHint(UIHint.Variable)]
		public FsmInt shapeCount;

		[UIHint(UIHint.Variable)]
		[Tooltip("Get the name of the physics 2D material of the colliding GameObject. Useful for triggering different effects. Audio, particles...")]
		public FsmString physics2dMaterialName;

		public override void Reset()
		{
			gameObjectHit = null;
			relativeVelocity = null;
			relativeSpeed = null;
			contactPoint = null;
			contactNormal = null;
			shapeCount = null;
			physics2dMaterialName = null;
		}

		private void StoreCollisionInfo()
		{
			if (base.Fsm.Collision2DInfo != null)
			{
				gameObjectHit.Value = base.Fsm.Collision2DInfo.gameObject;
				relativeSpeed.Value = base.Fsm.Collision2DInfo.relativeVelocity.magnitude;
				relativeVelocity.Value = base.Fsm.Collision2DInfo.relativeVelocity;
				physics2dMaterialName.Value = ((!(base.Fsm.Collision2DInfo.collider.sharedMaterial != null)) ? string.Empty : base.Fsm.Collision2DInfo.collider.sharedMaterial.name);
				shapeCount.Value = base.Fsm.Collision2DInfo.collider.shapeCount;
				if (base.Fsm.Collision2DInfo.contacts != null && base.Fsm.Collision2DInfo.contacts.Length > 0)
				{
					contactPoint.Value = base.Fsm.Collision2DInfo.contacts[0].point;
					contactNormal.Value = base.Fsm.Collision2DInfo.contacts[0].normal;
				}
			}
		}

		public override void OnEnter()
		{
			StoreCollisionInfo();
			Finish();
		}
	}
}
