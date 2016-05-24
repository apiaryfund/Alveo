using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Percentage Price Oscillator")]
    public class PPO : IndicatorBase
    {
        private readonly Array<double> ppoBuffer;
        private readonly Array<double> signalBuffer;

        public PPO()
        {
            indicator_chart_window = false;
            indicator_buffers = 2;
            indicator_color1 = Colors.SkyBlue;
            indicator_color2 = Colors.Red;

            FastEMA = 12;
            SlowEMA = 26;
            SignalEMA = 9;

            ppoBuffer = new Array<double>();
            signalBuffer = new Array<double>();
        }

        [Category("Settings")]
        [Description("Fast EMA")]
        [DisplayName("Fast EMA")]
        public int FastEMA { get; set; }

        [Category("Settings")]
        [Description("Slow EMA")]
        [DisplayName("Slow EMA")]
        public int SlowEMA { get; set; }

        [Category("Settings")]
        [Description("Signal EMA")]
        [DisplayName("Signal EMA")]
        public int SignalEMA { get; set; }

        protected override int Init()
        {
            SetIndexBuffer(0, ppoBuffer);
            SetIndexStyle(0, DRAW_LINE);
            SetIndexLabel(0, "PPO");

            SetIndexBuffer(1, signalBuffer);
            SetIndexStyle(1, DRAW_LINE);
            SetIndexLabel(1, "Signal");

            IndicatorDigits(Digits + 1);
            IndicatorShortName(string.Format("PPO({0},{1},{2})", FastEMA, SlowEMA, SignalEMA));

            return 0;
        }

        protected override int Start()
        {
            int limit = 0;
            int pos = IndicatorCounted();

            if (pos > 0)
                pos--;
            limit = Bars - pos;

            for (int i = 0; i < limit; i++)
                ppoBuffer[i] = (iMA(null, 0, FastEMA, 0, MODE_EMA, PRICE_CLOSE, i)
                    - iMA(null, 0, SlowEMA, 0, MODE_EMA, PRICE_CLOSE, i))
                    / iMA(null, 0, SlowEMA, 0, MODE_EMA, PRICE_CLOSE, i);

            for (int i = 0; i < limit; i++)
                signalBuffer[i] = iMAOnArray(ppoBuffer, Bars, SignalEMA, 0, MODE_EMA, i);

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
            if (!(values[2] is int) || (int)values[2] != FastEMA)
                return false;
            if (!(values[3] is int) || (int)values[3] != SlowEMA)
                return false;
            if (!(values[4] is int) || (int)values[4] != SignalEMA)
                return false;

            return true;
        }

    }
}
