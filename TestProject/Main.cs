using System;
using Npgsql;
using SqlBuilder;

namespace TestProject
{
	class MainClass
	{
		private static NpgsqlConnection GetConnection() {
			NpgsqlConnection con = new NpgsqlConnection("Server=localhost;Port=5432;User=postgres;Database=dezege");
			con.Open();
			
			return con;
		}
		
		public static void Main (string[] args)
		{
			using (var con = GetConnection())
			{
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
		}
	}
}
