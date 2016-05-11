using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Average True Range Indicator")]
    public class ATR : IndicatorBase
    {
        private readonly Array<double> _tr;
        private readonly Array<double> _vals;

        public ATR()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;

            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("ATR({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("ATR({0})", IndicatorPeriod));
            _vals = new Array<double>();
            _tr = new Array<double>();
            IndicatorDigits(5);
        }

        [Description("Period of the ATR Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("ATR({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("ATR({0})", IndicatorPeriod));
            SetIndexBuffer(0, _vals);
            ArraySetAsSeries(_tr, true);
            return 0;
        }

        protected override int Start()
        {
            var start = IndicatorCounted();
            var baseBars = GetHistory(Symbol, TimeFrame);
            if (baseBars.Count == 0)
                return 0;
            while (_tr.Count < Bars)
                _tr.Add(EMPTY_VALUE);

            for (var i = start; i < Math.Min(Bars, baseBars.Count); i++)
            {
                if (i == 0)
                    _tr[i, false] = (double)baseBars[i, false].High - (double)baseBars[i, false].Low;
                else
                {
                    _tr[i, false] =
                        (double)
                        Math.Max(baseBars[i, false].High - baseBars[i, false].Low,
                            Math.Max(Math.Abs(baseBars[i, false].High - baseBars[i - 1, false].Close),
                                Math.Abs(baseBars[i, false].Low - baseBars[i - 1, false].Close)));
                }

                if (i == IndicatorPeriod - 1)
                {
                    var sum = 0.0;
                    for (var j = 0; j < IndicatorPeriod; j++)
                        sum += _tr[j, false];
                    _vals[i, false] = sum/IndicatorPeriod;
                }
                else if (i >= IndicatorPeriod)
                {
                    _vals[i, false] = _vals[i - 1, false]
                                      + (_tr[i, false] - _tr[i - IndicatorPeriod, false])/IndicatorPeriod;
                }
            }

            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 3)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;
            if (!(values[2] is int) || (int)values[2] != IndicatorPeriod)
                return false;

            return true;
        }
    }
}