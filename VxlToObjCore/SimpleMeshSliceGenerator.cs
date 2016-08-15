using System;
using System.Collections.Generic;

namespace VxlToObj.Core
{
	public sealed class SimpleMeshSliceGenerator
	{
		public SimpleMeshSliceGenerator()
		{
		}

		public MeshSlices GenerateSlices(VoxelModel model, IProgressListener progress)
		{
			var slices = new MeshSlices();
			var dims = model.Dimensions;

			int totalsteps = 2 * (model.Width + model.Height + model.Depth);
			int step = 0;

			progress?.Report("Generating slices");

			for (int iaxis = 0; iaxis < 3; ++iaxis)
			{
				for (int iface = 0; iface < 2; ++iface)
				{

					var axis = (Axis3) iaxis;
					var paxis1 = (Axis3) ((iaxis + 1) % 3);
					var paxis2 = (Axis3) ((iaxis + 2) % 3);
					bool face = iface != 0;

					int dim0 = dims[axis];
					int dim1 = dims[paxis1];
					int dim2 = dims[paxis2];

					var slicelist = new MeshSlice[dim0];
					slices[axis, face] = slicelist;

					for (int layer = 0; layer < dim0; ++layer)
					{
						++step;
						progress?.Report((double)step / totalsteps);

						var faces = new List<int>();
						var ret = new List<IntVector3>();

						var pt1 = new IntVector3();
						pt1[axis] = layer;
						for (int x = 0; x < dim1; ++x)
						{
							pt1[paxis1] = x;
							for (int y = 0; y < dim2; ++y)
							{
								pt1[paxis2] = y;

								bool solid1 = model.IsVoxelSolid(pt1);

								var pt2 = pt1;
								bool solid2 = false;
								if (face)
								{
									pt2[axis] += 1;
									if (pt2[axis] < dim0)
									{
										solid2 = model.IsVoxelSolid(pt2);
									}
								}
								else
								{
									pt2[axis] -= 1;
									if (pt2[axis] >= 0)
									{
										solid2 = model.IsVoxelSolid(pt2);
									}
								}

								if (!solid1 || solid2)
								{
									continue;
								}

								// Create quad
								var qpt1 = pt1;
								if (face)
								{
									qpt1[axis] += 1;
									qpt1[paxis2] += 1;
								}
								var qpt2 = qpt1;
								qpt2[paxis1] += 1;
								var qpt3 = qpt1;
								var qpt4 = qpt2;
								if (face)
								{
									qpt3[paxis2] -= 1;
									qpt4[paxis2] -= 1;
								}
								else
								{
									qpt3[paxis2] += 1;
									qpt4[paxis2] += 1;
								}

								// Emit polygons
								faces.Add(ret.Count);
								ret.Add(qpt3);
								ret.Add(qpt4);
								ret.Add(qpt2);
								ret.Add(qpt1);
							} // for y
						} // for x

						slicelist[layer] = new MeshSlice()
						{
							Positions = ret.ToArray(),
							Faces = faces.ToArray(),

							Face = face,
							Axis = axis,
							Layer = layer
						};

					} // for slice
				} // iface
			} // iaxis
			return slices;
		}
	}
}

