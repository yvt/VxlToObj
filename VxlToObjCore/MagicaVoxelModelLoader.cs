using System;
using System.Collections.Generic;
namespace VxlToObj.Core
{
	public class MagicaVoxelModelLoader
	{
		static uint FourCCToUInt32(string cc)
		{
			return BitConverter.ToUInt32(new System.Text.ASCIIEncoding().GetBytes(cc), 0);
		}

		static class FourCCs
		{
			public static readonly uint Vox = FourCCToUInt32("VOX ");
			public static readonly uint Main = FourCCToUInt32("MAIN");
			public static readonly uint Size = FourCCToUInt32("SIZE");
			public static readonly uint Xyzi = FourCCToUInt32("XYZI");
			public static readonly uint Rgba = FourCCToUInt32("RGBA");
		}

		sealed class Chunk
		{
			public uint ChunkID { get; set; }
			public ArraySegment<byte> Contents { get; set; }
			public Chunk[] Children { get; set; }

			public static Chunk LoadChunk(ArraySegment<byte> bytes)
			{
				uint id = BitConverter.ToUInt32(bytes.Array, bytes.Offset);
				int contentsSize = BitConverter.ToInt32(bytes.Array, bytes.Offset + 4);
				int childrenSize = BitConverter.ToInt32(bytes.Array, bytes.Offset + 8);

				var children = new List<Chunk>();
				for (int offs = 0; offs < childrenSize;)
				{
					var child = LoadChunk(new ArraySegment<byte>(
						bytes.Array, bytes.Offset + 12 + offs, bytes.Count - 12 - offs));
					children.Add(child);

					int childSize = BitConverter.ToInt32(bytes.Array,
														 bytes.Offset + 12 + offs + 4);
					childSize += BitConverter.ToInt32(bytes.Array,
													  bytes.Offset + 12 + offs + 8);
					childSize += 12;
					offs += childSize;
				}

				return new Chunk()
				{
					ChunkID = id,
					Contents = new ArraySegment<byte>(
						bytes.Array, bytes.Offset + 12 + childrenSize, contentsSize),
					Children = children.ToArray()
				};
			}
		}

		public MagicaVoxelModelLoader()
		{
		}

		public VoxelModel LoadVoxelModel(byte[] bytes, IProgressListener progress)
		{
			uint magic = BitConverter.ToUInt32(bytes, 0);
			// int version = BitConverter.ToInt32(bytes, 4);
			if (magic != FourCCs.Vox)
			{
				throw new System.IO.IOException("Invalid magic number (wrong file format?).");
			}
			// FIXME: should we check the version number?

			var mainChunk = Chunk.LoadChunk(new ArraySegment<byte>(bytes, 8, bytes.Length - 8));
			if (mainChunk.ChunkID != FourCCs.Main)
			{
				throw new System.IO.IOException("File is corrupted. Bad root chunk ID (should be MAIN).");
			}

			Chunk sizeChunk = null;
			Chunk voxelChunk = null;
			Chunk paletteChunk = null;
			foreach (var chunk in mainChunk.Children)
			{
				if (chunk.ChunkID == FourCCs.Size)
				{
					sizeChunk = chunk;
				}
				else if (chunk.ChunkID == FourCCs.Xyzi)
				{
					voxelChunk = chunk;
				}
				else if (chunk.ChunkID == FourCCs.Rgba)
				{
					paletteChunk = chunk;
				}
			}

			if (sizeChunk == null)
			{
				throw new System.IO.IOException("File is corrupted. SIZE chunk was not found.");
			}
			if (voxelChunk == null)
			{
				throw new System.IO.IOException("File is corrupted. XYZI chunk was not found.");
			}
			if (paletteChunk == null)
			{
				throw new System.IO.IOException("VOX file without a palette (RGBA chunk) is currently not supported.");
			}

			// Parse size
			if (sizeChunk.Contents.Count < 12)
			{
				throw new System.IO.IOException("File is corrupted. SIZE chunk is too short.");
			}
			int dimX = BitConverter.ToInt32(sizeChunk.Contents.Array, sizeChunk.Contents.Offset);
			int dimY = BitConverter.ToInt32(sizeChunk.Contents.Array, sizeChunk.Contents.Offset + 4);
			int dimZ = BitConverter.ToInt32(sizeChunk.Contents.Array, sizeChunk.Contents.Offset + 8);
			if (dimX <= 0 || dimY <= 0 || dimZ <= 0)
			{
				throw new System.IO.IOException("File is corrupted. Bad dimensions.");
			}

			// Read palette
			var palette = new uint[256];
			{
				var paldata = paletteChunk.Contents;
				if (paldata.Count < 256 * 4)
				{
					throw new System.IO.IOException("File is corrupted. RGBA chunk is too short.");
				}
				for (int i = 0; i < 255; ++i)
				{
					uint r = paldata.Array[paldata.Offset + i * 4];
					uint g = paldata.Array[paldata.Offset + i * 4 + 1];
					uint b = paldata.Array[paldata.Offset + i * 4 + 2];
					palette[i + 1] = b | (g << 8) | (r << 16);
				}
			}

			// Read geometry
			var model = new VoxelModel(dimX, dimY, dimZ);
			int numVoxels = BitConverter.ToInt32(voxelChunk.Contents.Array, voxelChunk.Contents.Offset);
			if (voxelChunk.Contents.Count / 4 < numVoxels + 1)
			{
				throw new System.IO.IOException("File is corrupted. XYZI chunk is too short.");
			}
			progress?.Report("Reading voxels");
			{
				var data = voxelChunk.Contents;
				int end = data.Offset + 4 + numVoxels * 4;
				for (int i = data.Offset + 4; i < end; i += 4)
				{
					int x = data.Array[i];
					int y = data.Array[i + 1];
					int z = data.Array[i + 2];
					int color = data.Array[i + 3];
					if (x < 0 || x >= dimX || y < 0 || y >= dimY || z < 0 || z >= dimZ)
					{
						throw new System.IO.IOException("File is corrupted. Voxel location is out of bounds.");
					}
					model[x, y, z] = palette[color];
					if (((i & 8191) == 0))
					{
						progress?.Report((double)(i - data.Offset) / (double)(numVoxels * 4));
					}
				}
			}
			return model;
		}
	}
}

