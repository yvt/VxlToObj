using System;
using System.Runtime.CompilerServices;

namespace VxlToObj.Core
{
	public struct Vector3
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public Vector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public float this[Axis3 axis]
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

