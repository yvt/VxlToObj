﻿using System;
namespace VxlToObj
{
	public sealed class Kv6VoxelModelLoader
	{
		public Kv6VoxelModelLoader()
		{
		}

		struct Kv6Block
		{
			public uint color;
			public int zpos;
		}

		public VoxelModel LoadVoxelModel(System.IO.Stream stream, out Vector3 pivot)
		{
			var reader = new System.IO.BinaryReader(stream);

			{
				var buf = new byte[4];
				if (stream.Read(buf, 0, 4) < 4) 
				{
					throw new System.IO.IOException("Magic not read");
				}
				if (buf[0] != 'K' ||
				   buf[1] != 'v' ||
				   buf[2] != 'x' ||
				   buf[3] != 'l') 
				{
					throw new System.IO.IOException("Invalid magic");
				}
			}

			int xsiz = reader.ReadInt32();
			int ysiz = reader.ReadInt32();
			int zsiz = reader.ReadInt32();
			float xpivot = reader.ReadSingle();
			float ypivot = reader.ReadSingle();
			float zpivot = reader.ReadSingle();
			int numblocks = reader.ReadInt32();
			var blocks = new Kv6Block[numblocks];

			for (int i = 0; i < blocks.Length; ++i)
			{
				blocks[i].color = reader.ReadUInt32();
				blocks[i].zpos = (int) reader.ReadUInt16();
				reader.ReadUInt16(); // skip visFaces & lighting
			}

			var xyoffset = new int[xsiz * ysiz];

			// skip xoffset
			for (int i = 0; i < xsiz; ++i)
			{
				reader.ReadInt32();
			}
			for (int i = 0; i < xyoffset.Length; ++i)
			{
				xyoffset[i] = (int) reader.ReadUInt16();
			}

			int pos = 0;
			var model = new VoxelModel(xsiz, ysiz, zsiz);
			for (int x = 0; x < xsiz; ++x) {
				for (int y = 0; y < ysiz; ++y) {
					int sb = xyoffset[x * ysiz + y];
					for (int i = 0; i < sb; ++i) {
						var b = blocks[pos];
						model[x, y, b.zpos] = b.color;
						pos += 1;
					}
				}
			}

			pivot = new Vector3(xpivot, ypivot, zpivot);

			return model;
		}

		public VoxelModel LoadVoxelModel(System.IO.Stream stream)
		{
			Vector3 dummy;
			return LoadVoxelModel(stream, out dummy);
		}

		public VoxelModel LoadVoxelModel(byte[] bytes, out Vector3 pivot)
		{
			return LoadVoxelModel(new System.IO.MemoryStream(bytes, false), out pivot);
		}

		public VoxelModel LoadVoxelModel(byte[] bytes)
		{
			return LoadVoxelModel(new System.IO.MemoryStream(bytes, false));
		}
	}
}

