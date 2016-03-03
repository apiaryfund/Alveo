
using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using Alveo.UserCode;
using Alveo.Common;
using Alveo.Common.Classes;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Display MA in color based on trend direction")]
    public class Color_MA : IndicatorBase
   {

        #region Properties

        [Category("Settings")]
        public int MAPeriod{ get; set; }

        [Category("Settings")]
        public int MAType{ get; set; }

        #endregion

        public Color_MA()
        {
            copyright = "JF";
            link = "";
            indicator_chart_window = true;
            indicator_buffers = 3;
            indicator_color1 = Yellow;
            indicator_color2 = Green;
            indicator_color3 = Red;

            MAPeriod = 50;
            MAType = 0;

        }
        Array<double> ExtMapBuffer1 = new Array<double>();
        Array<double> ExtMapBuffer2 = new Array<double>();
        Array<double> ExtMapBuffer3 = new Array<double>();

        int    MAMode;
        string strMAType;
        protected override int Init()
        {
            IndicatorBuffers(3);

            SetIndexBuffer(2,ExtMapBuffer1);
            SetIndexBuffer(1,ExtMapBuffer2);
            SetIndexBuffer(0,ExtMapBuffer3);

            SetIndexStyle(2,DRAW_LINE,STYLE_SOLID,2);
            SetIndexStyle(1,DRAW_LINE,STYLE_SOLID,2);
            SetIndexStyle(0,DRAW_LINE,STYLE_SOLID,2);

            switch (MAType)
            {
                case 1: strMAType="EMA"; MAMode=MODE_EMA; break;
                case 2: strMAType="SMMA"; MAMode=MODE_SMMA; break;
                case 3: strMAType="LWMA"; MAMode=MODE_LWMA; break;
                case 4: strMAType="LSMA"; break;
                default: strMAType="SMA"; MAMode=MODE_SMA; break;
            }
            IndicatorShortName( strMAType+ " (" +MAPeriod + ") ");

            return(0);
        }

        double LSMA(int Rperiod, int shift)
        {
            int i;
            double sum;
            int length;
            double lengthvar;
            double tmp;
            double wt;

            length = Rperiod;

            sum = 0;
            for(i = length; i >= 1  ; i--)
            {
                lengthvar = length + 1;
                lengthvar /= 3;
                tmp = 0;
                tmp = ( i - lengthvar)*Close[length-i+shift];
                sum+=tmp;
            }
            wt = sum*6/(length*(length+1));

            return(wt);
        }
        protected override int Start()

        {

            double MA_Cur, MA_Prev;
            int limit;
            int counted_bars = IndicatorCounted();

            if (counted_bars<0) return(-1);

            if (counted_bars>0) counted_bars--;
            limit = Bars - counted_bars;

            int i;
            for(i=limit; i>=0; i--)
            {
                if (MAType == 4)
                {
                    MA_Cur = LSMA(MAPeriod,i);
                    MA_Prev = LSMA(MAPeriod,i+1);
                }
                else
                {
                    MA_Cur = iMA(null,0,MAPeriod,0,MAMode,PRICE_CLOSE,i);
                    MA_Prev = iMA(null,0,MAPeriod,0,MAMode,PRICE_CLOSE,i+1);
                }

                ExtMapBuffer3[i] = MA_Cur;
                ExtMapBuffer2[i] = MA_Cur;
                ExtMapBuffer1[i] = MA_Cur;

                if (MA_Prev > MA_Cur)
                {
                    ExtMapBuffer2[i] = EMPTY_VALUE;

                }
                else if (MA_Prev < MA_Cur)
                {
                    ExtMapBuffer1[i] = EMPTY_VALUE;

                }
                else
                {

                    ExtMapBuffer1[i]=EMPTY_VALUE;
                    ExtMapBuffer2[i]=EMPTY_VALUE;
                }

            }

            return(0);
        }

        #region Indicator parameters check

        [Description("Parameters order Symbol, TimeFrame, MAPeriod MAType")]
        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 4)
            return false;

            try
            {

                var val0 = (string)values[0];
                if (!string.IsNullOrEmpty(val0) && !string.IsNullOrEmpty(Symbol))
                {
                    if (!Symbol.Equals(val0, StringComparison.OrdinalIgnoreCase))
                    return false;
                }
                else if(!(string.IsNullOrEmpty(Symbol) && string.IsNullOrEmpty(val0)))
                return false;

                var val1 = (int) values[1];
                if (val1 != TimeFrame)
                return false;

                var val2 = (int) values[2];
                if (val2 != MAPeriod)
                return false;

                var val3 = (int) values[3];
                if (val3 != MAType)
                return false;

            }
            catch { return false; }

            return true;
        }

        [Description("Parameters order Symbol, TimeFrame, MAPeriod MAType")]
        public override void SetIndicatorParameters(params object[] values)
        {
            if (values.Length != 4)
            return;

            try
            {

                Symbol = (string)values[0];
                TimeFrame = (int) values[1];
                MAPeriod = (int)values[2];
                MAType = (int)values[3];

            }
            catch {}
        }

        #endregion

    }
}
