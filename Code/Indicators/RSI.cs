using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using System.Collections.Generic;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Relative Strength Index Indicator")]
    public class RSI : IndicatorBase
    {
        private readonly Array<double> _negBuf;
        private readonly Array<double> _posBuf;
        private readonly Array<double> _vals;
        private List<Array<double>> _levels;

        public RSI()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;

            IndicatorPeriod = 10;
            Levels.Values.Add(new Alveo.Interfaces.UserCode.Double(70));
            Levels.Values.Add(new Alveo.Interfaces.UserCode.Double(30));
            SetIndexLabel(0, string.Format("RSI({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("RSI({0})", IndicatorPeriod));

            PriceType = PriceConstants.PRICE_CLOSE;
            _vals = new Array<double>();
            _posBuf = new Array<double>();
            _negBuf = new Array<double>();
        }

        [Description("Period of the RSI Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        [Description("Price type on witch RSI will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            for (int x = 1; x < indicator_buffers; x++)
            {
                SetIndexBuffer(x, null);
            }
            indicator_buffers = Levels.Values.Count + 1;
            SetIndexLabel(0, string.Format("RSI({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("RSI({0})", IndicatorPeriod));


            SetIndexBuffer(0, _vals);

            _levels = new List<Array<double>>();
            int i = 0;
            while (i < Levels.Values.Count)
            {
                Array<double> _newLevel = new Array<double>();
                SetIndexLabel(i + 1, string.Format("Level {0}", i + 1));
                SetIndexStyle(i + 1, DRAW_LINE, (int)Levels.Style, Levels.Width, Levels.Color);
                SetIndexBuffer(i + 1, _newLevel);
                _levels.Add(_newLevel);
                i++;
            }

            SetIndexBuffer(i + 1, _posBuf);
            SetIndexBuffer(i + 2, _negBuf);
            return 0;
        }

        protected override int Start()
        {
            _vals[_vals.Count - 1] = 100.0;
            _vals[_vals.Count - 2] = 0.0;

            int l = 0;
            foreach (Array<double> _lvl in _levels)
            {
                for (int i = 0; i < _lvl.Count; i++)
                {
                    _lvl[i] = Levels.Values[l].Value;
                }
                l++;
            }

            if (Bars < IndicatorPeriod)
                return 0;

            var pos = Bars - IndicatorCounted();
            var data = GetHistory(Symbol, TimeFrame);
            if (data.Count == 0)
                return 0;
            if (pos > Bars - IndicatorPeriod - 1)
                pos = Bars - IndicatorPeriod - 1;

            decimal sumn = 0;
            decimal sump = 0;
            decimal rel;
            decimal positive;
            decimal negative;

            if (pos == Bars - IndicatorPeriod - 1)
            {
                var k = Bars - 2;
                while (k >= pos)
                {
                    rel = data[k].Close - data[k + 1].Close;
                    if (rel > 0)
                        sump += rel;
                    else
                        sumn -= rel;
                    k--;
                }

                positive = sump/IndicatorPeriod;
                negative = sumn/IndicatorPeriod;
                _posBuf[pos] = (double)positive;
                _negBuf[pos] = (double)negative;

                if (negative.Equals(0.0))
                    _vals[pos] = 0.0;
                else
                    _vals[pos] = (double)(100 - 100/(1 + positive/negative));

                pos--;
            }
            while (pos >= 0)
            {
                sumn = 0;
                sump = 0;
                rel = data[pos].Close - data[pos + 1].Close;
                if (rel > 0)
                    sump = rel;
                else
                    sumn = -rel;
                positive = ((decimal)_posBuf[pos + 1]*(IndicatorPeriod - 1) + sump)/IndicatorPeriod;
                negative = ((decimal)_negBuf[pos + 1]*(IndicatorPeriod - 1) + sumn)/IndicatorPeriod;
                _posBuf[pos] = (double)positive;
                _negBuf[pos] = (double)negative;

                if (negative.Equals(0.0))
                    _vals[pos] = 0.0;
                else
                    _vals[pos] = (double)(100 - 100/(1 + positive/negative));

                pos--;
            }
            return 0;
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
            if (!(values[2] is int) || (int)values[2] != IndicatorPeriod)
                return false;
            if (!(values[3] is PriceConstants) || (PriceConstants)values[3] != PriceType)
                return false;

            return true;
        }
    }
}