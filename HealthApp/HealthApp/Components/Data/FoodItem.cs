using System.ComponentModel.DataAnnotations;

namespace HealthApp.Components.Data
{
	public class FoodItem
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "food name is necessary.")]
		[StringLength(200, ErrorMessage = "The food name cannot be more than 200 characters")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public String Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		[Required(ErrorMessage = "protein value is necessary.")]
		[Range(0, 100, ErrorMessage = "protein value should be between 0 and 100")]
		public double Protein { get; set; } // Num of protein in g
		[Required(ErrorMessage = "total fat value is necessary.")]
		[Range(0, 100, ErrorMessage = "total fat value should be between 0 and 100")]
		public double Total_fat { get; set; } // Num of total fat in g
		[Required(ErrorMessage = "carbohydrates value is necessary.")]
		[Range(0, 100, ErrorMessage = "Carbohydrates value should be between 0 and 100")]
		public double Carbohydrates { get; set; } // Num of Carbohydrates in g
		[Required(ErrorMessage = "energy value is necessary.")]
		[Range(0, 100, ErrorMessage = "Energy value should be between 0 and 100")]
		public int Energy { get; set; }// calories
		[Required(ErrorMessage = "total sugars value is necessary.")]
		[Range(0, 1000, ErrorMessage = "total sugars value should be between 0 and 100")]
		public double Total_sugars { get; set; } // in gram
		[Required(ErrorMessage = "sodium value is necessary.")]
		[Range(0, 100, ErrorMessage = "Sodium value should be between 0 and 100")]
		public double Sodium { get; set; } // in milligram

		public void CopyFoodItem(FoodItem food)
		{
			Id = food.Id;
			Name = food.Name;
			Protein = food.Protein;
			Total_fat = food.Total_fat;
			Carbohydrates = food.Carbohydrates;
			Energy = food.Energy;
			Total_sugars = food.Total_sugars;
			Sodium = food.Sodium;
		}
	}
}
