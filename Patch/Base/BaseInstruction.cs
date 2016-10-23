namespace Pluton.Patcher
{
	public class BaseInstruction : IInstruction
	{
		internal static BaseInstruction ParseFromJSON(JSON.Object obj, params object[] args)
		{
			return new BaseInstruction();
		}
	}
}

