using System;

[Flags]
public enum dfAnchorStyle
{
	Top = 1,
	Bottom = 2,
	Left = 4,
	Right = 8,
	All = 0xF,
	CenterHorizontal = 0x40,
	CenterVertical = 0x80,
	Proportional = 0x100,
	None = 0
}
