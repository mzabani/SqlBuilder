using System;
using Npgsql;
using SqlBuilder;
using System.Collections.Generic;

namespace TestProject
{
	class MainClass
	{
		private static NpgsqlConnection GetConnection() {
			NpgsqlConnection con = new NpgsqlConnection("Server=localhost;Port=5432;User=postgres;Database=dezege");
			con.Open();
			
			return con;
		}

		private static void OneToManyWithInts(NpgsqlConnection con) {
			QueryBuilder<NpgsqlCommand> qb = new QueryBuilder<NpgsqlCommand>("places_editions");

			qb.AddSelectColumn("places_editions.editionid, places_editions.placeid, places_editions.date")
			  .AddSelectColumn("places_editions_votes.userid AS votersids")
			  .AddJoin("places_editions_votes", "places_editions.editionid", "places_editions_votes.editionid", JoinType.LeftOuterJoin)
			  .SetWhereCondition(new SqlBuilder.Conditions.NullCondition("places_editions.finished_date", false))
			  .AddOrderByColumn("places_editions.placeid")
			  .AddOrderByColumn("places_editions.date");

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
			QueryBuilder<NpgsqlCommand> qb = new QueryBuilder<NpgsqlCommand>("events");
				
			qb.AddSelectColumn("events.eventid, events_dates.eventdateid, events_dates.title, events_dates.description")
			  .AddJoin("events_dates", "events.eventid", "events_dates.eventid", JoinType.InnerJoin)
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
				OneToManyWithInts(con);
			}
		}
	}
}
