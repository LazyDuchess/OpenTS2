using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.Ini
{
    /// <summary>
    /// Parses an .ini file.
    /// </summary>
    public class IniFile
    {
        public Dictionary<string, Section> Sections;

        public IniFile(string path)
        {
            Read(File.ReadAllBytes(path));
        }

        public Section GetSection(string name)
        {
            if (Sections.TryGetValue(name, out var section))
                return section;
            return null;
        }

        public string GetProperty(string sectionName, string propertyName)
        {
            var section = GetSection(sectionName);
            if (section == null)
                return string.Empty;
            if (section.KeyValues.TryGetValue(propertyName, out var val))
                return val;
            return string.Empty;
        }

        private void Read(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new StreamReader(ms);
            var sections = new Dictionary<string, Section>();
            Section currentSection = null;
            while(ms.Position < ms.Length)
            {
                var line = reader.ReadLine().Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line[0] == ';') continue;
                if (line[0] == '[' && line[line.Length-1] == ']')
                {
                    currentSection = new Section();
                    sections[line.Substring(1, line.Length - 2)] = currentSection;
                    continue;
                }
                if (currentSection == null) continue;
                var split = line.Split('=');
                var key = split[0].Trim();
                var value = split[1].Trim();
                currentSection.KeyValues[key] = value;
            }
            Sections = sections;
        }
    }
}
