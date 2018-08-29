using AnimChainsSheetPacker.ProjectDataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AnimChainsSheetPacker
{
    public sealed class ProjectSerialization
    {
        public static Project LoadProject(string filePath)
        {
            Project project;
            XmlSerializer serializer = new XmlSerializer(typeof(Project));
            using (var stream = File.OpenRead(filePath))
            {
                project = serializer.Deserialize(stream) as Project;
            }
            return project;
        }

        public static void SaveProject(Project project, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Project));
            using (TextWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(
                    writer,
                    project
                );
            }
        }
    }
}
