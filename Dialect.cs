using System;

namespace SqlBuilder
{
	public static class Dialect
	{
		/// <summary>
		/// The string that is prepended to an identifier to identify it as a named parameter in a SqlCommand.
		/// </summary>
		public static string ParameterChar = ":";
	}
}
