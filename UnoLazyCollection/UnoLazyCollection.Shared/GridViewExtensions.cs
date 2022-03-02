using System;
using System.Collections.Generic;
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

namespace UnoLazyCollection
{
	public static class GridViewExtensions
	{
		public static DependencyProperty AddLazyLoadingSupportProperty { get; } = DependencyProperty.RegisterAttached(
			"AddLazyLoadingSupport",
			typeof(bool),
			typeof(GridViewExtensions),
			new PropertyMetadata(default(bool), (d, e) => d.Maybe<GridView>(control => OnAddLazyLoadingSupportChanged(control, e))));

		public static bool GetAddLazyLoadingSupport(GridView obj) => (bool)obj.GetValue(AddLazyLoadingSupportProperty);
		public static void SetAddLazyLoadingSupport(GridView obj, bool value) => obj.SetValue(AddLazyLoadingSupportProperty, value);

		private static DependencyProperty IsIncrementallyLoadingProperty { get; } = DependencyProperty.RegisterAttached(
			"IsIncrementallyLoading",
			typeof(bool),
			typeof(GridViewExtensions),
			new PropertyMetadata(default(bool)));

		private static bool GetIsIncrementallyLoading(GridView obj) => (bool)obj.GetValue(IsIncrementallyLoadingProperty);
		private static void SetIsIncrementallyLoading(GridView obj, bool value) => obj.SetValue(IsIncrementallyLoadingProperty, value);

		private static void OnAddLazyLoadingSupportChanged(GridView control, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
			{
				if (control.IsLoaded)
				{
					InstallIncrementalLoadingWorkaround(control, null!);
				}
				else
				{
					control.Loaded += InstallIncrementalLoadingWorkaround;
				}
			}
		}

		private static void InstallIncrementalLoadingWorkaround(object sender, RoutedEventArgs _)
		{
			var gv = (GridView)sender;
			var sv = ExtendedVisualTreeHelper.GetFirstDescendant<ScrollViewer>(gv);
			var loadingThreshold = 0.8;

			sv.ViewChanged += async (s, e) =>
			{
				if (gv.ItemsSource is not ISupportIncrementalLoading source) return;
				if (gv.Items.Count > 0 && !source.HasMoreItems) return;
				if (GetIsIncrementallyLoading(gv)) return;

				if (((sv.ExtentHeight - sv.VerticalOffset) / sv.ViewportHeight) - 1.0 <= loadingThreshold)
				{
					try
					{
						SetIsIncrementallyLoading(gv, true);
						await source.LoadMoreItemsAsync(1);
					}
					catch (Exception ex)
					{
						Console.WriteLine("failed to load more items: " + ex);
					}
					finally
					{
						SetIsIncrementallyLoading(gv, false);
					}
				}
			};
		}
	}
}