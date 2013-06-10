using System;
using System.Linq;
using System.Data;
using SqlBuilder.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace SqlBuilder
{
	/// <summary>
	/// Helps update registers by adding objects to be updated on a per-column basis (selectively).
	/// </summary>
	public class Updater
	{
		private class ObjectAndColumns {
			public string table;
			public string idColumn;
			public GetValue idGetter;
			public Object obj;
			public IDictionary<string, GetValue> chosenPropsOrFields;
		}

		private IList<ObjectAndColumns> regs;

		public void Update<T>(string table, Object obj, Expression<Func<T, Object>> idGetterExpr, params Expression<Func<T, Object>>[] getterExprs) {
			ObjectAndColumns reg = new ObjectAndColumns {
				table = table,
				chosenPropsOrFields = new Dictionary<string, GetValue>(getterExprs.Length),
				idColumn = ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr<T>(idGetterExpr),
				obj = obj
			};

			var tempGetters = ReflectionHelper.FetchGettersOf<T>();

			// Only add the ones we want
			foreach (var getterExpr in getterExprs)
			{
				string propOrFieldName = ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr<T>(getterExpr);
				reg.chosenPropsOrFields.Add(propOrFieldName, tempGetters[propOrFieldName]);
			}
			// Add the id getter
			reg.idGetter = tempGetters[reg.idColumn];

			regs.Add(reg);
		}

		private SqlFragment CreateUpdateStatement(ObjectAndColumns reg) {
			SqlFragment query = new SqlFragment("UPDATE " + reg.table + " SET ");
			
			// Update fields section
			for (int i = 0; i < reg.chosenPropsOrFields.Count; i++)
			{
				var getter = reg.chosenPropsOrFields.ElementAt(i);

				if (i == reg.chosenPropsOrFields.Count - 1)
					query.AppendText("{0}=", getter.Key)
						 .AppendParameter(getter.Value(reg.obj));
				else
					query.AppendText("{0}=", getter.Key)
						 .AppendParameter(getter.Value(reg.obj))
						 .AppendText(",");
			}

			// WHERE id=?
			query.AppendText(" WHERE {0}=", reg.idColumn)
				 .AppendParameter(reg.idGetter(reg.obj));

			return query;
		}

		public int ExecuteUpdateStatements(IDbConnection con)
		{
			// Check for no updates
			if (regs.Count == 0)
				return 0;

			// Creating the command and the parameters
			IDictionary<string, object> parameters = new Dictionary<string, object>(regs.Count * 2);
			IDictionary<object, int> parametersIdx = new Dictionary<object, int>(regs.Count * 2);

			// The StringBuilder with all the UPDATE statements
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			using (IDbCommand com = con.CreateCommand())
			{
				int parameterIdx = 0;
				foreach (ObjectAndColumns reg in regs)
				{
					SqlFragment frag = CreateUpdateStatement(reg);
					sb.Append(frag.ToSqlString(ref parameterIdx, parameters, parametersIdx) + ";");
				}

				// Defines the command text, composed of all the updates
				com.CommandText = sb.ToString();

				foreach (var param in parameters)
				{
					IDbDataParameter com_param = com.CreateParameter();
					com_param.ParameterName = param.Key;
					com_param.Value = param.Value;
					com.Parameters.Add(com_param);
				}

				Console.WriteLine("Executing the following updates:\n{0}", com.CommandText);

				return com.ExecuteNonQuery();
			}
		}

		public Updater() {
			this.regs = new List<ObjectAndColumns>(50);
		}
	}
}
