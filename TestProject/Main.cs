using System;
using Npgsql;
using SqlBuilder;
using SqlBuilder.Postgres;
using System.Collections.Generic;

namespace TestProject
{
	class MainClass
	{
		private static void SetCustomTypes() {
			//SqlBuilder.Types.RegisteredCustomTypes.RegisterCustomType<NodaTime.ZonedDateTime, DateTime>(new ZonedDateTimeUserType());
		}

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
			var t = new WhereConditionGeneratorTreeVisitor<SampleType>("test_table");
			t.Visit(x => x.prop2 > 4 || x.prop1 != "" && x.prop2 == 3);
			
			Console.WriteLine(t.Fragment.ToSqlString());

			/*
			using (var con = GetConnection())
			{
				SetCustomTypes();

				QueryBuilder qb = new QueryBuilder("events_dates_sales");

				qb.AddColumnsOf<EvDateSales>();

				List<EvDateSales> evdates = qb.List<EvDateSales>(con);

				foreach (EvDateSales evdate in evdates)
				{
					Console.WriteLine("{0}: {1}", evdate.saleid, evdate.available_until);
				}
			}*/
		}
	}
}
