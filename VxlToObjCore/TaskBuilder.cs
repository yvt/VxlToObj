using System;
namespace VxlToObj.Core
{
	sealed class TaskBuilderContext
	{
		public int numTasks = 0;
		public Func<IProgressListener, object> action;

		public IProgressListener CreateWrappedProgressListener(IProgressListener parent, int taskIndex, string taskName)
		{
			return new ProgressMapper(parent, (double)taskIndex / numTasks, 1.0 / numTasks, taskName);
		}

	}
	public static class TaskBuilder
	{
		public static TaskBuilder<TResult> Start<TResult>(string name, Func<IProgressListener, TResult> fn)
		{
			var context = new TaskBuilderContext()
			{
				numTasks = 1
			};
			context.action = (parent) =>
			{
				return fn(context.CreateWrappedProgressListener(parent, 0, name));
			};

			return new TaskBuilder<TResult>(context);
		}
	}

	public class TaskBuilder<T>
	{
		TaskBuilderContext context;
		bool consumed = false;

		internal TaskBuilder(TaskBuilderContext context)
		{
			this.context = context;
		}

		public TaskBuilder<TResult> Then<TResult>(string name, Func<T, IProgressListener, TResult> fn)
		{
			if (consumed)
			{
				throw new InvalidOperationException("Cannot 'Then' the same TaskBuilder twice");
			}

			consumed = true;
			var index = context.numTasks;
			context.numTasks += 1;
			var prevAction = context.action;
			context.action = (parent) =>
			{
				var param = (T)prevAction(parent);
				return fn(param, context.CreateWrappedProgressListener(parent, index, name));
			};

			return new TaskBuilder<TResult>(context);
		}

		public Func<IProgressListener, T> Complete()
		{
			consumed = true;
			return (progress) =>
			{
				return (T)context.action(progress);
			};
		}
	}
}

