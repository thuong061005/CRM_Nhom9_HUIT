using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json; // Cần cài NuGet: Newtonsoft.Json

using Ql_khachhang.Models; // SỬA LẠI namespace này theo tên Project của bạn

namespace Ql_khachhang.Controllers
{
    public class HomeController : Controller
    {
        // Khởi tạo kết nối CSDL
        private QL_KHACHHANG_DICHVUEntities db = new QL_KHACHHANG_DICHVUEntities();

        public ActionResult Index(string fromDate, string toDate)
        {
            // --- 1. XỬ LÝ LỌC NGÀY (FILTER) ---
            DateTime startDate, endDate;

            // Nếu không chọn ngày bắt đầu -> Mặc định là ngày 1/1 năm nay
            if (string.IsNullOrEmpty(fromDate))
                startDate = new DateTime(DateTime.Now.Year, 1, 1);
            else
                startDate = DateTime.Parse(fromDate);

            // Nếu không chọn ngày kết thúc -> Mặc định là hôm nay
            if (string.IsNullOrEmpty(toDate))
                endDate = DateTime.Now;
            else
                endDate = DateTime.Parse(toDate);

            // Đưa dữ liệu ngày ra View để giữ lại giá trị trong ô input
            ViewBag.FromDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.ToDate = endDate.ToString("yyyy-MM-dd");


            // --- 2. THỐNG KÊ CARDS (SỐ LIỆU TỔNG QUAN) ---
            try
            {
                ViewBag.TotalCustomers = db.KHACH_HANG.Count();
                ViewBag.TodayAppointments = db.LICH_HEN
                    .Count(x => x.NGAYGIO.Year == DateTime.Now.Year
                             && x.NGAYGIO.Month == DateTime.Now.Month
                             && x.NGAYGIO.Day == DateTime.Now.Day);
                ViewBag.TotalServices = db.DICH_VU.Count();

                // Doanh thu tháng này (Dùng ?? 0 để tránh lỗi nếu null)
                ViewBag.MonthlyRevenue = db.THANH_TOAN
                    .Where(x => x.NGAYTT.Month == DateTime.Now.Month && x.NGAYTT.Year == DateTime.Now.Year)
                    .Sum(x => (decimal?)x.TONGTIEN) ?? 0;
            }
            catch
            {
                // Nếu lỗi kết nối DB thì gán bằng 0 hết để web không sập
                ViewBag.TotalCustomers = 0;
                ViewBag.TodayAppointments = 0;
                ViewBag.TotalServices = 0;
                ViewBag.MonthlyRevenue = 0;
            }


            // --- 3. XỬ LÝ DỮ LIỆU BIỂU ĐỒ (THEO KHOẢNG THỜI GIAN) ---
            List<string> chartLabels = new List<string>();
            List<decimal> chartData = new List<decimal>();

            // Lấy dữ liệu thô trong khoảng thời gian đã chọn
            var rawData = db.THANH_TOAN
                .Where(x => x.NGAYTT >= startDate && x.NGAYTT <= endDate)
                .ToList();

            // Vòng lặp chạy từng tháng (hoặc từng ngày nếu khoảng cách ngắn)
            // Ở đây tôi làm theo THÁNG để báo cáo tổng quan
            DateTime iterator = new DateTime(startDate.Year, startDate.Month, 1);
            DateTime endIterator = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddDays(-1);

            while (iterator <= endIterator)
            {
                // Label: "10/2024"
                chartLabels.Add(iterator.ToString("MM/yyyy"));

                // Tính tổng tiền của tháng đó
                decimal total = rawData
                    .Where(x => x.NGAYTT.Month == iterator.Month && x.NGAYTT.Year == iterator.Year)
                    .Sum(x => x.TONGTIEN);

                // Chia cho 1 triệu để số nhỏ gọn (Đơn vị: Triệu VNĐ)
                chartData.Add(total / 1000000m);

                iterator = iterator.AddMonths(1);
            }

            // Chuyển sang JSON để View sử dụng
            ViewBag.ChartLabels = JsonConvert.SerializeObject(chartLabels);
            ViewBag.ChartData = JsonConvert.SerializeObject(chartData);


            // --- 4. LẤY DANH SÁCH LỊCH HẸN & PHẢN HỒI ---
            var model = new DashboardViewModel();

            // Lịch hẹn sắp tới (Lấy 5 cái)
            model.Appointments = db.LICH_HEN
                .Where(x => x.NGAYGIO >= DateTime.Today) // Lấy từ hôm nay trở đi
                .OrderBy(x => x.NGAYGIO)
                .Take(5)
                .ToList() // Execute SQL trước
                .Select(x => new AppointmentViewModel
                {
                    CustomerName = x.KHACH_HANG?.HOTEN ?? "Khách vãng lai",
                    ServiceName = x.DICH_VU?.TENDV ?? "Dịch vụ xóa",
                    Time = x.NGAYGIO.ToString("HH:mm dd/MM/yyyy"),
                    Status = x.TRANGTHAI
                }).ToList();

            ViewBag.AppointmentCount = model.Appointments.Count;

            // Phản hồi mới nhất (Lấy 5 cái)
            // Kiểm tra xem có bảng PHAN_HOI không, nếu chưa có thì để list rỗng
            try
            {
                model.Feedbacks = db.PHAN_HOI
                    .OrderByDescending(x => x.NGAYPH)
                    .Take(5)
                    .ToList()
                    .Select(x => new FeedbackViewModel
                    {
                        CustomerName = x.KHACH_HANG?.HOTEN ?? "Ẩn danh",
                        ServiceName = x.DICH_VU?.TENDV ?? "",
                        Rating = x.DANHGIA ?? 5,
                        Comment = x.NOIDUNG,
                        Date = x.NGAYPH.HasValue ? x.NGAYPH.Value.ToString("dd/MM/yyyy") : ""
                    }).ToList();
            }
            catch
            {
                model.Feedbacks = new List<FeedbackViewModel>();
            }

            return View(model);
        }
    }

    // --- VIEW MODELS (Các class chứa dữ liệu hiển thị) ---
    public class DashboardViewModel
    {
        public List<AppointmentViewModel> Appointments { get; set; } = new List<AppointmentViewModel>();
        public List<FeedbackViewModel> Feedbacks { get; set; } = new List<FeedbackViewModel>();
    }

    public class AppointmentViewModel
    {
        public string CustomerName { get; set; }
        public string ServiceName { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
    }

    public class FeedbackViewModel
    {
        public string CustomerName { get; set; }
        public string ServiceName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
    }
}