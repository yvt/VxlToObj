using System;
using System.Collections.Generic;

namespace VxlToObj
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			bool showhelp = false;
			string inputfmt = null;
			string outputfmt = null;
			string slicegen = "simple";
			string texgen = "simple";
			var p = new Mono.Options.OptionSet()
			{
				{ "i|in=", "specifies the input format. Currently, only 'vxl' is supported.",
					v => inputfmt = v },
				{ "o|out=", "specifies the output format. Currently, only 'obj' is supported.",
					v => outputfmt = v },
				{ "s|slicegen=", "specifies the slice generator. Currently, only 'simple' is supported.",
					v => slicegen = v },
				{ "t|texgen=",  "specifies the texture generator. Currently, only 'simple' is supported.",
					v => texgen = v },
				{ "h|help",  "show this message and exit",
				   v => showhelp = v != null }
			};

			List<string> extra;
			try {
				extra = p.Parse(args);
			} catch (Mono.Options.OptionException e) {
				Console.Error.WriteLine(e.Message);
				Console.Error.WriteLine("Try `VxlToObj --help' for more information.");
				return;
			}

			if (showhelp) {
				Console.WriteLine("USAGE: VxlToObj INFILE.vxl OUTFILE.obj");
				Console.WriteLine("");
				Console.WriteLine("OPTIONS:");
				Console.WriteLine("");
				p.WriteOptionDescriptions(Console.Out);
				return;
			}

			// Validate combinations
			if (slicegen != "simple")
			{
				Console.Error.WriteLine($"Unknown slice generator: {slicegen}");
				Environment.Exit(1);
				return;
			}
			if (texgen != "simple")
			{
				Console.Error.WriteLine($"Unknown texture generator: {texgen}");
				Environment.Exit(1);
				return;
			}
			if (inputfmt != "vxl")
			{
				Console.Error.WriteLine($"Unknown input format: {inputfmt}");
				Environment.Exit(1);
				return;
			}
			if (outputfmt != "obj")
			{
				Console.Error.WriteLine($"Unknown output format: {outputfmt}");
				Environment.Exit(1);
				return;
			}

			// Input files
			if (extra.Count != 2)
			{
				Console.Error.WriteLine("Two file names must be provided.");
				Environment.Exit(1);
				return;
			}

			var model = new VxlVoxelModelLoader().LoadVoxelModel(
				System.IO.File.ReadAllBytes(extra[0]));

			var slices = new SimpleMeshSliceGenerator().GenerateSlices(model);

			System.Drawing.Bitmap bmp;
			new SimpleMeshTextureGenerator().GenerateTextureAndUV(model, slices, out bmp);

			new ObjWriter().Save(slices, bmp, extra[1]);
		}
	}
}
