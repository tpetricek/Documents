#r @"..\IniFile.TypeProvider\bin\Debug\Ini.TypeProvider.dll"

type Ini = IniProvider.IniFile< "browscap.ini" >
let ini = Ini()