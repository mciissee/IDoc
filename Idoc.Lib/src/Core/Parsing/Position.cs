#pragma warning disable XS0001 // Find APIs marked as TODO in Mono


namespace Idoc.Lib
{
	public struct Position
	{
		public int Line { get; set; }
		public int Col { get; set; }

		public static bool operator ==(Position a, Position b)
		{
			return a.Line == b.Line && a.Col == b.Col;
		}

		public static bool operator !=(Position a, Position b)
		{
			return a.Line != b.Line || a.Col != b.Col;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Line + Col;
		}
	}
}
