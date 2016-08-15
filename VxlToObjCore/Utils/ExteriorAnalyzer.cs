using System;
using System.Collections.Generic;

namespace VxlToObj.Core
{
	/// <summary>
	/// Determines the inside/outside part of the voxel model.
	/// </summary>
	sealed class ExteriorAnalyzer
	{
		public VoxelModel Model { get; set; }

		bool[,,] exterior;
		Queue<IntVector3> queue;

		public ExteriorAnalyzer()
		{
		}

		public bool[,,] Analyze(IProgressListener progress)
		{
			int width = Model.Width;
			int height = Model.Height;
			int depth = Model.Depth;
			exterior = new bool[width, height, depth];

			queue = new Queue<IntVector3>();
			var model = this.Model;

			// Start the scan from the boundary
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					Traverse(x, y, 0);
					Traverse(x, y, depth - 1);
				}
			}
			for (int x = 0; x < width; ++x)
			{
				for (int z = 0; z < depth; ++z)
				{
					Traverse(x, 0, z);
					Traverse(x, height - 1, z);
				}
			}
			for (int y = 0; y < height; ++y)
			{
				for (int z = 0; z < depth; ++z)
				{
					Traverse(0, y, z);
					Traverse(width - 1, y, z);
				}
			}

			int numProcessed = 0;

			while (queue.Count > 0)
			{
				var p = queue.Dequeue();
				if (p.X > 0)
				{
					Traverse(p.X - 1, p.Y, p.Z);
				}
				if (p.X < width - 1)
				{
					Traverse(p.X + 1, p.Y, p.Z);
				}
				if (p.Y > 0)
				{
					Traverse(p.X, p.Y - 1, p.Z);
				}
				if (p.Y < height - 1)
				{
					Traverse(p.X, p.Y + 1, p.Z);
				}
				if (p.Z > 0)
				{
					Traverse(p.X, p.Y, p.Z - 1);
				}
				if (p.Z < depth - 1)
				{
					Traverse(p.X, p.Y, p.Z + 1);
				}

				++numProcessed;

				// this might move the progress bar in a funny way but
				// we can't do better since we don't know how many exterior voxels are there yet
				progress?.Report((double)numProcessed / (numProcessed + queue.Count));
			}

			var ret = exterior;
			exterior = null;
			return ret;
		}

		void Traverse(int x, int y, int z)
		{
			if (!exterior[x, y, z] && !Model.IsVoxelSolid(x, y, z))
			{
				exterior[x, y, z] = true;
				queue.Enqueue(new IntVector3(x, y, z));
			}
		}

	}
}

