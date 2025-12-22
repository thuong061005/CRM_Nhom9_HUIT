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
    public class KhuyenMaiController : Controller
    {
        private QL_KHACHHANG_DICHVUEntities db = new QL_KHACHHANG_DICHVUEntities();

        // GET: KhuyenMai
        public ActionResult Index()
        {
            return View(db.KHUYEN_MAI.ToList());
        }

        // GET: KhuyenMai/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KHUYEN_MAI kHUYEN_MAI = db.KHUYEN_MAI.Find(id);
            if (kHUYEN_MAI == null)
            {
                return HttpNotFound();
            }
            return View(kHUYEN_MAI);
        }

        // GET: KhuyenMai/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: KhuyenMai/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MAKM,TENKM,NGAYBD,NGAYKT,GIAMGIA")] KHUYEN_MAI kHUYEN_MAI)
        {
            if (ModelState.IsValid)
            {
                db.KHUYEN_MAI.Add(kHUYEN_MAI);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(kHUYEN_MAI);
        }

        // GET: KhuyenMai/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KHUYEN_MAI kHUYEN_MAI = db.KHUYEN_MAI.Find(id);
            if (kHUYEN_MAI == null)
            {
                return HttpNotFound();
            }
            return View(kHUYEN_MAI);
        }

        // POST: KhuyenMai/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MAKM,TENKM,NGAYBD,NGAYKT,GIAMGIA")] KHUYEN_MAI kHUYEN_MAI)
        {
            if (ModelState.IsValid)
            {
                db.Entry(kHUYEN_MAI).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(kHUYEN_MAI);
        }

        // GET: KhuyenMai/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KHUYEN_MAI kHUYEN_MAI = db.KHUYEN_MAI.Find(id);
            if (kHUYEN_MAI == null)
            {
                return HttpNotFound();
            }
            return View(kHUYEN_MAI);
        }

        // POST: KhuyenMai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            KHUYEN_MAI kHUYEN_MAI = db.KHUYEN_MAI.Find(id);
            db.KHUYEN_MAI.Remove(kHUYEN_MAI);
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
