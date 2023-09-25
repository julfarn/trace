#I __SOURCE_DIRECTORY__
#r "lib/net45/Alea.dll"
#r "System.Configuration"
open System.IO
Alea.Settings.Instance.Resource.AssemblyPath <- Path.Combine(__SOURCE_DIRECTORY__, "tools")
Alea.Settings.Instance.Resource.Path <- Path.GetTempPath()
