
using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using System.Collections.Generic;
using Alveo.UserCode;
using Alveo.Common;
using Alveo.Common.Enums;
using Alveo.Common.Classes;

namespace Alveo.UserCode
{
    //The AlignmentStrategy EA was designed to demonstrate how to take
    //a trading strategy and convert it into code.
    public class AlignmentStrategy : ExpertAdvisorBase
    {
        	
        #region Properties
        
        //These variables will be used to calculate the slope of the short, medium, and longer period EMA's
        double shortVal1;
        double shortVal2;
        double medVal1;
        double medVal2;
        double longVal1;
        double longVal2;
        
        //The two variables help to determine where to start and stop the slop calculation of the line. 0 is the newest bar on the chart.
       	[DisplayName("Start Index")]
		[Description("The first index used to calculate the slope of the line.")]       
        public int index1 { get; set; }
       	[DisplayName("End Index")]
		[Description("The last index used to calculate the slope of the line.")] 
        public int index2 { get; set; }
        
        //These variables will us keep track of what's going on
        //More explination will follow on each of these
        string symbol;
        double run;
        double pipPos;
        int currentBars;
        bool tradePlaced;
        int orderID;
        int lastUpdatedBarCount;
        
        //The period for each of the EMA's
        [DisplayName("Period")]
        public int period { get; set; }

        //The minimum slope (in pips) used for the trigger
        [DisplayName("Minimum Slope")]
        public double minSlope { get; set; }

        //The quantity or lot size for the order
        [DisplayName("Lot Size")]
        public double lotSize { get; set; }


        //These three variables allow the user to select three time frames
        //from the TimeFrame Enum.
        [DisplayName("Time Frame 1")]
        public TimeFrame timeFrame1 { get; set; }
        [DisplayName("Time Frame 2")]
        public TimeFrame timeFrame2 { get; set; }
        [DisplayName("Time Frame 3")]
        public TimeFrame timeFrame3 { get; set; }
        
        
        #endregion
        
        public AlignmentStrategy()
        {
            // Set the default values of each of the externalized variables
            period = 60;
            minSlope = 0.3;
            index1 = 0;
            index2 = 1;
            lotSize = 0.01;
            timeFrame1 = TimeFrame.M1;
            timeFrame2 = TimeFrame.M5;
            timeFrame3 = TimeFrame.M15;
            
            copyright = "";
            link = "";
        	
        }
        //+------------------------------------------------------------------+"
        //| expert initialization function                                   |"
        //+------------------------------------------------------------------+"
        protected override int Init()
        {
            //Assign our symbol variable to the symbol of the chart the EA is connected to            
            symbol = Symbol();
            //Our run is the number of bars between each index
            //Because Arrays start at index 0 we have to add one to the variable
            run = (index2 - index1) + 1;
            //pipPos is used to tell the program where the pip position is.
            pipPos = symbol.StartsWith("JPY") || symbol.EndsWith("JPY") ? 100 : 10000;
            //This particular EA is designed to only manage one trade at a time
            //It can be modified to allow more than that though
            //This variable consequently keeps the program up to date on whether it has already placed a trade
            tradePlaced = false;
            //orderID is the order number for the trade that was placed by the EA.
            orderID = 0;
            //current bars is used to check against the total number of bars on the chart
            //This is done to only allow trades to be placed on start up or every new bar
            currentBars = 0;
            //This variable is used to update the stoploss if the pending order get filled
            //to the lowest price of the previous bar.
            lastUpdatedBarCount = Bars;
            
            Core.OutputManager.Print("Alignment Strategy EA For " + symbol + " Now Starting Up.");
            
            return 0;
        }
        
        
        
        //+------------------------------------------------------------------+"
        //| expert deinitialization function                                 |"
        //+------------------------------------------------------------------+"
        protected override int Deinit()
        {
            // ENTER YOUR CODE HERE;
            Core.OutputManager.Print("Alignment Strategy EA For " + symbol + " Now Shutting Down.");
            return 0;
        }
        
