using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PackageTracker.Data.Utilities
{
    public static class XmlUtility<T>
    {
        public static string Serialize(T obj)
        {
            var serializer = new XmlSerializer(obj.GetType());
            using (var textWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings { Indent = true }))
                {
                    serializer.Serialize(xmlWriter, obj);
                    return textWriter.ToString();
                }
            }
        }

        public static T Deserialize(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        public static string SerializeOmitScheme(T obj)
        {
            var serializer = new XmlSerializer(obj.GetType());
            using (var textWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings { OmitXmlDeclaration = true }))
                {
                    var ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    serializer.Serialize(xmlWriter, obj, ns);
                    return textWriter.ToString();
                }
            }
        }
    }
}

