using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.Json;

namespace Zenskar_MAMS.Windows
{
    public partial class RequestDetailsWindow : Window
    {
        private readonly Dictionary<string, object> _currentValues;
        private readonly Dictionary<string, object> _requestedValues;
        private readonly Action _onApprove;
        private readonly Action _onReject;
        RequestsWindow RequestsWindow;

        public RequestDetailsWindow(Dictionary<string, object> currentValues, string updatedDataJson, Action onApprove = null, Action onReject = null)
        {
            InitializeComponent();
            _currentValues = currentValues;
            _requestedValues = JsonSerializer.Deserialize<Dictionary<string, object>>(updatedDataJson);
            _onApprove = onApprove;
            _onReject = onReject;

            PopulateComparisonPanels();
        }

        private void PopulateComparisonPanels()
        {
            foreach (var key in _currentValues.Keys)
            {
                var displayName = FormatPropertyName(key);
                var currentValue = FormatValue(_currentValues[key]);
                var requestedValue = FormatValue(_requestedValues.ContainsKey(key) ? _requestedValues[key] : null);
                bool hasChanged = currentValue != requestedValue;

                // Add current value
                AddValueToPanel(CurrentValuesPanel, displayName, currentValue, hasChanged ? Brushes.Gray : null);

                // Add requested value
                if (_requestedValues.ContainsKey(key))
                {
                    AddValueToPanel(RequestedValuesPanel, displayName, requestedValue, 
                        hasChanged ? (SolidColorBrush)Application.Current.Resources["SuccessBrush"] : null);
                }
            }
        }

        private string FormatPropertyName(string propertyName)
        {
            var result = System.Text.RegularExpressions.Regex.Replace(propertyName, "([A-Z])", " $1").Trim();
            return char.ToUpper(result[0]) + result.Substring(1);
        }

        private string FormatValue(object value)
        {
            if (value == null || value == DBNull.Value)
                return "-";

            if (value is JsonElement element)
            {
                switch (element.ValueKind)
                {
                    case JsonValueKind.String:
                        return element.GetString() ?? "-";
                    case JsonValueKind.Number:
                        return element.GetInt32().ToString();
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return element.GetBoolean().ToString();
                    default:
                        return "-";
                }
            }

            if (value is DateTime dateTime)
                return dateTime.ToShortDateString();

            return value.ToString();
        }

        private void AddValueToPanel(StackPanel panel, string label, string value, Brush highlightColor = null)
        {
            var container = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

            var labelBlock = new TextBlock
            {
                Text = label,
                Foreground = (SolidColorBrush)Application.Current.Resources["TextSecondaryBrush"],
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var valueBlock = new TextBlock
            {
                Text = value,
                Foreground = highlightColor ?? (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"],
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            container.Children.Add(labelBlock);
            container.Children.Add(valueBlock);
            panel.Children.Add(container);
        }

        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            _onApprove?.Invoke();
            DialogResult = false;
            Close();
        }

        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            _onReject?.Invoke();
            DialogResult = false;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
            Close();
        }
    }
}