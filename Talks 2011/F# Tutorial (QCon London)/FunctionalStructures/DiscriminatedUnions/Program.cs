using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscriminatedUnions
{
  // ----------------------------------------------------------------
  // Representing Schedule as a class hierarchy 

  enum ScheduleType { Never, Repeatedly, Once };

  abstract class Schedule {
    public Schedule(ScheduleType tag) { Tag = tag; }
    public ScheduleType Tag { get; private set; }
  }

  class Never : Schedule {
    public Never() : base(ScheduleType.Never) { }
  }

  class Repeatedly : Schedule {
    public Repeatedly(DateTime first, TimeSpan periodicity)
        : base(ScheduleType.Repeatedly) {
      StartDate = first;
      Interval = periodicity;
    }
    public DateTime StartDate { get; private set; }
    public TimeSpan Interval { get; private set; }
  }

  class Once : Schedule {
    public Once(DateTime when) 
      : base(ScheduleType.Once) { EventDate = when; }
    public DateTime EventDate { get; private set; }
  }

  // ----------------------------------------------------------------

  class Program
  {
    static DateTime GetNextOccurrence(Schedule schedule)
    {
      // Switch using the schedule type
      switch (schedule.Tag)
      {
        case ScheduleType.Never:
          return DateTime.MinValue;

        case ScheduleType.Once:
          var once = (Once)schedule;
          return once.EventDate > DateTime.Now ?
            once.EventDate : DateTime.MinValue;

        case ScheduleType.Repeatedly:
          var rp = (Repeatedly)schedule;
          var q = (DateTime.Now - rp.StartDate).TotalSeconds / 
                  rp.Interval.TotalSeconds;
          return rp.StartDate.AddSeconds
            ( rp.Interval.TotalSeconds * 
              (Math.Floor(Math.Max(q, 0.0)) + 1.0));
      }
      throw new InvalidOperationException();
    }

    // --------------------------------------------------------------

    static void Main(string[] args)
    {
      var schedule = new Repeatedly
        (new DateTime(2011, 3, 6), new TimeSpan(7, 0, 0, 0));
      var next = GetNextOccurrence(schedule);
      Console.WriteLine(next);
    }
  }
}
