namespace Monoboy.Desktop.Utility;

using System.Collections.Generic;
using System.IO;

public class IniFile
{
    const string GlobalSection = "__GLOBAL__";
    readonly Dictionary<string, Section> sections = new();

    public Section GetSection(string name)
    {
        if (sections.ContainsKey(name))
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
        if (sections.ContainsKey(GlobalSection))
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

    string ConvertToText()
    {
        string file = string.Empty;

        foreach (Section section in sections.Values)
        {
            if (section.Name != GlobalSection)
            {
                file += "[" + section.Name + "]\n";
            }

            foreach (KeyValuePair<string, string> pairs in section.pairs)
            {
                file += pairs.Key + "=" + pairs.Value + "\n";
            }

            file += "\n";
        }

        return file;
    }

    public void Load(string filepath = "Config.ini")
    {
        sections.Add(GlobalSection, new Section(GlobalSection));
        string currentSection = GlobalSection;

        if (File.Exists(filepath))
        {
            string[] lines = File.ReadAllLines(filepath);

            for (int i = 0; i < lines.Length; i++)
            {
                // Section Header
                if (lines[i].StartsWith("[") && lines[i].EndsWith("]"))
                {
                    currentSection = lines[i].Replace("[", "").Replace("]", "");
                    sections.Add(currentSection, new Section(currentSection));
                }
                else if (!lines[i].StartsWith("#") && !lines[i].StartsWith(";") && lines[i] != "")
                {
                    string[] valuePairs = lines[i].Split('=');
                    string key = valuePairs[0];
                    string value = valuePairs[1];
                    sections[currentSection].SetValue(key, value);
                }
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

    public Dictionary<string, string> pairs = new();

    public Section(string name)
    {
        Name = name;
    }

    public void SetValue(string key, object value)
    {
        if (pairs.ContainsKey(key))
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
        if (pairs.ContainsKey(key))
        {
            return pairs[key];
        }
        else
        {
            return defaultValue;
        }
    }
}
