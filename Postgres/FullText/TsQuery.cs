using System;
using System.Collections.Generic;

namespace SqlBuilder.Postgres
{
	public class TsQuery : SqlFragment
	{
		/// <summary>
		/// Represents a full text search query.
		/// </summary>
		/// <param name='configuration'>
		/// The desired DB configuration.
		/// </param>
		/// <param name='queryString'>
		/// A well formed vector string or a plain search string to be normalized.
		/// </param>
		/// <param name='normalize'>
		/// True if the words in the query string are to be ANDed or False if the query string is already well formatted.
		/// </param>
		public TsQuery(string configuration, string queryString, bool normalize)
		{
			if (normalize)
				this.AppendText("PLAINTO_TSQUERY(");
			else
				this.AppendText("TO_TSQUERY(");

			this.AppendParameter(configuration)
				.AppendText(",")
				.AppendParameter(queryString)
				.AppendText(")");
		}

		public TsQuery(TsQuery tsQuery) {
			this.AppendFragment(tsQuery);
		}
	}
}
