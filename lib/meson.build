apidump_lib = library('apidump', ['ApiDump.cs', 'ApiDump.Model.cs', 'ApiDump.Logging.cs'])

validator_lib = library('exmlval', ['Exml.Validator.cs'], cs_args : ['/reference:System.Xml.Linq.dll'], link_with: apidump_lib)

mono_path = meson.current_build_dir()
