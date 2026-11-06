using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using Gole.Core;

namespace Gole.Desktop;

public partial class PanelWindow
{
    ComboBox savedLooks, accessoryCategory;
    TextBox lookName, accessorySearch;
    TextBlock wardrobeMessage, filterMessage;
    readonly Random wardrobeRandom = new();

    void AddWardrobe(StackPanel controls)
    {
        Label(controls, "Meus looks");
        savedLooks = new ComboBox { DisplayMemberPath = "Name", Height = 36, ItemsSource = data.Looks };
        AutomationProperties.SetName(savedLooks, "Looks salvos");
        savedLooks.SelectionChanged += (_, _) => { if (savedLooks.SelectedItem is SavedLook look) lookName.Text = look.Name; };
        controls.Children.Add(savedLooks);
        lookName = new TextBox { MaxLength = 32, Margin = new Thickness(0, 6, 0, 6) };
        AutomationProperties.SetName(lookName, "Nome do look");
        controls.Children.Add(Theme.Text("Nome do look", 12, Theme.Muted));
        controls.Children.Add(lookName);
        var actions = new WrapPanel();
        actions.Children.Add(Theme.Button("Guardar novo look", () =>
        {
            if (string.IsNullOrWhiteSpace(lookName.Text))
            {
                wardrobeMessage.Text = "Dê um nome para seu look.";
                return;
            }
            if (data.Looks.Count >= Wardrobe.MaximumLooks)
            {
                wardrobeMessage.Text = "Você tem 30 looks. Exclua um para guardar outro.";
                return;
            }
            var look = new SavedLook { Name = lookName.Text.Trim(), Vessel = draftModel, Color = draftColor, Accessories = draftAccessories.ToList() };
            data.Looks.Add(look);
            if (!pet.Store.Save(data))
            {
                data.Looks.Remove(look);
                wardrobeMessage.Text = "Não foi possível guardar. Tente novamente.";
                return;
            }
            RefreshLooks(look);
            wardrobeMessage.Text = "Look guardado. O personagem continua com o visual aplicado.";
        }, false));
        actions.Children.Add(Theme.Button("Usar look", () =>
        {
            if (savedLooks.SelectedItem is not SavedLook look)
            {
                wardrobeMessage.Text = "Selecione um look.";
                return;
            }
            draftModel = look.Vessel;
            draftColor = look.Color;
            draftAccessories = look.Accessories.ToList();
            SaveAppearance();
            wardrobeMessage.Text = "Look aplicado.";
        }, false));
        actions.Children.Add(Theme.Button("Renomear look", () =>
        {
            if (savedLooks.SelectedItem is not SavedLook look || string.IsNullOrWhiteSpace(lookName.Text))
            {
                wardrobeMessage.Text = "Selecione um look e informe o nome.";
                return;
            }
            string old = look.Name;
            look.Name = lookName.Text.Trim();
            if (!pet.Store.Save(data))
            {
                look.Name = old;
                wardrobeMessage.Text = "Não foi possível renomear.";
                return;
            }
            RefreshLooks(look);
            wardrobeMessage.Text = "Nome atualizado.";
        }, false));
        actions.Children.Add(Theme.Button("Excluir look", () =>
        {
            if (savedLooks.SelectedItem is not SavedLook look)
                return;
            int index = data.Looks.IndexOf(look);
            data.Looks.Remove(look);
            if (!pet.Store.Save(data))
            {
                data.Looks.Insert(index, look);
                wardrobeMessage.Text = "Não foi possível excluir.";
                return;
            }
            RefreshLooks(null);
            wardrobeMessage.Text = "Look excluído. O visual do personagem foi mantido.";
        }, false));
        actions.Children.Add(Theme.Button("Monte um look", () =>
        {
            var look = Wardrobe.Suggest(wardrobeRandom, draftModel, draftAccessories);
            draftColor = look.Color;
            draftAccessories = look.Accessories;
            UpdateEditor();
            wardrobeMessage.Text = "Sugestão na prévia. Salve a personalização para vestir ou guarde como novo look.";
        }, false));
        controls.Children.Add(actions);
        wardrobeMessage = Theme.Text("Guarde o visual da prévia e use seus conjuntos com um clique em Usar look.", 12, Theme.Muted);
        controls.Children.Add(wardrobeMessage);
    }
    void RefreshLooks(SavedLook selected)
    {
        savedLooks.ItemsSource = null;
        savedLooks.ItemsSource = data.Looks;
        savedLooks.SelectedItem = selected;
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
