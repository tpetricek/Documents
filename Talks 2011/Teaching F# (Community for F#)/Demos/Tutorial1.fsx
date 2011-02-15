// ----------------------------------------------------------------------------
// Simple expressions
// ----------------------------------------------------------------------------

// Solving quadratic equations

let a = 3.0
let b = 2.0
let c = 5.0

let d = pown b 2 - 4.0 * a * c

if d < 0.0 then "No real solution"
elif d > 0.0 then "Two solutions"
else "One solution"

let x1 = (-b + sqrt d) / 2.0 * a
let x2 = (-b - sqrt d) / 2.0 * a


// Converting temperature 

module Demo1 = 

  let celsius = 20.0 
  let fahrenheit = celsius * 9.0 / 5.0 + 32.0

module Demo2 = 

  let fahrenheit = 100.0
  let celsius = (fahrenheit - 32.0) * 5.0 / 9.0


// ----------------------------------------------------------------------------
// Introducing functions - converting degrees

let celsiusToFahrenheit degrees = 
  degrees * 9.0 / 5.0 + 32.0

let fahrenheitToCelsius degrees = 
  (degrees - 32.0) * 5.0 / 9.0
