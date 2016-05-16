using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Parabolic Stop and Reverse Indicator")]
    public class SAR : IndicatorBase
    {
        private readonly Array<double> _vals;

        private bool _first = true;
        private bool save_dirlong;
        private double save_ep;
        private double save_last_high;
        private double save_last_low;
        private int save_lastreverse;
        private double save_sar;
        private double save_start;

        public SAR()
        {
            indicator_buffers = 1;
            indicator_chart_window = true;
            indicator_color1 = Colors.Red;

            Step = 0.02;
            Maximum = 0.2;
            SetIndexLabel(0, string.Format("SAR({0},{1})", Step, Maximum));
            IndicatorShortName(string.Format("SAR({0},{1})", Step, Maximum));

            _vals = new Array<double>();
        }

        [Description("Increment of SAR indicator")]
        [Category("Settings")]
        public double Step { get; set; }

        [Description("Maximum value of SAR indicator")]
        [Category("Settings")]
        public double Maximum { get; set; }

        protected override int Init()
        {
            SetIndexStyle(0, DRAW_ARROW);
            SetIndexArrow(0, 159);
            SetIndexLabel(0, string.Format("SAR({0},{1})", Step, Maximum));
            IndicatorShortName(string.Format("SAR({0},{1})", Step, Maximum));
            SetIndexBuffer(0, _vals);

            return 0;
        }

        protected override int Start()
        {
            var pos = Bars - IndicatorCounted();
            var data = GetHistory(Symbol, TimeFrame);
            if (data.Count == 0)
                return 0;

            bool dirlong;
            double start, last_high, last_low;
            double ep, sar, price_low, price_high, price;
            //---- nothing to calculate
            //if (Bars < 3) return (0);
            //---- first calculation?
            if (pos >= Bars - 1 || _first)
            {
                //---- initial settings
                pos = Bars - 2;
                _first = false;
                dirlong = true;
                start = Step;
                last_high = -10000000.0;
                last_low = 10000000.0;
                save_lastreverse = 0;
                sar = 0;
                while (pos > 0)
                {
                    save_lastreverse = pos;
                    price_low = (double)data[pos].Low;
                    price_high = (double)data[pos].High;
                    if (last_low > price_low)
                        last_low = price_low;
                    if (last_high < price_high)
                        last_high = price_high;
                    if (price_high > (double)data[pos + 1].High && price_low > (double)data[pos + 1].Low)
                        break;
                    if (price_high < (double)data[pos + 1].High && price_low < (double)data[pos + 1].Low)
                    {
                        dirlong = false;
                        break;
                    }
                    pos--;
                }
                //---- initial zero
                var k = pos;
                while (k < Bars)
                {
                    _vals[k] = 0.0;
                    k++;
                }
                //---- check further
                if (dirlong)
                {
                    _vals[pos] = (double)data[pos + 1].Low;
                    ep = (double)data[pos].High;
                }
                else
                {
                    _vals[pos] = (double)data[pos + 1].High;
                    ep = (double)data[pos].Low;
                }
                pos--;
            }
            else
            {
                //---- restore values from previous call to avoid full recalculation
                pos = save_lastreverse;
                start = save_start;
                dirlong = save_dirlong;
                last_high = save_last_high;
                last_low = save_last_low;
                ep = save_ep;
                sar = save_sar;
            }
            //----
            while (pos >= 0)
            {
                price_low = (double)data[pos].Low;
                price_high = (double)data[pos].High;
                price = _vals[pos + 1];
                //--- check for reverse
                if (dirlong && price_low < price)
                {
                    SaveLastReverse(pos + 1, true, start, price_low, last_high, ep, sar);
                    start = Step;
                    dirlong = false;
                    ep = price_low;
                    last_low = price_low;
                    _vals[pos] = last_high;
                    pos--;
                    continue;
                }
                if (!dirlong && price_high > price)
                {
                    SaveLastReverse(pos + 1, false, start, last_low, price_high, ep, sar);
                    start = Step;
                    dirlong = true;
                    ep = price_high;
                    last_high = price_high;
                    _vals[pos] = last_low;
                    pos--;
                    continue;
                }
                //--- calculate current value
                sar = price + start*(ep - price);
                //---- check long direction
                if (dirlong)
                {
                    if (ep < price_high && (start + Step) <= Maximum)
                        start += Step;
                    if (price_high < (double)data[pos + 1].High && pos == Bars - 2)
                        sar = price;

                    if (sar > (double)data[pos + 1].Low)
                        sar = (double)data[pos + 1].Low;
                    if (sar > (double)data[pos + 2].Low)
                        sar = (double)data[pos + 2].Low;

                    if (sar > price_low)
                    {
                        SaveLastReverse(pos + 1, true, start, price_low, last_high, ep, sar);
                        start = Step;
                        dirlong = false;
                        ep = price_low;
                        last_low = price_low;
                        _vals[pos] = last_high;
                        pos--;
                        continue;
                    }
                    if (ep < price_high)
                    {
                        last_high = price_high;
                        ep = price_high;
                    }
                }
                else
                {
                    if (ep > price_low && (start + Step) <= Maximum)
                        start += Step;
                    if (price_low < (double)data[pos + 1].Low && pos == Bars - 2)
                        sar = price;

                    if (sar < (double)data[pos + 1].High)
                        sar = (double)data[pos + 1].High;
                    if (sar < (double)data[pos + 2].High)
                        sar = (double)data[pos + 2].High;

                    if (sar < price_high)
                    {
                        SaveLastReverse(pos + 1, false, start, last_low, price_high, ep, sar);
                        start = Step;
                        dirlong = true;
                        ep = price_high;
                        last_high = price_high;
                        _vals[pos] = last_low;
                        pos--;
                        continue;
                    }
                    if (ep > price_low)
                    {
                        last_low = price_low;
                        ep = price_low;
                    }
                }
                _vals[pos] = sar;
                pos--;
            }

            return 0;
        }

        private void SaveLastReverse(int last, bool dir, double start, double low, double high, double ep, double sar)
        {
            save_lastreverse = last;
            save_dirlong = dir;
            save_start = start;
            save_last_low = low;
            save_last_high = high;
            save_ep = ep;
            save_sar = sar;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 4)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;
            if (!(values[2] is double) || !Step.Equals((double)values[2]))
                return false;
            if (!(values[3] is double) || !Maximum.Equals((double)values[3]))
                return false;

            return true;
        }
    }
}
