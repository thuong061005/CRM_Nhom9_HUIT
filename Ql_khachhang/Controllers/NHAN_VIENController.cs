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
    public class NHAN_VIENController : Controller
    {
        private QL_KHACHHANG_DICHVUEntities db = new QL_KHACHHANG_DICHVUEntities();

        // GET: NHAN_VIEN
        //public ActionResult Index()
        //{
        //    var nHAN_VIEN = db.NHAN_VIEN.Include(n => n.CHI_NHANH);
        //    return View(nHAN_VIEN.ToList());
        //}

        public ActionResult Index(string machinhanh, string chucvu)
        {
            var nv = db.NHAN_VIEN.Include(n => n.CHI_NHANH).AsQueryable();

            // 🔍 LỌC THEO CHI NHÁNH
            if (!string.IsNullOrEmpty(machinhanh))
            {
                nv = nv.Where(n => n.MACHINHANH == machinhanh);
            }

            // 🔍 LỌC THEO CHỨC VỤ
            if (!string.IsNullOrEmpty(chucvu))
            {
                nv = nv.Where(n => n.CHUCVU.Contains(chucvu));
            }

            // Dropdown chi nhánh
            ViewBag.MACHINHANH = new SelectList(
                db.CHI_NHANH,
                "MACHINHANH",
                "TENCHINHANH",
                machinhanh
            );

            ViewBag.ChucVu = chucvu;

            return View(nv.ToList());
        }


        // GET: NHAN_VIEN/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NHAN_VIEN nHAN_VIEN = db.NHAN_VIEN.Find(id);
            if (nHAN_VIEN == null)
            {
                return HttpNotFound();
            }
            return View(nHAN_VIEN);
        }

        // GET: NHAN_VIEN/Create
        public ActionResult Create()
        {
            ViewBag.MACHINHANH = new SelectList(db.CHI_NHANH, "MACHINHANH", "TENCHINHANH");
            return View();
        }

        // POST: NHAN_VIEN/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "HOTEN,CHUCVU,LUONG,MACHINHANH")] NHAN_VIEN nHAN_VIEN)
        {
            if (ModelState.IsValid)
            {
                int maxSo = db.NHAN_VIEN
                    .Where(n => n.MANV.StartsWith("NV"))
                    .AsEnumerable()
                    .Select(n => int.Parse(n.MANV.Substring(2)))
                    .DefaultIfEmpty(0)
                    .Max();

                nHAN_VIEN.MANV = "NV" + (maxSo + 1).ToString("D3");

                db.NHAN_VIEN.Add(nHAN_VIEN);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MACHINHANH = new SelectList(
                db.CHI_NHANH,
                "MACHINHANH",
                "TENCHINHANH",
                nHAN_VIEN.MACHINHANH
            );

            return View(nHAN_VIEN);
        }


        // GET: NHAN_VIEN/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NHAN_VIEN nHAN_VIEN = db.NHAN_VIEN.Find(id);
            if (nHAN_VIEN == null)
            {
                return HttpNotFound();
            }
            ViewBag.MACHINHANH = new SelectList(db.CHI_NHANH, "MACHINHANH", "TENCHINHANH", nHAN_VIEN.MACHINHANH);
            return View(nHAN_VIEN);
        }

        // POST: NHAN_VIEN/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MANV,HOTEN,CHUCVU,LUONG,MACHINHANH")] NHAN_VIEN nHAN_VIEN)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nHAN_VIEN).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MACHINHANH = new SelectList(db.CHI_NHANH, "MACHINHANH", "TENCHINHANH", nHAN_VIEN.MACHINHANH);
            return View(nHAN_VIEN);
        }

        // GET: NHAN_VIEN/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NHAN_VIEN nHAN_VIEN = db.NHAN_VIEN.Find(id);
            if (nHAN_VIEN == null)
            {
                return HttpNotFound();
            }
            return View(nHAN_VIEN);
        }

        // POST: NHAN_VIEN/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            NHAN_VIEN nHAN_VIEN = db.NHAN_VIEN.Find(id);
            db.NHAN_VIEN.Remove(nHAN_VIEN);
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
