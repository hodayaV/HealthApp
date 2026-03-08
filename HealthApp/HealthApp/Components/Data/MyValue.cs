using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Components.Data
{
	public class MyValue
	{
		public string Name { get; set; }
		public int? Num { get; set; }
		public string Units { get; set; }

		public MyValue(string name, string units)
		{
			Name = name;
			Units = units;
		}
	}
}