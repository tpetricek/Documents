open System

// ------------------------------------------------------------------
// Sample source code - function for working with schedule

type Schedule =
    | Never
    | Once of DateTime
    | Repeatedly of DateTime * TimeSpan

let sched = Repeatedly(DateTime(2011, 3, 6), TimeSpan(7, 0, 0, 0))


let getNextOccurrence(schedule) =
  match schedule with
  | Never -> DateTime.MinValue
  | Once(dt) -> 
      if (dt > DateTime.Now) then dt 
      else DateTime.MinValue
  | Repeatedly(startDate, interval) ->
      let q = (DateTime.Now - startDate).TotalSeconds / 
              interval.TotalSeconds
      startDate.AddSeconds
        ( interval.TotalSeconds * 
          (Math.Floor(max q 0.0) + 1.0))
