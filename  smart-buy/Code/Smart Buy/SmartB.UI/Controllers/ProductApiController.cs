﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using SmartB.UI.Areas.Admin.Models;
using SmartB.UI.Infrastructure;
using SmartB.UI.Models;
using SmartB.UI.Models.EntityFramework;
using SmartB.UI.Models.MobileModel;

namespace SmartB.UI.Controllers
{
    public class ProductApiController : ApiController
    {
        private SmartBuyEntities context = new SmartBuyEntities();

        //search product
        [AcceptVerbs("GET")]
        public List<ProductMobileModel> Search(string keyword)
        {

            //search product by dictionary
            var dictionaries = context.Dictionaries.Include(i => i.Product).Where(p => p.Name.Contains(keyword)).OrderBy(p => p.Name).ToList();

            var result = new List<ProductMobileModel>();
            foreach (Dictionary dictionary in dictionaries)
            {
                List<int?> minPrice = dictionary.Product.ProductAttributes
                    .OrderByDescending(x => x.LastUpdatedTime)
                    .Select(x => x.MinPrice)
                    .ToList();
                List<int?> maxPrice = dictionary.Product.ProductAttributes
                    .OrderByDescending(x => x.LastUpdatedTime)
                    .Select(x => x.MaxPrice)
                    .ToList();
                var productName = context.Products.Where(p => p.Id == dictionary.ProductId).Select(p => p.Name).FirstOrDefault();
                if (!result.Any(p => p.Name == productName))
                {
                    var info = new ProductMobileModel
                    {
                        ProductId = dictionary.ProductId.ToString(),
                        Name = productName,
                        MinPrice = minPrice[0].ToString(),
                        MaxPrice = maxPrice[0].ToString()
                    };
                    result.Add(info);
                }

            }
            // result.OrderBy(p => p.Name);

            return result;
        }

        [AcceptVerbs("GET")]
        public List<ProductMobileModel> GetProduct()
        {
            var products = context.Products.Include(i => i.ProductAttributes).ToList();
            var result = new List<ProductMobileModel>();
            foreach (Product product in products)
            {
                List<int?> minPrice = product.ProductAttributes
                    .OrderByDescending(x => x.LastUpdatedTime)
                    .Select(x => x.MinPrice)
                    .ToList();
                List<int?> maxPrice = product.ProductAttributes
                    .OrderByDescending(x => x.LastUpdatedTime)
                    .Select(x => x.MaxPrice)
                    .ToList();

                DateTime? lastUpdateTime = product.ProductAttributes
                    .OrderByDescending(x => x.LastUpdatedTime)
                    .Select(x => x.LastUpdatedTime).FirstOrDefault();
                string lastUpdateTimeFormat = String.Format("{0:MM-dd-yyyy}", lastUpdateTime);
                var proAtt = context.ProductAttributes.Where(p => p.ProductId == product.Id).OrderByDescending(x => x.LastUpdatedTime).FirstOrDefault();
                if (proAtt != null)
                {
                    var info = new ProductMobileModel
                    {
                        ProductId = product.Id.ToString(),
                        Name = product.Name,
                        MinPrice = minPrice[0].ToString(),
                        MaxPrice = maxPrice[0].ToString(),
                        LastUpdateTime = lastUpdateTimeFormat
                    };
                    result.Add(info);
                }
            }
            return result;
        }

