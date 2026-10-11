using System.Reflection;

namespace ViaTrade.Tests;

public class InterfaceDouble : DispatchProxy
{
	private Func<MethodInfo, object?[], object?> _invoke = null!;

	public static T Create<T>(Func<MethodInfo, object?[], object?> invoke)
		where T : class
	{
		var proxy = Create<T, InterfaceDouble>();
		((InterfaceDouble)(object)proxy)._invoke = invoke;
		return proxy;
	}

	protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
	{
		return _invoke(targetMethod!, args!);
	}
}
