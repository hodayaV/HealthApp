//using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace HealthApp.Components.Data
{
	public class FoodService : MyService<FoodService>
	{
		HttpClient MyHttpClient;
		public List<FoodItem> Foods { get; set; } = new List<FoodItem>();
		public List<FoodItem> FoodsPersonal { get; set; } = new List<FoodItem>();

		private FoodService()
		{
			SetFileName("foodItems");
			MyHttpClient = new HttpClient();
		}
		public new async Task LoadItems()  // Model
		{
			var url = "https://gist.githubusercontent.com/hodayaV/4a299a0872858467258e2a3b8710b1a2/raw/876ecf86aff5e603e078b25a83def7a424b74ab2/gistfile1.txt";
			var response = await MyHttpClient.GetAsync(url);
			await CheakResponse(response);
			base.LoadItems();
		}
		public override void SaveItems() // save only the FoodsPersonal // Model
		{
			System.IO.File.WriteAllText(FileS, System.Text.Json.JsonSerializer.Serialize(FoodsPersonal));
		}

		private async Task CheakResponse(HttpResponseMessage? response)
		{
			if (response != null && response.IsSuccessStatusCode)
			{
				await LoadFoods(response);
			}
			else
			{
				await Application.Current.MainPage.DisplayAlert("Error loading data.", $"Please check your internet connection, close the app and log in again", "OK");
			}
		}
		private async Task LoadFoods(HttpResponseMessage? response)
		{
			try
			{
				List<FoodItem>? items = await response.Content.ReadFromJsonAsync<List<FoodItem>>();
				if (items != null) { Foods = items.ToList(); }
			}
			catch { }
			finally
			{
				if (Foods.Count == 0)
				{
					await Application.Current.MainPage.DisplayAlert("Error loading data.", $"Please close the app and log in again", "OK");
				}
			}
		}
		public override void LoadItemsToTheList(string itemJson)// Load the PFoods that each user has entered
		{
			List<FoodItem> loadPersonalFoods = new List<FoodItem>();
			loadPersonalFoods.AddRange(JsonConvert.DeserializeObject<IEnumerable<FoodItem>>(itemJson) ?? Enumerable.Empty<FoodItem>());
			foreach (var food in loadPersonalFoods)
			{
				AddPersonalFood(food);
			}
		}
		public override void InitializeList()
		{
			FoodsPersonal = new List<FoodItem>();
		}

		public void AddPersonalFood(FoodItem food)
		{
			Foods.Add(food);// for use. this list is not for saving
			FoodsPersonal.Add(food); //need add for saveing
		}


		public int GetFreeId()// for personal food. the id < 0
		{
			int newId = 0;
			foreach (var foodPersonal in FoodsPersonal)
			{
				if (foodPersonal.Id < newId)
				{
					newId = foodPersonal.Id;
				}
			}
			return newId - 1;
		}
		private FoodItem SearchFoodById(int id)
		{
			FoodItem? food = Foods.Find(f => f.Id == id);
			if (food != null) { return food; }
			return new FoodItem();
		}
		//public void DeleteFoodItem(FoodItem eatItem)
		//{
		//	//FoodsPersonal.Remove(eatItem);
		//}
		private (double pro, double fat, double carbo, int energy, double sodium, double sugars, string name) FoodValues(FoodToday foodToday)
		{
			FoodItem food = SearchFoodById(foodToday.FoodItemId);
			double pro = foodToday.ValueByAmount(food.Protein);
			double fut = foodToday.ValueByAmount(food.Total_fat);
			double carbo = foodToday.ValueByAmount(food.Carbohydrates);
			int energy = (int)foodToday.ValueByAmount(food.Energy);
			double sodium = foodToday.ValueByAmount(food.Sodium);
			double sugars = foodToday.ValueByAmount(food.Total_sugars);
			string name = food.Name;
			return (pro, fut, carbo, energy, sodium, sugars, name);
		}
		public (double sumPro, double sumFat, double sumCarbo, int sumEnergy, double sumSodium, double sumSugars) SumFoodsValues(List<FoodToday> foodTodayL)
		{
			int sEnergy = 0;
			double sFat = 0, sPro = 0, sCarbo = 0, sSodium = 0, sSugars = 0;
			foreach (var food in foodTodayL)
			{
				var item = FoodValues(food);
				sEnergy += item.energy;
				sFat += item.fat;
				sPro += item.pro;
				sCarbo += item.carbo;
				sSodium += item.sodium;
				sSugars += item.sugars;
			}
			return (sPro, sFat, sCarbo, sEnergy, sSodium, sSugars);
		}
		public string FoodTodayString(FoodToday food)
		{
			var foodValues = FoodValues(food);
			return foodValues.name + " - " + food.Amount + " g.";
		}
		public string FoodTitleForAmount(FoodToday food)
		{
			string valueS = " values for";
			string foodString = FoodTodayString(food);
			int index = foodString.LastIndexOf("-");
			if (index != -1)
			{
				return foodString.Substring(0, index + 1) + valueS + foodString.Substring(index + 1);
			}
			return foodString;
		}
		public string FoodDetailsString(FoodToday foodT)
		{
			var food = FoodValues(foodT);
			string foodDetails = EnergyString(food.energy) + "\n";
			foodDetails += ProString(food.pro) + "\n";
			foodDetails += CarboString(food.carbo) + "\n";
			foodDetails += FatString(food.fat) + "\n";
			foodDetails += SodiumString(food.sodium) + "\n";
			foodDetails += SugarsString(food.sugars) + "\n";
			return foodDetails;
		}
	}
}
