using HackerKit.Models;
using System.Reflection;

namespace HackerKit.Services
{
	public static class AttributeService
    {
		public static void RegisterAllServicesWithAttributes(this IServiceCollection services)
		{
			//var assembly = Assembly.GetExecutingAssembly();

			////注册标记了SingletonAttribute的类
			//RegisterServicesWithAttribute<SingletonAttribute>(services, assembly, ServiceLifetime.Singleton);

			////注册标记了TransientAttribute的类
			//RegisterServicesWithAttribute<TransientAttribute>(services, assembly, ServiceLifetime.Transient);

			////注册标记了ScopedAttribute的类
			//RegisterServicesWithAttribute<ScopedAttribute>(services, assembly, ServiceLifetime.Scoped);

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				RegisterServicesWithAttribute<SingletonAttribute>(services, asm, ServiceLifetime.Singleton);
				RegisterServicesWithAttribute<TransientAttribute>(services, asm, ServiceLifetime.Transient);
				RegisterServicesWithAttribute<ScopedAttribute>(services, asm, ServiceLifetime.Scoped);
			}
		}

		private static void RegisterServicesWithAttribute<TAttribute>(
			IServiceCollection services,
			Assembly assembly,
			ServiceLifetime lifetime) where TAttribute : Attribute
		{
			var types = assembly.GetTypes()
				.Where(type => type.GetCustomAttribute<TAttribute>() != null
							  && !type.IsInterface
							  && !type.IsAbstract)
				.ToList();

			foreach (var type in types)
			{
				//查找该类型实现的所有接口
				var interfaces = type.GetInterfaces().ToList();

				if (interfaces.Any())
				{
					//如果实现了接口，则按接口注册
					foreach (var interfaceType in interfaces)
					{
						services.Add(new ServiceDescriptor(interfaceType, type, lifetime));
					}
				}
				else
				{
					//如果没有实现接口，按自身类型注册
					services.Add(new ServiceDescriptor(type, type, lifetime));
				}
			}
		}
	}
}
