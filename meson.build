project('ApiDump', 'cs')

apidump_lib = library('apidump', ['ApiDump.cs', 'ApiDump.Model.cs', 'ApiDump.Logging.cs'])

dumper = executable('apidumper', ['Dumper.cs'], link_with: apidump_lib)

test_lib = library('testlib', ['TestLibrary.cs'])

testsuite = executable('testsuite', ['Test.cs', 'TestUtils.cs'], link_with: apidump_lib)

test('api test', testsuite, args: [test_lib.full_path()])
