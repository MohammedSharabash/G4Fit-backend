using G4Fit.Helpers;
using G4Fit.Models.Domains;
using G4Fit.Models.Enums;
using G4Fit.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace G4Fit.Controllers.MVC
{
    [Authorize(Roles = "Supplier")]
    public class SupplierOrdersController : BaseController
    {
        public ActionResult Index()
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var Orders = db.OrderItems.Where(s => s.IsDeleted == false && s.Order.IsDeleted == false && (s.Order.OrderStatus == OrderStatus.Placed || s.Order.OrderStatus == OrderStatus.Delivered) && s.Service.SupplierId == Supplier.Id).OrderByDescending(s => s.Order.CreatedOn).GroupBy(w => w.Order).ToList();
            return View(Orders);
        }

        public ActionResult Details(long? OrderId)
        {
            var Supplier = db.Suppliers.FirstOrDefault(w => w.IsDeleted == false && w.UserId == CurrentUserId && w.IsAccepted == true);
            if (Supplier == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.SupplierId = Supplier.Id;
            if (OrderId.HasValue == false)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var Order = db.Orders.Find(OrderId);
                if (Order != null)
                {
                    return View(Order);
                }
            }
            return RedirectToAction("Index");
        }

    }
}