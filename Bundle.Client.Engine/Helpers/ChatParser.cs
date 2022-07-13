using Bundle.Client.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bundle.Client.Helpers
{
    /// <summary>
    /// This class parses JSON chat data from MC 1.6+ and returns the appropriate string to be printed.
    /// </summary>

    public static class ChatParser
    {
        /// <summary>
        /// The main function to convert text from MC 1.6+ JSON to MC 1.5.2 formatted text
        /// </summary>
        /// <param name="json">JSON serialized text</param>
        /// <param name="links">Optional container for links from JSON serialized text</param>
        /// <returns>Returns the translated text</returns>
        public static string ParseText(string json, List<string> links = null)
        {
            return JSONData2String(JsonConv.ParseJson(json), "", links);
        }

        /// <summary>
        /// Get the classic color tag corresponding to a color name
        /// </summary>
        /// <param name="colorname">Color Name</param>
        /// <returns>Color code</returns>
        private static string Color2tag(string colorname)
        {
            switch (colorname.ToLower())
            {
                /* MC 1.7+ Name           MC 1.6 Name           Classic tag */
                case "black":        /*  Blank if same  */      return "§0";
                case "dark_blue":                               return "§1";
                case "dark_green":                              return "§2";
                case "dark_aqua":       case "dark_cyan":       return "§3";
                case "dark_red":                                return "§4";
                case "dark_purple":     case "dark_magenta":    return "§5";
                case "gold":            case "dark_yellow":     return "§6";
                case "gray":                                    return "§7";
                case "dark_gray":                               return "";
                case "blue":                                    return "§9";
                case "green":                                   return "§a";
                case "aqua":            case "cyan":            return "§b";
                case "red":                                     return "§c";
                case "light_purple":    case "magenta":         return "§d";
                case "yellow":                                  return "§e";
                case "white":                                   return "§f";
                default: return "";
            }
        }


        /// <summary>
        /// Use a JSON Object to build the corresponding string
        /// </summary>
        /// <param name="data">JSON object to convert</param>
        /// <param name="colorcode">Allow parent color code to affect child elements (set to "" for function init)</param>
        /// <param name="links">Container for links from JSON serialized text</param>
        /// <returns>returns the Minecraft-formatted string</returns>
        private static string JSONData2String(JsonConv.JSONData data, string colorcode, List<string> links)
        {
            string extra_result = "";
            switch (data.Type)
            {
                case JsonConv.JSONData.DataType.Object:
                    if (data.Properties.ContainsKey("color"))
                    {
                        colorcode = Color2tag(JSONData2String(data.Properties["color"], "", links));
                    }
                    if (data.Properties.ContainsKey("clickEvent") && links != null)
                    {
                        JsonConv.JSONData clickEvent = data.Properties["clickEvent"];
                        if (clickEvent.Properties.ContainsKey("action")
                            && clickEvent.Properties.ContainsKey("value")
                            && clickEvent.Properties["action"].StringValue == "open_url"
                            && !String.IsNullOrEmpty(clickEvent.Properties["value"].StringValue))
                        {
                            links.Add(clickEvent.Properties["value"].StringValue);
                        }
                     }
                    if (data.Properties.ContainsKey("extra"))
                    {
                        JsonConv.JSONData[] extras = data.Properties["extra"].DataArray.ToArray();
                        foreach (JsonConv.JSONData item in extras)
                            extra_result = extra_result + JSONData2String(item, colorcode, links) + "§r";
                    }
                    if (data.Properties.ContainsKey("text"))
                    {
                        return colorcode + JSONData2String(data.Properties["text"], colorcode, links) + extra_result;
                    }
                    else if (data.Properties.ContainsKey("translate"))
                    {
                        List<string> using_data = new List<string>();
                        if (data.Properties.ContainsKey("using") && !data.Properties.ContainsKey("with"))
                            data.Properties["with"] = data.Properties["using"];
                        if (data.Properties.ContainsKey("with"))
                        {
                            JsonConv.JSONData[] array = data.Properties["with"].DataArray.ToArray();
                            for (int i = 0; i < array.Length; i++)
                            {
                                using_data.Add(JSONData2String(array[i], colorcode, links));
                            }
                        }
                        return colorcode + String.Join(" ", using_data) + extra_result;
                    }
                    else return extra_result;

                case JsonConv.JSONData.DataType.Array:
                    string result = "";
                    foreach (JsonConv.JSONData item in data.DataArray)
                    {
                        result += JSONData2String(item, colorcode, links);
                    }
                    return result;

                case JsonConv.JSONData.DataType.String:
                    return colorcode + data.StringValue;
            }

            return "";
        }

        /// <summary>
        /// Do a HTTP request to get a webpage or text data from a server file
        /// </summary>
        /// <param name="url">URL of resource</param>
        /// <returns>Returns resource data if success, otherwise a WebException is raised</returns>
        private static string DownloadString(string url)
        {
            System.Net.HttpWebRequest myRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            myRequest.Method = "GET";
            System.Net.WebResponse myResponse = myRequest.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();
            return result;
        }
    }
}
