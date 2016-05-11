using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Bill Williams' Accelerator/Decelerator Indicator")]
    public class AC : IndicatorBase
    {
        private readonly Array<double> _vals;

        public AC()
        {
            indicator_buffers = 1;
            Period1 = 5;
            Period2 = 34;
            Period3 = 5;
            indicator_chart_window = false;
            indicator_color1 = Colors.Red;

            _vals = new Array<double>();

            SetIndexLabel(0, string.Format("AC({0},{1},{2})", Period1, Period2, Period3));
            IndicatorShortName(string.Format("AC({0},{1},{2})", Period1, Period2, Period3));
            Smoothing = MovingAverageType.MODE_SMA;
            PriceBase = PriceConstants.PRICE_MEDIAN;
        }

        [Description("Fast MA Period")]
        [Category("Settings")]
        [DisplayName("Fast MA")]
        public int Period1 { get; set; }

        [Description("Slow MA Period")]
        [Category("Settings")]
        [DisplayName("Slow MA")]
        public int Period2 { get; set; }

        [Description("Forming MA Period")]
        [Category("Settings")]
        [DisplayName("Forming MA")]
        public int Period3 { get; set; }

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
            SetIndexLabel(0, string.Format("AC({0},{1},{2})", Period1, Period2, Period3));
            IndicatorShortName(string.Format("AC({0},{1},{2})", Period1, Period2, Period3));
            SetIndexBuffer(0, _vals);

            return 0;
        }

        protected override int Start()
        {
            double price1, price2, price3, price4;
            var pos = Bars - IndicatorCounted();
            pos += Period3;
            if (pos > Bars - Period2)
                pos = Bars - Period2;

            double sum = 0;
            for (var i = 1; i < Period3; i++, pos--)
            {
                price1 = iMA(Symbol, TimeFrame, Period1, 0, (int)Smoothing, (int)PriceBase, pos);
                price2 = iMA(Symbol, TimeFrame, Period2, 0, (int)Smoothing, (int)PriceBase, pos);
                price3 = (price1 - price2);
                sum += price3;
            }

            while (pos >= 0)
            {
                price1 = iMA(Symbol, TimeFrame, Period1, 0, (int)Smoothing, (int)PriceBase, pos);
                price2 = iMA(Symbol, TimeFrame, Period2, 0, (int)Smoothing, (int)PriceBase, pos);
                price3 = (price1 - price2);
                sum += price3;
                price4 = sum/Period3;
                _vals[pos] = price3 - price4;

                price1 = iMA(Symbol, TimeFrame, Period1, 0, (int)Smoothing, (int)PriceBase, pos + Period3 - 1);
                price2 = iMA(Symbol, TimeFrame, Period2, 0, (int)Smoothing, (int)PriceBase, pos + Period3 - 1);
                price3 = (price1 - price2);
                sum -= price3;
                pos--;
            }

            return 0;
        }

        [Description("Parameters order Symbol, TimeFrame, Period1, Period2, Period3, Smoothing, PriceBase")]
        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 7)
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

                if ((int)values[4] != Period3)
                    return false;

                if ((MovingAverageType)values[5] != Smoothing)
                    return false;

                if ((PriceConstants)values[6] != PriceBase)
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