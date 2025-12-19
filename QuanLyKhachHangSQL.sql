CREATE DATABASE QL_KHACHHANG_DICHVU;
GO

USE QL_KHACHHANG_DICHVU;
GO

-- Bảng 1: Khách hàng
CREATE TABLE KHACH_HANG (
    MAKH VARCHAR(5) PRIMARY KEY,
    HOTEN NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15) NOT NULL,
    EMAIL VARCHAR(50),
    LOAIKH NVARCHAR(30) CHECK (LOAIKH IN (N'Khách mới', N'Thân thiết', N'Tiềm năng')),
    DIEMTICHLUY INT DEFAULT 0
);

-- Bảng 2: Nhân viên
CREATE TABLE NHAN_VIEN (
    MANV VARCHAR(5) PRIMARY KEY,
    HOTEN NVARCHAR(100) NOT NULL,
    CHUCVU NVARCHAR(50),
    LUONG DECIMAL(18,2)
);

-- Bảng 3: Dịch vụ
CREATE TABLE DICH_VU (
    MADV VARCHAR(5) PRIMARY KEY,
    TENDV NVARCHAR(100) NOT NULL,
    GIA DECIMAL(18,2) NOT NULL,
    MOTA NVARCHAR(500)
);

-- Bảng 4: Lịch hẹn (liên kết KH, NV, DV)
CREATE TABLE LICH_HEN (
    MALICH VARCHAR(5) PRIMARY KEY,
    NGAYGIO DATETIME NOT NULL,
    TRANGTHAI NVARCHAR(30) CHECK (TRANGTHAI IN (N'Chờ xác nhận', N'Đã xác nhận', N'Đã hoàn thành', N'Hủy')),
    MAKH VARCHAR(5) FOREIGN KEY REFERENCES KHACH_HANG(MAKH),
    MANV VARCHAR(5) FOREIGN KEY REFERENCES NHAN_VIEN(MANV),
    MADV VARCHAR(5) FOREIGN KEY REFERENCES DICH_VU(MADV)
);

-- Bảng 5: Khuyến mãi
CREATE TABLE KHUYEN_MAI (
    MAKM VARCHAR(5) PRIMARY KEY,
    TENKM NVARCHAR(100),
    NGAYBD DATE,
    NGAYKT DATE,
    GIAMGIA DECIMAL(5,2) CHECK (GIAMGIA BETWEEN 0 AND 100)
);

-- 1. Tạo bảng trung gian N-N: Khách hàng - Khuyến mãi
CREATE TABLE KHACHHANG_KHUYENMAI (
    MAKH VARCHAR(5) FOREIGN KEY REFERENCES KHACH_HANG(MAKH),
    MAKM VARCHAR(5) FOREIGN KEY REFERENCES KHUYEN_MAI(MAKM),
    NGAY_APDUNG DATE DEFAULT GETDATE(),
    PRIMARY KEY (MAKH, MAKM)
);

-- Bảng 6: Thanh toán (liên kết KH, có thể sau khi dịch vụ hoàn tất)
CREATE TABLE THANH_TOAN (
    MATT VARCHAR(5) PRIMARY KEY,
    MAKH VARCHAR(5) FOREIGN KEY REFERENCES KHACH_HANG(MAKH),
    NGAYTT DATE NOT NULL,
    TONGTIEN DECIMAL(18,2) NOT NULL,
    -- Thêm N'Thẻ (POS)' vào dòng này
    PHUONGTHUC NVARCHAR(30) CHECK (PHUONGTHUC IN (N'Tiền mặt', N'Chuyển khoản', N'Ví điện tử', N'Thẻ (POS)')),
    MAKM VARCHAR(5) NULL FOREIGN KEY REFERENCES KHUYEN_MAI(MAKM)
);

-- Bảng 7: Báo cáo dữ liệu (tổng hợp doanh thu, lịch hẹn, phản hồi,...)
CREATE TABLE BAO_CAO (
    MABC VARCHAR(5) PRIMARY KEY,
    NGAYLAP DATE NOT NULL,
    DOANHTHU DECIMAL(18,2),
    SOLUONGKH INT,
    SOLUONGLICH INT,
    GHICHU NVARCHAR(300)
);

