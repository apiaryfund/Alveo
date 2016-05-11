using System;
using System.ComponentModel;
using Alveo.Interfaces.UserCode;
using System.Collections.Generic;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Stochastic oscillator Indicator")]
    public class Stochastic : IndicatorBase
    {
        private readonly Array<double> _highesBuffer = new Array<double>();
        private readonly Array<double> _lowesBuffer = new Array<double>();
        private readonly Array<double> _mainBuffer = new Array<double>();
        private readonly Array<double> _signalBuffer = new Array<double>();
        private List<Array<double>> _levels;

        private int draw_begin1;
        private int draw_begin2;

        public Stochastic()
        {
            indicator_buffers = 2;
            indicator_chart_window = false;
            KPeriod = 5;
            DPeriod = 3;
            Slowing = 3;
            PriceType = PriceConstants.PRICE_CLOSE;

            indicator_color1 = LightSeaGreen;
            indicator_color2 = Red;

            Levels.Values.Add(new Alveo.Interfaces.UserCode.Double(70));
            Levels.Values.Add(new Alveo.Interfaces.UserCode.Double(30));

            var short_name = "Sto(" + KPeriod + "," + DPeriod + "," + Slowing + ")";
            IndicatorShortName(short_name);
            SetIndexLabel(0, short_name);
            SetIndexLabel(1, "Signal");
        }

        [Description("%K Period of the Stochastic Indicator")]
        [Category("Settings")]
        [DisplayName("K-Period")]
        public int KPeriod { get; set; }

        [Description("%D Period of the Stochastic Indicator")]
        [Category("Settings")]
        [DisplayName("D-Period")]
        public int DPeriod { get; set; }

        [Description("Slowing value of the Stochastic Indicator")]
        [Category("Settings")]
        public int Slowing { get; set; }

        [Description("Moving Average type on witch Stochastic will be calculated")]
        [Category("Settings")]
        [DisplayName("MA Type")]
        public MovingAverageType MAType { get; set; }

        [Description("Price type on witch Stochastic will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            for (int x = 2; x < indicator_buffers; x++)
            {
                SetIndexBuffer(x, null);
            }
            indicator_buffers = Levels.Values.Count + 2;
            string short_name;


            SetIndexStyle(0, DRAW_LINE);
            SetIndexBuffer(0, _mainBuffer);
            SetIndexStyle(1, DRAW_LINE);
            SetIndexBuffer(1, _signalBuffer);

            short_name = "Sto(" + KPeriod + "," + DPeriod + "," + Slowing + ")";
            IndicatorShortName(short_name);
            SetIndexLabel(0, short_name);
            SetIndexLabel(1, "Signal");

            draw_begin1 = KPeriod + Slowing;
            draw_begin2 = draw_begin1 + DPeriod;
            SetIndexDrawBegin(0, draw_begin1);
            SetIndexDrawBegin(1, draw_begin2);

            _levels = new List<Array<double>>();
            int i = 0;
            while (i < Levels.Values.Count)
            {
                Array<double> _newLevel = new Array<double>();
                SetIndexLabel(i + 2, string.Format("Level {0}", i + 1));
                SetIndexStyle(i + 2, DRAW_LINE, (int)Levels.Style, Levels.Width, Levels.Color);
                SetIndexBuffer(i + 2, _newLevel);
                _levels.Add(_newLevel);
                i++;
            }

            SetIndexBuffer(i + 2, _highesBuffer);
            SetIndexBuffer(i + 3, _lowesBuffer);

            return (0);
        }

        protected override int Start()
        {
            
            int l = 0;
            foreach (Array<double> _lvl in _levels)
            {
                for (int j = 0; j < _lvl.Count; j++)
                {
                    _lvl[j] = Levels.Values[l].Value;
                }
                l++;
            }

            int i, k;
            var counted_bars = IndicatorCounted();
            double price;

            if (Bars <= draw_begin2)
                return (0);

            var data = GetHistory(Symbol, TimeFrame);
            if (data.Count == 0)
                return 0;
            if (counted_bars < 1)
            {
                for (i = 1; i <= draw_begin1; i++)
                    _mainBuffer[Bars - i] = 0;
                for (i = 1; i <= draw_begin2; i++)
                    _signalBuffer[Bars - i] = 0;
            }

            i = Bars - KPeriod;
            if (counted_bars > KPeriod)
                i = Bars - counted_bars - 1;
            while (i >= 0)
            {
                double min = 1000000;
                k = i + KPeriod - 1;
                while (k >= i)
                {
                    price = (double)data[k].Low;
                    if (min > price)
                        min = price;
                    k--;
                }
                _lowesBuffer[i] = min;
                i--;
            }

            i = Bars - KPeriod;
            if (counted_bars > KPeriod)
                i = Bars - counted_bars - 1;
            while (i >= 0)
            {
                double max = -1000000;
                k = i + KPeriod - 1;
                while (k >= i)
                {
                    price = (double)data[k].High;
                    if (max < price)
                        max = price;
                    k--;
                }
                _highesBuffer[i] = max;
                i--;
            }

            i = Bars - draw_begin1;
            if (counted_bars > draw_begin1)
                i = Bars - counted_bars - 1;
            while (i >= 0)
            {
                var sumlow = 0.0;
                var sumhigh = 0.0;
                for (k = (i + Slowing - 1); k >= i; k--)
                {
                    sumlow += (double)data[k].Close - _lowesBuffer[k];
                    sumhigh += _highesBuffer[k] - _lowesBuffer[k];
                }
                if (sumhigh == 0.0)
                    _mainBuffer[i] = 100.0;
                else
                    _mainBuffer[i] = sumlow/sumhigh*100;
                i--;
            }

            if (counted_bars > 0)
                counted_bars--;
            var limit = Bars - counted_bars;

            for (i = 0; i < limit; i++)
                _signalBuffer[i] = iMAOnArray(_mainBuffer, Bars, DPeriod, 0, MODE_SMA, i);

            return (0);
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 7)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;
            if (!(values[2] is int) || (int)values[2] != KPeriod)
                return false;
            if (!(values[3] is int) || (int)values[3] != DPeriod)
                return false;
            if (!(values[4] is int) || (int)values[4] != Slowing)
                return false;
            if (!(values[5] is MovingAverageType) || (MovingAverageType)values[5] != MAType)
                return false;
            if (!(values[6] is PriceConstants) || (PriceConstants)values[6] != PriceType)
                return false;

            return true;
        }
    }
}