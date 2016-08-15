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

		public void Apply(ref VoxelModel model)
		{
			var m = model;
			var ext = new ExteriorAnalyzer()
			{
				Model = m
			}.Analyze();
			var extexpanded = new DilationFilter()
			{
				Distance = Distance,
				DistanceType = DistanceType
			}.Apply(ext);

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
			}
		}
	}
}

