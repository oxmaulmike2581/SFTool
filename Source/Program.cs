using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace SFTool
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Welcome message
            Console.WriteLine("");
            Console.WriteLine("   *************************************************   ");
            Console.WriteLine(" ***************************************************** ");
            Console.WriteLine(" **                                                 ** ");
            Console.WriteLine(" **    ╔══════╗╔══════╗╔══════╗             ╔═╗     ** ");
            Console.WriteLine(" **    ║  ╔═══╝║  ╔═══╝╚═╗  ╔═╝             ║ ║     ** ");
            Console.WriteLine(" **    ║  ╚═══╗║  ╚═╗    ║  ║ ╔═════╗╔═════╗║ ║     ** ");
            Console.WriteLine(" **    ╚═══╗  ║║  ╔═╝    ║  ║ ║ ╔═╗ ║║ ╔═╗ ║║ ║     ** ");
            Console.WriteLine(" **    ╔═══╝  ║║  ║      ║  ║ ║ ╚═╝ ║║ ╚═╝ ║║ ╚╗    ** ");
            Console.WriteLine(" **    ╚══════╝╚══╝      ╚══╝ ╚═════╝╚═════╝╚══╝    ** ");
            Console.WriteLine(" **                                                 ** ");
            Console.WriteLine(" ***************************************************** ");
            Console.WriteLine("   *************************************************   ");
            Console.WriteLine("");

            // Print a help text if no arguments were provided
            if (args.Length < 1)
            {
                if (!File.Exists("sftool_links.txt"))
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("\t SFTool.exe -h model_hash -o output_path");
                    Console.WriteLine("\t OR");
                    Console.WriteLine("\t SFTool.exe -l links_file -o output_path");
                    Console.WriteLine("");
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("\t -h: \t ");
                    Console.WriteLine("\t     Model hash or whole link.");
                    Console.WriteLine("\t -l:");
                    Console.WriteLine("\t     Path to the file with list of links to batch download.");
                    Console.WriteLine("\t -o: (Optional)");
                    Console.WriteLine("\t     Path to the output directory. If does not specified, the current directory will used.");
                    Console.WriteLine("\t     NOTE: -h and -i cannot be used at the same time!");
                    Console.WriteLine("");
                    Console.WriteLine("[ERROR] No arguments were provided or file 'sftool_links.txt' was not found.");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadLine();
                    return;
                }
            }

            // ==================================================

            // Start a stopwatch
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // ==================================================

            // Create an instances of required classes
            ArgumentParser ap = new ArgumentParser();

            // Forming the output path
            string outputPath = String.Empty;

            if (args.Length < 3)
            {
                    outputPath = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                if (args[2].StartsWith("-o"))
                {
                    outputPath = Path.GetFullPath(args[3]);
                }
            }

            // If we have an "-h" argument, just process it as single hash
            // But if we have an "-l" argument, read a list from specified file and process hashes from it
            // Or if we have a "sftool_links.txt" file in application directory, both arguments are ignored
            if (args.Length > 0)
            {
                if (args[0].StartsWith("-h"))
                {
                    // Get the hash
                    string hash = ap.Parse(args[1]);

                    // Process the hash
                    ProcessModel(hash, outputPath);
                }
                else if (args[0].StartsWith("-l"))
                {
                    // If the file with the list of links exists, then process each line in it
                    if (File.Exists(args[1]))
                    {
                        // Print a message
                        Console.WriteLine("Processing hashes from file {0}", Path.GetFullPath(args[1]));

                        // Get list of hashes
                        FileInfo fi = new FileInfo(args[1]);
                        List<string> links = ap.Parse(fi);
                        Console.WriteLine("Hashes found: {0}", links.Count);

                        // Process each hash
                        foreach (string hash in links)
                        {
                            ProcessModel(hash, outputPath);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] File '{0}' does not exist.", args[1]);
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadLine();
                    }
                }
            }
            
            if (File.Exists("sftool_links.txt"))
            {
                // Get list of hashes
                FileInfo fi = new FileInfo("sftool_links.txt");
                List<string> links = ap.Parse(fi);

                // Process each hash
                foreach (string hash in links)
                {
                    ProcessModel(hash, outputPath);
                }
            }

            // ==================================================

            // Stop our stopwatch and print elapsed time
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("Time taken: {0}", elapsedTime);
        }

        // Worker
        public static void ProcessModel(string modelHash, string outputPath)
        {
            // Define variables
            bool isBINZ = false;
            string configBaseUrl = "https://sketchfab.com/i/models/";
            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

            // Create an instances of required classes
            Downloader dn = new Downloader();
            GZip gz = new GZip();

            // Print model ID
            Console.WriteLine("");
            Console.WriteLine("Got hash: {0} ({1} chars)", modelHash, modelHash.Length);

            // Get filtered config with general model data
            Dictionary<string, string> modelConfig = dn.GetConfig(configBaseUrl + modelHash, "model");
            Dictionary<string, SortedDictionary<int, string>> texturesConfig = dn.GetConfig(configBaseUrl + modelHash + "/textures", "textures");
            Dictionary<string, string> animsConfig = dn.GetConfig(configBaseUrl + modelHash + "/animations", "anims");

            // Check for data availability
            if (modelConfig.Count > 0)
            {
                Console.WriteLine("Got: Model config");
            }

            if (texturesConfig.Count > 0)
            {
                Console.WriteLine("Got: Textures config");
            }

            if (animsConfig.Count > 0)
            {
                Console.WriteLine("Got: Animations config");
            }

            // Print an short information about requested model
            Console.WriteLine("Model name: {0} (by {1})", modelConfig["modelName"], modelConfig["userName"]);

            // Initial check for BINZ
            if (modelConfig["osgjsUrl"].Contains("file.binz"))
			{
                isBINZ = true;
                Console.WriteLine("[WARNING] Found a new encrypted BINZ format. Importing is impossible at this moment.");
            }

            // ==================================================

            // Downloading model
            byte[] osgjs = dn.GetFileData(modelConfig["osgjsUrl"]);
            byte[] mfbin = dn.GetFileData(modelConfig["mfbinUrl"]);
            byte[] mfwbin = dn.GetFileData(modelConfig["mfwbinUrl"]);

            // Decompressing model and storing in resulting array
            if (!isBINZ)
            {
                files.Add("file.osgjs", gz.Decompress(osgjs));
                files.Add("model_file.bin", gz.Decompress(mfbin));
                files.Add("model_file_wireframe.bin", gz.Decompress(mfwbin));
            }
            else
            {
                files.Add("file.binz", osgjs);
                files.Add("model_file.binz", mfbin);
                files.Add("model_file_wireframe.binz", mfwbin);
            }

            // Print a message
            Console.WriteLine("Model files was successfully downloaded.");

            // ==================================================

            // Processing texture list
            foreach (KeyValuePair<string, SortedDictionary<int, string>> kvp in texturesConfig)
            {
                // Define variables
                string textureName = kvp.Key;
                KeyValuePair<int, string> highestTextureData = kvp.Value.First();

                // Print a texture name
                Console.WriteLine("Got: Texture - {0}", textureName);

                // Get texture file data
                byte[] texture = dn.GetFileData(highestTextureData.Value);

                // Store our texture in resulting array but first check for duplicate key entry
                if (!files.ContainsKey(textureName))
                {
                    files.Add(textureName, texture);
                }
            }

            // ==================================================

            // Processing animations list
            foreach (KeyValuePair<string, string> kvp in animsConfig)
            {
                // Define some variables
                string animName = kvp.Key;

                // Print an animation name
                Console.WriteLine("Got: Animation - {0}", animName);

                // Get animation file data
                byte[] animation = dn.GetFileData(kvp.Value);

                // Store it
                if (!files.ContainsKey(animName))
                {
                    files.Add(animName + ".bin", gz.Decompress(animation));
                }
            }

            // ==================================================

            // Preparing output paths
            string outputDirName = String.Format("{0} (by {1})", modelConfig["modelName"], modelConfig["userName"]);
            string outputDirPath = Path.Combine(outputPath, outputDirName);
            string outputModelDir = Path.Combine(outputDirPath, "meshes");
            string outputTexturesDir = Path.Combine(outputDirPath, "textures");
            string outputAnimsDir = Path.Combine(outputDirPath, "anims");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Set current path
            Directory.SetCurrentDirectory(outputPath);

            // Create directories, flush and move the files
            if (!Directory.Exists(outputDirName))
            {
                // Create a main model directory and set it as output
                Directory.CreateDirectory(outputDirName);
                Directory.SetCurrentDirectory(outputDirPath);

                // Flush all files
                foreach (KeyValuePair<string, byte[]> kvp in files)
                {
                    File.WriteAllBytes(Path.Combine(outputDirPath, kvp.Key), kvp.Value);
                }

                // Make separated directories for model, textures and animations and move corresponding files to them
                Directory.CreateDirectory(outputModelDir);

                // Create a directory only if corresponding config is present
                if (texturesConfig.Count > 0)
                {
                    Directory.CreateDirectory(outputTexturesDir);
                }

                if (animsConfig.Count > 0)
                {
                    Directory.CreateDirectory(outputAnimsDir);
                }

                // Move model files
                if (isBINZ)
                {
                    File.Move("file.binz", Path.Combine(outputModelDir, "file.binz"));
                    File.Move("model_file.binz", Path.Combine(outputModelDir, "model_file.binz"));
                    File.Move("model_file_wireframe.binz", Path.Combine(outputModelDir, "model_file_wireframe.binz"));
                }
                else
                {
                    File.Move("file.osgjs", Path.Combine(outputModelDir, "file.osgjs"));
                    File.Move("model_file.bin", Path.Combine(outputModelDir, "model_file.bin"));
                    File.Move("model_file_wireframe.bin", Path.Combine(outputModelDir, "model_file_wireframe.bin"));
                }

                // Move animations first (because all of them always have the same extension so we can filter them)
                foreach (FileInfo file in new DirectoryInfo(outputDirPath).GetFiles("*.bin"))
                {
                    file.MoveTo($@"{outputAnimsDir}\{file.Name}");
                }

                // Move all textures
                foreach (FileInfo file in new DirectoryInfo(outputDirPath).GetFiles("*.*"))
                {
                    file.MoveTo($@"{outputTexturesDir}\{file.Name}");
                }

                // Return to the application directory to prevent recursing
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                // Print a message
                Console.WriteLine("Written {0} files.", files.Count);
            }
            else
            {
                Console.WriteLine("[WARNING] Output directory already exists.");
            }
        }
    }

    public class ArgumentParser
    {
        public string Parse(string linkToParse)
        {
            return Clean(linkToParse);
        }

        // I had to add FileInfo here as an argument type, because you cannot use the same type of arguments in method overloads.
        public List<string> Parse(FileInfo input)
        {
            // Define resulting array
            List<string> result = new List<string>();

            // Perform next operations only if input file is exists.
            if (input.Exists)
            {
                using (StreamReader sr = input.OpenText())
                {
                    // Define resulting array
                    List<string> lines = new List<string>();

                    // Read all lines from our file
                    string s = String.Empty;
                    while ((s = sr.ReadLine()) != null)
                    {
                        lines.Add(s);
                    }

                    // Process each link
                    foreach (string link in lines)
                    {
                        // Clean and check validity of link
                        string cleanedLink = Clean(link);

                        // Add cleaned hash to the resulting array
                        result.Add(cleanedLink);
                    }
                }
            }

            return result;
        }

        public string Clean(string stringToClean)
        {
            if (stringToClean.Contains("https://sketchfab.com"))
            {
                // Split the link with "-" as delimiter
                string[] linkData = stringToClean.Split(Convert.ToChar("-"));

                // Read the last element of the array
                string modelHash = linkData.Last();

                // Add to the resulting array only if hash is valid
                if (IsValidHash(modelHash))
                {
                    return modelHash;
                }
                else
                {
                    return "false";
                }
            }
            else
            {
                if (IsValidHash(stringToClean))
                {
                    return stringToClean;
                }
                else
                {
                    return "false";
                }
            }
        }

        public bool IsValidHash(string hashToCheck)
        {
            if ((hashToCheck.Length < 26) | (hashToCheck.Length > 32))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
