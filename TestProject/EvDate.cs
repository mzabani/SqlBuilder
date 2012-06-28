using System;
using System.Collections.Generic;

namespace TestProject
{
	public class EvDate
	{
		public int eventdateid;
		
		public string title;
		
		private string _description;
		public string description {
			get
			{
				if (String.IsNullOrEmpty(_description))
					return "Sem descrição";
				
				return _description;
			}
			set
			{
				_description = value;
			}
		}
	}
}
