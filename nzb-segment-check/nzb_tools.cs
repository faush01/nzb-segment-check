using System.Xml;

namespace check_headers;

public class NzbTools
{
    public HashSet<string> GetSegments(string nzb_path)
    {
        HashSet<string> message_ids = new HashSet<string>();

        XmlDocument doc = new XmlDocument(); 
        doc.Load(nzb_path);
        XmlNodeList nodes = doc.GetElementsByTagName("segment");
        //int count = 0;
        foreach (XmlNode node in nodes)
        {
            string? id = node.ChildNodes[0]?.InnerText;
            if(!string.IsNullOrEmpty(id))
            {
                //if(count++ % 10 == 0) id += "-rem"; // for testing break every 10th
                message_ids.Add(id);
            }
        }

        return message_ids;
    }
}