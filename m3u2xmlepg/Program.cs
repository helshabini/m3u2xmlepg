using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace m3u2xmlepg
{
    class Program
    {
        public static string path = "/Users/hazemelshabini/Downloads/tv_channels_hazem48463_plus.m3u";
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "help":
                        Help();
                        break;
                    default:
                        if (!Convertm3u(args[0]))
                            Help();
                        break;
                }
            }
            else
            {
                Help();
            }
        }


        public static bool Convertm3u(string path)
        {
            List<string> channels = new List<string>();

            if (!File.Exists(path))
            {

            }

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    while (sr.Peek() >= 0)
                    {
                        var line = sr.ReadLine();
                        if (line.StartsWith("#EXTINF:"))
                        {
                            string name = string.Empty;
                            string logo = string.Empty;
                            string group = string.Empty;

                            if (line.Contains("tvg-name"))
                            {
                                string namepart = line.Substring(line.IndexOf("tvg-name") + 10);
                                name = XmlString(namepart.Substring(0, namepart.IndexOf('"')), true);
                            }
                            else
                            {
                                name = XmlString(line.Split(',')[1], true);
                            }

                            if (line.Contains("tvg-logo"))
                            {
                                string logopart = line.Substring(line.IndexOf("tvg-logo") + 10);
                                logo = XmlString(logopart.Substring(0, logopart.IndexOf('"')), true);
                            }

                            if (line.Contains("group-title"))
                            {
                                string grouppart = line.Substring(line.IndexOf("group-title") + 13);
                                group = XmlString(grouppart.Substring(0, grouppart.IndexOf('"')), true);
                            }

                            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(logo) && !string.IsNullOrWhiteSpace(group))
                            {
                                channels.Add(string.Format("<channel id=\"{0}\"><display-name>{0}</display-name><icon src=\"{1}\" /></channel><programme channel=\"{0}\" start=\"{3}\" stop=\"{4}\"><title lang=\"en\">{0}</title><category>{2}</category><previously-shown/></programme>", name, logo, group, DateTime.UtcNow.ToString("yyyyMMdd")+ "000000 +0000", DateTime.UtcNow.AddYears(1).ToString("yyyyMMdd")+ "000000 +0000"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening/processing m3u file: " + ex.Message);
                return false;
            }

            string output = Path.ChangeExtension(path, "xml");

            try
            {
                if (File.Exists(output))
                {
                    File.Delete(output);
                }

                using (StreamWriter sw = new StreamWriter(output))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                    sw.WriteLine("<tv>");
                    foreach (var channel in channels)
                    {
                        sw.WriteLine(channel);
                    }
                    sw.WriteLine("</tv>");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error creating/writing xml file: " + ex.Message);
                return false;
            }

            Console.WriteLine("Completed!");
            return true;
        }

        public static void Help()
        {
            Console.WriteLine("Welcome to m3u2xmlepg command line utility. The utility builds an xml/epg file from m3u playlists intended for use with media players and systems which require xml/epg to function such as Plex, xTeVe, or Telly. This is useful for adding IPTV m3u lists to these systems, even if the IPTV provider does not have accompanying EPG information. The utility will simply build an xml/epg file, with each channel containing one long running program (1 year).");
            Console.WriteLine();
            Console.WriteLine("Example (Linux/macOS): m3u2xmlepg /home/User/file.m3u");
            Console.WriteLine("Example (Windows): m3u2xmlepg \"C:\\Users\\User\\file.m3u\"");
        }

        public static string XmlString(string text, bool isAttribute = false)
        {
            var sb = new StringBuilder(text.Length);

            foreach (var chr in text)
            {
                if (chr == '<')
                    sb.Append("&lt;");
                else if (chr == '>')
                    sb.Append("&gt;");
                else if (chr == '&')
                    sb.Append("&amp;");

                // special handling for quotes
                else if (isAttribute && chr == '\"')
                    sb.Append("&quot;");
                else if (isAttribute && chr == '\'')
                    sb.Append("&apos;");

                // Legal sub-chr32 characters
                else if (chr == '\n')
                    sb.Append(isAttribute ? "&#xA;" : "\n");
                else if (chr == '\r')
                    sb.Append(isAttribute ? "&#xD;" : "\r");
                else if (chr == '\t')
                    sb.Append(isAttribute ? "&#x9;" : "\t");

                else
                {
                    if (chr < 32)
                        throw new InvalidOperationException("Invalid character in Xml String. Chr " +
                                                            Convert.ToInt16(chr) + " is illegal.");
                    sb.Append(chr);
                }
            }

            return sb.ToString();
        }
    }
}
