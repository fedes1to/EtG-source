namespace BraveDynamicTree
{
	public class b2TreeNode
	{
		public b2AABB fatAabb;

		public b2AABB tightAabb;

		public SpeculativeRigidbody rigidbody;

		public int parent;

		public int next;

		public int child1;

		public int child2;

		public int height;

		public bool IsLeaf()
		{
			return child1 == -1;
		}
	}
}
