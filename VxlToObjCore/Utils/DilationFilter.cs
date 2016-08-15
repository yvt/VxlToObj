using System;
namespace VxlToObj.Core
{
	/// <summary>
	/// Applies the dilation filter on <code>bool[,,]</code>.
	/// </summary>
	sealed class DilationFilter
	{
		public DilationFilter()
		{
			Distance = 1;
			DistanceType = DistanceType.Chebyshev;
		}

		public int Distance { get; set; }
		public DistanceType DistanceType { get; set; }

		public bool[,,] Apply(bool[,,] field, IProgressListener progress)
		{
			int d1 = field.GetLength(0);
			int d2 = field.GetLength(1);
			int d3 = field.GetLength(2);

			bool[,,] ret = new bool[d1, d2, d3];
			int dist = Distance;
			DistanceType type = DistanceType;

			for (int x = 0; x < d1; ++x)
			{
				for (int y = 0; y < d2; ++y)
				{
					for (int z = 0; z < d3; ++z)
					{
						switch (type)
						{
							case DistanceType.Manhattan:
								for (int s = -dist; s <= dist; ++s)
								{
									for (int t = -dist; t <= dist; ++t)
									{
										for (int u = -dist; u <= dist; ++u)
										{
											if (Math.Abs(s) + Math.Abs(t) + Math.Abs(u) > dist)
											{
												continue;
											}
											int cx = s + x, cy = t + y, cz = u + z;
											if (cx < 0 || cy < 0 || cz < 0 || cx >= d1 || cy >= d2 || cz >= d3)
											{
												continue;
											}
											if (field[cx, cy, cz])
											{
												ret[x, y, z] = true;
												goto DoneOne;
											}
										}
									}
								}
								break;
							case DistanceType.Chebyshev:
								for (int s = -dist; s <= dist; ++s)
								{
									for (int t = -dist; t <= dist; ++t)
									{
										for (int u = -dist; u <= dist; ++u)
										{
											int cx = s + x, cy = t + y, cz = u + z;
											if (cx < 0 || cy < 0 || cz < 0 || cx >= d1 || cy >= d2 || cz >= d3)
											{
												continue;
											}
											if (field[cx, cy, cz])
											{
												ret[x, y, z] = true;
												goto DoneOne;
											}
										}
									}
								}
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					DoneOne:;
					}
				}
				progress?.Report((double)(x + 1) / d1);
			}

			return ret;
		}
	}
}

