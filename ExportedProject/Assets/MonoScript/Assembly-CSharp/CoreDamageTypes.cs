using System;

[Flags]
public enum CoreDamageTypes
{
	None = 0,
	Void = 1,
	Magic = 2,
	Fire = 4,
	Ice = 8,
	Poison = 0x10,
	Water = 0x20,
	Electric = 0x40,
	SpecialBossDamage = 0x80
}
