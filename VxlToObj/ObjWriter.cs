using System;
using System.Drawing;

namespace VxlToObj
{
	public sealed class ObjWriter
	{
		public ObjWriter()
		{
		}

		public void Save(MeshSlices slices, Bitmap texture, string path)
		{
			var name = System.IO.Path.GetFileNameWithoutExtension(path);
			var mtlname = name + ".mtl";
			var imgname = name + ".png";
			var dirname = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path));

			using (var writer = System.IO.File.CreateText(path))
			{
				writer.WriteLine($"mtllib {mtlname}");
				writer.WriteLine("o Object");

				foreach (var slicelist in slices)
				{
					foreach (var slice in slicelist)
					{
						foreach (var vt in slice.Positions)
						{
							// Usually Y is the upward direction for OBJs
							writer.WriteLine($"v {vt.X} {-vt.Z} {vt.Y}");
						}
					}
				}

				foreach (var slicelist in slices)
				{
					foreach (var slice in slicelist)
					{
						foreach (var uv in slice.UVs)
						{
							writer.WriteLine($"vt {uv.X / texture.Width} {1f - uv.Y / texture.Height}");
						}
					}
				}

				writer.WriteLine("usemtl Material");
				writer.WriteLine("s off");

				int vtxidx = 1;
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
					}
				}
			} // using writer

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

			texture.Save(System.IO.Path.Combine(dirname, imgname),
						 System.Drawing.Imaging.ImageFormat.Png);
		}
	}
}

