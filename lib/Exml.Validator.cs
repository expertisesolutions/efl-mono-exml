using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace Exml
{
namespace Validator
{

public class ExmlValidator
{
    public static void Validate(Stream xml, Stream xsd, ValidationEventHandler handler)
    {
        XmlReader schemaReader = XmlReader.Create(xsd);
        XmlSchemaSet schema = new XmlSchemaSet();
        schema.Add(null, schemaReader);

        XmlReaderSettings settings = new XmlReaderSettings();
        settings.Schemas = schema;
        settings.ValidationType = ValidationType.Schema;
        settings.ValidationEventHandler += handler;
        XmlReader xmlReader = XmlReader.Create(xml, settings);
        XDocument doc = XDocument.Load(xmlReader);
    }
}

}
}
