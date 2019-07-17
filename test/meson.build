sample_lib = library('samplelib', ['TestLibrary.cs'])

test_lib = library('testlib', ['TestUtils.cs'])

apidump_suite = executable('apidump_test_suite',
                           ['ApiDump.Test.cs'],
                           link_with: [exml_lib, test_lib])

validator_suite = executable('validator_test_suite',
                             ['Exml.Validator.Test.cs'],
                             link_with: [exml_lib, test_lib])

testenv = environment()

testenv.prepend('MONO_PATH', mono_path)

test('api test', apidump_suite, args: [sample_lib.full_path()], env: testenv)
test('validator test', validator_suite, args: [join_paths(meson.current_source_dir(), 'samples')], env: testenv)
