using System;
namespace VxlToObj.Shell
{
	public class ConsoleProgressListener: VxlToObj.Core.IProgressListener
	{
		volatile string text = "Please wait...";
		float percentage = float.NaN; // "double" is not thread-safe!
		int spinnerState = 0;

		public bool AutoUpdate { get; set; }

		public ConsoleProgressListener()
		{
		}

		public void Start()
		{
		}

		public void Stop()
		{
			Console.WriteLine();
		}

		public void Update()
		{
			int conwidth = Math.Max(20, Console.BufferWidth - 1);
			var buf = new char[conwidth];

			for (int i = 0; i < buf.Length; ++i)
			{
				buf[i] = ' ';
			}

			int x1 = 0, x2 = buf.Length;

			// Draw spinner
			buf[x1] = @"/|\-"[spinnerState ^ 3];
			spinnerState = (spinnerState + 1) & 3;
			x1 += 2;

			// Draw text
			// (full-width characters are not considered for now)
			int textX = x1 + (x2 - x1) * 2 / 3;
			int textW = x2 - textX;
			string text = this.text;
			for (int i = 0; i < text.Length && i < textW; ++i)
			{
				buf[textX + i] = text[i];
			}
			x2 = textX - 1;

			// Draw progress bar
			buf[x1] = '[';
			buf[x2 - 1] = ']';
			++x1; --x2;
			double value = this.percentage;
			if (double.IsNaN(value) || double.IsInfinity(value))
			{
				var marquee = @"/^\v";
				for (int x = x1; x < x2; ++x)
				{
					buf[x] = marquee[(x - spinnerState) & 3];
				}
			}
			else {
				int prgwidth = x2 - x1;
				int prgvalue = (int)Math.Round(Math.Min(value, 1.0) * prgwidth);
				var marquee = @"***.";
				for (int x = x1; x < x1 + prgvalue; ++x)
				{
					buf[x] = marquee[(x - spinnerState) & 3];
				}
			}

			Console.Write("\r" + new string(buf));
		}

		public void Report(string text)
		{
			this.text = text;
			if (AutoUpdate)
			{
				Update();
			}
		}

		public void Report(double percentage)
		{
			this.percentage = (float) percentage;
			if (AutoUpdate)
			{
				Update();
			}
		}
	}
}

