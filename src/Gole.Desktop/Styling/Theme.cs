using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

using Gole.Core;
using Gole.Infrastructure;
namespace Gole.Desktop
{
    static partial class Theme
    {
        public static Brush B(string color)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }
        public static Brush Ink = B("#18343D"), Muted = B("#6A8086"), Teal = B("#087F83");
        public static EmojiTextBlock EmojiText(string value, double size, Brush brush)
        {
            return new EmojiTextBlock { FontSize = size, Foreground = brush, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8), Text = value };
        }
        public static TextBlock Text(string value, double size, Brush brush)
        {
            return new TextBlock { Text = value, FontSize = size, Foreground = brush, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) };
        }
        public static Button Button(string value, Action action, bool primary)
        {
            var b = new Button { Content = value, Padding = new Thickness(15, 10, 15, 10), Margin = new Thickness(0, 0, 8, 8), Cursor = Cursors.Hand, Background = primary ? Teal : B("#EAF2F0"), Foreground = primary ? Brushes.White : Ink, BorderThickness = new Thickness(0), FontWeight = FontWeights.SemiBold, MinHeight = 38, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center };
            b.Click += delegate
            {
                action();
            };
            return b;
        }
        public static void Init(Application app)
        {
            app.Resources = CreateResources();
        }
        internal static ResourceDictionary CreateResources()
        {
            return (ResourceDictionary)XamlReader.Parse(@"<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
   <Style x:Key='KeyboardFocus' TargetType='Control'>
     <Setter Property='Template'>
       <Setter.Value>
         <ControlTemplate TargetType='Control'>
           <Rectangle Margin='4' RadiusX='6' RadiusY='6' Stroke='#6E8991' StrokeThickness='1' StrokeDashArray='2 2' SnapsToDevicePixels='True'/>
         </ControlTemplate>
       </Setter.Value>
     </Setter>
   </Style>
   <Style TargetType='Button'><Setter Property='FocusVisualStyle' Value='{StaticResource KeyboardFocus}'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Button'><Border x:Name='b' CornerRadius='10' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' Padding='{TemplateBinding Padding}'><ContentPresenter HorizontalAlignment='{TemplateBinding HorizontalContentAlignment}' VerticalAlignment='{TemplateBinding VerticalContentAlignment}'/></Border><ControlTemplate.Triggers><Trigger Property='IsMouseOver' Value='True'><Setter TargetName='b' Property='Opacity' Value='.82'/></Trigger><Trigger Property='IsPressed' Value='True'><Setter TargetName='b' Property='Opacity' Value='.65'/></Trigger><Trigger Property='IsEnabled' Value='False'><Setter TargetName='b' Property='Opacity' Value='.4'/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
   <Style TargetType='TextBox'><Setter Property='Padding' Value='10,7'/><Setter Property='BorderBrush' Value='#CBDBD7'/><Setter Property='BorderThickness' Value='1'/><Setter Property='Background' Value='White'/><Setter Property='FontSize' Value='14'/></Style>
   <Style TargetType='ComboBox'>
    <Setter Property='Padding' Value='9,7'/><Setter Property='FontSize' Value='14'/><Setter Property='MinHeight' Value='36'/>
    <Setter Property='Background' Value='White'/><Setter Property='Foreground' Value='#18343D'/>
    <Setter Property='Template'><Setter.Value><ControlTemplate TargetType='ComboBox'>
      <Grid><ToggleButton Focusable='False' IsChecked='{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}' Background='{TemplateBinding Background}'>
        <ToggleButton.Template><ControlTemplate TargetType='ToggleButton'><Border Background='{TemplateBinding Background}' BorderBrush='#6A8086' BorderThickness='1' CornerRadius='6'/></ControlTemplate></ToggleButton.Template>
      </ToggleButton>
      <ContentPresenter Margin='10,5,27,5' VerticalAlignment='Center' IsHitTestVisible='False' Content='{TemplateBinding SelectionBoxItem}' ContentTemplate='{TemplateBinding SelectionBoxItemTemplate}' ContentTemplateSelector='{TemplateBinding ItemTemplateSelector}' ContentStringFormat='{TemplateBinding SelectionBoxItemStringFormat}'/>
      <TextBlock Text='▾' Foreground='{TemplateBinding Foreground}' HorizontalAlignment='Right' VerticalAlignment='Center' Margin='0,0,10,0' IsHitTestVisible='False'/>
      <Popup x:Name='PART_Popup' IsOpen='{TemplateBinding IsDropDownOpen}' Placement='Bottom' AllowsTransparency='True' Focusable='False'>
        <Border Background='{TemplateBinding Background}' BorderBrush='#6A8086' BorderThickness='1' MinWidth='{Binding ActualWidth, RelativeSource={RelativeSource TemplatedParent}}'>
          <ScrollViewer MaxHeight='300'><ItemsPresenter KeyboardNavigation.DirectionalNavigation='Contained'/></ScrollViewer>
        </Border>
      </Popup></Grid>
    </ControlTemplate></Setter.Value></Setter>
   </Style>
   <Style TargetType='ComboBoxItem'><Setter Property='Padding' Value='10,7'/><Setter Property='Foreground' Value='{Binding Foreground, RelativeSource={RelativeSource AncestorType=ComboBox}}'/><Setter Property='Background' Value='Transparent'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='ComboBoxItem'><Border Background='{TemplateBinding Background}' Padding='{TemplateBinding Padding}'><ContentPresenter/></Border><ControlTemplate.Triggers><Trigger Property='IsHighlighted' Value='True'><Setter Property='Background' Value='#087F83'/><Setter Property='Foreground' Value='White'/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>
   <Style TargetType='CheckBox'><Setter Property='Margin' Value='0,7,0,7'/><Setter Property='FontSize' Value='14'/><Setter Property='Foreground' Value='#18343D'/></Style>
   <Style TargetType='TabItem'><Setter Property='Padding' Value='14,10'/><Setter Property='FontSize' Value='14'/></Style>
   </ResourceDictionary>");
        }
    }
}
