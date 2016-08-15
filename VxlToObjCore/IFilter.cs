using System;
namespace VxlToObj.Core
{
	public interface IVoxelModelFilter
	{
		void Apply(ref VoxelModel model, IProgressListener progress);
	}
}

