using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace UnoLazyCollection
{
	public class PaginatedCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
	{
		public delegate Task<T[]> Fetch(int start, int count);

		private readonly Fetch _fetch;
		private int _start, _pageSize;

		public PaginatedCollection(Fetch fetch, int pageSize)
		{
			_fetch = fetch;
			_start = 0;
			_pageSize = pageSize;
		}

		public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
		{
			return Task.Run<LoadMoreItemsResult>(async () =>
			{
				var items = await _fetch(_start, _pageSize);
				await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
				{
					foreach (var item in items)
					{
						Add(item);
						if (Count > _pageSize)
						{
							await Task.Delay(20);
						}
					}
				});

				_start += items.Length;

				return new LoadMoreItemsResult() { Count = (uint)items.Length };
			}).AsAsyncOperation();
		}

		public bool HasMoreItems => true;
	}
}