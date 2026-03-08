//using MathNet.Numerics.Financial;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HealthApp.Components.Data
{
	public class FoodToday
	{
		public int FoodItemId { get; set; }
		public int Amount { get; set; } // in g
		public DateTime Date { get; set; }
		public FoodToday(int foodItemId, int amount)
		{
			Amount = amount;
			FoodItemId = foodItemId;
			Date = DateTime.Today;
		}
		public double ValueByAmount(double value)
		{
			return Amount * value / 100;
		}
	}
}

