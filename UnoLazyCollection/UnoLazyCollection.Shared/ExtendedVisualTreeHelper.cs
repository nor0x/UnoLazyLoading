using System.Linq;
using System.Linq.Expressions;
using System.Windows.Input;
using Uno.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

using System.Reflection;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using Uno.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace UnoLazyCollection
{
	public static class ExtendedVisualTreeHelper
	{
		public static T GetFirstDescendant<T>(DependencyObject reference) => GetDescendants(reference)
			.OfType<T>()
			.FirstOrDefault();

		public static T GetFirstDescendant<T>(DependencyObject reference, Func<T, bool> predicate) => GetDescendants(reference)
			.OfType<T>()
			.FirstOrDefault(predicate);

		public static IEnumerable<DependencyObject> GetDescendants(DependencyObject reference)
		{
			foreach (var child in GetChildren(reference))
			{
				yield return child;
				foreach (var grandchild in GetDescendants(child))
				{
					yield return grandchild;
				}
			}
		}

		public static IEnumerable<DependencyObject> GetChildren(DependencyObject reference)
		{
			return Enumerable
				.Range(0, VisualTreeHelper.GetChildrenCount(reference))
				.Select(x => VisualTreeHelper.GetChild(reference, x));
		}
	}
}