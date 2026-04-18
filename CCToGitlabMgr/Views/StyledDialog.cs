using System;
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
    }
}
