using HealthApp.Components.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthApp.Components.Data
{
	public class FoodTodayService : MyService<FoodTodayService>
	{
		private struct GlassOfWater
		{
			public DateTime Day {  get; set; }
			public int NumOfGlass {  get; set; }
		}
        private static int DAYS_FOR_SAVE = 30;
        public List<FoodToday> FoodsToday { get; set; } = new List<FoodToday>();
		private string FileWater;
		private List<GlassOfWater> GlassOfWaterList = new List<GlassOfWater>();
		private readonly FoodService FService = FoodService.INSTANCE;
		private FoodTodayService()
		{
			SetFileName("foodsToday");
			FileWater = Path.Combine(Environment.GetFolderPath
				(Environment.SpecialFolder.ApplicationData) + "GlassOfWaterList" + ".json");
		}
		private void LoadWater() //Model
		{
			if (File.Exists(FileWater))
			{
				var itemJson = File.ReadAllText(FileWater);
				try
				{
					GlassOfWaterList = JsonSerializer.Deserialize<List<GlassOfWater>>(itemJson) ?? new List<GlassOfWater>();
				}
				catch { }
			}
		}
		public override void SaveItems() //Model
		{
			DeleteOldFoods();
			DeleteOldWater();
			File.WriteAllText(FileS, JsonSerializer.Serialize(FoodsToday));
			File.WriteAllText(FileWater, JsonSerializer.Serialize(GlassOfWaterList));
		}
		public override void InitializeList()
		{
			FoodsToday = new List<FoodToday>();
		}
		public new void LoadItems()
		{
			base.LoadItems();
			LoadWater();
		}
		public override void LoadItemsToTheList(string itemJson)
		{
			FoodsToday.AddRange(JsonSerializer.Deserialize<IEnumerable<FoodToday>>(itemJson) ?? Enumerable.Empty<FoodToday>());
			//InitializeList(); // for restart if necessary
		}
		public int NumGlassBeforNumOfDays(int beforNumOfDays)
		{
			GlassOfWater? glass = GlassBeforNumOfDays(beforNumOfDays);
			if (glass != null) 
			{
				return ((GlassOfWater)glass).NumOfGlass; 
			}
			return 0;
		}
		private GlassOfWater? GlassBeforNumOfDays(int beforNumOfDays)
		{	
			if (GlassOfWaterList != null)
			{
				return GlassOfWaterList.Find(glass => DaysApart(glass.Day) == beforNumOfDays);
			}
			return null;
		}
		private int DeleteGlassOfWater(GlassOfWater? glass) //remove from GlassOfWaterList and return the num of glass
		{
			int num = 0;
			if (glass != null) 
			{
				num = ((GlassOfWater)glass).NumOfGlass;
				GlassOfWaterList.Remove((GlassOfWater)glass);
			}
			return num;
		}
		private void DeleteGlassOfWaterList(List<GlassOfWater> glassList) // remove the all list
		{
			foreach (var glass in glassList)
			{
				DeleteGlassOfWater(glass);
			}
		}
		public int AddGlassOfWater()
		{
			int num = DeleteGlassOfWater(GlassBeforNumOfDays(0)) + 1; // DeleteGlassOfWater return the num of glass
			GlassOfWaterList.Add(new GlassOfWater { Day = DateTime.Today, NumOfGlass = num });
			NotifyStateChanged();
			return num;
		}
		private void DeleteFoodItems(List<FoodToday> foodsForDelete)
		{
			foreach (var foodFD in foodsForDelete)
			{
				FoodsToday.Remove(foodFD);
			}
		}
		private void DeleteOldWater()
		{
			List<GlassOfWater> glassForDelete = new List<GlassOfWater>();
			foreach (var glassOfWater in GlassOfWaterList)
			{
				if (DaysApart(glassOfWater.Day) > DAYS_FOR_SAVE)
				{
					glassForDelete.Add(glassOfWater);
				}
			}
			DeleteGlassOfWaterList(glassForDelete);
		}
		private void DeleteOldFoods()
		{
			List<FoodToday> foodsForDelete = new List<FoodToday>();
			foreach (var food in FoodsToday)
			{
				if (DaysApart(food.Date) > DAYS_FOR_SAVE)
				{
					foodsForDelete.Add(food);
				}
			}
			DeleteFoodItems(foodsForDelete);
		}
		public List<FoodToday> FoodsBeforeNumOfDays(int days)
		{
			List<FoodToday> foodsBeforNumOfDays = new List<FoodToday>();
			if (days >= 0 && days <= DAYS_FOR_SAVE)
			{
				foreach (var food in FoodsToday)
				{
					if (DaysApart(food.Date) == days)
					{
						foodsBeforNumOfDays.Add(food);
					}
				}
			}
			return foodsBeforNumOfDays;
		}
		public void AddFood(int foodId, int amount)
		{
			if (amount > 0)// moust be.
			{
				FoodsToday.Add(new FoodToday(foodId, amount));
				NotifyStateChanged();
			}
		}
		public (double totalPro, double totalFat, double totalCarbo, int totalEnergy, double totalSodium, double totalSugars) TotalValuesBeforNumOfDays(int beforeNumOfDays)
		{
			List<FoodToday> foodsBeforeNumOfDays = FoodsBeforeNumOfDays(beforeNumOfDays);
			return FService.SumFoodsValues(foodsBeforeNumOfDays);
		}
	}
}

