using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using System.Collections.Generic;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Commodity Channel Index Indicator")]
    public class CCI : IndicatorBase
    {
        private readonly Array<double> _vals;
        private List<Array<double>> _levels;

        public CCI()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;
            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("CCI({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("CCI({0})", IndicatorPeriod));
            PriceType = PriceConstants.PRICE_CLOSE;
            _vals = new Array<double>();
        }

        [Description("Period of the CCI Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        [Description("Price type on witch CCI will be calculated")]
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
            SetIndexLabel(0, string.Format("CCI({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("CCI({0})", IndicatorPeriod));
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


            var pos = Bars - IndicatorCounted();
            if (pos > Bars - IndicatorPeriod)
                pos = Bars - IndicatorPeriod;
            var baseArray = GetPrice(GetHistory(Symbol, TimeFrame), PriceType);
            if (baseArray.Count == 0)
                return 0;

            double sum;
            double ma;
            double d;
            double r;
            var mul = 0.015/IndicatorPeriod;
            int k;

            while (pos >= 0)
            {
                sum = 0;
                k = pos + IndicatorPeriod - 1;
                ma = iMA(null, 0, IndicatorPeriod, 0, MODE_SMA, (int)PriceType, pos);
                while (k >= pos)
                {
                    sum += MathAbs(baseArray[k] - ma);
                    k--;
                }
                d = sum*mul;
                r = baseArray[pos] - ma;
                if (d.Equals(0))
                    _vals[pos] = 0;
                else
                    _vals[pos] = r/d;

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