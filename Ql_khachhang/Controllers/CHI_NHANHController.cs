using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ql_khachhang.Models;

namespace Ql_khachhang.Controllers
{
    public class CHI_NHANHController : Controller
    {
        private QL_KHACHHANG_DICHVUEntities db = new QL_KHACHHANG_DICHVUEntities();

        // GET: CHI_NHANH
        public ActionResult Index()
        {
            return View(db.CHI_NHANH.ToList());
        }

        // GET: CHI_NHANH/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var chiNhanh = db.CHI_NHANH
                             .Include(c => c.NHAN_VIEN)
                             .FirstOrDefault(c => c.MACHINHANH == id);

            if (chiNhanh == null)
                return HttpNotFound();

            return View(chiNhanh);
        }

        // GET: CHI_NHANH/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CHI_NHANH/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TENCHINHANH,DIACHI,SDT")] CHI_NHANH chiNhanh)
        {
            if (ModelState.IsValid)
            {
                // 🔥 SINH MÃ CHI NHÁNH TỰ ĐỘNG
                int maxSo = 0;

                if (db.CHI_NHANH.Any())
                {
                    maxSo = db.CHI_NHANH
                        .AsEnumerable() // ✅ QUAN TRỌNG
                        .Where(c => c.MACHINHANH.StartsWith("CN"))
                        .Select(c => int.Parse(c.MACHINHANH.Substring(2)))
                        .DefaultIfEmpty(0)
                        .Max();
                }

                chiNhanh.MACHINHANH = "CN" + (maxSo + 1).ToString("D2");

                db.CHI_NHANH.Add(chiNhanh);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(chiNhanh);
        }

        // GET: CHI_NHANH/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CHI_NHANH cHI_NHANH = db.CHI_NHANH.Find(id);
            if (cHI_NHANH == null)
            {
                return HttpNotFound();
            }
            return View(cHI_NHANH);
        }

        // POST: CHI_NHANH/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CHI_NHANH model)
        {
            if (!ModelState.IsValid)
                return View(model);

            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }


        public ActionResult Delete(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var chiNhanh = db.CHI_NHANH
                             .Include(c => c.NHAN_VIEN)
                             .FirstOrDefault(c => c.MACHINHANH == id);

            if (chiNhanh == null)
                return HttpNotFound();

            return View(chiNhanh);
        }


        // POST: CHI_NHANH/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public ActionResult DeleteConfirmed(string id)
        {
            var chiNhanh = db.CHI_NHANH
                             .Include(c => c.NHAN_VIEN)
                             .FirstOrDefault(c => c.MACHINHANH == id);

            if (chiNhanh == null)
                return HttpNotFound();

            // 🚨 CÒN NHÂN VIÊN → KHÔNG CHO XÓA
            if (chiNhanh.NHAN_VIEN.Any())
            {
                ViewBag.Error = "Không thể xóa chi nhánh vì vẫn còn nhân viên đang làm việc tại đây.";
                return View("Delete", chiNhanh);
            }

            db.CHI_NHANH.Remove(chiNhanh);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
