namespace BitcoinAnalyzer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            plotView1 = new OxyPlot.WindowsForms.PlotView();
            dateTimePicker1 = new DateTimePicker();
            dateTimePicker2 = new DateTimePicker();
            FetchDataButton = new Button();
            labelCurrentPrice = new Label();
            labelLowestPriceDate = new Label();
            labelHighestPriceDate = new Label();
            labelLowestVolume = new Label();
            labelHighestVolume = new Label();
            labelLongestBearish = new Label();
            labelLongestBullish = new Label();
            labelBestBuySell = new Label();
            labelBestSellBuy = new Label();
            SuspendLayout();
            // 
            // plotView1
            // 
            plotView1.Location = new Point(384, 105);
            plotView1.Name = "plotView1";
            plotView1.PanCursor = Cursors.Hand;
            plotView1.Size = new Size(500, 300);
            plotView1.TabIndex = 0;
            plotView1.Text = "plotView1";
            plotView1.ZoomHorizontalCursor = Cursors.SizeWE;
            plotView1.ZoomRectangleCursor = Cursors.SizeNWSE;
            plotView1.ZoomVerticalCursor = Cursors.SizeNS;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(12, 12);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(300, 31);
            dateTimePicker1.TabIndex = 1;
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.Location = new Point(12, 59);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new Size(300, 31);
            dateTimePicker2.TabIndex = 2;
            // 
            // FetchDataButton
            // 
            FetchDataButton.Location = new Point(12, 105);
            FetchDataButton.Name = "FetchDataButton";
            FetchDataButton.Size = new Size(112, 34);
            FetchDataButton.TabIndex = 3;
            FetchDataButton.Text = "Show Chart";
            FetchDataButton.UseVisualStyleBackColor = true;
            FetchDataButton.Click += FetchDataButton_Click;
            // 
            // labelCurrentPrice
            // 
            labelCurrentPrice.AutoSize = true;
            labelCurrentPrice.Location = new Point(12, 216);
            labelCurrentPrice.Name = "labelCurrentPrice";
            labelCurrentPrice.Size = new Size(0, 25);
            labelCurrentPrice.TabIndex = 4;
            // 
            // labelLowestPriceDate
            // 
            labelLowestPriceDate.AutoSize = true;
            labelLowestPriceDate.Location = new Point(12, 276);
            labelLowestPriceDate.Name = "labelLowestPriceDate";
            labelLowestPriceDate.Size = new Size(113, 25);
            labelLowestPriceDate.TabIndex = 5;
            labelLowestPriceDate.Text = "Lowest Price:";
            // 
            // labelHighestPriceDate
            // 
            labelHighestPriceDate.AutoSize = true;
            labelHighestPriceDate.Location = new Point(12, 314);
            labelHighestPriceDate.Name = "labelHighestPriceDate";
            labelHighestPriceDate.Size = new Size(119, 25);
            labelHighestPriceDate.TabIndex = 6;
            labelHighestPriceDate.Text = "Highest Price:";
            // 
            // labelLowestVolume
            // 
            labelLowestVolume.AutoSize = true;
            labelLowestVolume.Location = new Point(12, 355);
            labelLowestVolume.Name = "labelLowestVolume";
            labelLowestVolume.Size = new Size(136, 25);
            labelLowestVolume.TabIndex = 7;
            labelLowestVolume.Text = "Lowest Volume:";
            // 
            // labelHighestVolume
            // 
            labelHighestVolume.AutoSize = true;
            labelHighestVolume.Location = new Point(12, 392);
            labelHighestVolume.Name = "labelHighestVolume";
            labelHighestVolume.Size = new Size(142, 25);
            labelHighestVolume.TabIndex = 8;
            labelHighestVolume.Text = "Highest Volume:";
            // 
            // labelLongestBearish
            // 
            labelLongestBearish.AutoSize = true;
            labelLongestBearish.Location = new Point(12, 201);
            labelLongestBearish.Name = "labelLongestBearish";
            labelLongestBearish.Size = new Size(140, 25);
            labelLongestBearish.TabIndex = 9;
            labelLongestBearish.Text = "Longest Bearish:";
            // 
            // labelLongestBullish
            // 
            labelLongestBullish.AutoSize = true;
            labelLongestBullish.Location = new Point(12, 236);
            labelLongestBullish.Name = "labelLongestBullish";
            labelLongestBullish.Size = new Size(134, 25);
            labelLongestBullish.TabIndex = 10;
            labelLongestBullish.Text = "Longest Bullish:";
            // 
            // labelBestBuySell
            // 
            labelBestBuySell.AutoSize = true;
            labelBestBuySell.Location = new Point(365, 59);
            labelBestBuySell.Name = "labelBestBuySell";
            labelBestBuySell.Size = new Size(0, 25);
            labelBestBuySell.TabIndex = 11;
            // 
            // labelBestSellBuy
            // 
            labelBestSellBuy.AutoSize = true;
            labelBestSellBuy.Location = new Point(365, 12);
            labelBestSellBuy.Name = "labelBestSellBuy";
            labelBestSellBuy.Size = new Size(0, 25);
            labelBestSellBuy.TabIndex = 12;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(854, 450);
            Controls.Add(labelBestSellBuy);
            Controls.Add(labelBestBuySell);
            Controls.Add(labelLongestBullish);
            Controls.Add(labelLongestBearish);
            Controls.Add(labelHighestVolume);
            Controls.Add(labelLowestVolume);
            Controls.Add(labelHighestPriceDate);
            Controls.Add(labelLowestPriceDate);
            Controls.Add(labelCurrentPrice);
            Controls.Add(FetchDataButton);
            Controls.Add(dateTimePicker2);
            Controls.Add(dateTimePicker1);
            Controls.Add(plotView1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "Bitcoin Analyzer";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private OxyPlot.WindowsForms.PlotView plotView1;
        private DateTimePicker dateTimePicker1;
        private DateTimePicker dateTimePicker2;
        private Button FetchDataButton;
        private Label labelCurrentPrice;
        private Label labelLowestPriceDate;
        private Label labelHighestPriceDate;
        private Label labelLowestVolume;
        private Label labelHighestVolume;
        private Label labelLongestBearish;
        private Label labelLongestBullish;
        private Label labelBestBuySell;
        private Label labelBestSellBuy;
    }
}