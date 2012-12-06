using System;
using System.Linq;
using System.Data;
using SqlBuilder.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace SqlBuilder
{
	public class MassUpdater<T>
	{
		private string table;
		private string idColumn;
		private GetValue idGetter;

		private IList<T> regs;
		private IDictionary<string, GetValue> chosenPropsOrFields;

		public void AddRegisterToUpdate(T reg)
		{
			regs.Add(reg);
		}

		public int ExecuteUpdateStatement(IDbConnection con)
		{
			SqlFragment query = new SqlFragment("UPDATE " + table + " SET ");
			
			// Update fields section
			for (int i = 0; i < chosenPropsOrFields.Count; i++)
			{
				var getter = chosenPropsOrFields.ElementAt(i);

				if (i == chosenPropsOrFields.Count - 1)
					query.AppendText("{0}=t.{0}", getter.Key);
				else
					query.AppendText("{0}=t.{0},", getter.Key);
			}
			
			// Values and ids section
			query.AppendText(" FROM (VALUES ");
			for (int r = 0; r < regs.Count; r++)
			{
				T reg = regs[r];

				query.AppendText("(");

				for (int i = 0; i < chosenPropsOrFields.Count; i++)
				{
					var getter = chosenPropsOrFields.ElementAt(i);
					query.AppendParameter(getter.Value(reg));

					query.AppendText(",");
				}

				query.AppendParameter(idGetter(reg))
					 .AppendText(")");

				if (r < regs.Count - 1)
					query.AppendText(",");
			}
			query.AppendText(") AS t(");
			
			for (int i = 0; i < chosenPropsOrFields.Count; i++)
			{
				var getter = chosenPropsOrFields.ElementAt(i);
				query.AppendText(getter.Key + ",");
			}
			query.AppendText("id) WHERE " + idColumn + "=t.id");

			// Creating the command and the parameters
			IDictionary<string, object> parameters = new Dictionary<string, object>(regs.Count * (chosenPropsOrFields.Count + 1));
			IDictionary<object, int> parametersIdx = new Dictionary<object, int>(regs.Count * (chosenPropsOrFields.Count + 1));

			using (IDbCommand com = con.CreateCommand())
			{
				com.CommandText = query.ToSqlString(0, parameters, parametersIdx);

				foreach (var param in parameters)
				{
					IDbDataParameter com_param = com.CreateParameter();
					com_param.ParameterName = param.Key;
					com_param.Value = param.Value;
					com.Parameters.Add(com_param);
				}

				return com.ExecuteNonQuery();
			}
		}

		public MassUpdater(string table, Expression<Func<T, Object>> idGetterExpr, params Expression<Func<T, Object>>[] getterExprs) {
			this.table = table;

			chosenPropsOrFields = new Dictionary<string, GetValue>(getterExprs.Length);
			var tempGetters = ReflectionHelper.FetchGettersOf<T>();

			// Only add the ones we want
			foreach (var getterExpr in getterExprs)
			{
				string propOrFieldName = ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr<T>(getterExpr);
				chosenPropsOrFields.Add(propOrFieldName, tempGetters[propOrFieldName]);
			}
			// Add the id getter
			this.idColumn = ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr<T>(idGetterExpr);
			this.idGetter = chosenPropsOrFields[idColumn];

			this.regs = new List<T>(50);
		}
	}
}

