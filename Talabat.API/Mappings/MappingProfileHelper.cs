using AutoMapper;
using System.Reflection;

namespace Talabat.API.Mappings
{
	public static class MappingProfileHelper
	{
		public static void ApplyMappingsFromAssembly(Assembly assembly, Profile profile)
		{
			var types = assembly.GetExportedTypes()
				.Where(t => t.GetInterfaces().Any(i =>
					i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
				.ToList();

			foreach (var type in types)
			{
				var instance = Activator.CreateInstance(type);

				var methodInfo = type.GetMethod("Mapping")
					?? type.GetInterface("IMapFrom`1")!.GetMethod("Mapping");

				methodInfo?.Invoke(instance, new object[] { profile });

			}
		}
	}
}
