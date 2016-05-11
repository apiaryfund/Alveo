using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Fractals Indicator")]
    public class Fractals : IndicatorBase
    {
        private Array<double> _lowVals;
        private Array<double> _upVals;

        public Fractals()
        {
            indicator_buffers = 2;
            indicator_chart_window = true;
            indicator_color1 = Colors.Green;
            indicator_color2 = Colors.Red;
            SetIndexLabel(0, "Frac_Up");
            SetIndexLabel(1, "Frac_Dn");
            IndicatorShortName("Fractals");
            _upVals = new Array<double>();
            _lowVals = new Array<double>();
        }

        protected override int Init()
        {
            SetIndexStyle(0, DRAW_ARROW);
            SetIndexStyle(1, DRAW_ARROW);
            SetIndexArrow(0, 242);
            SetIndexArrow(1, 241);
            SetIndexBuffer(0, _upVals);
            SetIndexBuffer(1, _lowVals);

            return 0;
        }

        protected override int Start()
        {
            int pos = 0;
            int nCountedBars = IndicatorCounted();
            if (nCountedBars <= 2)
                pos = Bars - nCountedBars - 3;
            if (nCountedBars > 2)
            {
                nCountedBars--;
                pos = Bars - nCountedBars - 1;
            }

            double upVal;
            double downVal;
            double dCurrent;
            bool bFound;

            while (pos >= 2)
            {
                upVal = EMPTY_VALUE;
                downVal = EMPTY_VALUE;
                //----Fractals up
                bFound = false;
                dCurrent = High[pos];
                if (dCurrent > High[pos + 1] && dCurrent > High[pos + 2] && dCurrent > High[pos - 1]
                    && dCurrent > High[pos - 2])
                {
                    bFound = true;
                    upVal = dCurrent;
                }
                //----6 bars Fractal
                if (!bFound && (Bars - pos - 1) >= 3)
                {
                    if (dCurrent == High[pos + 1] && dCurrent > High[pos + 2] && dCurrent > High[pos + 3]
                        && dCurrent > High[pos - 1] && dCurrent > High[pos - 2])
                    {
                        bFound = true;
                        upVal = dCurrent;
                    }
                }
                //----7 bars Fractal
                if (!bFound && (Bars - pos - 1) >= 4)
                {
                    if (dCurrent >= High[pos + 1] && dCurrent == High[pos + 2]
                        && dCurrent > High[pos + 3] && dCurrent > High[pos + 4]
                        && dCurrent > High[pos - 1] && dCurrent > High[pos - 2])
                    {
                        bFound = true;
                        upVal = dCurrent;
                    }
                }
                //----8 bars Fractal                          
                if (!bFound && (Bars - pos - 1) >= 5)
                {
                    if (dCurrent >= High[pos + 1] && dCurrent == High[pos + 2]
                        && dCurrent == High[pos + 3] && dCurrent > High[pos + 4]
                        && dCurrent > High[pos + 5] && dCurrent > High[pos - 1]
                        && dCurrent > High[pos - 2])
                    {
                        bFound = true;
                        upVal = dCurrent;
                    }
                }
                //----9 bars Fractal                                        
                if (!bFound && (Bars - pos - 1) >= 6)
                {
                    if (dCurrent >= High[pos + 1] && dCurrent == High[pos + 2]
                        && dCurrent >= High[pos + 3] && dCurrent == High[pos + 4]
                        && dCurrent > High[pos + 5] && dCurrent > High[pos + 6]
                        && dCurrent > High[pos - 1] && dCurrent > High[pos - 2])
                    {
                        bFound = true;
                        upVal = dCurrent;
                    }
                }
                //----Fractals down
                bFound = false;
                dCurrent = Low[pos];
                if (dCurrent < Low[pos + 1] && dCurrent < Low[pos + 2] && dCurrent < Low[pos - 1]
                    && dCurrent < Low[pos - 2])
                {
                    bFound = true;
                    downVal = dCurrent;
                }
                //----6 bars Fractal
                if (!bFound && (Bars - pos - 1) >= 3)
                {
                    if (dCurrent == Low[pos + 1] && dCurrent < Low[pos + 2] && dCurrent < Low[pos + 3]
                        && dCurrent < Low[pos - 1] && dCurrent < Low[pos - 2])
                    {
                        bFound = true;
                        downVal = dCurrent;
                    }
                }
                //----7 bars Fractal
                if (!bFound && (Bars - pos - 1) >= 4)
                {
                    if (dCurrent <= Low[pos + 1] && dCurrent == Low[pos + 2] && dCurrent < Low[pos + 3]
                        && dCurrent < Low[pos + 4] && dCurrent < Low[pos - 1] && dCurrent < Low[pos - 2])
                    {
                        bFound = true;
                        downVal = dCurrent;
                    }
                }
                //----8 bars Fractal                          
                if (!bFound && (Bars - pos - 1) >= 5)
                {
                    if (dCurrent <= Low[pos + 1] && dCurrent == Low[pos + 2] && dCurrent == Low[pos + 3]
                        && dCurrent < Low[pos + 4] && dCurrent < Low[pos + 5] && dCurrent < Low[pos - 1]
                        && dCurrent < Low[pos - 2])
                    {
                        bFound = true;
                        downVal = dCurrent;
                    }
                }
                //----9 bars Fractal                                        
                if (!bFound && (Bars - pos - 1) >= 6)
                {
                    if (dCurrent <= Low[pos + 1] && dCurrent == Low[pos + 2] && dCurrent <= Low[pos + 3]
                        && dCurrent == Low[pos + 4] && dCurrent < Low[pos + 5] && dCurrent < Low[pos + 6]
                        && dCurrent < Low[pos - 1] && dCurrent < Low[pos - 2])
                        downVal = dCurrent;
                }
                _upVals[pos] = (double)upVal;
                _lowVals[pos] = (double)downVal;

                pos--;
            }

            return 0;
        }

        public override bool IsSameParameters(params object[] values)
        {
            if (values.Length != 2)
                return false;
            if ((values[0] != null && Symbol == null) || (values[0] == null && Symbol != null))
                return false;
            if (values[0] != null && (!(values[0] is string) || (string)values[0] != Symbol))
                return false;
            if (!(values[1] is int) || (int)values[1] != TimeFrame)
                return false;

            return true;
        }
    }
}