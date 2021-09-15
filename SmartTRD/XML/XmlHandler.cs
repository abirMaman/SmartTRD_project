using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SmartTRD.XML
{
    class XmlHandler
    {
        public static void SaveData(object Iclass_A, string path_A)
        {
            StreamWriter stream = null;
            try
            {
                XmlSerializer xml = new XmlSerializer(Iclass_A.GetType());
                stream = new StreamWriter(path_A);
                xml.Serialize(stream, Iclass_A);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }
    }

    public class XmlLoad<T>
    {
        public static Type type;

        public XmlLoad()
        {
            type = typeof(T);
        }
        public T loadData(string fileName_A)
        {
            T res;
            XmlSerializer xml = new XmlSerializer(type);
            FileStream fs = new FileStream(fileName_A, FileMode.Open, FileAccess.Read, FileShare.Read);
            res = (T)xml.Deserialize(fs);
            fs.Close();

            return res;
        }
    }
}