-- . Cập nhật bảng BAO_CAO
ALTER TABLE BAO_CAO
ADD 
    LOAI_BC NVARCHAR(50) DEFAULT N'Tổng hợp',
    NGAY_BATDAU DATE,
    NGAY_KETTHUC DATE;


-- Bảng 8: Phản hồi / chăm sóc sau bán
CREATE TABLE PHAN_HOI (
    MAPH VARCHAR(5) PRIMARY KEY,
    MAKH VARCHAR(5) FOREIGN KEY REFERENCES KHACH_HANG(MAKH),
    MADV VARCHAR(5) FOREIGN KEY REFERENCES DICH_VU(MADV),
    NOIDUNG NVARCHAR(500),
    DANHGIA INT CHECK (DANHGIA BETWEEN 1 AND 5),
    NGAYPH DATE
);

USE QL_KHACHHANG_DICHVU;
GO

-- Bảng mới: Chi nhánh
CREATE TABLE CHI_NHANH (
    MACHINHANH VARCHAR(5) PRIMARY KEY,              -- Mã chi nhánh (ví dụ: CN001)
    TENCHINHANH NVARCHAR(100) NOT NULL,             -- Tên chi nhánh (ví dụ: Chi nhánh TP.HCM)
    DIACHI NVARCHAR(200),                           -- Địa chỉ chi nhánh
    SDT VARCHAR(15),                                -- Số điện thoại chi nhánh
);

-- Cập nhật bảng NHAN_VIEN: Thêm khóa ngoại liên kết đến CHI_NHANH
ALTER TABLE NHAN_VIEN
ADD MACHINHANH VARCHAR(5) NULL FOREIGN KEY REFERENCES CHI_NHANH(MACHINHANH);

-- (Tùy chọn) Nếu muốn bắt buộc mỗi nhân viên phải thuộc một chi nhánh, thay NULL bằng NOT NULL
-- ALTER TABLE NHAN_VIEN
-- ALTER COLUMN MACHINHANH VARCHAR(5) NOT NULL;

USE QL_KHACHHANG_DICHVU;
GO

-- 1. Chèn dữ liệu bảng CHI_NHANH (Cần có trước để Nhân viên tham chiếu)
INSERT INTO CHI_NHANH (MACHINHANH, TENCHINHANH, DIACHI, SDT) VALUES
('CN001', N'Chi nhánh Quận 1', N'123 Nguyễn Huệ, P. Bến Nghé, Q.1, TP.HCM', '02839102030'),
('CN002', N'Chi nhánh Tân Phú', N'456 Lũy Bán Bích, P. Hòa Thạnh, Q. Tân Phú, TP.HCM', '02838112233'),
('CN003', N'Chi nhánh Gò Vấp', N'789 Phan Văn Trị, P.7, Q. Gò Vấp, TP.HCM', '02835889900'),
('CN004', N'Chi nhánh Quận 7', N'101 Nguyễn Thị Thập, P. Tân Phong, Q.7, TP.HCM', '02837778899'),
('CN005', N'Chi nhánh Thủ Đức', N'202 Võ Văn Ngân, P. Bình Thọ, TP. Thủ Đức', '02836667788'),
('CN006', N'Chi nhánh Bình Thạnh', N'303 Xô Viết Nghệ Tĩnh, P.25, Q. Bình Thạnh', '02835556677'),
('CN007', N'Chi nhánh Quận 3', N'404 Võ Văn Tần, P.5, Q.3, TP.HCM', '02834445566'),
('CN008', N'Chi nhánh Hà Nội 1', N'505 Cầu Giấy, Q. Cầu Giấy, Hà Nội', '02433334455'),
('CN009', N'Chi nhánh Hà Nội 2', N'606 Hai Bà Trưng, Q. Hoàn Kiếm, Hà Nội', '02432223344'),
('CN010', N'Chi nhánh Đà Nẵng', N'707 Nguyễn Văn Linh, Q. Hải Châu, Đà Nẵng', '02361112233');

