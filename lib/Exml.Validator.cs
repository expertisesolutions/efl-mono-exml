using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Schema;
using System.Collections.Generic;

using Exml.Logging;

namespace Exml
{

namespace Validator
{

public static class ExmlValidator
{
    public static List<ValidatorModel.ValidationIssue> Validate(string path)
    {
        using (var file = File.OpenRead(path))
        {
            return Validate(file);
        }
    }

    public static List<ValidatorModel.ValidationIssue> Validate(Stream stream)
    {
        // FIXME Maybe we should parametrize the settings
        var issues = new List<ValidatorModel.ValidationIssue>();
        var settings = new XmlReaderSettings();
        settings.ConformanceLevel = ConformanceLevel.Document;

        using (var reader = XmlReader.Create(stream, settings))
        {
            Stack<XmlModel.Widget> stack = new Stack<XmlModel.Widget>();
            XmlModel.Widget root = null;
            XmlModel.Widget current = null;

            try
            {
                while (reader.Read())
                {

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            Logger.Info($"Got element {reader.Name}");

                            // FIXME: Guarantee there is no more than one root inside exml
                            if (reader.Name == "exml")
                            {
                                continue; // Skip the outer tag
                            }
                            var parent = current;

                            try
                            {
                                current = new XmlModel.Widget(reader.Name, parent);
                            }
                            catch (ValidatorModel.ValidationException ex)
                            {
                                issues.AddRange(ex.Issues);
                                reader.Skip();
                            }

                            if (reader.HasAttributes)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    Logger.Info($"Adding attribute {reader.Name} valued {reader.Value}");
                                    var attributeIssues = current.AddAttribute(reader.Name, reader.Value);
                                    issues.AddRange(attributeIssues);
                                }
                            }

                            if (parent != null)
                            {
                                var parentIssues = parent.AddChild(current);
                                issues.AddRange(parentIssues);
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
                            if (reader.Name == "exml")
                            {
                                continue; // Skip outer tag
                            }
                            current = stack.Pop();
                            Logger.Info($"Popped element {current.Name}");
                            break;
                        default:
                            Logger.Info($"Node {reader.NodeType} with value {reader.Value}");
                            break;
                    }
                }
            }
            catch (XmlException ex)
            {
                issues.Add(new ValidatorModel.ValidationIssue("Failed to read XML file.", ex.Message,
                           ValidatorModel.ValidationIssueSeverity.CriticalError, reader as IXmlLineInfo));
            }

            Logger.Info($"Got tree: {root}");

            return issues;
        }
    }
}

}
}
