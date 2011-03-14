using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace LinqFormlets
{
  #region Functional utils

  struct Unit {
    public static Unit Value = new Unit();
  }

  #endregion

  #region Formlet declaration

  public class Environment : Dictionary<string, string> {
    public Environment(IEnumerable<Tuple<string, string>> values) : base() {
      foreach (var tup in values) this.Add(tup.Item1, tup.Item2);
    }
  }

  public class FormletResult<T> {
    public IEnumerable<XNode> Elements { get; set; }
    public Func<Environment, T> Collector { get; set; }
    public int Counter { get; set; }
  }

  public class Formlet<T> {
    public Func<int, FormletResult<T>> Func { get; set; }

    public T Evaluate(Environment env) {
      return Func(0).Collector(env);
    }

    public XDocument Render() {
      return new XDocument(new XElement("div", Func(0).Elements));
    }
  }

  #endregion

  static class Formlet
  {
    #region Idiom operations

    public static Formlet<T> Create<T>(Func<int, FormletResult<T>> f) {
      return new Formlet<T> { Func = f };
    }

    public static Formlet<Tuple<T1, T2>> Merge<T1, T2>(this Formlet<T1> first, Formlet<T2> second) {
      return Formlet.Create(i => {
        var v1 = first.Func(i);
        var v2 = second.Func(v1.Counter);
        return new FormletResult<Tuple<T1, T2>> {
          Elements = Enumerable.Concat(v1.Elements, v2.Elements),
          Collector = env => Tuple.Create(v1.Collector(env), v2.Collector(env)),
          Counter = v2.Counter
        };
      });
    }

    public static Formlet<T> Return<T>(T v) {
      return Formlet.Create(i => new FormletResult<T> {
        Elements = Enumerable.Empty<XElement>(),
        Collector = env => v,
        Counter = i
      });
    }

    public static Formlet<R> Select<T, R>(this Formlet<T> source, Func<T, R> func) {
      return Formlet.Create(i => {
        var value = source.Func(i);
        return new FormletResult<R> {
          Elements = value.Elements,
          Collector = env => func(value.Collector(env)),
          Counter = value.Counter
        };
      });
    }

    #endregion

    #region HTML Form Controls

    public static Formlet<Unit> Text(string s) {
      return Formlet.Create(i => new FormletResult<Unit> {
        Elements = new List<XNode> {
          new XText(s)
        },
        Collector = env => Unit.Value,
        Counter = i + 1
      });
    }

    public static Formlet<string> Input() {
      return Formlet.Create(i => new FormletResult<string> {
        Elements = new List<XElement> {
          new XElement("input", new XAttribute("name", "frmel" + i))
        },
        Collector = env => env["frmel" + i],
        Counter = i + 1
      });
    }

    public static Formlet<Unit> Tag(string name, object attrs = null) {
      return Formlet.Create(i => new FormletResult<Unit> {
        Elements = new List<XElement> {
          attrs == null ?
          new XElement(name) :
          new XElement(name, 
            from prop in attrs.GetType().GetProperties()
            let val = prop.GetValue(attrs, new object[0])
            select new XAttribute(prop.Name, val))
        },
        Collector = env => Unit.Value,
        Counter = i
      });
    }

    public static Formlet<string> DropDown(IEnumerable<Tuple<string, string>> values) {
      return Formlet.Create(i => new FormletResult<string> {
        Elements = new List<XElement> {
          new XElement("select", 
            new XAttribute("name", "frmel" + i),
            values.Select(o => 
              new XElement("option", o.Item2,
                new XAttribute("value", o.Item1) )))
        },
        Collector = env => env["frmel" + i],
        Counter = i + 1
      });
    }

    public static Formlet<string> DropDown(IEnumerable<string> values) {
      return Formlet.Create(i => new FormletResult<string> {
        Elements = new List<XElement> {
          new XElement("select", 
            new XAttribute("name", "frmel" + i),
            values.Select(o => 
              new XElement("option", o)))
        },
        Collector = env => env["frmel" + i],
        Counter = i + 1
      });
    }

    public static Formlet<int> DropDownRange(int from, int count) {
      return Formlet.
        DropDown(Enumerable.Range(from, count).Select(i => i.ToString())).
        Select(s => Int32.Parse(s));
    }

    #endregion

    #region LINQ syntax support

    public static Formlet<TResult> Join<TOuter, TInner, TKey, TResult>
      (this Formlet<TOuter> outer, Formlet<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector) {
      return outer.Merge(inner).Select(tup => resultSelector(tup.Item1, tup.Item2));
    }

    #endregion
  }
}