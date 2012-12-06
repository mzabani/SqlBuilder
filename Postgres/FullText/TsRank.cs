using System;
using System.Collections.Generic;

namespace SqlBuilder.Postgres
{
	[Flags]
	public enum FullTextNormalization
	{
		IgnoreDocumentLength = 0,
		OnePlusLogLength = 1,
		RankOverRankPlusOne = 32
	}

	public class TsRank : ProjectionFragment
	{
		private TsRank(double[] weights, TsVector tsVector, TsQuery tsQuery, FullTextNormalization? normalization)
		{
			this.AppendText("TS_RANK(");

			if (weights != null)
			{
				throw new NotImplementedException("weights != null is not yet implemented for TsRank");
			}

			this.AppendFragment(tsVector)
				.AppendText(",")
				.AppendFragment(tsQuery);

			if (normalization != null)
			{
				this.AppendText(",")
					.AppendParameter((int)normalization.Value);
			}

			this.AppendText(")");
		}

		public TsRank(double[] weights, TsVector tsVector, TsQuery tsQuery, FullTextNormalization normalization)
			: this(weights, tsVector, tsQuery, (FullTextNormalization?)normalization)
		{
		}

		public TsRank(TsVector tsVector, TsQuery tsQuery, FullTextNormalization normalization)
			: this(null, tsVector, tsQuery, normalization)
		{
		}

		public TsRank(TsVector tsVector, TsQuery tsQuery)
			: this(null, tsVector, tsQuery, null)
		{
		}

		// TODO:
		// Constructors that take column names
	}
}
