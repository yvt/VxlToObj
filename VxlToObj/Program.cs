using System;
using System.Collections.Generic;
using VxlToObj.Core;

namespace VxlToObj.Shell
{
	class ConsoleShellMain
	{
		static IVoxelModelFilter ParseFilter(string flt)
		{
			var parts = flt.Split(',');
			var opts = new Dictionary<string, string>();

			// Parse options
			for (int i = 1; i < parts.Length; ++i)
			{
				var part = parts[i];
				int idx = part.IndexOf('=');
				if (idx < 0)
				{
					throw new Mono.Options.OptionException("Bad filter parameter specification.: " + part, "filter");
				}
				opts[part.Substring(0, idx).ToLowerInvariant()] = part.Substring(idx + 1);
			}

			IVoxelModelFilter ret = null;

			switch (parts[0])
			{
				case "hollowify":
					ret = new HollowifyFilter();

					if (opts.ContainsKey("thickness"))
					{
						int thickness;
						if (!int.TryParse(opts["thickness"], out thickness))
						{
							throw new Mono.Options.OptionException($"Bad value for 'thickness': {opts["thickness"]}", "filter");
						}
						if (thickness < 1)
						{
							throw new Mono.Options.OptionException($"Bad value for 'thickness': {opts["thickness"]}", "filter");
						}
						((HollowifyFilter)ret).Distance = thickness;
						opts.Remove("thickness");
					}
					if (opts.ContainsKey("type"))
					{
						switch (opts["type"].ToLowerInvariant())
						{
							case "chebyshev":
								((HollowifyFilter)ret).DistanceType = DistanceType.Chebyshev;
								break;
							case "manhattan":
								((HollowifyFilter)ret).DistanceType = DistanceType.Manhattan;
								break;
							default:
								throw new Mono.Options.OptionException($"Unrecognized value of 'type': {opts["type"]}", "filter");
						}
						opts.Remove("type");
					}
					if (opts.ContainsKey("boundary"))
					{
						switch (opts["boundary"].ToLowerInvariant())
						{
							case "solid":
								((HollowifyFilter)ret).IsExteriorSolid = true;
								break;
							case "empty":
								((HollowifyFilter)ret).IsExteriorSolid = false;
								break;
							default:
								throw new Mono.Options.OptionException($"Unrecognized value of 'boundary': {opts["boundary"]}", "filter");
						}
						opts.Remove("boundary");
					}
					break;
				case "solidify":
					ret = new SolidifyFilter();
					break;
				default:
					throw new Mono.Options.OptionException($"Unknown filter: {parts[0]}", "filter");
			}

			// Check unprocessed options
			foreach (var opt in opts)
			{
				throw new Mono.Options.OptionException($"The filter {parts[0].ToLowerInvariant()} doesn't accept a parameter named '{opt.Key}'", "filter");
			}

			return ret;
		}

		public static void Main(string[] args)
		{
			bool showhelp = false;
			string inputfmt = null;
			string outputfmt = null;
			string slicegen = "simple";
			string texgen = "simple";
			var filters = new List<IVoxelModelFilter>();

			var p = new Mono.Options.OptionSet()
			{
				{ "i|in=", 
					"Specifies the input format. \n" +
					"- kv6: VOXLAP engine sprite format.\n" +
					"- vxl: VOXLAP engine worldmap format.\n" +
					"- vox: MagicaVoxel format.\n",
					v => inputfmt = v },
				{ "o|out=",
					"Specifies the output format.\n" +
					"- obj: Wavefront .obj file.",
					v => outputfmt = v },
				{ "f|filter=",
					"Specifies the filter to apply on the model.\n" +
					"Parameters can be specified like this: --filter=NAME,PROP=VALUE,...\n" + 
					"Multiple filters can be applied by specifying this option for multiple times.\n" +
					"- hollowify: Removes invisible voxels.\n" +
					"    thickness=VALUE: Thickness of the shell. (default = 1)\n" +
					"    type=chebyshev|manhattan: Specifies the distance function. (default = chebyshev)\n" +
					"    boundary=empty|solid: Specifies how the space outside the boundary is handled. (default = empty)\n" +
					"- solidify: Fills the invisible space.",
					v => filters.Add(ParseFilter(v)) },
				{ "s|slicegen=", 
					"Specifies the slice generator.\n" +
					"- simple: Trivial slice generator that generates up to 6 quads for each voxel.",
					v => slicegen = v },
				{ "t|texgen=",  "Specifies the texture generator.\n" +
					"- simple: Trivial texture generator. Requires the 'simple' slice generator.",
					v => texgen = v },
				{ "h|help",  "Show this message and exit",
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
				Console.WriteLine("USAGE: VxlToObj INFILE OUTFILE.obj [OPTIONS...]");
				Console.WriteLine("");
				Console.WriteLine("OPTIONS:");
				Console.WriteLine("");
				p.WriteOptionDescriptions(Console.Out);
				return;
			}

			// Input files
			if (extra.Count != 2)
			{
				Console.Error.WriteLine("Two file names must be provided.");
				Console.Error.WriteLine("Try `VxlToObj --help' for more information.");
				Environment.Exit(1);
				return;
			}
			string infile = extra[0];
			string outfile = extra[1];

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

			if (inputfmt == null)
			{
				if (infile.EndsWith(".vxl", StringComparison.InvariantCultureIgnoreCase))
				{
					inputfmt = "vxl";
				}
				else if (infile.EndsWith(".kv6", StringComparison.InvariantCultureIgnoreCase))
				{
					inputfmt = "kv6";
				}
				else if (infile.EndsWith(".vox", StringComparison.InvariantCultureIgnoreCase))
				{
					inputfmt = "vox";
				}
				else
				{
					Console.Error.WriteLine($"Cannot guess the input format; specify it with -i/--in");
					Environment.Exit(1);
					return;
				}
			} 
			else if (inputfmt != "vxl" && inputfmt != "kv6" && inputfmt != "vox")
			{
				Console.Error.WriteLine($"Unknown input format: {inputfmt}");
				Environment.Exit(1);
				return;
			}

			if (outputfmt == null)
			{
				if (outfile.EndsWith(".obj", StringComparison.InvariantCultureIgnoreCase))
				{
					outputfmt = "obj";
				}
				else
				{
					Console.Error.WriteLine($"Cannot guess the output format; specify it with -o/--out");
					Environment.Exit(1);
					return;
				}
			}
			else if (outputfmt != "obj")
			{
				Console.Error.WriteLine($"Unknown output format: {outputfmt}");
				Environment.Exit(1);
				return;
			}

			VoxelModel model;
			switch (inputfmt)
			{
				case "vxl":
					model = new VxlVoxelModelLoader().LoadVoxelModel(
						System.IO.File.ReadAllBytes(infile));
					break;
				case "kv6":
					model = new Kv6VoxelModelLoader().LoadVoxelModel(
						System.IO.File.ReadAllBytes(infile));
					break;
				case "vox":
					model = new MagicaVoxelModelLoader().LoadVoxelModel(
						System.IO.File.ReadAllBytes(infile));
					break;
				default:
					throw new InvalidOperationException();
			}

			foreach (var flt in filters)
			{
				flt.Apply(ref model);
			}

			var slices = new SimpleMeshSliceGenerator().GenerateSlices(model);

			System.Drawing.Bitmap bmp;
			new SimpleMeshTextureGenerator().GenerateTextureAndUV(model, slices, out bmp);

			new ObjWriter().Save(slices, bmp, outfile);
		}
	}
}
