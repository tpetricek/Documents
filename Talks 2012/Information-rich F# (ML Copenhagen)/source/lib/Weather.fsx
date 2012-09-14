[<Measure>]
type celsius

module Copenhagen =
  let temperature = 16.0<celsius>

let celsiusToKelvin (v:float<_>) = 
  ((v / 1.0<celsius>) + 272.15) * 1.0<Microsoft.FSharp.Data.UnitSystems.SI.UnitNames.kelvin>