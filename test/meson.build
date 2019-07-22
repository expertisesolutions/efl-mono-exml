sample_lib = library('samplelib', ['TestLibrary.cs'])

test_lib = library('testlib', ['TestUtils.cs'])

apidump_suite = executable('apidump_test_suite',
                           ['ApiDump.Test.cs'],
                           link_with: [exml_lib, test_lib])

validator_suite = executable('validator_test_suite',
                             ['Exml.Validator.Test.cs'],
                             link_with: [exml_lib, test_lib])

mono_path = get_option('mono-path')

if mono_path == ''
  mono_path = join_paths(meson.current_source_dir(), 'samples')
endif

efl_mono_path = ''

foreach path : mono_path.split(':')
    message('Searching for efl-dll in ' + path)
    path = join_paths(path, 'efl_mono.dll')

    if run_command('test', '-f', path).returncode() == 0
        efl_mono_path = path
        break
    endif

endforeach

if efl_mono_path == ''
    error('Couldn\'t find EFL# dll. Aborting.')
else
    message('EFL# location: ' + efl_mono_path)
endif

testenv = environment()

testenv.prepend('MONO_PATH', exml_mono_path)

test('api test', apidump_suite, args: [sample_lib.full_path()], env: testenv)
test('validator test', validator_suite, args: [join_paths(meson.current_source_dir(), 'samples'), efl_mono_path], env: testenv)
