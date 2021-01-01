using System.Collections.Generic;
using System.IO;

namespace Monoboy.Utility
{
    public class IniFile
    {
        private const string GlobalSection = "__GLOBAL__";
        private Dictionary<string, Section> sections = new Dictionary<string, Section>();

        public Section GetSection(string name)
        {
            if(sections.ContainsKey(name) == true)
            {
                return sections[name];
            }
            else
            {
                sections.Add(name, new Section(name));
                return sections[name];
            }
        }

        public Section GetGlobalSection()
        {
            if(sections.ContainsKey(GlobalSection) == true)
            {
                return sections[GlobalSection];
            }
            else
            {
                sections.Add(GlobalSection, new Section(GlobalSection));
                return sections[GlobalSection];
            }
        }

        public void Save(string filepath = "Config.ini")
        {
            File.WriteAllText(filepath, ConvertToText());
        }

        private string ConvertToText()
        {
            string file = string.Empty;

            foreach(Section section in sections.Values)
            {
                if(section.Name != GlobalSection)
                {
                    file += "[" + section.Name + "]\n";
                }

                foreach(KeyValuePair<string, string> pairs in section.pairs)
                {
                    file += pairs.Key + "=" + pairs.Value + "\n";
                }

                file += "\n";
            }

            return file;
        }

        public void Load(string filepath = "Config.ini")
        {
            string currentSection = GlobalSection;
            string[] lines = File.ReadAllLines(filepath);

            sections.Add(GlobalSection, new Section(GlobalSection));

            for(int i = 0; i < lines.Length; i++)
            {
                // Section Header
                if(lines[i].StartsWith("[") && lines[i].EndsWith("]"))
                {
                    currentSection = lines[i].Replace("[", "").Replace("]", "");
                    sections.Add(currentSection, new Section(currentSection));
                }
                else if(!lines[i].StartsWith("#") && !lines[i].StartsWith(";") && lines[i] != "")
                {
                    string[] valuePairs = lines[i].Split('=');
                    string key = valuePairs[0];
                    string value = valuePairs[1];
                    sections[currentSection].SetValue(key, value);
                }
            }
        }

        public override string ToString()
        {
            return ConvertToText();
        }
    }

    public class Section
    {
        public string Name { get; set; }

        public Dictionary<string, string> pairs = new Dictionary<string, string>();

        public Section(string name)
        {
            Name = name;
        }

        public void SetValue(string key, object value)
        {
            if(pairs.ContainsKey(key) == true)
            {
                pairs[key] = value.ToString();
            }
            else
            {
                pairs.Add(key, value.ToString());
            }
        }

        public string GetValue(string key, string defaultValue = "")
        {
            if(pairs.ContainsKey(key) == true)
            {
                return pairs[key];
            }
            else
            {
                return defaultValue;
            }
        }
    }
}