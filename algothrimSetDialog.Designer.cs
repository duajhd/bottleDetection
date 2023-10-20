
namespace bottleDetection
{
    partial class algothrimSetDialog
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
            System.Windows.Forms.TreeNode treeNode20 = new System.Windows.Forms.TreeNode("瓶身定位");
            System.Windows.Forms.TreeNode treeNode21 = new System.Windows.Forms.TreeNode("普通区域");
            System.Windows.Forms.TreeNode treeNode22 = new System.Windows.Forms.TreeNode("瓶底区域");
            System.Windows.Forms.TreeNode treeNode23 = new System.Windows.Forms.TreeNode("瓶身应力区域");
            System.Windows.Forms.TreeNode treeNode24 = new System.Windows.Forms.TreeNode("瓶口轮廓");
            System.Windows.Forms.TreeNode treeNode25 = new System.Windows.Forms.TreeNode("AI检测");
            System.Windows.Forms.TreeNode treeNode26 = new System.Windows.Forms.TreeNode("异常检测");
            System.Windows.Forms.TreeNode treeNode27 = new System.Windows.Forms.TreeNode("常用项", new System.Windows.Forms.TreeNode[] {
            treeNode20,
            treeNode21,
            treeNode22,
            treeNode23,
            treeNode24,
            treeNode25,
            treeNode26});
            System.Windows.Forms.TreeNode treeNode28 = new System.Windows.Forms.TreeNode("瓶身定位");
            System.Windows.Forms.TreeNode treeNode29 = new System.Windows.Forms.TreeNode("瓶口定位");
            System.Windows.Forms.TreeNode treeNode30 = new System.Windows.Forms.TreeNode("瓶底定位");
            System.Windows.Forms.TreeNode treeNode31 = new System.Windows.Forms.TreeNode("定位", new System.Windows.Forms.TreeNode[] {
            treeNode28,
            treeNode29,
            treeNode30});
            System.Windows.Forms.TreeNode treeNode32 = new System.Windows.Forms.TreeNode("横向尺寸");
            System.Windows.Forms.TreeNode treeNode33 = new System.Windows.Forms.TreeNode("纵向尺寸");
            System.Windows.Forms.TreeNode treeNode34 = new System.Windows.Forms.TreeNode("瓶全高");
            System.Windows.Forms.TreeNode treeNode35 = new System.Windows.Forms.TreeNode("歪脖");
            System.Windows.Forms.TreeNode treeNode36 = new System.Windows.Forms.TreeNode("垂直度");
            System.Windows.Forms.TreeNode treeNode37 = new System.Windows.Forms.TreeNode("圆形尺寸");
            System.Windows.Forms.TreeNode treeNode38 = new System.Windows.Forms.TreeNode("尺寸", new System.Windows.Forms.TreeNode[] {
            treeNode32,
            treeNode33,
            treeNode34,
            treeNode35,
            treeNode36,
            treeNode37});
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.treeView2 = new System.Windows.Forms.TreeView();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(147, 22);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(716, 525);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(890, 462);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 31);
            this.button1.TabIndex = 2;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(890, 499);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 31);
            this.button2.TabIndex = 3;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.treeView2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(358, 525);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DimGray;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(358, 52);
            this.panel1.TabIndex = 0;
            // 
            // treeView2
            // 
            this.treeView2.BackColor = System.Drawing.Color.DarkGray;
            this.treeView2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView2.Location = new System.Drawing.Point(3, 55);
            this.treeView2.Name = "treeView2";
            treeNode20.Name = "节点1";
            treeNode20.Text = "瓶身定位";
            treeNode21.Name = "节点2";
            treeNode21.Text = "普通区域";
            treeNode22.Name = "节点3";
            treeNode22.Text = "瓶底区域";
            treeNode23.Name = "节点4";
            treeNode23.Text = "瓶身应力区域";
            treeNode24.Name = "节点5";
            treeNode24.Text = "瓶口轮廓";
            treeNode25.Name = "节点6";
            treeNode25.Text = "AI检测";
            treeNode26.Name = "节点7";
            treeNode26.Text = "异常检测";
            treeNode27.Checked = true;
            treeNode27.Name = "节点0";
            treeNode27.Text = "常用项";
            treeNode28.Name = "节点9";
            treeNode28.Text = "瓶身定位";
            treeNode29.Name = "节点10";
            treeNode29.Text = "瓶口定位";
            treeNode30.Name = "节点11";
            treeNode30.Text = "瓶底定位";
            treeNode31.Name = "节点8";
            treeNode31.Text = "定位";
            treeNode32.Name = "节点1";
            treeNode32.Text = "横向尺寸";
            treeNode33.Name = "节点2";
            treeNode33.Text = "纵向尺寸";
            treeNode34.Name = "节点3";
            treeNode34.Text = "瓶全高";
            treeNode35.Name = "节点4";
            treeNode35.Text = "歪脖";
            treeNode36.Name = "节点5";
            treeNode36.Text = "垂直度";
            treeNode37.Name = "节点6";
            treeNode37.Text = "圆形尺寸";
            treeNode38.Name = "节点0";
            treeNode38.Text = "尺寸";
            this.treeView2.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode27,
            treeNode31,
            treeNode38});
            this.treeView2.Size = new System.Drawing.Size(352, 467);
            this.treeView2.TabIndex = 8;
            // 
            // algothrimSetDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.ClientSize = new System.Drawing.Size(977, 598);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "algothrimSetDialog";
            this.Text = "algothrimSetDialog";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TreeView treeView2;
        private System.Windows.Forms.Panel panel1;
    }
}