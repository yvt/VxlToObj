using System;
namespace VxlToObj.Core
{
	sealed class ProgressMapper: IProgressListener
	{
		public IProgressListener Parent { get; private set; }
		public double MinRange { get; private set; }
		public double MaxRange { get; private set; }
		public string Message { get; private set; }

		public ProgressMapper(IProgressListener parent, double min, double range, string message)
		{
			Parent = parent;
			MinRange = min;
			MaxRange = min + range;
			Message = message;
		}

		public void Report(string text)
		{
			if (Parent == null)
			{
				return;
			}
			if (Message != null)
			{
				if (Message.Length == 0)
				{
					Parent.Report(text);
				}
				else
				{
					if (string.IsNullOrWhiteSpace(text))
					{
						Parent.Report(Message);
					}
					else
					{
						Parent.Report(Message + ": " + text);
					}
				}
			}
		}

		public void Report(double percentage)
		{
			Parent?.Report(percentage * (MaxRange - MinRange) + MinRange);
		}
	}
}

