using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
internal sealed class _003C_003E__AnonType0<_003Cassembly_003E__T, _003Ctype_003E__T>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003Cassembly_003E__T _003Cassembly_003E;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003Ctype_003E__T _003Ctype_003E;

	public _003Cassembly_003E__T assembly
	{
		get
		{
			return _003Cassembly_003E;
		}
	}

	public _003Ctype_003E__T type
	{
		get
		{
			return _003Ctype_003E;
		}
	}

	[DebuggerHidden]
	public _003C_003E__AnonType0(_003Cassembly_003E__T assembly, _003Ctype_003E__T type)
	{
		_003Cassembly_003E = assembly;
		_003Ctype_003E = type;
	}

	[DebuggerHidden]
	public override bool Equals(object obj)
	{
		_003C_003E__AnonType0<_003Cassembly_003E__T, _003Ctype_003E__T> anon = obj as _003C_003E__AnonType0<_003Cassembly_003E__T, _003Ctype_003E__T>;
		return anon != null && EqualityComparer<_003Cassembly_003E__T>.Default.Equals(_003Cassembly_003E, anon._003Cassembly_003E) && EqualityComparer<_003Ctype_003E__T>.Default.Equals(_003Ctype_003E, anon._003Ctype_003E);
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		int num = (((-2128831035 ^ EqualityComparer<_003Cassembly_003E__T>.Default.GetHashCode(_003Cassembly_003E)) * 16777619) ^ EqualityComparer<_003Ctype_003E__T>.Default.GetHashCode(_003Ctype_003E)) * 16777619;
		num += num << 13;
		num ^= num >> 7;
		num += num << 3;
		num ^= num >> 17;
		return num + (num << 5);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		string[] obj = new string[6] { "{", " assembly = ", null, null, null, null };
		string text;
		if (_003Cassembly_003E != null)
		{
			_003Cassembly_003E__T val = _003Cassembly_003E;
			text = val.ToString();
		}
		else
		{
			text = string.Empty;
		}
		obj[2] = text;
		obj[3] = ", type = ";
		string text2;
		if (_003Ctype_003E != null)
		{
			_003Ctype_003E__T val2 = _003Ctype_003E;
			text2 = val2.ToString();
		}
		else
		{
			text2 = string.Empty;
		}
		obj[4] = text2;
		obj[5] = " }";
		return string.Concat(obj);
	}
}
