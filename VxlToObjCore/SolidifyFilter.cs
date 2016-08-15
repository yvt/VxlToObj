using System;
using System.Collections.Generic;

namespace VxlToObj.Core
{
	public class SolidifyFilter: IVoxelModelFilter
	{
		public SolidifyFilter()
		{
		}

		public void Apply(ref VoxelModel model)
		{
			var m = model;
			var ext = new ExteriorAnalyzer()
			{
				Model = m
			}.Analyze();

			// Find the initial points (solid voxels adjacent to interior empty voxels)
			var queue = new Queue<IntVector3>();
			int width = m.Width, height = m.Height, depth = m.Depth;
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					for (int z = 0; z < depth; ++z)
					{
						if (!model.IsVoxelSolid(x, y, z))
						{
							continue;
						}
						queue.Enqueue(new IntVector3(x, y, z));
					}
				}
			}
			while (queue.Count > 0)
			{
				var p = queue.Dequeue();
				uint color = m[p];
				if (p.X > 0)
				{
					Traverse(p.X - 1, p.Y, p.Z, m, ext, queue, color);
				}
				if (p.Y > 0)
				{
					Traverse(p.X, p.Y - 1, p.Z, m, ext, queue, color);
				}
				if (p.Z > 0)
				{
					Traverse(p.X, p.Y, p.Z - 1, m, ext, queue, color);
				}
				if (p.X < width - 1)
				{
					Traverse(p.X + 1, p.Y, p.Z, m, ext, queue, color);
				}
				if (p.Y < height - 1)
				{
					Traverse(p.X, p.Y + 1, p.Z, m, ext, queue, color);
				}
				if (p.Z < depth - 1)
				{
					Traverse(p.X, p.Y, p.Z + 1, m, ext, queue, color);
				}
			}
		}

		static void Traverse(int x, int y, int z, VoxelModel m, bool[,,] ext, Queue<IntVector3> queue, uint adjcolor)
		{
			if (m.IsVoxelSolid(x, y, z) || ext[x, y, z])
			{
				return;
			}
			m[x, y, z] = adjcolor;
			queue.Enqueue(new IntVector3(x, y, z));
		}
	}
}

