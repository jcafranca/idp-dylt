// Modified GlobalShortcut class with selective disable support
using HandyControl.Data;
using HandyControl.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using DDCInteligence.View;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DDCInteligence.Utilities.Helpers
{
    // Custom KeyBinding class that supports selective disabling
    public class SelectiveKeyBinding : KeyBinding
    {
        public bool CheckDisabledKeys { get; set; } = false;
        public Func<Key, bool> IsKeyDisabled { get; set; }
    }

    public class GlobalShortcut
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        internal static object BooleanBox(bool value) => value ? true : false;
        private static ObservableCollection<KeyBinding> KeyBindingCollection;
        private static readonly Dictionary<string, KeyBinding> CommandDic = new Dictionary<string, KeyBinding>();

        private static void KeyboardHook_KeyDown(object sender, KeyboardHookEventArgs e) => HitTest(e.Key);
        static GlobalShortcut() => KeyboardHook.KeyDown += KeyboardHook_KeyDown;

        private static void HitTest(Key key)
        {
            if (IsModifierKey(key)) return;
            var modifierKeys = Keyboard.Modifiers;
            var keyStr = modifierKeys != ModifierKeys.None ? $"{modifierKeys.ToString()}, {key.ToString()}" : key.ToString();
            ExecuteCommand(keyStr, key);
        }

        /// <summary>
        /// Execute command with selective disabled key check
        /// </summary>
        /// <param name="key"></param>
        /// <param name="originalKey"></param>
        private static void ExecuteCommand(string key, Key originalKey)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (CommandDic.ContainsKey(key))
            {
                var keyBinding = CommandDic[key];
                var command = keyBinding.Command;

                // Check if this is a SelectiveKeyBinding and if key checking is enabled
                if (keyBinding is SelectiveKeyBinding selectiveBinding &&
                    selectiveBinding.CheckDisabledKeys &&
                    selectiveBinding.IsKeyDisabled != null)
                {
                    if (selectiveBinding.IsKeyDisabled(originalKey))
                        return; // Key is disabled for this specific binding
                }

                ReferencesView openWindow = Application.Current.Windows.OfType<ReferencesView>().FirstOrDefault();
                var mainWindow = ApplicationIsActivated();

                if (openWindow != null && (keyBinding.Key == Key.F2 || keyBinding.Key == Key.Escape)) return;
                if (mainWindow)
                    if (command != null && command.CanExecute(keyBinding.CommandParameter))
                        command.Execute(keyBinding.CommandParameter);
            }
        }

        /// <summary>
        /// Returns true if the current application has focus, false otherwise
        /// </summary>
        /// <returns></returns>
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
                return false;       // No window is currently activated

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);
            return activeProcId == procId;
        }

        private static bool IsModifierKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.LeftAlt || key == Key.LeftShift || key == Key.LWin ||
                   key == Key.RightCtrl || key == Key.RightAlt || key == Key.RightShift || key == Key.RWin;
        }

        /// <summary>
        /// Initialize key binding list
        /// </summary>
        /// <param name="list"></param>
        public static void Init(List<KeyBinding> list)
        {
            CommandDic.Clear();

            if (list == null) return;
            KeyBindingCollection = new ObservableCollection<KeyBinding>(list);
            if (KeyBindingCollection.Count == 0) return;

            AddKeyBindings(KeyBindingCollection);
            KeyboardHook.Start();
        }

        private static void AddKeyBindings(IEnumerable<KeyBinding> keyBindings)
        {
            foreach (var item in keyBindings)
            {
                if (item.Key == Key.None) continue;

                if (item.Modifiers == ModifierKeys.None)
                {
                    CommandDic[item.Key.ToString()] = item;
                }
                else
                {
                    var keyStr = $"{item.Modifiers.ToString()}, {item.Key.ToString()}";
                    CommandDic[keyStr] = item;
                }
            }
        }

        public static readonly DependencyProperty HostProperty = DependencyProperty.RegisterAttached(
            "Host", typeof(bool), typeof(GlobalShortcut), new PropertyMetadata(false, OnHostChanged));

        private static void OnHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerHelper.IsInDesignMode) return;
            if (KeyBindingCollection != null)
                KeyBindingCollection.CollectionChanged -= KeyBindingCollection_CollectionChanged;

            KeyBindingCollection = GetKeyBindings(d);
            if (KeyBindingCollection != null)
                KeyBindingCollection.CollectionChanged += KeyBindingCollection_CollectionChanged;
        }

        private static void KeyBindingCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AddKeyBindings(KeyBindingCollection);
            KeyboardHook.Start();
        }

        public static void SetHost(DependencyObject element, bool value) => element.SetValue(HostProperty, BooleanBox(value));
        public static bool GetHost(DependencyObject element) => (bool)element.GetValue(HostProperty);
        public static readonly DependencyProperty KeyBindingsProperty = DependencyProperty.RegisterAttached(
            "KeyBindings", typeof(ObservableCollection<KeyBinding>), typeof(GlobalShortcut), new PropertyMetadata(new ObservableCollection<KeyBinding>()));
        public static void SetKeyBindings(DependencyObject element, ObservableCollection<KeyBinding> value) => element.SetValue(KeyBindingsProperty, value);
        public static ObservableCollection<KeyBinding> GetKeyBindings(DependencyObject element) => (ObservableCollection<KeyBinding>)element.GetValue(KeyBindingsProperty);
    }
}