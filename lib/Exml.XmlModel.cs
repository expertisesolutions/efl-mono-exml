using System;
using System.Text;
using System.Collections.Generic;

using Exml.Logging;
using ApiModel = Exml.ApiModel;

namespace Exml
{

namespace XmlModel
{

// Object model for EXML interfaces
public class Widget
{

    private static ApiModel.API s_api;

    public string Name { get; set; }
    public Dictionary<string, string> Attributes { get; set; }
    public List<Widget> Children { get; set; }
    public Widget Parent { get; set; }

    private bool _is_container;
    private ApiModel.Class _class;

    public Widget()
    {
        Attributes = new Dictionary<string, string>();
        Children = new List<Widget>();
    }

    public List<ValidatorModel.ValidationIssue> AddInfo(string name, Widget parent)
    {
        // RULE -  is it a valid widget name?
        string internal_name = name;
        var issues = new List<ValidatorModel.ValidationIssue>();

        if (!internal_name.Contains("."))
        {
            internal_name = "Efl.Ui." + internal_name;
        }

        _class = s_api.Classes.Find(c => c.Name == internal_name);

        if (_class == null)
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Unknown type {name}", "Type could not be found in the Efl.Ui namespace",
                                                          ValidatorModel.ValidationIssueSeverity.Error));
        }

        // TODO: Is it a container?
        if (_class != null)
        {
            _is_container = _class.Interfaces.Find(i => i.Name == "Efl.IPack") != null;
        }

        Name = name;
        Parent = parent;

        return issues;
    }

    public List<ValidatorModel.ValidationIssue> AddAttribute(string attrName, string value)
    {
        var issues = new List<ValidatorModel.ValidationIssue>();

        if (_class == null)
        {
            // Silently fail as we have no way of checking validity of an unkown type.
            return issues;
        }

        if (String.IsNullOrEmpty(attrName))
        {
            issues.Add(new ValidatorModel.ValidationIssue("Null or empty attribute name", "", ValidatorModel.ValidationIssueSeverity.Error));
            return issues; // No sense continue checking from here
        }


        if (!attrName.StartsWith("efl:"))
        {
            // Silently ignore.
            // This must be something else not related to EFL.
            return issues;
        }

        // Skip prefix "efl:" and capitalize first letter
        var prefix = "efl:";
        var actualName =  attrName.Substring(prefix.Length, 1).ToUpper() + attrName.Substring(prefix.Length + 1);

        if (actualName.EndsWith("Evt"))
        {
            // Events
            var evt = _class.Events.Find(e => e.Name == actualName);

            if (evt == null)
            {
                issues.Add(new ValidatorModel.ValidationIssue($"Event \"{attrName.Substring(prefix.Length)}\" does not exist in \"{Name}\"",
                                                              "",
                                                              ValidatorModel.ValidationIssueSeverity.Error));
            }
            // TODO: Actually store this event somewhere and its value for code generation.

            // TODO: Rule: Is Event name well formed (Valid C# Method name)?
        }
        else
        {
            // Properties
            var prop = _class.Properties.Find(p => p.Name == actualName);

            if (prop == null)
            {
                var setter = _class.Methods.Find(m => m.Name == "Set" + actualName);
                // Some properties like `Efl.IText.Text` are not generated as C# property wrappers
                // due to conflicts with some implementing classes like `Efl.Ui.Text` (the constructor
                // would clash with the property wrapper).
                // If this is the case, we check if there is a `SetFoo` method that takes 1 parameter
                // to be used as a replacement.
                if (setter == null || setter.Parameters.Count != 1)
                {
                    // Yep. Isto non ecziste
                    issues.Add(new ValidatorModel.ValidationIssue($"Property \"{attrName.Substring(prefix.Length)}\" does not exist in \"{Name}\"",
                                                                  "",
                                                                  ValidatorModel.ValidationIssueSeverity.Error));
                }
            }
            // TODO: Actually store this property somewhere and its value for code generation.

            // TODO: Rule: Is the property writable?
            // TODO: Rule: Is the value acceptable for the property?
        }

        return issues;
    }

    public List<ValidatorModel.ValidationIssue> AddChild(Widget child)
    {
        var issues = new List<ValidatorModel.ValidationIssue>();
        if (!_is_container)
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Type {Name} is not a container", "It can't have children",
                                                          ValidatorModel.ValidationIssueSeverity.Error));
        }

        // We still add so we can track the invalid information further down
        Children.Add(child);
        return issues;
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

    public static void SetApi(ApiModel.API api)
    {
        s_api = api;
    }
}

} // Model

} // Exml

