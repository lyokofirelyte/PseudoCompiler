using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace PseudoCompiler
{
    class PseudoEventHandler
    {

        private List<string> timers = new List<string>();
        private PseudoMain main;

        public PseudoEventHandler(PseudoMain i)
        {
            main = i;
        }

        public void focusGained(object sender, EventArgs e)
        {
            Control control = (Control)sender;

            if (!timers.Contains((control.Name)))
            {
                timers.Add(control.Name);
                startTimer(control);
            }

            if (main.containsSetting(control.Name))
            {
                control.Text = "> " + main.getSetting(control.Name); // to prevent glitch
            }
        }

        public void focusLost(object sender, EventArgs e)
        {

            Control c = (Control)sender;

            if (timers.Contains(c.Name))
            {
                timers.Remove(c.Name);
                if (sender is GenUtils.FancyLabel || sender is Label)
                {
                    c.BackColor = Color.Purple;
                }
            }

            if (main.containsSetting(c.Name))
            {
                c.Text = c.Name;
            }
        }

        public void onMouseDown(object o, MouseEventArgs e)
        {
            Control clicked = ((Control)o);

            switch (clicked.Name.ToLower())
            {
                default:

                    if (clicked.Name.Contains("color_"))
                    {
                        try
                        {
                            ConsoleColor color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), clicked.Name.Split('_')[1]);

                            if (clicked.Name.StartsWith("b"))
                            {
                                Console.BackgroundColor = color;
                                main.modifySetting("bcolor", color.ToString());
                            }
                            else
                            {
                                Console.ForegroundColor = color;
                                main.modifySetting("fcolor", color.ToString());
                            }

                            clicked.Parent.Parent.Dispose();
                        }
                        catch (Exception botchedColor)
                        {
                            MessageBox.Show("That color can not be displayed. Please pick a different color.");
                        }
                    }

                break;

                case "debugs":

                    bool currentDebug = Boolean.Parse(main.getSetting("debugs"));
                    main.modifySetting("debugs", !currentDebug + "");
                    clicked.Text = !currentDebug + "";

                break;

                case "source":

                    Process.Start("https://github.com/lyokofirelyte/PseudoCompiler");

                break;

                case "update":

                    main.modifySetting("update", main.getSetting("update").Equals("automatic") ? "disabled" : "automatic");
                    clicked.Text = main.getSetting("update");

                break;

                case "fcolor": case "bcolor":

                    Form settingsForm = new Form();

                    Panel panel = new Panel();
                    panel.Dock = DockStyle.Fill;
                    panel.AutoSize = false;
                    panel.Size = new Size(300, 300);
                    panel.AutoScroll = true;
                    panel.BackColor = Color.LightSlateGray;

                    settingsForm.FormBorderStyle = FormBorderStyle.FixedSingle;
                    settingsForm.Text = "Color Picker";
                    settingsForm.AutoScaleMode = AutoScaleMode.Font;
                    settingsForm.AutoSize = false;
                    settingsForm.Size = panel.Size;
                    settingsForm.MaximizeBox = false;
                    settingsForm.MinimizeBox = false;

                    int yAmt = 0;
                            
                    foreach (string set in Enum.GetNames(typeof(ConsoleColor)))
                    {
                        GenUtils.FancyLabel l = new GenUtils.FancyLabel()
                        {
                            Name = clicked.Name.ToLower() + "_" + set,
                            Location = new Point(5, settingsForm.Location.Y + yAmt + 5),
                            AutoSize = true,
                            Text = set.ToUpper(),
                            Font = new Font("Courier New", 20),
                            BackColor = Color.Purple,
                            ForeColor = Color.White,
                            TextAlign = ContentAlignment.MiddleCenter,
                            Parent = panel
                        };

                        Control cont = (Control)l;
                        cont.MouseDown += new MouseEventHandler(onMouseDown);
                        cont.MouseEnter += new EventHandler(focusGained);
                        cont.MouseLeave += new EventHandler(focusLost);

                        panel.Controls.Add(l);
                        l.Visible = true;
                        l.BringToFront();
                        yAmt += 10 + l.Height;
                    }

                    settingsForm.Controls.Add(panel);
                    settingsForm.ShowDialog();

                break;
            }

        }

        private async void startTimer(Control ctrl)
        {
            while (timers.Contains(ctrl.Name))
            {
                for (double i = 0; i < 1 && timers.Contains(ctrl.Name); i += 0.01)
                {
                    GenUtils.ColorRGB c = GenUtils.HSL2RGB(i, 0.5, 0.5);
                    await Task.Delay(50);
                    ctrl.BackColor = c;
                }

                if (!timers.Contains(ctrl.Name))
                {
                    if (ctrl is GenUtils.FancyLabel || ctrl is Label)
                    {
                        ctrl.BackColor = Color.Purple;
                    }

                    break;
                }
            }

            if (!timers.Contains(ctrl.Name))
            {
                if (ctrl is GenUtils.FancyLabel || ctrl is Label)
                {
                    ctrl.BackColor = Color.Purple;
                }
            }
        }
    }
}
