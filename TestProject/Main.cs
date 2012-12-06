using System;
using Npgsql;
using SqlBuilder;
using SqlBuilder.Postgres;
using System.Collections.Generic;

namespace TestProject
{
	class MainClass
	{
		private static NpgsqlConnection GetConnection() {
			NpgsqlConnection con = new NpgsqlConnection("Server=localhost;Port=5432;User=application;Database=dezege");
			con.Open();
			
			return con;
		}

		private static void OneToManyWithInts(NpgsqlConnection con) {
			QueryBuilder qb = new QueryBuilder("places_editions");

			qb.Select("places_editions.editionid, places_editions.placeid, places_editions.date")
			  .Select("places_editions_votes.userid AS votersids")
			  .Join("places_editions_votes", "places_editions.editionid", "places_editions_votes.editionid", JoinType.LeftOuterJoin)
			  .Where(new SqlBuilder.Conditions.NullCondition("places_editions.finished_date", false))
			  .Select("places_editions.placeid")
			  .Select("places_editions.date");

			// Mark the one-to-many relation during the return
			IList<PlaceEditionAndVoters> results = qb.List<PlaceEditionAndVoters, int>(con, x => x.votersids);

			foreach (var res in results)
			{
				Console.WriteLine("Editionid {0}", res.editionid);
				foreach (int voterid in res.votersids)
				{
					Console.Write("{0} ", voterid);
				}

				Console.WriteLine();
			}
		}

		private static void OneToManyWithClasses(NpgsqlConnection con) {
			QueryBuilder qb = new QueryBuilder("events");
				
			qb.Select("events.eventid, events_dates.eventdateid, events_dates.title, events_dates.description")
			  .Join("events_dates", "events.eventid", "events_dates.eventid", JoinType.InnerJoin)
			  .Take(2);
			
			foreach (Event res in qb.List<Event, EvDate>(con, x => x.evdates))
			{
				Console.WriteLine("eventid: {0}", res.eventid);
				foreach (EvDate evdate in res.evdates)
				{
					Console.WriteLine("-- title: {0}, desc: {1}", evdate.title, evdate.description);	
				}
			}

			Console.WriteLine("");
		}
		
		public static void Main (string[] args)
		{
			using (var con = GetConnection())
			{
				QueryBuilder qb = new QueryBuilder("events_dates");

				qb.AddColumnsOf<EvDate>("events_dates", x => x.eventdateid, x => x.title);

				qb.Where(Cond.In<EvDate>(x => x.eventdateid, new List<int> { 1, 2, 3, 4 }));
				qb.Where(Cond.IsNotNull<EvDate>(x => x.title));

				IDictionary<string, object> parameters = new Dictionary<string, object>();
				IDictionary<object, int> parametersIdx = new Dictionary<object, int>();

				Console.WriteLine(qb.ToSqlString());

				var tsvector = new TsVector("portuguese", "description", true);
				var tsquery = new TsQuery("portuguese", "comida & bebida", false);

				qb.Where(FullText.Match(tsvector, tsquery))
				  .Where(Cond.NotEqualTo("title", "titulo".ToSqlFragment()));
				qb.OrderBy(new TsRank(tsvector, tsquery));
				qb.Where(Cond.Like("title", "%nome%"));

				Console.WriteLine(qb.ToSqlString());
			}
		}
	}
}
