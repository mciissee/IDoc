using System;
using System.Threading.Tasks;
using Idoc.Lib;
namespace Idoc.CLI
{
    class MainClass
    {

        public static void Main(string[] args)
        {
            Setting.Language = Language.C;

            Setting.ExtractProtected = true;
            Setting.ExtractPublic = true;

            Setting.OutputDirectory = "/Users/mamadou/Desktop/Documentation";
            //Setting.TryAddInputDirectory("/Users/mamadou/Desktop/Unity Projects/Into The Hole/Assets/InfinityEngine");
            //Setting.TryAddInputDirectory("/Users/mamadou/Desktop/Unity Projects/InfinityEngine/InfinityEngine/src");
            Setting.TryAddInputDirectory("/Users/mamadou/Desktop/Profiler");

            // Setting.TryAddInputDirectory("/Users/mamadou/Downloads/eyeskills-eyeskills-eb8d6c5674c2/Unity/Assets/EyeSkills");
            Logger.onlogged += it =>
            {
                if (it.type == LogType.Error)
                    Console.WriteLine(it.message);
            };
            Lexer.CS.WriteToFile("./CS-GRAMMAR.txt");

            try
            {
                Task.WaitAll(IDoc.Instance.Run());
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                IDoc.Instance.Cancel();
                Console.WriteLine(e.InnerException?.Message ?? e.Message);
                Console.WriteLine(e.InnerException?.StackTrace ?? e.StackTrace);
            }
        }
    }
}