using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthApp.Components.Data
{
	public class MyProfileService : MyService<MyProfileService>
	{
		public enum Gender
		{
			Male,
			Female
		}
		public enum Activity
		{
			Inactive = 1200,
			PartlyActive = 1375,
			Active = 1550,
			VeryActive = 1725
		}

		public struct MoreValues
		{
			public Gender? MyGender { get; set; }
			public Activity? MyActivity { get; set; }
			public bool BloodPressure { get; set; }
			public bool Diabetes { get; set; }
		}
		public List<MyValue> MyValues { get; set; } = new List<MyValue>();
		public string FileMoreValues { get; }
		public List<Activity> Activitys { get; set; } = new List<Activity>();
		public MoreValues MoreValuesItem { get; set; } = new MoreValues();
		public Gender? MyGender { get; set; }
		public Activity? MyActivity { get; set; }
		public double? BMI { get; set; }
		private int? BMR { get; set; }
		public int? TDEE { get; set; }
		public bool MyBloodPressure
		{
			get => MoreValuesItem.BloodPressure;
			set => SetBloodPDiabetes(value, MyDiabetes);
		}
		public bool MyDiabetes
		{
			get => MoreValuesItem.Diabetes;
			set => SetBloodPDiabetes(MyBloodPressure, value);
		}
		private const string WEIGHT_S = "Weight", AGE_S = "Age", HEIGHT_S = "Height";
		public const string MALE_S = "Male", FEMALE_S = "Female";

		private MyProfileService()
		{
			SetFileName("myProfile");
			FileMoreValues = Path.Combine(Environment.GetFolderPath
				(Environment.SpecialFolder.ApplicationData) + "MoreValues" + ".json");
		}
		private void SaveMyValues() //Model
		{
			File.WriteAllText(FileS, JsonSerializer.Serialize(MyValues));

		}
		private void SaveMoreValueItem(Activity? act, Gender? gender, bool bloodP, bool diab) //Model
		{
			MoreValuesItem = new MoreValues { MyActivity = act, MyGender = gender, BloodPressure = bloodP, Diabetes = diab };
			File.WriteAllText(FileMoreValues, JsonSerializer.Serialize(MoreValuesItem));
		}

		public override void LoadItemsToTheList(string itemJson)
		{
			List<MyValue> loadMyValues = new List<MyValue>();
			loadMyValues.AddRange(JsonSerializer.Deserialize<IEnumerable<MyValue>>(itemJson) ?? Enumerable.Empty<MyValue>());
			//These actions are mainly for development, in case of a change it is necessary to change only the initial values
			InitializeList(); // Initialization of the goals defined in the initial list.
			SyncMyValues(loadMyValues);//Synchronization with the list we loaded from the file.
		}
		public override void InitializeList()
		{
			InitializeMyValues();
			InitializeActivitys();
		}
		public new void LoadItems()
		{
			base.LoadItems();
			LoadMoreValues(); // for reset- skip on this	
			//InitializeMyValues(); //for reset if nesesery
			CalculateBMI();
			CalculateTDEE();
		}
		

		public override void SaveItems()
		{
			SaveMoreValueItem(MyActivity, MyGender, MyBloodPressure, MyDiabetes);// for cheange in gender and activity
			SaveMyValues();
			CalculateBMI();
			CalculateTDEE();
			NotifyStateChanged();
		}
		private void SetBloodPDiabetes(bool bloodP, bool diab)
		{
			SaveMoreValueItem(MyActivity, MyGender, bloodP, diab);
			NotifyStateChanged();
		}
		
		private void InitializeMyValues()
		{
			MyValue Weight = new MyValue(WEIGHT_S, "kg");
			MyValue height = new MyValue(HEIGHT_S, "cm");
			MyValue age = new MyValue(AGE_S, "years");
			MyValues = new List<MyValue>() { Weight, height, age };
		}
		private void InitializeActivitys()
		{
			Activitys = new List<Activity>() { Activity.Inactive,
				Activity.PartlyActive, Activity.Active, Activity.VeryActive };
		}
		private void LoadMoreValues()
		{
			if (File.Exists(FileMoreValues))
			{
				var itemJson = File.ReadAllText(FileMoreValues);
				try
				{
					MoreValuesItem = JsonSerializer.Deserialize<MoreValues>(itemJson);
				}
				catch { }
			}
			UpdateMorData(MoreValuesItem);
		}
		private void UpdateMorData(MoreValues moreData)
		{
			MyGender = moreData.MyGender;
			MyActivity = moreData.MyActivity;
			MyDiabetes = moreData.Diabetes;
			MyBloodPressure = moreData.BloodPressure;
		}
		public bool IsMyGenger(string gender) => SearchGenderByName(gender) == MyGender;
		public bool IsMyActivity(Activity activ) => activ == MyActivity;
		public void UpdateGender(string? genderString)
		{
			MyGender = SearchGenderByName(genderString);
			CalculateTDEE();//depends on MyGender
		}
		public void UpdateActivity(Activity activity)
		{
			MyActivity = activity;
			CalculateTDEE(); //depends on MyActivity
		}
		private static Gender? SearchGenderByName(string? genderString)
		{
			if (genderString == MALE_S) { return Gender.Male; }
			if (genderString == FEMALE_S) { return Gender.Female; }
			return null;
		}
		private MyValue? SearchMyValueByName(string? myValueName)
		{
			return MyValues.Find(myValue => myValue.Name == myValueName);
		}

		private void SyncMyValues(List<MyValue> myValues)// for control in chenged only in InitializePersonalData().even if the FileS exist
		{
			foreach (MyValue myValue in myValues)
			{
				SyncMyValue(myValue);
			}
		}
		private void SyncMyValue(MyValue value) // cheange the num in MyValues to the num in value
		{
			if (value.Num != null)
			{
				MyValue? myValue = SearchMyValueByName(value.Name);
				if (myValue != null) { myValue.Num = value.Num; }
			}
		}
		private int? MyValueNum(string value)
		{
			MyValue? myValue = MyValues.Find(v => v.Name.Contains(value));
			if (myValue == null) { return null; }
			return myValue.Num;
		}
		public int? Weight() => MyValueNum(WEIGHT_S);
		public int? Age() => MyValueNum(AGE_S);
		public int? Height() => MyValueNum(HEIGHT_S);
		public string? MeaningOfBMI()
		{
			int? age = Age();
			if (age is null) { return null; }
			switch (age)
			{
				// the valu acording Ministry of Health.
				case >= 75:
					return MeaningOfBMI(23.01, 28, 30);
					break;
				case >= 65:
					return MeaningOfBMI(22, 27, 30);
					break;
				default:
					return MeaningOfBMI(18.5, 25, 30);
					break;
			}
		}
		private string? MeaningOfBMI(double underweight, double normal, double overweight)
		{
			if (BMI == null) { return null; }
			if (underweight > BMI) { return " underweight"; }
			if (normal > BMI) { return " normal weight"; }
			if (overweight > BMI) { return " overweight"; }
			return " obesity";
		}
		private void CalculateBMI()
		{
			int? weight = Weight(), height = Height();
			if (!IsNullOrZero(new List<double?> { weight, height }))
			{
#pragma warning disable CS8629 // Nullable value type may be null.
				BMI = (int)weight / Math.Pow(((double)height / 100), 2.0); //Calculate BMI 
#pragma warning restore CS8629 // Nullable value type may be null.
			}
			else { BMI = null; }
		}
		private void CalculateBMR() //acording harris & Benedict
		{
			int? weight = Weight(), age = Age(), height = Height();
			if (!IsNullOrZero(new List<double?> { age, weight, height }))
			{
				var BMRCoef = BMRCoefficientsByGender();
#pragma warning disable CS8629 // Nullable value type may be null.
				BMR = (int)(BMRCoef.Coef + weight * BMRCoef.WeightCoef + height * BMRCoef.HeightCoef + age * BMRCoef.AgeCoef);
#pragma warning restore CS8629 // Nullable value type may be null.
			}
			else { BMR = null; }

		}
		private void CalculateTDEE()
		{
			CalculateBMR();
			if (!(IsNullOrZero(new List<double?> { BMR }) || MyActivity == null))
			{
#pragma warning disable CS8629 // Nullable value type may be null.
				TDEE = (int)BMR * (int)MyActivity.Value / 1000;
#pragma warning restore CS8629 // Nullable value type may be null.
			}
		}
		public static Boolean IsNullOrZero(List<double?> doubles)
		{
			foreach (double? num in doubles)
			{
				if (num == null || num == 0) { return true; }
			}
			return false;
		}
		private (double WeightCoef, double HeightCoef, double AgeCoef, double Coef) BMRCoefficientsByGender() //Coefficients acording harris & Benedict
		{
			double WeightC = 0, HeightC = 0, AgeC = 0, Coefficient = 0;
			switch (MyGender)
			{
				case Gender.Male:
					WeightC = 13.397;
					HeightC = 4.799;
					AgeC = -5.677;
					Coefficient = 88.362;
					break;
				case Gender.Female:
					WeightC = 9.247;
					HeightC = 3.098;
					AgeC = -4.336;
					Coefficient = 447.593;
					break;
			}
			return (WeightC, HeightC, AgeC, Coefficient);
		}
		public string RecommendationsString(string recommendations)
		{
			string recoS = string.Empty;
			if (MyDiabetes)
			{
				recoS = RecMyDiabetesFor(recommendations);
			}
			if (MyBloodPressure)
			{
				recoS = RecMyBloodPressureFor(recommendations, recoS);
			}
			return recoS + ".";
		}
		private static string RecMyDiabetesFor(string recommendations)
		{
			switch (recommendations)
			{
				case ("PhysicalActivity"):
					return "Aerobic MyActivity: 30-60 minutes 4-6 times a week at light to moderate intensity, strength training: 2-3 sessions. Priority next to aerobic training.";
					
				case ("dietPrefer"):
					return " Potassium(zucchini, banana, orange, tomato, pumpkin, collard greens, melon, kiwi), Calcium";
					
				case ("dietReduce"):
					return "Sugar, trans fat, Animal fat, salt";
					
				case ("Additional"):
					return "Smoking prevention";
			}
			return "";
		}
		private string RecMyBloodPressureFor(string recommendations, string recommendationsS)
		{
			switch (recommendations)
			{
				case ("PhysicalActivity"):
					if (!MyDiabetes)
						{ return "Aerobic MyActivity: for at least 30 minutes 5 times a week."; }
					break;
				case ("dietPrefer"):
					if (MyDiabetes) { recommendationsS = ","; }
					return recommendationsS + " Whole grains, dietary fiber, vegetables";
					
				case ("dietReduce"):
					if (MyDiabetes) { recommendationsS.Replace("Animal fat, salt", ""); }
					return recommendationsS + "Animal fat, sodium up to 2 grams";
					
				case ("Additional"):
					return "Smoking prevention up to 5 cups of coffee or tea, Up to" + AlcoholPerGender() + "of alcohol per Day";
								
			}
			return recommendationsS;
		}
		private string AlcoholPerGender()
		{
			if (MyGender == Gender.Male)
			{
				return " 2 servings";
			}
			else { return "1 serving"; }
		}
	}
}
