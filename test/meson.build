test_lib = library('testlib', ['TestLibrary.cs'])

testsuite = executable('testsuite', ['Test.cs', 'TestUtils.cs'], link_with: apidump_lib)

testenv = environment()

testenv.prepend('MONO_PATH', mono_path)

test('api test', testsuite, args: [test_lib.full_path()], env: testenv)
