using System;
using System.Collections.Generic;

namespace VxlToObj.Core
{
	public class SolidifyFilter: IVoxelModelFilter
	{
		public SolidifyFilter()
		{
		}

		public void Apply(ref VoxelModel model, IProgressListener progress)
		{
			var m = model;

			progress?.Report("Analyzing structure");
			var ext = new ExteriorAnalyzer()
			{
				Model = m
			}.Analyze(new ProgressMapper(progress, 0, 1.0 / 3.0, null));

			// Find the initial points (solid voxels adjacent to interior empty voxels)
			progress?.Report("Planting seeds");
			var queue = new Queue<IntVector3>();
			int width = m.Width, height = m.Height, depth = m.Depth;
			int numVoxelsToProcess = 0;
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					for (int z = 0; z < depth; ++z)
					{
						if (!ext[x, y, z] && !model.IsVoxelSolid(x, y, z))
						{
							++numVoxelsToProcess;
						}
						if (!model.IsVoxelSolid(x, y, z))
						{
							continue;
						}
						queue.Enqueue(new IntVector3(x, y, z));
					}
				}
				progress?.Report((double)(x + 1) / width * (1.0 / 3.0) + (1.0 / 3.0));
			}
			numVoxelsToProcess += queue.Count;

			progress?.Report("Filling inside");
			int numProcessed = 0;
			while (queue.Count > 0)
			{
				++numProcessed;
				if ((numProcessed & 2047) == 0)
				{
					progress?.Report((double)numProcessed / numVoxelsToProcess * (1.0 / 3.0) + (2.0 / 3.0));
				}

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

