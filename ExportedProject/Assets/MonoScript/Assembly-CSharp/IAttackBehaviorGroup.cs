public interface IAttackBehaviorGroup
{
	int Count { get; }

	AttackBehaviorBase GetAttackBehavior(int index);
}
