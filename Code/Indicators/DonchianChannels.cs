using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;


namespace Alveo.UserCode
{
    [Serializable]
    [Description("Donchian Channels")]
    public class DonchianChannels : IndicatorBase
    {
        private readonly Array<double> upper;
        private readonly Array<double> middle;
        private readonly Array<double> lower;

        public DonchianChannels()
        {

            indicator_buffers = 3;
            indicator_chart_window = true;

            indicator_color1 = Colors.Red;
            indicator_color2 = Colors.Blue;
            indicator_color3 = Colors.Green;

            indicator_width1 = 1;
            indicator_width2 = 1;
            indicator_width3 = 1;

            BarsToCount = 20;

            upper = new Array<double>();
            middle = new Array<double>();
            lower = new Array<double>();
        }

        [Description("Bars to count")]
        [Category("Settings")]
        [DisplayName("Bars to count")]
        public int BarsToCount { get; set; }

        protected override int Init()
        {
            SetIndexBuffer(0, upper, true);
            SetIndexStyle(0, DRAW_LINE);
            SetIndexLabel(0, "Upper");

            SetIndexBuffer(1, middle);
            SetIndexStyle(1, DRAW_LINE);
            SetIndexLabel(1, "Middle");

            SetIndexBuffer(2, lower);
            SetIndexStyle(2, DRAW_LINE);
            SetIndexLabel(2, "Lower");

            IndicatorShortName(string.Format("DCH({0})", BarsToCount));

            return 0;
        }

        protected override int Start()
        {
            int pos = IndicatorCounted();
            int limit = Bars - pos;
            int i = 0;
            if (pos > 0)
                limit++;

            for (i = 0; i < limit;  i++)
            {
                 upper[i] = iHigh(Symbol(), Period(), iHighest(this.Symbol, this.TimeFrame, MODE_HIGH, BarsToCount, i));
                //upper[i] = iHigh(null, 0, iHighest(null, 0, MODE_HIGH, BarsToCount, i));
                lower[i] = iLow(Symbol(), Period(), iLowest(this.Symbol, this.TimeFrame, MODE_LOW, BarsToCount, i));
                middle[i] = (upper[i] + lower[i]) / 2; 
            }

            return 0;
        }
    }
}
