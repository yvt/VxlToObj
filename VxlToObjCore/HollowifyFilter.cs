using System;
namespace VxlToObj.Core
{
	public class HollowifyFilter: IVoxelModelFilter
	{
		public HollowifyFilter()
		{
			Distance = 1;
			DistanceType = DistanceType.Chebyshev;
			IsExteriorSolid = false;
		}

		public int Distance { get; set; }
		public DistanceType DistanceType { get; set; }
		public bool IsExteriorSolid { get; set; }

		public void Apply(ref VoxelModel model, IProgressListener progress)
		{
			var m = model;

			progress?.Report("Analyzing structure");
			var ext = new ExteriorAnalyzer()
			{
				Model = m
			}.Analyze(new ProgressMapper(progress, 0.0, 1.0 / 3.0, null));

			progress?.Report("Creating shell");
			var extexpanded = new DilationFilter()
			{
				Distance = Distance,
				DistanceType = DistanceType
			}.Apply(ext, new ProgressMapper(progress, 1.0 / 3.0, 1.0 / 3.0, null));

			progress?.Report("Removing invisible voxels");

			int d1 = m.Width;
			int d2 = m.Height;
			int d3 = m.Depth;
			int d = IsExteriorSolid ? 0 : Distance;
			for (int x = d; x < d1 - d; ++x)
			{
				for (int y = d; y < d2 - d; ++y)
				{
					for (int z = d; z < d3 - d; ++z)
					{
						if (!extexpanded[x, y, z])
						{
							m[x, y, z] = VoxelModel.EmptyVoxel;
						}
					}
				}
				progress?.Report((double)(x - d) / (d1 - d * 2) * (1.0 / 3.0) + 2.0 / 3.0);
			}
		}
	}
}

