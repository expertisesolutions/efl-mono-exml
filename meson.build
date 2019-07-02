project('ApiDump', 'cs')

apidump_lib = library('apidump', ['ApiDump.cs', 'ApiDump.Model.cs', 'ApiDump.Logging.cs'])

dumper = executable('apidumper', ['Dumper.cs'], link_with: apidump_lib)
