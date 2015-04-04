
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
    [Description("For 5 Digit Symbols Only - Alerts when Bid is within 1 pips of BBands 12,2 / while opposite to macd 8,17,9  momentum - used to trade pullbacks within Alignment")]
    public class BBBreach : ExpertAdvisorBase
    {
        	
        #region Properties
        #endregion
        
        public BBBreach()
        {
            
            copyright = "JF";
            link = "";
        	
        }
        protected override int Init()
        {
            return 0;
        }
        
        protected override int Deinit()
        {
            return 0;
        }
           double distancefromband=0.0001;

           bool notice=true;
        protected override int Start()
        {
            if(Volume[0]==1)
            {
                notice=true;
            }
            double MACD;
            MACD=iMACD(null,0,8,17,9,PRICE_CLOSE,MODE_SIGNAL,0);

            double LBand;
            LBand=iBands(null,0,12,2,0,PRICE_CLOSE,2,0);
            double UBand;
            UBand=iBands(null,0,12,2,0,PRICE_CLOSE,1,0);

            if(MACD>0 & (Bid-distancefromband)<= LBand & notice==true)
            {
                Alert("Buy "+Symbol()+" "+Period()," Lower Band hit");
                Print(("Buy "+Symbol()+" "+Period()+" Lower band hit"));
               // PlaySound("C:\\Users\\Fibus\\Documents\\Alveo\\Sounds\\go.wav"); "
                notice=true;
            }

            if(MACD<0 & (Bid+distancefromband)>= UBand & notice==true)
            {
                Alert("Sell "+Symbol()+" "+Period()," Upper band hit");
                Print(("Sell "+Symbol()+" "+Period()+" Upper band hit"));
            //    PlaySound("C:\\Users\\Fibus\\Documents\\Alveo\\Sounds\\go.wav"); "
                notice=true;
            }
            return 0;
        }
    }
}
