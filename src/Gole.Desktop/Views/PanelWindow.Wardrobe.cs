using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using Gole.Core;

namespace Gole.Desktop;

public partial class PanelWindow
{
    ComboBox accessoryCategory;
    WrapPanel lookCards;
    StackPanel lookForm;
    SavedLook editingLook;
    TextBox lookName, accessorySearch;
    TextBlock wardrobeMessage, filterMessage;
    readonly Random wardrobeRandom = new();

    void AddWardrobe(StackPanel controls)
    {
        Label(controls, "Meus looks");
        controls.Children.Add(Theme.Button("Criar look", () => BeginLook(null), true));
        lookForm = new StackPanel { Visibility = Visibility.Collapsed };
        lookForm.Children.Add(Theme.Text("Escolha as roupas abaixo, confira a prévia e dê um nome ao conjunto.", 12, Theme.Muted));
        lookName = new TextBox { MaxLength = 32 };
        AutomationProperties.SetName(lookName, "Nome do look");
        lookForm.Children.Add(Theme.Text("Nome do look", 12, Theme.Muted));
        lookForm.Children.Add(lookName);
        var actions = new WrapPanel();
        actions.Children.Add(Theme.Button("Salvar look", SaveLook, true));
        actions.Children.Add(Theme.Button("Cancelar look", () => { lookForm.Visibility = Visibility.Collapsed; editingLook = null; ReadDraft(); SyncControls(); UpdateEditor(); }, false));
        lookForm.Children.Add(actions);
        controls.Children.Add(lookForm);
        wardrobeMessage = Theme.Text("", 12, Theme.Muted);
        controls.Children.Add(wardrobeMessage);
        lookCards = new WrapPanel();
        controls.Children.Add(new ScrollViewer { Content = lookCards, MaxHeight = 280, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        RefreshLooks();
        controls.Children.Add(Theme.Button("Monte um look", () =>
        {
            var look = Wardrobe.Suggest(wardrobeRandom, draftModel, draftAccessories);
            draftColor = look.Color;
            draftAccessories = look.Accessories;
            UpdateEditor();
            wardrobeMessage.Text = "Sugestão na prévia. Crie um look para guardar ou salve a personalização para vestir.";
        }, false));
    }
    void BeginLook(SavedLook look)
    {
        editingLook = look;
        lookName.Text = look?.Name ?? "";
        if (look != null)
        {
            draftModel = look.Vessel;
            draftColor = look.Color;
            draftAccessories = look.Accessories.ToList();
            UpdateEditor();
        }
        lookForm.Visibility = Visibility.Visible;
        wardrobeMessage.Text = look == null ? "Criando um novo look a partir da prévia." : "Editando " + look.Name;
        lookName.Focus();
    }
    void SaveLook()
    {
        string name = lookName.Text.Trim();
        if (name.Length == 0)
        {
            wardrobeMessage.Text = "Dê um nome ao look.";
            return;
        }
        if (data.Looks.Any(l => l != editingLook && string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            wardrobeMessage.Text = "Já existe um look com esse nome. Escolha outro.";
            return;
        }
        if (editingLook == null && data.Looks.Count >= Wardrobe.MaximumLooks)
        {
            wardrobeMessage.Text = "Limite de 30 looks. Exclua um para criar outro.";
            return;
        }
        var next = new SavedLook { Name = name, Color = draftColor, Vessel = draftModel, Accessories = draftAccessories.ToList() };
        int index = editingLook == null ? data.Looks.Count : data.Looks.IndexOf(editingLook);
        var previous = editingLook;
        if (previous == null)
            data.Looks.Add(next);
        else
            data.Looks[index] = next;
        if (!pet.Store.Save(data))
        {
            if (previous == null)
                data.Looks.Remove(next);
            else
                data.Looks[index] = previous;
            wardrobeMessage.Text = "Não foi possível salvar o look. Tente novamente.";
            return;
        }
        editingLook = null;
        lookForm.Visibility = Visibility.Collapsed;
        RefreshLooks();
        wardrobeMessage.Text = "Look salvo. Clique na foto para vestir.";
    }
    void RefreshLooks()
    {
        lookCards.Children.Clear();
        foreach (var look in data.Looks)
        {
            var content = new StackPanel();
            content.Children.Add(new Bottle
            {
                Width = 120,
                Height = 132,
                Vessel = look.Vessel,
                Tint = look.Color,
                EquippedAccessories = look.Accessories.ToArray(),
                MotionEnabled = false,
                Tag = "Look thumbnail"
            });
            var label = Theme.Text(look.Name, 13, Theme.Ink);
            label.TextAlignment = TextAlignment.Center;
            content.Children.Add(label);
            var use = Theme.Button("", () => ApplyLook(look), false);
            use.Content = content;
            use.Padding = new Thickness(6);
            use.Width = 144;
            AutomationProperties.SetName(use, "Vestir look: " + look.Name);
            var card = new StackPanel { Width = 152, Margin = new Thickness(0, 0, 8, 12) };
            card.Children.Add(use);
            card.Children.Add(Theme.Button("Editar", () => BeginLook(look), false));
            var delete = Theme.Button("Excluir", () =>
            {
                int index = data.Looks.IndexOf(look);
                data.Looks.Remove(look);
                if (!pet.Store.Save(data))
                {
                    data.Looks.Insert(index, look);
                    wardrobeMessage.Text = "Não foi possível excluir.";
                    return;
                }
                if (editingLook == look)
                {
                    editingLook = null;
                    lookForm.Visibility = Visibility.Collapsed;
                }
                RefreshLooks();
                wardrobeMessage.Text = "Look excluído.";
            }, false);
            AutomationProperties.SetName(delete, "Excluir look: " + look.Name);
            AutomationProperties.SetName((Button)card.Children[1], "Editar look: " + look.Name);
            card.Children.Add(delete);
            lookCards.Children.Add(card);
        }
        Theme.ApplyNight(this, data.NightMode);
    }
    void ApplyLook(SavedLook look)
    {
        // A look changes only appearance, never unsaved name/personality/settings.
        var oldModel = data.Vessel;
        var oldColor = data.Color;
        var oldItems = data.Accessories;
        data.Vessel = look.Vessel;
        data.Color = look.Color;
        data.Accessories = look.Accessories.ToList();
        if (!pet.Store.Save(data))
        {
            data.Vessel = oldModel;
            data.Color = oldColor;
            data.Accessories = oldItems;
            wardrobeMessage.Text = "Não foi possível vestir o look.";
            return;
        }
        draftModel = look.Vessel;
        draftColor = look.Color;
        draftAccessories = look.Accessories.ToList();
        pet.Apply();
        UpdateEditor();
        wardrobeMessage.Text = "Vestindo " + look.Name;
    }
    void AddAccessoryFilters(StackPanel controls)
    {
        accessorySearch = new TextBox();
        AutomationProperties.SetName(accessorySearch, "Buscar acessórios");
        controls.Children.Add(Theme.Text("Buscar acessórios", 12, Theme.Muted));
        controls.Children.Add(accessorySearch);
        accessoryCategory = new ComboBox { ItemsSource = Wardrobe.Categories, SelectedIndex = 0, Height = 36, Margin = new Thickness(0, 6, 0, 6) };
        AutomationProperties.SetName(accessoryCategory, "Categoria de acessórios");
        controls.Children.Add(accessoryCategory);
        filterMessage = Theme.Text("", 12, Theme.Muted);
        controls.Children.Add(filterMessage);
        accessorySearch.TextChanged += (_, _) => FilterAccessories();
        accessoryCategory.SelectionChanged += (_, _) => FilterAccessories();
    }
    void FilterAccessories()
    {
        int count = 0;
        for (int i = 0; i < accessoryButtons.Count; i++)
        {
            bool match = Wardrobe.Matches(i, accessorySearch.Text, accessoryCategory.SelectedItem as string ?? "Todos");
            accessoryButtons[i].Visibility = match ? Visibility.Visible : Visibility.Collapsed;
            if (i > 0 && match)
                count++;
        }
        filterMessage.Text = count == 0 ? "Nenhum acessório encontrado. Tente outro nome ou categoria." : count + " acessórios encontrados";
    }
}
