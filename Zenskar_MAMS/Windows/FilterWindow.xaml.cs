using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;

namespace Zenskar_MAMS.Windows
{
    public partial class FilterWindow : Window
    {
        private readonly string _columnName;
        private readonly List<FilterItem> _allItems;
        private readonly Action<string, IEnumerable<string>> _onApplyFilter;

        public FilterWindow(string columnName, DataView dataView, Action<string, IEnumerable<string>> onApplyFilter)
        {
            InitializeComponent();
            _columnName = columnName;
            _onApplyFilter = onApplyFilter;

            Title = $"Filter {columnName}";

            // Get unique values for the column
            _allItems = dataView.Table.AsEnumerable()
                .Select(row => row[columnName]?.ToString() ?? string.Empty)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new FilterItem { Value = x, IsSelected = true })
                .ToList();

            ValuesListBox.ItemsSource = _allItems;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.ToLower();
            var filteredItems = _allItems.Where(item =>
                item.Value.ToLower().Contains(searchText)).ToList();
            ValuesListBox.ItemsSource = filteredItems;
        }

        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (_allItems != null)
            {
                foreach (var item in _allItems)
                {
                    item.IsSelected = true;
                }
                ValuesListBox.Items.Refresh();
            }
        }

        private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in _allItems)
            {
                item.IsSelected = false;
            }
            ValuesListBox.Items.Refresh();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedValues = _allItems
                .Where(item => item.IsSelected)
                .Select(item => item.Value)
                .ToList();

            _onApplyFilter(_columnName, selectedValues);
            DialogResult = true;
            Close();
        }


        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _onApplyFilter(_columnName, null);
            DialogResult = false;
            Close();
        }
    }
}