using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Win32;
using System.Linq;
using System.Threading.Tasks;
using EnvironMint.Models;
using EnvironMint.Services;
using System.Windows.Media.Animation;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Xml;
using System.Diagnostics;
using EnvironMint.Controls;
using EnvironMint.Styles;

namespace EnvironMint
{
    public partial class MainWindow : Window
    {
        private List<Tool> selectedTools = new List<Tool>();
        private Tool? currentTool = null;
        private ToolDetectionService toolDetectionService;
        private ToolManagementService toolManagementService;
        private ProjectScannerService projectScannerService;
        private ScriptGeneratorService scriptGeneratorService;
        private Settings appSettings;
        private string? selectedProjectDirectory;

        private NotificationBanner? _currentNotification;
        private Grid? _loadingOverlay;
        private TabControl? _mainTabControl;

        public MainWindow()
        {
            InitializeComponent();

            toolDetectionService = new ToolDetectionService();
            toolManagementService = new ToolManagementService(toolDetectionService);
            scriptGeneratorService = new ScriptGeneratorService();
            projectScannerService = new ProjectScannerService(toolDetectionService, toolManagementService.Tools);

            LoadSettings();

            UpdateToolsList();

            ScriptTypeComboBox.SelectionChanged += ScriptTypeComboBox_SelectionChanged;

            if (ScriptTypeComboBox != null)
            {
                SetScriptTypeFromSettings();
            }

            this.Loaded += MainWindow_Loaded;

            if (appSettings.AutoScanOnStartup)
            {
                Task.Run(async () =>
                {
                    await Dispatcher.InvokeAsync(async () =>
                    {
                        await Task.Delay(1000);
                        await PerformSystemScan();
                    });
                });
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _mainTabControl = FindVisualChild<TabControl>(this, null);

            if (_mainTabControl != null)
            {
                FixComboBoxControls();

                _mainTabControl.SelectionChanged += TabControl_SelectionChanged;
            }
            else
            {
                FixAllComboBoxControls();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FixComboBoxControls();
        }

        private void FixComboBoxControls()
        {
            FixAllComboBoxControls();
            FixSpecificComboBoxes();
        }

        private void FixAllComboBoxControls()
        {
            var comboBoxes = FindVisualChildren<ComboBox>(this);

            foreach (var comboBox in comboBoxes)
            {
                FixSingleComboBox(comboBox);
            }
        }

        private void FixSpecificComboBoxes()
        {
            string[] comboBoxNames = new string[]
            {
                "ToolCategoryComboBox",
                "ScriptTypeComboBox",
                "TargetOSComboBox",
                "DefaultScriptLanguageComboBox"
            };

            foreach (string name in comboBoxNames)
            {
                var comboBox = FindName(name) as ComboBox;
                if (comboBox != null)
                {
                    FixSingleComboBox(comboBox);
                }
            }
        }

        private void FixSingleComboBox(ComboBox comboBox)
        {
            try
            {
                comboBox.Style = null;

                var toggleButton = FindVisualChild<ToggleButton>(comboBox, null);
                if (toggleButton != null)
                {
                    toggleButton.Click += (s, e) =>
                    {
                        comboBox.IsDropDownOpen = !comboBox.IsDropDownOpen;
                    };
                }

                comboBox.PreviewMouseLeftButtonDown += (s, e) =>
                {
                    ComboBox cb = s as ComboBox;
                    if (cb != null)
                    {
                        cb.IsDropDownOpen = !cb.IsDropDownOpen;
                        e.Handled = true;
                    }
                };

                comboBox.IsEditable = comboBox.IsEditable;
                comboBox.StaysOpenOnEdit = true;
                comboBox.IsTextSearchEnabled = true;

                // force update visual state
                comboBox.UpdateLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fixing ComboBox: {ex.Message}");
            }
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T t)
                {
                    yield return t;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent, string? childName) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    if (childName == null || (typedChild is FrameworkElement fe && fe.Name == childName))
                        return typedChild;
                }

                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                    return result;
            }

            return null;
        }
        private void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            if (_currentNotification != null)
            {
                MainGrid.Children.Remove(_currentNotification);
            }

