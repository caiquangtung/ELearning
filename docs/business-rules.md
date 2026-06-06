---
title: Business Rules
scope: ELearning LMS
status: living-document
updated: 2026-05-31
---

# Business Rules

Tài liệu này là source of truth cho rule nghiệp vụ của hệ thống LMS. Mục tiêu là giúp BA, developer, tester và reviewer hiểu rõ điều kiện hợp lệ, quyền truy cập, trạng thái và ràng buộc theo từng module.

## 1. Role, Permission, Portal

### Platform roles

| Role                  | Ý nghĩa                               | Portal mặc định                                            |
| --------------------- | ------------------------------------- | ---------------------------------------------------------- |
| `Admin`               | Quản trị toàn hệ thống                | `/admin`                                                   |
| `Instructor`          | Giảng viên, quản lý nội dung/lớp/quiz | `/teach`                                                   |
| `Learner` / `Student` | Người học                             | `/learn`                                                   |
| `OrgAdmin`            | Quản trị tổ chức B2B                  | Chưa tách portal riêng, dùng quyền backend theo permission |

### Portal rules

- Learner Portal chỉ hiển thị nghiệp vụ học: courses, classes, orders, notifications, AI learning path.
- Teacher Portal chỉ hiển thị nghiệp vụ dạy: classes, courses, quizzes, AI quiz generation, AI grading, notifications.
- Admin Portal chỉ hiển thị nghiệp vụ quản trị: organizations, campaigns, license pools, reports, announcements, notifications.
- Ẩn menu không đủ; route phải có guard đúng role.
- Admin được phép vào Admin Portal và Teacher Portal.
- Instructor không được vào Admin Portal.
- Learner không được vào Teacher/Admin Portal.
- Legacy routes như `/courses`, `/training-classes`, `/orders`, `/dashboard` phải redirect theo role để không làm vỡ link cũ.

### Permission rules

- API protected bằng permission, không chỉ kiểm tra role trực tiếp.
- Permission được resolve từ role tại request time, không embed trong JWT.
- Admin có toàn bộ permissions.
- Instructor có quyền đọc/tạo/cập nhật/publish course, quản lý class/session, quiz/AI liên quan giảng dạy.
- Learner có quyền đọc course/class, tạo enrollment/order/payment, xem report cá nhân và notifications.

## 2. Identity & User

- Email đăng ký phải hợp lệ và không trùng.
- Password phải đạt policy validation hiện có.
- User luôn phải có ít nhất một platform role.
- Không được remove role cuối cùng của user.
- Chỉ Admin được assign platform roles.
- Profile update yêu cầu `firstName`, `lastName` không rỗng và giới hạn 100 ký tự.
- Org-scoped roles là membership trong organization, tách biệt với platform roles.

## 3. Organization

- Organization name là bắt buộc, tối đa 200 ký tự.
- Slug nếu có thì tối đa 200 ký tự.
- Một user không được là member trùng trong cùng organization.
- Department phải thuộc đúng organization khi add member.
- Parent department phải thuộc cùng organization.
- Organization role phải thuộc danh sách hợp lệ trong `OrganizationRoles`.
- User ngoài organization không được xem chi tiết organization nếu không có quyền phù hợp.

## 4. Course & Content

### Course

- Course title là bắt buộc.
- Description tối đa 4000 ký tự ở application validation.
- Course khởi tạo ở trạng thái `Draft`.
- Course chỉ được publish khi có ít nhất một lesson.
- Course price không được âm.
- Currency là bắt buộc và được chuẩn hóa uppercase.

### Section & Lesson

- Section title là bắt buộc.
- Lesson title là bắt buộc.
- Sort order tăng theo thứ tự thêm mới trong course.
- Lesson asset phải có file name, content type, storage key, URL và size > 0.

### Video

- Upload video yêu cầu `courseId`, `sectionId`, `lessonId`.
- File name tối đa 512 ký tự.
- Content type phải bắt đầu bằng `video/`.
- Duration nếu có phải > 0.
- Watch progress yêu cầu duration > 0, position >= 0, watchedSeconds >= 0.

