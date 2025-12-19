using Ql_khachhang.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ql_khachhang.Controllers
{
    public class LICHHENController : Controller
    {
        private QL_KHACHHANG_DICHVUEntities db = new QL_KHACHHANG_DICHVUEntities();

        // --- 1. DANH SÁCH LỊCH HẸN ---
        public ActionResult Index()
        {
            var lichHens = db.LICH_HEN.Include(l => l.KHACH_HANG).Include(l => l.NHAN_VIEN).Include(l => l.DICH_VU);
            return View(lichHens.ToList());
        }

        // --- 2. TẠO MỚI (GET) ---
        public ActionResult Create()
        {
            // 1. Logic tự động sinh mã LH+1
            // Lấy mã lịch hẹn cuối cùng trong DB
            var lastLich = db.LICH_HEN.OrderByDescending(x => x.MALICH).FirstOrDefault();
            string nextID = "LH001"; // Mặc định nếu chưa có dữ liệu

            if (lastLich != null)
            {
                // Tách phần số từ mã (VD: "LH015" -> 15)
                string numericPart = lastLich.MALICH.Substring(2);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    // Tăng thêm 1 và định dạng lại chuỗi (000)
                    nextID = "LH" + (lastNumber + 1).ToString("D3");
                }
            }

            // Gán mã vào Model để truyền ra View
            var model = new LICH_HEN { MALICH = nextID, NGAYGIO = DateTime.Now };

            ViewBag.MAKH = new SelectList(db.KHACH_HANG, "MAKH", "HOTEN");
            ViewBag.MANV = new SelectList(db.NHAN_VIEN, "MANV", "HOTEN");
            ViewBag.MADV = new SelectList(db.DICH_VU, "MADV", "TENDV");

            return View(model);
        }

        // --- 3. XỬ LÝ TẠO MỚI (POST) + KIỂM TRA TRÙNG LỊCH ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LICH_HEN lichHen)
        {
            if (ModelState.IsValid)
            {
                // 1. KIỂM TRA TRÙNG MÃ (Phòng trường hợp 2 người cùng mở form một lúc)
                if (db.LICH_HEN.Any(x => x.MALICH == lichHen.MALICH))
                {
                    var lastLich = db.LICH_HEN.OrderByDescending(x => x.MALICH).FirstOrDefault();
                    int lastNum = int.Parse(lastLich.MALICH.Substring(2));
                    lichHen.MALICH = "LH" + (lastNum + 1).ToString("D3");
                }

                // 2. LOGIC KIỂM TRA TRÙNG LỊCH (Khoảng cách 60 phút)
                DateTime newStart = lichHen.NGAYGIO;
                DateTime newEnd = lichHen.NGAYGIO.AddMinutes(60);

                // Kiểm tra xem nhân viên này có lịch nào bị giao thoa thời gian không
                var isDuplicate = db.LICH_HEN.Any(l =>
                    l.MANV == lichHen.MANV &&
                    l.TRANGTHAI != "Hủy" &&
                    ((newStart >= l.NGAYGIO && newStart < DbFunctions.AddMinutes(l.NGAYGIO, 60)) ||
                     (newEnd > l.NGAYGIO && newEnd <= DbFunctions.AddMinutes(l.NGAYGIO, 60))));

                if (isDuplicate)
                {
                    // Nếu trùng, báo lỗi ra màn hình
                    ModelState.AddModelError("", "Nhân viên này đã có lịch hẹn khác trong khoảng thời gian này (mỗi ca 60p). Vui lòng chọn giờ khác!");
                }
                else
                {
                    // Nếu mọi thứ ổn, tiến hành lưu
                    try
                    {
                        db.LICH_HEN.Add(lichHen);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                    }
                }
            }

            // Nếu có lỗi, load lại các DropdownList để người dùng chọn lại
            ViewBag.MAKH = new SelectList(db.KHACH_HANG, "MAKH", "HOTEN", lichHen.MAKH);
            ViewBag.MANV = new SelectList(db.NHAN_VIEN, "MANV", "HOTEN", lichHen.MANV);
            ViewBag.MADV = new SelectList(db.DICH_VU, "MADV", "TENDV", lichHen.MADV);
            return View(lichHen);
        }

        // --- 4. CHỈNH SỬA (GET) ---
        public ActionResult Edit(string id)
        {
            var lichHen = db.LICH_HEN.Find(id);
            if (lichHen == null) return HttpNotFound();

            ViewBag.MAKH = new SelectList(db.KHACH_HANG, "MAKH", "HOTEN", lichHen.MAKH);
            ViewBag.MANV = new SelectList(db.NHAN_VIEN, "MANV", "HOTEN", lichHen.MANV);
            ViewBag.MADV = new SelectList(db.DICH_VU, "MADV", "TENDV", lichHen.MADV);
            return View(lichHen);
        }

        // --- 5. XỬ LÝ CHỈNH SỬA (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LICH_HEN lichHen)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lichHen).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(lichHen);
        }

        // --- 6. XÓA ---
        public ActionResult Delete(string id)
        {
            var lichHen = db.LICH_HEN.Find(id);
            if (lichHen != null)
            {
                db.LICH_HEN.Remove(lichHen);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
