using System;
using System.Text;
using System.Collections.Generic;

using Exml.Logging;

namespace Exml
{

namespace XmlModel
{

// Object model for EXML interfaces
public class Widget
{
    public string Name { get; set; }
    public Dictionary<string, string> Attributes { get; set; }
    public List<Widget> Children { get; set; }
    public Widget Parent { get; set; }

    public Widget(string name, Widget parent)
    {
        // TODO: Rule -  is it a valid widget name?
        Name = name;
        // TODO: Is it a container?
        Attributes = new Dictionary<string, string>();
        Children = new List<Widget>();
        Parent = parent;
    }

    public List<ValidatorModel.ValidationIssue> AddAttribute(string name, string value)
    {
        // TODO: Rule: Does the property exist?
        // TODO: Rule: Is the property writable?
        // TODO: Rule: Is the value acceptable for the property?
        return new List<ValidatorModel.ValidationIssue>();
    }

    public List<ValidatorModel.ValidationIssue> AddChild(Widget child)
    {
        // TODO: Can we add this child (Is this a container?)?
        Children.Add(child);
        return new List<ValidatorModel.ValidationIssue>();
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

} // Model

} // Exml