-- 2. Chèn dữ liệu bảng KHACH_HANG
INSERT INTO KHACH_HANG (MAKH, HOTEN, SDT, EMAIL, LOAIKH, DIEMTICHLUY) VALUES
('KH001', N'Nguyễn Văn An', '0901234567', 'an.nguyen@email.com', N'Thân thiết', 150),
('KH002', N'Trần Thị Bích', '0912345678', 'bich.tran@email.com', N'Khách mới', 0),
('KH003', N'Lê Hoàng Cường', '0923456789', 'cuong.le@email.com', N'Tiềm năng', 50),
('KH004', N'Phạm Minh Dung', '0934567890', 'dung.pham@email.com', N'Thân thiết', 300),
('KH005', N'Hoàng Văn Em', '0945678901', 'em.hoang@email.com', N'Khách mới', 10),
('KH006', N'Vũ Thị Phương', '0956789012', 'phuong.vu@email.com', N'Tiềm năng', 60),
('KH007', N'Đặng Văn Giang', '0967890123', 'giang.dang@email.com', N'Thân thiết', 500),
('KH008', N'Bùi Thị Hoa', '0978901234', 'hoa.bui@email.com', N'Khách mới', 0),
('KH009', N'Đỗ Văn Hùng', '0989012345', 'hung.do@email.com', N'Tiềm năng', 40),
('KH010', N'Ngô Thị Lan', '0990123456', 'lan.ngo@email.com', N'Thân thiết', 200),
('KH011', N'Lý Văn Minh', '0909998887', 'minh.ly@email.com', N'Khách mới', 0);

-- 3. Chèn dữ liệu bảng DICH_VU
INSERT INTO DICH_VU (MADV, TENDV, GIA, MOTA) VALUES
('DV001', N'Cắt tóc nam', 100000, N'Cắt tóc tạo kiểu cho nam giới'),
('DV002', N'Cắt tóc nữ', 200000, N'Cắt tóc tạo kiểu layer, bob... cho nữ'),
('DV003', N'Gội đầu dưỡng sinh', 150000, N'Gội đầu thảo dược kết hợp massage cổ vai gáy'),
('DV004', N'Nhuộm tóc thời trang', 800000, N'Nhuộm các màu hot trend, thuốc nhập khẩu'),
('DV005', N'Uốn tóc Setting', 600000, N'Uốn xoăn lọn to, giữ nếp lâu'),
('DV006', N'Massage Body đá nóng', 450000, N'Massage toàn thân với đá nóng thư giãn 60 phút'),
('DV007', N'Chăm sóc da mặt cơ bản', 300000, N'Làm sạch sâu, đắp mặt nạ dưỡng ẩm'),
('DV008', N'Lấy nhân mụn chuẩn y khoa', 250000, N'Quy trình 12 bước, không sưng đỏ'),
('DV009', N'Tẩy tế bào chết toàn thân', 350000, N'Sử dụng muối khoáng và tinh dầu thiên nhiên'),
('DV010', N'Làm Nail trọn gói', 200000, N'Cắt da, sơn gel tay chân');

-- 4. Chèn dữ liệu bảng KHUYEN_MAI
INSERT INTO KHUYEN_MAI (MAKM, TENKM, NGAYBD, NGAYKT, GIAMGIA) VALUES
('KM001', N'Mừng khai trương', '2024-01-01', '2024-01-31', 20.00),
('KM002', N'Khuyến mãi Valentine', '2024-02-10', '2024-02-15', 15.00),
('KM003', N'Chào hè rực rỡ', '2024-06-01', '2024-06-30', 10.00),
('KM004', N'Black Friday', '2024-11-25', '2024-11-30', 50.00),
('KM005', N'Giáng sinh an lành', '2024-12-20', '2024-12-25', 25.00),
('KM006', N'Khách hàng thân thiết', '2024-01-01', '2024-12-31', 5.00),
('KM007', N'Sinh nhật khách hàng', '2024-01-01', '2024-12-31', 30.00),
('KM008', N'Combo Gội + Cắt', '2024-03-01', '2024-03-31', 10.00),
('KM009', N'Mua 1 tặng 1', '2024-04-30', '2024-05-01', 50.00),
('KM010', N'Tri ân phái đẹp 8/3', '2024-03-05', '2024-03-09', 20.00);

