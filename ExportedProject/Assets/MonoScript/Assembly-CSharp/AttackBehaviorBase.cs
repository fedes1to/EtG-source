public abstract class AttackBehaviorBase : BehaviorBase
{
	public abstract bool IsReady();

	public abstract float GetMinReadyRange();

	public abstract float GetMaxRange();
}
