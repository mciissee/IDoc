using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Idoc.Lib
{

	/// <summary>
	/// Project setting class that not depends on the current platform of the project.
	/// This means that this class is usable for Unity Project and also for C# Window application.
	/// </summary>
	[Serializable]
	public class Setting
	{
		private Language language;
		private List<string> inputDirectories;
		private string outputDirectory;
		private string projectName;
		private string projectVersion;

		private bool sortNames;
		private bool extractPublic;
		private bool extractPrivate;
		private bool extractProtected;
		private bool extractInternal;
		private bool extractUndocumentedMembers;

		/// <summary>
		/// Gets or sets the language of the source files that will be searched by the program.
		/// </summary>
		public static Language Language
		{
			get => Instance.language;
			set => Instance.language = value;

		}

		/// <summary>
		/// Gets or sets the html template of the project
		/// </summary>
		public static string HtmlTemplate { get; set; }

		/// <summary>
		/// The output directorie of the html files
		/// </summary>
		public static string OutputDirectory { get => Instance.outputDirectory; set => Instance.outputDirectory = value; }

		/// <summary>
		/// The name of the project
		/// </summary>
		public static string ProjectName { get => Instance.projectName; set => Instance.projectName = value; }

		/// <summary>
		/// The version of the project
		/// </summary>
		public static string ProjectVersion { get => Instance.projectVersion; set => Instance.projectVersion = value; }

		/// <summary>
		/// The folders where to find the files
		/// </summary>
		public static List<string> InputDirectories => Instance.inputDirectories;


		/// <summary>
		/// Gets or sets a value indicating if the members and namespaces will be sorted during the generation of the html pages
		/// </summary>
		public static bool SortNames { get => Instance.sortNames; set => Instance.sortNames = value; }

		/// <summary>
		/// Gets or sets value indicating if the members declared public will be inclued during the generation of the html pages
		/// </summary>
		public static bool ExtractPublic { get => Instance.extractPublic; set => Instance.extractPublic = value; }

		/// <summary>
		/// Gets or sets value indicating if the members declared protected will be inclued during the generation of the html pages
		/// </summary>
		public static bool ExtractProtected { get => Instance.extractProtected; set => Instance.extractProtected = value; }

		/// <summary>
		/// Gets or sets value indicating if the members declared private will be inclued during the generation of the html pages
		/// </summary>
		public static bool ExtractPrivate { get => Instance.extractPrivate; set => Instance.extractPrivate = value; }

		/// <summary>
		/// Gets or sets value indicating if the members declared internal will be inclued during the generation of the html pages
		/// </summary>
		public static bool ExtractInternal { get => Instance.extractInternal; set => Instance.extractInternal = value; }

		/// <summary>
		/// Gets or sets value indicating if the members which are not documented should be extracted
		/// </summary>
		public static bool ExtractUndocumentedMembers { get => Instance.extractUndocumentedMembers; set => Instance.extractUndocumentedMembers = value; }

		/// <summary>
		/// The path where the config file is saved
		/// </summary>
		public static string FilePath => Path.Combine(Directory.GetCurrentDirectory(), "Config.idocconfig");

		private static Setting instance;
		private static Setting Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Setting();
					if (File.Exists(FilePath))
					{
						LoadSettings();
					}
					else
					{
						instance = new Setting();
					}
				}
				return instance;
			}

			set { instance = value; }
		}

		private Setting()
		{
			inputDirectories = new List<string>();
			outputDirectory = Directory.GetCurrentDirectory();
			extractPublic = true;
			extractUndocumentedMembers = true;
		}

		/// <summary>
		/// Adds the given folder to the input directories if it is not added
		/// </summary>
		/// <param name="folder">The folder</param>
		/// <returns><c>true</c> if the folder is added <c>false</c> otherwise</returns>
		public static bool TryAddInputDirectory(string folder)
		{
			if (InputDirectories.Contains(folder))
			{
				InputDirectories.Remove(folder);
				return true;
			}

			// add the new folder only if it's not a sub-foler of any added folder.
			var parent = InputDirectories.Find(folder.StartsWith);
			if (parent == null)
			{
				InputDirectories.Add(folder);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Saves the settings at <see cref="FilePath"/>
		/// </summary>
		public static void SaveSettings()
		{
			if (File.Exists(FilePath))
			{
				File.Delete(FilePath);
			}
			using (var stream = new FileStream(FilePath, FileMode.Create))
			{
				var bf = new BinaryFormatter();
				bf.Serialize(stream, Instance);
				stream.Close();
			}
		}

		/// <summary>
		/// Saves the settings from <see cref="FilePath"/>
		/// </summary>
		private static void LoadSettings()
		{
			using (var stream = new FileStream(FilePath, FileMode.Open))
			{
				var bf = new BinaryFormatter();
				Instance = (Setting)bf.Deserialize(stream);
				stream.Close();
			}
		}

	}
}