        //get all market
        [HttpGet]
        public List<MarketMobileModel> GetMarket()
        {
            var result = new List<MarketMobileModel>();
            var markets = context.Markets.ToList();
            var defaulMarket = new MarketMobileModel
            {
                Id = markets[0].Id.ToString(),
                Name = markets[0].Name,
                Address = markets[0].Address,
                Latitude = markets[0].Latitude.GetValueOrDefault(),
                Longitude = markets[0].Longitude.GetValueOrDefault()
            };
            result.Add(defaulMarket);

            markets.RemoveAt(0);
            var marketsOrder = markets.OrderBy(m => m.Name).ToList();
            for (int i = 0; i < marketsOrder.Count(); i++)
            {

                var item = new MarketMobileModel
                {
                    Id = marketsOrder[i].Id.ToString(),
                    Name = marketsOrder[i].Name,
                    Address = marketsOrder[i].Address,
                    Latitude = marketsOrder[i].Latitude.Value,
                    Longitude = marketsOrder[i].Longitude.Value
                };
                result.Add(item);
            }

            //foreach (Market market in markets)
            //{
            //    var item = new MarketMobileModel
            //    {
            //        Id = market.Id.ToString(),
            //        Name = market.Name,
            //        Address = market.Address,
            //        Latitude = market.Latitude,
            //        Longitude = market.Longitude
            //    };
            //    result.Add(item);
            //}

            return result;
        }

