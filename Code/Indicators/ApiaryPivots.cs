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
    [Description("")]
    //The Following is some sample code for creating an Indicator using the Alveo Platform and C#
    public class ApiaryPivots : IndicatorBase
    {
        #region Properties
        
        //Declare Global Variables--
        //These Two Arrays will be used to define
        //When something should be drawn on the chart
        private Array<double> pivotHighs;
        private Array<double> pivotLows;

        private Array<double> majorPivotHighs;
        private Array<double> majorPivotLows;
        
        #endregion
        
        
        public ApiaryPivots()
        {
            // Basic indicator initialization.
            //Define number of chart buffers etc.
            indicator_buffers = 4;
            indicator_chart_window = true;
            
            //Eternal Variables
            pipDiff = 0.08;
            posDiff = 3;
            pivotIcon = 117;

            indicator_color1 = Colors.Orange;
            indicator_color2 = Colors.Orange;
            indicator_color3 = Colors.Red;
            indicator_color4 = Colors.Red;
            
            //Allocate memory for our global Array's
            pivotHighs = new Array<double>();
            pivotLows = new Array<double>();
            majorPivotHighs = new Array<double>();
            majorPivotLows = new Array<double>();
            
            copyright = "Apiary Fund, LLC";
            link = "https://apiaryfund.com";
        }
        
        //Declare Our External Values
        [Category("Settings")]
        [DisplayName("Vertical Variation")]
        public double pipDiff { get; set; }
        
        [Category("Settings")]
        [DisplayName("Horizontal Variation")]
        public int posDiff { get; set; }
        
        [Category("Settings (Appearance)")]
        [DisplayName("Pivot WingDing")]
        public int pivotIcon { get; set; }
        
        //+------------------------------------------------------------------+");
        //| Custom indicator initialization function                         |");
        //+------------------------------------------------------------------+");
        protected override int Init()
        {
            //Be sure to set the attributes for each chart buffer
            SetIndexStyle(0, DRAW_ARROW, STYLE_DOT);
            SetIndexArrow(0, pivotIcon);
            SetIndexBuffer(0, pivotHighs);
            SetIndexLabel(0, "Pivot High");
            
            SetIndexStyle(1, DRAW_ARROW, STYLE_DOT);
            SetIndexArrow(1, pivotIcon);
            SetIndexBuffer(1, pivotLows);
            SetIndexLabel(1, "Pivot Low");

            SetIndexStyle(2, DRAW_ARROW, STYLE_DOT);
            SetIndexArrow(2, pivotIcon);
            SetIndexBuffer(2, majorPivotHighs);
            SetIndexLabel(2, "Major Pivot High");

            SetIndexStyle(3, DRAW_ARROW, STYLE_DOT);
            SetIndexArrow(3, pivotIcon);
            SetIndexBuffer(3, majorPivotLows);
            SetIndexLabel(3, "Major Pivot Low");
           

            return 0;
        }
        
        //+------------------------------------------------------------------+");
        //| Custom indicator deinitialization function                       |");
        //+------------------------------------------------------------------+");
        protected override int Deinit()
        {
            // ENTER YOUR CODE HERE
            return 0;
        }
        
        //+------------------------------------------------------------------+");
        //| Custom indicator iteration function                              |");
        //+------------------------------------------------------------------+");
        protected override int Start()
        {
            //These variables will come in handy in just a moment
            var pos = Bars;
            double lastHigh = 0.0;
            double lastLow = 0.0;
            int lastHighPos = 0;
            int lastLowPos = 0;
            bool lastWasHigh = false;
            bool lastWasMajor = false;
            
            //Here we are starting from the end
            //of our chart (or array) and working towards the
            //most recent bar
            while (pos >= 0)
            {
                //Fractals help us identify momentum
                //changes in the market, perfect for our pivots!
                //We start by looking at the next upper fractal
                double pvtHigh = iFractals(Symbol, TimeFrame, MODE_UPPER, pos);
                //next we check to make sure it belongs on the chart and 
                //meets our posDiff and pipDiff calculations
                if (pvtHigh != EMPTY_VALUE && (MathAbs(lastLowPos - pos) >= posDiff || (MathAbs(pvtHigh - lastLow) / ((pvtHigh + lastLow) / 2)) * 100 >= pipDiff))
                {
                    //We want to make sure we don't double draw
                    //So we check to see if the last pivot drawn wasn't a high
                    if (!lastWasHigh)
                    {
                        //assign the high point of the bar to our upper buffer
                        if(isMajorPivot(pos, true))
                        {
                            majorPivotHighs[pos] = High[pos];
                            lastWasMajor = true;
                        }
                        else
                        {
                            pivotHighs[pos] = High[pos];
                            lastWasMajor = false;
                        }
                        
                        //remember the last high value
                        lastHigh = High[pos];
                        lastHighPos = pos;
                        //change lastWasHigh to true
                        lastWasHigh = true;
                    }
                    else
                    {
                        //If it was a high, we want to see
                        //Which point is higher if the new point is higher
                        if (pvtHigh > lastHigh)
                        {
                            //if it is then we need to tell our buffer
                            //to remove the lower one and replace it
                            if(lastWasMajor)
                            {
                                majorPivotHighs[lastHighPos] = EMPTY_VALUE;
                            }
                            else
                            {
                                pivotHighs[lastHighPos] = EMPTY_VALUE;
                            }
                            if(isMajorPivot(pos, true))
                            {
                                majorPivotHighs[pos] = High[pos];
                                lastWasMajor = true;
                            }
                            else
                            {
                                pivotHighs[pos] = High[pos];
                                lastWasMajor = false;
                            }
                            lastHigh = High[pos];
                            lastHighPos = pos;
                            lastWasHigh = true;
                        }
                    }
                }
                //Do the same as about but on the lower side of the chart
                double pvtLow = iFractals(Symbol, TimeFrame, MODE_LOWER, pos);
                if (pvtLow != EMPTY_VALUE && (MathAbs(pos - lastHighPos) >= posDiff || (MathAbs(lastHigh - pvtLow) / ((lastHigh + pvtLow) / 2)) * 100 >= pipDiff))
                {
                    if (lastWasHigh)
                    {
                        if(isMajorPivot(pos, false))
                        {
                            majorPivotLows[pos] = Low[pos];
                            lastWasMajor = true;
                        }
                        else
                        {
                            pivotLows[pos] = Low[pos];
                            lastWasMajor = false;
                        }
                        lastLow = Low[pos];
                        lastLowPos = pos;
                        lastWasHigh = false;
                    }
                    else
                    {
                        if (pvtLow < lastLow)
                        {
                            if(lastWasMajor)
                            {
                                majorPivotLows[lastLowPos] = EMPTY_VALUE;
                            }
                            else
                            {
                                pivotLows[lastLowPos] = EMPTY_VALUE;
                            }
                            if(isMajorPivot(pos, false))
                            {
                                majorPivotLows[pos] = Low[pos];
                                lastWasMajor = true;
                            }
                            else
                            {
                                pivotLows[pos] = Low[pos];
                                lastWasMajor = false;
                            }
                            lastLow = Low[pos];
                            lastLowPos = pos;
                            lastWasHigh = false;
                        }
                    }
                }
                
                //We also want to make sure one candle
                //doesn't have a high pivot and a low pivot
                if (lastLowPos == lastHighPos)
                {
                    pivotHighs[lastHighPos] = EMPTY_VALUE;
                    pivotLows[lastLowPos] = EMPTY_VALUE;
                }
                
                pos--;
            }
            
            
            return 0;
        }

        //+------------------------------------------------------------------+
        //| Checks if a pivot is a major pivot,                              |
        //| returns true if it is, false otherwise                           |    
        //+------------------------------------------------------------------+
        private bool isMajorPivot(int pos, bool high)
        {
            if (high)
            {
                //if high pivot, check if previous ten bars have lower highs
                int endBar = pos + 11;
                if (endBar > Bars)
                {
                    return false;
                }
                for (int i = pos; i < endBar; i++)
                {
                    if (High[i] > High[pos])
                    {
                        return false;
                    }
                }
                //Check if next ten bars have lower highs 
                endBar = pos - 10;
                if (endBar < 0)
                {
                    return false;
                }
                for (int i = pos; i > endBar; i--)
                {
                    if (High[i] > High[pos])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                //if low pivot, check if previous ten bars have higher lows
                int endBar = pos + 11;
                if(endBar > Bars)
                {
                    return false;
                }
                for(int i = pos; i < endBar; i++)
                {
                    if(Low[i] < Low[pos])
                    {
                        return false;
                    }
                }
                //Check if next ten bars have lower highs
                endBar = pos - 11;
                if(endBar < 0)
                {
                    return false;
                }
                for(int i = pos; i > endBar; i--)
                {
                    if(Low[i] < Low[pos])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        
        
        //+------------------------------------------------------------------+
        //| AUTO GENERATED CODE. THIS METHODS USED FOR INDICATOR CACHING     |
        //+------------------------------------------------------------------+
        #region Auto Generated Code
        
        [Description("Parameters order Symbol, TimeFrame")]
        public override bool IsSameParameters(params object[] values)
        {
            if(values.Length != 4)
                return false;
            
            if(!CompareString(Symbol, (string)values[0]))
                return false;
            
            if(TimeFrame != (int)values[1])
                return false;

            if (pipDiff != (double)values[2])
                return false;

            if (posDiff != (int)values[3])
                return false;
            
            return true;
        }
        
        [Description("Parameters order Symbol, TimeFrame")]
        public override void SetIndicatorParameters(params object[] values)
        {
            if(values.Length != 2)
                throw new ArgumentException("Invalid parameters number");
            
            Symbol = (string)values[0];
            TimeFrame = (int)values[1];
            pipDiff = (double)values[2];
            posDiff = (int)values[3];
            
        }
        
        #endregion
    }
}

