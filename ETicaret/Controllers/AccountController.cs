using ETicaret.DB;
using ETicaret.Models.Account;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ETicaret.Controllers
{
    public class AccountController : BaseController
    {
       

        // GET: Account
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Models.Account.RegisterModels user, HttpPostedFileBase Foto)
        {
            try
            {
                if (user.Member.Password != user.rePassword)
                {
                    throw new Exception("Şifreler aynı değildir");
                }
                if (context.Members.Any(x => x.Email == user.Member.Email))
                {
                    throw new Exception("Aynı isimde kayıtlı e-posta adresi bulunmaktadır.");
                }
            user.Member.MemberType = DB.MemberType.Customer;
            user.Member.AddedDate = DateTime.Now;
            context.Members.Add(user.Member);
            context.SaveChanges();
                return RedirectToAction("Login", "Account");
            }
            catch(Exception ex)
            {
                ViewBag.ReError = ex.Message; //passwordlar esit degilse yukarıdaki mesajı ekrana bastırıcak
                return View();

            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Models.Account.LoginModels model)
        {
            try
            {
                var user = context.Members.FirstOrDefault((x => x.Email == model.Member.Email && x.Password == model.Member.Password));
                if(user != null)
                {
                    Session["LogonUser"] = user;
                    return RedirectToAction("index", "i");
                }
                else
                {
                    ViewBag.ReError = "Kullanici bilgileriniz yanlis."; //Giris bilgileri yanlissa ekrana basilacak
                    return View();
                }
            }
            catch(Exception ex)
            {
                ViewBag.ReError = "Kullanici bilgileriniz yanlis."; //Giris bilgileri yanlissa ekrana basilacak
                return View();
            }
        }


        public ActionResult Logout()
        {
            Session["LogonUser"] = null;
            return RedirectToAction("Login", "Account");
        }
 

        [HttpGet]
        public ActionResult Profil(int id)
        {
            var user = context.Members.FirstOrDefault(x => x.Id == id);
            List<DB.Addresses> adress = context.Addresses.Where(x => x.Member_Id == id).ToList(); /*userin adresleri alındı*/
            if (user == null)
            {
                return RedirectToAction("Index", "i");
            }
            ProfilModels model = new ProfilModels()
            {
                Members = user,
                Addresses=adress
            };
            return View(model);
        }

        [HttpGet]
        public ActionResult ProfilEdit()
        {
            int id = base.CurrentUserId();
            var user = context.Members.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return RedirectToAction("Index", "i");
            }
            ProfilModels model = new ProfilModels()
            {
                Members = user
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult ProfilEdit(ProfilModels models, HttpPostedFileBase Foto)
        {

            int id = CurrentUserId();
                var updateMember = context.Members.FirstOrDefault(x => x.Id == id);
                if (Foto != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(updateMember.ProfileImageName)))
                    {
                        System.IO.File.Delete(Server.MapPath(updateMember.ProfileImageName));
                    }
                    WebImage img = new WebImage(Foto.InputStream);
                    FileInfo fotoinfo = new FileInfo(Foto.FileName);
                    string newfoto = Guid.NewGuid().ToString() + fotoinfo.Extension;
                   /*extension resim uzantısı*/
                   img.Resize(350, 350);
                  img.Save("~/images/mi/" + newfoto);
                string filepath = "/images/mi/" + newfoto;
                updateMember.ProfileImageName = filepath;
            }
          
                updateMember.ModifiedDate = DateTime.Now;
                updateMember.Name = models.Members.Name;
                updateMember.Email = models.Members.Email;
                updateMember.Surname = models.Members.Surname;
                updateMember.Bio = models.Members.Bio;
                if (string.IsNullOrEmpty(models.Members.Password) == false)
                {
                    updateMember.Password = models.Members.Password;
                 }
                 
                context.SaveChanges();
              Session["Logonuser"] = (DB.Members)updateMember;
              return RedirectToAction("Profil", "Account", new { id = CurrentUserId() });


        }

        [HttpPost]
        public ActionResult Addresses(DB.Addresses address)
        {
                address.Id = Guid.NewGuid();
            address.Member_Id = CurrentUserId();
            address.AddedDate = DateTime.Now;
            context.Addresses.Add(address);
            context.SaveChanges();
            return RedirectToAction("Profil", "Account",new { id=CurrentUserId()});
        }

        [HttpGet]
        public ActionResult AddressesDelete(string id)
        {
            var guid = new Guid(id);
            var deladress = context.Addresses.FirstOrDefault(x => x.Id == guid);
            context.Addresses.Remove(deladress);
            context.SaveChanges();
            return RedirectToAction("Profil", "Account", new { id = CurrentUserId() });
        }



    }
}