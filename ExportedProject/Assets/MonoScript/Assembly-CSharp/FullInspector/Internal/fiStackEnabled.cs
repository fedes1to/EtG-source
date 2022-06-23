namespace FullInspector.Internal
{
	public class fiStackEnabled
	{
		private int _count;

		public bool Enabled
		{
			get
			{
				return _count > 0;
			}
		}

		public void Push()
		{
			_count++;
		}

		public void Pop()
		{
			_count--;
			if (_count < 0)
			{
				_count = 0;
			}
		}
	}
}
