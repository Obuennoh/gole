using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Gole.Core;

namespace Gole.Desktop;

internal sealed class DrinkWindow : Window
{
    internal TextBox AmountBox
    {
        get;
    }
    internal Button ConfirmButton
    {
        get;
    }
    internal Bottle Preview
    {
        get;
    }
    internal int AmountMl
    {
        get; private set;
    }

    internal DrinkWindow(HydrationState state)
    {
        Title = "Registrar água";
        Width = 420;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Theme.B("#F5F8FA");
        FontFamily = new FontFamily("Segoe UI");
        var content = new StackPanel { Margin = new Thickness(24) };
        content.Children.Add(Theme.Text("Quanto você bebeu?", 22, Theme.Ink));
        var presets = new UniformGrid { Columns = 2, Margin = new Thickness(0, 12, 0, 8) };
        AmountBox = new TextBox { Text = state.LastAmountMl.ToString(), MaxLength = 4, Width = 100, Margin = new Thickness(0, 0, 10, 0) };
        System.Windows.Automation.AutomationProperties.SetName(AmountBox, "Quantidade de água em mililitros");
        foreach (int value in new[] { HydrationState.SmallSipMl, 100, HydrationState.RegistrationMl, 500 })
        {
            int ml = value;
            string label = ml == HydrationState.SmallSipMl ? "1 gole · ~25 ml" : ml + " ml";
            presets.Children.Add(Theme.Button(label, () => AmountBox.Text = ml.ToString(), false));
        }
        content.Children.Add(presets);
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(AmountBox);
        row.Children.Add(Theme.Text("ml", 14, Theme.Muted));
        content.Children.Add(row);
        Preview = new Bottle { Width = 108, Height = 125, Tint = state.Color, Vessel = state.Vessel, EquippedAccessories = state.Accessories.ToArray() };
        content.Children.Add(Preview);
        var levelLabel = Theme.Text("", 13, Theme.Ink);
        levelLabel.TextAlignment = TextAlignment.Center;
        content.Children.Add(levelLabel);
        var hint = Theme.Text(HydrationState.RegistrationMl + " ml repõem 100% do pet. Máximo: 100%.", 12, Theme.Muted);
        hint.TextAlignment = TextAlignment.Center;
        content.Children.Add(hint);
        ConfirmButton = Theme.Button("Registrar água", () => { DialogResult = true; }, true);
        ConfirmButton.IsDefault = true;
        content.Children.Add(ConfirmButton);
        var cancel = Theme.Button("Cancelar", () => { DialogResult = false; }, false);
        cancel.IsCancel = true;
        content.Children.Add(cancel);
        Content = content;
        void Refresh()
        {
            bool valid = int.TryParse(AmountBox.Text, out int amount) && amount is >= HydrationState.MinimumDrinkMl and <= HydrationState.MaximumDrinkMl;
            AmountMl = valid ? amount : 0;
            ConfirmButton.IsEnabled = valid;
            Preview.Level = valid ? state.LevelAfterDrink(DateTime.UtcNow, amount) : state.Level(DateTime.UtcNow);
            Preview.InvalidateVisual();
            levelLabel.Text = valid ? "Após registrar: " + Math.Round(Preview.Level * 100) + "%" : "Informe entre 25 e 1.000 ml.";
        }
        AmountBox.TextChanged += (_, _) => Refresh();
        Loaded += (_, _) => { AmountBox.Focus(); AmountBox.SelectAll(); Refresh(); };
        Refresh();
    }
}
