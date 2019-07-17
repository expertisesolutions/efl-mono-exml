using System;
using System.Collections.Generic;
using System.Xml;

using Exml.Logging;

namespace Exml
{

namespace ValidatorModel
{

public enum ValidationIssueSeverity
{
    Warning,
    Error,
    CriticalError,
}

/// <summary>Class to report issues when validating the XML.</summary>
public class ValidationIssue
{
    public int Line { get; private set; }
    public int Position { get; private set; }
    public string Message { get; private set; }
    public string Details { get; private set; }
    public ValidationIssueSeverity Severity { get; private set; }

    public ValidationIssue(string message, string details, ValidationIssueSeverity severity,
                           IXmlLineInfo lineInfo = null)
    {
        Message = message;
        Details = details;
        Severity = severity;

        if (lineInfo != null)
        {
            AddContext(lineInfo);
        }
    }

    public void AddContext(IXmlLineInfo lineInfo)
    {
        if (lineInfo.HasLineInfo())
        {
            Line = lineInfo.LineNumber;
            Position = lineInfo.LinePosition;
            Logger.Info($"Added line info at line {Line} and position {Position}");
        }
    }
}

public class ValidationException : Exception
{
    public List<ValidationIssue> Issues { get; private set; }

    public ValidationException(string message) : base(message)
    {
        Issues = new List<ValidationIssue>();
    }

    public void AddIssue(ValidationIssue issue)
    {
        Issues.Add(issue);
    }
}

} // ValidatorModel

} // Exml
