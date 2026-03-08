using System;
using System.Text.Json;

namespace HealthApp.Components.Data
{
	public abstract class MyService<T> where T : class
	{
		protected string FileS { get; set; }
		private static readonly Lazy<T> _INSTANCE = new Lazy<T>(() => Activator.CreateInstance(typeof(T), true) as T);

		public static T INSTANCE => _INSTANCE.Value;
		public event Action? OnChange;

		public MyService()
		{
			SetFileName("");
			//FileS = Path.Combine(Environment.GetFolderPath
			//	(Environment.SpecialFolder.ApplicationData), fileName + ".json");
		}
		protected void SetFileName(string fileName)
		{
			FileS = Path.Combine(Environment.GetFolderPath
			   (Environment.SpecialFolder.ApplicationData), fileName + ".json");
		}
		public abstract void InitializeList();
		public abstract void LoadItemsToTheList(string itemJson);
		public abstract void SaveItems();
		public void LoadItems()
		{
			if (File.Exists(FileS))
			{
				var itemJson = File.ReadAllText(FileS);
				try
				{
					LoadItemsToTheList(itemJson);
					return;             // only if the load not secssecs we go to initializelist
				}
				catch { }
			}
			InitializeList();
		}
		public async void Save()
		{
			SaveItems();
			await Application.Current.MainPage.DisplayAlert("Save", $"saved successful", "OK");
		}
		public static double MyRound(double num)
		{
			return Math.Round(num, 2);
		}
		public static double DaysApart(DateTime day)
		{
			return (DateTime.Today - day).TotalDays;
		}
		public static int? StringToIntOrNull(object? obj)
		{
			int? num = null;
			if (obj != null && obj.ToString() != null)
			{
				try
				{
					num = Int32.Parse(obj.ToString());
				}
				catch { }
			}
			return num;
		}
		public static int StringToInt(object? obj)
		{
			int? num = StringToIntOrNull(obj);
			if (num == null) { return -1; }
			return (int)num;
		}
		public static string EnergyString(int energy) => StringForValue("energy", energy, "calories");
		public static string ProString(double protein) => StringForValue("protein", protein, "g");
		public static string CarboString(double carbo) => StringForValue("carbohydrates", carbo, "g");
		public static string FatString(double fat) => StringForValue("fat", fat, "g");
		public static string SodiumString(double sodium) => StringForValue("sodium", sodium, "mg");
		public static string SugarsString(double sugars) => StringForValue("sugars", sugars, "g");
		private static string StringForValue(string name, double value, string unit)
		{
			return name + ": " + MyRound(value) + " " + unit + ".";
		}
		protected void NotifyStateChanged() => OnChange?.Invoke();
	}
}