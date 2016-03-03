
using System;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Media;
using Alveo.Interfaces.UserCode;
using Alveo.UserCode;
using Alveo.Common;
using Alveo.Common.Classes;

namespace Alveo.UserCode
{
    [Serializable]
    public class OCO : ScriptBase
    {

        #region Properties

        [Category("Settings")]
        public double BuyOrderPrice{ get; set; }

        [Category("Settings")]
        public double BuyLots{ get; set; }

        [Category("Settings")]
        public int BuyStopLoss{ get; set; }

        [Category("Settings")]
        public int BuyTakeProfit{ get; set; }

        [Category("Settings")]
        public int BuyOrderType{ get; set; }

        [Category("Settings")]
        public double SellOrderPrice{ get; set; }

        [Category("Settings")]
        public double SellLots{ get; set; }

        [Category("Settings")]
        public int SellStopLoss{ get; set; }

        [Category("Settings")]
        public int SellTakeProfit{ get; set; }

        [Category("Settings")]
        public int SellOrderType{ get; set; }

        [Category("Settings")]
        public int Slippage{ get; set; }

        #endregion

        //+------------------------------------------------------------------+
        //| Script default constructor. DO NOT REMOVE                        |
        //+------------------------------------------------------------------+
        public OCO()
        {

            copyright = "JF";
            link = "";
            BuyOrderPrice = 0.0;
            BuyLots = 0.1;
            BuyStopLoss = 200;
            BuyTakeProfit = 200;
            BuyOrderType = OP_BUYSTOP;
            SellOrderPrice = 0.0;
            SellLots = 0.1;
            SellStopLoss = 200;
            SellTakeProfit = 200;
            SellOrderType = OP_SELLSTOP;
            Slippage = 1;

        }

private const double TIMEOUT = 3
;

        double bsl,ssl,btp,stp;
        int bt,st;
        bool cont = false;
        bool flip = true;
        int  err1,err;
        protected override int Start()
        {

            if(BuyStopLoss > 0)
            bsl = BuyOrderPrice - BuyStopLoss*Point;
            else
            bsl = 0.0;
            if(SellStopLoss > 0)
            ssl = SellOrderPrice + SellStopLoss*Point;
            else
            ssl = 0.0;
            if(BuyTakeProfit > 0)
            btp = BuyOrderPrice + BuyTakeProfit*Point;
            else
            btp = 0.0;
            if(SellTakeProfit > 0)
            stp = SellOrderPrice - SellTakeProfit*Point;
            else
            stp = 0.0;

            bt = OrderSend(Symbol(), BuyOrderType, BuyLots, BuyOrderPrice, Slippage, bsl, btp, "Buy OCO", 0, null, 0);
            if(CheckError())
            return(-1);
            st = OrderSend(Symbol(), SellOrderType, SellLots, SellOrderPrice, Slippage, ssl, stp, "Sell OCO", 0, null, 0);
            if(CheckError())
            {
                RemoveOrder(bt);
                return(-1);
            }

            cont = IsTradeAllowed();
            err1 = 0;
            while(cont)
            {
                if(flip)
                ShowLabel();
                else
                HideLabel();
                flip = !flip;
                err = 0;

                if(OrderSelect(bt, SELECT_BY_TICKET, MODE_TRADES))
                {
                    if(OrderType() <= OP_SELL)
                    {
                        RemoveOrder(st);
                        cont = false;
                    }
                }
                else
                err++;

                if(OrderSelect(st, SELECT_BY_TICKET, MODE_TRADES))
                {
                    if(OrderType() <= OP_SELL)
                    {
                        RemoveOrder(bt);
                        cont = false;
                    }
                }
                else
                err++;
                if(err > 0)
                err1++;
                if(err1 > TIMEOUT)
                cont = false;
                Sleep(500);
                cont = cont & IsTradeAllowed();
            }

            HideLabel();

            return(0);
        }

        void ShowLabel()
        {
           WindowRedraw();
        }

        void HideLabel()
        {
            WindowRedraw();
        }

        bool CheckError()
        {
            int err = GetLastError();
            if(err == ERR_NO_ERROR) return(false);
            return(true);
        }

        void RemoveOrder(int ticket)
        {
            if(OrderSelect(ticket, SELECT_BY_TICKET, MODE_TRADES))
            if(OrderType() <= OP_SELL)
            OrderClose(ticket, OrderLots(), OrderClosePrice(), Slippage);
            else
            OrderDelete(ticket);
            CheckError();
        }
    }
}
