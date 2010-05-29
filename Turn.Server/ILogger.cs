
namespace Turn.Server
{
	public interface ILogger
	{
		void WriteWarning(string warning);
		void WriteError(string error);
	}

	public class NullLogger
		: ILogger
	{
		public void WriteWarning(string warning)
		{
		}

		public void WriteError(string error)
		{
		}
	}
}
