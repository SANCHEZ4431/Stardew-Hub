using System.Drawing.Drawing2D;
using System.Resources;

namespace SVLauncher
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnLaunch;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnLang;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.Button btnUpdateLauncher;
        private System.Windows.Forms.Button btnChangePath;
        private System.Windows.Forms.Button btnMenu;
        private System.Windows.Forms.RadioButton rbVanilla;
        private System.Windows.Forms.RadioButton rbMods;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Timer processCheckTimer;
        private System.Windows.Forms.Timer launchTimer;
        private System.Windows.Forms.GroupBox groupOptions;
        private System.Windows.Forms.Panel progressBg;
        private System.Windows.Forms.Panel progressFill;
        private System.Windows.Forms.PictureBox pbIcon;
        private System.Windows.Forms.Button btnDevMode;
        private System.Windows.Forms.Button btnToggleAnim;
        private System.Windows.Forms.Button btnResize;
        private System.Windows.Forms.Timer uiTimer;
        private float fade = 0f;
        private int glowPhase = 0;
        private int progressTarget = 0;
        private int gradientShift = 0;
        private class CyberStream
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Speed { get; set; }
            public int Length { get; set; }
            public int Opacity { get; set; }
        }

        private List<CyberStream> streams = new List<CyberStream>();
        private Random rnd = new Random();
        private int shimmerX = -100;
        private bool isAnimationsEnabled = true;
        private bool isLargeMode = false;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            topPanel = new Panel();
            pbIcon = new PictureBox();
            btnLang = new Button();
            btnConfig = new Button();
            btnUpdateLauncher = new Button();
            btnChangePath = new Button();
            btnClose = new Button();
            lblTitle = new Label();
            btnLaunch = new Button();
            btnUpdate = new Button();
            btnCancel = new Button();
            btnMenu = new Button();
            btnDevMode = new Button();
            btnToggleAnim = new Button();
            btnResize = new Button();
            rbVanilla = new RadioButton();
            rbMods = new RadioButton();
            lblStatus = new Label();
            Color buttonBg = Color.FromArgb(64, 64, 64);
            Color buttonHover = Color.FromArgb(90, 90, 90);
            Color win11Bg = Color.FromArgb(32, 32, 32);
            Color win11Panel = Color.FromArgb(40, 40, 40);
            Color win11Accent = Color.FromArgb(0, 120, 215);
            Color win11Text = Color.WhiteSmoke;
            Color win11Secondary = Color.FromArgb(60, 60, 60);
            processCheckTimer = new System.Windows.Forms.Timer(components);
            launchTimer = new System.Windows.Forms.Timer(components);
            groupOptions = new GroupBox();
            progressBg = new Panel();
            progressFill = new Panel();
            topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbIcon).BeginInit();
            groupOptions.SuspendLayout();
            progressBg.SuspendLayout();
            uiTimer = new System.Windows.Forms.Timer();
            uiTimer.Interval = 16;
            uiTimer.Tick += UiTimer_Tick;
            uiTimer.Start();
            btnConfig.Location = btnDevMode.Location;
            btnConfig.Size = btnDevMode.Size;
            this.Opacity = 0;
            SuspendLayout();

            pbIcon.BackColor = Color.Transparent;
            pbIcon.Image = Properties.Resources.icon;
            pbIcon.Location = new Point(16, 8);
            pbIcon.Name = "pbIcon";
            pbIcon.Size = new Size(32, 32);
            pbIcon.SizeMode = PictureBoxSizeMode.Zoom;
            pbIcon.TabStop = false;

            topPanel.BackColor = Color.FromArgb(40, 40, 40);
            topPanel.Controls.Add(pbIcon);
            topPanel.Controls.Add(btnLang);
            topPanel.Controls.Add(btnConfig);
            topPanel.Controls.Add(btnDevMode);
            topPanel.Controls.Add(btnUpdateLauncher);
            topPanel.Controls.Add(btnChangePath);
            topPanel.Controls.Add(btnClose);
            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(btnMenu);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(482, 45);
            topPanel.TabIndex = 4;
            topPanel.MouseDown += topPanel_MouseDown;

            this.btnToggleAnim = new System.Windows.Forms.Button();
            this.btnResize = new System.Windows.Forms.Button();
            SetupIconButton(btnToggleAnim, "✨", new Point(400, 90), buttonBg, buttonHover);
            this.btnToggleAnim.Size = new Size(30, 30);
            SetupIconButton(btnResize, "🔍", new Point(400, 130), buttonBg, buttonHover);
            this.btnResize.Size = new Size(30, 30);
            this.Controls.Add(this.btnToggleAnim);
            this.Controls.Add(this.btnResize);
            this.btnToggleAnim.BringToFront();
            this.btnResize.BringToFront();

            SetupIconButton(btnLang, "ENG", new Point(482 - 200, 8), buttonBg, buttonHover);
            SetupIconButton(btnConfig, "⚙", new Point(482 - 160, 8), buttonBg, buttonHover);
            SetupIconButton(btnDevMode, "🛠", new Point(482 - 160, 8), buttonBg, buttonHover);
            SetupIconButton(btnUpdateLauncher, "↻", new Point(482 - 120, 8), buttonBg, buttonHover);
            SetupIconButton(btnChangePath, "📁", new Point(482 - 80, 8), buttonBg, buttonHover);

            btnLang.BackColor = Color.FromArgb(50, 50, 50);
            btnLang.FlatAppearance.BorderSize = 0;
            btnLang.FlatStyle = FlatStyle.Flat;
            btnLang.Font = new Font("Segoe UI", 7F);
            btnLang.ForeColor = Color.White;
            btnLang.Location = new Point(253, 6);
            btnLang.Name = "btnLang";
            btnLang.Size = new Size(40, 32);
            btnLang.TabIndex = 1;
            btnLang.UseVisualStyleBackColor = false;
            btnLang.Click += btnLang_Click;

            btnConfig.BackColor = Color.FromArgb(50, 50, 50);
            btnConfig.FlatAppearance.BorderSize = 0;
            btnConfig.FlatStyle = FlatStyle.Flat;
            btnConfig.ForeColor = Color.White;
            btnConfig.Location = new Point(298, 6);
            btnConfig.Name = "btnConfig";
            btnConfig.Size = new Size(40, 32);
            btnConfig.TabIndex = 2;
            btnConfig.UseVisualStyleBackColor = false;
            btnConfig.Click += btnConfig_Click;

            btnUpdateLauncher.BackColor = Color.FromArgb(50, 50, 50);
            btnUpdateLauncher.FlatAppearance.BorderSize = 0;
            btnUpdateLauncher.FlatStyle = FlatStyle.Flat;
            btnUpdateLauncher.ForeColor = Color.White;
            btnUpdateLauncher.Location = new Point(343, 6);
            btnUpdateLauncher.Name = "btnUpdateLauncher";
            btnUpdateLauncher.Size = new Size(40, 32);
            btnUpdateLauncher.TabIndex = 3;
            btnUpdateLauncher.UseVisualStyleBackColor = false;
            btnUpdateLauncher.Click += btnUpdateLauncher_Click;

            btnChangePath.BackColor = Color.FromArgb(50, 50, 50);
            btnChangePath.FlatAppearance.BorderSize = 0;
            btnChangePath.FlatStyle = FlatStyle.Flat;
            btnChangePath.ForeColor = Color.White;
            btnChangePath.Location = new Point(388, 6);
            btnChangePath.Name = "btnChangePath";
            btnChangePath.Size = new Size(40, 32);
            btnChangePath.TabIndex = 4;
            btnChangePath.UseVisualStyleBackColor = false;
            btnChangePath.Click += btnChangePath_Click;

            btnClose.Dock = DockStyle.Right;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(437, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(45, 45);
            btnClose.TabIndex = 5;
            btnClose.Text = "✕";
            btnClose.Click += btnClose_Click;

            btnDevMode.Name = "btnDevMode";
            btnDevMode.Click += this.btnDevMode_Click;
            btnDevMode.Location = new Point(298, 6);
            btnDevMode.Size = new Size(40, 32);
            btnDevMode.BackColor = Color.FromArgb(50, 50, 50);
            btnDevMode.FlatAppearance.BorderSize = 0;
            btnDevMode.FlatStyle = FlatStyle.Flat;
            btnDevMode.ForeColor = Color.White;
            btnDevMode.UseVisualStyleBackColor = false;

            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(56, 11);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(123, 21);
            lblTitle.TabIndex = 6;
            lblTitle.Text = "STARDEW HUB";

            btnMenu.BackColor = Color.FromArgb(50, 50, 50);
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.FlatStyle = FlatStyle.Flat;
            btnMenu.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnMenu.ForeColor = Color.White;
            btnMenu.Location = new Point(390, 6);
            btnMenu.Name = "btnMenu";
            btnMenu.Size = new Size(40, 32);
            btnMenu.TabIndex = 7;
            btnMenu.Text = "☰";
            btnMenu.UseVisualStyleBackColor = false;
            btnMenu.Click += btnMenu_Click;

            btnLaunch.BackColor = Color.FromArgb(0, 120, 215);
            btnLaunch.FlatAppearance.BorderSize = 0;
            btnLaunch.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 190);
            btnLaunch.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 140, 255);
            btnLaunch.FlatStyle = FlatStyle.Flat;
            btnLaunch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLaunch.ForeColor = Color.White;
            btnLaunch.Location = new Point(40, 232);
            btnLaunch.Name = "btnLaunch";
            btnLaunch.Size = new Size(160, 40);
            btnLaunch.TabIndex = 7;
            btnLaunch.Text = "ИГРАТЬ";
            btnLaunch.UseVisualStyleBackColor = false;
            btnLaunch.Click += btnLaunch_Click;

            btnUpdate.BackColor = Color.FromArgb(60, 60, 60);
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 80);
            btnUpdate.FlatStyle = FlatStyle.Flat;
            btnUpdate.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnUpdate.ForeColor = Color.White;
            btnUpdate.Location = new Point(280, 232);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(160, 40);
            btnUpdate.TabIndex = 8;
            btnUpdate.Text = "ОБНОВИТЬ";
            btnUpdate.UseVisualStyleBackColor = false;
            btnUpdate.Click += btnUpdate_Click;

            btnCancel.BackColor = Color.FromArgb(180, 60, 60);
            btnCancel.Enabled = false;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 80, 80);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(280, 232);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(160, 40);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "ОТМЕНА";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Visible = false;
            btnCancel.Click += btnCancel_Click;

            rbVanilla.Checked = true;
            rbVanilla.Font = new Font("Segoe UI", 9F);
            rbVanilla.ForeColor = Color.WhiteSmoke;
            rbVanilla.Location = new Point(20, 30);
            rbVanilla.Name = "rbVanilla";
            rbVanilla.Size = new Size(200, 24);
            rbVanilla.TabIndex = 0;
            rbVanilla.TabStop = true;
            rbVanilla.Text = "Оригинальная версия";

            rbMods.Font = new Font("Segoe UI", 9F);
            rbMods.ForeColor = Color.WhiteSmoke;
            rbMods.Location = new Point(20, 60);
            rbMods.Name = "rbMods";
            rbMods.Size = new Size(200, 24);
            rbMods.TabIndex = 1;
            rbMods.Text = "SMAPI (с модами)";

            lblStatus.BackColor = Color.Transparent;
            lblStatus.ForeColor = Color.White;
            lblStatus.Location = new Point(40, 192);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(400, 23);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Статус: Готов";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;

            processCheckTimer.Enabled = true;
            processCheckTimer.Interval = 1500;
            processCheckTimer.Tick += processCheckTimer_Tick;

            launchTimer.Interval = 30;
            launchTimer.Tick += launchTimer_Tick;

            groupOptions.BackColor = Color.FromArgb(40, 40, 40);
            groupOptions.Controls.Add(rbVanilla);
            groupOptions.Controls.Add(rbMods);
            groupOptions.ForeColor = Color.WhiteSmoke;
            groupOptions.Location = new Point(40, 70);
            groupOptions.Name = "groupOptions";
            groupOptions.Size = new Size(400, 100);
            groupOptions.TabIndex = 4;
            groupOptions.TabStop = false;
            groupOptions.Text = " РЕЖИМ ЗАПУСКА ";

            progressBg.BackColor = Color.FromArgb(80, 80, 80);
            progressBg.Controls.Add(progressFill);
            progressBg.Location = new Point(40, 185);
            progressBg.Name = "progressBg";
            progressBg.Size = new Size(400, 4);
            progressBg.TabIndex = 2;

            progressFill.BackColor = Color.FromArgb(0, 120, 215);
            progressFill.Location = new Point(0, 0);
            progressFill.Name = "progressFill";
            progressFill.Size = new Size(0, 4);
            progressFill.TabIndex = 0;
            progressFill.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(0, 140, 255)))
                    g.FillRectangle(b, progressFill.ClientRectangle);
                Rectangle shimmerRect = new Rectangle(shimmerX, 0, 60, progressFill.Height);

                using (LinearGradientBrush lb = new LinearGradientBrush(
                    shimmerRect,
                    Color.FromArgb(0, Color.White),
                    Color.FromArgb(180, Color.White),
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(lb, shimmerRect);
                }
            };

            BackColor = Color.FromArgb(32, 32, 32);
            ClientSize = new Size(482, 292);
            Controls.Add(btnCancel);
            Controls.Add(btnUpdate);
            Controls.Add(progressBg);
            Controls.Add(lblStatus);
            Controls.Add(btnLaunch);
            Controls.Add(groupOptions);
            Controls.Add(topPanel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbIcon).EndInit();
            groupOptions.ResumeLayout(false);
            progressBg.ResumeLayout(false);
            ResumeLayout(false);

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Рисуем базовый фиолетовый цвет
            
            using (SolidBrush baseBrush = new SolidBrush(Color.FromArgb(30, 15, 45))) // Темно-фиолетовый
            {
                e.Graphics.FillRectangle(baseBrush, this.ClientRectangle);
            }

            // Создаем эффект "плавающего" градиентного пятна
            // Используем ваши переменные gradientShift и waveOffset для анимации
            float xOffset = (float)Math.Sin(waveOffset) * 50;
            float yOffset = (float)Math.Cos(waveOffset * 0.5f) * 30;

            using (LinearGradientBrush lgb = new LinearGradientBrush(
                new PointF(-50 + xOffset, -50 + yOffset),
                new PointF(this.Width + xOffset, this.Height + yOffset),
                Color.FromArgb(45, 25, 70), // Светло-фиолетовый
                Color.FromArgb(20, 10, 35))) // Почти черный фиолетовый
            {
                e.Graphics.FillRectangle(lgb, this.ClientRectangle);
            }
        }

        private void SetupIconButton(Button btn, string text, Point loc, Color bg, Color hover)
        {
            btn.BackColor = bg;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = hover;
            btn.ForeColor = Color.WhiteSmoke;
            btn.Location = loc;
            btn.Size = new Size(36, 36);
            btn.Text = text;
            btn.Font = new Font("Segoe Fluent Icons", 12F);
            btn.UseVisualStyleBackColor = false;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control is Button btn)
            {
                btn.MouseEnter += (s, ev) =>
                {
                    btn.ForeColor = Color.White;
                    btn.BackColor = ControlPaint.Light(btn.BackColor, 0.2f);
                };
                btn.MouseLeave += (s, ev) =>
                {
                    btn.ForeColor = Color.White;
                };
            }
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized) return;
            if (fade < 1f)
            {
                fade += 0.04f;
                this.Opacity = fade;
            }
            glowPhase += 3;
            if (glowPhase > 360) glowPhase = 0;
            int glow = (int)(Math.Sin(glowPhase * Math.PI / 180) * 40 + 80);
            if (progressFill.Width < progressTarget)
                progressFill.Width += 4;
            btnLaunch.BackColor = Color.FromArgb(0, glow, 255);
            btnLaunch.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, glow + 30, 255);
            
            gradientShift += 1;
            colorOffset += 0.02f;
            waveOffset += 0.01f; // Немного увеличим скорость для наглядности

            // Заставляет форму перерисовать фон
            // Вызываем перерисовку только если окно активно
            if (this.WindowState != FormWindowState.Minimized)
            {
                this.Invalidate();
            }
            progressFill.Invalidate();
        }

        public void SetProgress(int percent)
        {
            percent = Math.Max(0, Math.Min(100, percent));
            progressTarget = (progressBg.Width * percent) / 100;
        }
    }
}