            _currentNotification = new NotificationBanner
            {
                Message = message,
                NotificationType = type,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 80, 20, 0),
                Width = 300
            };

            if (_currentNotification.MessageText != null)
            {
                _currentNotification.MessageText.Text = message;
            }

            Grid.SetRowSpan(_currentNotification, 3);
            MainGrid.Children.Add(_currentNotification);

            _currentNotification.Closed += (s, e) =>
            {
                MainGrid.Children.Remove(_currentNotification);
                _currentNotification = null;
            };
        }

        private void ShowLoadingOverlay(string message = "Loading...")
        {
            if (_loadingOverlay == null)
            {
                _loadingOverlay = new Grid
                {
                    Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0))
                };

                var stackPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var spinner = new LoadingSpinner
                {
                    Width = 60,
                    Height = 60,
                    Margin = new Thickness(0, 0, 0, 15)
                };

                var messageText = new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                stackPanel.Children.Add(spinner);
                stackPanel.Children.Add(messageText);
                _loadingOverlay.Children.Add(stackPanel);

                Grid.SetRowSpan(_loadingOverlay, 3);
                MainGrid.Children.Add(_loadingOverlay);

                _loadingOverlay.Opacity = 0;
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = Animations.EaseOut
                };
                _loadingOverlay.BeginAnimation(OpacityProperty, fadeIn);
            }
        }

        private void HideLoadingOverlay()
        {
            if (_loadingOverlay != null)
            {
                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = Animations.EaseIn
                };

                fadeOut.Completed += (s, e) =>
                {
                    MainGrid.Children.Remove(_loadingOverlay);
                    _loadingOverlay = null;
                };

                _loadingOverlay.BeginAnimation(OpacityProperty, fadeOut);
            }
        }

        private void UpdateStatus(string message)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = Animations.EaseIn
            };

            fadeOut.Completed += (s, e) =>
            {
                StatusTextBlock.Text = message;

                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(150),
                    EasingFunction = Animations.EaseOut
                };

                StatusTextBlock.BeginAnimation(OpacityProperty, fadeIn);
            };

            StatusTextBlock.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void LoadSettings()
        {
            appSettings = Settings.Load();

            PopulateDriveCheckboxes();
            AutoScanCheckBox.IsChecked = appSettings.AutoScanOnStartup;

            foreach (ComboBoxItem item in DefaultScriptLanguageComboBox.Items)
            {
                if (item.Content.ToString() == appSettings.DefaultScriptLanguage)
                {
                    DefaultScriptLanguageComboBox.SelectedItem = item;
                    break;
                }
            }

            toolDetectionService.ScanDrives = appSettings.ScanDrives;
        }

        private void PopulateDriveCheckboxes()
        {
            DrivesCheckBoxPanel.Children.Clear();

            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                try
                {
                    if (drive.IsReady)
                    {
                        System.Windows.Controls.CheckBox driveCheckBox = new System.Windows.Controls.CheckBox
                        {
                            Content = $"{drive.Name} ({drive.VolumeLabel})",
                            Tag = drive.Name.TrimEnd('\\'),
                            Margin = new Thickness(0, 0, 0, 10),
                            IsChecked = appSettings.ScanDrives.Contains(drive.Name.TrimEnd('\\'))
                        };

                        DrivesCheckBoxPanel.Children.Add(driveCheckBox);
                    }
                }
                catch
                {
                    // skip drives causing errors (like removable drives that were removed)
                }
            }

            if (DrivesCheckBoxPanel.Children.Count == 0)
            {
                TextBlock noDrivesMessage = new TextBlock
                {
                    Text = "No drives found. Please ensure you have at least one drive available.",
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = Brushes.Red
                };

                DrivesCheckBoxPanel.Children.Add(noDrivesMessage);
            }
        }

        private void SetScriptTypeFromSettings()
        {
            foreach (ComboBoxItem item in ScriptTypeComboBox.Items)
            {
                if (item.Content.ToString() == appSettings.DefaultScriptLanguage)
                {
                    ScriptTypeComboBox.SelectedItem = item;
                    break;
                }
            }

            SetSyntaxHighlighting(appSettings.DefaultScriptLanguage);
        }

        private void ScriptTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ScriptTypeComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string scriptType = selectedItem.Content?.ToString() ?? "PowerShell";
                SetSyntaxHighlighting(scriptType);

                if (!string.IsNullOrWhiteSpace(ScriptPreviewEditor.Text))
                {
                    GenerateScript_Click(sender, null);
                }
            }
        }

        private void SetSyntaxHighlighting(string scriptType)
        {
            switch (scriptType)
            {
                case "PowerShell":
                    ScriptPreviewEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("PowerShell");
                    break;
                case "Bash":
                    ScriptPreviewEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Bash");
                    break;
                default:
                    ScriptPreviewEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("PowerShell");
                    break;
            }
        }

        private void UpdateToolsList()
        {
            var tools = toolManagementService.Tools;

            ToolsListBox.Items.Clear();
            foreach (var tool in tools)
            {
                ListBoxItem item = new ListBoxItem();
                StackPanel panel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };

                if (tool.IsInstalled)
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = "✓ ",
                        Foreground = Brushes.Green,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 5, 0)
                    });
                }

                panel.Children.Add(new TextBlock
                {
                    Text = tool.Name,
                    VerticalAlignment = VerticalAlignment.Center
                });

                item.Content = panel;
                item.Tag = tool.Name;

                ToolsListBox.Items.Add(item);
            }

            AvailableToolsListBox.Items.Clear();
            foreach (var tool in tools)
            {
                bool isInstalled = toolDetectionService.IsToolInstalled(tool);
                tool.IsInstalled = isInstalled;

                ListBoxItem item = new ListBoxItem();
                StackPanel panel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };

                if (isInstalled)
                {
                    panel.Children.Add(new TextBlock
                    {
                        Text = "✓ ",
                        Foreground = Brushes.Green,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 5, 0)
                    });
                }

                panel.Children.Add(new TextBlock
                {
                    Text = tool.Name,
                    VerticalAlignment = VerticalAlignment.Center
                });

                item.Content = panel;
                item.Tag = tool.Name;

                AvailableToolsListBox.Items.Add(item);
            }

            SelectedToolsListBox.Items.Clear();
            foreach (var tool in selectedTools)
            {
                SelectedToolsListBox.Items.Add(tool.Name);
            }
        }

        private void ClearToolDetails()
        {
            ToolNameTextBox.Text = string.Empty;
            ToolVersionTextBox.Text = string.Empty;
            ToolCategoryComboBox.SelectedIndex = -1;
            InstallCommandTextBox.Text = string.Empty;
            ValidationScriptTextBox.Text = string.Empty;
            currentTool = null;
        }

        private void DisplayToolDetails(Tool tool)
        {
            ToolNameTextBox.Text = tool.Name;
            ToolVersionTextBox.Text = tool.Version;

            for (int i = 0; i < ToolCategoryComboBox.Items.Count; i++)
            {
                var item = ToolCategoryComboBox.Items[i] as ComboBoxItem;
                if (item != null && item.Content.ToString() == tool.Category)
                {
                    ToolCategoryComboBox.SelectedIndex = i;
                    break;
                }
            }

            InstallCommandTextBox.Text = tool.InstallCommand;
            ValidationScriptTextBox.Text = tool.ValidationScript;
            currentTool = tool;
        }

        private void ToolsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToolsListBox.SelectedIndex >= 0)
            {
                var selectedItem = ToolsListBox.SelectedItem as ListBoxItem;
                if (selectedItem != null && selectedItem.Tag != null)
                {
                    string selectedToolName = selectedItem.Tag.ToString() ?? string.Empty;
                    Tool? selectedTool = toolManagementService.FindToolByName(selectedToolName);

                    if (selectedTool != null)
                    {
                        DisplayToolDetails(selectedTool);
                    }
                }
            }
        }

        private void AddNewTool_Click(object sender, RoutedEventArgs e)
        {
            ClearToolDetails();
            ToolCategoryComboBox.SelectedIndex = 0;
            StatusTextBlock.Text = "Adding new tool...";
        }

        private async void ScanSystem_Click(object sender, RoutedEventArgs e)
        {
            await PerformSystemScan();
        }

        private async Task PerformSystemScan()
        {
            try
            {
                UpdateStatus("Scanning system for installed tools...");
                ShowLoadingOverlay("Scanning System...");

                toolDetectionService.ScanDrives = appSettings.ScanDrives;

                var results = await toolManagementService.ScanForInstalledToolsAsync();

                int installedCount = results.Count(r => r.Value);
                UpdateStatus($"Scan complete. Found {installedCount} installed tools.");

                UpdateToolsList();

                HideLoadingOverlay();

                ShowNotification($"Scan complete! Found {installedCount} of {results.Count} tools installed.", NotificationType.Success);

                ShowScanResults(results);
            }
            catch (Exception ex)
            {
                HideLoadingOverlay();
                System.Windows.MessageBox.Show($"Error scanning system: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Scan failed.");
                ShowNotification("Scan failed. Please try again.", NotificationType.Error);
            }
        }

        /*
         CreateProgressOverlay is temp. broken 
         */

        private UIElement CreateProgressOverlay(string message)
        {
            Grid overlay = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0))
            };

            StackPanel progressPanel = new StackPanel
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            Border spinner = new Border
            {
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(20),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };

            RotateTransform rotateTransform = new RotateTransform();
            spinner.RenderTransform = rotateTransform;

            DoubleAnimation rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            };

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

            TextBlock messageText = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 16,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            progressPanel.Children.Add(spinner);
            progressPanel.Children.Add(messageText);
            overlay.Children.Add(progressPanel);

            return overlay;
        }

        private void ShowScanResults(Dictionary<Tool, bool> results)
        {
            Window resultsWindow = new Window
            {
                Title = "Scan Results",
                Width = 500,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = (SolidColorBrush)FindResource("BackgroundBrush")
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Border headerBorder = new Border
            {
                Background = (SolidColorBrush)FindResource("PrimaryBrush"),
                Padding = new Thickness(20, 15, 20, 15)
            };

            TextBlock headerText = new TextBlock
            {
                Text = "Installation Scan Results",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            };

            headerBorder.Child = headerText;
            Grid.SetRow(headerBorder, 0);
            grid.Children.Add(headerBorder);

            // Add summary
            int installedCount = results.Count(r => r.Value);
            Border summaryBorder = new Border
            {
                Padding = new Thickness(20, 15, 20, 15),
                BorderBrush = (SolidColorBrush)FindResource("BorderBrush"),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };

            TextBlock summaryText = new TextBlock
            {
                Text = $"Found {installedCount} of {results.Count} tools installed on your system.",
                Margin = new Thickness(0, 0, 0, 5)
            };

            summaryBorder.Child = summaryText;
            Grid.SetRow(summaryBorder, 1);
            grid.Children.Add(summaryBorder);

            Border listBorder = new Border
            {
                Margin = new Thickness(20, 20, 20, 20),
                Background = (SolidColorBrush)FindResource("CardBrush"),
                BorderBrush = (SolidColorBrush)FindResource("BorderBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4)
            };

            System.Windows.Controls.ListView listView = new System.Windows.Controls.ListView
            {
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent
            };

            var groupedResults = results.GroupBy(r => r.Value);

            foreach (var group in groupedResults.OrderByDescending(g => g.Key))
            {
                listView.Items.Add(new System.Windows.Controls.ListViewItem
                {
                    Content = new TextBlock
                    {
                        Text = group.Key ? "Installed Tools" : "Not Installed",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 10, 0, 5),
                        Foreground = group.Key ? Brushes.Green : Brushes.Red
                    },
                    IsEnabled = false,
                    Background = Brushes.Transparent
                });

                foreach (var result in group)
                {
                    StackPanel itemPanel = new StackPanel
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Margin = new Thickness(15, 5, 5, 5)
                    };

                    TextBlock icon = new TextBlock
                    {
                        Text = group.Key ? "✓" : "✗",
                        FontWeight = FontWeights.Bold,
                        Foreground = group.Key ? Brushes.Green : Brushes.Red,
                        Margin = new Thickness(0, 0, 10, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    TextBlock name = new TextBlock
                    {
                        Text = result.Key.Name,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    itemPanel.Children.Add(icon);
                    itemPanel.Children.Add(name);

                    listView.Items.Add(new System.Windows.Controls.ListViewItem
                    {
                        Content = itemPanel,
                        Background = Brushes.Transparent
                    });
                }
            }

            listBorder.Child = listView;
            Grid.SetRow(listBorder, 2);
            grid.Children.Add(listBorder);

            Border buttonBorder = new Border
            {
                Padding = new Thickness(20, 15, 20, 15),
                BorderBrush = (SolidColorBrush)FindResource("BorderBrush"),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Background = (SolidColorBrush)FindResource("CardBrush")
            };

            System.Windows.Controls.Button closeButton = new System.Windows.Controls.Button
            {
                Content = "Close",
                Width = 100,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Style = (Style)FindResource("PrimaryButtonStyle")
            };
            closeButton.Click += (s, e) => resultsWindow.Close();

            buttonBorder.Child = closeButton;
            Grid.SetRow(buttonBorder, 3);
            grid.Children.Add(buttonBorder);

            resultsWindow.Content = grid;
            resultsWindow.ShowDialog();
        }

        private void SaveTool_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ToolNameTextBox.Text))
            {
                System.Windows.MessageBox.Show("Tool name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedItem = ToolCategoryComboBox.SelectedItem as ComboBoxItem;
                var selectedCategory = selectedItem?.Content.ToString() ?? "Utilities";

                if (currentTool == null)
                {
                    currentTool = new Tool
                    {
                        Name = ToolNameTextBox.Text,
                        Version = ToolVersionTextBox.Text,
                        Category = selectedCategory,
                        InstallCommand = InstallCommandTextBox.Text,
                        ValidationScript = ValidationScriptTextBox.Text
                    };

                    toolManagementService.AddTool(currentTool);
                    UpdateStatus($"Added new tool: {currentTool.Name}");
                }
                else
                {
                    currentTool.Name = ToolNameTextBox.Text;
                    currentTool.Version = ToolVersionTextBox.Text;
                    currentTool.Category = selectedCategory;
                    currentTool.InstallCommand = InstallCommandTextBox.Text;
                    currentTool.ValidationScript = ValidationScriptTextBox.Text;

                    toolManagementService.UpdateTool(currentTool);
                    UpdateStatus($"Updated tool: {currentTool.Name}");
                }

                UpdateToolsList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving tool: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTool_Click(object sender, RoutedEventArgs e)
        {
            if (currentTool != null && System.Windows.MessageBox.Show($"Are you sure you want to delete {currentTool.Name}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    toolManagementService.DeleteTool(currentTool);
                    UpdateToolsList();
                    ClearToolDetails();
                    UpdateStatus("Tool deleted");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error deleting tool: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddToEnvironment_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableToolsListBox.SelectedIndex >= 0)
            {
                var selectedItem = AvailableToolsListBox.SelectedItem as ListBoxItem;
                if (selectedItem != null && selectedItem.Tag != null)
                {
                    string selectedToolName = selectedItem.Tag.ToString() ?? string.Empty;
                    Tool? selectedTool = toolManagementService.FindToolByName(selectedToolName);

                    if (selectedTool != null && !selectedTools.Contains(selectedTool))
                    {
                        selectedTools.Add(selectedTool);
                        UpdateToolsList();
                        UpdateStatus($"Added {selectedTool.Name} to environment");
                    }
                }
            }
        }

        private void RemoveFromEnvironment_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedToolsListBox.SelectedIndex >= 0 && SelectedToolsListBox.SelectedItem != null)
            {
                string selectedToolName = SelectedToolsListBox.SelectedItem.ToString() ?? string.Empty;
                Tool? selectedTool = selectedTools.Find(t => t.Name == selectedToolName);

                if (selectedTool != null)
                {
                    selectedTools.Remove(selectedTool);
                    UpdateToolsList();
                    UpdateStatus($"Removed {selectedTool.Name} from environment");
                }
            }
        }

        private void GenerateScript_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTools.Count == 0)
            {
                System.Windows.MessageBox.Show("Please select at least one tool for your environment.",
                    "No Tools Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string environmentName = EnvironmentNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(environmentName))
            {
                environmentName = "DevEnvironment";
            }

            var selectedItem = TargetOSComboBox.SelectedItem as ComboBoxItem;
            string targetOS = selectedItem?.Content.ToString() ?? "Windows";

            string script;
            var scriptTypeItem = ScriptTypeComboBox.SelectedItem as ComboBoxItem;
            string scriptType = scriptTypeItem?.Content.ToString() ?? "PowerShell";

            if (scriptType == "PowerShell")
            {
                script = scriptGeneratorService.GeneratePowerShellScript(environmentName, selectedTools);
            }
            else
            {
                script = scriptGeneratorService.GenerateBashScript(environmentName, selectedTools);
            }

            ScriptPreviewEditor.Text = script;
            UpdateStatus("Script generated");

            ((System.Windows.Controls.TabControl)((Grid)this.Content).Children[1]).SelectedIndex = 2;
        }

        private void CopyScript_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ScriptPreviewEditor.Text))
            {
                System.Windows.Clipboard.SetText(ScriptPreviewEditor.Text);
                UpdateStatus("Script copied to clipboard");
            }
        }

        private void SaveScript_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ScriptPreviewEditor.Text))
            {
                return;
            }

            var scriptTypeItem = ScriptTypeComboBox.SelectedItem as ComboBoxItem;
            string scriptType = scriptTypeItem?.Content.ToString() ?? "PowerShell";
            string extension = scriptType == "PowerShell" ? ".ps1" : ".sh";

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = scriptType == "PowerShell"
                    ? "PowerShell Script|*.ps1|All Files|*.*"
                    : "Bash Script|*.sh|All Files|*.*",
                DefaultExt = extension,
                FileName = $"{EnvironmentNameTextBox.Text.Replace(" ", "")}_Setup"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, ScriptPreviewEditor.Text);
                    UpdateStatus($"Script saved to {saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error saving script: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RunScript_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("This feature is not implemented yet. For security reasons, please save the script and run it manually.",
                "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                appSettings.AutoScanOnStartup = AutoScanCheckBox.IsChecked ?? false;

                var selectedLanguageItem = DefaultScriptLanguageComboBox.SelectedItem as ComboBoxItem;
                if (selectedLanguageItem != null)
                {
                    appSettings.DefaultScriptLanguage = selectedLanguageItem.Content.ToString();
                }

                appSettings.ScanDrives.Clear();
                foreach (var child in DrivesCheckBoxPanel.Children)
                {
                    if (child is System.Windows.Controls.CheckBox checkBox && checkBox.IsChecked == true && checkBox.Tag != null)
                    {
                        string driveTag = checkBox.Tag.ToString() ?? "C:";
                        appSettings.ScanDrives.Add(driveTag);
                    }
                }

                if (appSettings.ScanDrives.Count == 0)
                {
                    appSettings.ScanDrives.Add("C:");
                    System.Windows.MessageBox.Show("At least one drive must be selected for scanning. Defaulting to C: drive.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                appSettings.Save();

                toolDetectionService.ScanDrives = appSettings.ScanDrives;

                UpdateStatus("Settings saved successfully");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Are you sure you want to reset all settings to default values?",
                "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                appSettings = new Settings();

                AutoScanCheckBox.IsChecked = appSettings.AutoScanOnStartup;

                foreach (ComboBoxItem item in DefaultScriptLanguageComboBox.Items)
                {
                    if (item.Content.ToString() == appSettings.DefaultScriptLanguage)
                    {
                        DefaultScriptLanguageComboBox.SelectedItem = item;
                        break;
                    }
                }

                PopulateDriveCheckboxes();

                appSettings.Save();

                toolDetectionService.ScanDrives = appSettings.ScanDrives;

                UpdateStatus("Settings reset to defaults");
            }
        }

        private void BrowseProjectDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Project Directory",
                ShowNewFolderButton = false
            };

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                selectedProjectDirectory = dialog.SelectedPath;
                ProjectDirectoryTextBox.Text = selectedProjectDirectory;

                DetectedTechnologiesPanel.Children.Clear();
                DetectedTechnologiesPanel.Children.Add(new TextBlock
                {
                    Text = "Click 'Scan Project' to analyze this directory.",
                    Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                    TextWrapping = TextWrapping.Wrap
                });

                ApplyRecommendationsButton.IsEnabled = false;

                UpdateStatus($"Selected project directory: {selectedProjectDirectory}");
            }
        }

        private async void ScanProject_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedProjectDirectory))
            {
                System.Windows.MessageBox.Show("Please select a project directory first.", "No Directory Selected",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                UpdateStatus("Scanning project...");
                ShowLoadingOverlay("Analyzing Project...");
                
                // disbale ui during scan
                IsEnabled = false;

                DetectedTechnologiesPanel.Children.Clear();

                await projectScannerService.ScanProjectAsync(selectedProjectDirectory);

                DisplayScanResults();

                ApplyRecommendationsButton.IsEnabled = projectScannerService.RecommendedTools.Count > 0;

                HideLoadingOverlay();

                UpdateStatus("Project scan complete.");
            }
            catch (Exception ex)
            {
                HideLoadingOverlay();
                System.Windows.MessageBox.Show($"Error scanning project: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Project scan failed.");
            }
            finally
            {
                // enable ui after scan
                IsEnabled = true;
            }
        }

        private void DisplayScanResults()
        {
            Dispatcher.Invoke(() =>
            {
                var recommendedTools = projectScannerService.RecommendedTools;
                var detectedTechnologies = projectScannerService.DetectedTechnologies;
                var detectedVersions = projectScannerService.DetectedVersions;

                if (recommendedTools.Count == 0)
                {
                    DetectedTechnologiesPanel.Children.Add(new TextBlock
                    {
                        Text = "No recognizable project technologies were detected. Try selecting a different directory.",
                        Foreground = (SolidColorBrush)FindResource("TextSecondaryBrush"),
                        TextWrapping = TextWrapping.Wrap
                    });
                    return;
                }

                DetectedTechnologiesPanel.Children.Add(new TextBlock
                {
                    Text = "The following technologies were detected in your project:",
                    Margin = new Thickness(0, 0, 0, 15),
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16
                });

                Border detectedTechBorder = new Border
                {
                    BorderBrush = (SolidColorBrush)FindResource("BorderBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 0, 0, 20),
                    Background = (SolidColorBrush)FindResource("CardBrush")
                };

                StackPanel detectedTechPanel = new StackPanel();

                var techCategories = new Dictionary<string, List<string>>
                {
                    { "Languages", new List<string>() },
                    { "Frameworks", new List<string>() },
                    { "Databases", new List<string>() },
                    { "Build Tools", new List<string>() },
                    { "Cloud Services", new List<string>() },
                    { "Other", new List<string>() }
                };

                foreach (var tech in detectedTechnologies.Keys)
                {
                    string category = "Other";

                    if (new[] { "C#", "JavaScript", "TypeScript", "Python", "Java", "Kotlin", "Ruby", "PHP", "Go", "Rust", "Swift", "C++", "C", "F#", "Visual Basic", "Dart", "HTML", "CSS", "SCSS", "Sass", "Less", "R" }.Contains(tech))
                    {
                        category = "Languages";
                    }
                    else if (new[] { "React", "Angular", "Vue.js", "Next.js", "Nuxt.js", "Svelte", "ASP.NET", "Django", "Flask", "FastAPI", "Spring Boot", "Laravel", "Ruby on Rails", "Express.js", "Blazor", "Flutter", "WPF", "Windows Forms", "Xamarin/MAUI" }.Contains(tech))
                    {
                        category = "Frameworks";
                    }
                    else if (new[] { "PostgreSQL", "MySQL", "MongoDB", "SQL Server", "SQLite", "Redis", "Firebase", "DynamoDB", "CosmosDB", "Entity Framework", "Entity Framework Core", "NHibernate", "Dapper", "Sequelize", "Mongoose", "TypeORM", "Prisma", "SQLAlchemy", "Django ORM", "ActiveRecord", "JPA", "Hibernate" }.Contains(tech))
                    {
                        category = "Databases";
                    }
                    else if (new[] { "Webpack", "Vite", "Rollup", "Gulp", "Grunt", "Babel", "Jest", "Cypress", "ESLint", "Prettier", "npm", "Yarn", "pnpm", "Maven", "Gradle", "Make", "CMake", "Pipenv", "Poetry", "Bundler", "Composer" }.Contains(tech))
                    {
                        category = "Build Tools";
                    }
                    else if (new[] { "AWS", "Azure", "Google Cloud", "Vercel", "Netlify", "Heroku", "Digital Ocean", "Docker", "Docker Compose", "Kubernetes", "Terraform", "GitHub Actions", "Azure DevOps", "GitLab CI", "Serverless Framework" }.Contains(tech))
                    {
                        category = "Cloud Services";
                    }

                    techCategories[category].Add(tech);
                }

                foreach (var category in techCategories.Keys)
                {
                    if (techCategories[category].Count > 0)
                    {
                        detectedTechPanel.Children.Add(new TextBlock
                        {
                            Text = category,
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 10, 0, 5)
                        });

                        WrapPanel techWrapPanel = new WrapPanel
                        {
                            Margin = new Thickness(10, 0, 0, 10)
                        };

                        foreach (var tech in techCategories[category])
                        {
                            Border techChip = new Border
                            {
                                Background = (SolidColorBrush)FindResource("PrimaryBrush"),
                                CornerRadius = new CornerRadius(15),
                                Padding = new Thickness(10, 5, 10, 5),
                                Margin = new Thickness(0, 5, 10, 5)
                            };

                            StackPanel chipContent = new StackPanel
                            {
                                Orientation = Orientation.Horizontal
                            };

                            TextBlock techText = new TextBlock
                            {
                                Text = tech,
                                Foreground = Brushes.White,
                                VerticalAlignment = VerticalAlignment.Center
                            };

                            chipContent.Children.Add(techText);

                            if (detectedVersions.ContainsKey(tech))
                            {
                                chipContent.Children.Add(new TextBlock
                                {
                                    Text = $" v{detectedVersions[tech]}",
                                    Foreground = Brushes.White,
                                    Opacity = 0.8,
                                    VerticalAlignment = VerticalAlignment.Center
                                });
                            }

                            techChip.Child = chipContent;
                            techWrapPanel.Children.Add(techChip);
                        }

                        detectedTechPanel.Children.Add(techWrapPanel);
                    }
                }

                detectedTechBorder.Child = detectedTechPanel;
                DetectedTechnologiesPanel.Children.Add(detectedTechBorder);

                DetectedTechnologiesPanel.Children.Add(new TextBlock
                {
                    Text = "Recommended Tools:",
                    Margin = new Thickness(0, 15, 0, 15),
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16
                });

                foreach (var tech in recommendedTools)
                {
                    Border techBorder = new Border
                    {
                        BorderBrush = (SolidColorBrush)FindResource("BorderBrush"),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 0, 0, 15),
                        Background = (SolidColorBrush)FindResource("CardBrush")
                    };

                    StackPanel techPanel = new StackPanel();

                    techPanel.Children.Add(new TextBlock
                    {
                        Text = tech.Key,
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(0, 0, 0, 10)
                    });

                    foreach (var tool in tech.Value)
                    {
                        StackPanel toolPanel = new StackPanel
                        {
                            Orientation = System.Windows.Controls.Orientation.Horizontal,
                            Margin = new Thickness(10, 5, 0, 5)
                        };

                        bool isInstalled = toolDetectionService.IsToolInstalled(tool);

                        if (isInstalled)
                        {
                            toolPanel.Children.Add(new TextBlock
                            {
                                Text = "✓ ",
                                Foreground = Brushes.Green,
                                FontWeight = FontWeights.Bold,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(0, 0, 5, 0)
                            });

                            toolPanel.Children.Add(new TextBlock
                            {
                                Text = $"{tool.Name} (Already Installed)",
                                VerticalAlignment = VerticalAlignment.Center
                            });
                        }
                        else
                        {
                            toolPanel.Children.Add(new TextBlock
                            {
                                Text = "→ ",
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(0, 0, 5, 0)
                            });

                            toolPanel.Children.Add(new TextBlock
                            {
                                Text = tool.Name,
                                VerticalAlignment = VerticalAlignment.Center
                            });
                        }

                        techPanel.Children.Add(toolPanel);
                    }

                    techBorder.Child = techPanel;
                    DetectedTechnologiesPanel.Children.Add(techBorder);
                }
            });
        }

        private void ApplyRecommendationsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tech in projectScannerService.RecommendedTools)
            {
                foreach (var tool in tech.Value)
                {
                    if (!selectedTools.Contains(tool))
                    {
                        selectedTools.Add(tool);
                    }
                }
            }

            UpdateToolsList();

            ((System.Windows.Controls.TabControl)((Grid)this.Content).Children[1]).SelectedIndex = 1;

            ShowNotification("Recommended tools added to environment setup.", NotificationType.Success);
        }
    }
}