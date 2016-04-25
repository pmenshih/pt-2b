using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace pt_2b.Models
{
    public class TestBox
    {
        public Test test;
        public int currentQuestion = 0;
        public string disableBack = "disabled";
    }

    [XmlRoot(ElementName = "Test")]
    public class Test
    {
        public int? id { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string code { get; set; }

        [XmlAttribute]
        public string username { get; set; }

        [XmlArray("Questions")]
        [XmlArrayItem("Question", typeof(Question))]
        public Question[] questions { get; set; }

        [XmlIgnore]
        public string raw { get; set; }

        public Test DeserializeFromXmlString(string xml)
        {
            XmlSerializer s = new XmlSerializer(typeof(Test));
            Test t = (Test)s.Deserialize(GenerateStreamFromString(xml));
            //сохраним исходный сценарий анкеты
            t.raw = xml;
            return t;
        }

        public string SerializeToXmlString()
        {
            XmlSerializer s = new XmlSerializer(typeof(Test));
            Utf8StringWriter sw = new Utf8StringWriter();
            
            s.Serialize(sw, this);
            sw.Close();
            return sw.ToString();
        }

        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

    public sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }

    public class Question
    {
        [XmlAttribute]
        public string type { get; set; }

        [XmlAttribute]
        public int position { get; set; }

        [XmlAttribute]
        public string isSecret { get; set; }

        [XmlAttribute]
        public string text { get; set; }

        [XmlArray("Answers")]
        [XmlArrayItem("Answer", typeof(Answer))]
        public Answer[] answers { get; set; }

        [XmlAttribute]
        public string answer = "";

        public string keys = "";
    }
    
    public class Answer
    {
        [XmlAttribute]
        public int position { get; set; }

        [XmlAttribute]
        public int keyto { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class ViewMessage
    {
        public string message { get; set; }
    }

    public class TestAnswer
    {
        public int? id { get; set; }

        public string testCode { get; set; }
        
        public string testUsername { get; set; }
        
        public string raw { get; set; }

        public DateTime dateCreate { get; set; }
    }
}