# EFL# XML Interfaces

This is a suite of libraries and tools to work with EFL#-based XML UI files.

It will be composed of:

- [x] Helper tool to extract API information from EFL# based libraries.
- [ ] XML validation library
- [ ] Code generation compiler
- [ ] Other tools

# Building and testing

EXML# currently uses [Meson](https://mesonbuild.com/), Mono and EFL# (the latter
for testing purposes for now, builtin) for building. Dotnet core integration is
also planned.

```
$ meson build
$ cd build
$ ninja
$ meson test
```

# EXML# Specification

EXML is a simple XML format inspired by
[Xamarin Android](https://docs.microsoft.com/en-us/xamarin/android/)'s AXML.
Elements are either _Containers_ (elements with children containers or
widgets, like Boxes, Spotlights, etc) or _Widgets_ (like Button, Radio, etc).

Sample EXML# file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<exml xmlns:efl="http://www.enlightenment.org/EXML">
<Box efl:orientation="Vertical">
        <Button efl:text="Ok" />
        <Button efl:text="Cancel" />
</Box>
</exml>
```

# Validation Library Architecture

## Getting API data

It is handled through reflection by the `ApiDump` component. It loads
data from the binding `efl-mono.dll` to an internal model akin to
the library `Eolian-Cxx` used by the binding generator,
with top level collections for:

* Classes
* FunctionPointers
* Enums
* Structs

These items are tagged in the DLL with the `Efl.Eo.BindingEntity` C\#
attribute, which is also used by the binding itself.

For simpler loading and keeping the model class relationships relatively
simple, type references currently are simple strings and methods will be
added for querying it. For example, implemented interfaces information
is stored a list of `TypeRef` (single strings) with the name of the
interfaces.

The data loaded from the DLL can be serialized and deserialized through
C\#'s
[BinaryFormatter](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.formatters.binary.binaryformatter?view=netframework-4.8)
class. Currently the DLL built with the `mono-beta` attribute stands
around 6MB with the serialized metadata around 8MB.

## Reading API Data

TBD

## Validation rules and error reporting

There are two main kinds of errors: Hard and soft errors.

Hard errors generally will mean a XML syntax errors where `XMLReader`
would raise a `XmlException` exception and can’t continue.

Soft errors mean errors in the xml related to the meaning of the
information, like wrong widgets, properties, etc.

### Essential info in error messages

* Line number
    * So the plugin can report back to the user

### Hard errors that stop the validation

* Malformed XML
    * Typically raised as `XmlException`

### Soft (semantic) errors

* Does the tag exist?
    * Check if the tag is an instantiatable class
* If it has children, is it an Container?
    * Check if the parent tag implements `Efl.IContainer`?
* All attributes refer to valid properties?
    * All attributes are listed as properties in the class?
* Is the attribute read only?
    * Read only attributes can’t receive values from XML;
* Naive property type checking
    * Ints vs strings
    * Maybe use C\#'s `Convert` class?
