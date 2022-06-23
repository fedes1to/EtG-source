using UnityEngine;

public class dfKeyEventArgs : dfControlEventArgs
{
	public KeyCode KeyCode { get; set; }

	public char Character { get; set; }

	public bool Control { get; set; }

	public bool Shift { get; set; }

	public bool Alt { get; set; }

	internal dfKeyEventArgs(dfControl source, KeyCode Key, bool Control, bool Shift, bool Alt)
		: base(source)
	{
		KeyCode = Key;
		this.Control = Control;
		this.Shift = Shift;
		this.Alt = Alt;
	}

	public override string ToString()
	{
		return string.Format("Key: {0}, Control: {1}, Shift: {2}, Alt: {3}", KeyCode, Control, Shift, Alt);
	}
}
