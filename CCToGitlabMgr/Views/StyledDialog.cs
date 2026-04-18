using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CCToGitlabMgr.Views
{
    /// <summary>
    /// Dark-themed dialog windows matching the app style.
    /// </summary>
    public static class StyledDialog
    {
        // Theme colors
        private static readonly Color BgColor = Color.FromRgb(0x0F, 0x14, 0x20);
        private static readonly Color SurfaceColor = Color.FromRgb(0x1A, 0x20, 0x35);
        private static readonly Color BorderColor = Color.FromRgb(0x2A, 0x34, 0x54);
        private static readonly Color TextColor = Color.FromRgb(0xF0, 0xF3, 0xFF);
        private static readonly Color Text2Color = Color.FromRgb(0x8B, 0x95, 0xB0);
        private static readonly Color AccentColor = Color.FromRgb(0xFF, 0x6B, 0x35);
        private static readonly Color OkColor = Color.FromRgb(0x3E, 0xCF, 0x8E);
        private static readonly Color WarnColor = Color.FromRgb(0xFB, 0xBF, 0x24);
        private static readonly Color DangerColor = Color.FromRgb(0xEF, 0x44, 0x44);

        private static readonly Brush BgBrush = new SolidColorBrush(BgColor);
        private static readonly Brush SurfaceBrush = new SolidColorBrush(SurfaceColor);
        private static readonly Brush BorderBrush = new SolidColorBrush(BorderColor);
        private static readonly Brush TextBrush = new SolidColorBrush(TextColor);
        private static readonly Brush Text2Brush = new SolidColorBrush(Text2Color);
        private static readonly Brush AccentBrush = new SolidColorBrush(AccentColor);
        private static readonly Brush OkBrush = new SolidColorBrush(OkColor);
        private static readonly Brush WarnBrush = new SolidColorBrush(WarnColor);
        private static readonly Brush DangerBrush = new SolidColorBrush(DangerColor);

        public enum DialogIcon { Warning, Info, Danger, Question }

        /// <summary>
        /// Show a styled Yes/No/Cancel confirmation dialog.
        /// </summary>
        public static MessageBoxResult ShowConfirm(
            string title,
            string heading,
            string message,
            string[] options,
            DialogIcon icon = DialogIcon.Warning,
            MessageBoxButton buttons = MessageBoxButton.YesNoCancel)
        {
            MessageBoxResult dialogResult = MessageBoxResult.Cancel;

            var win = new Window
            {
                Title = title,
                Width = 520, Height = 320,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                ResizeMode = ResizeMode.NoResize,
                Background = BgBrush,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                BorderThickness = new Thickness(0)
            };

            // Pick border color by icon type
            Brush dialogBorderBrush;
            switch (icon)
            {
                case DialogIcon.Warning: dialogBorderBrush = WarnBrush; break;
                case DialogIcon.Danger: dialogBorderBrush = DangerBrush; break;
                case DialogIcon.Info: dialogBorderBrush = AccentBrush; break;
                default: dialogBorderBrush = AccentBrush; break;
            }

            // Main container with colored border
            var outerBorder = new Border
            {
                Background = BgBrush,
                BorderBrush = dialogBorderBrush,
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(12),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 30,
                    Opacity = 0.6,
                    ShadowDepth = 0
                }
            };

            var mainStack = new StackPanel();

            // Title bar with accent top line
            var titleBar = new Border
            {
                Background = SurfaceBrush,
                Padding = new Thickness(20, 14, 20, 14),
                CornerRadius = new CornerRadius(11, 11, 0, 0),
                BorderBrush = dialogBorderBrush,
                BorderThickness = new Thickness(0, 2, 0, 0)
            };
            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = TextBrush
            };
            titleBar.Child = titleText;
            titleBar.MouseLeftButtonDown += (s, e) => win.DragMove();
            mainStack.Children.Add(titleBar);

            // Content area
            var content = new StackPanel { Margin = new Thickness(24, 20, 24, 20) };

            // Icon + Heading row
            var headingPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 16) };

            var iconText = new TextBlock
            {
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            switch (icon)
            {
                case DialogIcon.Warning:
                    iconText.Text = "\u26A0";
                    iconText.Foreground = WarnBrush;
                    break;
                case DialogIcon.Danger:
                    iconText.Text = "\u2716";
                    iconText.Foreground = DangerBrush;
                    break;
                case DialogIcon.Info:
                    iconText.Text = "i";
                    iconText.Foreground = AccentBrush;
                    break;
                case DialogIcon.Question:
                    iconText.Text = "?";
                    iconText.Foreground = AccentBrush;
                    break;
            }

            headingPanel.Children.Add(iconText);
            var headingText = new TextBlock
            {
                Text = heading,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = TextBrush,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            headingPanel.Children.Add(headingText);
            content.Children.Add(headingPanel);

            // Message
            var msgBlock = new TextBlock
            {
                Text = message,
                FontSize = 13,
                Foreground = Text2Brush,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16)
            };
            content.Children.Add(msgBlock);

            // Options
            if (options != null)
            {
                var optionsPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
                var optionBrushes = new Brush[] { OkBrush, WarnBrush, Text2Brush };
                for (int i = 0; i < options.Length; i++)
                {
                    var optBlock = new TextBlock
                    {
                        FontSize = 13,
                        Foreground = TextBrush,
                        Margin = new Thickness(0, 0, 0, 6),
                        TextWrapping = TextWrapping.Wrap
                    };
                    var bullet = new System.Windows.Documents.Run
                    {
                        Text = (i < 3 ? new[] { "\u25B6 ", "\u25B6 ", "\u25B6 " }[i] : "\u25B6 "),
                        Foreground = i < optionBrushes.Length ? optionBrushes[i] : Text2Brush,
                        FontWeight = FontWeights.Bold
                    };
                    optBlock.Inlines.Add(bullet);
                    optBlock.Inlines.Add(new System.Windows.Documents.Run { Text = options[i] });
                    optionsPanel.Children.Add(optBlock);
                }
                content.Children.Add(optionsPanel);
            }

            mainStack.Children.Add(content);

            // Button bar
            var buttonBar = new Border
            {
                Background = SurfaceBrush,
                Padding = new Thickness(24, 12, 24, 16),
                CornerRadius = new CornerRadius(0, 0, 12, 12)
            };
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            if (buttons == MessageBoxButton.YesNoCancel || buttons == MessageBoxButton.YesNo)
            {
                var btnYes = MakeButton("Yes", OkBrush, true);
                btnYes.Click += (s, e) => { dialogResult = MessageBoxResult.Yes; win.Close(); };
                buttonPanel.Children.Add(btnYes);

                var btnNo = MakeButton("No", WarnBrush, false);
                btnNo.Click += (s, e) => { dialogResult = MessageBoxResult.No; win.Close(); };
                buttonPanel.Children.Add(btnNo);

                if (buttons == MessageBoxButton.YesNoCancel)
                {
                    var btnCancel = MakeButton("Cancel", null, false);
                    btnCancel.Click += (s, e) => { dialogResult = MessageBoxResult.Cancel; win.Close(); };
                    buttonPanel.Children.Add(btnCancel);
                }
            }
            else if (buttons == MessageBoxButton.OKCancel)
            {
                var btnOk = MakeButton("OK", AccentBrush, true);
                btnOk.Click += (s, e) => { dialogResult = MessageBoxResult.OK; win.Close(); };
                buttonPanel.Children.Add(btnOk);

                var btnCancel = MakeButton("Cancel", null, false);
                btnCancel.Click += (s, e) => { dialogResult = MessageBoxResult.Cancel; win.Close(); };
                buttonPanel.Children.Add(btnCancel);
            }
            else
            {
                var btnOk = MakeButton("OK", AccentBrush, true);
                btnOk.Click += (s, e) => { dialogResult = MessageBoxResult.OK; win.Close(); };
                buttonPanel.Children.Add(btnOk);
            }

            buttonBar.Child = buttonPanel;
            mainStack.Children.Add(buttonBar);

            outerBorder.Child = mainStack;
            win.Content = outerBorder;
            win.ShowDialog();

            return dialogResult;
        }

        /// <summary>
        /// Show a diff/output viewer window with syntax coloring.
        /// </summary>
        public static void ShowDiffViewer(string title, string diffOutput)
        {
            var win = new Window
            {
                Title = title,
                Width = 900, Height = 650,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                Background = BgBrush,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                BorderThickness = new Thickness(0)
            };

            var outerBorder = new Border
            {
                Background = BgBrush,
                BorderBrush = AccentBrush,
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(12),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 30,
                    Opacity = 0.6,
                    ShadowDepth = 0
                }
            };

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Title bar with accent top line
            var titleBar = new Border
            {
                Background = SurfaceBrush,
                Padding = new Thickness(20, 12, 20, 12),
                CornerRadius = new CornerRadius(11, 11, 0, 0),
                BorderBrush = AccentBrush,
                BorderThickness = new Thickness(0, 2, 0, 0)
            };
            var titlePanel = new DockPanel();
            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = TextBrush,
                VerticalAlignment = VerticalAlignment.Center
            };
            var closeBtn = new Button
            {
                Content = "\u2715",
                FontSize = 14,
                Foreground = Text2Brush,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            closeBtn.Click += (s, e) => win.Close();
            DockPanel.SetDock(closeBtn, Dock.Right);
            titlePanel.Children.Add(closeBtn);
            titlePanel.Children.Add(titleText);
            titleBar.Child = titlePanel;
            titleBar.MouseLeftButtonDown += (s, e) => win.DragMove();
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            // Diff content with syntax coloring
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(0)
            };

            var diffPanel = new StackPanel { Margin = new Thickness(0) };
            var lines = diffOutput.Split(new[] { '\n' }, StringSplitOptions.None);

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                var lineBlock = new TextBlock
                {
                    Text = line,
                    FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
                    FontSize = 12,
                    Padding = new Thickness(20, 2, 20, 2),
                    TextWrapping = TextWrapping.NoWrap
                };

                if (line.StartsWith("+++") || line.StartsWith("---"))
                {
                    lineBlock.Foreground = TextBrush;
                    lineBlock.FontWeight = FontWeights.Bold;
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x20, 0x3B, 0x82, 0xF6));
                }
                else if (line.StartsWith("+"))
                {
                    lineBlock.Foreground = new SolidColorBrush(Color.FromRgb(0x6E, 0xE7, 0xB7));
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x18, 0x3E, 0xCF, 0x8E));
                }
                else if (line.StartsWith("-"))
                {
                    lineBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xFC, 0xA5, 0xA5));
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x18, 0xEF, 0x44, 0x44));
                }
                else if (line.StartsWith("@@"))
                {
                    lineBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xA7, 0x8B, 0xFA));
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x10, 0xA7, 0x8B, 0xFA));
                    lineBlock.FontWeight = FontWeights.SemiBold;
                }
                else if (line.StartsWith("diff "))
                {
                    lineBlock.Foreground = AccentBrush;
                    lineBlock.FontWeight = FontWeights.Bold;
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x14, 0xFF, 0x6B, 0x35));
                    lineBlock.Padding = new Thickness(20, 8, 20, 8);
                    lineBlock.Margin = new Thickness(0, 4, 0, 0);
                }
                else
                {
                    lineBlock.Foreground = Text2Brush;
                }

                diffPanel.Children.Add(lineBlock);
            }

            scrollViewer.Content = diffPanel;
            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);

            // Bottom stats bar
            int added = 0, removed = 0, files = 0;
            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                if (line.StartsWith("+") && !line.StartsWith("+++")) added++;
                else if (line.StartsWith("-") && !line.StartsWith("---")) removed++;
                else if (line.StartsWith("diff ")) files++;
            }

            var statsBar = new Border
            {
                Background = SurfaceBrush,
                Padding = new Thickness(20, 10, 20, 14),
                CornerRadius = new CornerRadius(0, 0, 12, 12)
            };
            var statsPanel = new DockPanel();

            var statsText = new TextBlock
            {
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            statsText.Inlines.Add(new System.Windows.Documents.Run
            {
                Text = $"{files} file{(files != 1 ? "s" : "")} changed   ",
                Foreground = TextBrush,
                FontWeight = FontWeights.SemiBold
            });
            statsText.Inlines.Add(new System.Windows.Documents.Run
            {
                Text = $"+{added} ",
                Foreground = OkBrush,
                FontWeight = FontWeights.Bold
            });
            statsText.Inlines.Add(new System.Windows.Documents.Run
            {
                Text = $"-{removed}",
                Foreground = DangerBrush,
                FontWeight = FontWeights.Bold
            });

            var closeBtn2 = MakeButton("Close", null, false);
            closeBtn2.Click += (s, e) => win.Close();
            DockPanel.SetDock(closeBtn2, Dock.Right);
            statsPanel.Children.Add(closeBtn2);
            statsPanel.Children.Add(statsText);

            statsBar.Child = statsPanel;
            Grid.SetRow(statsBar, 2);
            mainGrid.Children.Add(statsBar);

            outerBorder.Child = mainGrid;
            win.Content = outerBorder;
            win.ShowDialog();
        }

        /// <summary>
        /// Show a styled git status viewer.
        /// </summary>
        public static void ShowStatusViewer(string statusOutput, string branch)
        {
            var win = CreateViewerWindow("Git Status", 700, 500, AccentBrush);

            var outerBorder = CreateViewerBorder(AccentBrush);
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Title bar
            var titleBar = CreateTitleBar("Git Status", AccentBrush, win);
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            // Content
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var contentPanel = new StackPanel { Margin = new Thickness(0) };

            // Branch header
            if (!string.IsNullOrWhiteSpace(branch))
            {
                var branchBar = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(0x18, 0x3B, 0x82, 0xF6)),
                    Padding = new Thickness(20, 10, 20, 10)
                };
                var branchText = new TextBlock { FontSize = 13 };
                branchText.Inlines.Add(new System.Windows.Documents.Run
                    { Text = "On branch ", Foreground = Text2Brush });
                branchText.Inlines.Add(new System.Windows.Documents.Run
                    { Text = branch, Foreground = OkBrush, FontWeight = FontWeights.Bold,
                      FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New") });
                branchBar.Child = branchText;
                contentPanel.Children.Add(branchBar);
            }

            var lines = statusOutput.Split(new[] { '\n' }, StringSplitOptions.None);
            int modified = 0, added = 0, deleted = 0, untracked = 0;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                if (string.IsNullOrWhiteSpace(line)) continue;

                var lineBlock = new TextBlock
                {
                    FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
                    FontSize = 12,
                    Padding = new Thickness(20, 3, 20, 3),
                    TextWrapping = TextWrapping.NoWrap
                };

                // Parse porcelain-style or regular status
                if (line.Length >= 2 && line[1] == 'M' || (line.Length >= 2 && line[0] == 'M'))
                {
                    lineBlock.Text = "\u270E  " + line.Substring(Math.Min(3, line.Length));
                    lineBlock.Foreground = WarnBrush;
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x10, 0xFB, 0xBF, 0x24));
                    modified++;
                }
                else if (line.StartsWith("A ") || line.StartsWith("?"))
                {
                    lineBlock.Text = "\u2795  " + line.Substring(Math.Min(3, line.Length));
                    lineBlock.Foreground = OkBrush;
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x10, 0x3E, 0xCF, 0x8E));
                    if (line.StartsWith("?")) untracked++; else added++;
                }
                else if (line.StartsWith("D ") || (line.Length >= 2 && line[1] == 'D'))
                {
                    lineBlock.Text = "\u2796  " + line.Substring(Math.Min(3, line.Length));
                    lineBlock.Foreground = DangerBrush;
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x10, 0xEF, 0x44, 0x44));
                    deleted++;
                }
                else if (line.Contains("modified:"))
                {
                    var file = line.Trim();
                    lineBlock.Text = "\u270E  " + file;
                    lineBlock.Foreground = WarnBrush;
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x10, 0xFB, 0xBF, 0x24));
                    modified++;
                }
                else if (line.Contains("new file:"))
                {
                    var file = line.Trim();
                    lineBlock.Text = "\u2795  " + file;
                    lineBlock.Foreground = OkBrush;
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x10, 0x3E, 0xCF, 0x8E));
                    added++;
                }
                else if (line.Contains("deleted:"))
                {
                    var file = line.Trim();
                    lineBlock.Text = "\u2796  " + file;
                    lineBlock.Foreground = DangerBrush;
                    lineBlock.Background = new SolidColorBrush(Color.FromArgb(0x10, 0xEF, 0x44, 0x44));
                    deleted++;
                }
                else if (line.Contains("Untracked files") || line.Contains("Changes not staged") ||
                         line.Contains("Changes to be committed") || line.Contains("On branch") ||
                         line.Contains("nothing to commit"))
                {
                    // Section headers
                    lineBlock.Text = line.Trim();
                    lineBlock.Foreground = TextBrush;
                    lineBlock.FontWeight = FontWeights.SemiBold;
                    lineBlock.Padding = new Thickness(20, 10, 20, 4);
                    lineBlock.FontFamily = SystemFonts.MessageFontFamily;
                    lineBlock.FontSize = 13;
                }
                else if (line.Trim().Length > 0 && !line.Trim().StartsWith("(use"))
                {
                    // Untracked file lines (just indented filenames)
                    var trimmed = line.Trim();
                    if (!trimmed.StartsWith("#") && !trimmed.StartsWith("("))
                    {
                        lineBlock.Text = "\u2022  " + trimmed;
                        lineBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xA7, 0x8B, 0xFA));
                        untracked++;
                    }
                    else
                    {
                        lineBlock.Text = "   " + line.Trim();
                        lineBlock.Foreground = Text2Brush;
                        lineBlock.FontSize = 11;
                    }
                }
                else
                {
                    lineBlock.Text = "   " + line.Trim();
                    lineBlock.Foreground = Text2Brush;
                    lineBlock.FontSize = 11;
                }

                contentPanel.Children.Add(lineBlock);
            }

            scrollViewer.Content = contentPanel;
            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);

            // Stats bar
            var statsBar = new Border
            {
                Background = SurfaceBrush,
                Padding = new Thickness(20, 10, 20, 14),
                CornerRadius = new CornerRadius(0, 0, 12, 12)
            };
            var statsPanel = new DockPanel();
            var statsText = new TextBlock { FontSize = 12, VerticalAlignment = VerticalAlignment.Center };

            if (modified + added + deleted + untracked == 0)
            {
                statsText.Inlines.Add(new System.Windows.Documents.Run
                    { Text = "\u2713 Working tree clean", Foreground = OkBrush, FontWeight = FontWeights.Bold });
            }
            else
            {
                if (modified > 0)
                    statsText.Inlines.Add(new System.Windows.Documents.Run
                        { Text = $"\u270E {modified} modified   ", Foreground = WarnBrush, FontWeight = FontWeights.SemiBold });
                if (added > 0)
                    statsText.Inlines.Add(new System.Windows.Documents.Run
                        { Text = $"\u2795 {added} added   ", Foreground = OkBrush, FontWeight = FontWeights.SemiBold });
                if (deleted > 0)
                    statsText.Inlines.Add(new System.Windows.Documents.Run
                        { Text = $"\u2796 {deleted} deleted   ", Foreground = DangerBrush, FontWeight = FontWeights.SemiBold });
                if (untracked > 0)
                    statsText.Inlines.Add(new System.Windows.Documents.Run
                        { Text = $"\u2022 {untracked} untracked", Foreground = new SolidColorBrush(Color.FromRgb(0xA7, 0x8B, 0xFA)), FontWeight = FontWeights.SemiBold });
            }

            var closeBtn = MakeButton("Close", null, false);
            closeBtn.Click += (s, e) => win.Close();
            DockPanel.SetDock(closeBtn, Dock.Right);
            statsPanel.Children.Add(closeBtn);
            statsPanel.Children.Add(statsText);
            statsBar.Child = statsPanel;
            Grid.SetRow(statsBar, 2);
            mainGrid.Children.Add(statsBar);

            outerBorder.Child = mainGrid;
            win.Content = outerBorder;
            win.ShowDialog();
        }

        /// <summary>
        /// Show a styled tags list viewer.
        /// </summary>
        public static void ShowTagsViewer(string tagsOutput)
        {
            var win = CreateViewerWindow("Git Tags", 650, 450, new SolidColorBrush(Color.FromRgb(0xA7, 0x8B, 0xFA)));
            var purpleBrush = new SolidColorBrush(Color.FromRgb(0xA7, 0x8B, 0xFA));

            var outerBorder = CreateViewerBorder(purpleBrush);
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Title bar
            var titleBar = CreateTitleBar("Version Tags", purpleBrush, win);
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            // Content
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var contentPanel = new StackPanel { Margin = new Thickness(0) };
            var lines = tagsOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int tagCount = 0;

            foreach (var rawLine in lines)
            {
                var trimmed = rawLine.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // Parse: "tagname    message text"
                var spaceIdx = trimmed.IndexOf(' ');
                string tagName, message;
                if (spaceIdx > 0)
                {
                    tagName = trimmed.Substring(0, spaceIdx);
                    message = trimmed.Substring(spaceIdx).Trim();
                }
                else
                {
                    tagName = trimmed;
                    message = "";
                }

                tagCount++;
                var isEven = tagCount % 2 == 0;

                var rowBorder = new Border
                {
                    Background = isEven ? new SolidColorBrush(Color.FromArgb(0x08, 0xA7, 0x8B, 0xFA)) : Brushes.Transparent,
                    Padding = new Thickness(20, 10, 20, 10)
                };

                var rowPanel = new DockPanel();

                // Tag icon + name
                var tagBlock = new TextBlock
                {
                    FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
                    FontSize = 13,
                    VerticalAlignment = VerticalAlignment.Center,
                    MinWidth = 160
                };
                tagBlock.Inlines.Add(new System.Windows.Documents.Run
                    { Text = "\uD83C\uDFF7 ", FontSize = 14 }); // tag emoji
                tagBlock.Inlines.Add(new System.Windows.Documents.Run
                    { Text = tagName, Foreground = purpleBrush, FontWeight = FontWeights.Bold });

                DockPanel.SetDock(tagBlock, Dock.Left);
                rowPanel.Children.Add(tagBlock);

                // Message
                var msgBlock = new TextBlock
                {
                    Text = message,
                    FontSize = 12,
                    Foreground = Text2Brush,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(16, 0, 0, 0)
                };
                rowPanel.Children.Add(msgBlock);

                rowBorder.Child = rowPanel;
                contentPanel.Children.Add(rowBorder);
            }

            if (tagCount == 0)
            {
                var emptyBlock = new TextBlock
                {
                    Text = "No tags found.\nCreate your first tag to mark a version.",
                    Foreground = Text2Brush,
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 40)
                };
                contentPanel.Children.Add(emptyBlock);
            }

            scrollViewer.Content = contentPanel;
            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);

            // Bottom bar
            var bottomBar = new Border
            {
                Background = SurfaceBrush,
                Padding = new Thickness(20, 10, 20, 14),
                CornerRadius = new CornerRadius(0, 0, 12, 12)
            };
            var bottomPanel = new DockPanel();
            var countText = new TextBlock
            {
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };
            countText.Inlines.Add(new System.Windows.Documents.Run
                { Text = $"{tagCount} tag{(tagCount != 1 ? "s" : "")}", Foreground = TextBrush, FontWeight = FontWeights.SemiBold });

            var closeBtn = MakeButton("Close", null, false);
            closeBtn.Click += (s, e) => win.Close();
            DockPanel.SetDock(closeBtn, Dock.Right);
            bottomPanel.Children.Add(closeBtn);
            bottomPanel.Children.Add(countText);
            bottomBar.Child = bottomPanel;
            Grid.SetRow(bottomBar, 2);
            mainGrid.Children.Add(bottomBar);

            outerBorder.Child = mainGrid;
            win.Content = outerBorder;
            win.ShowDialog();
        }

        /// <summary>
        /// Show a styled branches viewer with current branch highlighted.
        /// </summary>
        public static void ShowBranchesViewer(string branchOutput)
        {
            var tealBrush = new SolidColorBrush(Color.FromRgb(0x2D, 0xD4, 0xBF));
            var win = CreateViewerWindow("Git Branches", 700, 500, tealBrush);
            var outerBorder = CreateViewerBorder(tealBrush);

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // title
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // current branch header
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // list
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // stats

            // Title bar
            var titleBar = CreateTitleBar("Git Branches", tealBrush, win);
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            // Parse branches
            var lines = (branchOutput ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string currentBranch = null;
            var localBranches = new List<string>();
            var remoteBranches = new List<string>();

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                bool isCurrent = line.StartsWith("*");
                var name = line.TrimStart('*').Trim();

                // Skip HEAD pointers
                if (name.Contains("->")) continue;

                if (name.StartsWith("remotes/") || name.StartsWith("origin/"))
                {
                    remoteBranches.Add(name);
                }
                else
                {
                    localBranches.Add(name);
                    if (isCurrent) currentBranch = name;
                }
            }

            // Current branch header
            var curHeader = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x18, 0x2D, 0xD4, 0xBF)),
                Padding = new Thickness(20, 12, 20, 12),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x1E, 0x27, 0x3F)),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            var curPanel = new StackPanel { Orientation = Orientation.Horizontal };
            curPanel.Children.Add(new TextBlock
            {
                Text = "\u2726",
                FontSize = 16,
                Foreground = tealBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            });
            curPanel.Children.Add(new TextBlock
            {
                Text = "Current: ",
                FontSize = 13,
                Foreground = Text2Brush,
                VerticalAlignment = VerticalAlignment.Center
            });
            curPanel.Children.Add(new TextBlock
            {
                Text = currentBranch ?? "(unknown)",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = tealBrush,
                FontFamily = new FontFamily("Consolas"),
                VerticalAlignment = VerticalAlignment.Center
            });
            curHeader.Child = curPanel;
            Grid.SetRow(curHeader, 1);
            mainGrid.Children.Add(curHeader);

            // Branch list
            var scrollViewer = CreateStyledScrollViewer();
            var listStack = new StackPanel { Margin = new Thickness(0) };
            scrollViewer.Content = listStack;
            Grid.SetRow(scrollViewer, 2);
            mainGrid.Children.Add(scrollViewer);

            int rowIdx = 0;

            // Local branches section
            if (localBranches.Count > 0)
            {
                var localHeader = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x12, 0x17, 0x28)),
                    Padding = new Thickness(20, 8, 20, 8)
                };
                localHeader.Child = new TextBlock
                {
                    Text = $"LOCAL  ({localBranches.Count})",
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Text2Brush,
                    FontFamily = new FontFamily("Consolas")
                };
                listStack.Children.Add(localHeader);

                foreach (var branch in localBranches)
                {
                    bool isCur = branch == currentBranch;
                    var row = new Border
                    {
                        Background = new SolidColorBrush(isCur
                            ? Color.FromArgb(0x15, 0x2D, 0xD4, 0xBF)
                            : rowIdx % 2 == 0
                                ? Color.FromRgb(0x0F, 0x14, 0x20)
                                : Color.FromRgb(0x12, 0x18, 0x28)),
                        Padding = new Thickness(20, 10, 20, 10),
                        BorderBrush = isCur
                            ? new SolidColorBrush(Color.FromArgb(0x40, 0x2D, 0xD4, 0xBF))
                            : Brushes.Transparent,
                        BorderThickness = new Thickness(isCur ? 2 : 0, 0, 0, 0)
                    };

                    var rowPanel = new StackPanel { Orientation = Orientation.Horizontal };

                    if (isCur)
                    {
                        rowPanel.Children.Add(new TextBlock
                        {
                            Text = "\u25CF ",
                            FontSize = 12,
                            Foreground = tealBrush,
                            VerticalAlignment = VerticalAlignment.Center
                        });
                    }
                    else
                    {
                        rowPanel.Children.Add(new TextBlock
                        {
                            Text = "\u25CB ",
                            FontSize = 12,
                            Foreground = Text2Brush,
                            VerticalAlignment = VerticalAlignment.Center
                        });
                    }

                    rowPanel.Children.Add(new TextBlock
                    {
                        Text = branch,
                        FontSize = 13,
                        FontWeight = isCur ? FontWeights.Bold : FontWeights.Normal,
                        Foreground = isCur ? tealBrush : TextBrush,
                        FontFamily = new FontFamily("Consolas"),
                        VerticalAlignment = VerticalAlignment.Center
                    });

                    if (isCur)
                    {
                        rowPanel.Children.Add(new Border
                        {
                            Background = new SolidColorBrush(Color.FromArgb(0x30, 0x2D, 0xD4, 0xBF)),
                            CornerRadius = new CornerRadius(4),
                            Padding = new Thickness(8, 2, 8, 2),
                            Margin = new Thickness(12, 0, 0, 0),
                            Child = new TextBlock
                            {
                                Text = "HEAD",
                                FontSize = 10,
                                FontWeight = FontWeights.Bold,
                                Foreground = tealBrush,
                                FontFamily = new FontFamily("Consolas")
                            }
                        });
                    }

                    row.Child = rowPanel;
                    listStack.Children.Add(row);
                    rowIdx++;
                }
            }

            // Remote branches section
            if (remoteBranches.Count > 0)
            {
                var remoteHeader = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x12, 0x17, 0x28)),
                    Padding = new Thickness(20, 8, 20, 8),
                    Margin = new Thickness(0, 4, 0, 0)
                };
                remoteHeader.Child = new TextBlock
                {
                    Text = $"REMOTE  ({remoteBranches.Count})",
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Text2Brush,
                    FontFamily = new FontFamily("Consolas")
                };
                listStack.Children.Add(remoteHeader);

                foreach (var branch in remoteBranches)
                {
                    var row = new Border
                    {
                        Background = new SolidColorBrush(rowIdx % 2 == 0
                            ? Color.FromRgb(0x0F, 0x14, 0x20)
                            : Color.FromRgb(0x12, 0x18, 0x28)),
                        Padding = new Thickness(20, 10, 20, 10)
                    };

                    var rowPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    rowPanel.Children.Add(new TextBlock
                    {
                        Text = "\u2601 ",
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x81, 0x8C, 0xF8)),
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    rowPanel.Children.Add(new TextBlock
                    {
                        Text = branch,
                        FontSize = 13,
                        Foreground = Text2Brush,
                        FontFamily = new FontFamily("Consolas"),
                        VerticalAlignment = VerticalAlignment.Center
                    });

                    row.Child = rowPanel;
                    listStack.Children.Add(row);
                    rowIdx++;
                }
            }

            // Stats bar
            var statsBar = new Border
            {
                Background = SurfaceBrush,
                Padding = new Thickness(20, 10, 20, 10),
                CornerRadius = new CornerRadius(0, 0, 11, 11),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            var statsPanel = new StackPanel { Orientation = Orientation.Horizontal };
            statsPanel.Children.Add(new TextBlock
            {
                Text = $"{localBranches.Count} local",
                FontSize = 12,
                Foreground = tealBrush,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 16, 0)
            });
            statsPanel.Children.Add(new TextBlock
            {
                Text = $"{remoteBranches.Count} remote",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x81, 0x8C, 0xF8)),
                FontWeight = FontWeights.SemiBold
            });
            statsBar.Child = statsPanel;
            Grid.SetRow(statsBar, 3);
            mainGrid.Children.Add(statsBar);

            outerBorder.Child = mainGrid;
            win.Content = outerBorder;
            win.ShowDialog();
        }

        /// <summary>
        /// Show a styled stash list viewer.
        /// </summary>
        public static void ShowStashViewer(string stashOutput)
        {
            var amberBrush = new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24));
            var win = CreateViewerWindow("Git Stash List", 700, 400, amberBrush);
            var outerBorder = CreateViewerBorder(amberBrush);

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var titleBar = CreateTitleBar("Stash List — Saved Changes", amberBrush, win);
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            var scrollViewer = CreateStyledScrollViewer();
            Grid.SetRow(scrollViewer, 1);
            var listStack = new StackPanel();
            scrollViewer.Content = listStack;

            var lines = (stashOutput ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;

            foreach (var line in lines)
            {
                // Format: stash@{0}: WIP on main: abc1234 message
                //    or:  stash@{0}: On main: message
                var row = new Border
                {
                    Background = new SolidColorBrush(count % 2 == 0
                        ? Color.FromRgb(0x0F, 0x14, 0x20)
                        : Color.FromRgb(0x12, 0x18, 0x28)),
                    Padding = new Thickness(20, 12, 20, 12)
                };

                var rowPanel = new StackPanel { Orientation = Orientation.Horizontal };

                // Parse stash index
                var colonIdx = line.IndexOf(':');
                var stashId = colonIdx > 0 ? line.Substring(0, colonIdx) : $"stash@{{{count}}}";
                var stashMsg = colonIdx > 0 ? line.Substring(colonIdx + 1).Trim() : line;

                // Stash badge
                rowPanel.Children.Add(new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(0x30, 0xFB, 0xBF, 0x24)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(8, 3, 8, 3),
                    Margin = new Thickness(0, 0, 12, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = new TextBlock
                    {
                        Text = stashId,
                        FontSize = 11,
                        FontWeight = FontWeights.Bold,
                        Foreground = amberBrush,
                        FontFamily = new FontFamily("Consolas")
                    }
                });

                rowPanel.Children.Add(new TextBlock
                {
                    Text = stashMsg,
                    FontSize = 13,
                    Foreground = TextBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                });

                row.Child = rowPanel;
                listStack.Children.Add(row);
                count++;
            }

            // Stats bar
            var statsBar = new Border
            {
                Background = SurfaceBrush,
                Padding = new Thickness(20, 10, 20, 10),
                CornerRadius = new CornerRadius(0, 0, 11, 11),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            statsBar.Child = new TextBlock
            {
                Text = $"{count} stash entr{(count == 1 ? "y" : "ies")}",
                FontSize = 12,
                Foreground = amberBrush,
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(statsBar, 2);
            mainGrid.Children.Add(statsBar);

            mainGrid.Children.Add(scrollViewer);
            outerBorder.Child = mainGrid;
            win.Content = outerBorder;
            win.ShowDialog();
        }

        /// <summary>
        /// Show a styled branch graph viewer (git log --graph).
        /// </summary>
        public static void ShowGraphViewer(string structuredOutput, string graphOutput = "",
            Action<string> onCheckout = null, Action<string, string> onDiff = null)
        {
            var graphAccent = new SolidColorBrush(Color.FromRgb(0x2D, 0xD4, 0xBF));
            var win = CreateViewerWindow("Branch Graph", 1000, 680, graphAccent);
            var outerBorder = CreateViewerBorder(graphAccent);

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var titleBar = CreateTitleBar("Branch Graph — Visual Commit Tree", graphAccent, win);
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            var scrollViewer = CreateStyledScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Grid.SetRow(scrollViewer, 1);

            // Parse structured data: hash|parents|refs|subject
            var commits = new System.Collections.Generic.List<GraphCommit>();
            var lines = (structuredOutput ?? "").Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { '|' }, 4);
                if (parts.Length < 4) continue;
                commits.Add(new GraphCommit
                {
                    Hash = parts[0].Trim(),
                    Parents = parts[1].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                    Refs = parts[2].Trim(),
                    Subject = parts[3].Trim()
                });
            }

            // Build hash→index lookup
            var hashToIdx = new System.Collections.Generic.Dictionary<string, int>();
            for (int i = 0; i < commits.Count; i++)
                hashToIdx[commits[i].Hash] = i;

            // === Branch-aware lane allocation ===
            // Step 1: Find the "main" branch and trace its first-parent chain
            var mainBranchHashes = new System.Collections.Generic.HashSet<string>();
            string mainTipHash = null;

            // Find main/master tip commit
            foreach (var c in commits)
            {
                if (string.IsNullOrEmpty(c.Refs)) continue;
                // Look for main, master, or origin/main, origin/master
                var refNames = c.Refs.Split(',');
                foreach (var r in refNames)
                {
                    var rr = r.Trim();
                    if (rr == "main" || rr == "master" ||
                        rr.Contains("origin/main") || rr.Contains("origin/master") ||
                        rr.Contains("HEAD -> main") || rr.Contains("HEAD -> master"))
                    {
                        mainTipHash = c.Hash;
                        break;
                    }
                }
                if (mainTipHash != null) break;
            }

            // Trace main branch first-parent chain
            if (mainTipHash != null)
            {
                var current = mainTipHash;
                while (current != null && hashToIdx.ContainsKey(current))
                {
                    mainBranchHashes.Add(current);
                    var idx = hashToIdx[current];
                    var parents = commits[idx].Parents;
                    current = (parents.Length > 0 && !string.IsNullOrEmpty(parents[0])) ? parents[0] : null;
                }
            }
            else
            {
                // Fallback: if no main found, treat all as main
                foreach (var c in commits) mainBranchHashes.Add(c.Hash);
            }

            // Step 2: Identify branch tips (commits with refs that are NOT in main chain)
            // Each gets its own lane
            var branchLanes = new System.Collections.Generic.Dictionary<string, int>(); // hash→column for non-main branches
            int nextLane = 1; // lane 0 = main

            foreach (var c in commits)
            {
                if (mainBranchHashes.Contains(c.Hash)) continue;
                if (string.IsNullOrEmpty(c.Refs)) continue;
                // This is a branch tip not on main — assign a lane
                if (!branchLanes.ContainsKey(c.Hash))
                {
                    branchLanes[c.Hash] = nextLane;
                    // Trace back from this tip to the merge base (where it joins main)
                    var cur = c.Hash;
                    while (cur != null && hashToIdx.ContainsKey(cur))
                    {
                        if (mainBranchHashes.Contains(cur)) break;
                        branchLanes[cur] = nextLane;
                        var parents = commits[hashToIdx[cur]].Parents;
                        cur = (parents.Length > 0 && !string.IsNullOrEmpty(parents[0])) ? parents[0] : null;
                    }
                    nextLane++;
                }
            }

            // Step 3: Also assign lanes to commits not on main that have no refs
            // (intermediate commits between a branch tip and main)
            // Already handled in step 2 trace-back

            // Step 4: Assign columns
            int maxCol = 0;
            foreach (var c in commits)
            {
                if (branchLanes.ContainsKey(c.Hash))
                    c.Column = branchLanes[c.Hash];
                else
                    c.Column = 0; // main line
                if (c.Column > maxCol) maxCol = c.Column;
            }

            // Color palette for lanes
            var laneColors = new[]
            {
                Color.FromRgb(0x3E, 0xCF, 0x8E), // green
                Color.FromRgb(0x38, 0xBD, 0xF8), // sky
                Color.FromRgb(0xFF, 0x6B, 0x35), // orange
                Color.FromRgb(0x81, 0x8C, 0xF8), // lavender
                Color.FromRgb(0xFB, 0xBF, 0x24), // gold
                Color.FromRgb(0xF4, 0x72, 0xB6), // pink
                Color.FromRgb(0x2D, 0xD4, 0xBF), // teal
            };

            // Draw
            const double ROW_H = 50;
            const double COL_W = 40;
            const double LEFT_MARGIN = 24;
            const double NODE_R = 6;
            const double TEXT_LEFT = 20; // extra gap after graph area

            double graphAreaWidth = LEFT_MARGIN + (maxCol + 1) * COL_W + TEXT_LEFT;
            double totalHeight = commits.Count * ROW_H + 30;
            double totalWidth = Math.Max(graphAreaWidth + 600, 950);

            var canvas = new System.Windows.Controls.Canvas
            {
                Width = totalWidth,
                Height = totalHeight,
                Background = new SolidColorBrush(Color.FromRgb(0x0F, 0x14, 0x20))
            };

            // Draw connecting lines first (behind nodes)
            for (int i = 0; i < commits.Count; i++)
            {
                var c = commits[i];
                double cx = LEFT_MARGIN + c.Column * COL_W + COL_W / 2;
                double cy = i * ROW_H + ROW_H / 2;

                foreach (var parentHash in c.Parents)
                {
                    if (string.IsNullOrEmpty(parentHash)) continue;
                    if (!hashToIdx.ContainsKey(parentHash)) continue;
                    int pIdx = hashToIdx[parentHash];
                    var parent = commits[pIdx];
                    double px = LEFT_MARGIN + parent.Column * COL_W + COL_W / 2;
                    double py = pIdx * ROW_H + ROW_H / 2;

                    var lineColor = laneColors[c.Column % laneColors.Length];

                    if (c.Column == parent.Column)
                    {
                        // Straight vertical line
                        var line = new System.Windows.Shapes.Line
                        {
                            X1 = cx, Y1 = cy + NODE_R,
                            X2 = px, Y2 = py - NODE_R,
                            Stroke = new SolidColorBrush(lineColor),
                            StrokeThickness = 2.5,
                            Opacity = 0.8
                        };
                        canvas.Children.Add(line);
                    }
                    else
                    {
                        // Curved connection using Bezier path
                        var path = new System.Windows.Shapes.Path();
                        var geo = new PathGeometry();
                        var figure = new PathFigure { StartPoint = new Point(cx, cy + NODE_R) };

                        // Bezier curve: go down from current, curve to parent column
                        double midY = (cy + py) / 2;
                        var bezier = new BezierSegment(
                            new Point(cx, midY),
                            new Point(px, midY),
                            new Point(px, py - NODE_R),
                            true);
                        figure.Segments.Add(bezier);
                        geo.Figures.Add(figure);
                        path.Data = geo;
                        path.Stroke = new SolidColorBrush(lineColor);
                        path.StrokeThickness = 2.5;
                        path.Opacity = 0.7;
                        path.Fill = null;
                        canvas.Children.Add(path);
                    }
                }
            }

            // Diff selection state (stored in selectionLabel.Tag)
            var selectionLabel = new TextBlock
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x80, 0x88, 0xA0)),
                Margin = new Thickness(16, 8, 16, 8),
                Text = "Right-click a commit for actions  |  Select two commits to compare"
            };

            // Draw nodes and labels
            for (int i = 0; i < commits.Count; i++)
            {
                var c = commits[i];
                double cx = LEFT_MARGIN + c.Column * COL_W + COL_W / 2;
                double cy = i * ROW_H + ROW_H / 2;
                var nodeColor = laneColors[c.Column % laneColors.Length];
                bool isHead = c.Refs.Contains("HEAD");

                // Clickable row background (transparent but hit-testable)
                var rowHit = new System.Windows.Shapes.Rectangle
                {
                    Width = totalWidth,
                    Height = ROW_H,
                    Fill = new SolidColorBrush(i % 2 == 0
                        ? Color.FromArgb(0x00, 0, 0, 0)
                        : Color.FromArgb(0x0A, 0xFF, 0xFF, 0xFF)),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = c // store commit reference
                };
                System.Windows.Controls.Canvas.SetLeft(rowHit, 0);
                System.Windows.Controls.Canvas.SetTop(rowHit, i * ROW_H);

                // Hover effect
                var rowIndex = i;
                rowHit.MouseEnter += (s, e) =>
                {
                    rowHit.Fill = new SolidColorBrush(Color.FromArgb(0x18, 0xFF, 0xFF, 0xFF));
                };
                rowHit.MouseLeave += (s, e) =>
                {
                    rowHit.Fill = new SolidColorBrush(rowIndex % 2 == 0
                        ? Color.FromArgb(0x00, 0, 0, 0)
                        : Color.FromArgb(0x0A, 0xFF, 0xFF, 0xFF));
                };

                // Build context menu
                var ctxMenu = CreateGraphContextMenu(c, win, onCheckout, onDiff,
                    rowHit, selectionLabel);
                rowHit.ContextMenu = ctxMenu;
                canvas.Children.Insert(0, rowHit); // behind graph elements

                // Node glow for HEAD
                if (isHead)
                {
                    var glow = new System.Windows.Shapes.Ellipse
                    {
                        Width = NODE_R * 5, Height = NODE_R * 5,
                        Fill = new SolidColorBrush(Color.FromArgb(0x40, nodeColor.R, nodeColor.G, nodeColor.B)),
                        IsHitTestVisible = false
                    };
                    System.Windows.Controls.Canvas.SetLeft(glow, cx - NODE_R * 2.5);
                    System.Windows.Controls.Canvas.SetTop(glow, cy - NODE_R * 2.5);
                    canvas.Children.Add(glow);
                }

                // Node circle
                var circle = new System.Windows.Shapes.Ellipse
                {
                    Width = NODE_R * 2, Height = NODE_R * 2,
                    Fill = new SolidColorBrush(nodeColor),
                    Stroke = new SolidColorBrush(Color.FromRgb(0x0F, 0x14, 0x20)),
                    StrokeThickness = 2,
                    IsHitTestVisible = false
                };
                System.Windows.Controls.Canvas.SetLeft(circle, cx - NODE_R);
                System.Windows.Controls.Canvas.SetTop(circle, cy - NODE_R);
                canvas.Children.Add(circle);

                // Text area: hash + refs + subject
                double textX = graphAreaWidth;
                double textY = cy - 16;

                // Hash
                var hashTb = new TextBlock
                {
                    Text = c.Hash,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24)),
                    IsHitTestVisible = false
                };
                System.Windows.Controls.Canvas.SetLeft(hashTb, textX);
                System.Windows.Controls.Canvas.SetTop(hashTb, textY);
                canvas.Children.Add(hashTb);

                // Ref badges
                double badgeX = textX + 65;
                if (!string.IsNullOrWhiteSpace(c.Refs))
                {
                    var refs = c.Refs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var rawRef in refs)
                    {
                        var refName = rawRef.Trim();
                        if (string.IsNullOrEmpty(refName)) continue;

                        // Determine badge color
                        Color badgeColor;
                        if (refName.Contains("HEAD"))
                            badgeColor = Color.FromRgb(0xEF, 0x44, 0x44); // red for HEAD
                        else if (refName.StartsWith("tag:"))
                            badgeColor = Color.FromRgb(0xFB, 0xBF, 0x24); // gold for tags
                        else if (refName.Contains("origin/"))
                            badgeColor = Color.FromRgb(0x81, 0x8C, 0xF8); // lavender for remote
                        else
                            badgeColor = Color.FromRgb(0x3E, 0xCF, 0x8E); // green for local

                        // Clean display name
                        var displayRef = refName.Replace("HEAD -> ", "HEAD\u2192");

                        var badge = new Border
                        {
                            Background = new SolidColorBrush(Color.FromArgb(0x30, badgeColor.R, badgeColor.G, badgeColor.B)),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(0x80, badgeColor.R, badgeColor.G, badgeColor.B)),
                            BorderThickness = new Thickness(1),
                            CornerRadius = new CornerRadius(4),
                            Padding = new Thickness(6, 1, 6, 1),
                            IsHitTestVisible = false,
                            Child = new TextBlock
                            {
                                Text = displayRef,
                                FontFamily = new FontFamily("Consolas"),
                                FontSize = 10,
                                Foreground = new SolidColorBrush(badgeColor),
                                FontWeight = FontWeights.Bold
                            }
                        };
                        badge.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        System.Windows.Controls.Canvas.SetLeft(badge, badgeX);
                        System.Windows.Controls.Canvas.SetTop(badge, textY - 2);
                        canvas.Children.Add(badge);
                        badgeX += badge.DesiredSize.Width + 5;
                    }
                }

                // Subject line
                var subjectTb = new TextBlock
                {
                    Text = c.Subject,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0xC8, 0xE0)),
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 500,
                    IsHitTestVisible = false
                };
                System.Windows.Controls.Canvas.SetLeft(subjectTb, textX);
                System.Windows.Controls.Canvas.SetTop(subjectTb, textY + 16);
                canvas.Children.Add(subjectTb);
            }

            // Bottom status bar
            var statusBar = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x12, 0x18, 0x28)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x34, 0x54)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Child = selectionLabel
            };

            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(statusBar, 2);
            mainGrid.Children.Add(statusBar);

            scrollViewer.Content = canvas;
            mainGrid.Children.Add(scrollViewer);
            outerBorder.Child = mainGrid;
            win.Content = outerBorder;
            win.ShowDialog();
        }

        /// <summary>
        /// Extracts a local branch name from refs string (e.g. "HEAD -> Test1, origin/main" → "Test1")
        /// </summary>
        private static string GetLocalBranchFromRefs(string refs)
        {
            if (string.IsNullOrWhiteSpace(refs)) return null;
            foreach (var r in refs.Split(','))
            {
                var trimmed = r.Trim();
                // "HEAD -> branchname"
                if (trimmed.Contains("HEAD -> "))
                {
                    return trimmed.Substring(trimmed.IndexOf("HEAD -> ") + 8).Trim();
                }
                // Skip origin/, tags
                if (trimmed.StartsWith("origin/") || trimmed.StartsWith("tag:")) continue;
                // Plain branch name like "main", "Test1"
                if (!string.IsNullOrWhiteSpace(trimmed) && trimmed != "HEAD")
                    return trimmed;
            }
            return null;
        }

        private static System.Windows.Controls.ContextMenu CreateGraphContextMenu(
            GraphCommit c, Window win,
            Action<string> onCheckout, Action<string, string> onDiff,
            System.Windows.Shapes.Rectangle rowHit,
            TextBlock selectionLabel)
        {
            var menuBg = Color.FromRgb(0x14, 0x1A, 0x2E);
            var menuBorder = Color.FromRgb(0x30, 0x3C, 0x60);
            var hoverBg = Color.FromRgb(0x1E, 0x28, 0x45);

            // Custom ContextMenu template — fully dark, no white borders
            var ctxMenuTemplate = new ControlTemplate(typeof(System.Windows.Controls.ContextMenu));
            var ctxBorderFactory = new FrameworkElementFactory(typeof(Border));
            ctxBorderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(menuBg));
            ctxBorderFactory.SetValue(Border.BorderBrushProperty, new SolidColorBrush(menuBorder));
            ctxBorderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            ctxBorderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            ctxBorderFactory.SetValue(Border.PaddingProperty, new Thickness(0, 6, 0, 6));
            ctxBorderFactory.SetValue(UIElement.SnapsToDevicePixelsProperty, true);
            var ctxItemsFactory = new FrameworkElementFactory(typeof(StackPanel));
            ctxItemsFactory.SetValue(StackPanel.IsItemsHostProperty, true);
            ctxItemsFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0));
            ctxBorderFactory.AppendChild(ctxItemsFactory);
            ctxMenuTemplate.VisualTree = ctxBorderFactory;

            var ctxMenu = new System.Windows.Controls.ContextMenu
            {
                Background = new SolidColorBrush(menuBg),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                HasDropShadow = true,
                Template = ctxMenuTemplate
            };

            // Styled menu item builder
            // Custom MenuItem template that removes the white icon column
            var menuItemTemplate = new ControlTemplate(typeof(System.Windows.Controls.MenuItem));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "Bd";
            borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            borderFactory.SetValue(Border.PaddingProperty, new Thickness(0));
            var cpFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            cpFactory.SetValue(ContentPresenter.ContentSourceProperty, "Header");
            cpFactory.SetValue(ContentPresenter.MarginProperty, new Thickness(0));
            cpFactory.SetValue(ContentPresenter.RecognizesAccessKeyProperty, false);
            borderFactory.AppendChild(cpFactory);
            menuItemTemplate.VisualTree = borderFactory;
            // Hover trigger
            var hoverTrigger = new Trigger { Property = System.Windows.Controls.MenuItem.IsHighlightedProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(hoverBg), "Bd"));
            menuItemTemplate.Triggers.Add(hoverTrigger);

            Func<string, string, Color, System.Windows.Controls.MenuItem> makeItem = (icon, text, color) =>
            {
                var panel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
                panel.Children.Add(new TextBlock
                {
                    Text = icon,
                    FontSize = 15,
                    Width = 26,
                    Foreground = new SolidColorBrush(color),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                });
                panel.Children.Add(new TextBlock
                {
                    Text = text,
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xE0, 0xE4, 0xF0)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(6, 0, 12, 0)
                });
                var item = new System.Windows.Controls.MenuItem
                {
                    Header = panel,
                    Padding = new Thickness(10, 7, 16, 7),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Template = menuItemTemplate
                };
                return item;
            };

            // Separator builder
            Func<System.Windows.Controls.Separator> makeSep = () =>
            {
                return new System.Windows.Controls.Separator
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x34, 0x54)),
                    Margin = new Thickness(12, 5, 12, 5),
                    Height = 1
                };
            };

            // --- Header: commit info ---
            var headerPanel = new StackPanel { Margin = new Thickness(14, 6, 14, 8) };
            headerPanel.Children.Add(new TextBlock
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24)),
                Text = c.Hash
            });
            var subjectTrim = c.Subject.Length > 45 ? c.Subject.Substring(0, 45) + "\u2026" : c.Subject;
            headerPanel.Children.Add(new TextBlock
            {
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x70, 0x78, 0x98)),
                Text = subjectTrim,
                Margin = new Thickness(0, 2, 0, 0)
            });
            ctxMenu.Items.Add(new System.Windows.Controls.MenuItem
            {
                Header = headerPanel,
                IsEnabled = false,
                Padding = new Thickness(0),
                Background = Brushes.Transparent,
                Template = menuItemTemplate
            });
            ctxMenu.Items.Add(makeSep());

            // --- Switch to branch ---
            var localBranch = GetLocalBranchFromRefs(c.Refs);
            if (localBranch != null && onCheckout != null)
            {
                var switchItem = makeItem("\u2387", $"Switch to  {localBranch}", Color.FromRgb(0x3E, 0xCF, 0x8E));
                var branchName = localBranch;
                switchItem.Click += (s, e) =>
                {
                    win.Close();
                    onCheckout(branchName);
                };
                ctxMenu.Items.Add(switchItem);
                ctxMenu.Items.Add(makeSep());
            }

            // --- Select as Diff A ---
            var selectA = makeItem("\u2460", "Set as compare source  (A)", Color.FromRgb(0x38, 0xBD, 0xF8));
            var capturedCommitA = c;
            var capturedRowA = rowHit;
            selectA.Click += (s, e) =>
            {
                selectionLabel.Tag = capturedCommitA.Hash;
                capturedRowA.Fill = new SolidColorBrush(Color.FromArgb(0x20, 0x38, 0xBD, 0xF8));
                selectionLabel.Text = $"\u2460 Source (A): {capturedCommitA.Hash}  \u2014  {capturedCommitA.Subject}     Right-click another commit \u2192 Compare";
                selectionLabel.Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8));
            };
            ctxMenu.Items.Add(selectA);

            // --- Compare with A ---
            if (onDiff != null)
            {
                var compareItem = makeItem("\u2194", "Compare with source  (A \u2194 B)", Color.FromRgb(0xFB, 0xBF, 0x24));
                var capturedCommitB = c;
                compareItem.Click += (s, e) =>
                {
                    var hashA = selectionLabel.Tag as string;
                    if (string.IsNullOrEmpty(hashA))
                    {
                        selectionLabel.Text = "\u26A0  Select a source commit (A) first, then compare";
                        selectionLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                        return;
                    }
                    if (hashA == capturedCommitB.Hash)
                    {
                        selectionLabel.Text = "\u26A0  Cannot compare a commit with itself";
                        selectionLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                        return;
                    }
                    win.Close();
                    onDiff(hashA, capturedCommitB.Hash);
                };
                ctxMenu.Items.Add(compareItem);
            }

            return ctxMenu;
        }

        private class GraphCommit
        {
            public string Hash { get; set; }
            public string[] Parents { get; set; }
            public string Refs { get; set; }
            public string Subject { get; set; }
            public int Column { get; set; }
        }

        /// <summary>
        /// Show a styled diff viewer between two commits.
        /// </summary>
        public static void ShowDiffViewer(string hashA, string hashB, string statOutput, string fullOutput)
        {
            var diffAccent = new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24));
            var win = CreateViewerWindow($"Diff: {hashA} ↔ {hashB}", 950, 650, diffAccent);
            var outerBorder = CreateViewerBorder(diffAccent);

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var titleBar = CreateTitleBar($"Diff: {hashA} \u2194 {hashB}", diffAccent, win);
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            var scrollViewer = CreateStyledScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Grid.SetRow(scrollViewer, 1);

            var contentStack = new StackPanel { Margin = new Thickness(16, 8, 16, 16) };

            // Stat summary
            if (!string.IsNullOrWhiteSpace(statOutput))
            {
                var statBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x14, 0x1A, 0x2C)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x34, 0x54)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(16, 12, 16, 12),
                    Margin = new Thickness(0, 0, 0, 16)
                };

                var statPanel = new StackPanel();
                statPanel.Children.Add(new TextBlock
                {
                    Text = "Files Changed",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24)),
                    Margin = new Thickness(0, 0, 0, 8)
                });

                foreach (var line in statOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    var tb = new TextBlock
                    {
                        FontFamily = new FontFamily("Consolas"),
                        FontSize = 12,
                        Margin = new Thickness(0, 2, 0, 2)
                    };

                    // Color the +/- indicators
                    if (trimmed.Contains("|"))
                    {
                        var pipeIdx = trimmed.IndexOf('|');
                        tb.Inlines.Add(new System.Windows.Documents.Run(trimmed.Substring(0, pipeIdx + 1))
                        {
                            Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0xC8, 0xE0))
                        });
                        var rest = trimmed.Substring(pipeIdx + 1);
                        foreach (char ch in rest)
                        {
                            if (ch == '+')
                                tb.Inlines.Add(new System.Windows.Documents.Run("+") { Foreground = new SolidColorBrush(Color.FromRgb(0x3E, 0xCF, 0x8E)) });
                            else if (ch == '-')
                                tb.Inlines.Add(new System.Windows.Documents.Run("-") { Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)) });
                            else
                                tb.Inlines.Add(new System.Windows.Documents.Run(ch.ToString()) { Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0xC8, 0xE0)) });
                        }
                    }
                    else
                    {
                        tb.Foreground = new SolidColorBrush(Color.FromRgb(0x80, 0x88, 0xA0));
                        tb.Text = trimmed;
                        tb.FontWeight = FontWeights.SemiBold;
                    }
                    statPanel.Children.Add(tb);
                }

                statBorder.Child = statPanel;
                contentStack.Children.Add(statBorder);
            }

            // Full diff
            if (!string.IsNullOrWhiteSpace(fullOutput))
            {
                var diffBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(0x0C, 0x10, 0x1C)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x34, 0x54)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(16, 12, 16, 12)
                };

                var diffPanel = new StackPanel();
                diffPanel.Children.Add(new TextBlock
                {
                    Text = "Full Diff",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x81, 0x8C, 0xF8)),
                    Margin = new Thickness(0, 0, 0, 8)
                });

                foreach (var line in fullOutput.Split(new[] { '\n' }, StringSplitOptions.None))
                {
                    Color lineColor;
                    if (line.StartsWith("+"))
                        lineColor = Color.FromRgb(0x3E, 0xCF, 0x8E); // green
                    else if (line.StartsWith("-"))
                        lineColor = Color.FromRgb(0xEF, 0x44, 0x44); // red
                    else if (line.StartsWith("@@"))
                        lineColor = Color.FromRgb(0x81, 0x8C, 0xF8); // lavender
                    else if (line.StartsWith("diff ") || line.StartsWith("index "))
                        lineColor = Color.FromRgb(0xFB, 0xBF, 0x24); // gold
                    else
                        lineColor = Color.FromRgb(0x80, 0x88, 0xA0); // dim

                    var bgColor = line.StartsWith("+") ? Color.FromArgb(0x10, 0x3E, 0xCF, 0x8E)
                                : line.StartsWith("-") ? Color.FromArgb(0x10, 0xEF, 0x44, 0x44)
                                : Colors.Transparent;

                    var lineBorder = new Border
                    {
                        Background = new SolidColorBrush(bgColor),
                        Padding = new Thickness(4, 0, 4, 0),
                        Child = new TextBlock
                        {
                            Text = line,
                            FontFamily = new FontFamily("Consolas"),
                            FontSize = 11,
                            Foreground = new SolidColorBrush(lineColor),
                            TextWrapping = TextWrapping.NoWrap
                        }
                    };
                    diffPanel.Children.Add(lineBorder);
                }

                diffBorder.Child = diffPanel;
                contentStack.Children.Add(diffBorder);
            }

            scrollViewer.Content = contentStack;
            mainGrid.Children.Add(scrollViewer);
            outerBorder.Child = mainGrid;
            win.Content = outerBorder;
            win.ShowDialog();
        }

        /// <summary>
        /// Show a styled git log viewer.
        /// </summary>
        public static void ShowLogViewer(string logOutput)
        {
            var cyan = new SolidColorBrush(Color.FromRgb(0x22, 0xD3, 0xEE));
            var win = CreateViewerWindow("Git Log — Commit History", 850, 550, cyan);
            var outerBorder = CreateViewerBorder(cyan);

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Title bar
            var titleBar = CreateTitleBar("Commit History", cyan, win);
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            // Content
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var contentPanel = new StackPanel { Margin = new Thickness(0) };
            var lines = logOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int commitCount = 0;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim().Trim('"');
                if (string.IsNullOrEmpty(line)) continue;

                // Parse: hash|date|author|message
                var parts = line.Split(new[] { '|' }, 4);
                commitCount++;
                var isEven = commitCount % 2 == 0;

                var rowBorder = new Border
                {
                    Background = isEven
                        ? new SolidColorBrush(Color.FromArgb(0x08, 0x22, 0xD3, 0xEE))
                        : Brushes.Transparent,
                    Padding = new Thickness(20, 8, 20, 8)
                };

                if (parts.Length >= 4)
                {
                    var hash = parts[0];
                    var date = parts[1];
                    var author = parts[2];
                    var message = parts[3];

                    // Parse date to friendly format
                    string friendlyDate = date;
                    if (DateTime.TryParse(date, out DateTime dt))
                    {
                        var diff = DateTime.Now - dt;
                        if (diff.TotalMinutes < 60) friendlyDate = $"{(int)diff.TotalMinutes}m ago";
                        else if (diff.TotalHours < 24) friendlyDate = $"{(int)diff.TotalHours}h ago";
                        else if (diff.TotalDays < 7) friendlyDate = $"{(int)diff.TotalDays}d ago";
                        else friendlyDate = dt.ToString("yyyy-MM-dd HH:mm");
                    }

                    var rowGrid = new Grid();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });  // hash
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // message
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // author
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // date

                    // Hash
                    var hashBlock = new TextBlock
                    {
                        Text = hash,
                        FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
                        FontSize = 12,
                        Foreground = cyan,
                        FontWeight = FontWeights.SemiBold,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(hashBlock, 0);
                    rowGrid.Children.Add(hashBlock);

                    // Message
                    var msgBlock = new TextBlock
                    {
                        Text = message,
                        FontSize = 12,
                        Foreground = TextBrush,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        Margin = new Thickness(8, 0, 8, 0)
                    };
                    Grid.SetColumn(msgBlock, 1);
                    rowGrid.Children.Add(msgBlock);

                    // Author
                    var authorBlock = new TextBlock
                    {
                        Text = author,
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(0xA7, 0x8B, 0xFA)),
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    Grid.SetColumn(authorBlock, 2);
                    rowGrid.Children.Add(authorBlock);

                    // Date
                    var dateBlock = new TextBlock
                    {
                        Text = friendlyDate,
                        FontSize = 11,
                        Foreground = Text2Brush,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    Grid.SetColumn(dateBlock, 3);
                    rowGrid.Children.Add(dateBlock);

                    rowBorder.Child = rowGrid;
                }
                else
                {
                    // Fallback: oneline format
                    var textBlock = new TextBlock
                    {
                        Text = line,
                        FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New"),
                        FontSize = 12,
                        Foreground = TextBrush
                    };
                    rowBorder.Child = textBlock;
                }

                contentPanel.Children.Add(rowBorder);
            }

            scrollViewer.Content = contentPanel;
            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);

            // Bottom bar
            var bottomBar = new Border
            {
                Background = SurfaceBrush,
                Padding = new Thickness(20, 10, 20, 14),
                CornerRadius = new CornerRadius(0, 0, 12, 12)
            };
            var bottomPanel = new DockPanel();
            var countText = new TextBlock
            {
                Text = $"Showing {commitCount} commit{(commitCount != 1 ? "s" : "")}",
                FontSize = 12,
                Foreground = TextBrush,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };

            var closeBtn = MakeButton("Close", null, false);
            closeBtn.Click += (s, e) => win.Close();
            DockPanel.SetDock(closeBtn, Dock.Right);
            bottomPanel.Children.Add(closeBtn);
            bottomPanel.Children.Add(countText);
            bottomBar.Child = bottomPanel;
            Grid.SetRow(bottomBar, 2);
            mainGrid.Children.Add(bottomBar);

            outerBorder.Child = mainGrid;
            win.Content = outerBorder;
            win.ShowDialog();
        }

        // === Help Viewer ===

        // Color palette for help viewer
        private static readonly Color HelpAccent1 = Color.FromRgb(0x38, 0xBD, 0xF8); // sky blue
        private static readonly Color HelpAccent2 = Color.FromRgb(0x81, 0x8C, 0xF8); // lavender
        private static readonly Color HelpGold = Color.FromRgb(0xFB, 0xBF, 0x24);
        private static readonly Color HelpMint = Color.FromRgb(0x34, 0xD3, 0x99);

        /// <summary>
        /// Show the professional Hebrew help guide with sidebar navigation.
        /// Sidebar on RIGHT (Hebrew RTL), content on LEFT.
        /// </summary>
        public static void ShowHelpViewer(int initialChapterIndex = 0)
        {
            var chapters = HelpContent.GetChapters();

            var win = new Window
            {
                Title = "מדריך עזרה",
                Width = 1150, Height = 880,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                Background = Brushes.Transparent,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
            };

            // Outer glow border
            var outerBorder = new Border
            {
                Background = Brushes.Transparent,
                Margin = new Thickness(16),
                CornerRadius = new CornerRadius(16),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = HelpAccent1,
                    BlurRadius = 40,
                    Opacity = 0.3,
                    ShadowDepth = 0
                }
            };

            // Inner border with gradient border effect
            var innerBorder = new Border
            {
                CornerRadius = new CornerRadius(14),
                BorderThickness = new Thickness(1.5),
                ClipToBounds = true
            };
            innerBorder.BorderBrush = new LinearGradientBrush(
                HelpAccent1, HelpAccent2, 0);

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // ═══ TITLE BAR ═══
            var titleBar = new Border
            {
                CornerRadius = new CornerRadius(13, 13, 0, 0),
                Padding = new Thickness(0),
            };
            titleBar.Background = new LinearGradientBrush(
                Color.FromRgb(0x0C, 0x10, 0x1C),
                Color.FromRgb(0x12, 0x18, 0x2B), 90);

            var titleOuter = new DockPanel { Margin = new Thickness(0) };

            // Accent top line
            var topAccent = new Border
            {
                Height = 3,
                CornerRadius = new CornerRadius(13, 13, 0, 0),
            };
            topAccent.Background = new LinearGradientBrush(
                HelpAccent1, HelpAccent2, 0);
            DockPanel.SetDock(topAccent, Dock.Top);
            titleOuter.Children.Add(topAccent);

            var titleContent = new DockPanel { Margin = new Thickness(24, 12, 24, 12) };

            // Close button
            var closeBtn = new Button
            {
                Content = "\u2715",
                FontSize = 14,
                Foreground = Text2Brush,
                Background = new SolidColorBrush(Color.FromRgb(0x1A, 0x20, 0x35)),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x34, 0x54)),
                Cursor = System.Windows.Input.Cursors.Hand,
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            closeBtn.Click += (s, e) => win.Close();
            DockPanel.SetDock(closeBtn, Dock.Left);
            titleContent.Children.Add(closeBtn);

            // Title right side (RTL: visually on right)
            var titleRight = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                FlowDirection = FlowDirection.RightToLeft
            };
            titleRight.Children.Add(new TextBlock
            {
                Text = "מדריך עזרה",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = TextBrush,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0)
            });
            titleRight.Children.Add(new TextBlock
            {
                Text = "  |  ",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0x2A, 0x34, 0x54)),
                VerticalAlignment = VerticalAlignment.Center
            });
            titleRight.Children.Add(new TextBlock
            {
                Text = "ClearCase \u2192 GitLab Migration Guide",
                FontSize = 12,
                Foreground = Text2Brush,
                VerticalAlignment = VerticalAlignment.Center
            });
            DockPanel.SetDock(titleRight, Dock.Right);
            titleContent.Children.Add(titleRight);

            titleOuter.Children.Add(titleContent);
            titleBar.Child = titleOuter;
            Grid.SetRow(titleBar, 0);
            root.Children.Add(titleBar);
            titleBar.MouseLeftButtonDown += (s, e) => win.DragMove();

            // ═══ MAIN AREA: Content (left) + Sidebar (right) ═══
            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // content
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) }); // sidebar
            Grid.SetRow(mainGrid, 1);
            mainGrid.Background = new SolidColorBrush(Color.FromRgb(0x0B, 0x0F, 0x1A));

            // ─── SIDEBAR (Column 1 = RIGHT) ───
            var sidebarBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x22, 0x3A)),
                BorderThickness = new Thickness(1, 0, 0, 0),
                CornerRadius = new CornerRadius(0, 0, 13, 0)
            };
            sidebarBorder.Background = new LinearGradientBrush(
                Color.FromRgb(0x0A, 0x0E, 0x1A),
                Color.FromRgb(0x0E, 0x13, 0x22), 180);
            Grid.SetColumn(sidebarBorder, 1);

            var sidebarDock = new DockPanel();

            // Sidebar header
            var navHeader = new Border
            {
                Padding = new Thickness(20, 18, 20, 14),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x22, 0x3A)),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            var navHeaderStack = new StackPanel { FlowDirection = FlowDirection.RightToLeft };
            var navTitleRow = new StackPanel { Orientation = Orientation.Horizontal, FlowDirection = FlowDirection.RightToLeft };
            navTitleRow.Children.Add(new TextBlock
            {
                Text = "\u2630",
                FontSize = 16,
                Foreground = new SolidColorBrush(HelpAccent1),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            });
            navTitleRow.Children.Add(new TextBlock
            {
                Text = "תוכן עניינים",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = TextBrush,
                VerticalAlignment = VerticalAlignment.Center
            });
            navHeaderStack.Children.Add(navTitleRow);

            // Progress bar under header
            var progressBg = new Border
            {
                Height = 3,
                CornerRadius = new CornerRadius(2),
                Background = new SolidColorBrush(Color.FromRgb(0x1A, 0x22, 0x3A)),
                Margin = new Thickness(0, 12, 0, 0)
            };
            var progressFill = new Border
            {
                Height = 3,
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 0
            };
            progressFill.Background = new LinearGradientBrush(HelpAccent1, HelpAccent2, 0);
            var progressGrid = new Grid { Height = 3, Margin = new Thickness(0, 12, 0, 0) };
            progressGrid.Children.Add(progressBg);
            progressGrid.Children.Add(progressFill);
            navHeaderStack.Children.Add(progressGrid);

            navHeader.Child = navHeaderStack;
            DockPanel.SetDock(navHeader, Dock.Top);
            sidebarDock.Children.Add(navHeader);

            // Sidebar footer
            var navFooter = new Border
            {
                Padding = new Thickness(20, 12, 20, 12),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x1A, 0x22, 0x3A)),
                BorderThickness = new Thickness(0, 1, 0, 0),
            };
            var footerText = new TextBlock
            {
                FlowDirection = FlowDirection.RightToLeft,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x4A, 0x55, 0x78)),
                TextAlignment = TextAlignment.Center
            };
            footerText.Inlines.Add(new System.Windows.Documents.Run("CC\u2192GL ") { FontWeight = FontWeights.Bold });
            footerText.Inlines.Add(new System.Windows.Documents.Run("Migration Tool v1.0"));
            navFooter.Child = footerText;
            DockPanel.SetDock(navFooter, Dock.Bottom);
            sidebarDock.Children.Add(navFooter);

            // Nav scroll area
            var navScroll = CreateStyledScrollViewer();
            var navStack = new StackPanel { Margin = new Thickness(10, 8, 10, 8) };

            // ─── CONTENT AREA (Column 0 = LEFT) ───
            var contentScroll = CreateStyledScrollViewer();
            var contentPanel = new StackPanel
            {
                Margin = new Thickness(40, 30, 40, 40),
                FlowDirection = FlowDirection.RightToLeft
            };
            Grid.SetColumn(contentScroll, 0);
            contentScroll.Content = contentPanel;

            // Track state
            var navButtons = new List<Border>();
            var currentIndex = new int[] { -1 };

            // ─── BUILD NAVIGATION ITEMS ───
            for (int i = 0; i < chapters.Count; i++)
            {
                var chapter = chapters[i];
                var idx = i;

                // Determine category color
                Color catColor;
                if (idx == 0) catColor = HelpGold;                          // intro
                else if (idx <= 9) catColor = HelpAccent1;                  // steps
                else if (idx == chapters.Count - 1) catColor = HelpMint;    // glossary
                else catColor = HelpAccent2;                                // extras

                var navItem = new Border
                {
                    Padding = new Thickness(12, 9, 12, 9),
                    Margin = new Thickness(0, 1, 0, 1),
                    CornerRadius = new CornerRadius(8),
                    Background = Brushes.Transparent,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    FlowDirection = FlowDirection.RightToLeft
                };

                var navGrid = new Grid();
                navGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
                navGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Number/icon badge
                var badge = new Border
                {
                    Width = 28, Height = 28,
                    CornerRadius = new CornerRadius(8),
                    Background = new SolidColorBrush(Color.FromArgb(0x20, catColor.R, catColor.G, catColor.B)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var badgeText = new TextBlock
                {
                    Text = chapter.Icon,
                    FontSize = chapter.Icon.Length <= 2 ? 11 : 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(catColor),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontFamily = chapter.Icon.Length <= 2
                        ? new FontFamily("Consolas")
                        : new FontFamily("Segoe UI Emoji")
                };
                badge.Child = badgeText;
                Grid.SetColumn(badge, 0);
                navGrid.Children.Add(badge);

                // Title — extract just the Hebrew part after "—" if present
                var displayTitle = chapter.Title;
                var navTitleBlock = new TextBlock
                {
                    Text = displayTitle,
                    FontSize = 12.5,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xCF, 0xE5)),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                Grid.SetColumn(navTitleBlock, 1);
                navGrid.Children.Add(navTitleBlock);

                navItem.Child = navGrid;
                navButtons.Add(navItem);

                // Click
                navItem.MouseLeftButtonDown += (s, e) =>
                {
                    HelpRenderChapter(contentPanel, chapters[idx], idx);
                    HelpSetActiveNav(navButtons, idx, currentIndex, progressFill, navStack.ActualWidth, chapters.Count);
                    contentScroll.ScrollToTop();
                };
                // Hover
                navItem.MouseEnter += (s, e) =>
                {
                    if (currentIndex[0] != idx)
                        navItem.Background = new SolidColorBrush(Color.FromRgb(0x14, 0x1B, 0x2E));
                };
                navItem.MouseLeave += (s, e) =>
                {
                    if (currentIndex[0] != idx)
                        navItem.Background = Brushes.Transparent;
                };

                navStack.Children.Add(navItem);
            }

            navScroll.Content = navStack;
            sidebarDock.Children.Add(navScroll);
            sidebarBorder.Child = sidebarDock;

            mainGrid.Children.Add(contentScroll);
            mainGrid.Children.Add(sidebarBorder);
            root.Children.Add(mainGrid);

            innerBorder.Child = root;
            outerBorder.Child = innerBorder;
            win.Content = outerBorder;

            // Show initial chapter
            var startIdx = Math.Max(0, Math.Min(initialChapterIndex, chapters.Count - 1));
            HelpRenderChapter(contentPanel, chapters[startIdx], startIdx);
            HelpSetActiveNav(navButtons, startIdx, currentIndex, progressFill, 240, chapters.Count);

            win.ShowDialog();
        }

        // ── Render chapter content ──
        private static void HelpRenderChapter(StackPanel panel, HelpContent.HelpChapter chapter, int index)
        {
            panel.Children.Clear();

            // Determine chapter accent color
            Color accent;
            if (index == 0) accent = HelpGold;
            else if (index <= 9) accent = HelpAccent1;
            else accent = HelpAccent2;

            // ── HERO HEADER ──
            var heroBorder = new Border
            {
                CornerRadius = new CornerRadius(14),
                Margin = new Thickness(0, 0, 0, 28),
                Padding = new Thickness(28, 24, 28, 24),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x30, accent.R, accent.G, accent.B))
            };
            heroBorder.Background = new LinearGradientBrush(
                Color.FromArgb(0x30, accent.R, accent.G, accent.B),
                Color.FromArgb(0x08, accent.R, accent.G, accent.B), 135);

            var heroStack = new StackPanel { FlowDirection = FlowDirection.RightToLeft };

            // Icon badge large
            var heroBadge = new Border
            {
                Width = 52, Height = 52,
                CornerRadius = new CornerRadius(14),
                Margin = new Thickness(0, 0, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            heroBadge.Background = new LinearGradientBrush(
                Color.FromArgb(0x40, accent.R, accent.G, accent.B),
                Color.FromArgb(0x15, accent.R, accent.G, accent.B), 135);
            heroBadge.Child = new TextBlock
            {
                Text = chapter.Icon,
                FontSize = chapter.Icon.Length <= 2 ? 22 : 26,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(accent),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = chapter.Icon.Length <= 2
                    ? new FontFamily("Consolas")
                    : new FontFamily("Segoe UI Emoji")
            };
            heroStack.Children.Add(heroBadge);

            // Chapter title
            heroStack.Children.Add(new TextBlock
            {
                Text = chapter.Title,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = TextBrush,
                TextWrapping = TextWrapping.Wrap
            });

            // Subtitle line
            var subtitleLine = new Border
            {
                Height = 3,
                Width = 60,
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 14, 0, 0)
            };
            subtitleLine.Background = new LinearGradientBrush(
                accent, Color.FromArgb(0x00, accent.R, accent.G, accent.B), 0);
            heroStack.Children.Add(subtitleLine);

            heroBorder.Child = heroStack;
            panel.Children.Add(heroBorder);

            // ── PARAGRAPH CARDS ──
            for (int p = 0; p < chapter.Paragraphs.Length; p++)
            {
                var para = chapter.Paragraphs[p];
                var lines = para.Split('\n');
                bool hasTitle = lines.Length > 1 && lines[0].Length < 60
                    && !lines[0].StartsWith("•") && !lines[0].StartsWith("1")
                    && !lines[0].StartsWith("✓");

                // Card with left accent bar (visually right in RTL)
                var cardOuter = new Grid { Margin = new Thickness(0, 0, 0, 14) };
                cardOuter.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                cardOuter.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });

                var cardBody = new Border
                {
                    CornerRadius = new CornerRadius(10, 0, 0, 10),
                    Padding = new Thickness(24, 18, 24, 18),
                    Background = new SolidColorBrush(
                        p % 2 == 0
                            ? Color.FromRgb(0x10, 0x15, 0x25)
                            : Color.FromRgb(0x13, 0x19, 0x2C)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x1C, 0x24, 0x3C)),
                    BorderThickness = new Thickness(1, 1, 0, 1)
                };
                Grid.SetColumn(cardBody, 0);

                // Accent bar on right side (RTL = visually right)
                var accentBar = new Border
                {
                    CornerRadius = new CornerRadius(0, 6, 6, 0),
                    Width = 4,
                };

                // Vary accent bar color per paragraph
                Color barColor;
                if (hasTitle) barColor = accent;
                else if (p == 0) barColor = HelpMint;
                else barColor = Color.FromArgb(0x60, accent.R, accent.G, accent.B);
                accentBar.Background = new SolidColorBrush(barColor);
                Grid.SetColumn(accentBar, 1);

                var paraStack = new StackPanel { FlowDirection = FlowDirection.RightToLeft };

                if (hasTitle)
                {
                    // Section title with icon
                    var titleRow = new StackPanel { Orientation = Orientation.Horizontal, FlowDirection = FlowDirection.RightToLeft };
                    titleRow.Children.Add(new TextBlock
                    {
                        Text = "\u25C6",
                        FontSize = 10,
                        Foreground = new SolidColorBrush(accent),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 0)
                    });
                    titleRow.Children.Add(new TextBlock
                    {
                        Text = " " + lines[0].TrimEnd(':'),
                        FontSize = 15,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(accent),
                        TextWrapping = TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    paraStack.Children.Add(titleRow);
                    paraStack.Children.Add(new Border
                    {
                        Height = 1,
                        Margin = new Thickness(0, 10, 0, 12),
                        Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x27, 0x3F)),
                    });

                    var rest = string.Join("\n", lines, 1, lines.Length - 1).TrimStart('\n');
                    HelpRenderFormattedText(paraStack, rest, accent);
                }
                else
                {
                    HelpRenderFormattedText(paraStack, para, accent);
                }

                cardBody.Child = paraStack;
                cardOuter.Children.Add(cardBody);
                cardOuter.Children.Add(accentBar);
                panel.Children.Add(cardOuter);
            }

            panel.Children.Add(new Border { Height = 30 });
        }

        // ── Render formatted Hebrew text with bullets, numbers, checks ──
        private static void HelpRenderFormattedText(StackPanel parent, string text, Color accent)
        {
            var accentBrush = new SolidColorBrush(accent);
            var lines = text.Split('\n');

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd();
                if (string.IsNullOrEmpty(line))
                {
                    parent.Children.Add(new Border { Height = 6 });
                    continue;
                }

                var tb = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 24,
                    FlowDirection = FlowDirection.RightToLeft
                };

                if (line.StartsWith("•"))
                {
                    // ── Bullet point ──
                    var bulletText = line.TrimStart('•').Trim();
                    tb.Margin = new Thickness(0, 3, 14, 3);

                    var dashIdx = bulletText.IndexOf("—");
                    if (dashIdx > 0 && dashIdx < 45)
                    {
                        var bold = bulletText.Substring(0, dashIdx).Trim();
                        var rest = bulletText.Substring(dashIdx);
                        tb.Inlines.Add(new System.Windows.Documents.Run("\u25B8 ") { Foreground = accentBrush, FontSize = 12 });
                        tb.Inlines.Add(new System.Windows.Documents.Run(bold + " ") { Foreground = TextBrush, FontWeight = FontWeights.SemiBold, FontSize = 13 });
                        tb.Inlines.Add(new System.Windows.Documents.Run(rest) { Foreground = Text2Brush, FontSize = 13 });
                    }
                    else
                    {
                        tb.Inlines.Add(new System.Windows.Documents.Run("\u25B8 ") { Foreground = accentBrush, FontSize = 12 });
                        tb.Inlines.Add(new System.Windows.Documents.Run(bulletText) { Foreground = TextBrush, FontSize = 13 });
                    }
                }
                else if (line.StartsWith("✓"))
                {
                    // ── Checkmark ──
                    tb.Margin = new Thickness(0, 3, 14, 3);
                    tb.Inlines.Add(new System.Windows.Documents.Run("\u2714 ") { Foreground = OkBrush, FontWeight = FontWeights.Bold, FontSize = 14 });
                    tb.Inlines.Add(new System.Windows.Documents.Run(line.TrimStart('✓').Trim()) { Foreground = TextBrush, FontSize = 13 });
                }
                else if (line.Length > 0 && char.IsDigit(line[0]) && line.Contains("."))
                {
                    // ── Numbered step ──
                    var dotIdx = line.IndexOf('.');
                    if (dotIdx > 0 && dotIdx <= 2)
                    {
                        var num = line.Substring(0, dotIdx);
                        var rest = line.Substring(dotIdx + 1).Trim();
                        tb.Margin = new Thickness(0, 5, 8, 5);

                        // Number circle
                        var numPanel = new StackPanel { Orientation = Orientation.Horizontal, FlowDirection = FlowDirection.RightToLeft, Margin = new Thickness(0, 4, 0, 4) };
                        var numBadge = new Border
                        {
                            Width = 24, Height = 24,
                            CornerRadius = new CornerRadius(12),
                            Background = new SolidColorBrush(Color.FromArgb(0x30, accent.R, accent.G, accent.B)),
                            Margin = new Thickness(0, 0, 0, 0),
                            Child = new TextBlock
                            {
                                Text = num,
                                FontSize = 12,
                                FontWeight = FontWeights.Bold,
                                Foreground = accentBrush,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };
                        numPanel.Children.Add(numBadge);

                        var dashIdx = rest.IndexOf("—");
                        var restTb = new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 10, 0),
                            FlowDirection = FlowDirection.RightToLeft
                        };
                        if (dashIdx > 0 && dashIdx < 45)
                        {
                            var bold = rest.Substring(0, dashIdx).Trim();
                            var after = rest.Substring(dashIdx);
                            restTb.Inlines.Add(new System.Windows.Documents.Run(bold + " ") { Foreground = TextBrush, FontWeight = FontWeights.SemiBold, FontSize = 13 });
                            restTb.Inlines.Add(new System.Windows.Documents.Run(after) { Foreground = Text2Brush, FontSize = 13 });
                        }
                        else
                        {
                            restTb.Inlines.Add(new System.Windows.Documents.Run(rest) { Foreground = TextBrush, FontSize = 13 });
                        }
                        numPanel.Children.Add(restTb);
                        parent.Children.Add(numPanel);
                        continue; // skip adding tb
                    }
                    else
                    {
                        tb.Text = line;
                        tb.Foreground = TextBrush;
                        tb.FontSize = 13;
                    }
                }
                else
                {
                    // ── Regular text ──
                    tb.Text = line;
                    tb.Foreground = new SolidColorBrush(Color.FromRgb(0xC8, 0xCF, 0xE5));
                    tb.FontSize = 13;
                    tb.Margin = new Thickness(0, 2, 0, 2);
                }

                parent.Children.Add(tb);
            }
        }

        // ── Update active nav state ──
        private static void HelpSetActiveNav(List<Border> navButtons, int activeIdx, int[] currentIndex,
            Border progressFill, double totalWidth, int totalChapters)
        {
            // Reset previous
            if (currentIndex[0] >= 0 && currentIndex[0] < navButtons.Count)
            {
                navButtons[currentIndex[0]].Background = Brushes.Transparent;
            }

            currentIndex[0] = activeIdx;

            if (activeIdx >= 0 && activeIdx < navButtons.Count)
            {
                // Active item: gradient background + left accent border
                navButtons[activeIdx].Background = new LinearGradientBrush(
                    Color.FromArgb(0x25, HelpAccent1.R, HelpAccent1.G, HelpAccent1.B),
                    Color.FromArgb(0x08, HelpAccent1.R, HelpAccent1.G, HelpAccent1.B), 0);

                // Update progress bar
                double pct = (double)(activeIdx + 1) / totalChapters;
                progressFill.Width = Math.Max(10, totalWidth * pct);
            }
        }

        // === Shared helpers for viewer windows ===

        private static Window CreateViewerWindow(string title, double width, double height, Brush borderBrush)
        {
            return new Window
            {
                Title = title,
                Width = width, Height = height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                Background = BgBrush,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                BorderThickness = new Thickness(0)
            };
        }

        private static Border CreateViewerBorder(Brush accentBrush)
        {
            return new Border
            {
                Background = BgBrush,
                BorderBrush = accentBrush,
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(12),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 30,
                    Opacity = 0.6,
                    ShadowDepth = 0
                }
            };
        }

        private static Border CreateTitleBar(string title, Brush accentBrush, Window win)
        {
            var titleBar = new Border
            {
                Background = SurfaceBrush,
                Padding = new Thickness(20, 12, 20, 12),
                CornerRadius = new CornerRadius(11, 11, 0, 0),
                BorderBrush = accentBrush,
                BorderThickness = new Thickness(0, 2, 0, 0)
            };
            var titlePanel = new DockPanel();
            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = TextBrush,
                VerticalAlignment = VerticalAlignment.Center
            };
            var closeBtn = new Button
            {
                Content = "\u2715",
                FontSize = 14,
                Foreground = Text2Brush,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            closeBtn.Click += (s, e) => win.Close();
            DockPanel.SetDock(closeBtn, Dock.Right);
            titlePanel.Children.Add(closeBtn);
            titlePanel.Children.Add(titleText);
            titleBar.Child = titlePanel;
            titleBar.MouseLeftButtonDown += (s, e) => win.DragMove();
            return titleBar;
        }

        private static Button MakeButton(string text, Brush accentBrush, bool isPrimary)
        {
            var btn = new Button
            {
                Content = text,
                MinWidth = 100,
                Height = 36,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(16, 0, 16, 0)
            };

            if (isPrimary && accentBrush != null)
            {
                btn.Foreground = Brushes.White;
                btn.Background = accentBrush;
                btn.BorderThickness = new Thickness(0);
            }
            else
            {
                btn.Foreground = TextBrush;
                btn.Background = SurfaceBrush;
                btn.BorderBrush = BorderBrush;
                btn.BorderThickness = new Thickness(1);
            }

            return btn;
        }

        /// <summary>
        /// Create a ScrollViewer with dark-themed thin scrollbar.
        /// Uses Loaded event to restyle the scrollbar after render.
        /// </summary>
        private static ScrollViewer CreateStyledScrollViewer()
        {
            var sv = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            };

            sv.Loaded += (s, e) =>
            {
                // Find the vertical scrollbar in the visual tree and restyle it
                var scrollBar = FindVisualChild<System.Windows.Controls.Primitives.ScrollBar>(sv);
                if (scrollBar == null) return;

                scrollBar.Width = 8;
                scrollBar.Background = new SolidColorBrush(Color.FromRgb(0x12, 0x17, 0x28));
                scrollBar.BorderThickness = new Thickness(0);
                scrollBar.Opacity = 0.7;

                // Restyle the thumb via resources on the scrollbar
                var thumbStyle = new Style(typeof(System.Windows.Controls.Primitives.Thumb));
                var thumbTemplate = new ControlTemplate(typeof(System.Windows.Controls.Primitives.Thumb));
                var thumbBorder = new FrameworkElementFactory(typeof(Border), "thumbBd");
                thumbBorder.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x38, 0x50, 0x78)));
                thumbBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
                thumbBorder.SetValue(Border.MarginProperty, new Thickness(1, 2, 1, 2));
                thumbTemplate.VisualTree = thumbBorder;

                var hoverTrigger = new Trigger
                {
                    Property = System.Windows.Controls.Primitives.Thumb.IsMouseOverProperty,
                    Value = true
                };
                hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty,
                    new SolidColorBrush(Color.FromRgb(0x58, 0x70, 0x98)), "thumbBd"));
                thumbTemplate.Triggers.Add(hoverTrigger);

                thumbStyle.Setters.Add(new Setter(
                    System.Windows.Controls.Primitives.Thumb.TemplateProperty, thumbTemplate));

                scrollBar.Resources[typeof(System.Windows.Controls.Primitives.Thumb)] = thumbStyle;
            };

            return sv;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found) return found;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
