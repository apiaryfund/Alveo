using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Bill Williams' Awesome Indicator")]
    public class AO : IndicatorBase
    {
        public static int DefaultPeriod1 = 5;
        public static int DefaultPeriod2 = 34;
        public static MovingAverageType DefaultSmoothing = MovingAverageType.MODE_SMA;
        public static PriceConstants DefaultPriceConstants = PriceConstants.PRICE_MEDIAN;
        private readonly Array<double> _vals;

        public AO()
        {
            indicator_buffers = 1;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;

            _vals = new Array<double>();

            Period1 = DefaultPeriod1;
            Period2 = DefaultPeriod2;
            SetIndexLabel(0, string.Format("AO({0},{1})", Period1, Period2));
            IndicatorShortName(string.Format("AO({0},{1})", Period1, Period2));

            Smoothing = DefaultSmoothing;
            PriceBase = DefaultPriceConstants;
        }

        [Description("Fast MA Period")]
        [Category("Settings")]
        [DisplayName("Fast MA")]
        public int Period1 { get; set; }

        [Description("Slow MA Period")]
        [Category("Settings")]
        [DisplayName("Slow MA")]
        public int Period2 { get; set; }

        [Description("Smoothing method")]
        [Category("Settings")]
        [DisplayName("Smoothing method")]
        public MovingAverageType Smoothing { get; set; }

        [Description("Base price")]
        [Category("Settings")]
        [DisplayName("Base price")]
        public PriceConstants PriceBase { get; set; }

        protected override int Init()
        {
            SetIndexLabel(0, string.Format("AO({0},{1})", Period1, Period2));
            IndicatorShortName(string.Format("AO({0},{1})", Period1, Period2));
            SetIndexStyle(0, DRAW_HISTOGRAM);
            SetIndexBuffer(0, _vals);

            return 0;
        }

        protected override int Start()
        {
            double price1, price2, price3;
            var pos = Bars - IndicatorCounted();
            if (pos > Bars - Period2)
                pos = Bars - Period2;

            while (pos >= 0)
            {
                price1 = iMA(Symbol, TimeFrame, Period1, 0, (int)Smoothing, (int)PriceBase, pos);
                price2 = iMA(Symbol, TimeFrame, Period2, 0, (int)Smoothing, (int)PriceBase, pos);
                price3 = (price1 - price2);
                _vals[pos] = price3;
                pos--;
            }

            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 6)
                return false;

            try
            {
                var val0 = (string)values[0];
                if (!string.IsNullOrEmpty(val0) && !string.IsNullOrEmpty(Symbol))
                {
                    if (!Symbol.Equals(val0, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                else if (!(string.IsNullOrEmpty(Symbol) && string.IsNullOrEmpty(val0)))
                    return false;

                if ((int)values[1] != TimeFrame)
                    return false;

                if ((int)values[2] != Period1)
                    return false;

                if ((int)values[3] != Period2)
                    return false;

                if ((MovingAverageType)values[4] != Smoothing)
                    return false;

                if ((PriceConstants)values[5] != PriceBase)
                    return false;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("IsSameParameters", ex);
            }

            return true;
        }
    }
}