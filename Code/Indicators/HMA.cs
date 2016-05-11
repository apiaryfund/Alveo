using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using Alveo.Common.Classes;
using Alveo.UserCode;
using Alveo.Common;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("")]
    public class HMA : IndicatorBase
    {
        #region Properties
        
        Array<double> UpTrend;
        Array<double> DownTrend;
        Array<double> ExtMapBuffer;
        
        int price = 0;
        
        #endregion
        
        
        public HMA()
        {
            // Basic indicator initialization. Don't use this constructor to calculate values
            
            indicator_buffers = 2;
            indicator_chart_window = true;
        	period = 21;
        	method = 3;

            indicator_width1 = 2;
            indicator_width2 = 2;
            indicator_color1 = Colors.Blue;
            indicator_color2 = Colors.Red;
            
            UpTrend = new Array<double>();
            DownTrend = new Array<double>();
            ExtMapBuffer = new Array<double>();
            
            copyright = "";
            link = "";
        }
        
        
        [Category("Settings")]
        [DisplayName("MA Period")]
        public int period { get; set; }
        
        [Category("Settings")]
        [DisplayName("Method")]
        public int method { get; set; }
        
        //+------------------------------------------------------------------+");
        //| Custom indicator initialization function                         |");
        //+------------------------------------------------------------------+");
        protected override int Init()
        {
        	// ENTER YOUR CODE HERE
        	IndicatorBuffers(2);
        	SetIndexBuffer(0, UpTrend);
        	SetIndexArrow(0, 159);
        	SetIndexBuffer(1, DownTrend);
        	SetIndexArrow(1, 159);

			SetIndexStyle(0, DRAW_LINE, STYLE_SOLID);
            SetIndexLabel(0, "HMA(" + period + ").Bull");
            SetIndexStyle(1, DRAW_LINE, STYLE_SOLID);
            SetIndexLabel(1, "HMA(" + period + ").Bear");

			IndicatorShortName("Hull Moving Average("+period+")"); 
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
        
        protected double WMA(int x, int p) 
		{ 
			return(iMA(null, 0, p, 0, method, price, x)); 
		} 
        
        //+------------------------------------------------------------------+");
        //| Custom indicator iteration function                              |");
        //+------------------------------------------------------------------+");
        protected override int Start()
        {
			int counted_bars = IndicatorCounted(); 

			if(counted_bars < 0) 
				return(-1); 

			int x = 0; 
			double p = MathSqrt(period); 
			int e = Bars - counted_bars + period + 1; 

			Array<double> vect = new Array<double>();
			Array<double> trend = new Array<double>();

			if(e > Bars) 
				e = Bars; 
			
			ArrayResize(vect, e); 
			ArraySetAsSeries(vect, true);
			ArrayResize(trend, e); 
			ArraySetAsSeries(trend, true); 
			ArrayResize(ExtMapBuffer, e);
			ArraySetAsSeries(ExtMapBuffer, true);
			for(x = e; x >= 0; x--) 
			{ 
				vect[x] = 2*WMA(x, period/2) - WMA(x, period); 
			}

			for(x = 0; x < e-period; x++)
			{
				ExtMapBuffer[x] = iMAOnArray(vect, 0, (int)p, 0, method, x); 
			}
			for(x = e-period; x >= 0; x--)
			{ 
				trend[x] = trend[x+1];
				if (ExtMapBuffer[x]> ExtMapBuffer[x+1]) trend[x] =1;
				if (ExtMapBuffer[x]< ExtMapBuffer[x+1]) trend[x] =-1;

				if (trend[x]>0)
				{ 
					UpTrend[x] = ExtMapBuffer[x];
					if (trend[x+1]<0) 
					{
						UpTrend[x+1]=ExtMapBuffer[x+1];
					}
					
					DownTrend[x] = EMPTY_VALUE;

				}
				else 
				{
					if (trend[x]<0)
					{ 
						DownTrend[x] = ExtMapBuffer[x]; 
						if (trend[x+1]>0)
						{
							DownTrend[x+1]=ExtMapBuffer[x+1];
						}

						UpTrend[x] = EMPTY_VALUE;

					} 
				}
			}
        	return 0;
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

            if (period != (int)values[2])
                return false;

            if (method != (int)values[3])
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
            period = (int)values[2];
            method = (int)values[3];
            
        }
        
        #endregion
    }
}

