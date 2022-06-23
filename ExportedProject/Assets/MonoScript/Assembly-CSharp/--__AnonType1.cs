using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
internal sealed class _003C_003E__AnonType1<_003CDist_003E__T, _003CPair_003E__T>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CDist_003E__T _003CDist_003E;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CPair_003E__T _003CPair_003E;

	public _003CDist_003E__T Dist
	{
		get
		{
			return _003CDist_003E;
		}
	}

	public _003CPair_003E__T Pair
	{
		get
		{
			return _003CPair_003E;
		}
	}

	[DebuggerHidden]
	public _003C_003E__AnonType1(_003CDist_003E__T Dist, _003CPair_003E__T Pair)
	{
		_003CDist_003E = Dist;
		_003CPair_003E = Pair;
	}

	[DebuggerHidden]
	public override bool Equals(object obj)
	{
		_003C_003E__AnonType1<_003CDist_003E__T, _003CPair_003E__T> anon = obj as _003C_003E__AnonType1<_003CDist_003E__T, _003CPair_003E__T>;
		return anon != null && EqualityComparer<_003CDist_003E__T>.Default.Equals(_003CDist_003E, anon._003CDist_003E) && EqualityComparer<_003CPair_003E__T>.Default.Equals(_003CPair_003E, anon._003CPair_003E);
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		int num = (((-2128831035 ^ EqualityComparer<_003CDist_003E__T>.Default.GetHashCode(_003CDist_003E)) * 16777619) ^ EqualityComparer<_003CPair_003E__T>.Default.GetHashCode(_003CPair_003E)) * 16777619;
		num += num << 13;
		num ^= num >> 7;
		num += num << 3;
		num ^= num >> 17;
		return num + (num << 5);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		string[] obj = new string[6] { "{", " Dist = ", null, null, null, null };
		string text;
		if (_003CDist_003E != null)
		{
			_003CDist_003E__T val = _003CDist_003E;
			text = val.ToString();
		}
		else
		{
			text = string.Empty;
		}
		obj[2] = text;
		obj[3] = ", Pair = ";
		string text2;
		if (_003CPair_003E != null)
		{
			_003CPair_003E__T val2 = _003CPair_003E;
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
