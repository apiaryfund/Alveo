using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Simple Moving Avarage Indicator")]
    public class SMA : IndicatorBase
    {
        private readonly Array<double> _values;

        public SMA()
        {
            indicator_buffers = 1;
            indicator_chart_window = true;
            indicator_color1 = Colors.Red;

            IndicatorPeriod = 10;
            SetIndexLabel(0, string.Format("SMA({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("SMA({0})", IndicatorPeriod));

            PriceType = PriceConstants.PRICE_CLOSE;
            _values = new Array<double>();
        }

        [Description("Period of the SMA Indicator")]
        [Category("Settings")]
        [DisplayName("Period")]
        public int IndicatorPeriod { get; set; }

        [Description("Price type on witch SMA will be calculated")]
        [Category("Settings")]
        [DisplayName("Price Type")]
        public PriceConstants PriceType { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("SMA({0})", IndicatorPeriod));
            IndicatorShortName(string.Format("SMA({0})", IndicatorPeriod));
            SetIndexBuffer(0, _values);

            return 0;
        }

        protected override int Start()
        {
            var baseArray = GetPrice(GetHistory(Symbol, TimeFrame), PriceType);
            if (baseArray.Count == 0)
                return 0;
            for (var i = IndicatorCounted(); i < Math.Min(baseArray.Count, Bars); i++)
            {
                if (i < IndicatorPeriod - 1)
                    _values[i, false] = EMPTY_VALUE;
                else if (i == IndicatorPeriod - 1)
                {
                    var sum = 0.0;
                    for (var j = 0; j < IndicatorPeriod; j++)
                        sum += baseArray[j, false];
                    _values[i, false] = sum/IndicatorPeriod;
                }
                else
                {
                    _values[i, false] = _values[i - 1, false]
                                        + (baseArray[i, false] - baseArray[i - IndicatorPeriod, false])/IndicatorPeriod;
                }
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

        [Description(
            "Parameters order Symbol, TimeFrame, maPeriod_1, maMethod_1, maAppPrice_1, maPeriod_2, maMethod_2, maPeriod_3 maMethod_3"
            )]
        public override void SetIndicatorParameters(params object[] values)
        {
            if (values.Length != 4)
                return;

            try
            {
                Symbol = (string)values[0];
                TimeFrame = (int)values[1];
                IndicatorPeriod = (int)values[2];
                PriceType = (PriceConstants)values[3];
            }
            catch (Exception ex)
            {
                Logger.ErrorException("SetIndicatorParameters", ex);
            }
        }
    }
}