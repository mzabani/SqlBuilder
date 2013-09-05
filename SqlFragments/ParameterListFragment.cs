using System;
using SqlBuilder;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;

namespace SqlBuilder
{
	public class ParameterListFragment : SqlFragment
	{
		private bool Finalized;
		private bool HasAtLeastOneParameter;

		public ParameterListFragment AddParameter(object val) {
			if (Finalized)
				throw new InvalidOperationException("This IN condition is already finished. You either called Finish() or used one of the constructors that receive a list.");

			if (HasAtLeastOneParameter)
				AppendText(", ");
			else
				AppendText("(");

			HasAtLeastOneParameter = true;
			AppendParameter(val);
			return this;
		}

		public ParameterListFragment Finish() {
			AppendText(")");
			Finalized = true;
			return this;
		}

		public ParameterListFragment() : base() {
			Finalized = false;
			HasAtLeastOneParameter = false;
		}

		public ParameterListFragment(IEnumerable values) : this()
		{
			foreach (object val in values)
				AddParameter(val);

			Finish();
		}
	}
}
