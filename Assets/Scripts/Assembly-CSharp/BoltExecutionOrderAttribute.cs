using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class BoltExecutionOrderAttribute : Attribute
{
	private readonly int _executionOrder;

	public int executionOrder
	{
		get
		{
			return _executionOrder;
		}
	}

	public BoltExecutionOrderAttribute(int order)
	{
		_executionOrder = order;
	}
}
