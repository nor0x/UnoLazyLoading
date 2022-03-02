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
	public static class ListViewExtensions
	{
		public static DependencyProperty AddLazyLoadingSupportProperty { get; } = DependencyProperty.RegisterAttached(
			"AddLazyLoadingSupport",
			typeof(bool),
			typeof(ListViewExtensions),
			new PropertyMetadata(default(bool), (d, e) => d.Maybe<ListView>(control => OnAddLazyLoadingSupportChanged(control, e))));

		public static bool GetAddLazyLoadingSupport(ListView obj) => (bool)obj.GetValue(AddLazyLoadingSupportProperty);
		public static void SetAddLazyLoadingSupport(ListView obj, bool value) => obj.SetValue(AddLazyLoadingSupportProperty, value);

		private static DependencyProperty IsIncrementallyLoadingProperty { get; } = DependencyProperty.RegisterAttached(
			"IsIncrementallyLoading",
			typeof(bool),
			typeof(ListViewExtensions),
			new PropertyMetadata(default(bool)));

		private static bool GetIsIncrementallyLoading(ListView obj) => (bool)obj.GetValue(IsIncrementallyLoadingProperty);
		private static void SetIsIncrementallyLoading(ListView obj, bool value) => obj.SetValue(IsIncrementallyLoadingProperty, value);

		private static void OnAddLazyLoadingSupportChanged(ListView control, DependencyPropertyChangedEventArgs e)
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
			var lv = (ListView)sender;
			var sv = ExtendedVisualTreeHelper.GetFirstDescendant<ScrollViewer>(lv);
			var loadingThreshold = 0.5;

			sv.ViewChanged += async (s, e) =>
			{

				if (lv.ItemsSource is not ISupportIncrementalLoading source) return;
				if (lv.Items.Count > 0 && !source.HasMoreItems) return;
				if (GetIsIncrementallyLoading(lv)) return;

				if (((sv.ExtentHeight - sv.VerticalOffset) / sv.ViewportHeight) - 1.0 <= loadingThreshold)
				{
					try
					{
						SetIsIncrementallyLoading(lv, true);
						await source.LoadMoreItemsAsync(1);
					}
					catch (Exception ex)
					{
						Console.WriteLine("failed to load more items: " + ex);
					}
					finally
					{
						SetIsIncrementallyLoading(lv, false);
					}
				}
			};
		}
	}
}