## 5. Training Class & Sessions

- Training class phải gắn với một course hợp lệ.
- Class title là bắt buộc.
- `maxLearners` phải > 0.
- Instructor chỉ được assign nếu user có role `Instructor` hoặc `Admin`.
- Session offline phải có location.
- Session online/live cần có thông tin meeting nếu workflow yêu cầu.
- Session overlap của instructor phải được chặn với các session chưa cancel.
- Cancel session không được tính là conflict lịch.
- Capacity class phải được kiểm tra khi checkout class.

## 6. Commerce, Checkout, Orders

### Order

- Order phải có `buyerUserId`.
- Currency là bắt buộc.
- Order item phải có:
  - `referenceId`
  - `itemType` hợp lệ
  - `quantity > 0`
  - `unitPriceCents >= 0`
- Chỉ được add item khi order còn `Draft`.
- Order chỉ được submit checkout khi:
  - trạng thái là `Draft`
  - có ít nhất một item
  - total > 0
  - checkout timeout > 0
- Chỉ order `PendingPayment` mới được mark paid.
- Paid order không được cancel.
- Cancel order phải có reason.
- Discount không được âm và không được vượt subtotal.

### Supported order item types

- `Course`
- `TrainingClass`
- `LicensePool`

### Price validation

- Course phải tồn tại và có price > 0 để checkout.
- Training class phải tồn tại và có price > 0 để checkout.
- License pool phải tồn tại và có seat price > 0 để checkout.
- Training class checkout phải kiểm tra capacity để tránh overbooking.

## 7. Campaign, Coupon, Promotion

- Campaign name là bắt buộc, tối đa 200 ký tự.
- Campaign scope phải hợp lệ.
- Campaign start time là bắt buộc.
- Coupon code là bắt buộc, tối đa 64 ký tự.
- `perBuyerMaxRedemptions` phải > 0.
- Promotion percent off phải nằm trong 1-100.
- Coupon chỉ hợp lệ khi:
  - tồn tại
  - chưa expired/disabled
  - buyer chưa vượt redemption limit
  - campaign của coupon tồn tại
  - campaign eligible với order
- Stacking rule MVP: lấy discount phần trăm cao nhất theo item trong các campaign/coupon eligible.
- Quote/preview không được mutate order/payment state.

## 8. License Pool

- License pool phải thuộc organization.
- Name là bắt buộc, tối đa 200 ký tự.
- Total seats phải > 0 và <= 100000.
- Optional expiry: `null` nghĩa là không hết hạn.
- Assign license không được vượt quá available seats.
- Revoked assignment không được tính là active seat.
- Expired pool không nên được assign license mới.

## 9. Quiz & Assessment

### Quiz

- Quiz title là bắt buộc.
- Time limit nếu có phải >= 1 phút.
- Passing score không được âm.
- Quiz chỉ publish khi có ít nhất một question.
- Tất cả multiple-choice questions phải có ít nhất một option trước khi publish.
- Chỉ published quiz mới được start attempt.

### Question

- Question text là bắt buộc, tối đa 2000 ký tự.
- Question type hợp lệ: `MultipleChoice`, `Essay`, `Code`.
- Points không được âm.
- Option text là bắt buộc, tối đa 1000 ký tự.

### Attempt & grading

- Attempt phải submitted trước khi grade.
- AI grading suggestion chỉ áp dụng cho submitted attempt.
- AI grading suggestion chỉ có ý nghĩa khi attempt có essay/code answer.

## 10. Reviews

- Review phải gắn với course tồn tại.
- Rating phải nằm trong range hợp lệ của aggregate.
- Learner phải đủ điều kiện review theo completion/certificate rule.
- Review moderation chỉ chấp nhận status hợp lệ.
- Reject review cần moderation reason rõ ràng.
- Rating summary chỉ tính review published.

## 11. Certificates

