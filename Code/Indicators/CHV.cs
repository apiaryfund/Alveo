using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Chaikin Volatility")]
    public class CHV : IndicatorBase
    {
        private readonly Array<double> chvBuffer;
        private readonly Array<double> hl;

        public CHV()
        {
            indicator_chart_window = false;
            indicator_buffers = 1;
            indicator_color1 = Colors.Navy;

            SmoothPeriod = 10;
            ROCPeriod = 10;
            TypeSmooth = 1;

            chvBuffer = new Array<double>();
            hl = new Array<double>();
        }

        [Category("Settings")]
        [Description("Smooth Period")]
        [DisplayName("Smooth Period")]
        public int SmoothPeriod { get; set; }

        [Category("Settings")]
        [Description("ROC Period")]
        [DisplayName("ROC Period")]
        public int ROCPeriod { get; set; }

        [Category("Settings")]
        [Description("0 - SMA, 1 - EMA")]
        [DisplayName("Smooth Type")]
        public int TypeSmooth { get; set; }

        protected override int Init()
        {
            string smoothStr;
            if(TypeSmooth < 0 || TypeSmooth > 1)
                TypeSmooth = 1;
            if(TypeSmooth == 0)
                smoothStr = "SMA";
            else
                smoothStr = "EMA";

            IndicatorBuffers(2);
            SetIndexBuffer(0, chvBuffer);
            SetIndexStyle(0, DRAW_LINE);
            SetIndexEmptyValue(0, 0);
            SetIndexLabel(0, string.Format("CHV({0},{1})", SmoothPeriod, smoothStr));

            SetIndexBuffer(1, hl);
            SetIndexEmptyValue(1, 0);

            IndicatorShortName(string.Format("Chaikin Volatility({0},{1})", SmoothPeriod, smoothStr));
            IndicatorDigits(1);
            return 0;
        }

        protected override int Start()
        {
            int pos = IndicatorCounted();
            int limit = 0;
            int i = 0;
            double currValue = 0;
            double shiftValue = 0;

            if (pos < 0)
                return -1;
            if(pos == 0)
            {
                limit = Bars - 1;
                for(i = limit; i >= 0; i--)
                    hl[i] = High[i] - Low[i];

                for(i = limit-2*SmoothPeriod; i >= 0; i--)
                {
                    currValue = iMAOnArray(hl, 0, SmoothPeriod, 0, TypeSmooth, i);
                    shiftValue = iMAOnArray(hl, 0, SmoothPeriod, 0, TypeSmooth, i + ROCPeriod);
                    chvBuffer[i] = (currValue - shiftValue) / shiftValue * 100;
                }

            }
            if(pos > 0)
            {
                limit = Bars - pos;
                for (i = limit; i >= 0; i--)
                    hl[i] = High[i] - Low[i];

                for(i = limit; i >= 0; i--)
                {
                    currValue = iMAOnArray(hl, 0, SmoothPeriod, 0, TypeSmooth, i);
                    shiftValue = iMAOnArray(hl, 0, SmoothPeriod, 0, TypeSmooth, i + ROCPeriod);
                    chvBuffer[i] = (currValue - shiftValue) / shiftValue * 100;
                }
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
            if (!(values[2] is int) || (int)values[2] != SmoothPeriod)
                return false;
            if (!(values[3] is int) || (int)values[3] != ROCPeriod)
                return false;
            if (!(values[4] is int) || (int)values[4] != TypeSmooth)
                return false;
            return true;
        }
    }
}
