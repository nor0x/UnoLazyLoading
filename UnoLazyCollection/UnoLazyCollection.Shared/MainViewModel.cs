using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Uno.UI.Common;

namespace UnoLazyCollection
{
    public class MainViewModel : ViewModelBase
    {
        List<Item> _allItems;
        int _count = 500;
        ObservableCollection<Item> _listItems;
        PaginatedCollection<Item> _listLazyItems;
        ObservableCollection<Item> _gridItems;
        PaginatedCollection<Item> _gridLazyItems;

        public int Count { get => _count; set => SetProperty(ref _count, value); }
        public ObservableCollection<Item> ListItems { get => _listItems; set => SetProperty(ref _listItems, value); }
        public PaginatedCollection<Item> ListLazyItems { get => _listLazyItems; set => SetProperty(ref _listLazyItems, value); }
        public ObservableCollection<Item> GridItems { get => _gridItems; set => SetProperty(ref _gridItems, value); }
        public PaginatedCollection<Item> GridLazyItems{ get => _gridLazyItems; set => SetProperty(ref _gridLazyItems, value); }

        public ICommand LoadListLazyCommand { get; }
        public ICommand LoadListNonLazyCommand { get; }
        public ICommand LoadGridLazyCommand { get; }
        public ICommand LoadGridNonLazyCommand { get; }
        public ICommand SetupCommand { get; }

        public MainViewModel()
        {
            _allItems = new List<Item>();

            LoadListLazyCommand = new DelegateCommand(LoadListLazy);
            LoadListNonLazyCommand = new DelegateCommand(LoadListNonLazy);
            LoadGridLazyCommand = new DelegateCommand(LoadGridLazy);
            LoadGridNonLazyCommand = new DelegateCommand(LoadGridNonLazy);
            SetupCommand = new DelegateCommand(Setup);
        }

        void Setup()
        {
            _allItems = new List<Item>();
            for (int i = 0; i < Count; i++)
            {
                _allItems.Add(new Item(Guid.NewGuid().ToString("N"), $"https://picsum.photos/seed/item{i}/400/300"));
            }
        }

        void LoadListLazy()
        {
            Setup();
            ListLazyItems = new PaginatedCollection<Item>(
            
                async (start, size) =>
                {
                    var response = await FetchItems(start, size);
                    return response;
                },
                pageSize: 25
            );
        }

        void LoadListNonLazy()
        {
            Setup();
            ListItems = new ObservableCollection<Item>(_allItems);
        }

        void LoadGridLazy()
        {
            Setup();
            GridLazyItems = new PaginatedCollection<Item>(
                async (start, size) =>
                {
                    var response = await FetchItems(start, size);
                    return response;
                },
                pageSize: 25
            );
        }

        void LoadGridNonLazy()
        {
            Setup();
            GridItems = new ObservableCollection<Item>(_allItems);
        }

        async Task<Item[]> FetchItems(int start, int size)
        {
            //simulates some work to get new items
            await Task.Delay(200);
            return _allItems.Skip(start).Take(size).ToArray();
        }
    }

    public record Item(string Name, string Image);
}
