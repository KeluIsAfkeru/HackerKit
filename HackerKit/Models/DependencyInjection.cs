namespace HackerKit.Models
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SingletonAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class)]
	public class TransientAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class)]
	public class ScopedAttribute : Attribute { }
}
