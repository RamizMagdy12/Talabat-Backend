using AutoMapper;
using System.Reflection;

namespace Talabat.API.Mappings
{
	public class MappingProfile:Profile
	{
		public MappingProfile()
		{
			var assemblies = new[]
			{
				Assembly.Load("Talabat.API"),
				Assembly.Load("Talabat.Core"),
				Assembly.Load("Talabat.Repository"),
				Assembly.Load("Talabat.Services")
			};
			foreach(var assembly in assemblies)
			{
				MappingProfileHelper.ApplyMappingsFromAssembly(assembly, this);
			}
		}
	}
}
