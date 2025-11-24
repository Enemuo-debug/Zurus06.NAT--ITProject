using System.Collections.Generic;

namespace server.tools
{
    public class GraphData
    {
        public List<Device> Devices { get; set; }
        public List<Link> Links { get; set; }
    }

    public class Device
    {
        public string Type { get; set; }
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class Link
    {
        public LinkEnd From { get; set; }
        public LinkEnd To { get; set; }
    }

    public class LinkEnd
    {
        public string Type { get; set; }
        public int Id { get; set; }
    }
}
