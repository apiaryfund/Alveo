using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Ichimoku Kinko Hyo Indicator")]
    public class Ichimoku : IndicatorBase
    {
        private readonly Array<double> _ChinkouSpan;
        private readonly Array<double> _KijunSen;
        private readonly Array<double> _SenkouSpanA;
        private readonly Array<double> _SenkouSpanA2;
        private readonly Array<double> _SenkouSpanB;
        private readonly Array<double> _SenkouSpanB2;
        private readonly Array<double> _TenkanSen;

        private int a_begin;

        public Ichimoku()
        {
            indicator_buffers = 7;
            indicator_chart_window = true;
            TenkanSen = 9;
            KijunSen = 26;
            SenkouSpanB = 52;

            indicator_color1 = Colors.Blue;
            indicator_color2 = Colors.Green;
            indicator_color3 = Colors.Red;
            indicator_color4 = Colors.Indigo;
            indicator_color5 = Colors.Orange;
            indicator_color6 = Colors.SaddleBrown;
            indicator_color7 = Colors.CornflowerBlue;

            SetIndexLabel(0, string.Format("Tenkan Sen({0})", TenkanSen));
            SetIndexLabel(1, string.Format("Kijun Sen({0})", KijunSen));
            SetIndexLabel(2, "Senkou Span A");
            SetIndexLabel(3, string.Format("Senkou Span B({0})", SenkouSpanB));
            SetIndexLabel(4, "Chinkou Span");
            SetIndexLabel(5, "SenkouSpanA2");
            SetIndexLabel(6, "SenkouSpanB2");

            IndicatorShortName(string.Format("Ichimoku({0},{1},{2})", TenkanSen, KijunSen, SenkouSpanB));

            _TenkanSen = new Array<double>();
            _KijunSen = new Array<double>();
            _SenkouSpanA = new Array<double>();
            _SenkouSpanB = new Array<double>();
            _ChinkouSpan = new Array<double>();
            _SenkouSpanA2 = new Array<double>();
            _SenkouSpanB2 = new Array<double>();
        }

        [Description("Tenkan Sen averaging period of the Ichimoku Indicator")]
        [Category("Settings")]
        [DisplayName("Tenkan Sen")]
        public int TenkanSen { get; set; }

        [Description("Kijun Sen averaging period of the Ichimoku Indicator")]
        [Category("Settings")]
        [DisplayName("Kijun Sen")]
        public int KijunSen { get; set; }

        [Description("Senkou SpanB averaging period of the Ichimoku Indicator")]
        [Category("Settings")]
        [DisplayName("Senkou Span-B")]
        public int SenkouSpanB { get; set; }

        protected override int Init()
        {
            //----
            SetIndexStyle(0, DRAW_LINE);
            SetIndexBuffer(0, _TenkanSen);
            SetIndexDrawBegin(0, TenkanSen - 1);
            SetIndexLabel(0, string.Format("Tenkan Sen({0})", TenkanSen));
            //----
            SetIndexStyle(1, DRAW_LINE);
            SetIndexBuffer(1, _KijunSen);
            SetIndexDrawBegin(1, KijunSen - 1);
            SetIndexLabel(1, string.Format("Kijun Sen({0})", KijunSen));
            //----
            a_begin = KijunSen;
            if (a_begin < TenkanSen)
                a_begin = TenkanSen;
            SetIndexStyle(2, DRAW_HISTOGRAM, STYLE_DOT);
            SetIndexBuffer(2, _SenkouSpanA);
            SetIndexDrawBegin(2, KijunSen + a_begin - 1);
            SetIndexShift(2, KijunSen);
            SetIndexStyle(5, DRAW_LINE, STYLE_DOT);
            SetIndexBuffer(5, _SenkouSpanA2);
            SetIndexDrawBegin(5, KijunSen + a_begin - 1);
            SetIndexShift(5, KijunSen);
            //----
            SetIndexStyle(3, DRAW_HISTOGRAM, STYLE_DOT);
            SetIndexBuffer(3, _SenkouSpanB);
            SetIndexDrawBegin(3, KijunSen + SenkouSpanB - 1);
            SetIndexShift(3, KijunSen);
            SetIndexStyle(6, DRAW_LINE, STYLE_DOT);
            SetIndexBuffer(6, _SenkouSpanB2);
            SetIndexDrawBegin(6, KijunSen + SenkouSpanB - 1);
            SetIndexShift(6, KijunSen);
            SetIndexLabel(3, string.Format("Senkou Span B({0})", SenkouSpanB));
            //----
            SetIndexStyle(4, DRAW_LINE);
            SetIndexBuffer(4, _ChinkouSpan);
            SetIndexShift(4, -KijunSen);

            IndicatorShortName(string.Format("Ichimoku({0},{1},{2})", TenkanSen, KijunSen, SenkouSpanB));

            return 0;
        }

        protected override int Start()
        {
            int i = 0;
            int k = 0;
            var pos = IndicatorCounted();
            double high = 0;
            double low = 0;
            double price = 0;

            if (Bars <= TenkanSen || Bars <= KijunSen || Bars <= SenkouSpanB)
                return 0;

            if (pos < 1)
            {
                for (i = 1; i < TenkanSen; i++) _TenkanSen[Bars - i] = 0;
                for (i = 1; i < KijunSen; i++) _KijunSen[Bars - i] = 0;
                for (i = 1; i < a_begin; i++) { _SenkouSpanA[Bars - i] = 0; _SenkouSpanA2[Bars - i] = 0; }
                for (i = 1; i < SenkouSpanB; i++) { _SenkouSpanB[Bars - i] = 0; _SenkouSpanB2[Bars - i] = 0; }
            }

            //---- Tenkan Sen
            i = Bars - TenkanSen;
            if (pos > TenkanSen)
                i = Bars - pos - 1;
            while (i >= 0)
            {
                high = High[i];
                low = Low[i];
                k = i + TenkanSen - 1;
                while (k >= i)
                {
                    price = High[k];
                    if (high < price)
                        high = price;
                    price = Low[k];
                    if (low > price)
                        low = price;
                    k--;
                }

                _TenkanSen[i] = (double)(high + low)/2;

                i--;
            }
            //---- Kijun Sen
            i = Bars - KijunSen;
            if (pos > KijunSen)
                i = Bars - pos - 1;
            while (i >= 0)
            {
                high = High[i];
                low = Low[i];
                k = i + KijunSen - 1;
                while (k >= i)
                {
                    price = High[k];
                    if (high < price)
                        high = price;
                    price = Low[k];
                    if (low > price)
                        low = price;
                    k--;
                }
                _KijunSen[i] = (double)(high + low)/2;
                i--;
            }
            //---- Senkou Span A
            i = Bars - a_begin + 1; // Bars - a_begin + 1;
            if (pos > a_begin - 1)
                i = Bars - pos - 1;
            while (i >= 0)
            {
                price = (_KijunSen[i] + _TenkanSen[i])/2;
                _SenkouSpanA[i] = price;
                _SenkouSpanA2[i] = price;
                i--;
            }
            //---- Senkou Span B
            i = Bars - SenkouSpanB;
            if (pos > SenkouSpanB)
                i = Bars - pos - 1;
            while (i >= 0)
            {
                high = High[i];
                low = Low[i];
                k = i - 1 + SenkouSpanB;
                while (k >= i)
                {
                    price = High[k];
                    if (high < price)
                        high = price;
                    price = Low[k];
                    if (low > price)
                        low = price;
                    k--;
                }
                price = (high + low)/2;
                _SenkouSpanB[i] = price;
                _SenkouSpanB2[i] = price;
                i--;
            }
            //---- Chinkou Span
            i = Bars - 1;
            if (pos > 1) i = Bars - pos - 1;
            while (i >= 0)
            {
                _ChinkouSpan[i] = Close[i];
                i--;
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
            if (!(values[2] is int) || (int)values[2] != TenkanSen)
                return false;
            if (!(values[3] is int) || (int)values[3] != KijunSen)
                return false;
            if (!(values[4] is int) || (int)values[4] != SenkouSpanB)
                return false;

            return true;
        }
    }
}