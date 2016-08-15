using System;
namespace VxlToObj.Core
{
	public interface IProgressListener
	{
		void Report(double percentage);
		void Report(string text);
	}
}