- Certificate chỉ issue khi learner đạt completion rules.
- Completion MVP gồm:
  - attendance >= 80%
  - learning progress = 100%
  - quiz passed
- Certificate có verification code public để xác thực.
- Revoked certificate không được xem là verifiable active certificate.
- Certificate template name là bắt buộc.
- Certificate PDF/download là output từ certificate đã issue.

## 12. Notifications & Announcements

- Notification phải có userId, title, body.
- Title tối đa 200 ký tự.
- Body tối đa 4000 ký tự.
- Action URL tối đa 1000 ký tự.
- Message/announcement phải có:
  - sender
  - subject
  - body
  - ít nhất một recipient
- Announcement send yêu cầu permission gửi notification.
- Unread count phải trả về 0 thay vì phá UI nếu lỗi backend.

## 13. AI Features

### Shared AI rules

- AI feature phải ghi audit log với feature, provider/model, promptVersion/inputHash khi có request.
- Redis cache dùng cho các AI result có input ổn định.
- Local deterministic provider là default để demo không phụ thuộc external API key.
- AI output là recommendation/draft/suggestion, không tự động mutate enrollment/order/certificate.

### Semantic Search

- Query là bắt buộc, tối đa 500 ký tự.
- Limit nằm trong 1-20.
- Chỉ rank published courses.
- Response phải có score, matched concepts và reasons để giải thích.

### Learning Path

- Goal là bắt buộc, tối đa 500 ký tự.
- Current skills tối đa 500 ký tự.
- Target role tối đa 200 ký tự.
- Max courses nằm trong 1-12.
- Generated learning path là draft, không auto assign learner.
- Courses trong path không được trùng.

### RAG Learning Assistant

- Chat responses phải trả lại citations liên kết đến Course/Section/Lesson.
- Nếu không tìm thấy nội dung phù hợp, trả về: "I don't have enough course material to answer that."
- Knowledge reindex phải chạy lại khi nội dung khóa học thay đổi.
- RAG assistant chỉ là support, không tự động thay đổi trạng thái học viên, điểm số, hay đăng ký.

### Recommendations

- Limit recommendation phải trong range validator hiện có.
- Recommendation không nên trả course đã mua nếu service có đủ signal.
- Fallback popularity được phép dùng khi learner chưa có history.

### Risk & grading

- Learner risk là decision-support, không được tự động khóa học viên.
- Essay grading là suggestion; instructor/manual grading vẫn là authoritative.

## 14. Reporting

- Admin dashboard hiển thị metrics toàn hệ thống.
- Student dashboard chỉ hiển thị dữ liệu của learner hiện tại.
- Instructor dashboard chỉ hiển thị dữ liệu teaching scope của instructor.
- Organization analytics chỉ đọc được khi user có quyền report/org phù hợp.
- Report export là permission riêng, không mặc định cho mọi reader.

## 15. Cross-Cutting Rules

- Business invariants nằm trong Domain aggregate khi có thể.
- Input shape/range validation nằm ở Application validators.
- Authorization nằm ở API policy/permission layer và guard FE.
- FE không được coi là nguồn bảo mật; FE chỉ hỗ trợ UX.
- Các operation có side effect quan trọng cần audit log: auth, payment, role, review moderation, license, campaign.
- Cache phải có invalidation hoặc TTL hợp lý cho data có thể thay đổi.
- Public API chỉ được expose dữ liệu published/safe-to-display.

## 16. Planned / Not Fully Implemented

- Enrollment aggregate đầy đủ và attendance workflow chưa hoàn chỉnh.
- Waitlist, class transfer, enrollment expiry là planned.
- SSO, GDPR deletion/anonymization là planned.
- SCORM, DRM, subtitle/accessibility nâng cao là planned.
- Production payment gateway, refund, invoice tax rules là planned.
- Advanced promotion stacking/rule engine chưa thay thế MVP best-discount rule.
- Organization-specific portal chưa tách riêng khỏi Admin/OrgAdmin permission model.
- Vector database/LLM provider thật chưa bắt buộc; AI hiện dùng local deterministic provider.
