using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SFTool
{
    public class Downloader
    {
        // Resulting arrays. Used in GetConfig.
        Dictionary<string, string> modelData = new Dictionary<string, string>();
        Dictionary<string, SortedDictionary<int, string>> texData = new Dictionary<string, SortedDictionary<int, string>>();
        Dictionary<string, string> animData = new Dictionary<string, string>();

        // Download models, textures, animations
        public byte[] GetFileData(string url)
        {
            // Create an instance of the WebClient class
            WebClient wc = new WebClient();

            // Download file as byte array
            return wc.DownloadData(url);
        }

        // Clean the name from restricted characters
        public static string CleanName(string name)
        {
            if (name.Length <= 0)
            {
                throw new Exception("The name must contain at least one character.");
            }

            name = name.Replace('"', '=');
            name = name.Replace('*', 'x');
            name = name.Replace('?', '7');
            name = name.Replace(":", " -");
            name = name.Replace('\\', '-');
            name = name.Replace('/', '-');
            name = name.Replace('ä', 'a');
            name = name.Replace('Ä', 'A');
            name = name.Replace('é', 'e');
            name = name.Replace('É', 'E');
            name = name.Replace('Š', 'S');
            name = name.Replace("|", " -");
            name = name.Replace('ł', 'l');

            return name;
        }

        // Download configuration files and return parsed data
        public dynamic GetConfig(string url, string configType)
        {
            // Perform a request to get the file data
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // If the server answered with code 200 - continue.
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Define some variables
                Dictionary<string, dynamic> jsonData;
                string dataFromStream;

                // Deserialize our data
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Read our stream and convert it to UTF-8 string
                    using (StreamReader reader = new StreamReader(dataStream, Encoding.UTF8))
                    {
                        dataFromStream = reader.ReadToEnd();
                    }

                    // Deserialize
                    JObject jObject = JObject.Parse(dataFromStream);
                    jsonData = jObject.ToObject<Dictionary<string, dynamic>>();

                    // Work with the config based on its content
                    switch (configType)
                    {
                        case "model": 
                            // Set up variables
                            foreach (KeyValuePair<string, dynamic> kvp in jsonData)
                            {
                                switch (kvp.Key)
                                {
                                    case "name":
                                        modelData.Add("modelName", CleanName(kvp.Value.ToString()));
                                        break;

                                    case "user":
                                        modelData.Add("userName", kvp.Value.GetValue("username").ToString());
                                        break;

                                    case "files":
                                        modelData.Add("osgjsUrl", kvp.Value[0]["osgjsUrl"].ToString());
                                        break;
                                }
                            }

                            // Add additional links
                            modelData.Add("mfbinUrl", modelData["osgjsUrl"].Replace("file.osgjs.gz", "model_file.bin.gz"));
                            modelData.Add("mfwbinUrl", modelData["osgjsUrl"].Replace("file.osgjs.gz", "model_file_wireframe.bin.gz"));

                            break;

                        case "textures": 
                            // Define variables
                            List<dynamic> texJsonData = new List<dynamic>();
                            
                            // Process JSON data
                            foreach (KeyValuePair<string, dynamic> kvp in jsonData)
                            {
                                // Get "results" array and process it
                                foreach (JObject kvp2 in kvp.Value)
                                {
                                    // Add an array with data about each texture to the general array
                                    texJsonData.Add(kvp2.ToObject<Dictionary<string, dynamic>>());
                                }
                            }

                            // Process each texture array
                            foreach (var texList in texJsonData)
                            {
                                // Removing String.Empty gives me error CS0165
                                string texName = String.Empty;

                                foreach (KeyValuePair<string, dynamic> tex in texList)
                                {
                                    if (tex.Key == "name")
                                    {
                                        // Convert this value to proper type even if it already stored in this type
                                        texName = tex.Value.ToString();
                                    }

                                    else if (tex.Key == "images")
                                    {
                                        SortedDictionary<int, string> heightDict = new SortedDictionary<int, string>(Comparer<int>.Create((x, y) => y.CompareTo(x)));
                                        
                                        if (!texData.ContainsKey(texName))
                                        {
                                            texData.Add(texName, heightDict);
                                        }

                                        foreach (JObject t in tex.Value)
                                        {
                                            // Convert these values to proper type even if they are already stored in these types
                                            int texHeight = Convert.ToInt32(t.GetValue("height"));
                                            string texUrl = t.GetValue("url").ToString();

                                            // Add the data to the storage
                                            if (!heightDict.ContainsKey(texHeight))
                                            {
                                                heightDict.Add(texHeight, texUrl);
                                            }
                                        }
                                    }
                                }
                            }

                            break;

                        case "anims": 
                            // Define variables
                            List<dynamic> animJsonData = new List<dynamic>();

                            // Process JSON data
                            foreach (KeyValuePair<string, dynamic> kvp in jsonData)
                            {
                                // Get "results" array and process it
                                foreach (JObject kvp2 in kvp.Value)
                                {
                                    // Add an array with data about each animation to the general array
                                    animJsonData.Add(kvp2.ToObject<Dictionary<string, dynamic>>());
                                }
                            }

                            // Process each animation array
                            foreach (var animsList in animJsonData)
                            {
                                string animName = String.Empty;
                                string animUrl = String.Empty;

                                foreach (KeyValuePair<string, dynamic> anim in animsList)
                                {
                                    if (anim.Key == "name")
                                    {
                                        animName = CleanName(anim.Value.ToString());
                                    }

                                    if (anim.Key == "url")
                                    {
                                        animUrl = anim.Value.ToString();
                                    }
                                }

                                if (!animData.ContainsKey(animName))
                                {
                                    animData.Add(animName, animUrl);
                                }
                            }

                            break;
                    }
                }

                // Close the stream to prevent memory leaks.
                response.Close();

                // Return our data
                switch (configType)
                {
                    case "model":
                        return modelData;
                    case "textures":
                        return texData;
                    case "anims":
                        return animData;
                    default:
                        return new Dictionary<string, dynamic>();
                }
            }

            // Return an error message
            return "ERROR";
        }
    }
}
