# SystemQMS

Backend API cho hệ thống quản lý xếp hàng (Queue Management System - QMS), được xây dựng bằng ASP.NET Core và PostgreSQL. Ứng dụng cung cấp các API để quản lý dịch vụ, quầy phục vụ, thiết bị hiển thị, phiếu số thứ tự, công thức cấp số và cấu hình hiển thị/âm thanh.

## Công nghệ sử dụng

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- PostgreSQL thông qua `Npgsql.EntityFrameworkCore.PostgreSQL`
- Swagger / OpenAPI
- SignalR
- ClosedXML để xuất danh sách phiếu ra Excel

## Chức năng chính

- Quản lý dịch vụ: tạo, xem, cập nhật, lọc và xóa dịch vụ.
- Quản lý quầy: tạo, cập nhật, xem chi tiết, lọc và xem các phiếu tại quầy.
- Quản lý thiết bị: đăng ký, cập nhật trạng thái/thông tin/âm lượng, tạo mã đăng nhập và đăng nhập thiết bị.
- Quản lý phiếu số thứ tự và xuất dữ liệu phiếu ra Excel.
- Cấu hình công thức cấp số, xem trước số và chuyển dịch vụ sang chế độ dùng chung.
- Cấu hình giao diện hiển thị và mẫu đọc số thứ tự.
- Phát sinh chuỗi âm thanh từ số thứ tự, tên quầy và mẫu đọc.

## Yêu cầu

- .NET SDK 8.x
- PostgreSQL 14 trở lên (khuyến nghị)
- Git

## Cài đặt và chạy

```bash
git clone https://github.com/dkhoa123/SystemQMS.git
cd SystemQMS
dotnet restore
```

Tạo database PostgreSQL, sau đó cập nhật chuỗi kết nối trong `SystemQMS/appsettings.json` hoặc dùng biến cấu hình của ASP.NET Core:

```json
{
  "ConnectionStrings": {
    "ConnectionDb": "Host=localhost;Port=5432;Database=SystemQMS;Username=postgres;Password=your-password"
  },
  "DeviceAuth": {
    "Secret": "your-long-random-secret"
  }
}
```

Không commit mật khẩu database hoặc secret thật vào repository. Có thể đặt các giá trị trên bằng biến môi trường:

```bash
export ConnectionStrings__ConnectionDb="Host=localhost;Port=5432;Database=SystemQMS;Username=postgres;Password=your-password"
export DeviceAuth__Secret="your-long-random-secret"
```

Áp dụng migration và chạy ứng dụng:

```bash
dotnet ef database update --project SystemQMS
dotnet run --project SystemQMS
```

Các URL mặc định trong môi trường Development:

- HTTP API: `http://localhost:5201`
- HTTPS API: `https://localhost:7058`
- Swagger UI: `http://localhost:5201/swagger` hoặc `https://localhost:7058/swagger`

## API chính

| Nhóm | Base route | Mô tả |
|---|---|---|
| Services | `/api/Services` | Quản lý dịch vụ và các phiếu đã cấp |
| Counters | `/api/Counters` | Quản lý quầy và phiếu tại quầy |
| Devices | `/api/Device` | Quản lý thiết bị và mã đăng nhập |
| Stickers | `/api/Stickers` | Tạo và xuất phiếu |
| Number formulas | `/api/NumberFomular` | Cấu hình quy tắc cấp số |
| Display config | `/api/v1/configs/display` | Cấu hình giao diện và bảng màu |
| Audio config | `/api/v1/configs/audio` | Cấu hình, xem trước mẫu đọc số |

Swagger là nguồn tham khảo đầy đủ nhất cho request/response của từng endpoint.

## Cấu trúc dự án

```text
SystemQMS/
├── Controllers/     # HTTP API controllers
├── Hubs/            # SignalR hubs
├── Migrations/      # EF Core database migrations
├── Models/          # Entity và DbContext
├── Services/        # Business services và interfaces
├── ViewModels/      # DTO cho request/response
├── wwwroot/         # File âm thanh và tài nguyên tĩnh
├── Program.cs       # Dependency injection và HTTP pipeline
└── appsettings*.json
```

## Migration

Tạo migration mới:

```bash
dotnet ef migrations add MigrationName --project SystemQMS
```

Áp dụng migration:

```bash
dotnet ef database update --project SystemQMS
```

## Ghi chú phát triển

- Swagger chỉ được bật khi `ASPNETCORE_ENVIRONMENT=Development`.
- Một số API cấu hình hiện lấy `unitId` mặc định là `1`; khi triển khai đa đơn vị cần thay thế bằng cơ chế xác định đơn vị từ request hoặc user context.
- `QmsHub` đã được đăng ký trong dependency injection để phục vụ thông báo thời gian thực; cần map hub vào pipeline khi triển khai client SignalR.
- API hiện chưa cấu hình middleware xác thực/ủy quyền đầy đủ; cần bổ sung trước khi đưa lên môi trường production.

## Đóng góp

1. Tạo branch mới từ `main`.
2. Thực hiện thay đổi và bổ sung migration nếu schema thay đổi.
3. Chạy `dotnet build` và kiểm tra Swagger trước khi tạo pull request.
4. Mô tả rõ thay đổi API hoặc database trong pull request.

## License

Repository hiện chưa khai báo license. Vui lòng liên hệ chủ repository trước khi sử dụng hoặc phân phối lại cho mục đích khác.
