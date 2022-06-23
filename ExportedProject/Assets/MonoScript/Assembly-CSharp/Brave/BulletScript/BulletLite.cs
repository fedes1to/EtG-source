namespace Brave.BulletScript
{
	public class BulletLite : Bullet
	{
		public BulletLite(string bankName = null, bool suppressVfx = false, bool firstBulletOfAttack = false)
			: base(bankName, suppressVfx, firstBulletOfAttack)
		{
		}

		public override void Initialize()
		{
			m_tasks.Add(new TaskLite(this));
		}

		public virtual void Start()
		{
		}

		public virtual int Update(ref int state)
		{
			return Done();
		}

		protected int Done()
		{
			return -1;
		}
	}
}
