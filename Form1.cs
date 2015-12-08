using System;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PCGUI
{
    public partial class Form1 : Form
    {

        private Size beginResize;
        private PseudoMain main;
        private WebBrowser wb;
        public Thread runThread;

        public Form1()
        {
            InitializeComponent();
            main = new PseudoMain(this);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            DoubleBuffered = true;
            removeSizeManagers();
            MaximizeBox = false;

            AllowDrop = true;
            DragDrop += Form1_DragDrop;
            DragEnter += Form1_DragEnter;

            foreach (string manager in new string[] { "console", "input" })
            {
                RichTBox rtb = new RichTBox();
                rtb.BorderStyle = BorderStyle.None;
                rtb.Name = manager;
                rtb.ForeColor = Color.White;
                rtb.Font = new Font("Consolas", 18);

                TranslucentPanel tp = new TranslucentPanel();
                rtb.Parent = tp;
                tp.Name = manager + "_panel";
                tp.BackColor = Color.FromArgb(30, 0, 0, 0);
                tp.Size = Controls.Find("sizeManager_" + manager, true)[0].Size;
                tp.Location = Controls.Find("sizeManager_" + manager, true)[0].Location;
                rtb.Size = tp.Size;
                tp.Controls.Add(rtb);
                rtb.Parent = tp;
                rtb.Dock = DockStyle.Fill;
                tp.BorderStyle = BorderStyle.None;

                Controls.Add(tp);
            }

            panel_save.Location = panel_run_code.Location;
            button_save.Location = button_run_code.Location;

            RichTBox rt = new RichTBox();
            rt.Name = "editor";
            rt.ForeColor = Color.White;
            rt.Font = new Font("Consolas", 18);
            rt.BorderStyle = BorderStyle.None;

            TranslucentPanel t = new TranslucentPanel();
            rt.Parent = t;
            t.BorderStyle = BorderStyle.None;
            t.Controls.Add(rt);
            t.Name = "editor" + "_panel";
            t.BackColor = Color.FromArgb(30, 0, 0, 0);
            t.Size = Controls.Find("sizeManager_" + "console", true)[0].Size;
            t.Location = Controls.Find("sizeManager_" + "console", true)[0].Location;
            rt.Size = t.Size;
            rt.Parent = t;
            rt.Dock = DockStyle.Fill;
            Controls.Add(t);
            getTBox("editor").Hide();

            RichTBox console = getTBox("console");
            console.ReadOnly = true;

            RichTBox input = getTBox("input");
            input.Multiline = false;

            input.KeyDown += (a, b) =>
            {
                if (b.KeyCode == Keys.Enter && (runThread == null || !runThread.IsAlive))
                {
                    string txt = input.Text;
                    input.Clear();

                    if (txt.Equals("run"))
                    {
                        writeToConsole("Please use the run button to safely launch your code.", Color.Red);
                    }
                    else
                    {
                        main.manageInputs(txt, main.name);
                    }

                    b.SuppressKeyPress = true;
                    b.Handled = true;
                }
            };

            loadedFile.Font = new Font("Consolas", 20);
            loadedFile.Text = "no file loaded";
            loadedFile.TextAlign = ContentAlignment.MiddleCenter;

            string[] desc = new string[]
            {
                "save changes",
                "about",
                "open new file",
                "refresh file",
                "run code",
                "stop code",
                "main console",
                "view source",
                "edit code"
            };

            int z = 0;

            foreach (Control c in new Control[] { panel_save, panel_about, panel_open_new_file, panel_refresh, panel_run_code, panel_stop_code, panel_running, panel_source, panel_edit })
            {
                c.MouseEnter += (a, b) =>
                {
                    c.BackColor = Color.CadetBlue;
                    Cursor = Cursors.Hand;
                    if (!findControl(c.Name + "_info").Visible)
                    {
                        findControl(c.Name + "_info").Show();
                        findControl(c.Name + "_info").BringToFront();
                    }
                };

                c.MouseLeave += (a, b) =>
                {
                    c.BackColor = Color.Transparent;
                    Cursor = Cursors.Default;
                    if (findControl(c.Name + "_info").Visible)
                    {
                        findControl(c.Name + "_info").Hide();
                    }
                };

                Label label = new Label();
                label.Text = desc[z];
                label.Font = new Font("Consolas", 12);
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.AutoSize = false;
                label.Location = new Point(c.Location.X, c.Location.Y - c.Size.Height);
                label.Size = c.Size;
                label.Visible = false;
                label.Name = c.Name + "_info";
                label.ForeColor = Color.White;
                label.BackColor = Color.Transparent;
                Controls.Add(label);

                z++;
            }

            foreach (Control c in new Control[] { button_save, button_about, button_edit, button_open_new_file, button_refresh, button_running, button_run_code, button_source, button_stop_code })
            {
                c.MouseEnter += (a, b) =>
                {
                    //c.Location = new Point(c.Location.X, c.Location.Y - 3);
                    c.Parent.BackColor = Color.CadetBlue;
                    Cursor = Cursors.Hand;
                    if (!findControl(c.Name.Replace("button", "panel") + "_info").Visible)
                    {
                        findControl(c.Name.Replace("button", "panel") + "_info").Show();
                    }
                };

                c.MouseLeave += (a, b) =>
                {
                    //c.Location = new Point(c.Location.X, c.Location.Y + 3);
                    c.Parent.BackColor = Color.Transparent;
                    Cursor = Cursors.Default;
                    if (findControl(c.Name.Replace("button", "panel") + "_info").Visible)
                    {
                        findControl(c.Name.Replace("button", "panel") + "_info").Hide();
                    }
                };
            }

            main.init();

            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                main.name = ofd.FileName;
                loadedFile.Text = main.name.Split('\\')[main.name.Split('\\').Length - 1];
                Opacity = 0;
                Visible = true;
                BringToFront();
                Show();
                WindowState = FormWindowState.Normal;

                for (float i = 0; i <= 1; i += 0.02f)
                {
                    Opacity = i;
                    await Task.Delay(5);
                }

                Opacity = 1;

                if (main.start(main.name))
                {
                    input.Focus();
                }
                else
                {
                    modVisible(new Control[] { button_refresh, panel_refresh, button_run_code, panel_run_code, button_open_new_file, panel_open_new_file, button_running, panel_running, button_edit, panel_edit, button_about, panel_about, button_source, panel_source, loadedFile, button_stop_code, panel_stop_code, input }, false);
                }
            }
            else
            {
                Environment.Exit(0);
            }
        }

        public RichTBox getTBox(string name)
        {
            return (RichTBox)Controls.Find(name, true)[0];
        }

        public void clearConsole()
        {
            getTBox("console").Clear();
        }

        delegate void SetTextCallback(string text, Color color);

        public void writeToConsole(string text)
        {
            writeToConsole(text, Color.White);
        }

        public void writeToConsole(string text, Color color)
        {
            if (getTBox("console").InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(writeToConsole);
                this.Invoke(d, new object[] { text, color });
            }
            else
            {
                RichTBox c = getTBox("console");
                c.SelectionStart = c.TextLength;
                c.SelectionLength = 0;

                c.SelectionColor = color;
                c.AppendText(text + "\n");
                c.SelectionColor = c.ForeColor;
                c.ScrollToCaret();
            }
        }

        private void removeSizeManagers()
        {
            foreach (Control c in Controls)
            {
                if (c.Name.StartsWith("sizeManager"))
                {
                    c.Visible = false;
                }
            }
        }

        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            beginResize = ((Form)sender).Size;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            int startHeight = beginResize.Height;
            int endHeight = ((Form)sender).Size.Height;
            int startWidth = beginResize.Width;
            int endWidth = ((Form)sender).Size.Width;

            foreach (string cc in new string[] { "console_panel", "console", "editor_panel", "editor", "wb" })
            {
                if (Controls.ContainsKey(cc))
                {
                    Control c = Controls.Find(cc, true)[0];
                    int newHeight = endHeight - startHeight;
                    int newWidth = endWidth - startWidth;
                    c.Size = new Size(c.Width + newWidth, c.Height + newHeight);
                }

            }

            foreach (string cc in new string[] { "panel_save", "button_save", "panel_run_code", "panel_stop_code", "panel_refresh", "panel_open_new_file", "input", "input_panel" })
            {
                Control c = Controls.Find(cc, true)[0];
                int newHeight = endHeight - startHeight;
                c.Location = new Point(c.Location.X, c.Location.Y + newHeight);
            }

            foreach (string cc in new string[] { "button_exit", "button_minimize" })
            {
                Control c = Controls.Find(cc, true)[0];
                int newWidth = endWidth - startWidth;
                c.Location = new Point(c.Location.X + newWidth, c.Location.Y);
            }

            int w = endWidth - startWidth;
            Controls.Find("input_panel", true)[0].Size = new Size(Controls.Find("input_panel", true)[0].Size.Width + w, Controls.Find("input_panel", true)[0].Height);
            Controls.Find("panel1", true)[0].Size = new Size(Controls.Find("panel1", true)[0].Size.Width + w, Controls.Find("panel1", true)[0].Height);
            Controls.Find("loadedFile", true)[0].Size = new Size(Controls.Find("loadedFile", true)[0].Size.Width + w, Controls.Find("loadedFile", true)[0].Height);

            Control console = Controls.Find("console", true)[0];
            console.Text = console.Text + "";

            foreach (Control c in Controls)
            {
                if (c.Name.EndsWith("_info") && c.Location.Y > 10)
                {
                    int newHeight = endHeight - startHeight;
                    c.Location = new Point(c.Location.X, c.Location.Y + newHeight);
                }
            }

            foreach (Control c in Controls)
            {
                c.Refresh();
            }
        }

        private void button_open_new_file_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                modVisible(new Control[] { findControl("editor_panel"), findControl("editor") }, false);
                modVisible(new Control[] { findControl("console"), findControl("console_panel") }, true);
                clearConsole();
                main = new PseudoMain(this);
                main.init();
                main.name = ofd.FileName;
                loadedFile.Text = main.name.Split('\\')[main.name.Split('\\').Length - 1];
                main.start(main.name);
            }
        }

        private void panel_open_new_file_Click(object sender, EventArgs e)
        {
            button_open_new_file_Click(sender, e);
        }

        private void panel_open_new_file_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button_about_Click(object sender, EventArgs e)
        {
            disposeBrowser();
            modVisible(new Control[] { getTBox("editor"), findControl("editor_panel") }, false);
            modVisible(new Control[] { getTBox("console"), findControl("console_panel") }, true);
            modVisible(new Control[] { button_refresh, panel_refresh, button_run_code, panel_run_code, button_open_new_file, panel_open_new_file, panel_stop_code, button_stop_code }, true);
            modVisible(new Control[] { panel_save, button_save }, false);
            main.manageInputs("about", main.name);
        }

        private void panel_about_MouseEnter(object sender, EventArgs e)
        {
            panel_about.BackColor = Color.Black;
        }

        private void button_run_code_Click(object sender, EventArgs e)
        {
            if (main.name != null && !main.name.Equals("") && main.name.EndsWith(".txt"))
            {
                disposeBrowser();
                modVisible(new Control[] {getTBox("editor"), findControl("editor_panel") }, false);
                modVisible(new Control[] { getTBox("console"), findControl("console_panel") }, true);
                modVisible(new Control[] { button_refresh, panel_refresh, button_run_code, panel_run_code, button_open_new_file, panel_open_new_file, button_running, panel_running, button_edit, panel_edit, button_about, panel_about, button_source, panel_source, loadedFile}, false);
                panel_stop_code.Location = panel_run_code.Location;
                findControl("panel_stop_code_info").Location = findControl("panel_run_code_info").Location;
                main.manageInputs("run", main.name);
            }
            else
            {
                MessageBox.Show("You have not chosen a valid file to run!", "Error");
                writeToConsole("You must open a txt file to run the program.", Color.Red);
            }
        }

        public void finish()
        {
            panel_stop_code.Location = new Point(panel_run_code.Location.X + panel_stop_code.Width + 7, panel_run_code.Location.Y);
            findControl("panel_stop_code_info").Location = new Point(panel_stop_code.Location.X, panel_stop_code.Location.Y - panel_stop_code.Height);
            modVisible(new Control[] { button_refresh, panel_refresh, button_run_code, panel_run_code, button_open_new_file, panel_open_new_file, button_running, panel_running, button_edit, panel_edit, button_about, panel_about, button_source, panel_source, loadedFile}, true);
        }

        private void panel_run_code_Click(object sender, EventArgs e)
        {
            button_run_code_Click(sender, e);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Environment.Exit(0);
            }
            catch (Exception) { }
        }

        private void panel_source_Paint(object sender, PaintEventArgs e)
        {
            button_source_Click(sender, e);
        }

        private void button_source_Click(object sender, EventArgs e)
        {
            wb = new WebBrowser();
            wb.Size = new Size(findControl("console_panel").Size.Width, Size.Height - sizeManager_console.Location.Y);
            wb.Location = sizeManager_console.Location;
            wb.Name = "wb";
            Controls.Add(wb);
            modVisible(new Control[] { getTBox("console"), getTBox("editor"), findControl("console_panel"), findControl("editor_panel") }, false);
            wb.Show();
            wb.BringToFront();
            wb.Url = new Uri("https://github.com/lyokofirelyte/PseudoCompiler");
            modVisible(new Control[] { button_refresh, panel_refresh, button_run_code, panel_run_code, button_open_new_file, panel_open_new_file, panel_stop_code, button_stop_code }, false);
            modVisible(new Control[] { panel_save, button_save }, false);
        }

        private Control findControl(string name)
        {
            return Controls.Find(name, true)[0];
        }

        private void panel_source_Click(object sender, EventArgs e)
        {
            button_source_Click(sender, e);
        }

        private void panel_main_Click(object sender, EventArgs e)
        {
            button_running_Click(sender, e);
        }

        private void button_running_Click(object sender, EventArgs e)
        {
            modVisible(new Control[] { getTBox("editor"), findControl("editor_panel")}, false);
            modVisible(new Control[] { getTBox("console"), findControl("console_panel") }, true);
            modVisible(new Control[] { button_refresh, panel_refresh, button_run_code, panel_run_code, button_open_new_file, panel_open_new_file, panel_stop_code, button_stop_code }, true);
            modVisible(new Control[] { panel_save, button_save }, false);
            disposeBrowser();
        }

        private void disposeBrowser()
        {
            try
            {
                wb.Dispose();
                wb = null;
            }
            catch (Exception) { }
        }

        private void panel_about_Click(object sender, EventArgs e)
        {
            button_about_Click(sender, e);
        }

        private void panel_edit_Click(object sender, EventArgs e)
        {
            button_edit_Click(sender, e);
        }

        private void button_edit_Click(object sender, EventArgs e)
        {
            disposeBrowser();
            modVisible(new Control[] { getTBox("console"), Controls.Find("console_panel", true)[0] }, false);
            modVisible(new Control[] { getTBox("editor"), Controls.Find("editor_panel", true)[0] }, true);
            modVisible(new Control[] { panel_save, button_save }, true);
            modVisible(new Control[] { button_refresh, panel_refresh, button_run_code, panel_run_code, button_open_new_file, panel_open_new_file, panel_stop_code, button_stop_code }, false);

            panel_save.Location = panel_run_code.Location;
            button_save.Location = button_run_code.Location;
            button_save.BringToFront();

            RichTBox tb = getTBox("editor");
            tb.Multiline = true;
            tb.ReadOnly = false;
            tb.Clear();

            foreach (string s in System.IO.File.ReadAllLines(main.name))
            {
                tb.AppendText(s + "\n");
            }
        }

        private void modVisible(Control[] ctrls, bool visible)
        {
            foreach (Control c in ctrls)
            {
                if (c != null)
                {
                    if (visible)
                    {
                        c.Show();
                    }
                    else
                    {
                        c.Hide();
                    }
                }
            }
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private async void pictureBox1_Click(object sender, EventArgs e)
        {
            writeToConsole(":(", Color.CadetBlue);
            for (float i = 1; i >= 0; i -= 0.02f)
            {
                Opacity = i;
                await Task.Delay(5);
            }

            Environment.Exit(0);
        }

        /* This method is from http://stackoverflow.com/questions/17748446/custom-resize-handle-in-border-less-form-c-sharp, I didn't write this. */
        /* Ain't nobody got time for this just to resize a window! */
        protected override void WndProc(ref Message m)
        {
            const UInt32 WM_NCHITTEST = 0x0084;
            const UInt32 WM_MOUSEMOVE = 0x0200;

            const UInt32 HTLEFT = 10;
            const UInt32 HTRIGHT = 11;
            const UInt32 HTBOTTOMRIGHT = 17;
            const UInt32 HTBOTTOM = 15;
            const UInt32 HTBOTTOMLEFT = 16;
            const UInt32 HTTOP = 12;
            const UInt32 HTTOPLEFT = 13;
            const UInt32 HTTOPRIGHT = 14;

            const int RESIZE_HANDLE_SIZE = 10;
            bool handled = false;
            if (m.Msg == WM_NCHITTEST || m.Msg == WM_MOUSEMOVE)
            {
                Size formSize = this.Size;
                Point screenPoint = new Point(m.LParam.ToInt32());
                Point clientPoint = this.PointToClient(screenPoint);

                Dictionary<UInt32, Rectangle> boxes = new Dictionary<UInt32, Rectangle>() {
            {HTBOTTOMLEFT, new Rectangle(0, formSize.Height - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
            {HTBOTTOM, new Rectangle(RESIZE_HANDLE_SIZE, formSize.Height - RESIZE_HANDLE_SIZE, formSize.Width - 2*RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
            {HTBOTTOMRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, formSize.Height - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
            {HTRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, formSize.Height - 2*RESIZE_HANDLE_SIZE)},
            {HTTOPRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
            {HTTOP, new Rectangle(RESIZE_HANDLE_SIZE, 0, formSize.Width - 2*RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
            {HTTOPLEFT, new Rectangle(0, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
            {HTLEFT, new Rectangle(0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, formSize.Height - 2*RESIZE_HANDLE_SIZE) }
        };

                foreach (KeyValuePair<UInt32, Rectangle> hitBox in boxes)
                {
                    if (hitBox.Value.Contains(clientPoint))
                    {
                        m.Result = (IntPtr)hitBox.Key;
                        handled = true;
                        break;
                    }
                }
            }

            if (!handled)
                base.WndProc(ref m);
        }

        private void button_exit_MouseEnter(object sender, EventArgs e)
        {
            button_exit.BackColor = Color.CadetBlue;
            Cursor = Cursors.Hand;
        }

        private void button_exit_MouseLeave(object sender, EventArgs e)
        {
            button_exit.BackColor = Color.Transparent;
            Cursor = Cursors.Default;
        }

        private void button_minimize_MouseEnter(object sender, EventArgs e)
        {
            button_minimize.BackColor = Color.CadetBlue;
            Cursor = Cursors.Hand;
        }

        private void button_minimize_MouseLeave(object sender, EventArgs e)
        {
            button_minimize.BackColor = Color.Transparent;
            Cursor = Cursors.Default;
        }

        private void button_stop_code_Click(object sender, EventArgs e)
        {
            if (runThread != null && runThread.IsAlive)
            {
                try
                {
                    runThread.Abort();
                } catch (Exception)
                {
                    writeToConsole("There's no code to stop.", Color.Red);
                }
            }
            else
            {
                writeToConsole("There's no code to stop.", Color.Red);
            }
        }

        private void panel_stop_code_Click(object sender, EventArgs e)
        {
            button_stop_code_Click(sender, e);
        }

        private void button_refresh_Click(object sender, EventArgs e)
        {
            string name = main.name;
            clearConsole();
            main = new PseudoMain(this);
            main.init();
            main.name = name;
            loadedFile.Text = main.name.Split('\\')[main.name.Split('\\').Length - 1];
            main.start(main.name);
        }

        private void panel_refresh_Click(object sender, EventArgs e)
        {
            button_refresh_Click(sender, e);
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            System.IO.File.WriteAllLines(main.name, getTBox("editor").Text.Split('\n'));
            button_running_Click(sender, e);
            button_refresh_Click(sender, e);
        }

        private void panel_save_Click(object sender, EventArgs e)
        {
            button_save_Click(sender, e);
        }

        protected bool validData;
        string path;
        protected Image image;
        protected Thread getImageThread;

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            validData = GetFilename(out filename, e);
            if (validData)
            {
                path = filename;
                getImageThread = new Thread(new ThreadStart(LoadImage));
                getImageThread.Start();
                e.Effect = DragDropEffects.Copy;
            }
            else
                e.Effect = DragDropEffects.None;
        }
        private bool GetFilename(out string filename, DragEventArgs e)
        {
            bool ret = false;
            filename = String.Empty;
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = ((IDataObject)e.Data).GetData("FileDrop") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is String))
                    {
                        filename = ((string[])data)[0];
                        string ext = Path.GetExtension(filename).ToLower();
                        if ((ext == ".jpg") || (ext == ".png") || (ext == ".bmp") || (ext == ".gif"))
                        {
                            ret = true;
                        }
                    }
                }
            }
            return ret;
        }
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (validData)
            {
                while (getImageThread.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(0);
                }
                BackgroundImage = image;
            }
        }
        protected void LoadImage()
        {
            image = new Bitmap(path);
        }

        Bitmap bitmap, img;
        Graphics bmpgraphic;
    }
}
