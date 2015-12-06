namespace PCGUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.sizeManager_console = new System.Windows.Forms.Panel();
            this.button_run_code = new System.Windows.Forms.PictureBox();
            this.button_running = new System.Windows.Forms.PictureBox();
            this.panel_running = new System.Windows.Forms.Panel();
            this.sizeManager_input = new System.Windows.Forms.Panel();
            this.button_stop_code = new System.Windows.Forms.PictureBox();
            this.button_refresh = new System.Windows.Forms.PictureBox();
            this.button_open_new_file = new System.Windows.Forms.PictureBox();
            this.button_edit = new System.Windows.Forms.PictureBox();
            this.panel_edit = new System.Windows.Forms.Panel();
            this.button_source = new System.Windows.Forms.PictureBox();
            this.panel_source = new System.Windows.Forms.Panel();
            this.loadedFile = new System.Windows.Forms.Label();
            this.panel_run_code = new System.Windows.Forms.Panel();
            this.panel_stop_code = new System.Windows.Forms.Panel();
            this.panel_refresh = new System.Windows.Forms.Panel();
            this.panel_open_new_file = new System.Windows.Forms.Panel();
            this.panel_about = new System.Windows.Forms.Panel();
            this.button_about = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.button_minimize = new System.Windows.Forms.PictureBox();
            this.button_exit = new System.Windows.Forms.PictureBox();
            this.panel_save = new System.Windows.Forms.Panel();
            this.button_save = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.button_run_code)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_running)).BeginInit();
            this.panel_running.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.button_stop_code)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_refresh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_open_new_file)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_edit)).BeginInit();
            this.panel_edit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.button_source)).BeginInit();
            this.panel_source.SuspendLayout();
            this.panel_run_code.SuspendLayout();
            this.panel_stop_code.SuspendLayout();
            this.panel_refresh.SuspendLayout();
            this.panel_open_new_file.SuspendLayout();
            this.panel_about.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.button_about)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_minimize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_exit)).BeginInit();
            this.panel_save.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.button_save)).BeginInit();
            this.SuspendLayout();
            // 
            // sizeManager_console
            // 
            this.sizeManager_console.Location = new System.Drawing.Point(12, 102);
            this.sizeManager_console.Name = "sizeManager_console";
            this.sizeManager_console.Size = new System.Drawing.Size(1217, 486);
            this.sizeManager_console.TabIndex = 0;
            // 
            // button_run_code
            // 
            this.button_run_code.BackColor = System.Drawing.Color.Transparent;
            this.button_run_code.Image = ((System.Drawing.Image)(resources.GetObject("button_run_code.Image")));
            this.button_run_code.Location = new System.Drawing.Point(45, -1);
            this.button_run_code.Name = "button_run_code";
            this.button_run_code.Size = new System.Drawing.Size(48, 48);
            this.button_run_code.TabIndex = 1;
            this.button_run_code.TabStop = false;
            this.button_run_code.Click += new System.EventHandler(this.button_run_code_Click);
            // 
            // button_running
            // 
            this.button_running.BackColor = System.Drawing.Color.Transparent;
            this.button_running.Image = ((System.Drawing.Image)(resources.GetObject("button_running.Image")));
            this.button_running.Location = new System.Drawing.Point(45, -1);
            this.button_running.Name = "button_running";
            this.button_running.Size = new System.Drawing.Size(48, 48);
            this.button_running.TabIndex = 2;
            this.button_running.TabStop = false;
            this.button_running.Click += new System.EventHandler(this.button_running_Click);
            // 
            // panel_running
            // 
            this.panel_running.BackColor = System.Drawing.Color.Transparent;
            this.panel_running.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel_running.BackgroundImage")));
            this.panel_running.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel_running.Controls.Add(this.button_running);
            this.panel_running.Location = new System.Drawing.Point(12, 49);
            this.panel_running.Name = "panel_running";
            this.panel_running.Size = new System.Drawing.Size(139, 47);
            this.panel_running.TabIndex = 3;
            this.panel_running.Click += new System.EventHandler(this.panel_main_Click);
            // 
            // sizeManager_input
            // 
            this.sizeManager_input.Location = new System.Drawing.Point(12, 594);
            this.sizeManager_input.Name = "sizeManager_input";
            this.sizeManager_input.Size = new System.Drawing.Size(1217, 35);
            this.sizeManager_input.TabIndex = 4;
            // 
            // button_stop_code
            // 
            this.button_stop_code.BackColor = System.Drawing.Color.Transparent;
            this.button_stop_code.Image = ((System.Drawing.Image)(resources.GetObject("button_stop_code.Image")));
            this.button_stop_code.Location = new System.Drawing.Point(45, -1);
            this.button_stop_code.Name = "button_stop_code";
            this.button_stop_code.Size = new System.Drawing.Size(48, 48);
            this.button_stop_code.TabIndex = 5;
            this.button_stop_code.TabStop = false;
            this.button_stop_code.Click += new System.EventHandler(this.button_stop_code_Click);
            // 
            // button_refresh
            // 
            this.button_refresh.BackColor = System.Drawing.Color.Transparent;
            this.button_refresh.Image = ((System.Drawing.Image)(resources.GetObject("button_refresh.Image")));
            this.button_refresh.Location = new System.Drawing.Point(45, -1);
            this.button_refresh.Name = "button_refresh";
            this.button_refresh.Size = new System.Drawing.Size(48, 48);
            this.button_refresh.TabIndex = 6;
            this.button_refresh.TabStop = false;
            this.button_refresh.Click += new System.EventHandler(this.button_refresh_Click);
            // 
            // button_open_new_file
            // 
            this.button_open_new_file.BackColor = System.Drawing.Color.Transparent;
            this.button_open_new_file.Image = ((System.Drawing.Image)(resources.GetObject("button_open_new_file.Image")));
            this.button_open_new_file.Location = new System.Drawing.Point(45, -1);
            this.button_open_new_file.Name = "button_open_new_file";
            this.button_open_new_file.Size = new System.Drawing.Size(48, 48);
            this.button_open_new_file.TabIndex = 8;
            this.button_open_new_file.TabStop = false;
            this.button_open_new_file.Click += new System.EventHandler(this.button_open_new_file_Click);
            // 
            // button_edit
            // 
            this.button_edit.BackColor = System.Drawing.Color.Transparent;
            this.button_edit.Image = ((System.Drawing.Image)(resources.GetObject("button_edit.Image")));
            this.button_edit.Location = new System.Drawing.Point(45, -1);
            this.button_edit.Name = "button_edit";
            this.button_edit.Size = new System.Drawing.Size(48, 48);
            this.button_edit.TabIndex = 2;
            this.button_edit.TabStop = false;
            this.button_edit.Click += new System.EventHandler(this.button_edit_Click);
            // 
            // panel_edit
            // 
            this.panel_edit.BackColor = System.Drawing.Color.Transparent;
            this.panel_edit.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel_edit.BackgroundImage")));
            this.panel_edit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel_edit.Controls.Add(this.button_edit);
            this.panel_edit.Location = new System.Drawing.Point(160, 49);
            this.panel_edit.Name = "panel_edit";
            this.panel_edit.Size = new System.Drawing.Size(139, 47);
            this.panel_edit.TabIndex = 4;
            this.panel_edit.Click += new System.EventHandler(this.panel_edit_Click);
            // 
            // button_source
            // 
            this.button_source.BackColor = System.Drawing.Color.Transparent;
            this.button_source.Image = ((System.Drawing.Image)(resources.GetObject("button_source.Image")));
            this.button_source.Location = new System.Drawing.Point(45, -1);
            this.button_source.Name = "button_source";
            this.button_source.Size = new System.Drawing.Size(48, 48);
            this.button_source.TabIndex = 2;
            this.button_source.TabStop = false;
            this.button_source.Click += new System.EventHandler(this.button_source_Click);
            // 
            // panel_source
            // 
            this.panel_source.BackColor = System.Drawing.Color.Transparent;
            this.panel_source.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel_source.BackgroundImage")));
            this.panel_source.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel_source.Controls.Add(this.button_source);
            this.panel_source.Location = new System.Drawing.Point(305, 49);
            this.panel_source.Name = "panel_source";
            this.panel_source.Size = new System.Drawing.Size(139, 47);
            this.panel_source.TabIndex = 10;
            this.panel_source.Click += new System.EventHandler(this.panel_source_Click);
            // 
            // loadedFile
            // 
            this.loadedFile.BackColor = System.Drawing.Color.Transparent;
            this.loadedFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadedFile.ForeColor = System.Drawing.Color.White;
            this.loadedFile.Location = new System.Drawing.Point(595, 49);
            this.loadedFile.Name = "loadedFile";
            this.loadedFile.Size = new System.Drawing.Size(634, 47);
            this.loadedFile.TabIndex = 11;
            this.loadedFile.Text = "---------------------------------------------------------------------------------" +
    "----";
            // 
            // panel_run_code
            // 
            this.panel_run_code.BackColor = System.Drawing.Color.Transparent;
            this.panel_run_code.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel_run_code.BackgroundImage")));
            this.panel_run_code.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel_run_code.Controls.Add(this.button_run_code);
            this.panel_run_code.Location = new System.Drawing.Point(12, 635);
            this.panel_run_code.Name = "panel_run_code";
            this.panel_run_code.Size = new System.Drawing.Size(139, 47);
            this.panel_run_code.TabIndex = 4;
            this.panel_run_code.Click += new System.EventHandler(this.panel_run_code_Click);
            // 
            // panel_stop_code
            // 
            this.panel_stop_code.BackColor = System.Drawing.Color.Transparent;
            this.panel_stop_code.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel_stop_code.BackgroundImage")));
            this.panel_stop_code.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel_stop_code.Controls.Add(this.button_stop_code);
            this.panel_stop_code.Location = new System.Drawing.Point(160, 635);
            this.panel_stop_code.Name = "panel_stop_code";
            this.panel_stop_code.Size = new System.Drawing.Size(139, 47);
            this.panel_stop_code.TabIndex = 5;
            this.panel_stop_code.Click += new System.EventHandler(this.panel_stop_code_Click);
            // 
            // panel_refresh
            // 
            this.panel_refresh.BackColor = System.Drawing.Color.Transparent;
            this.panel_refresh.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel_refresh.BackgroundImage")));
            this.panel_refresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel_refresh.Controls.Add(this.button_refresh);
            this.panel_refresh.Location = new System.Drawing.Point(305, 635);
            this.panel_refresh.Name = "panel_refresh";
            this.panel_refresh.Size = new System.Drawing.Size(139, 47);
            this.panel_refresh.TabIndex = 6;
            this.panel_refresh.Click += new System.EventHandler(this.panel_refresh_Click);
            // 
            // panel_open_new_file
            // 
            this.panel_open_new_file.BackColor = System.Drawing.Color.Transparent;
            this.panel_open_new_file.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel_open_new_file.BackgroundImage")));
            this.panel_open_new_file.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel_open_new_file.Controls.Add(this.button_open_new_file);
            this.panel_open_new_file.Location = new System.Drawing.Point(456, 635);
            this.panel_open_new_file.Name = "panel_open_new_file";
            this.panel_open_new_file.Size = new System.Drawing.Size(139, 47);
            this.panel_open_new_file.TabIndex = 7;
            this.panel_open_new_file.Click += new System.EventHandler(this.panel_open_new_file_Click);
            // 
            // panel_about
            // 
            this.panel_about.BackColor = System.Drawing.Color.Transparent;
            this.panel_about.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel_about.BackgroundImage")));
            this.panel_about.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel_about.Controls.Add(this.button_about);
            this.panel_about.Location = new System.Drawing.Point(450, 49);
            this.panel_about.Name = "panel_about";
            this.panel_about.Size = new System.Drawing.Size(139, 47);
            this.panel_about.TabIndex = 11;
            this.panel_about.Click += new System.EventHandler(this.panel_about_Click);
            this.panel_about.MouseEnter += new System.EventHandler(this.panel_about_MouseEnter);
            // 
            // button_about
            // 
            this.button_about.BackColor = System.Drawing.Color.Transparent;
            this.button_about.Image = ((System.Drawing.Image)(resources.GetObject("button_about.Image")));
            this.button_about.Location = new System.Drawing.Point(45, -1);
            this.button_about.Name = "button_about";
            this.button_about.Size = new System.Drawing.Size(48, 48);
            this.button_about.TabIndex = 2;
            this.button_about.TabStop = false;
            this.button_about.Click += new System.EventHandler(this.button_about_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSlateGray;
            this.panel1.Controls.Add(this.pictureBox3);
            this.panel1.Controls.Add(this.button_minimize);
            this.panel1.Controls.Add(this.button_exit);
            this.panel1.Location = new System.Drawing.Point(-17, -15);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1263, 49);
            this.panel1.TabIndex = 12;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(26, 14);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(35, 35);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 5;
            this.pictureBox3.TabStop = false;
            // 
            // button_minimize
            // 
            this.button_minimize.BackColor = System.Drawing.Color.Transparent;
            this.button_minimize.Image = ((System.Drawing.Image)(resources.GetObject("button_minimize.Image")));
            this.button_minimize.Location = new System.Drawing.Point(1180, 16);
            this.button_minimize.Name = "button_minimize";
            this.button_minimize.Size = new System.Drawing.Size(30, 30);
            this.button_minimize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.button_minimize.TabIndex = 4;
            this.button_minimize.TabStop = false;
            this.button_minimize.Click += new System.EventHandler(this.pictureBox2_Click);
            this.button_minimize.MouseEnter += new System.EventHandler(this.button_minimize_MouseEnter);
            this.button_minimize.MouseLeave += new System.EventHandler(this.button_minimize_MouseLeave);
            // 
            // button_exit
            // 
            this.button_exit.BackColor = System.Drawing.Color.Transparent;
            this.button_exit.Image = ((System.Drawing.Image)(resources.GetObject("button_exit.Image")));
            this.button_exit.Location = new System.Drawing.Point(1216, 16);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(30, 30);
            this.button_exit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.button_exit.TabIndex = 3;
            this.button_exit.TabStop = false;
            this.button_exit.Click += new System.EventHandler(this.pictureBox1_Click);
            this.button_exit.MouseEnter += new System.EventHandler(this.button_exit_MouseEnter);
            this.button_exit.MouseLeave += new System.EventHandler(this.button_exit_MouseLeave);
            // 
            // panel_save
            // 
            this.panel_save.BackColor = System.Drawing.Color.Transparent;
            this.panel_save.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel_save.BackgroundImage")));
            this.panel_save.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel_save.Controls.Add(this.button_save);
            this.panel_save.Location = new System.Drawing.Point(601, 635);
            this.panel_save.Name = "panel_save";
            this.panel_save.Size = new System.Drawing.Size(139, 47);
            this.panel_save.TabIndex = 9;
            this.panel_save.Visible = false;
            this.panel_save.Click += new System.EventHandler(this.panel_save_Click);
            // 
            // button_save
            // 
            this.button_save.BackColor = System.Drawing.Color.Transparent;
            this.button_save.Image = ((System.Drawing.Image)(resources.GetObject("button_save.Image")));
            this.button_save.Location = new System.Drawing.Point(45, -1);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(48, 48);
            this.button_save.TabIndex = 8;
            this.button_save.TabStop = false;
            this.button_save.Visible = false;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SlateGray;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1241, 696);
            this.Controls.Add(this.panel_save);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel_about);
            this.Controls.Add(this.panel_open_new_file);
            this.Controls.Add(this.panel_refresh);
            this.Controls.Add(this.panel_stop_code);
            this.Controls.Add(this.panel_run_code);
            this.Controls.Add(this.loadedFile);
            this.Controls.Add(this.panel_source);
            this.Controls.Add(this.panel_edit);
            this.Controls.Add(this.sizeManager_input);
            this.Controls.Add(this.panel_running);
            this.Controls.Add(this.sizeManager_console);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "PSGUI";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResizeBegin += new System.EventHandler(this.Form1_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.button_run_code)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_running)).EndInit();
            this.panel_running.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.button_stop_code)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_refresh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_open_new_file)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_edit)).EndInit();
            this.panel_edit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.button_source)).EndInit();
            this.panel_source.ResumeLayout(false);
            this.panel_run_code.ResumeLayout(false);
            this.panel_stop_code.ResumeLayout(false);
            this.panel_refresh.ResumeLayout(false);
            this.panel_open_new_file.ResumeLayout(false);
            this.panel_about.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.button_about)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_minimize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_exit)).EndInit();
            this.panel_save.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.button_save)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel sizeManager_console;
        private System.Windows.Forms.PictureBox button_run_code;
        private System.Windows.Forms.PictureBox button_running;
        private System.Windows.Forms.Panel panel_running;
        private System.Windows.Forms.Panel sizeManager_input;
        private System.Windows.Forms.PictureBox button_stop_code;
        private System.Windows.Forms.PictureBox button_refresh;
        private System.Windows.Forms.PictureBox button_open_new_file;
        private System.Windows.Forms.PictureBox button_edit;
        private System.Windows.Forms.Panel panel_edit;
        private System.Windows.Forms.PictureBox button_source;
        private System.Windows.Forms.Panel panel_source;
        private System.Windows.Forms.Label loadedFile;
        private System.Windows.Forms.Panel panel_run_code;
        private System.Windows.Forms.Panel panel_stop_code;
        private System.Windows.Forms.Panel panel_refresh;
        private System.Windows.Forms.Panel panel_open_new_file;
        private System.Windows.Forms.Panel panel_about;
        private System.Windows.Forms.PictureBox button_about;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox button_minimize;
        private System.Windows.Forms.PictureBox button_exit;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Panel panel_save;
        private System.Windows.Forms.PictureBox button_save;
    }
}

