using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Generic.Collections
{
    public class XMLSerializer<T>
    {
        public static void SerializeObjectToFile(T obj, string Filename)
        {
            File.WriteAllText(Filename, SerializeObjectToString(obj));
        }

        public static T DeserializeObjectFromFile(string Filename)
        {
            return DeserializeObjectFromString(File.ReadAllText(Filename));
        }

        public static string SerializeObjectToString(T obj)
        {
            string xmlString = null;
            MemoryStream memoryStream = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(typeof(T));
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xs.Serialize(xmlTextWriter, obj);
            xmlString = Encoding.UTF8.GetString(memoryStream.ToArray());
            return xmlString;
        }

        public static T DeserializeObjectFromString(string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

            return (T)xs.Deserialize(memoryStream);
        }
    }
}
