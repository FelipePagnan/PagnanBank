using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BankingSystem.Desktop.Behaviors;

/// <summary>
/// Attached behaviors for TextBox. <c>DigitsOnly</c> restricts input to numeric
/// characters, blocking both typing and pasting of non-digits.
/// Combine with MaxLength to cap the number of digits (e.g. 11 for a CPF).
/// </summary>
public static class TextBoxBehaviors
{
    public static readonly DependencyProperty DigitsOnlyProperty =
        DependencyProperty.RegisterAttached(
            "DigitsOnly", typeof(bool), typeof(TextBoxBehaviors),
            new PropertyMetadata(false, OnDigitsOnlyChanged));

    public static bool GetDigitsOnly(DependencyObject obj) => (bool)obj.GetValue(DigitsOnlyProperty);
    public static void SetDigitsOnly(DependencyObject obj, bool value) => obj.SetValue(DigitsOnlyProperty, value);

    private static void OnDigitsOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
            return;

        if ((bool)e.NewValue)
        {
            textBox.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(textBox, OnPaste);
        }
        else
        {
            textBox.PreviewTextInput -= OnPreviewTextInput;
            DataObject.RemovePastingHandler(textBox, OnPaste);
        }
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        => e.Handled = !e.Text.All(char.IsDigit);

    private static void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            var text = (string)e.DataObject.GetData(DataFormats.Text)!;
            if (!text.All(char.IsDigit))
                e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }
}
