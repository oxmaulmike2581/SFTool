using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace SketchfabToolCLI
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Check if no arguments were provided
            if (args.Length < 1)
            {
                throw new Exception("Usage: SFTool.exe model_hash output_path");
            }

            // ==================================================

            // Define variables
            string configBaseUrl = "https://sketchfab.com/i/models/";
            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
            string modelHash;

            // ==================================================

            // Support for whole link
            if (args[0].Contains("https://sketchfab.com/"))
            {
                // Split the link with "-" as delimiter
                string[] linkData = args[0].Split(Convert.ToChar("-"));

                // Read the last element of the array
                modelHash = linkData.Last();
            }
            else
            {
                modelHash = args[0];
            }

            // Start a stopwatch
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Print model ID
            Console.WriteLine("Got hash: {0} ({1} chars)", modelHash, modelHash.Length);

            // Create an instances of required classes
            Downloader dn = new Downloader();
            GZip gz = new GZip();

            // ==================================================

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

            // ==================================================

            // Downloading model
            byte[] osgjs = dn.GetFileData(modelConfig["osgjsUrl"]);
            byte[] mfbin = dn.GetFileData(modelConfig["mfbinUrl"]);
            byte[] mfwbin = dn.GetFileData(modelConfig["mfwbinUrl"]);

            // Decompressing model and storing in resulting array
            files.Add("file.osgjs", gz.Decompress(osgjs));
            files.Add("model_file.bin", gz.Decompress(mfbin));
            files.Add("model_file_wireframe.bin", gz.Decompress(mfwbin));

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
            string outputPath = args.Length == 1 ? AppDomain.CurrentDomain.BaseDirectory : args[1];
            string outputDirName = String.Format("{0} (by {1})", modelConfig["modelName"], modelConfig["userName"]);
            string outputDirPath = Path.Combine(outputPath, outputDirName);
            string outputModelDir = Path.Combine(outputDirPath, "meshes");
            string outputTexturesDir = Path.Combine(outputDirPath, "textures");
            string outputAnimsDir = Path.Combine(outputDirPath, "anims");

            // Create directories, flush and move the files, creating the archive
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
                File.Move("file.osgjs", Path.Combine(outputModelDir, "file.osgjs"));
                File.Move("model_file.bin", Path.Combine(outputModelDir, "model_file.bin"));
                File.Move("model_file_wireframe.bin", Path.Combine(outputModelDir, "model_file_wireframe.bin"));

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

                // Print a message
                Console.WriteLine("Written {0} files.", files.Count);
            }
            else
            {
                throw new Exception("Output directory already exists.");
            }

            // ==================================================

            // Stop our stopwatch and print elapsed time
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("Time taken: {0}", elapsedTime);

            // ==================================================

            // Wait for user input to close
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
