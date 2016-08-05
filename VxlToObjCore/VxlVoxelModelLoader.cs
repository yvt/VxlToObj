using System;
namespace VxlToObj.Core
{
	public sealed class VxlVoxelModelLoader
	{
		public VxlVoxelModelLoader()
		{
		}

		public VoxelModel LoadVoxelModel(byte[] bytes)
		{
			var model = new VoxelModel(512, 512, 64);

			int pos = 0;

			for (int y = 0; y < 512; ++y)
			{
				for (int x = 0; x < 512; ++x)
				{
					int z = 0;
					uint color = 0;
					while (true)
					{
						int number_4byte_chunks = bytes[pos];
						int top_color_start = bytes[pos + 1];
						int top_color_end = bytes[pos + 2];
						int bottom_color_start;
						int bottom_color_end;
						int len_top;
						int len_bottom;

						int colorpos = pos + 4;
						for (z = top_color_start; z <= top_color_end; z++)
						{
							color = BitConverter.ToUInt32(bytes, colorpos);
							colorpos += 4;
							model[x, y, z] = color;
						}

						if (top_color_end == 62)
						{
							model[x, y, 63] = model[x, y, 62];
						}

						len_bottom = top_color_end - top_color_start + 1;

						if (number_4byte_chunks == 0)
						{
							pos += 4 * (len_bottom + 1);
							break;
						}

						len_top = (number_4byte_chunks - 1) - len_bottom;

						pos += (int) bytes[pos] * 4;

						bottom_color_end = bytes[pos + 3];
						bottom_color_start = bottom_color_end - len_top;

						for (z = bottom_color_start; z < bottom_color_end; z++)
						{
							color = BitConverter.ToUInt32(bytes, colorpos);
							colorpos += 4;
							model[x, y, z] = color;
						}
						if (bottom_color_end == 63)
						{
							model[x, y, 63] = model[x, y, 62];
						}
					} // while (true)
				} // for x
			} // for y

			return model;
		}
	}
}

