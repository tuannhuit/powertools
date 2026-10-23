using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace PowerTools.Core.Behaviours
{
    public static class RichTextBoxBehavior
    {
        private static bool _isInternalChange = false;
        private static DispatcherTimer _debounceTimer;

        public static readonly DependencyProperty HighlighterTargetProperty = DependencyProperty.RegisterAttached(
            "HighlighterTarget",
            typeof(RichTextBox),
            typeof(RichTextBoxBehavior),
            new PropertyMetadata(null, OnHighlighterTargetChanged));

        public static RichTextBox GetHighlighterTarget(DependencyObject obj) => (RichTextBox)obj.GetValue(HighlighterTargetProperty);
        public static void SetHighlighterTarget(DependencyObject obj, RichTextBox value) => obj.SetValue(HighlighterTargetProperty, value);

        
        public static readonly DependencyProperty ActiveTokensProperty = DependencyProperty.RegisterAttached(
            "ActiveTokens",
            typeof(IEnumerable<string>),
            typeof(RichTextBoxBehavior),
            new PropertyMetadata(null, OnActiveTokensChanged));

        public static IEnumerable<string> GetActiveTokens(DependencyObject obj) => (IEnumerable<string>)obj.GetValue(ActiveTokensProperty);
        public static void SetActiveTokens(DependencyObject obj, IEnumerable<string> value) => obj.SetValue(ActiveTokensProperty, value);

        private static void OnHighlighterTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.TextChanged -= TextBox_TextChanged;
                textBox.Loaded -= TextBox_Loaded;

                if (e.NewValue is RichTextBox rtb)
                {
                    textBox.TextChanged += TextBox_TextChanged;
                    textBox.Loaded += TextBox_Loaded;

                    if (textBox.IsLoaded)
                    {
                        InitializeAndFormat(textBox, rtb);
                    }
                }
            }
        }

        private static void OnActiveTokensChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox && GetHighlighterTarget(textBox) is RichTextBox rtb)
            {
                InitializeAndFormat(textBox, rtb);
            }
        }

        private static void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && GetHighlighterTarget(textBox) is RichTextBox rtb)
            {
                InitializeAndFormat(textBox, rtb);
            }
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInternalChange) return;

            if (sender is TextBox textBox && GetHighlighterTarget(textBox) is RichTextBox rtb)
            {
                _isInternalChange = true;

                // Fast Sync: Instantly reflect structural line elements into the document
                var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                string targetText = textBox.Text ?? string.Empty;

                rtb.Document.Blocks.Clear();
                var p = new Paragraph { Margin = new Thickness(0) };

                string[] lines = targetText.Replace("\r\n", "\n").Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        p.Inlines.Add(new Run(lines[i])
                        {
                            FontFamily = textBox.FontFamily,
                            FontSize = textBox.FontSize
                        });
                    }
                    if (i < lines.Length - 1) p.Inlines.Add(new LineBreak());
                }
                rtb.Document.Blocks.Add(p);

                _isInternalChange = false;

                // HIGH-PERFORMANCE DEBOUNCE: Stop mid-keystroke scanning passes
                if (_debounceTimer == null)
                {
                    _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
                    _debounceTimer.Tick += (s, args) =>
                    {
                        _debounceTimer.Stop();
                        ApplyTokenFormatting(rtb, GetActiveTokens(textBox), textBox.FontFamily, textBox.FontSize);
                    };
                }

                _debounceTimer.Stop();
                _debounceTimer.Start();
            }
        }

        private static void InitializeAndFormat(TextBox textBox, RichTextBox rtb)
        {
            _isInternalChange = true;

            var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            string targetText = textBox.Text ?? string.Empty;

            rtb.Document.Blocks.Clear();
            var p = new Paragraph { Margin = new Thickness(0) };

            string[] lines = targetText.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                {
                    var run = new Run(lines[i])
                    {
                        FontFamily = textBox.FontFamily,
                        FontSize = textBox.FontSize
                    };
                    p.Inlines.Add(run);
                }
                if (i < lines.Length - 1) p.Inlines.Add(new LineBreak());
            }
            rtb.Document.Blocks.Add(p);

            ApplyTokenFormatting(rtb, GetActiveTokens(textBox), textBox.FontFamily, textBox.FontSize);

            _isInternalChange = false;
        }

        private static void ApplyTokenFormatting(RichTextBox rtb, IEnumerable<string> tokens, FontFamily editorFont, double editorSize)
        {
            var allowedTokens = tokens?.ToList() ?? new List<string>();
            if (!allowedTokens.Any()) return;

            // BATCH OPTIMIZATION 1: Clear everything globally in one single operation 
            // instead of resetting properties iteratively run-by-run.
            var fullRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            fullRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
            //fullRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            fullRange.ApplyPropertyValue(TextElement.FontFamilyProperty, editorFont);
            fullRange.ApplyPropertyValue(TextElement.FontSizeProperty, editorSize);

            // BATCH OPTIMIZATION 2: Fast forward scan loop
            TextPointer position = rtb.Document.ContentStart;
            while (position != null && position.CompareTo(rtb.Document.ContentEnd) < 0)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);
                    int earliestIndex = int.MaxValue;
                    string matchedToken = null;

                    foreach (var token in allowedTokens)
                    {
                        int index = textRun.IndexOf(token, StringComparison.Ordinal);
                        if (index >= 0 && index < earliestIndex)
                        {
                            earliestIndex = index;
                            matchedToken = token;
                        }
                    }

                    if (matchedToken != null)
                    {
                        TextPointer startMatch = position.GetPositionAtOffset(earliestIndex, LogicalDirection.Forward);
                        TextPointer endMatch = startMatch.GetPositionAtOffset(matchedToken.Length, LogicalDirection.Forward);

                        var tokenRange = new TextRange(startMatch, endMatch);
                        tokenRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8C9AB")));
                        //tokenRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.SemiBold);
                        tokenRange.ApplyPropertyValue(TextElement.FontFamilyProperty, editorFont);
                        tokenRange.ApplyPropertyValue(TextElement.FontSizeProperty, editorSize);

                        position = endMatch;
                        continue;
                    }
                }
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
        }
    }
}