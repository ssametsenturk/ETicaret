using ETicaret.DB;
using ETicaret.Models;
using ETicaret.Models.Account;
using ETicaret.Models.i;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaret.Controllers
{
    public class iController : BaseController
    {
     
     
        // GET: i
        [HttpGet]
        public ActionResult Index(int id=0)
        {
            IQueryable<DB.Products> products = context.Products;
            DB.Categories category = null;
            if (id>0)
            {
                products = products.Where(x => x.Category_Id == id);
                category = context.Categories.FirstOrDefault(x => x.Id == id);
            }

            var viewModel = new Models.i.IndexModel()
            {
                Products = products.ToList(),
                Category=category
            };
            
            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Product(int id=0)
        {
            var pro = context.Products.FirstOrDefault(x => x.Id == id);
            if (pro == null)
            {
                return RedirectToAction("Index", "i");
            }
            ProductModels model = new ProductModels()
            {
                Product = pro,
                Comments=pro.Comments.ToList()
            }; 
            return View(model);
        }

        public JsonResult YorumYap(string yorum, int productid)
        {
            var uye = (DB.Members)Session["LogonUser"];
         
            if (yorum != null)
            {
               var product= context.Products.FirstOrDefault(x => x.Id == productid);
                   product.Comments.Add(new Comments { Member_Id = Convert.ToInt32(uye.Id), Product_Id = productid, Text = yorum, AddedDate = DateTime.Now });
                context.SaveChanges();
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

   
        [HttpGet]
        public ActionResult AddBasket(int id)
        {
            List<BasketModelssip> basket = null;
            if (Session["Basket"] == null)
            {
                basket = new List<BasketModelssip>();
            }
            else
            {
                basket = (List<BasketModelssip>)Session["Basket"];
            }

            if (basket.Any(x=>x.Product.Id==id))
            {
                var pro = basket.FirstOrDefault(x => x.Product.Id == id);
                if (pro.Product.IsContinued == true )
                {
                    if (pro.Product.UnitsInStock > pro.Count && pro.Product.UnitsInStock > 0)
                    {
                    pro.Count++;
                    }
                    else 
                    {
                        TempData["Error"] = "Yeterli stok yok";
                    }
                }
                else
                {
                    TempData["Error"] = "Ürün satışı durduruldu";
                }
            }
            else
            {
                var pro = context.Products.FirstOrDefault(x => x.Id == id);
                if (pro != null && pro.IsContinued && pro.UnitsInStock>0) 
                {
                    basket.Add(new BasketModelssip()
                    {
                        Product = pro,
                        Count = 1
                    }
                        );
                }
                else
                {
                    TempData["Error"] = "Ürün satışı yoktur.";
                }
            }
            Session["Basket"] = basket;

            return RedirectToAction("Basket", "i");
        }

        [HttpGet]
        public ActionResult DecreaseBasket(int id)
        {
            List<BasketModelssip> basket = null;
            if (Session["Basket"] == null)
            {
                basket = new List<BasketModelssip>();
            }
            else
            {
                basket = (List<BasketModelssip>)Session["Basket"];
            }

            if (basket.Any(x => x.Product.Id == id))
            {
                var pro = basket.FirstOrDefault(x => x.Product.Id == id);
                if (pro.Product.IsContinued)
                {
                    if (pro.Count > 1)
                    {
                        pro.Count--;
                       
                    }
                    else
                    {
                        pro.Count= 0;
                       
                    }
                }
                else
                {
                    TempData["Error"] = "Ürün satışı durduruldu";
                }
            }
            else
            {
               //hata
            }
            basket.RemoveAll(x => x.Count < 1);
             Session["Basket"] = basket;

            return RedirectToAction("Basket", "i");
        }

        [HttpGet]
        public ActionResult Basket()
        {
            List<BasketModelssip> model = (List<BasketModelssip>)Session["Basket"];
            if (model == null)
            {
               model = new List<BasketModelssip>();
            }

            if ( (DB.Members)CurrentUser() !=null )
            {
                int adresmemberid = CurrentUserId();
                ViewBag.CurrentAdress = context.Addresses.Where(x=>x.Member_Id==adresmemberid).Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                    }).ToList();

            }
           
            
            //foreach(KeyValuePair<int,int> item in basket.Products)
            //{
            //    var product = context.Products.FirstOrDefault(x => x.Id == item.Key);
            //    if(product != null)
            //    {
            //        model.Add(new BasketModelssip
            //        {
            //            Product = product,
            //            Count = item.Value
            //        });
            //    }
            //}
            ViewBag.TotalPrice = model.Select(x => x.Product.Price * x.Count).Sum();

            return View(model);

        }

        [HttpGet]
        public ActionResult RemoveBasket(int id)
        {
            List<BasketModelssip> basket = null;
            if (Session["Basket"] == null)
            {
                basket = new List<BasketModelssip>();
            }
            else
            {
                basket = (List<BasketModelssip>)Session["Basket"];
            }
            basket.RemoveAll(x=>x.Product.Id==id);
            Session["Basket"] = basket;
            return RedirectToAction("Basket", "i");
        }

        [HttpGet]
        public ActionResult ClearBasket()
        {
            List<BasketModelssip> basket = null;
            if (Session["Basket"] == null)
            {
                basket = new List<BasketModelssip>();
            }
            else
            {
                basket = (List<BasketModelssip>)Session["Basket"];
                
            }
            basket.Clear();
            Session["Basket"] = basket;
            return RedirectToAction("Basket", "i");
        }

        [HttpPost]  /*Basket.cshtml den satın al butonuyla dropdownlistteki adres id mizi aldık.Sepet bilgileride Sessiondan çekicez.*/
        public ActionResult BuyPro(string Address,string OrderDescription)
        {

            if (CurrentUser() == null )
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                try
                {
                    var basket = (List<Models.i.BasketModelssip>)Session["Basket"];
                    var guid = new Guid(Address);
                    var  orderdesc = OrderDescription;
                    var myaddress = context.Addresses.FirstOrDefault(x => x.Id == guid);
                    var order = new DB.Orders()
                    {
                        AddedDate = DateTime.Now,
                        Address = myaddress.AdresDescription,
                        Member_Id = CurrentUserId(),
                        //status:Siparis verildi=SV-Ödeme bildirimi=OB-Ödeme onaylandı=OO
                        Status = "SV",
                        Description= orderdesc,
                        Id=Guid.NewGuid()  //Guid Id yi atadım.
                    };
                    foreach (Models.i.BasketModelssip item in basket)
                    {
                        var orderdetail = new DB.OrderDetails()
                        {
                            AddedDate = DateTime.Now,
                            //  Order_Id = order.Id,
                            Product_Id = item.Product.Id,
                            Price = item.Product.Price * item.Count,
                            Quantity = item.Count,
                            Id = Guid.NewGuid()
                        };
                        order.OrderDetails.Add(orderdetail); //Direk içerisine eklendiğinden OrderId si otomatikmen Ordere bağlanıcak 
                        var updateproduct = context.Products.FirstOrDefault(x => x.Id == item.Product.Id);

                       
                        if (updateproduct.UnitsInStock >= item.Count && updateproduct.UnitsInStock>=0 && updateproduct != null && updateproduct.IsContinued== true) //Ürün kontrolü
                        {
                            updateproduct.UnitsInStock = updateproduct.UnitsInStock - item.Count; //Satıştan sonra ürünümüzün stoğunu güncelledim.
                        }
                        else  //Ürün kontrolünde aksilik çıkarsa ürün stok bitmesi satıştan kaldırılma vb.
                        {
                            throw new Exception(string.Format("{0} ürününde yeterli stok kalmamıştır yada satıştan kaldırılmıştır.Siparişiniz iptal ediliyor anlayışınız için teşekkürler.",item.Product.Name));
                        }
                        
                    }
                    context.Orders.Add(order);  //Orderi ekledik ki order details de etkilensin
                    context.SaveChanges();
                    
                }
                catch(Exception e)
                {
                    ViewBag.Error = e.Message;
                }
                int memberid = CurrentUserId();
                var userorders = context.Orders.Where(x => x.Member_Id == memberid).ToList().OrderByDescending(x=>x.AddedDate);
             
                return View(userorders);
            }
            
        }
        [HttpGet]  /*Yeni sipariş vermeden siparişlerimi görmek istersem*/
        public ActionResult BuyPro()
        {
            int memberid = CurrentUserId();
            var userorders = context.Orders.Where(x => x.Member_Id == memberid).ToList();

            return View(userorders);
        }

        [HttpGet]
        public ActionResult OrderDetails(string id)
        {
            var orid = new Guid(id);
            var details = context.OrderDetails.Where(x => x.Order_Id == orid).ToList();
            return View(details);
        }


        [HttpGet]
        //[HttpPost]
        public JsonResult GetProductDes(int id)
        {
            var pro = context.Products.FirstOrDefault(x => x.Id == id);
            return Json(pro.Description, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult Product(DB.Members member)
        //{
        //    return RedirectToAction("Product","i")
        //}
    }
}