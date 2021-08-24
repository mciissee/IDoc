namespace Idoc.Lib
{
	public class PatternFunction
	{
		public string Name { get; set; }
		public string Body { get; set; }
		public string[] Params { get; set; }

		public PatternFunction(string name, string body, string[] @params)
		{
			Name = name;
			Body = body;
			Params = @params;
		}
	}
}