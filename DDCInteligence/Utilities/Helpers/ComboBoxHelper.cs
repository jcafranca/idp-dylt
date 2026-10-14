using System.Windows;
using System.Windows.Controls;


namespace DDCInteligence.Utilities.Helpers
{
    public static class ComboBoxHelper
    {
        // CharacterCasing Attached Property
        public static readonly DependencyProperty CharacterCasingProperty =
            DependencyProperty.RegisterAttached("CharacterCasing", typeof(CharacterCasing), typeof(ComboBoxHelper),
                new PropertyMetadata(CharacterCasing.Normal, OnCharacterCasingChanged));

        // MaxLength Attached Property
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.RegisterAttached("MaxLength", typeof(int), typeof(ComboBoxHelper),
                new PropertyMetadata(0, OnMaxLengthChanged));

        public static CharacterCasing GetCharacterCasing(DependencyObject obj)
        {
            return (CharacterCasing)obj.GetValue(CharacterCasingProperty);
        }

        public static void SetCharacterCasing(DependencyObject obj, CharacterCasing value)
        {
            obj.SetValue(CharacterCasingProperty, value);
        }

        private static void OnCharacterCasingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ComboBox comboBox = obj as ComboBox;
            if (comboBox != null)
            {
                comboBox.Loaded += (s, args) =>
                {
                    TextBox textBox = (TextBox)comboBox.Template.FindName("PART_EditableTextBox", comboBox);
                    if (textBox != null)
                    {
                        textBox.CharacterCasing = (CharacterCasing)e.NewValue;
                    }
                };
            }
        }
        public static int GetMaxLength(DependencyObject obj)
        {
            return (int)obj.GetValue(MaxLengthProperty);
        }

        public static void SetMaxLength(DependencyObject obj, int value)
        {
            obj.SetValue(MaxLengthProperty, value);
        }

        private static void OnMaxLengthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is ComboBox comboBox)
            {
                comboBox.Loaded += (s, args) =>
                {
                    if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
                    {
                        textBox.MaxLength = (int)e.NewValue;
                    }
                };
            }
        }
    }
}
