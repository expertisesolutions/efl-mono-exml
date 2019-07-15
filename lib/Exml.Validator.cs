using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Schema;
using System.Collections.Generic;

using ApiDump.Logging;

namespace Exml
{
namespace Validator
{

// Internal representation of the EXML while we read it
internal class Widget
{
    public string Name { get; set; }
    public Dictionary<string, string> Attributes { get; set; }
    public List<Widget> Children { get; set; }
    public Widget Parent { get; set; }

    public Widget()
    {
        Attributes = new Dictionary<string, string>();
        Children = new List<Widget>();
    }

    public override String ToString()
    {
        return ToString(0);
    }

    public string ToString(int indent)
    {
        var spaces = new String(' ', 4 * indent);
        var sb = new StringBuilder();

        sb.AppendLine(spaces + $"Widget: {Name}");

        foreach (var entry in Attributes)
        {
            sb.AppendLine(spaces + $"    attrib: {entry.Key} => {entry.Value}");
        }

        if (Children.Count > 0)
        {
            foreach (var child in Children)
            {
                sb.AppendLine(spaces + child.ToString(indent + 1));
            }
        }

        return sb.ToString();
    }
}

public static class ExmlValidator
{
    public static void Validate(string path)
    {
        using (var file = File.OpenRead(path))
        {
            Validate(file);
        }
    }

    public static void Validate(Stream stream)
    {
        // FIXME Maybe we should parametrize the settings
        var settings = new XmlReaderSettings();
        settings.ConformanceLevel = ConformanceLevel.Document;

        using (var reader = XmlReader.Create(stream, settings))
        {
            Stack<Widget> stack = new Stack<Widget>();
            Widget root = null;
            Widget current = null;

            while (reader.Read())
            {

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        Logger.Info($"Got element {reader.Name}");
                        var parent = current;

                        current = new Widget();
                        current.Parent = parent;
                        current.Name = reader.Name;
                        // Do we need to remember before advancing to the attributes?
                        bool isEmptyElement = reader.IsEmptyElement;

                        if (reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                Logger.Info($"Adding attribute {reader.Name} valued {reader.Value}");
                                current.Attributes[reader.Name] = reader.Value;
                            }
                        }

                        if (parent != null)
                        {
                            parent.Children.Add(current);
                        }

                        if (root == null)
                        {
                            root = current;
                        }

                        if (!reader.IsEmptyElement)
                        {
                            Logger.Info($"Pushing element {current.Name}");
                            stack.Push(current);
                        }
                        else
                        {
                            Logger.Info($"Element {reader.Name} has no children. Not pushing...");
                            current = parent;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        current = stack.Pop();
                        Logger.Info($"Popped element {current.Name}");
                        break;
                    default:
                        Logger.Info($"Node {reader.NodeType} with value {reader.Value}");
                        break;
                }
            }

            Logger.Info($"Got tree: {root}");
        }
    }
}

}
}
