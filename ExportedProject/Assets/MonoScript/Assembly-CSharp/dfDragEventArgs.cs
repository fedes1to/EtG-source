using UnityEngine;

public class dfDragEventArgs : dfControlEventArgs
{
	public dfDragDropState State { get; set; }

	public object Data { get; set; }

	public Vector2 Position { get; set; }

	public dfControl Target { get; set; }

	public Ray Ray { get; set; }

	internal dfDragEventArgs(dfControl source)
		: base(source)
	{
		State = dfDragDropState.None;
	}

	internal dfDragEventArgs(dfControl source, dfDragDropState state, object data, Ray ray, Vector2 position)
		: base(source)
	{
		Data = data;
		State = state;
		Position = position;
		Ray = ray;
	}
}
