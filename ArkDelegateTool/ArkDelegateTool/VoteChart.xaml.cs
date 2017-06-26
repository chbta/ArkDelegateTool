using ArkDelegateToolLib;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ArkDelegateTool
{
    /// <summary>
    /// Interaction logic for VoteChart.xaml
    /// </summary>
    public partial class VoteChart : UserControl
    {
        BalanceTracker balanceTracker;
        public string currentAddress { get; set; }

        int cx = 50;
        int cy = 295;
        int hx = 290;
        int hy = 290;

        int minx;
        int maxx;
        int miny1;
        int maxy1;
        int miny2;
        int maxy2;

        public VoteChart()
        {
            InitializeComponent();
        }

        public void loadTracker(BalanceTracker balanceTracker)
        {
            this.balanceTracker = balanceTracker;
            draw();
        }

        public void setCurrentAddress(string address)
        {
            this.currentAddress = address;
            draw();
        }

        public void draw()
        {
            canvas.Children.Clear();
            drawPlot();
            drawGraph();
        }

        private void drawGraph()
        {
            LineColour = Brushes.Black;

            drawLine(0, 0, hx, 0);//horizontal line
            for (int i = 0; i <= 10; i++)
            {
                int xfactor = i * hx / 10;
                int tfactor = (minx * (10 - i) + maxx * i) / 10;
                drawText(xfactor + 5, 2, Helper.convertTime(tfactor).ToString(), 30);
                drawLine(xfactor, 2, xfactor, -3);
            }


            drawLine(0, 0, 0, hy);//vertical line
            for (int i = 0; i <= 10; i++)
            {
                int yfactor = i * hy / 10;
                int bfactor = (miny1 * (10 - i) + maxy1 * i) / 10;
                drawText(-35, yfactor + 10, (bfactor/1000).ToString() + "k");
                drawLine(2, yfactor, -3, yfactor);
            }


            if (currentAddress != null)
            {
                drawLine(hx, 0, hx, hy);//vertical line, right
                for (int i = 0; i <= 10; i++)
                {
                    int yfactor = i * hy / 10;
                    int bfactor = (miny2 * (10 - i) + maxy2 * i) / 10;
                    drawText(hx, yfactor + 15, bfactor.ToString());
                    drawLine(hx-2, yfactor, hx + 3, yfactor);
                }
            }
        }

        private void drawPlot()
        {
            LineColour = Brushes.Red;
            var bpsTime = balanceTracker.getBalanceChangePoints(new DateTime(2017, 6, 8), DateTime.Now);
            var bpsBal = bpsTime.Select(t => balanceTracker.getTotalBalanceAtTime(t)).ToList();
            int mint = bpsTime[0];
            minx = mint;
            int maxt = bpsTime[bpsTime.Count - 1];
            maxx = maxt;
            double minb = bpsBal.Min();
            miny1 = (int)minb;
            double maxb = bpsBal.Max();
            maxy1 = (int)maxb;
            var bpsTimes = bpsTime.Select(t => ((t - mint) * hx) / (maxt - mint)).ToList();
            bpsBal = bpsBal.Select(b => ((b - minb) * hy) / (maxb - minb)).ToList();
            for (int i = 0; i + 1 < bpsTimes.Count; i++)
            {
                drawLine(bpsTimes[i], bpsBal[i], bpsTimes[i + 1], bpsBal[i + 1]);
            }

            if (currentAddress != null)
            {
                LineColour = Brushes.Blue;
                var accBT = balanceTracker.accountBalanceTrackers.Where(abt => abt.Address.Equals(currentAddress)).Single();
                //var aBalances = abtTimes.Select(t => ((accBT.getBalanceAt(t) - minb) * hy) / (maxb - minb)).ToList();
                var aBalances = bpsTime.Select(t => (accBT.getBalanceAt(t))).ToList(); //bpsTime.Select(t => ((hy*accBT.getBalanceAt(t) / balanceTracker.getTotalBalanceAtTime(t)))).ToList();
                double mina = 0;//aBalances.Min();
                double maxa = aBalances.Max();
                if (mina == maxa) maxa++;
                aBalances = aBalances.Select(b => (b - mina) * hx / (maxa - mina)).ToList();
                for (int i = 0; i < bpsTime.Count - 1; i++)
                {
                    drawLine(bpsTimes[i], aBalances[i], bpsTimes[i + 1], aBalances[i + 1]);
                }
                drawLine(bpsTimes.Last(), aBalances.Last(), bpsTime.Last(), aBalances.Last());

                miny2 = 0;//*/ (int) mina;
                maxy2 = /*100;//*/ (int) maxa;

                /*var accBT = balanceTracker.accountBalanceTrackers.Where(abt => abt.address.Equals(currentAddress)).Single();
                var abtTimes = accBT.getBalanceChangePoints();
                var aTimes = abtTimes.Select(t => ((t - mint) * hx) / (maxt - mint)).ToList();
                var aBalances = abtTimes.Select(t => ((accBT.getBalanceAt(t) - minb) * hy) / (maxb - minb)).ToList();
                for (int i=0; i<abtTimes.Count-1; i++)
                {
                    drawLine(aTimes[i], aBalances[i], aTimes[i+1], aBalances[i]);
                }
                drawLine(aTimes.Last(), aBalances.Last(), bpsTime.Last(), aBalances.Last());*/
            }
        }

        private Brush LineColour;
        private void drawLine(double x1, double y1, double x2, double y2)
        {
            canvas.Children.Add(new Line()
            {
                X1 = cx + x1,
                Y1 = cy - y1,
                X2 = cx + x2,
                Y2 = cy - y2,
                StrokeThickness = 1,
                Stroke = LineColour
            });
        }

        private void drawText(double x, double y, string text, int angle = 0)
        {
            var textBlock = new TextBlock()
            {
                Text = text,
                RenderTransform = new RotateTransform(angle)
            };
            Canvas.SetLeft(textBlock, cx + x);
            Canvas.SetTop(textBlock, cy - y);
            canvas.Children.Add(textBlock);
        }
    }
}
