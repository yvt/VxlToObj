﻿using System;
using System.Runtime.CompilerServices;

namespace VxlToObj.Core
{
	public sealed class VoxelModel
	{
		private readonly int width, height, depth;
		private readonly uint[] data;

		public const uint EmptyVoxel = 0xffffffff;
		
		public VoxelModel(int width, int height, int depth)
		{
			this.width = width;
			this.height = height;
			this.depth = depth;
			data = new uint[width * height * depth];
			for (int i = 0; i < data.Length; ++i)
			{
				data[i] = EmptyVoxel;
			}
		}

		public int Width => width;
		public int Height => height;
		public int Depth => depth;

		public IntVector3 Dimensions =>
			new IntVector3(width, height, depth);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIndexForVoxel(int x, int y, int z)
		{
			return (x * height + y) * depth + z;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetIndexForVoxel(IntVector3 p)
		{
			return (p.X * height + p.Y) * depth + p.Z;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsVoxelSolid(int x, int y, int z)
		{
			return this[x, y, z] != EmptyVoxel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsVoxelSolid(IntVector3 p)
		{
			return this[p] != EmptyVoxel;
		}

		public uint this[int x, int y, int z]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return data[GetIndexForVoxel(x, y, z)];
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				data[GetIndexForVoxel(x, y, z)] = value;
			}
		}

		public uint this[IntVector3 pos]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return data[GetIndexForVoxel(pos)];
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				data[GetIndexForVoxel(pos)] = value;
			}
		}
	}
}

