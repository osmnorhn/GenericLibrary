using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Generic.Utils.Serialization
{
    public static class SerializationExtensions
    {
        public static string BinarySerialize(this object o)
        {
            if (null != o)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, o);

                    byte[] buffer = stream.ToArray();

                    return Convert.ToBase64String(buffer);
                }
            }
            return null;
        }
        public static object BinaryDeserialize(this string base64)
        {
            byte[] buffer = Convert.FromBase64String(base64);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                IFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
        }

        public static T BinaryDeserialize<T>(this string base64)
        {
            return (T)BinaryDeserialize(base64);
        }


        public static string XmlSerialize(this object obj)
        {
            if (null != obj)
            {
                StringWriter sw = new StringWriter();
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(sw, obj);
                return sw.ToString();
            }

            return null;
        }

        public static T XmlDeserialize<T>(this string xml)
        {
            if (!String.IsNullOrEmpty(xml))
            {
                StringReader sr = new StringReader(xml);
                XmlSerializer xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(sr);
            }
            return default(T);
        }
    }
}