-- 5. Chèn dữ liệu bảng NHAN_VIEN
INSERT INTO NHAN_VIEN (MANV, HOTEN, CHUCVU, LUONG, MACHINHANH) VALUES
('NV001', N'Nguyễn Thị Mai', N'Quản lý', 15000000, 'CN001'),
('NV002', N'Trần Văn Nam', N'Kỹ thuật viên', 8000000, 'CN001'),
('NV003', N'Lê Thị Thu', N'Lễ tân', 7000000, 'CN002'),
('NV004', N'Phạm Văn Hậu', N'Kỹ thuật viên', 8500000, 'CN002'),
('NV005', N'Hoàng Thị Yến', N'Kỹ thuật viên', 9000000, 'CN003'),
('NV006', N'Vũ Văn Tài', N'Bảo vệ', 6000000, 'CN003'),
('NV007', N'Đặng Thị Kim', N'Quản lý', 14000000, 'CN004'),
('NV008', N'Bùi Văn Long', N'Kỹ thuật viên', 8200000, 'CN005'),
('NV009', N'Đỗ Thị Ngọc', N'Lễ tân', 7200000, 'CN006'),
('NV010', N'Ngô Văn Phát', N'Kỹ thuật viên', 8800000, 'CN001');

-- 6. Chèn dữ liệu bảng KHACHHANG_KHUYENMAI (Bảng trung gian)
INSERT INTO KHACHHANG_KHUYENMAI (MAKH, MAKM, NGAY_APDUNG) VALUES
('KH001', 'KM006', '2024-02-01'),
('KH004', 'KM006', '2024-02-15'),
('KH007', 'KM007', '2024-05-20'),
('KH010', 'KM006', '2024-03-10'),
('KH001', 'KM001', '2024-01-15'),
('KH002', 'KM001', '2024-01-20'),
('KH003', 'KM008', '2024-03-05'),
('KH005', 'KM003', '2024-06-12'),
('KH006', 'KM003', '2024-06-15'),
('KH008', 'KM009', '2024-05-01');

-- 7. Chèn dữ liệu bảng LICH_HEN
INSERT INTO LICH_HEN (MALICH, NGAYGIO, TRANGTHAI, MAKH, MANV, MADV) VALUES
('LH001', '2024-10-25 09:00:00', N'Đã hoàn thành', 'KH001', 'NV002', 'DV001'),
('LH002', '2024-10-25 10:30:00', N'Đã hoàn thành', 'KH002', 'NV004', 'DV003'),
('LH003', '2024-10-26 14:00:00', N'Hủy', 'KH003', 'NV005', 'DV004'),
('LH004', '2024-10-26 15:30:00', N'Đã xác nhận', 'KH004', 'NV002', 'DV006'),
('LH005', '2024-10-27 09:00:00', N'Chờ xác nhận', 'KH005', 'NV008', 'DV002'),
('LH006', '2024-10-27 11:00:00', N'Đã xác nhận', 'KH006', 'NV010', 'DV007'),
('LH007', '2024-10-28 08:30:00', N'Đã hoàn thành', 'KH007', 'NV002', 'DV005'),
('LH008', '2024-10-28 16:00:00', N'Chờ xác nhận', 'KH008', 'NV004', 'DV009'),
('LH009', '2024-10-29 10:00:00', N'Đã xác nhận', 'KH009', 'NV005', 'DV008'),
('LH010', '2024-10-30 13:30:00', N'Chờ xác nhận', 'KH010', 'NV010', 'DV010');

-- 8. Chèn dữ liệu bảng THANH_TOAN
INSERT INTO THANH_TOAN (MATT, MAKH, NGAYTT, TONGTIEN, PHUONGTHUC, MAKM) VALUES
('TT001', 'KH001', '2024-10-25', 95000, N'Tiền mặt', 'KM006'), -- Giảm 5% của 100k
('TT002', 'KH002', '2024-10-25', 150000, N'Ví điện tử', NULL),
('TT003', 'KH007', '2024-10-28', 420000, N'Chuyển khoản', 'KM007'), -- Giảm 30% của 600k
('TT004', 'KH004', '2024-10-26', 450000, N'Thẻ (POS)', NULL), -- Sửa lại logic: POS chưa có trong check, dùng Tiền mặt tạm hoặc update check
('TT005', 'KH006', '2024-10-27', 270000, N'Tiền mặt', 'KM008'), -- Giảm 10% 300k
('TT006', 'KH009', '2024-10-29', 250000, N'Ví điện tử', NULL),
('TT007', 'KH001', '2024-09-15', 200000, N'Tiền mặt', NULL),
('TT008', 'KH010', '2024-08-20', 190000, N'Chuyển khoản', 'KM006'),
('TT009', 'KH003', '2024-07-10', 800000, N'Ví điện tử', NULL),
('TT010', 'KH005', '2024-10-01', 120000, N'Tiền mặt', NULL);

