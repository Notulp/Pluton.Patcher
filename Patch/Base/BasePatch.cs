namespace Pluton.Patcher
{
	public class BasePatch : IPatch
	{
		virtual public bool Patch() => true;

		internal static BaseInstruction ParseFromJSON(JSON.Object obj, params object[] args)
		{
			return new BaseInstruction();
		}
	}
}

