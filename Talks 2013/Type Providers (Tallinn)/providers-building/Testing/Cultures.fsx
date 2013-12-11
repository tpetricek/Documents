#r @"..\Cultures.TypeProvider\bin\Debug\Cultures.TypeProvider.dll"

let cult = CultureProvider.Cultures.Estonian
cult.NativeName

System.DateTime.Now.ToString("MMMM", cult)