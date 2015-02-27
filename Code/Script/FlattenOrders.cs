
using System;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using Alveo.UserCode;

namespace Alveo.UserCode
{
    [Serializable]
    [Description("Flattens all current positions and returns total profit")]
    public class FlattenOrders : ScriptBase
    {
        	
        #region Properties
        #endregion
        
        public FlattenOrders	()
        {
            	
            // Basic indicator initialization. Don't use this constructor to calculate values
            copyright = "Apiary Investment Fund, LLC";
            link = "";
        	
        }
        
        //+------------------------------------------------------------------+"
        //| script program start function                                    |"
        //+------------------------------------------------------------------+"
        //The purpose of this script is to 
        //close all the current open positions
        //on the current account and display in
        //an alert the total number of orders
        //closed and their combined profit.
        //This is where your script starts.......
        protected override int Start()
        {
        	//Declare a variable ordersProfit to keep tract of
        	//total profit of all closed orders.
        	double ordersProfit = 0;
        	//Declare a variable to record the total number
        	//of orders to report later. Calling OrdersTotal()
        	//method returns this number
        	int orderTotal = OrdersTotal();
        	//To select an order, OrderSelect() must be called
        	//This method requires either the orders position #
        	//or it's ticket # as the first parameter. Then as the
        	//second parameter use SELECT_BY_POS or SELECT_BY_TICKET
        	//to tell the system how to select the order.
        	//The last parameter requires MODE_TRADES (Defualt) or
        	//MODE_HISTORY to tell the system to either find your order
        	//in the order history or in your current trade pool.
        	//This method will return a bool value to indicate if the
        	//action was successful or not.
        	bool selectOrder = OrderSelect(0, SELECT_BY_POS, MODE_TRADES);
        	//If selectOrder is true, that means an order has
        	//been selected for use. So in order to make sure 
        	//we take care of all the orders, run the following 
        	//for as long as an order can still be selected
        	while (selectOrder)
        	{
        		//Grab the ticket number by calling OrderTicket()
        		//This method returns the ticket number as an int
        		int ticketNum = OrderTicket();
        		//To check if the closing of the order was
        		//successful, we'll throw it in an if statement.
        		//As of right now OrderClose only supports it's first
        		//parameter (ticket number). The other three
        		//parameters are for lot size, price, and slippage.
        		//There is also an optional color parameter, but that
        		//also has yet to be implemented. In the future,
        		//we hope to allow the user to use these parameters 
        		//for more specific closing of an order
        		if (OrderClose(ticketNum, 0, 0, 0))
        		{
        			//If the order closed successfully,
        			//add it's profit to the total profit
        			ordersProfit += OrderProfit();
        		}
        		//Try to select another order like above,
        		//If it returns true, the loops will run again
        		//for the newly selected order, otherwise it 
        		//will break out of the loop
        		selectOrder = OrderSelect(0, SELECT_BY_POS, MODE_TRADES);
        	}
        	//This method displays an alert box to the user. The alert as
        	//implemented below will display the total number of orders
        	//closed and their combined profit.
        	Alert("Your total profit on " + orderTotal + " orders is " + ordersProfit);
        	        	
            return 0;
        }
    }
}