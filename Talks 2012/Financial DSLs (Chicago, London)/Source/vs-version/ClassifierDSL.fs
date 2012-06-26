module ClassifierDSL 
open System

type Classifier<'T> = PT of ((DateTime * float)[] -> 'T)

module Price =
  // ----------------------------------------------------------------
  // Simple classifiers that extract value or check property

  /// Checks whether the price is rising over the whole checked range
  let rising = PT (fun input ->
    input |> Seq.pairwise |> Seq.forall (fun ((_, a), (_, b)) -> b >= a))

  /// Checks whether the price is declining over the whole checked range
  let declining = PT (fun input ->
    input |> Seq.pairwise |> Seq.forall (fun ((_, a), (_, b)) -> b <= a))

  /// Gets the minimum over the whole range
  let minimum = PT (fun input ->
    input |> Seq.map snd |> Seq.min)

  /// Gets the maximum over the whole range
  let maximum = PT (fun input ->
    input |> Seq.map snd |> Seq.max)

  /// Checks that the price is at least the specified value in the whole range
  let atLeast min = PT (Seq.forall (fun (_, v) -> v >= min))

  /// Checks that the price is at most the specified value in the whole range
  let atMost max = PT (Seq.forall (fun (_, v) -> v <= max))

  // ----------------------------------------------------------------
  // Composing multiple classifiers

  /// Checks two properties of subsequent parts of the input
  /// (the results are combined using specified boolean operator)
  let sequenceUsing op (PT f1) (PT f2) = PT (fun input ->
    let length = input.Length
    let input1 = input.[0 .. length/2 - (if length%2=0 then 1 else 0)]
    let input2 = input.[length/2 .. length-1]
    op (f1 input1) (f2 input2))

  /// Checks that two properties both hold over the whole input
  let parallelUsing op (PT f1) (PT f2) = PT (fun input -> 
    op (f1 input) (f2 input))

  /// Checks that two properties hold for subsequent parts of the input
  let sequenceAnd = sequenceUsing (&&)
  /// Checks that two properties hold for the same input
  let parallelAnd = parallelUsing (&&)
  /// Checks that one of the properties holds for subsequent parts of the input
  let sequenceOr = sequenceUsing (||)
  /// Checks that one of the properties holds for the same input
  let parallelOr = parallelUsing (||)

  let (-&-) = sequenceAnd
  let (-|-) = sequenceOr
  let (=&=) = parallelAnd 
  let (=|=) = parallelOr

  /// Checks that the price is withing a specified range over the whole input
  let inRange min max = (atLeast min) =&= (atMost max)

  /// Checks that the property holds over an approximation 
  /// obtained using linear regression
  let regression (PT f) = PT (fun values ->
    // TODO: Use date time in case it is not linear
    let xavg = float (values.Length - 1) / 2.0
    let yavg = Seq.averageBy snd values
    let sums = values |> Seq.mapi (fun x (_, v) -> 
      (float x - xavg) * (v - yavg), pown (float x - xavg) 2)
    let v1 = Seq.sumBy fst sums
    let v2 = Seq.sumBy snd sums
    let a = v1 / v2
    let b = yavg - a * xavg 
    values |> Array.mapi (fun x (dt, _) -> (dt, a * (float x) + b)) |> f)

/// Does the property hold over the entire data set?
let run (PT f) (data:(DateTime * float)[]) = 
  f data