        //+------------------------------------------------------------------+"
        //| expert start function                                            |"
        //+------------------------------------------------------------------+"
        protected override int Start()
        {
        	//Check if a trade has been placed, if so we can try and modify it
            if (tradePlaced)
            {
                //Loop through all of the orders and check if the orderID variable
                //matches any of the current orders
            	bool stillOpen = false;
            	for (int i = 0; i < OrdersTotal(); i++)
            	{
            		bool selected = OrderSelect(i, SELECT_BY_POS);
            		if (selected && orderID == OrderTicket())
            		{
            			stillOpen = true;
            			break;
            		}
            	}
                //If the order is still open...
            	if (stillOpen)
            	{
                    //Check to see if the order is still pending.
                    //If so, we must check to see if the pending order's stop loss has been hit  ***Alveo Doesn't Check StopLoss On Pending Orders*** 
                    //If the stoploss was hit, then delete the order or tell the program there are no trades currently open
            		if (OrdersStatus() == (int)OrderStatus.PendingNew)
            		{
            			int orderType = OrderType();
            			if (orderType == (int)TradeOperations.OP_BUYSTOP)
            			{
            				if (OrderClosePrice() <= OrderStopLoss())
            				{
            					OrderDelete(orderID);
            					tradePlaced = false;
            				}
            			}
            			else if (orderType == (int)TradeOperations.OP_SELLSTOP)
            			{
            				if (OrderClosePrice() >= OrderStopLoss())
            				{
            					OrderDelete(orderID);
            					tradePlaced = false;
            				}
            			}
            		} //If the order is filled and a new bar has appeared, move the stoploss up to the previous bars' low
            		else if (lastUpdatedBarCount < Bars && OrdersStatus() == (int)OrderStatus.Filled)
            		{
            			int orderType = OrderType();
	            		if (orderType == (int)TradeOperations.OP_BUY)
	            		{
	            			OrderModify(orderID, OrderOpenPrice(), Low[1], 0, null);
	            			lastUpdatedBarCount = Bars;
	            		}
	            		else if (orderType == (int)TradeOperations.OP_SELL)
	            		{
	            			OrderModify(orderID, OrderOpenPrice(), High[1], 0, null);
	            			lastUpdatedBarCount = Bars;
	            		}
            		}
            	}//If the order isn't open (this get's called when the stoploss was hit) then set tradePlaced to false
            	else
            	{
            		tradePlaced = false;
            	}
            }
            //If no new bar has appeared, return because we only want to check at every new bar
            if (currentBars == Bars) return 0;
            //If there is a new bar, set that number of bars to currentBars
            currentBars = Bars;
            
            //The iMA method is used to select a value out of an indicator using moving averages at a specified index
            shortVal1 = iMA(symbol, (int)timeFrame1, period, 0, (int)MovingAverageType.MODE_EMA, PRICE_CLOSE, index1);
            shortVal2 = iMA(symbol, (int)timeFrame1, period, 0, (int)MovingAverageType.MODE_EMA, PRICE_CLOSE, index2);
            
            medVal1 = iMA(symbol, (int)timeFrame2, period, 0, (int)MovingAverageType.MODE_EMA, PRICE_CLOSE, index1);
            medVal2 = iMA(symbol, (int)timeFrame2, period, 0, (int)MovingAverageType.MODE_EMA, PRICE_CLOSE, index2);
            
            longVal1 = iMA(symbol, (int)timeFrame3, period, 0, (int)MovingAverageType.MODE_EMA, PRICE_CLOSE, index1);
            longVal2 = iMA(symbol, (int)timeFrame3, period, 0, (int)MovingAverageType.MODE_EMA, PRICE_CLOSE, index2);
            
            //----------------For BUY STOP-------------------// Check to see if each EMA produces a slope greater than the minimum
            if (!tradePlaced && calcSlope(shortVal1, shortVal2) > minSlope && calcSlope(medVal1, medVal2) > minSlope && calcSlope(longVal1, longVal2) > minSlope)
            {
            	Core.OutputManager.Print("Trying to Place Buy Trade Now For " + symbol);
            	//Place Buy Stop Trade
            	//First Find last pivot high for the price of the BUYSTOP
            	double price = EMPTY_VALUE;
            	int i = 0;
            	while (price == EMPTY_VALUE && i < Bars)
            	{
            		price = iPivots(symbol, (int)timeFrame1, 0.08, 3, MODE_UPPER, i);
            		i++;
            	}
            	if (price == EMPTY_VALUE) return 0; // Return if we get an invalid price
                //Find the last pivot low for the stoploss
            	double stopLoss = EMPTY_VALUE;
            	i = 0;
            	while (stopLoss == EMPTY_VALUE && i < Bars)
            	{
            		stopLoss = iPivots(symbol, (int)timeFrame1, 0.08, 3, MODE_LOWER, i);
            		i++;
            	}
            	if (stopLoss == EMPTY_VALUE) return 0; // Return if the the price is invalid
            	if (price < MarketInfo(symbol, (int)Marketinfo.MODE_ASK)) return 0; //If the pivot price is less than the current market price, return because the order would be filled imediately
            	//Set our variables to let the program know a trade has been set
                tradePlaced = true;
            	lastUpdatedBarCount = Bars;
                //Send the order as a BUYSTOP
            	OrderSend(symbol, (int)TradeOperations.OP_BUYSTOP, lotSize, price, 0, stopLoss, 0, "Order Opened By Alignment Strategy EA");
                //Find the order number and set it to our orderID variable for future reference
            	OrderSelect(OrdersTotal() - 1, SELECT_BY_POS);
            	orderID = OrderTicket();
            } //---------------------------For SELL STOP-------------------------// Essentially the same as with the BUYSTOP just reverses the values
            else if (!tradePlaced && calcSlope(shortVal2, shortVal1) > minSlope && calcSlope(medVal2, medVal1) > minSlope && calcSlope(longVal2, longVal1) > minSlope)
            {
            	Core.OutputManager.Print("Trying to Place Sell Trade Now For " + symbol);
            	//Place Buy Stop Trade
            	//First Find last pivot high
            	double price = EMPTY_VALUE;
            	int i = 0;
            	while (price == EMPTY_VALUE && i < Bars)
            	{
            		price = iPivots(symbol, (int)timeFrame1, 0.08, 3, MODE_LOWER, i);
            		i++;
            	}
            	if (price == EMPTY_VALUE) return 0;
            	double stopLoss = EMPTY_VALUE;
            	i = 0;
            	while (stopLoss == EMPTY_VALUE && i < Bars)
            	{
            		stopLoss = iPivots(symbol, (int)timeFrame1, 0.08, 3, MODE_UPPER, i);
            		i++;
            	}
            	if (stopLoss == EMPTY_VALUE) return 0;
            	if (price > MarketInfo(symbol, (int)Marketinfo.MODE_BID)) return 0;
            	tradePlaced = true;
            	lastUpdatedBarCount = Bars;
            	OrderSend(symbol, (int)TradeOperations.OP_SELLSTOP, lotSize, price, 0, stopLoss, 0, "Order Opened By Alignment Strategy EA");
            	OrderSelect(OrdersTotal() - 1, SELECT_BY_POS);
            	orderID = OrderTicket();
            }
        	
            return 0;
        }
        //This method calculated our slope of the EMA prices and converts it to pips.
        private double calcSlope(double a, double b)
        {
        	return ((a - b) / run) * pipPos;
        }
    }
}