-- Lưu ý: Ở bảng THANH_TOAN, tôi dùng giá trị 'Tiền mặt', 'Chuyển khoản', 'Ví điện tử' khớp với Check Constraint.

-- 9. Chèn dữ liệu bảng PHAN_HOI
INSERT INTO PHAN_HOI (MAPH, MAKH, MADV, NOIDUNG, DANHGIA, NGAYPH) VALUES
('PH001', 'KH001', 'DV001', N'Dịch vụ rất tốt, nhân viên nhiệt tình.', 5, '2024-10-25'),
('PH002', 'KH002', 'DV003', N'Gội đầu thư giãn, nhưng nước hơi lạnh.', 4, '2024-10-25'),
('PH003', 'KH007', 'DV005', N'Tóc uốn đẹp, giữ nếp lâu. Sẽ quay lại.', 5, '2024-10-28'),
('PH004', 'KH004', 'DV006', N'Kỹ thuật viên tay nghề cao.', 5, '2024-10-26'),
('PH005', 'KH006', 'DV007', N'Không gian hơi ồn ào.', 3, '2024-10-27'),
('PH006', 'KH009', 'DV008', N'Lấy mụn sạch, không đau.', 5, '2024-10-29'),
('PH007', 'KH003', 'DV004', N'Màu nhuộm không giống mẫu lắm.', 3, '2024-07-11'),
('PH008', 'KH010', 'DV010', N'Làm nail tỉ mỉ, đẹp.', 4, '2024-08-21'),
('PH009', 'KH005', 'DV002', N'Bình thường, không có gì đặc sắc.', 3, '2024-10-02'),
('PH010', 'KH008', 'DV009', N'Rất hài lòng về dịch vụ tẩy tế bào chết.', 5, '2024-09-15');

-- 10. Chèn dữ liệu bảng BAO_CAO
INSERT INTO BAO_CAO (MABC, NGAYLAP, DOANHTHU, SOLUONGKH, SOLUONGLICH, GHICHU, LOAI_BC, NGAY_BATDAU, NGAY_KETTHUC) VALUES
('BC001', '2024-01-31', 150000000, 120, 150, N'Doanh thu tháng 1 ổn định', N'Báo cáo tháng', '2024-01-01', '2024-01-31'),
('BC002', '2024-02-29', 145000000, 110, 140, N'Doanh thu giảm nhẹ do nghỉ tết', N'Báo cáo tháng', '2024-02-01', '2024-02-29'),
('BC003', '2024-03-31', 160000000, 130, 160, N'Tăng trưởng tốt sau tết', N'Báo cáo tháng', '2024-03-01', '2024-03-31'),
('BC004', '2024-04-30', 180000000, 150, 180, N'Doanh thu cao dịp lễ 30/4', N'Báo cáo tháng', '2024-04-01', '2024-04-30'),
('BC005', '2024-05-31', 155000000, 125, 155, N'Ổn định', N'Báo cáo tháng', '2024-05-01', '2024-05-31'),
('BC006', '2024-06-30', 140000000, 115, 145, N'Mùa mưa khách ít hơn', N'Báo cáo tháng', '2024-06-01', '2024-06-30'),
('BC007', '2024-06-30', 930000000, 750, 930, N'Tổng kết 6 tháng đầu năm', N'Báo cáo quý', '2024-01-01', '2024-06-30'),
('BC008', '2024-07-31', 150000000, 120, 150, N'Triển khai gói khuyến mãi mới hiệu quả', N'Báo cáo tháng', '2024-07-01', '2024-07-31'),
('BC009', '2024-08-31', 165000000, 135, 165, N'Doanh thu tăng trưởng đều', N'Báo cáo tháng', '2024-08-01', '2024-08-31'),
('BC010', '2024-09-30', 170000000, 140, 170, N'Chuẩn bị kế hoạch cuối năm', N'Báo cáo tháng', '2024-09-01', '2024-09-30');