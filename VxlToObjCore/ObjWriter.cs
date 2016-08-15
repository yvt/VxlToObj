using System;
using System.Drawing;
using System.Linq;

namespace VxlToObj.Core
{
	public sealed class ObjWriter
	{
		public ObjWriter()
		{
		}

		public void Save(MeshSlices slices, Bitmap texture, string path, IProgressListener progress)
		{
			var name = System.IO.Path.GetFileNameWithoutExtension(path);
			var mtlname = name + ".mtl";
			var imgname = name + ".png";
			var dirname = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path));

			progress?.Report("Opening the output file");
			progress?.Report(0);

			using (var writer = System.IO.File.CreateText(path))
			{
				progress?.Report("Writing headers");

				writer.WriteLine($"mtllib {mtlname}");
				writer.WriteLine("o Object");

				progress?.Report("Writing position");
				int numTotalSlices = slices.Select((slicelist) => slicelist.Length).Sum();
				int sliceIndex = 0;
				foreach (var slicelist in slices)
				{
					foreach (var slice in slicelist)
					{
						foreach (var vt in slice.Positions)
						{
							// Usually Y is the upward direction for OBJs
							writer.WriteLine($"v {vt.X} {-vt.Z} {vt.Y}");
						}
						++sliceIndex;
						progress?.Report((double)sliceIndex / numTotalSlices * (1.0 / 3.0));
					}
				}

				progress?.Report("Writing UV coordinates");
				sliceIndex = 0;
				foreach (var slicelist in slices)
				{
					foreach (var slice in slicelist)
					{
						foreach (var uv in slice.UVs)
						{
							writer.WriteLine($"vt {uv.X / texture.Width} {1f - uv.Y / texture.Height}");
						}
						++sliceIndex;
						progress?.Report((double)sliceIndex / numTotalSlices * (1.0 / 3.0) + (1.0 / 3.0));
					}
				}

				writer.WriteLine("usemtl Material");
				writer.WriteLine("s off");

				progress?.Report("Writing faces");

				int vtxidx = 1;
				sliceIndex = 0;
				foreach (var slicelist in slices)
				{
					foreach (var slice in slicelist)
					{
						foreach (var f in slice.MeshSliceFaces)
						{
							writer.Write("f");
							for (int i = f.StartIndex; i < f.EndIndex; ++i)
							{
								writer.Write($" {vtxidx + i}/{vtxidx + i}");
							}
							writer.WriteLine();
						}
						vtxidx += slice.Positions.Length;

						++sliceIndex;
						progress?.Report((double)sliceIndex / numTotalSlices * (1.0 / 3.0) + (2.0 / 3.0));
					}
				}
			} // using writer

			progress?.Report("Saving material");

			using (var writer = System.IO.File.CreateText(System.IO.Path.Combine(dirname, mtlname)))
			{
				writer.WriteLine("newmtl Material");
				writer.WriteLine("Ka 0 0 0");
				writer.WriteLine("Kd 1 1 1");
				writer.WriteLine("Ks 0.2 0.2 0.2");
				writer.WriteLine("Ni 1");
				writer.WriteLine("d 1");
				writer.WriteLine("illum 2");
				writer.WriteLine($"map_Kd {imgname}");
			}

			progress?.Report("Saving texture");

			texture.Save(System.IO.Path.Combine(dirname, imgname),
						 System.Drawing.Imaging.ImageFormat.Png);
		}
	}
}

