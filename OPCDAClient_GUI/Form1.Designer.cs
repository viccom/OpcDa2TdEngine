﻿namespace OPCDAClient_GUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            groupBox1 = new GroupBox();
            groupBox6 = new GroupBox();
            logsText = new RichTextBox();
            groupBox5 = new GroupBox();
            tdengine_bt = new Button();
            tdengine_run = new Label();
            label9 = new Label();
            groupBox4 = new GroupBox();
            opcda_bt = new Button();
            opcda_run = new Label();
            label8 = new Label();
            tabPage2 = new TabPage();
            dataView = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewTextBoxColumn();
            tabPage3 = new TabPage();
            button7 = new Button();
            upload_bt = new Button();
            button5 = new Button();
            button4 = new Button();
            button3 = new Button();
            groupBox7 = new GroupBox();
            tagsTable = new DataGridView();
            Column7 = new DataGridViewTextBoxColumn();
            Column8 = new DataGridViewTextBoxColumn();
            Column9 = new DataGridViewTextBoxColumn();
            Column10 = new DataGridViewTextBoxColumn();
            groupBox3 = new GroupBox();
            textBox7 = new TextBox();
            label13 = new Label();
            textBox6 = new TextBox();
            label12 = new Label();
            textBox5 = new TextBox();
            label6 = new Label();
            textBox4 = new TextBox();
            label5 = new Label();
            textBox3 = new TextBox();
            label4 = new Label();
            groupBox2 = new GroupBox();
            label3 = new Label();
            textBox2 = new TextBox();
            label2 = new Label();
            textBox1 = new TextBox();
            label1 = new Label();
            Inst_install = new CheckBox();
            label14 = new Label();
            Inst_run = new CheckBox();
            restart = new Button();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox6.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox4.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataView).BeginInit();
            tabPage3.SuspendLayout();
            groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tagsTable).BeginInit();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            tabControl1.Location = new Point(0, 50);
            tabControl1.Margin = new Padding(4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1009, 687);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(groupBox1);
            tabPage1.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            tabPage1.Location = new Point(4, 30);
            tabPage1.Margin = new Padding(4);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4);
            tabPage1.Size = new Size(1001, 653);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "概览";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(groupBox6);
            groupBox1.Controls.Add(groupBox5);
            groupBox1.Controls.Add(groupBox4);
            groupBox1.Location = new Point(8, 17);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(985, 627);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "后台服务状态";
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(logsText);
            groupBox6.Location = new Point(27, 159);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new Size(933, 446);
            groupBox6.TabIndex = 4;
            groupBox6.TabStop = false;
            groupBox6.Text = "程序日志：";
            // 
            // logsText
            // 
            logsText.Location = new Point(6, 27);
            logsText.Name = "logsText";
            logsText.Size = new Size(921, 413);
            logsText.TabIndex = 0;
            logsText.Text = "";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(tdengine_bt);
            groupBox5.Controls.Add(tdengine_run);
            groupBox5.Controls.Add(label9);
            groupBox5.Location = new Point(519, 27);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(441, 113);
            groupBox5.TabIndex = 3;
            groupBox5.TabStop = false;
            groupBox5.Text = "TdEngine写数服务";
            // 
            // tdengine_bt
            // 
            tdengine_bt.Location = new Point(310, 39);
            tdengine_bt.Name = "tdengine_bt";
            tdengine_bt.Size = new Size(90, 38);
            tdengine_bt.TabIndex = 2;
            tdengine_bt.Text = "button2";
            tdengine_bt.UseVisualStyleBackColor = true;
            tdengine_bt.Click += tdengine_bt_Click;
            // 
            // tdengine_run
            // 
            tdengine_run.AutoSize = true;
            tdengine_run.ForeColor = SystemColors.ButtonHighlight;
            tdengine_run.Location = new Point(165, 48);
            tdengine_run.Name = "tdengine_run";
            tdengine_run.Size = new Size(64, 21);
            tdengine_run.TabIndex = 1;
            tdengine_run.Text = "label11";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(25, 48);
            label9.Name = "label9";
            label9.Size = new Size(90, 21);
            label9.TabIndex = 0;
            label9.Text = "服务状态：";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(opcda_bt);
            groupBox4.Controls.Add(opcda_run);
            groupBox4.Controls.Add(label8);
            groupBox4.Location = new Point(27, 27);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(441, 113);
            groupBox4.TabIndex = 0;
            groupBox4.TabStop = false;
            groupBox4.Text = "OPC采集服务";
            // 
            // opcda_bt
            // 
            opcda_bt.Location = new Point(308, 39);
            opcda_bt.Name = "opcda_bt";
            opcda_bt.Size = new Size(90, 38);
            opcda_bt.TabIndex = 2;
            opcda_bt.Text = "button1";
            opcda_bt.UseVisualStyleBackColor = true;
            opcda_bt.Click += opcda_bt_Click;
            // 
            // opcda_run
            // 
            opcda_run.AutoSize = true;
            opcda_run.ForeColor = SystemColors.ButtonHighlight;
            opcda_run.Location = new Point(155, 48);
            opcda_run.Name = "opcda_run";
            opcda_run.Size = new Size(64, 21);
            opcda_run.TabIndex = 1;
            opcda_run.Text = "label10";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(31, 48);
            label8.Name = "label8";
            label8.Size = new Size(90, 21);
            label8.TabIndex = 0;
            label8.Text = "服务状态：";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(dataView);
            tabPage2.Location = new Point(4, 30);
            tabPage2.Margin = new Padding(4);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(4);
            tabPage2.Size = new Size(1001, 653);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "数据";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataView
            // 
            dataView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataView.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5, Column6 });
            dataView.Location = new Point(-4, 0);
            dataView.Name = "dataView";
            dataView.Size = new Size(1003, 650);
            dataView.TabIndex = 0;
            // 
            // Column1
            // 
            Column1.FillWeight = 200F;
            Column1.HeaderText = "名称";
            Column1.Name = "Column1";
            Column1.Width = 200;
            // 
            // Column2
            // 
            Column2.FillWeight = 200F;
            Column2.HeaderText = "描述";
            Column2.Name = "Column2";
            Column2.Width = 200;
            // 
            // Column3
            // 
            Column3.HeaderText = "类型";
            Column3.Name = "Column3";
            // 
            // Column4
            // 
            Column4.FillWeight = 200F;
            Column4.HeaderText = "时间";
            Column4.Name = "Column4";
            Column4.Width = 200;
            // 
            // Column5
            // 
            Column5.FillWeight = 160F;
            Column5.HeaderText = "数值";
            Column5.Name = "Column5";
            Column5.Width = 160;
            // 
            // Column6
            // 
            Column6.HeaderText = "质量";
            Column6.Name = "Column6";
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(button7);
            tabPage3.Controls.Add(upload_bt);
            tabPage3.Controls.Add(button5);
            tabPage3.Controls.Add(button4);
            tabPage3.Controls.Add(button3);
            tabPage3.Controls.Add(groupBox7);
            tabPage3.Controls.Add(groupBox3);
            tabPage3.Controls.Add(groupBox2);
            tabPage3.Location = new Point(4, 30);
            tabPage3.Margin = new Padding(4);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(1001, 653);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "配置";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            button7.Location = new Point(873, 262);
            button7.Name = "button7";
            button7.Size = new Size(95, 34);
            button7.TabIndex = 7;
            button7.Text = "重置点表";
            button7.UseVisualStyleBackColor = true;
            // 
            // upload_bt
            // 
            upload_bt.Location = new Point(601, 262);
            upload_bt.Name = "upload_bt";
            upload_bt.Size = new Size(95, 34);
            upload_bt.TabIndex = 6;
            upload_bt.Text = "上传点表";
            upload_bt.UseVisualStyleBackColor = true;
            upload_bt.Click += Upload_bt_Click;
            // 
            // button5
            // 
            button5.Location = new Point(737, 262);
            button5.Name = "button5";
            button5.Size = new Size(95, 34);
            button5.TabIndex = 5;
            button5.Text = "保存点表";
            button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(154, 262);
            button4.Name = "button4";
            button4.Size = new Size(95, 34);
            button4.TabIndex = 4;
            button4.Text = "重置配置";
            button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(34, 262);
            button3.Name = "button3";
            button3.Size = new Size(95, 34);
            button3.TabIndex = 3;
            button3.Text = "保存配置";
            button3.UseVisualStyleBackColor = true;
            // 
            // groupBox7
            // 
            groupBox7.Controls.Add(tagsTable);
            groupBox7.Location = new Point(8, 305);
            groupBox7.Name = "groupBox7";
            groupBox7.Size = new Size(985, 348);
            groupBox7.TabIndex = 2;
            groupBox7.TabStop = false;
            groupBox7.Text = "点表：";
            // 
            // tagsTable
            // 
            tagsTable.AllowUserToAddRows = false;
            tagsTable.AllowUserToDeleteRows = false;
            tagsTable.AllowUserToOrderColumns = true;
            tagsTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            tagsTable.Columns.AddRange(new DataGridViewColumn[] { Column7, Column8, Column9, Column10 });
            tagsTable.Location = new Point(26, 27);
            tagsTable.Name = "tagsTable";
            tagsTable.Size = new Size(934, 312);
            tagsTable.TabIndex = 0;
            // 
            // Column7
            // 
            Column7.HeaderText = "名称";
            Column7.Name = "Column7";
            Column7.Width = 200;
            // 
            // Column8
            // 
            Column8.HeaderText = "描述";
            Column8.Name = "Column8";
            Column8.Width = 200;
            // 
            // Column9
            // 
            Column9.HeaderText = "类型";
            Column9.Name = "Column9";
            // 
            // Column10
            // 
            Column10.HeaderText = "OPC Item";
            Column10.Name = "Column10";
            Column10.Width = 400;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(textBox7);
            groupBox3.Controls.Add(label13);
            groupBox3.Controls.Add(textBox6);
            groupBox3.Controls.Add(label12);
            groupBox3.Controls.Add(textBox5);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(textBox4);
            groupBox3.Controls.Add(label5);
            groupBox3.Controls.Add(textBox3);
            groupBox3.Controls.Add(label4);
            groupBox3.Location = new Point(8, 103);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(985, 149);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "TDEngine 数据库：";
            // 
            // textBox7
            // 
            textBox7.Location = new Point(546, 95);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(414, 28);
            textBox7.TabIndex = 9;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(446, 98);
            label13.Name = "label13";
            label13.Size = new Size(98, 21);
            label13.TabIndex = 8;
            label13.Text = "Password：";
            // 
            // textBox6
            // 
            textBox6.Location = new Point(122, 95);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(271, 28);
            textBox6.TabIndex = 7;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(13, 98);
            label12.Name = "label12";
            label12.Size = new Size(106, 21);
            label12.TabIndex = 6;
            label12.Text = "UserName：";
            // 
            // textBox5
            // 
            textBox5.Location = new Point(747, 40);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(213, 28);
            textBox5.TabIndex = 5;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(661, 44);
            label6.Name = "label6";
            label6.Size = new Size(82, 21);
            label6.TabIndex = 4;
            label6.Text = "DbName:";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(546, 40);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(100, 28);
            textBox4.TabIndex = 3;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(486, 44);
            label5.Name = "label5";
            label5.Size = new Size(58, 21);
            label5.TabIndex = 2;
            label5.Text = "Port：";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(123, 40);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(270, 28);
            textBox3.TabIndex = 1;
            textBox3.Text = "locahost";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(59, 44);
            label4.Name = "label4";
            label4.Size = new Size(61, 21);
            label4.TabIndex = 0;
            label4.Text = "Host：";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(textBox2);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(textBox1);
            groupBox2.Location = new Point(8, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(985, 85);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "OPC DA 服务器：";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(464, 42);
            label3.Name = "label3";
            label3.Size = new Size(79, 21);
            label3.TabIndex = 3;
            label3.Text = "ProgID：";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(546, 38);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(414, 28);
            textBox2.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(59, 38);
            label2.Name = "label2";
            label2.Size = new Size(61, 21);
            label2.TabIndex = 1;
            label2.Text = "Host：";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(122, 34);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(271, 28);
            textBox1.TabIndex = 0;
            textBox1.Text = "localhost";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label1.Location = new Point(461, 16);
            label1.Name = "label1";
            label1.Size = new Size(145, 26);
            label1.TabIndex = 1;
            label1.Text = "后台服务状态：";
            // 
            // Inst_install
            // 
            Inst_install.Appearance = Appearance.Button;
            Inst_install.AutoSize = true;
            Inst_install.BackColor = Color.FloralWhite;
            Inst_install.Checked = true;
            Inst_install.CheckState = CheckState.Checked;
            Inst_install.FlatStyle = FlatStyle.Flat;
            Inst_install.ForeColor = SystemColors.ButtonHighlight;
            Inst_install.Location = new Point(369, 13);
            Inst_install.Name = "Inst_install";
            Inst_install.Size = new Size(52, 31);
            Inst_install.TabIndex = 5;
            Inst_install.Text = "未知";
            Inst_install.UseVisualStyleBackColor = false;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label14.Location = new Point(221, 16);
            label14.Name = "label14";
            label14.Size = new Size(145, 26);
            label14.TabIndex = 4;
            label14.Text = "后台服务安装：";
            // 
            // Inst_run
            // 
            Inst_run.Appearance = Appearance.Button;
            Inst_run.AutoSize = true;
            Inst_run.BackColor = Color.FloralWhite;
            Inst_run.Checked = true;
            Inst_run.CheckState = CheckState.Checked;
            Inst_run.FlatStyle = FlatStyle.Flat;
            Inst_run.ForeColor = SystemColors.ButtonHighlight;
            Inst_run.Location = new Point(612, 13);
            Inst_run.Name = "Inst_run";
            Inst_run.Size = new Size(52, 31);
            Inst_run.TabIndex = 6;
            Inst_run.Text = "未知";
            Inst_run.UseVisualStyleBackColor = false;
            // 
            // restart
            // 
            restart.Location = new Point(696, 12);
            restart.Name = "restart";
            restart.Size = new Size(97, 36);
            restart.TabIndex = 7;
            restart.Text = "重启";
            restart.UseVisualStyleBackColor = true;
            restart.Click += restart_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1009, 729);
            Controls.Add(restart);
            Controls.Add(Inst_run);
            Controls.Add(Inst_install);
            Controls.Add(label14);
            Controls.Add(tabControl1);
            Controls.Add(label1);
            Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "Form1";
            Text = "GUI";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox6.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataView).EndInit();
            tabPage3.ResumeLayout(false);
            groupBox7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)tagsTable).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private GroupBox groupBox1;
        private GroupBox groupBox3;
        private GroupBox groupBox2;
        private Label label3;
        private TextBox textBox2;
        private Label label2;
        private TextBox textBox1;
        private TextBox textBox5;
        private Label label6;
        private TextBox textBox4;
        private Label label5;
        private TextBox textBox3;
        private Label label4;
        private DataGridView dataView;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewTextBoxColumn Column6;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private Label label1;
        private Label label9;
        private Label label8;
        private Label tdengine_run;
        private Button opcda_bt;
        private Label opcda_run;
        private Button tdengine_bt;
        private GroupBox groupBox6;
        private RichTextBox logsText;
        private TextBox textBox7;
        private Label label13;
        private TextBox textBox6;
        private Label label12;
        private GroupBox groupBox7;
        private DataGridView tagsTable;
        private Button button3;
        private Button button7;
        private Button upload_bt;
        private Button button5;
        private Button button4;
        private DataGridViewTextBoxColumn Column7;
        private DataGridViewTextBoxColumn Column8;
        private DataGridViewTextBoxColumn Column9;
        private DataGridViewTextBoxColumn Column10;
        private CheckBox Inst_install;
        private Label label14;
        private CheckBox Inst_run;
        private Button restart;
    }
}
