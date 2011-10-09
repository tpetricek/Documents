using System;
using System.Collections.Generic;
using System.Linq;

namespace EeekSoft.Functional
{
	public interface IFunctionalEnumerable<T> : IDisposable
	{
		Tuple<T, IFunctionalEnumerable<T>>? GetNext();
	}

	public class FunctionalEnumerator<T> : IEnumerator<T>
	{
		IFunctionalEnumerable<T> _enumerable;

		bool _moved = false;
		IFunctionalEnumerable<T> _current;
		T _currentValue;

		public FunctionalEnumerator(IFunctionalEnumerable<T> val)
		{
			_moved = false;
			_enumerable = val;
			_current = _enumerable;
		}

		public bool MoveNext()
		{
			var next = _current.GetNext();
			if (!next.HasValue) return false;
			_currentValue = next.Value.First;
			_current = next.Value.Second;				
			_moved = true;
			return true;
		}

		public void Reset()
		{
			_current = _enumerable;
		}

		public T Current
		{
			get { 
				if (!_moved) 
					throw new Exception("Enumerator is in incorrect state - call MoveNext first!");
				return _currentValue;
			}
		}

		public void Dispose()
		{
			_enumerable.Dispose();
		}

		object System.Collections.IEnumerator.Current
		{
			get { return Current; }
		}
	}

	public abstract class Sequence<R> : IEnumerable<R>, IFunctionalEnumerable<R>
	{
		internal class Cons<T> : Sequence<T>
		{
			T _value;
			IFunctionalEnumerable<T> _rest;

			public Cons(T value, Sequence<T> rest)
			{
				_value = value; _rest = rest;
			}

			public override Tuple<T, IFunctionalEnumerable<T>>? GetNext()
			{
				return Tuple.New(_value, _rest);
			}			
		}

		internal class Nil<T> : Sequence<T>
		{
			public override Tuple<T, IFunctionalEnumerable<T>>? GetNext()
			{
				return null;
			}
		}

		public IEnumerator<R> GetEnumerator()
		{
			return new FunctionalEnumerator<R>(this);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public abstract Tuple<R, IFunctionalEnumerable<R>>? GetNext();

		void IDisposable.Dispose()
		{ }
	}

	public static class Sequence
	{
		public static Sequence<T> Empty<T>()
		{
			return new Sequence<T>.Nil<T>();
		}

		public static Sequence<T> Cons<T>(T val, Sequence<T> list)
		{
			return new Sequence<T>.Cons<T>(val, list);
		}
	}

	public abstract class LazySequence<R> : IEnumerable<R>, IFunctionalEnumerable<R>
	{
		internal class Cons<T> : LazySequence<T>
		{
			T _value;
			Func<LazySequence<T>> _rest;

			public Cons(T value, Func<LazySequence<T>> rest)
			{
				_value = value; _rest = rest;
			}

			public override Tuple<T, IFunctionalEnumerable<T>>? GetNext()
			{
				return Tuple.New(_value, (IFunctionalEnumerable<T>)_rest());
			}
		}

		internal class Nil<T> : LazySequence<T>
		{
			public override Tuple<T, IFunctionalEnumerable<T>>? GetNext()
			{
				return null;
			}
		}

		public IEnumerator<R> GetEnumerator()
		{
			return new FunctionalEnumerator<R>(this);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public abstract Tuple<R, IFunctionalEnumerable<R>>? GetNext();

		void IDisposable.Dispose()
		{ }
	}
	
	public static class LazySequence
	{
		public static LazySequence<T> Empty<T>()
		{
			return new LazySequence<T>.Nil<T>();
		}

		public static LazySequence<T> Cons<T>(T val, Func<LazySequence<T>> listFunc)
		{
			return new LazySequence<T>.Cons<T>(val, listFunc);
		}
	}
}
