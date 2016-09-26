
using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections.Generic;
using Alveo.Interfaces.UserCode;
using Alveo.UserCode;
using Alveo.Common;
using Alveo.Common.Classes;
using Alveo.Common.Classes.Notifications;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("A simple EA that sends a notification when a pip target has been hit.")]
    public class PipTargetNotification : ExpertAdvisorBase
    {
        	
        #region Properties
        
        //Declare win/loss targets as public properties in order to be able to change the tagets when EA is started.
        public double PipWinTarget { get; set; }
        public double PipLossTarget { get; set; }
        
        #endregion
        
        //Declare a private list of ticket #s that have already triggered a notification. This prevents more than one notification to be sent per Order.
        private List<int> _notifiedID = new List<int>();
        
        public PipTargetNotification()
        {
            copyright = "Apiary Fund";
            link = "https://apiaryfund.com";
        	
            //Set the default pip target values
            PipWinTarget = 10;
            PipLossTarget = -5;
        }
        
        protected override int Start()
        {
        	//Loop through all the orders by selecting them sequentially
            int i = 0;
            while (OrderSelect(i, 0))
        	{
            	//Ensure the order has previously been filled
            	if (OrderFillTime() != null)
        		{
            		int ticket = OrderTicket();
            		//Ensure a notification has not yet been sent of the selected order
            		if (!_notifiedID.Contains(ticket))
        			{
	            		//Calculate pips on the selected order
	            		var pips = GetPips(OrderSymbol(), OrderOpenPrice(), OrderType());
	            		//Evaluate if the pips have reached either target. If so, send a notification with a button to quickly close the order
            			if (pips >= PipWinTarget)
        				{
	        				Notify(string.Format("Order #{0} has made {1:0.00} pips!", ticket, pips), Severity.Normal, new Button("Close Order", () => { OrderClose(ticket, 0, 0, 0); }), ActionOnViewed.Clear);
	        				_notifiedID.Add(ticket);
        				}
            			else if (pips <= PipLossTarget)
        				{
            				Notify(string.Format("Order #{0} has lost {1:0.00} pips!", ticket, pips), Severity.Moderate, new Button("Close Order", () => { OrderClose(ticket, 0, 0, 0); }), ActionOnViewed.Clear);
            				_notifiedID.Add(ticket);
        				}
        			}
        		}
            	i++;
        	}
            
            return 0;
        }
        
        private double GetPips(string symbol, double price, int side)
    	{
        	var currentPrice = side == 0 ? MarketInfo(symbol, MODE_BID) : MarketInfo(symbol, MODE_ASK);
        	
        	return (side == 0 ? currentPrice - price : price - currentPrice) * (Chart.Symbol.Contains("JPY") ? 100 : 10000);
    	}
    }
}