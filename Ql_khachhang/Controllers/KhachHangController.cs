using Ql_khachhang.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ql_khachhang.Controllers
{
    public class KhachHangController : Controller
    {
        private QL_KHACHHANG_DICHVUEntities db = new QL_KHACHHANG_DICHVUEntities();

        // --- 1. DANH SÁCH KHÁCH HÀNG ---
        public ActionResult Index()
        {
            var dsKhachHang = db.KHACH_HANG.ToList();

            foreach (var kh in dsKhachHang)
            {
                // 1. Tự lấy tổng tiền từ bảng THANH_TOAN trong Database
                decimal tongChiTieu = db.THANH_TOAN
                    .Where(t => t.MAKH == kh.MAKH)
                    .Sum(t => (decimal?)t.TONGTIEN) ?? 0;

                // 2. Tự tính lại Điểm (100k = 1 điểm)
                kh.DIEMTICHLUY = (int)(tongChiTieu / 100000);

                // 3. Tự xét hạng theo chuẩn CRM
                if (tongChiTieu >= 5000000) kh.LOAIKH = "Thân thiết";
                else if (tongChiTieu >= 1000000) kh.LOAIKH = "Tiềm năng";
                else kh.LOAIKH = "Khách mới";
            }

            // Lưu tất cả thay đổi vào DB một lần duy nhất
            db.SaveChanges();

            return View(dsKhachHang);
        }

        // --- 2. TẠO MỚI (GET) ---
        public ActionResult Create()
        {
            // LOGIC 1: TỰ ĐỘNG SINH MÃ KH+1
            var lastKH = db.KHACH_HANG.OrderByDescending(x => x.MAKH).FirstOrDefault();
            string nextID = "KH001";
            if (lastKH != null)
            {
                if (int.TryParse(lastKH.MAKH.Substring(2), out int lastNum))
                {
                    nextID = "KH" + (lastNum + 1).ToString("D3");
                }
            }

            var model = new KHACH_HANG
            {
                MAKH = nextID,
                LOAIKH = "Khách mới",
                DIEMTICHLUY = 0
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KHACH_HANG kh)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra trùng mã (phòng ngừa)
                if (db.KHACH_HANG.Any(x => x.MAKH == kh.MAKH))
                {
                    ModelState.AddModelError("MAKH", "Mã khách hàng đã tồn tại!");
                    return View(kh);
                }

                db.KHACH_HANG.Add(kh);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(kh);
        }

        // --- 3. CHỈNH SỬA (GET) ---
        public ActionResult Edit(string id)
        {
            var kh = db.KHACH_HANG.Find(id);
            if (kh == null) return HttpNotFound();
            return View(kh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KHACH_HANG kh)
        {
            if (ModelState.IsValid)
            {
                db.Entry(kh).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(kh);
        }

        // --- 4. LOGIC 2: CẬP NHẬT THỨ HẠNG KHÁCH HÀNG (Dựa trên chi tiêu) ---
        // Bạn có thể gọi hàm này sau mỗi lần khách thanh toán thành công
        public ActionResult UpdateRank(string id)
        {
            var kh = db.KHACH_HANG.Find(id);
            if (kh == null) return HttpNotFound();

            // 1. Tính tổng chi tiêu
            decimal totalSpent = db.THANH_TOAN
                .Where(t => t.MAKH == id)
                .Sum(t => (decimal?)t.TONGTIEN) ?? 0;

            // 2. Logic cập nhật Thứ hạng (Rank)
            if (totalSpent >= 5000000) kh.LOAIKH = "Thân thiết";
            else if (totalSpent >= 1000000) kh.LOAIKH = "Tiềm năng";
            else kh.LOAIKH = "Khách mới";

            // 3. Logic Tích điểm (Ví dụ: 100k được 1 điểm)
            kh.DIEMTICHLUY = (int)(totalSpent / 100000);

            db.SaveChanges();

            // Gửi một thông báo nhỏ cho ngườ   i dùng biết đã cập nhật xong
            TempData["Success"] = "Đã cập nhật thứ hạng và điểm cho khách hàng " + kh.HOTEN;

            return RedirectToAction("Index");
        }

        // --- 5. XÓA ---
        public ActionResult Delete(string id)
        {
            var kh = db.KHACH_HANG.Find(id);
            if (kh != null)
            {
                // Lưu ý: Nếu khách có Lịch hẹn hoặc Thanh toán thì sẽ bị lỗi FK
                // Nên kiểm tra trước khi xóa
                if (db.LICH_HEN.Any(l => l.MAKH == id))
                {
                    TempData["Error"] = "Không thể xóa khách hàng đã có lịch hẹn!";
                    return RedirectToAction("Index");
                }
                db.KHACH_HANG.Remove(kh);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
