---
title: Triển khai — Lưu ý & vấn đề đã biết
status: active
---

# Notice — vấn đề khi triển khai

Tài liệu ghi các **rủi ro, hạn chế và điểm cần lưu ý** khi triển khai backend/frontend và tích hợp, để tránh bỏ sót khi làm sprint tiếp theo.

## Kiến trúc & naming

- **Từ khóa C# `class`**: aggregate lớp học được đặt tên **`TrainingClass`** trong code (tránh keyword `class`). API dùng resource **`training-classes`**.
- **Quyền truy cập mới**: mỗi miền năng lực mới (ví dụ lịch lớp) cần thêm `Permissions.*`, cập nhật `PermissionMap` (Admin / OrgAdmin / Instructor / Learner) và seed nếu có — tránh chỉ gắn attribute trên API mà role không có quyền.

## Dữ liệu & EF Core

- **Migration**: thêm bảng `classes` / `sessions` cần chỉnh tên bảng tránh nhầm với khái niệm SQL/`CLASS`. Kiểm tra **global query filter** (soft delete) thống nhất với `Course` và các aggregate khác.
- **Khóa ngoại**: lớp học gắn `CourseId` — cần quyết định chỉ cho phép course **Published** hay vẫn cho **Draft** (ảnh hưởng UX và validation).
- **Phát hiện trùng lịch giảng viên**: cần truy vấn **xuyên aggregate** (mọi lớp, mọi session) theo `UserId` + khoảng thời gian; dễ sai nếu chỉ kiểm tra trong một aggregate hoặc thiếu index `(instructor_id, start_utc, end_utc)`.

## Tích hợp Zoom

- Hiện tại: **`NoOpZoomMeetingService`** tạo meeting id/URL giả khi session `Zoom`; đủ cho dev/API contract.
- Production: cần **Zoom OAuth app**, credential bảo mật (Key Vault / secrets), thay `IZoomMeetingService` bằng client gọi Zoom REST.
- **Webhook** (kết thúc meeting, attendance ở sprint sau) yêu cầu URL công khai, xác thực signature, idempotency — phức tạp hơn chỉ “tạo meeting”.

## Hạ tầng & file

- Upload nội dung khóa học hiện dùng **local storage** (`IFileStorage`); chuyển S3/Azure Blob là thay provider + cấu hình, cần migration đường dẫn/URL đã lưu nếu đã có dữ liệu production.

## Frontend (Angular)

- Sprint plan: nhiều màn hình (auth, org, course, lịch…) **chưa gắn với tiến độ backend** — cần theo dõi riêng trong `frontend/README.md`, tránh coi “backend xong” là “sprint user-facing xong”.

## QA & vận hành

- **Integration/E2E tests** cho identity/organizations và flow mới vẫn là khoản nợ kỹ thuật; regression dễ lệch nếu chỉ dựa unit test.
- **Rate limiting, lockout, audit log** auth (ghi trong sprint plan) ảnh hưởng bảo mật production — nên ưu tiên trước khi mở rộng tính năng.

## Đồng bộ tài liệu

- Khi đóng sprint hoặc đổi scope, cập nhật `docs/sprint-plan.md` và các guide kỹ thuật liên quan để snapshot “đã làm / còn lại” không lệch với code.

---

*Cập nhật lần cuối: khi gặp vấn đề mới trong triển khai, bổ sung mục có ngày và ngữ cảnh ngắn.*
