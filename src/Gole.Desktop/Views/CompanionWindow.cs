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
using Gole.Desktop.Updates;
using Gole.Desktop.Audio;
namespace Gole.Desktop
{
    public class CompanionWindow : Window
    {
        public HydrationState Data
        {
            get
            {
                return session.State;
            }
        }
        public JsonStateStore Store
        {
            get; private set;
        }
        readonly HydrationSession session;
        readonly IWaterSound waterSound;
        internal DesktopUpdateService Updates
        {
            get;
        }
        public Bottle Art;
        Forms.NotifyIcon tray;
        DispatcherTimer clock;
        DateTime bubbleUntil, happyUntil, lastFrame;
        Border bubble;
        TextBlock speech;
        StackPanel root;
        PanelWindow panel;
        bool dragged;
        Point start;
        double phase;
        Random random = new Random();
        readonly DrinkReactions drinkReactions;
        public CompanionWindow(HydrationSession session, JsonStateStore store) : this(session, store, null) { }

        internal CompanionWindow(HydrationSession session, JsonStateStore store, DesktopUpdateService updates, IWaterSound waterSound = null)
        {
            this.waterSound = waterSound ?? new WaterSound();
            Updates = updates;
            drinkReactions = new DrinkReactions(random);
            this.session = session;
            Store = store;
            Title = "Gole · seu pet de água";
            Width = 276;
            Height = Data.Size + 122;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            FontFamily = new FontFamily("Segoe UI");
            root = new StackPanel();
            speech = Theme.Text("", 13, Theme.Ink);
            speech.Margin = new Thickness(0);
            bubble = new Border { Background = Theme.B("#FFFFF9"), CornerRadius = new CornerRadius(18, 18, 5, 18), Padding = new Thickness(15, 11, 15, 11), MaxWidth = 256, MinHeight = 62, Margin = new Thickness(9, 6, 9, 0), Child = speech, Opacity = 0 };
            root.Children.Add(bubble);
            Art = new Bottle { Width = Data.Size * 220 / 255, Height = Data.Size, Cursor = Cursors.Hand, ToolTip = "Arraste para mover · clique para abrir as ações · botão direito para opções" };
            System.Windows.Automation.AutomationProperties.SetName(Art, "Garrafinha Gole. Clique para registrar água ou arraste para mover.");
            Art.MouseLeftButtonDown += delegate (object sender, MouseButtonEventArgs e)
            {
                start = PointToScreen(e.GetPosition(this));
                dragged = false;
                Art.CaptureMouse();
                e.Handled = true;
            };
            Art.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (e.LeftButton != MouseButtonState.Pressed || !Art.IsMouseCaptured)
                    return;
                Point point = PointToScreen(e.GetPosition(this));
                double dx = point.X - start.X, dy = point.Y - start.Y;
                var source = PresentationSource.FromVisual(this);
                if (source != null)
                {
                    var v = source.CompositionTarget.TransformFromDevice.Transform(new Vector(dx, dy));
                    dx = v.X;
                    dy = v.Y;
                }
                if (dragged || Math.Abs(dx) + Math.Abs(dy) > 4)
                {
                    dragged = true;
                    Left += dx;
                    Top += dy;
                    start = point;
                }
            };
            Art.MouseLeftButtonUp += delegate
            {
                Art.ReleaseMouseCapture();
                if (dragged)
                {
                    KeepVisible();
                    Save();
                }
                else
                    OpenMenu();
            };
            Art.MouseRightButtonUp += delegate
            {
                OpenMenu();
            };
            root.Children.Add(Art);
            Content = root;
            Loaded += delegate
            {
                if (tray == null)
                    SetupTray();
                clock.Start();
                Place();
                Speak("Oi! Sou " + Data.Name + ". Bora de água?", 8);
            };
            KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Escape)
                    HidePet();
                if (e.Key == Key.Enter || e.Key == Key.Space)
                    OpenMenu();
            };
            clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            clock.Tick += delegate
            {
                Tick();
            };
            CompositionTarget.Rendering += Render;
            Closed += delegate
            {
                clock.Stop();
                this.waterSound.Dispose();
                CompositionTarget.Rendering -= Render;
                if (tray != null)
                {
                    tray.Visible = false;
                    tray.Icon.Dispose();
                    tray.Dispose();
                }
            };
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += DisplayChanged;
            Closed += delegate
            {
                Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= DisplayChanged;
            };
        }
        void DisplayChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(KeepVisible));
        }
        void Render(object sender, EventArgs args)
        {
            DateTime now = DateTime.UtcNow;
            if ((now - lastFrame).TotalMilliseconds < 65 || !IsVisible)
                return;
            lastFrame = now;
            if (Data.Motion)
                phase += .10;
            Art.Phase = phase;
            Art.Level = Data.Level(now);
            Art.Tint = Data.Color;
            Art.EquippedAccessories = Data.Accessories;
            Art.Vessel = Data.Vessel;
            Art.Paused = Data.Paused(now);
            Art.Celebrate = now < happyUntil;
            Art.InvalidateVisual();
        }
        public void Save()
        {
            Data.X = Left;
            Data.Y = Top;
            Store.Save(Data);
        }
        public void Apply()
        {
            Art.Width = Data.Size * 220 / 255;
            Art.Height = Data.Size;
            Height = Data.Size + 122;
            KeepVisible();
            Save();
        }
        void Place()
        {
            Rect area = SystemParameters.WorkArea;
            Left = Data.X == -99999 ? area.Right - Width - 30 : Data.X;
            Top = Data.Y == -99999 ? area.Bottom - Height - 10 : Data.Y;
            KeepVisible();
        }
        public void KeepVisible()
        {
            // WPF coordinates are DIPs. Keep the pet on a connected monitor after docking or DPI changes.
            var source = PresentationSource.FromVisual(this);
            var toDevice = source == null ? Matrix.Identity : source.CompositionTarget.TransformToDevice;
            Point center = toDevice.Transform(new Point(Left + Width / 2, Top + Height / 2));
            var screen = Forms.Screen.FromPoint(new System.Drawing.Point((int)center.X, (int)center.Y));
            var area = screen.WorkingArea;
            var fromDevice = toDevice;
            fromDevice.Invert();
            Point min = fromDevice.Transform(new Point(area.Left, area.Top));
            Point max = fromDevice.Transform(new Point(area.Right, area.Bottom));
            Left = Math.Max(min.X, Math.Min(Left, max.X - Width));
            Top = Math.Max(min.Y, Math.Min(Top, max.Y - Height));
        }
        public void ShowPet()
        {
            Show();
            KeepVisible();
        }
        public void HidePet()
        {
            Hide();
        }
        public void Speak(string text, int seconds)
        {
            speech.Text = text;
            bubble.Opacity = 1;
            bubbleUntil = DateTime.UtcNow.AddSeconds(seconds);
            if (Data.SpeechSound)
                waterSound.Play();
        }
        public void Drink()
        {
            DateTime now = DateTime.UtcNow;
            session.Drink(now);
            happyUntil = now.AddSeconds(7);
            Speak(drinkReactions.Next(), 10);
            Save();
            if (panel != null)
                panel.RefreshStats();
        }
        public bool CanUndo
        {
            get
            {
                return session.CanUndo(DateTime.UtcNow);
            }
        }
        public void Undo()
        {
            if (!session.Undo(DateTime.UtcNow))
                return;
            happyUntil = DateTime.MinValue;
            Speak("Registro desfeito. Sem crise, eu espero o gole de verdade.", 8);
            Save();
            if (panel != null)
                panel.RefreshStats();
        }
        public void Pause(int minutes)
        {
            session.Pause(DateTime.UtcNow, minutes);
            Speak("Pausa de " + minutes + " min. Zzz…", 8);
            Save();
            if (panel != null)
                panel.RefreshStats();
        }
        public void Resume()
        {
            session.Resume(DateTime.UtcNow);
            Speak("Voltei! Bora de água?", 8);
            Save();
            if (panel != null)
                panel.RefreshStats();
        }
        public void ConfigureInterval(int minutes)
        {
            session.SetInterval(DateTime.UtcNow, minutes);
            Save();
        }
        public string NextText()
        {
            DateTime now = DateTime.UtcNow;
            if (Data.Paused(now))
                return "Pausado até " + new DateTime(Data.PauseEnd, DateTimeKind.Utc).ToLocalTime().ToString("HH:mm");
            int mins = Math.Max(0, (int)Math.Ceiling(TimeSpan.FromTicks(Data.NextReminder - now.Ticks).TotalMinutes));
            return mins == 0 ? "Hora de um gole" : "Próximo lembrete em " + mins + " min";
        }
        public void PreviewPhrase()
        {
            ShowPet();
            Speak(Phrase(), 12);
        }
        string Phrase()
        {
            return ReminderPhrases.Next(random, HydrationExpression.AtLevel(Data.Level(DateTime.UtcNow)));
        }
        void Tick()
        {
            DateTime now = DateTime.UtcNow;
            if (bubble.Opacity > 0 && now > bubbleUntil)
                bubble.Opacity = 0;
            var tick = session.Advance(now);
            if (tick.Notify)
            {
                string text = Phrase();
                if (IsVisible)
                    Speak(text, 22);
                if (Data.Notifications)
                {
                    tray.BalloonTipTitle = Data.Name + " quer um gole";
                    tray.BalloonTipText = text;
                    tray.BalloonTipIcon = Forms.ToolTipIcon.Info;
                    tray.ShowBalloonTip(8000);
                }
                if (!IsVisible && Data.SpeechSound)
                    waterSound.Play();
            }
            if (tick.Chatter && IsVisible)
                Speak(Phrase(), 14);
            if (tick.Changed)
                Save();
            tray.Text = ("Gole · " + Data.Today(now) + " ml hoje · " + (Data.Paused(now) ? "pausado" : "ativo")).Substring(0, Math.Min(63, ("Gole · " + Data.Today(now) + " ml hoje · " + (Data.Paused(now) ? "pausado" : "ativo")).Length));
            if (panel != null)
                panel.RefreshStats();
        }
        void Item(ContextMenu menu, string title, Action action)
        {
            var item = new MenuItem { Header = title, Padding = new Thickness(9, 7, 9, 7) };
            item.Click += delegate
            {
                action();
            };
            menu.Items.Add(item);
        }
        void OpenMenu()
        {
            var menu = new ContextMenu { FontFamily = FontFamily, FontSize = 14, PlacementTarget = Art };
            Item(menu, "✓  Já bebi um gole", Drink);
            Item(menu, "Abrir painel", delegate
            {
                OpenPanel(0);
            });
            if (CanUndo)
                Item(menu, "Desfazer último registro", Undo);
            menu.Items.Add(new Separator());
            Item(menu, "◷  " + NextText(), delegate
            {
                OpenPanel(2);
            });
            Item(menu, "Personalizar meu pet", delegate
            {
                OpenPanel(1);
            });
            menu.Items.Add(new Separator());
            Item(menu, "Diminuir", delegate
            {
                Data.Size = Math.Max(100, Data.Size - 25);
                Apply();
            });
            Item(menu, "Aumentar", delegate
            {
                Data.Size = Math.Min(280, Data.Size + 25);
                Apply();
            });
            Item(menu, "Esconder na bandeja", HidePet);
            Item(menu, "Sair do Gole", Exit);
            Art.ContextMenu = menu;
            menu.IsOpen = true;
        }
        void SetupTray()
        {
            tray = new Forms.NotifyIcon();
            using (var bitmap = new System.Drawing.Bitmap(32, 32))
            {
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(System.Drawing.Color.Transparent);
                    using (var b = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(18, 160, 170)))
                        g.FillEllipse(b, 4, 6, 24, 24);
                    g.FillRectangle(System.Drawing.Brushes.MediumAquamarine, 11, 1, 10, 7);
                    g.FillEllipse(System.Drawing.Brushes.White, 10, 13, 4, 5);
                    g.FillEllipse(System.Drawing.Brushes.White, 19, 13, 4, 5);
                    g.DrawArc(System.Drawing.Pens.White, 12, 17, 9, 7, 0, 180);
                }
                IntPtr handle = bitmap.GetHicon();
                tray.Icon = (System.Drawing.Icon)System.Drawing.Icon.FromHandle(handle).Clone();
                DestroyIcon(handle);
            }
            tray.Text = "Gole · seu pet de água";
            tray.Visible = true;
            var menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Mostrar garrafinha", null, delegate
            {
                Dispatcher.Invoke(new Action(ShowPet));
            });
            menu.Items.Add("Já bebi um gole", null, delegate
            {
                Dispatcher.Invoke(new Action(Drink));
            });
            menu.Items.Add("Abrir Gole", null, delegate
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    OpenPanel(0);
                }));
            });
            menu.Items.Add("Sair", null, delegate
            {
                Dispatcher.Invoke(new Action(Exit));
            });
            tray.ContextMenuStrip = menu;
            tray.DoubleClick += delegate
            {
                Dispatcher.Invoke(new Action(ShowPet));
            };
            tray.BalloonTipClicked += delegate
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    ShowPet();
                    OpenMenu();
                }));
            };
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr icon);
        public void OpenPanel(int tab)
        {
            if (panel == null)
            {
                panel = new PanelWindow(this);
                panel.Closed += delegate
                {
                    panel = null;
                };
            }
            panel.SelectPage(tab);
            panel.Show();
            panel.Activate();
        }
        void Exit()
        {
            Save();
            Application.Current.Shutdown();
        }
    }
}
