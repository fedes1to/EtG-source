namespace FullInspector
{
	public class tkControlEditor
	{
		public bool Debug;

		public tkIControl Control;

		public object Context;

		public tkControlEditor(tkIControl control)
			: this(false, control)
		{
		}

		public tkControlEditor(bool debug, tkIControl control)
		{
			Debug = debug;
			Control = control;
			int nextId = 0;
			control.InitializeId(ref nextId);
		}
	}
}
