using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Keltner Channels")]
    public class KeltnerChannels : IndicatorBase
    {
        private readonly Array<double> _upper;
        private readonly Array<double> _middle;
        private readonly Array<double> _lower;

        public KeltnerChannels()
        {
            indicator_buffers = 3;
            indicator_chart_window = true;

            indicator_color1 = Colors.Red;
            indicator_color2 = Colors.DarkBlue;
            indicator_color3 = Colors.Red;

            period = 10;

            _upper = new Array<double>();
            _middle = new Array<double>();
            _lower = new Array<double>();
        }

        [Description("Period")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int period { get; set; }

        protected override int Init()
        {
            SetIndexBuffer(0, _upper);
            SetIndexStyle(0, DRAW_LINE);
            SetIndexShift(0, 0);
            SetIndexDrawBegin(0, 0);

            SetIndexBuffer(1, _middle);
            SetIndexStyle(1, DRAW_LINE, STYLE_DASHDOT);
            SetIndexShift(1, 0);
            SetIndexDrawBegin(1, 0);

            SetIndexBuffer(2, _lower);
            SetIndexStyle(2, DRAW_LINE);
            SetIndexShift(2, 0);
            SetIndexDrawBegin(2, 0);

            SetIndexLabel(0, "KChanUp(" + period + ")");
            SetIndexLabel(1, "KChanMid(" + period + ")");
            SetIndexLabel(2, "KChanLow(" + period + ")");

            IndicatorShortName(string.Format("KCH({0})", period));

            return 0;
        }

        protected override int Start()
        {
            int limit = 0;
            double avg = 0;
            int pos = IndicatorCounted();
            if (pos < 0)
                return -1;
            if (pos > 0)
                pos--;

            limit = Bars - pos;
            if (pos == 0)
                limit -= 1 + period;

            for (int i = 0; i < limit; i++)
            {
                _middle[i] = iMA(null, 0, period, 0, MODE_SMA, PRICE_TYPICAL, i);
                avg = findAvg(period, i);
                _upper[i] = _middle[i] + avg;
                _lower[i] = _middle[i] - avg;
            }
                return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 5)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;
            if (!(values[2] is int) || (int)values[2] != period) 
                return false;
            return true;
        }

        private double findAvg(int period, int shift)
        {
            double sum = 0;
            for(int i = shift; i < (shift + period); i++)
            {
                sum += High[i] - Low[i];
            }

            sum = sum / period;
            return sum;
        }

    }
}