        //get history by username
        [HttpGet]
        public List<HistoryMobileModel> GetHistory(string username)
        {
            var result = new List<HistoryMobileModel>();
            DateTime minusThirty = DateTime.Today.AddDays(-30);
            DateTime minusZero = DateTime.Today.AddDays(0);
            try
            {
                var histories = from item in context.Histories
                                where item.BuyTime >= minusThirty && item.BuyTime <= minusZero && item.Username.Equals(username)
                                select item;
                histories = histories.OrderByDescending(item => item.BuyTime);

                foreach (History history in histories)
                {
                    string buyTimeFormat = String.Format("{0:dd-MM-yyyy}", history.BuyTime);
                    var item = new HistoryMobileModel
                    {
                        Id = history.Id.ToString(),
                        Username = username,
                        BuyTime = buyTimeFormat
                    };
                    result.Add(item);
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        //get history detail
        [HttpGet]
        public List<HistoryDetailMobileModel> GetHistoryDetail(int id)
        {
            var result = new List<HistoryDetailMobileModel>();
            try
            {
                var historyDetails = context.HistoryDetails.Where(h => h.HistoryId == id);
                var modelProduct = from p in context.ProductAttributes
                                   group p by p.ProductId into grp
                                   select grp.OrderByDescending(o => o.LastUpdatedTime).FirstOrDefault();

                if (historyDetails == null)
                {
                    return null;
                }

                foreach (HistoryDetail historyDetail in historyDetails)
                {
                    var item = new HistoryDetailMobileModel
                    {
                        Id = historyDetail.Id.ToString(),
                        HistoryId = historyDetail.HistoryId.ToString(),
                        ProductName = historyDetail.Product.Name,
                        MinPrice = historyDetail.MinPrice.ToString(),
                        MaxPrice = historyDetail.MaxPrice.ToString(),
                    };
                    foreach (ProductAttribute p in modelProduct)
                    {
                        if (p.ProductId == historyDetail.ProductId)
                        {
                            item.MinPriceToday = p.MinPrice.ToString();
                            item.MaxPriceToday = p.MaxPrice.ToString();
                        }
                    }
                    result.Add(item);
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        //get suggest product
        [AcceptVerbs("GET")]
        public List<ProductMobileModel> GetSuggestProduct(string username)
        {
            try
            {
                var productModel = (from p in context.Products
                                    from h in context.HistoryDetails
                                    where h.ProductId == p.Id && h.History.Username.Equals(username)
                                    group h by p into productMostBuyGroup
                                    select new ProductMostBuy
                                    {
                                        Product = productMostBuyGroup.Key,
                                        numberOfBuy = productMostBuyGroup.Count()
                                    }).Where(x => x.numberOfBuy >= 5).OrderByDescending(o => o.numberOfBuy).Take(5);

                var result = new List<ProductMobileModel>();
                foreach (ProductMostBuy pmb in productModel)
                {
                    var product = (from p in context.ProductAttributes
                                   where p.ProductId == pmb.Product.Id
                                   group p by p.ProductId into grp
                                   select grp.OrderByDescending(o => o.LastUpdatedTime).FirstOrDefault()).FirstOrDefault();
                    DateTime? lastUpdateTime = product.LastUpdatedTime;
                    string lastUpdateTimeFormat = String.Format("{0:MM-dd-yyyy}", lastUpdateTime);
                    var info = new ProductMobileModel
                    {
                        ProductId = product.ProductId.ToString(),
                        Name = product.Product.Name,
                        MinPrice = product.MinPrice.ToString(),
                        MaxPrice = product.MaxPrice.ToString(),
                        LastUpdateTime = lastUpdateTimeFormat
                    };
                    result.Add(info);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            context.Dispose();
        }

        //Save user price
        [HttpPost]
        public bool SaveUserPrice(List<UserPrice> parseJson)
        {
            var check = false;

            // define epsilon
            var ep = 0.1;            
            try
            {
                for (int i = 0; i < parseJson.Count; i++)
                {
                    var pId = parseJson[i].ProductId;
                    var updatedPrice = parseJson[i].UpdatedPrice;

                    var minPrice = context.ProductAttributes.Where(p => p.ProductId == pId)
                        .OrderByDescending(x => x.LastUpdatedTime)
                        .Select(x => x.MinPrice)
                        .FirstOrDefault();


                    var maxPrice = context.ProductAttributes.Where(p => p.ProductId == pId)
                        .OrderByDescending(x => x.LastUpdatedTime)
                        .Select(x => x.MaxPrice)
                        .FirstOrDefault();

                    var averagePrice = (minPrice + maxPrice) / 2;
                    var rangeFrom = minPrice - ep * averagePrice;
                    var rangeTo = maxPrice + ep * averagePrice;

                    if ((updatedPrice >= rangeFrom && updatedPrice <= minPrice) || (updatedPrice >= maxPrice && updatedPrice <= rangeTo))
                    {
                        var userPrice = new UserPrice();
                        userPrice.Username = parseJson[i].Username;
                        userPrice.MarketId = parseJson[i].MarketId;
                        userPrice.ProductId = pId;
                        userPrice.UpdatedPrice = updatedPrice;
                        userPrice.LastUpdatedTime = DateTime.Now;
                        userPrice.isApprove = false;
                        context.UserPrices.Add(userPrice);
                        context.SaveChanges();
                    }                    
                }
                check = true;
                return check;
            }
            catch
            {
                return check;
            }
        }

        //Save cart
        [HttpPost]
        public bool SaveCart(List<CartHistory> dataList)
        {
            var check = false;
            //JavaScriptSerializer ser = new JavaScriptSerializer();
            //List<CartHistory> dataList = ser.Deserialize<List<CartHistory>>(totaldata);
            try
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    var username = dataList[i].Username;
                    var pid = dataList[i].ProductId;
                    var now = DateTime.Now.Date;
                    var dupHistory = context.Histories.Where(his => his.Username == username &&
                        his.BuyTime == now).FirstOrDefault();



                    if (dupHistory == null)
                    {
                        var newHitory = new History();
                        newHitory.Username = username;
                        newHitory.BuyTime = now;

                        context.Histories.Add(newHitory);
                        var newHistoryDetail = new HistoryDetail();
                        newHistoryDetail.History = newHitory;
                        newHistoryDetail.ProductId = pid;
                        newHistoryDetail.MinPrice = dataList[i].MinPrice;
                        newHistoryDetail.MaxPrice = dataList[i].MaxPrice;
                        context.HistoryDetails.Add(newHistoryDetail);

                    }
                    else
                    {
                        var historyId = (from h in context.Histories
                                         where h.BuyTime == now && h.Username == username
                                         select h.Id).First();

                        var dupProductId = context.HistoryDetails.Where(p => p.ProductId == pid && p.HistoryId == historyId).FirstOrDefault();
                        if (dupProductId == null)
                        {
                            var newHistoryDetail = new HistoryDetail();
                            newHistoryDetail.History = dupHistory;
                            newHistoryDetail.ProductId = pid;
                            newHistoryDetail.MinPrice = dataList[i].MinPrice;
                            newHistoryDetail.MaxPrice = dataList[i].MaxPrice;
                            context.HistoryDetails.Add(newHistoryDetail);

                        }
                    }
                    context.SaveChanges();

                }

                check = true;
                return check;
            }
            catch
            {
                return check;
            }
        }

        [HttpGet]
        public Profile getProfile(string username)
        {
            //var result = new Profile();
            var profile = context.Profiles.Where(p => p.Username.Equals(username)).FirstOrDefault();

            var result = new Profile
            {
                Username = username,
                FirstRoute = profile.FirstRoute,
                FirstStartAddress = profile.FirstStartAddress,
                FirstEndAddress = profile.FirstEndAddress,
                FirstRouteName = profile.FirstRouteName,
                SecondRoute = profile.SecondRoute,
                SecondRouteName = profile.SecondRouteName,
                SecondStartAddress = profile.SecondStartAddress,
                SecondEndAddress = profile.SecondEndAddress,
                ThirdRoute = profile.ThirdRoute,
                ThirdRouteName = profile.ThirdRouteName,
                ThirdStartAddress = profile.ThirdStartAddress,
                ThirdEndAddress = profile.ThirdEndAddress,
            };

            return result;
        }
        private readonly string _xmlPath = ConstantManager.ConfigPath;
        [HttpGet]
        public TimeSyncMobileModel getTimeServer()
        {
            bool CheckTimeSync = false;
            DateTime today = DateTime.Now;
            string todayFormat = String.Format("{0:MM-dd-yyyy}", today);
            
            string result = "";
            XDocument doc = XDocument.Load(_xmlPath);
            var hour = doc.Root.Element("parseTime").Element("hour");
            if (hour != null)
            {
                int h = Convert.ToInt32(hour.Value) + 1;
                result += h + ":";
            }
            var minute = doc.Root.Element("parseTime").Element("minute");
            if (minute != null)
            {
                result += minute.Value;
            }
            DateTime timeSync = Convert.ToDateTime(result);

            if (today >= timeSync)
            {
                CheckTimeSync = true;
            } else
            CheckTimeSync = false;

            TimeSyncMobileModel model = new TimeSyncMobileModel
            {
                todayFormat = todayFormat,
                CheckTimeSync = CheckTimeSync
            };
            return model;
        }

        [HttpGet]
        public List<HistoryDetailMobileModel> GetHistoryDetail(string username)
        {
            var result = new List<HistoryDetailMobileModel>();
            try
            {
                var historyDetails = context.HistoryDetails.Where(h => h.History.Username == username).ToList();
                var modelProduct = from p in context.ProductAttributes
                                   group p by p.ProductId into grp
                                   select grp.OrderByDescending(o => o.LastUpdatedTime).FirstOrDefault();

                if (historyDetails == null)
                {
                    return null;
                }

                foreach (HistoryDetail historyDetail in historyDetails)
                {
                    var item = new HistoryDetailMobileModel
                    {
                        Id = historyDetail.Id.ToString(),
                        HistoryId = historyDetail.HistoryId.ToString(),
                        ProductName = historyDetail.Product.Name,
                        MinPrice = historyDetail.MinPrice.ToString(),
                        MaxPrice = historyDetail.MaxPrice.ToString(),
                    };
                    foreach (ProductAttribute p in modelProduct)
                    {
                        if (p.ProductId == historyDetail.ProductId)
                        {
                            item.MinPriceToday = p.MinPrice.ToString();
                            item.MaxPriceToday = p.MaxPrice.ToString();
                        }
                    }
                    result.Add(item);
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        [HttpGet]
        public bool getCheckApprove()
        {            
            if (UpdateUserPriceModel.checkApprove == true)
            {
                UpdateUserPriceModel.checkApprove = false;
                return true;
            }
            return false;
        }
    }


}