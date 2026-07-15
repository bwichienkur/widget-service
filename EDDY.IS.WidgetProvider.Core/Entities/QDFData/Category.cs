using System;
using System.Collections.Generic;
using System.Text;

namespace EDDY.IS.WidgetProvider.Core.Entities.QDFData
{
	public class Category : IMEField
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }

		public int Id { get { return CategoryId; } }
		public string Name { get { return CategoryName; } }

		public Category()
		{ }

		public Category(MatchingEngine.Category c)
		{
			CategoryName = c.CategoryName;
			CategoryId = c.CategoryId;
		}
	}
}
