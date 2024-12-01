using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

class Program
{
    static void Main(string[] args)
    {
        // Check if arguments are passed
        if (args.Length > 0)
        {
            string path = args[0];
            string dest = "";

            string jsonText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pref.json"));
            jsonText = jsonText.Replace("\\", "\\\\");
            Prefs json = JsonSerializer.Deserialize<Prefs>(jsonText);

            if (Directory.Exists(json.directory))
            {
                Console.WriteLine("Directory for a Source 2 game already set!");
                dest = json.directory;
            }
            else
            {
                Console.WriteLine("Directory for a source 2 game is invalid (you can edit in prefs.json by changing the value of \"directory\")\nPlease write the directory of the game where the compiler is located in (such as Counter Strike 2)");
                dest = Path.GetFullPath(Console.ReadLine()); // steam/steamapps/common/CSGO
            }
            
            string contentDest = Path.Combine(dest, "content", "csgo_addons"); // steam/steamapps/common/CSGO/content/csgo_addons
            string gameDest = Path.Combine(dest, "game", "csgo_addons"); // steam/steamapps/common/CSGO/game/csgo_addons

            if (Directory.Exists(path))
            {
                // Move to the content destination.
                Console.WriteLine($"Moving {path} to {contentDest}...");
                string rawDest = Path.Combine(contentDest, Path.GetFileName(args[0]));
                MoveAcrossDrives(new DirectoryInfo(path), new DirectoryInfo(rawDest));
                Console.WriteLine("Successfully moved the addon folder to the destination");

                // Obtain all files
                Console.WriteLine("Getting all files...");
                List<string> files = new List<string>();
                foreach (string f in Directory.GetFiles(rawDest, "*", SearchOption.AllDirectories)) files.Add(f);

                Console.WriteLine("Compiling...");
                string rc = Path.Combine(dest, "game", "bin", "win64", "resourcecompiler.exe");
                var proc = Process.Start(rc, files.ToArray());
                proc.WaitForExit();
                
                Console.WriteLine("Compilation complete");

                Console.WriteLine("Regurgitating...");
                MoveAcrossDrives(
                    new DirectoryInfo(
                        Path.Combine(gameDest, Path.GetFileName(args[0]))), 
                    new DirectoryInfo(
                        Path.Combine(Directory.GetParent(path).ToString(), $"{Path.GetFileName(path)}_compiled")));

                Console.WriteLine("Clearing...");
                Directory.Delete(rawDest, true);
                Directory.Delete(Path.Combine(gameDest, Path.GetFileName(args[0])), true);

                Console.WriteLine($"\nComplete!\n\nYou can find your compiled assets in \"{path}_compiled\".");
            }
            else
            {
                Console.WriteLine("You need to drop a folder, not a file");
            }
        }
        else
        {
            Console.WriteLine("No folder was dropped onto the executable");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void MoveAcrossDrives(DirectoryInfo src, DirectoryInfo dest)
    {
        if (!Directory.Exists(dest.FullName)) Directory.CreateDirectory(dest.FullName);
        if (Directory.Exists(dest.ToString()));
        foreach (FileInfo f in src.GetFiles()) f.CopyTo(Path.Combine(dest.ToString(), f.Name), true);
        foreach (DirectoryInfo d in src.GetDirectories())
        {
            DirectoryInfo dir = dest.CreateSubdirectory(d.Name);
            MoveAcrossDrives(d, dir);
        }
    }

    public class Prefs
    {
        public string directory { get; set; }
    }
}