using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using THANHTOAN.Models;

public class ThanhToanController : Controller
{
    private QL_KHACHHANG_DICHVUEntities db = new QL_KHACHHANG_DICHVUEntities();

    // ================= INDEX =================
    public ActionResult Index()
    {
        // Thêm .Include để lấy dữ liệu từ bảng liên quan
        var thanh_toan = db.THANH_TOAN.Include(t => t.KHACH_HANG).Include(t => t.KHUYEN_MAI);
        return View(thanh_toan.ToList());
    }

    // ================= DETAILS =================
    public ActionResult Details(string id)
    {
        if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        var tt = db.THANH_TOAN
            .Include(t => t.KHACH_HANG)
            .Include(t => t.KHUYEN_MAI)
            .FirstOrDefault(t => t.MATT == id);

        if (tt == null) return HttpNotFound();
        return View(tt);
    }

    // ================= CREATE =================
    public ActionResult Create()
    {
        LoadDropdown();
        return View(new THANH_TOAN { NGAYTT = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(THANH_TOAN model)
    {
        if (string.IsNullOrEmpty(model.MAKH))
        {
            ModelState.AddModelError("MAKH", "Vui lòng chọn khách hàng.");
        }

        if (model.TONGTIEN <= 0)
        {
            ModelState.AddModelError("TONGTIEN", "Số tiền thanh toán phải lớn hơn 0.");
        }
        if (string.IsNullOrEmpty(model.PHUONGTHUC))
        {
            ModelState.AddModelError("PHUONGTHUC", "Bạn chưa chọn phương thức thanh toán.");
        }

        model.MATT = GenerateMaTT();
        model.NGAYTT = DateTime.Now;

        // Xóa lỗi mặc định để cập nhật giá trị mới
        ModelState.Remove("MATT");
        ModelState.Remove("TONGTIEN");

        if (ModelState.IsValid)
        {
            try
            {
                // Nếu có chọn mã khuyến mãi
                if (!string.IsNullOrEmpty(model.MAKM))
                {
                    var km = db.KHUYEN_MAI.Find(model.MAKM);
                    if (km != null)
                    {
                        
                        decimal phanTramGiam = km.GIAMGIA ?? 0;
                        model.TONGTIEN = Math.Round(model.TONGTIEN * (1 - (phanTramGiam / 100)), 2);
                    }
                }

                db.THANH_TOAN.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", "Lỗi Database: " + msg);
            }
        }

        LoadDropdown(model);
        return View(model);
    }
    // ================= EDIT =================
    public ActionResult Edit(string id)
    {
        if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        var tt = db.THANH_TOAN.Find(id);
        if (tt == null) return HttpNotFound();

        LoadDropdown(tt);
        return View(tt);
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public ActionResult Edit(THANH_TOAN model)
{
    if (string.IsNullOrEmpty(model.PHUONGTHUC))
    {
        ModelState.AddModelError("PHUONGTHUC", "Bạn chưa chọn phương thức thanh toán.");
    }
    // 1. Xử lý chuỗi rỗng cho MAKM
    if (string.IsNullOrWhiteSpace(model.MAKM)) model.MAKM = null;

    // 2. Xóa cache ModelState của TONGTIEN để cập nhật giá trị tính toán mới
    ModelState.Remove("TONGTIEN");

    if (ModelState.IsValid)
    {
        try
        {
            // 3. Logic tính lại tiền (Nếu kế toán sửa giá gốc hoặc sửa mã KM)
            if (!string.IsNullOrEmpty(model.MAKM))
            {
                var km = db.KHUYEN_MAI.Find(model.MAKM);
                if (km != null)
                {
                    // Kiểm tra hạn dùng của KM tại thời điểm NGAYTT
                    if (model.NGAYTT >= km.NGAYBD && model.NGAYTT <= km.NGAYKT)
                    {
                        decimal tile = km.GIAMGIA ?? 0;
                        model.TONGTIEN = Math.Round(model.TONGTIEN * (1 - (tile / 100)), 2);
                    }
                }
            }

            // 4. Cập nhật vào DB
            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.InnerException?.Message ?? ex.Message;
            ModelState.AddModelError("", "Không thể cập nhật: " + msg);
        }
    }

    LoadDropdown(model);
    return View(model);
}

    // ================= DELETE =================
    public ActionResult Delete(string id)
    {
        if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        var tt = db.THANH_TOAN
            .Include(t => t.KHACH_HANG)
            .Include(t => t.KHUYEN_MAI)
            .FirstOrDefault(t => t.MATT == id);

        if (tt == null) return HttpNotFound();
        return View(tt);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(string MATT)
    {
        var tt = db.THANH_TOAN.Find(MATT);
        db.THANH_TOAN.Remove(tt);
        db.SaveChanges();
        return RedirectToAction("Index");
    }

    // ================= SUPPORT =================
    private void LoadDropdown(THANH_TOAN tt = null)
    {
        ViewBag.MAKH = new SelectList(db.KHACH_HANG, "MAKH", "HOTEN", tt?.MAKH);

        // Lọc khuyến mãi: Chỉ lấy các mã còn hạn sử dụng so với ngày hôm nay
        DateTime today = DateTime.Today;
        var dsKhuyenMaiHieuLuc = db.KHUYEN_MAI
            .Where(km => today >= km.NGAYBD && today <= km.NGAYKT)
            .ToList();

        ViewBag.MAKM = new SelectList(dsKhuyenMaiHieuLuc, "MAKM", "TENKM", tt?.MAKM);

        ViewBag.PHUONGTHUC = new SelectList(new[]
        {
        "Tiền mặt", "Chuyển khoản", "Ví điện tử", "Thẻ (POS)"
    }, tt?.PHUONGTHUC);
    }

    private string GenerateMaTT()
    {
        var last = db.THANH_TOAN.OrderByDescending(x => x.MATT).Select(x => x.MATT).FirstOrDefault();
        if (last == null) return "TT001";
        int num = int.Parse(last.Substring(2));
        return "TT" + (num + 1).ToString("D3");
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            db?.Dispose();
        }
        base.Dispose(disposing);
    }
}

