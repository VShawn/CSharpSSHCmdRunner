using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CSharpSSHCmdRunner
{
    public class SSHConnectionInfo
    {
        public string ToXmlString()
        {
            var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(this.GetType());
            serializer.Serialize(stringwriter, this);
            return stringwriter.ToString();
        }
        public void ToXmlFile(string xmlPath)
        {
            if (File.Exists(xmlPath))
                File.Delete(xmlPath);
            Stream s = new FileStream(xmlPath, FileMode.OpenOrCreate);
            using (StreamWriter sw = new StreamWriter(s, Encoding.UTF8))
            {
                sw.Write(ToXmlString());
            }
        }
        /// <summary>
        /// 从XML字符串中构建对象
        /// </summary>
        /// <param name="xmlText"></param>
        /// <returns></returns>
        public SSHConnectionInfo CreateFromXmlString(string xmlText)
        {
            try
            {
                var xmldes = new XmlSerializer(typeof(SSHConnectionInfo));
                return xmldes.Deserialize(new System.IO.StringReader(xmlText)) as SSHConnectionInfo;
            }
            catch (Exception e)
            {

                return null;
            }
        }
        /// <summary>
        /// 从XML文件构建对象
        /// </summary>
        /// <param name="xmlPath">xml文件路径</param>
        /// <returns></returns>
        public SSHConnectionInfo CreateFromXmlFile(string xmlPath)
        {
            if (!File.Exists(xmlPath))
                return null;
            string xml = "";
            using (StreamReader sr = new StreamReader(xmlPath, Encoding.Default))
            {
                xml = sr.ReadToEnd();
            }
            return CreateFromXmlString(xml);
        }


        /// <summary>
        /// 目标计算机IP地址，例如“192.168.0.1”
        /// </summary>
        public string IpArrress = "like:192.168.0.1";

        /// <summary>
        /// SSH 端口
        /// </summary>
        public int Port = 22;

        /// <summary>
        /// SSH 登录用户名
        /// </summary>
        public string UserName = "like:root";

        /// <summary>
        /// SSH 密码
        /// </summary>
        public string Pwd = "like:pwd";
    }
}
