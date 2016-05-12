namespace GraphsFromExcel
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.interpolChart1 = new GraphsFromExcel.InterpolChart();
            this.SuspendLayout();
            // 
            // interpolChart1
            // 
            this.interpolChart1.AutoSize = true;
            this.interpolChart1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.interpolChart1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.interpolChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.interpolChart1.Location = new System.Drawing.Point(0, 0);
            this.interpolChart1.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.interpolChart1.Name = "interpolChart1";
            this.interpolChart1.Size = new System.Drawing.Size(1140, 851);
            this.interpolChart1.TabIndex = 2;
            this.interpolChart1.Load += new System.EventHandler(this.interpolChart1_Load);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 851);
            this.Controls.Add(this.interpolChart1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private InterpolChart interpolChart1;
    }
}

