using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text;

using System.Reflection;


namespace Idoc.Lib
{
	class Person
	{
		private int age = 10;
	}


	public class IDoc
	{
		private Dictionary<Language, Func<IParser>> parsers;
		private int step;
		private int totalSteps;
		private Stopwatch watch;
		private static IDoc instance;
		private CancellationTokenSource cancellation;
		private List<Token> ignores;
		public bool IsRunning { get; private set; }

		public float Progression => IsRunning && totalSteps > 0 ? step / (float)totalSteps : 0f;

		public static IDoc Instance => instance ?? (instance = new IDoc());

		public IDoc()
		{
			parsers = new Dictionary<Language, Func<IParser>>
			{
				[Language.CS] = () => new CSParser(),
				[Language.C] = () => new CParser(),
				[Language.Java] = () => new JParser(),
			};
			watch = new Stopwatch();


			var person = new Person();
		
			var field = person.GetType().GetRuntimeField("age");
			Console.WriteLine($"age {field.GetValue(person)}");


		}

		private string[] GetKeywords()
		{
			switch (Setting.Language)
			{
				case Language.CS:
					Array.Sort(CSParser.BuiltIn);
					return CSParser.BuiltIn;
				case Language.C:
					Array.Sort(CParser.BuiltIn);
					return CParser.BuiltIn;
				case Language.Java:
					Array.Sort(JParser.BuiltIn);
					return JParser.BuiltIn;
				default:
					return new string[0];
			}
		}

		public async Task Run()
		{
			if (IsRunning)
			{
				Logger.Log("Idoc is already running please wait or cancel the current build...");
				return;
			}

			Terminate();

			Logger.Clear();

			if (string.IsNullOrEmpty(Setting.OutputDirectory))
			{
				Logger.LogError("An output path must be specified");
				return;
			}

			void mkdir(string directory)
			{
				if (!Directory.Exists(directory))
					Directory.CreateDirectory(directory);
			}


			IsRunning = true;
			watch.Start();
			ignores = new List<Token>();
			var extension = Setting.Language.Extension();
			Logger.Log($"Scanning folders for {extension} source files...");
			var sources = new List<string>();
			foreach (var dir in Setting.InputDirectories)
			{
				sources.AddRange(Directory.GetFiles(dir, $"*{extension}", SearchOption.AllDirectories));
			}

			totalSteps = sources.Count;
			Logger.Log($"${totalSteps} file(s) found...");

			if (totalSteps == 0)
			{
				watch.Stop();
				Logger.Log("Build finished.. time elapsed : " + watch.Elapsed);
				return;
			}

			mkdir(Setting.OutputDirectory);
			mkdir(Setting.OutputDirectory + "/json");
			mkdir(Setting.OutputDirectory + "/xml");
			mkdir(Setting.OutputDirectory + "/js");

			totalSteps += 2; // 1 for processing members step and 1 for generating files

			cancellation = new CancellationTokenSource();

			var instantiate = parsers[Setting.Language];
			var tasks = new List<Task<List<IDocItem>>>();

			foreach (var path in sources)
			{
				tasks.Add(instantiate().Parse(File.ReadAllText(path), path, cancellation));
			}

			await Task.WhenAll(tasks).ContinueWith(continuation =>
			{
				if (continuation.Exception == null && !continuation.IsCanceled)
				{
					Logger.Log("Parsing OK...");
					Logger.Log("Processing members..." + Progression * 100);

					var groups = continuation.Result
											.SelectMany(it => it)
											.GroupBy(it => it.Signature)
											.ToList();

					var members = new List<IDocItem>();
					IDocItem item;
					foreach (var group in groups)
					{
						item = group.ElementAt(0);
						foreach (var it in group)
						{
							if (ReferenceEquals(it, item))
								continue;
							item.Combine(it);
						}
						members.Add(item);
					}

					IncrementStep();

					Logger.Log($"Generating files to the directory {Setting.OutputDirectory}...");
					var output = new Dictionary<string, Dictionary<string, object>>();
					foreach (var it in members)
					{
						it.Build(output);
					}

					var builder = new StringBuilder();
					builder.AppendLine($"const MEMBERS = [");
					var body = string.Join(",", output.Values.Select(it => it.ToJSON()).ToArray());
					builder.AppendLine(body);
					builder.AppendLine("];");


					builder.AppendLine($"const ROOT_MEMBERS = [");
					body = string.Join(",", output.Values.Where(it => it["scope"].ToString() == "").Select(it => it.ToJSON()).ToArray());
					builder.AppendLine(body);
					builder.AppendLine("];");

					builder.AppendLine($"const BUILTIN_WORDS_REGEX = /\\b(?:{string.Join("|", GetKeywords())})\\b/g;");

					WriteToFile($"{Setting.OutputDirectory}/js/idoc-data.js", builder.ToString());


					var json = output.ToJSON();

					WriteToFile($"{Setting.OutputDirectory}/json/idoc.json", json);

					try
					{
						WriteToFile($"{Setting.OutputDirectory}/xml/idoc.xml", json.ToXML("members"));
					}
					catch
					{
						Logger.LogError("Xml generation failed");
					}

					step = totalSteps;
					watch.Stop();
					ignores.Clear();
				}
				else
				{
					Console.WriteLine(continuation.Exception);
					Logger.LogError("Task canceled");
				}
				Logger.Log("Parsing finished.. time elapsed : " + watch.Elapsed);
			});
		}

		public void WriteToFile(string filePath, string fileContent)
		{
			Logger.Log($"Generating {filePath}...");
			if (File.Exists(filePath))
				File.Delete(filePath);

			using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				stream.SetLength(0);
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(fileContent);
				}
			}
		}

		public void Terminate()
		{
			IsRunning = false;
			step = 0;
			totalSteps = 0;
		}

		public void Cancel()
		{
			if (!IsRunning)
				return;
			Terminate();
			cancellation?.Cancel();
			Logger.Log("Task cancelled..");
		}

		internal void Ignore(Token token)
		{
			ignores = ignores ?? new List<Token>();
			ignores.Add(token);
		}
		internal bool ShouldIgnore(Token token) => ignores.Contains(token);
		internal void IncrementStep() => step++;
	}
}
