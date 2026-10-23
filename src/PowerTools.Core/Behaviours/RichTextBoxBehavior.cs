using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PowerTools.Core.Behaviours
{
    public static class RichTextBoxBehavior
    {
        private static bool _isInternalChange = false;

        // 1. Links the editable front TextBox to the background coloring panel
        public static readonly DependencyProperty HighlighterTargetProperty =
            DependencyProperty.RegisterAttached("HighlighterTarget", typeof(RichTextBox), typeof(RichTextBoxBehavior),
                new PropertyMetadata(null, OnHighlighterTargetChanged));

        public static RichTextBox GetHighlighterTarget(DependencyObject obj) => (RichTextBox)obj.GetValue(HighlighterTargetProperty);
        public static void SetHighlighterTarget(DependencyObject obj, RichTextBox value) => obj.SetValue(HighlighterTargetProperty, value);

        // 2. The dynamic list of tokens that get the gray background and bold text
        public static readonly DependencyProperty ActiveTokensProperty =
            DependencyProperty.RegisterAttached("ActiveTokens", typeof(IEnumerable<string>), typeof(RichTextBoxBehavior),
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
                InitializeAndFormat(textBox, rtb);
            }
        }

        private static void InitializeAndFormat(TextBox textBox, RichTextBox rtb)
        {
            _isInternalChange = true;

            var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            string targetText = textBox.Text ?? string.Empty;

            string currentRtbText = textRange.Text;
            if (currentRtbText.EndsWith(Environment.NewLine))
                currentRtbText = currentRtbText.Substring(0, currentRtbText.Length - Environment.NewLine.Length);

            if (currentRtbText != targetText)
            {
                rtb.Document.Blocks.Clear();
                var p = new Paragraph { Margin = new Thickness(0) };

                string[] lines = targetText.Replace("\r\n", "\n").Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        // CRITICAL FIX: Explicitly assign the layout font metrics directly to the
                        // Run containers as they are generated so it stays Consolas immediately on load.
                        var run = new Run(lines[i])
                        {
                            FontFamily = textBox.FontFamily,
                            FontSize = textBox.FontSize,
                            //Foreground = Brushes.Black
                        };
                        p.Inlines.Add(run);
                    }
                    if (i < lines.Length - 1) p.Inlines.Add(new LineBreak());
                }
                rtb.Document.Blocks.Add(p);
            }

            ApplyTokenFormatting(rtb, GetActiveTokens(textBox), textBox.FontFamily, textBox.FontSize);

            _isInternalChange = false;
        }

        private static void ApplyTokenFormatting(RichTextBox rtb, IEnumerable<string> tokens, FontFamily editorFont, double editorSize)
        {
            var allowedTokens = tokens?.ToList() ?? new List<string>();

            TextPointer position = rtb.Document.ContentStart;
            while (position != null && position.CompareTo(rtb.Document.ContentEnd) < 0)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    TextPointer endOfRun = position.GetPositionAtOffset(textRun.Length, LogicalDirection.Forward);
                    var cleanRange = new TextRange(position, endOfRun);
                    cleanRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                    //cleanRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                    cleanRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

                    // CRITICAL FIX: Force standard plain text segments to retain Consolas font families
                    cleanRange.ApplyPropertyValue(TextElement.FontFamilyProperty, editorFont);
                    cleanRange.ApplyPropertyValue(TextElement.FontSizeProperty, editorSize);

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

                        tokenRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E4E4E4")));
                        //tokenRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")));
                        tokenRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.SemiBold);

                        // CRITICAL FIX: Retain exact font metrics on styled spans so text alignment matches perfectly
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