﻿using SmartB.UI.Models.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.Script.Services;

namespace SmartB.UI.Areas.Admin.Controllers
{
    public class ManageProductController : Controller
    {
        private SmartBuyEntities context = new SmartBuyEntities();
      //  private const int PageSize = 10;
        //
        // GET: /Admin/ManageProduct/

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            var products = from p in context.Products
                           select p;
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.Name.ToUpper().Contains(searchString.ToUpper()));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(s => s.Name);
                    break;
                default:
                    products = products.OrderBy(s => s.Name);
                    break;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(products.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            //bind drop down list
            var market = from m in context.Markets
                         orderby m.Name
                         select m;
            ViewBag.ddlMarket = new SelectList(market, "Id", "Name");
            return View();
        }

        public JsonResult SaveSellProduct(string productName, int marketId, int sellPrice)
        {
            try
            {
                // Not exist
                Product product = context.Products.FirstOrDefault(x => x.Name.Equals(productName));
                var market = context.Markets.Where(m => m.Id.Equals(marketId)).FirstOrDefault();
                if (product != null)
                {
                    var sellProduct = context.SellProducts
                        .FirstOrDefault(x => x.ProductId == product.Id && x.MarketId == marketId);
                    if (sellProduct != null)
                    {
                        TempData["create"] = "Duplicate";
                    }
                }
                else
                {
                    // add product
                    var newProduct = new SmartB.UI.Models.EntityFramework.Product
                    {
                        Name = productName,
                        IsActive = true,
                    };
                    var addedProduct = context.Products.Add(newProduct);

                    var newSellProduct = new SmartB.UI.Models.EntityFramework.SellProduct //add SellProduct
                    {
                        Market = market,
                        Product = addedProduct,
                        SellPrice = sellPrice,
                        LastUpdatedTime = DateTime.Now
                    };
                    var addedSellProduct = context.SellProducts.Add(newSellProduct);
                    context.SaveChanges(); // Save to database

                    var productAttribute = new SmartB.UI.Models.EntityFramework.ProductAttribute
                    {
                        ProductId = addedProduct.Id,
                        MinPrice = sellPrice,
                        MaxPrice = sellPrice,
                        LastUpdatedTime = DateTime.Now,
                    };
                    var addedProductAtt = context.ProductAttributes.Add(productAttribute);
                    context.SaveChanges(); // Save to database
                    // add Product Dictionary
                    var dictionaries = productName.Split(';').ToList();
                    var dupProductDictionary = context.Dictionaries.Where(p => p.Name.Equals(productName)).FirstOrDefault();
                    foreach (string dictionary in dictionaries)
                    {
                        if (dupProductDictionary == null && dictionary != "")
                        {
                            var ProductDic = new SmartB.UI.Models.EntityFramework.Dictionary
                            {
                                Name = dictionary,
                                ProductId = addedProduct.Id
                            };
                            var addProductDic = context.Dictionaries.Add(ProductDic);
                        }
                    }

                    context.SaveChanges(); // Save to database

                    TempData["create"] = "Success";
                }
               return Json(JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(JsonRequestBehavior.AllowGet);
            }
            
        }

        public ActionResult Edit(int id)
        {
            var sellProduct = context.SellProducts.FirstOrDefault(x => x.Product.Id == id);
            var listSellProduct = context.SellProducts
                .Include(x => x.Market)
                .Where(x => x.ProductId == id)
                .ToList();
            
            //bind drop down list
            List<Market> listMarket = new List<Market>();
            foreach (var sp in listSellProduct)
            {
                var market = from m in context.Markets where m.Id == sp.MarketId
                             orderby m.Name
                             select m;
                listMarket.Add(market.FirstOrDefault());
            }
            var sortListMarket = listMarket.OrderBy(m => m.Name);
            ViewBag.ddlMarket = new SelectList(sortListMarket, "Id", "Name");
            return View(sellProduct);
        }

        [HttpPost]
        public RedirectToRouteResult Edit(SellProduct model)
        {
            var product = context.Products.Where(x => x.Name == model.Product.Name).FirstOrDefault();
            var sellProduct = context.SellProducts.FirstOrDefault(x => x.ProductId == product.Id && x.Market.Id == model.MarketId);
            string message;
            if (sellProduct != null)
            {
                sellProduct.MarketId = model.MarketId;
                sellProduct.SellPrice = model.SellPrice;
                sellProduct.LastUpdatedTime = System.DateTime.Now;
                context.SaveChanges();
                //add new product Attribute      
                var oldAttribute = context.ProductAttributes.Where(p => p.ProductId == product.Id).OrderByDescending(p => p.LastUpdatedTime).FirstOrDefault(); // get old Attribute
                if (sellProduct.SellPrice < oldAttribute.MinPrice)
                {
                    var productAttribute = new SmartB.UI.Models.EntityFramework.ProductAttribute
                    {
                        ProductId = product.Id,
                        MinPrice = sellProduct.SellPrice,
                        MaxPrice = oldAttribute.MaxPrice,
                        LastUpdatedTime = DateTime.Now,
                    };
                    var addedProductAtt = context.ProductAttributes.Add(productAttribute);
                    context.SaveChanges(); // Save to database
                }
                
                else if (sellProduct.SellPrice > oldAttribute.MaxPrice)
                {
                    var productAttribute = new SmartB.UI.Models.EntityFramework.ProductAttribute
                    {
                        ProductId = product.Id,
                        MinPrice = oldAttribute.MinPrice,
                        MaxPrice = sellProduct.SellPrice,
                        LastUpdatedTime = DateTime.Now,
                    };
                    var addedProductAtt = context.ProductAttributes.Add(productAttribute);
                    context.SaveChanges(); // Save to database
                }
                else if (sellProduct.SellPrice > oldAttribute.MinPrice && sellProduct.SellPrice < oldAttribute.MaxPrice)
                {
                    var productAttribute = new SmartB.UI.Models.EntityFramework.ProductAttribute
                    {
                        ProductId = product.Id,
                        MinPrice = oldAttribute.MinPrice,
                        MaxPrice = oldAttribute.MaxPrice,
                        LastUpdatedTime = DateTime.Now,
                    };
                    var addedProductAtt = context.ProductAttributes.Add(productAttribute);
                    context.SaveChanges(); // Save to database
                }
                
                context.SaveChanges(); // Save to database
                message = "Success";
            }else
            {
                message = "Failed";
            }
            TempData["edit"] = message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public RedirectToRouteResult Delete(int[] ids)
        {
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    var sellProduct = context.SellProducts.FirstOrDefault(x => x.Id == id);
                    if (sellProduct != null)
                    {
                        context.SellProducts.Remove(sellProduct);
                    }
                }
                context.SaveChanges();
                TempData["delete"] = "Done";
            }
            else
            {
                TempData["delete"] = "Empty";
            }
            return RedirectToAction("Index");
        }
        protected override void Dispose(bool disposing)
        {
            context.Dispose();
            base.Dispose(disposing);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<string> GetProductName(string pre)
        {
            SmartBuyEntities context = new SmartBuyEntities();
            List<string> allCompanyName = new List<string>();
            
                allCompanyName = (from a in context.Products
                                  where a.Name.StartsWith(pre)
                                  select a.Name).ToList();
            return allCompanyName;
        }

        [HttpPost]
        public ActionResult SetActive(int id)
        {
            var product = context.Products.FirstOrDefault(x => x.Id == id);
            bool statusFlag = false;
            if (ModelState.IsValid)
            {
                if (product.IsActive == true)
                {
                    product.IsActive = false;
                    statusFlag = false;
                }
                else
                {
                    product.IsActive = true;
                    statusFlag = true;
                }
                context.SaveChanges();
            }

            // Display the confirmation message
            var results = new Product
            {
                IsActive = statusFlag
            };

            return Json(results);
        }
        public JsonResult ChangePrice(string product, string market)
        {
            var listSellProduct = context.SellProducts.ToList();
            var getProduct = context.Products.Where(p => p.Name.Equals(product)).FirstOrDefault();
            var sellProduct = listSellProduct.Where(s => s.Product.Id == getProduct.Id && s.Market.Id == Convert.ToInt32(market)).FirstOrDefault();
            int price = Convert.ToInt32(sellProduct.SellPrice);
            return Json(price);
        }
    }
}
