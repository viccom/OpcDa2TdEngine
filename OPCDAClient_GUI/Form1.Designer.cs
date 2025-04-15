namespace OPCDAClient_GUI
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
            TabControl = new TabControl();
            overview = new TabPage();
            groupBox1 = new GroupBox();
            logClear_bt = new Button();
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
            rtdata = new TabPage();
            rtDataGridView = new Zuby.ADGV.AdvancedDataGridView();
            config = new TabPage();
            label3 = new Label();
            button7 = new Button();
            textBox2 = new TextBox();
            upload_bt = new Button();
            textBox5 = new TextBox();
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
            label6 = new Label();
            textBox4 = new TextBox();
            label5 = new Label();
            textBox3 = new TextBox();
            label4 = new Label();
            groupBox2 = new GroupBox();
            label2 = new Label();
            textBox1 = new TextBox();
            label1 = new Label();
            Inst_install = new CheckBox();
            label14 = new Label();
            Inst_run = new CheckBox();
            restart = new Button();
            label7 = new Label();
            mqtt_node = new TextBox();
            mqtt_bt = new Button();
            install_bt = new Button();
            TabControl.SuspendLayout();
            overview.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox6.SuspendLayout();
            groupBox5.SuspendLayout();
            groupBox4.SuspendLayout();
            rtdata.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rtDataGridView).BeginInit();
            config.SuspendLayout();
            groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tagsTable).BeginInit();
            groupBox3.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // TabControl
            // 
            TabControl.Controls.Add(overview);
            TabControl.Controls.Add(rtdata);
            TabControl.Controls.Add(config);
            TabControl.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TabControl.Location = new Point(24, 90);
            TabControl.Margin = new Padding(4);
            TabControl.Name = "TabControl";
            TabControl.SelectedIndex = 0;
            TabControl.Size = new Size(1537, 735);
            TabControl.TabIndex = 0;
            // 
            // overview
            // 
            overview.Controls.Add(groupBox1);
            overview.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            overview.Location = new Point(8, 55);
            overview.Margin = new Padding(4);
            overview.Name = "overview";
            overview.Padding = new Padding(4);
            overview.Size = new Size(1521, 672);
            overview.TabIndex = 0;
            overview.Text = "概览";
            overview.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(logClear_bt);
            groupBox1.Controls.Add(groupBox6);
            groupBox1.Controls.Add(groupBox5);
            groupBox1.Controls.Add(groupBox4);
            groupBox1.Location = new Point(8, 18);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1506, 626);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "后台服务状态";
            // 
            // logClear_bt
            // 
            logClear_bt.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
            logClear_bt.Location = new Point(1394, 172);
            logClear_bt.Name = "logClear_bt";
            logClear_bt.Size = new Size(75, 23);
            logClear_bt.TabIndex = 1;
            logClear_bt.Text = "清除";
            logClear_bt.UseVisualStyleBackColor = true;
            logClear_bt.Click += LogClear_Click;
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(logsText);
            groupBox6.Location = new Point(27, 159);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new Size(1453, 446);
            groupBox6.TabIndex = 4;
            groupBox6.TabStop = false;
            groupBox6.Text = "程序日志：";
            // 
            // logsText
            // 
            logsText.EnableAutoDragDrop = true;
            logsText.Location = new Point(6, 42);
            logsText.Name = "logsText";
            logsText.Size = new Size(1441, 426);
            logsText.TabIndex = 0;
            logsText.Text = "";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(tdengine_bt);
            groupBox5.Controls.Add(tdengine_run);
            groupBox5.Controls.Add(label9);
            groupBox5.Location = new Point(896, 43);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(578, 113);
            groupBox5.TabIndex = 3;
            groupBox5.TabStop = false;
            groupBox5.Text = "TdEngine写数服务";
            // 
            // tdengine_bt
            // 
            tdengine_bt.Location = new Point(405, 51);
            tdengine_bt.Name = "tdengine_bt";
            tdengine_bt.Size = new Size(146, 62);
            tdengine_bt.TabIndex = 2;
            tdengine_bt.Text = "button2";
            tdengine_bt.UseVisualStyleBackColor = true;
            tdengine_bt.Visible = false;
            tdengine_bt.Click += tdengine_bt_Click;
            // 
            // tdengine_run
            // 
            tdengine_run.AutoSize = true;
            tdengine_run.ForeColor = SystemColors.ButtonHighlight;
            tdengine_run.Location = new Point(206, 62);
            tdengine_run.Name = "tdengine_run";
            tdengine_run.Size = new Size(130, 41);
            tdengine_run.TabIndex = 1;
            tdengine_run.Text = "label11";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(22, 62);
            label9.Name = "label9";
            label9.Size = new Size(178, 41);
            label9.TabIndex = 0;
            label9.Text = "服务状态：";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(opcda_bt);
            groupBox4.Controls.Add(opcda_run);
            groupBox4.Controls.Add(label8);
            groupBox4.Location = new Point(27, 43);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(660, 113);
            groupBox4.TabIndex = 0;
            groupBox4.TabStop = false;
            groupBox4.Text = "OPC采集服务";
            // 
            // opcda_bt
            // 
            opcda_bt.Location = new Point(458, 51);
            opcda_bt.Name = "opcda_bt";
            opcda_bt.Size = new Size(146, 62);
            opcda_bt.TabIndex = 2;
            opcda_bt.Text = "button1";
            opcda_bt.UseVisualStyleBackColor = true;
            opcda_bt.Visible = false;
            opcda_bt.Click += opcda_bt_Click;
            // 
            // opcda_run
            // 
            opcda_run.AutoSize = true;
            opcda_run.ForeColor = SystemColors.ButtonHighlight;
            opcda_run.Location = new Point(293, 62);
            opcda_run.Name = "opcda_run";
            opcda_run.Size = new Size(130, 41);
            opcda_run.TabIndex = 1;
            opcda_run.Text = "label10";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(31, 62);
            label8.Name = "label8";
            label8.Size = new Size(178, 41);
            label8.TabIndex = 0;
            label8.Text = "服务状态：";
            // 
            // rtdata
            // 
            rtdata.Controls.Add(rtDataGridView);
            rtdata.Location = new Point(8, 55);
            rtdata.Name = "rtdata";
            rtdata.Padding = new Padding(3);
            rtdata.Size = new Size(1521, 672);
            rtdata.TabIndex = 3;
            rtdata.Text = "数据";
            rtdata.UseVisualStyleBackColor = true;
            // 
            // rtDataGridView
            // 
            rtDataGridView.AllowUserToAddRows = false;
            rtDataGridView.AllowUserToDeleteRows = false;
            rtDataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            rtDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            rtDataGridView.FilterAndSortEnabled = true;
            rtDataGridView.FilterStringChangedInvokeBeforeDatasourceUpdate = true;
            rtDataGridView.Location = new Point(3, 3);
            rtDataGridView.MaxFilterButtonImageHeight = 23;
            rtDataGridView.Name = "rtDataGridView";
            rtDataGridView.RightToLeft = RightToLeft.No;
            rtDataGridView.RowHeadersWidth = 82;
            rtDataGridView.Size = new Size(1518, 669);
            rtDataGridView.SortStringChangedInvokeBeforeDatasourceUpdate = true;
            rtDataGridView.TabIndex = 0;
            rtDataGridView.VirtualMode = true;
            // 
            // config
            // 
            config.Controls.Add(label3);
            config.Controls.Add(button7);
            config.Controls.Add(textBox2);
            config.Controls.Add(upload_bt);
            config.Controls.Add(textBox5);
            config.Controls.Add(button5);
            config.Controls.Add(button4);
            config.Controls.Add(button3);
            config.Controls.Add(groupBox7);
            config.Controls.Add(groupBox3);
            config.Controls.Add(groupBox2);
            config.Location = new Point(8, 55);
            config.Margin = new Padding(4);
            config.Name = "config";
            config.Size = new Size(1521, 672);
            config.TabIndex = 2;
            config.Text = "配置";
            config.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(756, 47);
            label3.Name = "label3";
            label3.Size = new Size(155, 41);
            label3.TabIndex = 3;
            label3.Text = "ProgID：";
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
            // textBox2
            // 
            textBox2.Location = new Point(919, 47);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(414, 48);
            textBox2.TabIndex = 2;
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
            // textBox5
            // 
            textBox5.Location = new Point(1120, 144);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(213, 48);
            textBox5.TabIndex = 5;
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
            tagsTable.RowHeadersWidth = 82;
            tagsTable.Size = new Size(1487, 340);
            tagsTable.TabIndex = 0;
            // 
            // Column7
            // 
            Column7.HeaderText = "名称";
            Column7.MinimumWidth = 10;
            Column7.Name = "Column7";
            Column7.Width = 200;
            // 
            // Column8
            // 
            Column8.HeaderText = "描述";
            Column8.MinimumWidth = 10;
            Column8.Name = "Column8";
            Column8.Width = 200;
            // 
            // Column9
            // 
            Column9.HeaderText = "类型";
            Column9.MinimumWidth = 10;
            Column9.Name = "Column9";
            Column9.Width = 200;
            // 
            // Column10
            // 
            Column10.HeaderText = "OPC Item";
            Column10.MinimumWidth = 10;
            Column10.Name = "Column10";
            Column10.Width = 400;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(textBox7);
            groupBox3.Controls.Add(label13);
            groupBox3.Controls.Add(textBox6);
            groupBox3.Controls.Add(label12);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(textBox4);
            groupBox3.Controls.Add(label5);
            groupBox3.Controls.Add(textBox3);
            groupBox3.Controls.Add(label4);
            groupBox3.Location = new Point(8, 103);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(1465, 149);
            groupBox3.TabIndex = 1;
            groupBox3.TabStop = false;
            groupBox3.Text = "TDEngine 数据库：";
            // 
            // textBox7
            // 
            textBox7.Location = new Point(911, 105);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(414, 48);
            textBox7.TabIndex = 9;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(665, 105);
            label13.Name = "label13";
            label13.Size = new Size(195, 41);
            label13.TabIndex = 8;
            label13.Text = "Password：";
            // 
            // textBox6
            // 
            textBox6.Location = new Point(216, 98);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(271, 48);
            textBox6.TabIndex = 7;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(13, 98);
            label12.Name = "label12";
            label12.Size = new Size(211, 41);
            label12.TabIndex = 6;
            label12.Text = "UserName：";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(911, 40);
            label6.Name = "label6";
            label6.Size = new Size(162, 41);
            label6.TabIndex = 4;
            label6.Text = "DbName:";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(729, 37);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(100, 48);
            textBox4.TabIndex = 3;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(593, 37);
            label5.Name = "label5";
            label5.Size = new Size(114, 41);
            label5.TabIndex = 2;
            label5.Text = "Port：";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(217, 40);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(270, 48);
            textBox3.TabIndex = 1;
            textBox3.Text = "locahost";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(59, 44);
            label4.Name = "label4";
            label4.Size = new Size(122, 41);
            label4.TabIndex = 0;
            label4.Text = "Host：";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(textBox1);
            groupBox2.Location = new Point(8, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(985, 85);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "OPC DA 服务器：";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(59, 38);
            label2.Name = "label2";
            label2.Size = new Size(122, 41);
            label2.TabIndex = 1;
            label2.Text = "Host：";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(216, 38);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(271, 48);
            textBox1.TabIndex = 0;
            textBox1.Text = "localhost";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label1.Location = new Point(994, 16);
            label1.Name = "label1";
            label1.Size = new Size(288, 50);
            label1.TabIndex = 1;
            label1.Text = "节点服务状态：";
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
            Inst_install.Location = new Point(282, 19);
            Inst_install.Name = "Inst_install";
            Inst_install.Size = new Size(92, 51);
            Inst_install.TabIndex = 5;
            Inst_install.Text = "未知";
            Inst_install.UseVisualStyleBackColor = false;
            Inst_install.Click += Inst_install_Click;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 134);
            label14.Location = new Point(7, 16);
            label14.Name = "label14";
            label14.Size = new Size(288, 50);
            label14.TabIndex = 4;
            label14.Text = "本地服务安装：";
            // 
            // Inst_run
            // 
            Inst_run.Appearance = Appearance.Button;
            Inst_run.AutoSize = true;
            Inst_run.BackColor = Color.Gray;
            Inst_run.Checked = true;
            Inst_run.CheckState = CheckState.Checked;
            Inst_run.FlatStyle = FlatStyle.Flat;
            Inst_run.ForeColor = SystemColors.ButtonHighlight;
            Inst_run.Location = new Point(1273, 19);
            Inst_run.Name = "Inst_run";
            Inst_run.Size = new Size(92, 51);
            Inst_run.TabIndex = 6;
            Inst_run.Text = "未知";
            Inst_run.UseVisualStyleBackColor = false;
            // 
            // restart
            // 
            restart.Location = new Point(1370, 12);
            restart.Name = "restart";
            restart.Size = new Size(113, 54);
            restart.TabIndex = 7;
            restart.Text = "重启";
            restart.UseVisualStyleBackColor = true;
            restart.Visible = false;
            restart.Click += restart_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Microsoft YaHei UI", 14.25F, FontStyle.Bold);
            label7.Location = new Point(511, 16);
            label7.Name = "label7";
            label7.Size = new Size(212, 50);
            label7.TabIndex = 8;
            label7.Text = "当前节点：";
            // 
            // mqtt_node
            // 
            mqtt_node.Location = new Point(709, 16);
            mqtt_node.Name = "mqtt_node";
            mqtt_node.Size = new Size(146, 48);
            mqtt_node.TabIndex = 9;
            mqtt_node.Text = "127.0.0.1";
            // 
            // mqtt_bt
            // 
            mqtt_bt.Location = new Point(861, 12);
            mqtt_bt.Name = "mqtt_bt";
            mqtt_bt.Size = new Size(119, 54);
            mqtt_bt.TabIndex = 10;
            mqtt_bt.Text = "连接";
            mqtt_bt.UseVisualStyleBackColor = true;
            // 
            // install_bt
            // 
            install_bt.BackColor = SystemColors.ControlDark;
            install_bt.Location = new Point(374, 13);
            install_bt.Name = "install_bt";
            install_bt.Size = new Size(126, 54);
            install_bt.TabIndex = 11;
            install_bt.Text = "启动";
            install_bt.UseVisualStyleBackColor = false;
            install_bt.Click += install_bt_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(19F, 41F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(1574, 829);
            Controls.Add(install_bt);
            Controls.Add(mqtt_bt);
            Controls.Add(mqtt_node);
            Controls.Add(label7);
            Controls.Add(restart);
            Controls.Add(Inst_run);
            Controls.Add(Inst_install);
            Controls.Add(label14);
            Controls.Add(TabControl);
            Controls.Add(label1);
            Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterParent;
            Text = "GUI";
            TabControl.ResumeLayout(false);
            overview.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox6.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            rtdata.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rtDataGridView).EndInit();
            config.ResumeLayout(false);
            config.PerformLayout();
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

        private TabControl TabControl;
        private TabPage overview;
        private TabPage config;
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
        private Label label7;
        private TextBox mqtt_node;
        private Button mqtt_bt;
        private TabPage rtdata;
        private Zuby.ADGV.AdvancedDataGridView rtDataGridView;
        private Button install_bt;
        private Button logClear_bt;
    }
}
