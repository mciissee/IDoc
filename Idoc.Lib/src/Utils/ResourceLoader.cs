using System.IO;
using System.Reflection;

namespace Idoc.Lib
{
	public static class ResourceLoader
	{
		public static string Load(string name)
		{

			var assembly = Assembly.GetAssembly(typeof(ResourceLoader));
			var path = $"Idoc.Lib.res.{name}";

			using (var stream = assembly.GetManifestResourceStream(path))
			{
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
