sample_lib = library('samplelib', ['TestLibrary.cs'])

test_lib = library('testlib', ['TestUtils.cs'])

apidump_suite = executable('apidump_test_suite',
                           ['ApiDump.Test.cs'],
                           link_with: [apidump_lib, test_lib])

testenv = environment()

testenv.prepend('MONO_PATH', mono_path)

test('api test', apidump_suite, args: [sample_lib.full_path()], env: testenv)
