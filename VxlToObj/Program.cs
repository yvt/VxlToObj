using System;

namespace VxlToObj
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var model = new VxlVoxelModelLoader().LoadVoxelModel(
				System.IO.File.ReadAllBytes(args[0]));

			var slices = new SimpleMeshSliceGenerator().GenerateSlices(model);

			System.Drawing.Bitmap bmp;
			new SimpleMeshTextureGenerator().GenerateTextureAndUV(model, slices, out bmp);

			new ObjWriter().Save(slices, bmp, args[1]);
		}
	}
}
