using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace HealthApp.Components.Data
{
	public class GoalService : MyService<GoalService>
	{
		public struct Target
		{
			public GoalItem.MinMax Minmax { get; set; }
			public double Num { get; set; }
		}
		public struct TargetData
		{
			public string[] Priorities { get; set; }
			public CalculateMyTarget Calculate { get; set; }
			public string Unit { get; set; }
			public TargetData(string[] priorities, CalculateMyTarget calculate, string unit)
			{
				Priorities = priorities;
				Calculate = new CalculateMyTarget(calculate);
				Unit = unit;
			}
		}

#pragma warning disable IDE0044 // Add readonly modifier // Warning removal is for future extensions. can add option for set deltacalorie
		private int DeltaCalorie = 300;
#pragma warning restore IDE0044 // Add readonly modifier
		private const double PRO_PER_KG = 1.6, PRO_PER_KG_FOR_LINES = 2;
		private const string S_BODY_LINES = "Body lines", S_WEIGHT_LOSS = "Weight loss", S_WEIGHT_GAIN = "Weight gain";
		private const string PRO_UNIT = " g of protein", CAL_UNIT = " calories";
		private static readonly string[] PRIO_FOR_CAL_TARGET = [S_WEIGHT_GAIN, S_WEIGHT_LOSS, S_BODY_LINES];
		private static readonly string[] PRIO_FOR_PRO_TARGET = [S_BODY_LINES, S_WEIGHT_GAIN, S_WEIGHT_LOSS];
		public delegate Target CalculateMyTarget(int a, GoalItem b);
		public TargetData ProTargetData = new(PRIO_FOR_PRO_TARGET, CalculateProTarget, PRO_UNIT);
		public TargetData CalTargetData = new(PRIO_FOR_CAL_TARGET, CalculateCalTarget, CAL_UNIT);

		public List<GoalItem> Goals { get; set; } = new List<GoalItem>();
		public static Target PRO_TARGET { get; set; } = new Target();
		public static Target CAL_TARGET { get; set; } = new Target();
		private GoalService()
		{
			SetFileName("goals");
		}
		public override void SaveItems()// Model
		{
			File.WriteAllText(FileS, JsonSerializer.Serialize(Goals));
			NotifyStateChanged();
		}
		public override void LoadItemsToTheList(string itemJson)
		{
			List<GoalItem> goals = new List<GoalItem>();
			goals.AddRange(JsonSerializer.Deserialize<IEnumerable<GoalItem>>(itemJson) ?? Enumerable.Empty<GoalItem>()); //
																														 //These actions are mainly for development, in case of a change it is necessary to change only the initial goals
			InitializeList(); // Initialization of the goals defined in the initial list.
			SyncGoals(goals); //Synchronization with the list we loaded from the file.	
		}
		public override void InitializeList()
		{
			GoalItem.Target calTarForLines = new GoalItem.Target { Minmax = GoalItem.MinMax.Minimum, Num = 0 };
			GoalItem.Target calTarForLoss = new GoalItem.Target { Minmax = GoalItem.MinMax.Maximum, Num = -DeltaCalorie };
			GoalItem.Target calTarForGain = new GoalItem.Target { Minmax = GoalItem.MinMax.Minimum, Num = DeltaCalorie };
			GoalItem.Target ProTarForLines = new GoalItem.Target { Minmax = GoalItem.MinMax.Minimum, Num = PRO_PER_KG_FOR_LINES };
			GoalItem.Target ProTarForLoss = new GoalItem.Target { Minmax = GoalItem.MinMax.Minimum, Num = PRO_PER_KG };
			GoalItem.Target ProTarForGain = new GoalItem.Target { Minmax = GoalItem.MinMax.Minimum, Num = PRO_PER_KG };
			GoalItem bodyLines = new GoalItem(S_BODY_LINES, calTarForLines, ProTarForLines);
			GoalItem weightLoss = new GoalItem(S_WEIGHT_LOSS, calTarForLoss, ProTarForLoss);
			GoalItem weightGain = new GoalItem(S_WEIGHT_GAIN, calTarForGain, ProTarForGain);
			Goals = new List<GoalItem>() { bodyLines, weightLoss, weightGain };
		}
		
		private void SyncGoals(List<GoalItem> goals)// Synchronization with Goals. for control in chenged only in InitializeGoals().even if the FileS exist
		{
			foreach (GoalItem goal in goals)
			{
				SyncGoal(goal);
			}
		}
		private void SyncGoal(GoalItem goal) //Synchronization with Goals. if the goal activ, cheange in Goals to active
		{
			if (goal.IsActive)
			{
				GoalItem? goalItem = SearchGoalByName(goal.Name);
				if (goalItem != null) { goalItem.IsActive = true; }
			}
		}
		private GoalItem? ActiveGoal(string name)// surch goal in Goals (by name). if its active, return the goal 
		{
			GoalItem? goal = SearchGoalByName(name);
			if (goal != null && goal.IsActive) { return goal; }
			return null;
		}
		private GoalItem? SearchGoalByName(string name)
		{
			return Goals.Find(goal => goal.Name == name);
		}
		public static double ProTargetNum() // Note, call this function after the called to Calculate target. 
		{
			return PRO_TARGET.Num;
		}
		public static int CalTargetNum()    // Note, call this function after called to Calculate target. 
		{
			return (int)CAL_TARGET.Num;
		}
		private Target? CalculateTarget(int? TDEEOrWeight, TargetData targetData)
		{
			GoalItem? myGoal = SelectGoalForTarget(targetData.Priorities); // acording important for goal 
			if (myGoal != null && TDEEOrWeight != null && TDEEOrWeight != 0)
			{
				return targetData.Calculate((int)TDEEOrWeight, myGoal);
			}
			return null;
		}
		public string CalculateAndGetTargetString(int? TDEEOrWeight, double ate, TargetData targetData)
		{
			Target? myTar = CalculateTarget(TDEEOrWeight, targetData);
			if (myTar != null)
			{
				return GetTargetString((Target)myTar, targetData.Unit, ate);
			}
			else
			{
				return "Please insert personal data or goal.";
			}
		}
		private static string GetTargetString(Target target, string unit, double ate)
		{
			if (target.Num == 0) { return "Please insert personal data or goal."; }
			return AteFromTargetString(target, unit, ate) + MeetingTheGoalString(target, ate, unit);
		}
		private static string AteFromTargetString(Target target, string unit, double ate)
		{
			return "You ate " + MyRound(ate) + unit + " out of a target of a " + target.Minmax + " of " + MyRound(target.Num) + unit + ". ";
		}
		private static string MeetingTheGoalString(Target target, double ate, string unit)
		{
			switch (ate / target.Num * 100) //Calculating the percentage of ate out of the target num
			{
				case < 70:
					return FarFromTarget(target.Minmax);
					break;
				case < 100:
					return CloseToTheTarget(target.Minmax);
					break;
				default:
					return PassedTarget(target.Minmax);
					break;
			}

		}
		private static string FarFromTarget(GoalItem.MinMax minMax)
		{
			if (minMax == GoalItem.MinMax.Minimum)
			{ return "you need get more"; }
			else { return ""; }
		}


		private static string PassedTarget(GoalItem.MinMax minmax)
		{
			if (minmax == GoalItem.MinMax.Maximum)
			{
				return "You passed your maximum target";
			}
			else
			{
				return "Well done, you have achieved your goal";
			}
		}

		private static string CloseToTheTarget(GoalItem.MinMax minmax)
		{
			if (minmax == GoalItem.MinMax.Maximum)
			{
				return "Note, you are close to the maximum";
			}
			else
			{
				return "In a little while you reach your target";
			}
		}

		private static Target CalculateCalTarget(int TDEE, GoalItem goal)
		{
			return (CAL_TARGET = new Target { Minmax = goal.CalorieTarget.Minmax, Num = (int)TDEE + goal.CalorieTarget.Num });
		}
		private static Target CalculateProTarget(int weight, GoalItem goal)
		{
			return (PRO_TARGET = new Target { Minmax = goal.ProTarget.Minmax, Num = ((int)weight * goal.ProTarget.Num) });
		}
		private GoalItem? SelectGoalForTarget(string[] goalsPriorities)// select acording priorities
		{
			for (int i = 0; i < goalsPriorities.Length; i++)
			{
				GoalItem? goalForTarget = ActiveGoal(goalsPriorities[i]);
				if (goalForTarget != null)
				{
					return goalForTarget;
				}
			}
			return null;
		}
	}
}
