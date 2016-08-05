using System;
using System.Runtime.CompilerServices;

namespace VxlToObj.Core
{
	public enum Axis3
	{
		X = 0, Y, Z
	}

	public struct IntVector3
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }

		public IntVector3(int x, int y, int z)
		{
			X = x; Y = y; Z = z;
		}

		public int this[Axis3 axis]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				switch (axis)
				{
					case Axis3.X: return X;
					case Axis3.Y: return Y;
					case Axis3.Z: return Z;
					default: throw new ArgumentOutOfRangeException(nameof(axis));
				}
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				switch (axis)
				{
					case Axis3.X: X = value; break;
					case Axis3.Y: Y = value; break;
					case Axis3.Z: Z = value; break;
					default: throw new ArgumentOutOfRangeException(nameof(axis));
				}
			}
		}
	}
}

