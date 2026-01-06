namespace GitUI.CommandsDialogs
{
    partial class FormCustomizeToolbars
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
            this.labelAvailable = new System.Windows.Forms.Label();
            this.labelCurrent = new System.Windows.Forms.Label();
            this.listBoxAvailable = new System.Windows.Forms.ListBox();
            this.listBoxCurrent = new System.Windows.Forms.ListBox();
            this.buttonAddAll = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonMoveUp = new System.Windows.Forms.Button();
            this.buttonMoveDown = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonDefaults = new System.Windows.Forms.Button();
            this.comboBoxToolbar = new System.Windows.Forms.ComboBox();
            this.labelToolbar = new System.Windows.Forms.Label();
            this.textBoxFilterAvailable = new System.Windows.Forms.TextBox();
            this.textBoxFilterCurrent = new System.Windows.Forms.TextBox();
            this.buttonClearAvailableFilter = new System.Windows.Forms.Button();
            this.buttonClearCurrentFilter = new System.Windows.Forms.Button();
            this.buttonClearCurrent = new System.Windows.Forms.Button();
            this.buttonAddToolbar = new System.Windows.Forms.Button();
            this.buttonRemoveToolbar = new System.Windows.Forms.Button();
            this.comboBoxCategory = new System.Windows.Forms.ComboBox();
            this.labelPosition = new System.Windows.Forms.Label();
            this.buttonToolbarLayout = new System.Windows.Forms.Button();
            this.labelShow = new System.Windows.Forms.Label();
            this.comboBoxDisplayMode = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // labelAvailable
            // 
            this.labelAvailable.AutoSize = true;
            this.labelAvailable.Location = new System.Drawing.Point(12, 45);
            this.labelAvailable.Name = "labelAvailable";
            this.labelAvailable.Size = new System.Drawing.Size(100, 13);
            this.labelAvailable.TabIndex = 0;
            this.labelAvailable.Text = "Available actions:";
            // 
            // labelCurrent
            // 
            this.labelCurrent.AutoSize = true;
            this.labelCurrent.Location = new System.Drawing.Point(440, 45);
            this.labelCurrent.Name = "labelCurrent";
            this.labelCurrent.Size = new System.Drawing.Size(85, 13);
            this.labelCurrent.TabIndex = 1;
            this.labelCurrent.Text = "Current actions:";
            // 
            // textBoxFilterAvailable
            // 
            this.textBoxFilterAvailable.Location = new System.Drawing.Point(12, 70);
            this.textBoxFilterAvailable.Name = "textBoxFilterAvailable";
            this.textBoxFilterAvailable.PlaceholderText = "Filter...";
            this.textBoxFilterAvailable.Size = new System.Drawing.Size(320, 23);
            this.textBoxFilterAvailable.TabIndex = 2;
            this.textBoxFilterAvailable.TextChanged += new System.EventHandler(this.TextBoxFilterAvailable_TextChanged);
            // 
            // buttonClearAvailableFilter
            // 
            this.buttonClearAvailableFilter.Location = new System.Drawing.Point(338, 70);
            this.buttonClearAvailableFilter.Name = "buttonClearAvailableFilter";
            this.buttonClearAvailableFilter.Size = new System.Drawing.Size(25, 23);
            this.buttonClearAvailableFilter.TabIndex = 24;
            this.buttonClearAvailableFilter.Text = "✕";
            this.buttonClearAvailableFilter.UseVisualStyleBackColor = true;
            this.buttonClearAvailableFilter.Click += new System.EventHandler(this.ButtonClearAvailableFilter_Click);
            // 
            // listBoxAvailable
            // 
            this.listBoxAvailable.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxAvailable.FormattingEnabled = true;
            this.listBoxAvailable.ItemHeight = 24;
            this.listBoxAvailable.Location = new System.Drawing.Point(12, 100);
            this.listBoxAvailable.Name = "listBoxAvailable";
            this.listBoxAvailable.Size = new System.Drawing.Size(350, 355);
            this.listBoxAvailable.TabIndex = 3;
            this.listBoxAvailable.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ListBox_DrawItem);
            this.listBoxAvailable.DoubleClick += new System.EventHandler(this.ListBoxAvailable_DoubleClick);
            // 
            // textBoxFilterCurrent
            // 
            this.textBoxFilterCurrent.Location = new System.Drawing.Point(440, 70);
            this.textBoxFilterCurrent.Name = "textBoxFilterCurrent";
            this.textBoxFilterCurrent.PlaceholderText = "Filter...";
            this.textBoxFilterCurrent.Size = new System.Drawing.Size(320, 23);
            this.textBoxFilterCurrent.TabIndex = 4;
            this.textBoxFilterCurrent.TextChanged += new System.EventHandler(this.TextBoxFilterCurrent_TextChanged);
            // 
            // buttonClearCurrentFilter
            // 
            this.buttonClearCurrentFilter.Location = new System.Drawing.Point(765, 70);
            this.buttonClearCurrentFilter.Name = "buttonClearCurrentFilter";
            this.buttonClearCurrentFilter.Size = new System.Drawing.Size(25, 23);
            this.buttonClearCurrentFilter.TabIndex = 25;
            this.buttonClearCurrentFilter.Text = "✕";
            this.buttonClearCurrentFilter.UseVisualStyleBackColor = true;
            this.buttonClearCurrentFilter.Click += new System.EventHandler(this.ButtonClearCurrentFilter_Click);
            // 
            // listBoxCurrent
            // 
            this.listBoxCurrent.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxCurrent.FormattingEnabled = true;
            this.listBoxCurrent.ItemHeight = 24;
            this.listBoxCurrent.Location = new System.Drawing.Point(440, 100);
            this.listBoxCurrent.Name = "listBoxCurrent";
            this.listBoxCurrent.Size = new System.Drawing.Size(350, 355);
            this.listBoxCurrent.TabIndex = 5;
            this.listBoxCurrent.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ListBox_DrawItem);
            this.listBoxCurrent.DoubleClick += new System.EventHandler(this.ListBoxCurrent_DoubleClick);
            // 
            // buttonAddAll
            // 
            this.buttonAddAll.Image = global::GitUI.Properties.Images.ToolbarArrowAddAll;
            this.buttonAddAll.Location = new System.Drawing.Point(375, 101);
            this.buttonAddAll.Name = "buttonAddAll";
            this.buttonAddAll.Size = new System.Drawing.Size(50, 45);
            this.buttonAddAll.TabIndex = 28;
            this.buttonAddAll.UseVisualStyleBackColor = true;
            this.buttonAddAll.Click += new System.EventHandler(this.ButtonAddAll_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Image = global::GitUI.Properties.Images.ToolbarArrowRight;
            this.buttonAdd.Location = new System.Drawing.Point(375, 169);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(50, 45);
            this.buttonAdd.TabIndex = 6;
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.ButtonAdd_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Image = global::GitUI.Properties.Images.ToolbarArrowLeft;
            this.buttonRemove.Location = new System.Drawing.Point(375, 224);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(50, 45);
            this.buttonRemove.TabIndex = 7;
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.ButtonRemove_Click);
            // 
            // buttonMoveUp
            // 
            this.buttonMoveUp.Image = global::GitUI.Properties.Images.ToolbarArrowUp;
            this.buttonMoveUp.Location = new System.Drawing.Point(375, 279);
            this.buttonMoveUp.Name = "buttonMoveUp";
            this.buttonMoveUp.Size = new System.Drawing.Size(50, 45);
            this.buttonMoveUp.TabIndex = 8;
            this.buttonMoveUp.UseVisualStyleBackColor = true;
            this.buttonMoveUp.Click += new System.EventHandler(this.ButtonMoveUp_Click);
            // 
            // buttonMoveDown
            // 
            this.buttonMoveDown.Image = global::GitUI.Properties.Images.ToolbarArrowDown;
            this.buttonMoveDown.Location = new System.Drawing.Point(375, 334);
            this.buttonMoveDown.Name = "buttonMoveDown";
            this.buttonMoveDown.Size = new System.Drawing.Size(50, 45);
            this.buttonMoveDown.TabIndex = 9;
            this.buttonMoveDown.UseVisualStyleBackColor = true;
            this.buttonMoveDown.Click += new System.EventHandler(this.ButtonMoveDown_Click);
            // 
            // buttonClearCurrent
            // 
            this.buttonClearCurrent.Image = global::GitUI.Properties.Images.ToolbarCross;
            this.buttonClearCurrent.Location = new System.Drawing.Point(375, 389);
            this.buttonClearCurrent.Name = "buttonClearCurrent";
            this.buttonClearCurrent.Size = new System.Drawing.Size(50, 45);
            this.buttonClearCurrent.TabIndex = 16;
            this.buttonClearCurrent.UseVisualStyleBackColor = true;
            this.buttonClearCurrent.Click += new System.EventHandler(this.ButtonClearCurrent_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(548, 495);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 25);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(633, 495);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 25);
            this.buttonCancel.TabIndex = 11;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(718, 495);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 25);
            this.buttonApply.TabIndex = 12;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.ButtonApply_Click);
            // 
            // buttonDefaults
            // 
            this.buttonDefaults.Location = new System.Drawing.Point(12, 495);
            this.buttonDefaults.Name = "buttonDefaults";
            this.buttonDefaults.Size = new System.Drawing.Size(100, 25);
            this.buttonDefaults.TabIndex = 13;
            this.buttonDefaults.Text = "Defaults";
            this.buttonDefaults.UseVisualStyleBackColor = true;
            this.buttonDefaults.Click += new System.EventHandler(this.ButtonDefaults_Click);
            // 
            // comboBoxToolbar
            // 
            this.comboBoxToolbar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxToolbar.FormattingEnabled = true;
            this.comboBoxToolbar.Location = new System.Drawing.Point(80, 12);
            this.comboBoxToolbar.Name = "comboBoxToolbar";
            this.comboBoxToolbar.Size = new System.Drawing.Size(210, 23);
            this.comboBoxToolbar.TabIndex = 14;
            this.comboBoxToolbar.SelectedIndexChanged += new System.EventHandler(this.ComboBoxToolbar_SelectedIndexChanged);
            // 
            // buttonAddToolbar
            // 
            this.buttonAddToolbar.Image = global::GitUI.Properties.Images.RemoteAdd;
            this.buttonAddToolbar.Location = new System.Drawing.Point(297, 11);
            this.buttonAddToolbar.Name = "buttonAddToolbar";
            this.buttonAddToolbar.Size = new System.Drawing.Size(30, 25);
            this.buttonAddToolbar.TabIndex = 17;
            this.buttonAddToolbar.UseVisualStyleBackColor = true;
            this.buttonAddToolbar.Click += new System.EventHandler(this.ButtonAddToolbar_Click);
            // 
            // buttonRemoveToolbar
            // 
            this.buttonRemoveToolbar.Image = global::GitUI.Properties.Images.RemoteDelete;
            this.buttonRemoveToolbar.Location = new System.Drawing.Point(332, 11);
            this.buttonRemoveToolbar.Name = "buttonRemoveToolbar";
            this.buttonRemoveToolbar.Size = new System.Drawing.Size(30, 25);
            this.buttonRemoveToolbar.TabIndex = 18;
            this.buttonRemoveToolbar.UseVisualStyleBackColor = true;
            this.buttonRemoveToolbar.Click += new System.EventHandler(this.ButtonRemoveToolbar_Click);
            // 
            // labelToolbar
            // 
            this.labelToolbar.AutoSize = true;
            this.labelToolbar.Location = new System.Drawing.Point(12, 15);
            this.labelToolbar.Name = "labelToolbar";
            this.labelToolbar.Size = new System.Drawing.Size(50, 13);
            this.labelToolbar.TabIndex = 15;
            this.labelToolbar.Text = "Toolbar:";
            // 
            // comboBoxCategory
            // 
            this.comboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCategory.FormattingEnabled = true;
            this.comboBoxCategory.Location = new System.Drawing.Point(184, 42);
            this.comboBoxCategory.Name = "comboBoxCategory";
            this.comboBoxCategory.Size = new System.Drawing.Size(177, 21);
            this.comboBoxCategory.TabIndex = 23;
            this.comboBoxCategory.SelectedIndexChanged += new System.EventHandler(this.ComboBoxCategory_SelectedIndexChanged);
            // 
            // labelPosition
            // 
            this.labelPosition.AutoSize = true;
            this.labelPosition.Location = new System.Drawing.Point(440, 15);
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.Size = new System.Drawing.Size(120, 13);
            this.labelPosition.TabIndex = 19;
            this.labelPosition.Text = "Set toolbars positions:";
            // 
            // buttonToolbarLayout
            // 
            this.buttonToolbarLayout.Location = new System.Drawing.Point(580, 10);
            this.buttonToolbarLayout.Name = "buttonToolbarLayout";
            this.buttonToolbarLayout.Size = new System.Drawing.Size(120, 25);
            this.buttonToolbarLayout.TabIndex = 20;
            this.buttonToolbarLayout.Text = "Toolbars layout";
            this.buttonToolbarLayout.UseVisualStyleBackColor = true;
            this.buttonToolbarLayout.Click += new System.EventHandler(this.ButtonToolbarLayout_Click);
            // 
            // labelShow
            // 
            this.labelShow.AutoSize = true;
            this.labelShow.Location = new System.Drawing.Point(618, 463);
            this.labelShow.Name = "labelShow";
            this.labelShow.Size = new System.Drawing.Size(40, 13);
            this.labelShow.TabIndex = 26;
            this.labelShow.Text = "Show:";
            // 
            // comboBoxDisplayMode
            // 
            this.comboBoxDisplayMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDisplayMode.FormattingEnabled = true;
            this.comboBoxDisplayMode.Items.AddRange(new object[] {
            "Icons",
            "Icons and text"});
            this.comboBoxDisplayMode.Location = new System.Drawing.Point(663, 460);
            this.comboBoxDisplayMode.Name = "comboBoxDisplayMode";
            this.comboBoxDisplayMode.Size = new System.Drawing.Size(130, 21);
            this.comboBoxDisplayMode.TabIndex = 27;
            this.comboBoxDisplayMode.SelectedIndexChanged += new System.EventHandler(this.ComboBoxDisplayMode_SelectedIndexChanged);
            // 
            // 
            // FormCustomizeToolbars
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(803, 530);
            this.Controls.Add(this.comboBoxDisplayMode);
            this.Controls.Add(this.labelShow);
            this.Controls.Add(this.buttonToolbarLayout);
            this.Controls.Add(this.labelPosition);
            this.Controls.Add(this.buttonRemoveToolbar);
            this.Controls.Add(this.buttonAddToolbar);
            this.Controls.Add(this.labelToolbar);
            this.Controls.Add(this.comboBoxToolbar);
            this.Controls.Add(this.buttonDefaults);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonClearCurrent);
            this.Controls.Add(this.buttonMoveDown);
            this.Controls.Add(this.buttonMoveUp);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonAddAll);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.listBoxCurrent);
            this.Controls.Add(this.buttonClearCurrentFilter);
            this.Controls.Add(this.textBoxFilterCurrent);
            this.Controls.Add(this.listBoxAvailable);
            this.Controls.Add(this.buttonClearAvailableFilter);
            this.Controls.Add(this.textBoxFilterAvailable);
            this.Controls.Add(this.comboBoxCategory);
            this.Controls.Add(this.labelCurrent);
            this.Controls.Add(this.labelAvailable);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormCustomizeToolbars";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Customize Toolbars";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelAvailable;
        private System.Windows.Forms.Label labelCurrent;
        private System.Windows.Forms.TextBox textBoxFilterAvailable;
        private System.Windows.Forms.Button buttonClearAvailableFilter;
        private System.Windows.Forms.TextBox textBoxFilterCurrent;
        private System.Windows.Forms.Button buttonClearCurrentFilter;
        private System.Windows.Forms.ListBox listBoxAvailable;
        private System.Windows.Forms.ListBox listBoxCurrent;
        private System.Windows.Forms.Button buttonAddAll;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonMoveUp;
        private System.Windows.Forms.Button buttonMoveDown;
        private System.Windows.Forms.Button buttonClearCurrent;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonDefaults;
        private System.Windows.Forms.ComboBox comboBoxToolbar;
        private System.Windows.Forms.Button buttonAddToolbar;
        private System.Windows.Forms.Button buttonRemoveToolbar;
        private System.Windows.Forms.Label labelToolbar;
        private System.Windows.Forms.ComboBox comboBoxCategory;
        private System.Windows.Forms.Label labelPosition;
        private System.Windows.Forms.Button buttonToolbarLayout;
        private System.Windows.Forms.Label labelShow;
        private System.Windows.Forms.ComboBox comboBoxDisplayMode;
    }
}
