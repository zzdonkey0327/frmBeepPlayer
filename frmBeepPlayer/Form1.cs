using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace frmBeepPlayer
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll")]
        public static extern bool Beep(int frequency, int duration);

        int[] freq = { 523, 587, 659, 698, 784, 880, 988, 1046 };

        private SizeF originalClientSize;
        private Rectangle originalPalMainRect;
        private Rectangle originalAutoPlayRect;
        private Rectangle[] originalBtnRects;
        private Button[] buttons;

        public Form1()
        {
            InitializeComponent();
            InitializeButton();

            // 將電子琴鍵按鈕放大並重新排列
            buttons = new Button[] { btn1, btn2, btn3, btn4, btn5, btn6, btn7, btn8 };
            int padding = 5;
            // 動態計算每個按鍵的寬度和高度，適應 palMain 的當前大小
            int keyWidth = (palMain.Width - ((buttons.Length - 1) * padding)) / buttons.Length;
            int keyHeight = palMain.Height > 40 ? palMain.Height - 40 : palMain.Height;

            int startX = (palMain.Width - (buttons.Length * keyWidth + (buttons.Length - 1) * padding)) / 2;
            if (startX < 0) startX = 0;
            int startY = (palMain.Height - keyHeight) / 2;

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Size = new Size(keyWidth, keyHeight);
                buttons[i].Location = new Point(startX + i * (keyWidth + padding), startY);
                buttons[i].TabStop = false; // 取消焦點，避免出現藍色框框
            }

            btnAutoPlay.TabStop = false; // 取消自動播放按鈕的焦點

            // 記錄初始長寬比，以便在調整視窗大小時等比例縮放
            originalClientSize = this.ClientSize;
            originalPalMainRect = new Rectangle(palMain.Location, palMain.Size);
            originalAutoPlayRect = new Rectangle(btnAutoPlay.Location, btnAutoPlay.Size);
            originalBtnRects = new Rectangle[buttons.Length];

            for (int i = 0; i < buttons.Length; i++)
            {
                originalBtnRects[i] = new Rectangle(buttons[i].Location, buttons[i].Size);
            }

            // 綁定視窗大小改變事件
            this.SizeChanged += frmBeePlayer_SizeChanged;
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            btn.Enabled = false;
            Beep(freq[btn.TabIndex], 300);
            btn.Enabled = true;
        }

        private void InitializeButton()
        {
            btn2.Click += btn1_Click;
            btn3.Click += btn1_Click;
            btn4.Click += btn1_Click;
            btn5.Click += btn1_Click;
            btn6.Click += btn1_Click;
            btn7.Click += btn1_Click;
            btn8.Click += btn1_Click;
        }

        private async void btnAutoPlay_Click(object sender, EventArgs e)
        {
            btnAutoPlay.Enabled = false;

            // 小蜜蜂音符: 5 3 3, 4 2 2, 1 2 3 4 5 5 5 (對應陣列索引)
            // 對應頻率 freq[] 索引: Do=0, Re=1, Mi=2, Fa=3, So=4, La=5, Si=6
            int[] melody = { 4, 2, 2, -1, 3, 1, 1, -1, 0, 1, 2, 3, 4, 4, 4, -1 };
            int duration = 300; 

            Color originalColor = SystemColors.Control;
            if (buttons.Length > 0) originalColor = Color.White;

            await Task.Run(() =>
            {
                foreach (int note in melody)
                {
                    if (note == -1) // 休止符
                    {
                        System.Threading.Thread.Sleep(300);
                    }
                    else
                    {
                        // 透過 Invoke 在主執行緒更新 UI 呈現按下的效果
                        this.Invoke(new Action(() => buttons[note].BackColor = Color.LightSkyBlue));

                        Beep(freq[note], duration);

                        // 恢復原本的按鈕顏色
                        this.Invoke(new Action(() => buttons[note].BackColor = originalColor));

                        System.Threading.Thread.Sleep(50); // 音符間隔
                    }
                }
            });

            btnAutoPlay.Enabled = true;
        }

        private void frmBeePlayer_SizeChanged(object sender, EventArgs e)
        {
            if (originalClientSize.Width == 0 || originalClientSize.Height == 0) return;

            float xRatio = this.ClientSize.Width / originalClientSize.Width;
            float yRatio = this.ClientSize.Height / originalClientSize.Height;

            palMain.Location = new Point((int)(originalPalMainRect.X * xRatio), (int)(originalPalMainRect.Y * yRatio));
            palMain.Size = new Size((int)(originalPalMainRect.Width * xRatio), (int)(originalPalMainRect.Height * yRatio));

            btnAutoPlay.Location = new Point((int)(originalAutoPlayRect.X * xRatio), (int)(originalAutoPlayRect.Y * yRatio));
            btnAutoPlay.Size = new Size((int)(originalAutoPlayRect.Width * xRatio), (int)(originalAutoPlayRect.Height * yRatio));
            float autoPlayFontSize = 9f * Math.Min(xRatio, yRatio);
            if (autoPlayFontSize > 1) btnAutoPlay.Font = new Font(btnAutoPlay.Font.FontFamily, autoPlayFontSize);

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Location = new Point((int)(originalBtnRects[i].X * xRatio), (int)(originalBtnRects[i].Y * yRatio));
                buttons[i].Size = new Size((int)(originalBtnRects[i].Width * xRatio), (int)(originalBtnRects[i].Height * yRatio));

                // 動態調整字體大小
                float newFontSize = 9f * Math.Min(xRatio, yRatio);
                if (newFontSize > 1)
                {
                    buttons[i].Font = new Font(buttons[i].Font.FontFamily, newFontSize);
                }
            }
        }
    }
}