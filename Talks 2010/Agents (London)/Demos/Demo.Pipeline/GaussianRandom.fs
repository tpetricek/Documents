module Demo.ImagePipeline.Utilities

open System

/// Adaptation of Peter J. Acklam's Perl implementation. 
/// See http://home.online.no/~pjacklam/notes/invnorm/
/// This approximation has a relative error of 1.15 Ă— 10â’9 or less. 
let GaussianInverseSimple value =
  
    // Lower and upper breakpoints
    let plow = 0.02425
    let phigh = 1.0 - plow

    let p = if (phigh < value) then 1.0 - value else value
    let sign = if (phigh < value) then -1.0 else 1.0

    if p < plow then
        // Rational approximation for tail
        let c = [| -7.784894002430293e-03; -3.223964580411365e-01;
                    -2.400758277161838e+00; -2.549732539343734e+00;
                    4.374664141464968e+00; 2.938163982698783e+00 |]
        let d = [| 7.784695709041462e-03; 3.224671290700398e-01;
                    2.445134137142996e+00; 3.754408661907416e+00 |]
        let q = Math.Sqrt(-2.0 * Math.Log(p))
        sign * (((((c.[0] * q + c.[1]) * q + c.[2]) * q + c.[3]) * q + c.[4]) * q + c.[5]) /
                                  ((((d.[0] * q + d.[1]) * q + d.[2]) * q + d.[3]) * q + 1.0)

    else
        // Rational approximation for central region
        let a = [| -3.969683028665376e+01; 2.209460984245205e+02;
                    -2.759285104469687e+02; 1.383577518672690e+02;
                    -3.066479806614716e+01; 2.506628277459239e+00 |]
        let b = [| -5.447609879822406e+01; 1.615858368580409e+02;
                    -1.556989798598866e+02; 6.680131188771972e+01;
                    -1.328068155288572e+01 |]
        let q = p - 0.5
        let r = q * q
        (((((a.[0] * r + a.[1]) * r + a.[2]) * r + a.[3]) * r + a.[4]) * r + a.[5]) * q /
                                  (((((b.[0] * r + b.[1]) * r + b.[2]) * r + b.[3]) * r + b.[4]) * r + 1.0)


/// <summary>
/// Calculates an approximation of the inverse of the cumulative normal distribution.
/// </summary>
/// <param name="cumulativeDistribution">The percentile as a fraction (.50 is the fiftieth percentile). 
/// Must be greater than 0 and less than 1.</param>
/// <param name="mean">The underlying distribution's average (i.e., the value at the 50th percentile) (</param>
/// <param name="standardDeviation">The distribution's standard deviation</param>
/// <returns>The value whose cumulative normal distribution (given mean and stddev) is the percentile given as an argument.</returns>
let GaussianInverse cumulativeDistribution mean standardDeviation = 
    if not (0.0 < cumulativeDistribution && cumulativeDistribution < 1.0) then
        raise (new ArgumentOutOfRangeException("cumulativeDistribution"))

    let result = GaussianInverseSimple cumulativeDistribution
    mean + result * standardDeviation

/// Creates a new instance of a normally distributed random value generator
/// using the specified mean and standard deviation and seed.
type GaussianRandom(mean, standardDeviation, ?seed) =
    let random = 
        match seed with 
        | None -> new Random()
        | Some(seed) -> new Random(seed)

        
    /// Samples the distribution and returns a random integer. Returns 
    /// a normally distributed random number rounded to the nearest integer
    member x.NextInteger() =
        int (Math.Floor(x.Next() + 0.5))

    /// Samples the distribution; returns a random sample from a normal distribution
    member x.Next() =
        let mutable x = 0.0

        // Get the next value in the interval (0, 1) 
        // from the underlying uniform distribution
        while x = 0.0 || x = 1.0 do
            x <- random.NextDouble()

        // Transform uniform into normal
        GaussianInverse x mean standardDeviation

