using System.Collections.Generic;

namespace HealthApp.Components.Data
{

	public class GoalItem
	{
		public enum MinMax
		{
			Minimum,
			Maximum
		}
		public struct Target
		{
			public MinMax Minmax { get; set; }
			public double Num { get; set; }
		}
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public Target CalorieTarget { get; set; }
		public Target ProTarget { get; set; }
		public GoalItem(string name, Target calorieTarget, Target proTarget)
		{
			Name = name;
			CalorieTarget = calorieTarget;
			ProTarget = proTarget;
		}
	}
}
