namespace bdmanager.src.ProxyTest {
  partial class ProxyTestSettingsForm {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
            this.delayGroupBox = new System.Windows.Forms.GroupBox();
            this.delayNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.requestsCountGroupBox = new System.Windows.Forms.GroupBox();
            this.requestsCountNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.fullLogCheckBox = new System.Windows.Forms.CheckBox();
            this.domainsGroupBox = new System.Windows.Forms.GroupBox();
            this.customDomainsRichTextBox = new System.Windows.Forms.RichTextBox();
            this.customDomainsCheckBox = new System.Windows.Forms.CheckBox();
            this.customDomainsGroupBox = new System.Windows.Forms.GroupBox();
            this.commandsGroupBox = new System.Windows.Forms.GroupBox();
            this.customCommandsRichTextBox = new System.Windows.Forms.RichTextBox();
            this.customCommandsCheckBox = new System.Windows.Forms.CheckBox();
            this.customCommandsGroupBox = new System.Windows.Forms.GroupBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.delayGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.delayNumericUpDown)).BeginInit();
            this.requestsCountGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.requestsCountNumericUpDown)).BeginInit();
            this.domainsGroupBox.SuspendLayout();
            this.customDomainsGroupBox.SuspendLayout();
            this.commandsGroupBox.SuspendLayout();
            this.customCommandsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // delayGroupBox
            // 
            this.delayGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.delayGroupBox.Controls.Add(this.delayNumericUpDown);
            this.delayGroupBox.Location = new System.Drawing.Point(12, 12);
            this.delayGroupBox.Name = "delayGroupBox";
            this.delayGroupBox.Size = new System.Drawing.Size(448, 42);
            this.delayGroupBox.TabIndex = 0;
            this.delayGroupBox.TabStop = false;
            this.delayGroupBox.Text = "Ожидание между командами в секундах";
            // 
            // delayNumericUpDown
            // 
            this.delayNumericUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.delayNumericUpDown.Location = new System.Drawing.Point(3, 16);
            this.delayNumericUpDown.Name = "delayNumericUpDown";
            this.delayNumericUpDown.Size = new System.Drawing.Size(442, 20);
            this.delayNumericUpDown.TabIndex = 0;
            // 
            // requestsCountGroupBox
            // 
            this.requestsCountGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.requestsCountGroupBox.Controls.Add(this.requestsCountNumericUpDown);
            this.requestsCountGroupBox.Location = new System.Drawing.Point(12, 60);
            this.requestsCountGroupBox.Name = "requestsCountGroupBox";
            this.requestsCountGroupBox.Size = new System.Drawing.Size(448, 42);
            this.requestsCountGroupBox.TabIndex = 1;
            this.requestsCountGroupBox.TabStop = false;
            this.requestsCountGroupBox.Text = "Количество запросов к домену";
            // 
            // requestsCountNumericUpDown
            // 
            this.requestsCountNumericUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.requestsCountNumericUpDown.Location = new System.Drawing.Point(3, 16);
            this.requestsCountNumericUpDown.Name = "requestsCountNumericUpDown";
            this.requestsCountNumericUpDown.Size = new System.Drawing.Size(442, 20);
            this.requestsCountNumericUpDown.TabIndex = 0;
            // 
            // fullLogCheckBox
            // 
            this.fullLogCheckBox.AutoSize = true;
            this.fullLogCheckBox.Location = new System.Drawing.Point(12, 108);
            this.fullLogCheckBox.Name = "fullLogCheckBox";
            this.fullLogCheckBox.Size = new System.Drawing.Size(249, 17);
            this.fullLogCheckBox.TabIndex = 2;
            this.fullLogCheckBox.Text = "Расширенный лог с выводом ответа хостов";
            this.fullLogCheckBox.UseVisualStyleBackColor = true;
            // 
            // domainsGroupBox
            // 
            this.domainsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.domainsGroupBox.Controls.Add(this.customDomainsGroupBox);
            this.domainsGroupBox.Controls.Add(this.customDomainsCheckBox);
            this.domainsGroupBox.Location = new System.Drawing.Point(12, 131);
            this.domainsGroupBox.Name = "domainsGroupBox";
            this.domainsGroupBox.Size = new System.Drawing.Size(448, 180);
            this.domainsGroupBox.TabIndex = 3;
            this.domainsGroupBox.TabStop = false;
            this.domainsGroupBox.Text = "Домены";
            // 
            // customDomainsRichTextBox
            // 
            this.customDomainsRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customDomainsRichTextBox.Location = new System.Drawing.Point(3, 16);
            this.customDomainsRichTextBox.Name = "customDomainsRichTextBox";
            this.customDomainsRichTextBox.Size = new System.Drawing.Size(430, 113);
            this.customDomainsRichTextBox.TabIndex = 4;
            this.customDomainsRichTextBox.Text = "";
            // 
            // customDomainsCheckBox
            // 
            this.customDomainsCheckBox.AutoSize = true;
            this.customDomainsCheckBox.Location = new System.Drawing.Point(6, 19);
            this.customDomainsCheckBox.Name = "customDomainsCheckBox";
            this.customDomainsCheckBox.Size = new System.Drawing.Size(212, 17);
            this.customDomainsCheckBox.TabIndex = 5;
            this.customDomainsCheckBox.Text = "Использовать свой список доменов";
            this.customDomainsCheckBox.UseVisualStyleBackColor = true;
            // 
            // customDomainsGroupBox
            // 
            this.customDomainsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customDomainsGroupBox.Controls.Add(this.customDomainsRichTextBox);
            this.customDomainsGroupBox.Location = new System.Drawing.Point(6, 42);
            this.customDomainsGroupBox.Name = "customDomainsGroupBox";
            this.customDomainsGroupBox.Size = new System.Drawing.Size(436, 132);
            this.customDomainsGroupBox.TabIndex = 6;
            this.customDomainsGroupBox.TabStop = false;
            this.customDomainsGroupBox.Text = "Список доменов";
            // 
            // commandsGroupBox
            // 
            this.commandsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commandsGroupBox.Controls.Add(this.customCommandsGroupBox);
            this.commandsGroupBox.Controls.Add(this.customCommandsCheckBox);
            this.commandsGroupBox.Location = new System.Drawing.Point(12, 317);
            this.commandsGroupBox.Name = "commandsGroupBox";
            this.commandsGroupBox.Size = new System.Drawing.Size(448, 180);
            this.commandsGroupBox.TabIndex = 4;
            this.commandsGroupBox.TabStop = false;
            this.commandsGroupBox.Text = "Команды";
            // 
            // customCommandsRichTextBox
            // 
            this.customCommandsRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customCommandsRichTextBox.Location = new System.Drawing.Point(3, 16);
            this.customCommandsRichTextBox.Name = "customCommandsRichTextBox";
            this.customCommandsRichTextBox.Size = new System.Drawing.Size(430, 113);
            this.customCommandsRichTextBox.TabIndex = 0;
            this.customCommandsRichTextBox.Text = "";
            // 
            // customCommandsCheckBox
            // 
            this.customCommandsCheckBox.AutoSize = true;
            this.customCommandsCheckBox.Location = new System.Drawing.Point(6, 19);
            this.customCommandsCheckBox.Name = "customCommandsCheckBox";
            this.customCommandsCheckBox.Size = new System.Drawing.Size(206, 17);
            this.customCommandsCheckBox.TabIndex = 1;
            this.customCommandsCheckBox.Text = "Использовать свой список команд";
            this.customCommandsCheckBox.UseVisualStyleBackColor = true;
            // 
            // customCommandsGroupBox
            // 
            this.customCommandsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customCommandsGroupBox.Controls.Add(this.customCommandsRichTextBox);
            this.customCommandsGroupBox.Location = new System.Drawing.Point(6, 42);
            this.customCommandsGroupBox.Name = "customCommandsGroupBox";
            this.customCommandsGroupBox.Size = new System.Drawing.Size(436, 132);
            this.customCommandsGroupBox.TabIndex = 2;
            this.customCommandsGroupBox.TabStop = false;
            this.customCommandsGroupBox.Text = "Список команд";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(385, 509);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Отмена";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.applyButton.Location = new System.Drawing.Point(304, 509);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 6;
            this.applyButton.Text = "Применить";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // ProxyTestSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(472, 544);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.commandsGroupBox);
            this.Controls.Add(this.domainsGroupBox);
            this.Controls.Add(this.fullLogCheckBox);
            this.Controls.Add(this.requestsCountGroupBox);
            this.Controls.Add(this.delayGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ProxyTestSettingsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройки подбора команд";
            this.delayGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.delayNumericUpDown)).EndInit();
            this.requestsCountGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.requestsCountNumericUpDown)).EndInit();
            this.domainsGroupBox.ResumeLayout(false);
            this.domainsGroupBox.PerformLayout();
            this.customDomainsGroupBox.ResumeLayout(false);
            this.commandsGroupBox.ResumeLayout(false);
            this.commandsGroupBox.PerformLayout();
            this.customCommandsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox delayGroupBox;
    private System.Windows.Forms.NumericUpDown delayNumericUpDown;
    private System.Windows.Forms.GroupBox requestsCountGroupBox;
    private System.Windows.Forms.NumericUpDown requestsCountNumericUpDown;
    private System.Windows.Forms.CheckBox fullLogCheckBox;
    private System.Windows.Forms.GroupBox domainsGroupBox;
    private System.Windows.Forms.RichTextBox customDomainsRichTextBox;
    private System.Windows.Forms.CheckBox customDomainsCheckBox;
    private System.Windows.Forms.GroupBox customDomainsGroupBox;
    private System.Windows.Forms.GroupBox commandsGroupBox;
    private System.Windows.Forms.RichTextBox customCommandsRichTextBox;
    private System.Windows.Forms.CheckBox customCommandsCheckBox;
    private System.Windows.Forms.GroupBox customCommandsGroupBox;
    private System.Windows.Forms.Button cancelButton;
    private System.Windows.Forms.Button applyButton;
  }
}
