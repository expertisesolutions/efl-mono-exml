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
        var issues = new List<ValidatorModel.ValidationIssue>();

        var full_name = "Efl.Ui." + name;

        _class = s_api.Classes.Find(c => c.Name == full_name || c.Name == name);

        if (_class == null)
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Unknown type {name}", "Type could not be found either in the Efl.Ui namespace or fully qualified",
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

        if (Attributes.TryGetValue(actualName, out var currentValue))
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Attribute \"{attrName}\" is already defined", "",
                                                          ValidatorModel.ValidationIssueSeverity.Warning));
            return issues;
        }

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

            // TODO: Rule: Is Event name well formed (Valid C# Method name)?
        }
        else
        {
            // Properties
            ApiModel.TypeRef propertyType = null;
            bool hasProperty = false;
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
                else
                {
                    propertyType = setter.Parameters[0].Type;
                    hasProperty = true;
                }
            }
            else
            {
                if (!prop.HasSet)
                {
                    issues.Add(new ValidatorModel.ValidationIssue($"Property \"{attrName.Substring(prefix.Length)}\" is not writeable",
                                                                  "",
                                                                  ValidatorModel.ValidationIssueSeverity.Error));
                }
                else
                {
                    propertyType = prop.Type;
                    hasProperty = true;
                }
            }

            if (hasProperty)
            {
                if (propertyType == null)
                {
                    issues.Add(new ValidatorModel.ValidationIssue("Type for \"{attrName}\" is null", "",
                                                                  ValidatorModel.ValidationIssueSeverity.Error));
                }

                CheckValueCompatibility(propertyType, attrName, value, ref issues);
            }
        }

        Attributes[actualName] = value;

        return issues;
    }

    private void CheckValueCompatibility(ApiModel.TypeRef type, string propertyName, string value, ref List<ValidatorModel.ValidationIssue> issues)
    {
        try
        {
            switch (type.Name)
            {
                default: break;
                case "System.String": break;
                case "System.Boolean": Boolean.Parse(value); break;
                case "System.Char": Char.Parse(value); break;
                case "System.SByte": SByte.Parse(value); break;
                case "System.Int16": Int16.Parse(value); break;
                case "System.Int32": Int32.Parse(value); break;
                case "System.Int64": Int64.Parse(value); break;
                case "System.Byte": Byte.Parse(value); break;
                case "System.UInt16": UInt16.Parse(value); break;
                case "System.UInt32": UInt32.Parse(value); break;
                case "System.UInt64": UInt64.Parse(value); break;
                case "System.Single": Single.Parse(value); break;
                case "System.Double": Double.Parse(value); break;
                case "System.Decimal": Decimal.Parse(value); break;
            }
        }
        catch (ArgumentNullException)
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Property \"{propertyName}\" can not have a null value", "",
                                                          ValidatorModel.ValidationIssueSeverity.Error));
        }
        catch (FormatException)
        {
            issues.Add(new ValidatorModel.ValidationIssue($"\"{value}\" is not a valid value for property \"{propertyName}\" of type \"{type.Name}\"", "",
                                                          ValidatorModel.ValidationIssueSeverity.Error));
        }
        catch (OverflowException)
        {
            issues.Add(new ValidatorModel.ValidationIssue($"Value overflow for property \"{propertyName}\" of type \"{type.Name}\"", "",
                                                          ValidatorModel.ValidationIssueSeverity.Error));
        }
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

