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
    public class DichVuController : Controller
    {
        private QL_KHACHHANG_DICHVUEntities db = new QL_KHACHHANG_DICHVUEntities();

        // GET: DichVu
        public ActionResult Index()
        {
            return View(db.DICH_VU.ToList());
        }

        // GET: DichVu/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DICH_VU dICH_VU = db.DICH_VU.Find(id);
            if (dICH_VU == null)
            {
                return HttpNotFound();
            }
            return View(dICH_VU);
        }

        // GET: DichVu/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DichVu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MADV,TENDV,GIA,MOTA")] DICH_VU dICH_VU)
        {
            if (ModelState.IsValid)
            {
                db.DICH_VU.Add(dICH_VU);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(dICH_VU);
        }

        // GET: DichVu/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DICH_VU dICH_VU = db.DICH_VU.Find(id);
            if (dICH_VU == null)
            {
                return HttpNotFound();
            }
            return View(dICH_VU);
        }

        // POST: DichVu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MADV,TENDV,GIA,MOTA")] DICH_VU dICH_VU)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dICH_VU).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(dICH_VU);
        }

        // GET: DichVu/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DICH_VU dICH_VU = db.DICH_VU.Find(id);
            if (dICH_VU == null)
            {
                return HttpNotFound();
            }
            return View(dICH_VU);
        }

        // POST: DichVu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            DICH_VU dICH_VU = db.DICH_VU.Find(id);
            db.DICH_VU.Remove(dICH_VU);
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
