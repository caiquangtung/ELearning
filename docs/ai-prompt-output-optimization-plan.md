# AI Prompt and Output Optimization Plan

Tài liệu này đề xuất kế hoạch tối ưu prompt và output cho module AI của ELearning, dựa trên các tài liệu hiện có về kiến trúc AI/RAG, quality evaluation, và runbook vận hành.

## Mục tiêu

- Chuyển prompt từ dạng gõ tay theo từng lần sang template có version control.
- Đo chất lượng bằng bộ eval cố định thay vì đánh giá cảm tính.
- Chuẩn hóa luồng AI thành pipeline có thể tái sử dụng.
- Thêm validation để hạn chế hallucination và output sai schema.
- Lưu lại prompt version, model version, và kết quả eval để đảm bảo reproducibility.

## Bối cảnh hiện tại

- AI architecture đã có prompt version cho các feature chính và có logging metadata phục vụ audit.
- AI quality evaluation đã có golden dataset, rubric, và checklist cho RAG.
- AI/RAG runbook đã ghi nhận các cấu hình provider, fallback, và đánh giá RAG bằng dataset cố định.
- RAG assistant hiện đã có citations và fallback extractive khi provider lỗi hoặc output không hợp lệ.

Kết luận: nền tảng đã có các mảnh ghép quan trọng, nhưng chưa chuẩn hóa đủ mạnh ở lớp prompt lifecycle và regression workflow.

## Phạm vi áp dụng

Ưu tiên 4 luồng đang có nhiều giá trị nhất:

- Quiz question generation.
- Essay grading suggestions.
- Learning path generation.
- RAG learning assistant.

Nếu cần mở rộng, cùng một pattern có thể áp dụng cho semantic search summary hoặc các assistant workflow khác.

## Kế hoạch triển khai

### 1. Prompt Engineering có cấu trúc

Mục tiêu: biến prompt thành artifact được quản lý như code.

Việc cần làm:

- Tách prompt thành 4 phần rõ ràng:
  - system prompt: vai trò, ràng buộc, nguyên tắc an toàn.
  - context: dữ liệu đầu vào như course info, retrieved chunks, rubric, user intent.
  - instruction: nhiệm vụ cụ thể cần thực hiện.
  - output format: schema/JSON shape cần trả về.
- Lưu prompt template trong repo dưới dạng `.md`, `.yaml`, hoặc `.json` thay vì hard-code trực tiếp trong service.
- Gắn version cho từng template, ví dụ `rag-learning-assistant-v1`, `quiz-question-generator-v1`.
- Áp dụng same-template pattern cho 3 đến 5 prompt thường dùng nhất trước.

Đề xuất cấu trúc thư mục:

- `src/ELearning.Infrastructure/Ai/Prompts/quiz-question-generator-v1.yaml`
- `src/ELearning.Infrastructure/Ai/Prompts/essay-grading-v1.yaml`
- `src/ELearning.Infrastructure/Ai/Prompts/learning-path-generator-v1.yaml`
- `src/ELearning.Infrastructure/Ai/Prompts/rag-learning-assistant-v1.yaml`

Kết quả mong đợi:

- Prompt có history rõ ràng trong git.
- Dễ review diff khi prompt thay đổi.
- Giảm nguy cơ sửa prompt ngẫu hứng trong code.

### 2. Evaluation thay cho cảm giác ổn

Mục tiêu: có bộ test cố định để phát hiện regression ngay khi prompt hoặc model đổi.

Việc cần làm:

- Tạo golden dataset gồm 15 đến 20 câu hỏi mẫu cho chatbot E-Learning.
- Ghi rõ expected outcome, citation expectation, refusal expectation, và tiêu chí pass/fail.
- Chạy eval lại mỗi khi thay prompt version, model version, retrieval threshold, hoặc output schema.
- Dùng `Promptfoo` hoặc một script kiểm thử đơn giản bằng Python/C# để so sánh output giữa các lần chạy.

Suggested test mix:

- 5 câu hỏi fact-based từ lesson cụ thể.
- 4 câu hỏi course overview hoặc concept explanation.
- 3 câu hỏi out-of-scope để kiểm refusal.
- 2 câu hỏi access-boundary để kiểm quyền truy cập.
- 2 câu hỏi provider failure hoặc malformed output để kiểm fallback.

Kết quả mong đợi:

- Có baseline đo được trước/sau mỗi lần thay đổi.
- Biết ngay thay đổi nào làm giảm groundedness, citation quality, hoặc refusal accuracy.

### 3. Workflow hóa chuỗi thao tác AI

Mục tiêu: biến retrieve → rerank → generate → validate thành pipeline có thể gọi lại, không phải chuỗi prompt rời rạc.

Việc cần làm:

- Chuẩn hóa các bước xử lý thành function rõ ràng.
- Tách phần retrieval, answer synthesis, và output validation thành các stage riêng.
- Nếu dùng Semantic Kernel, tận dụng Planner/Plugins để đóng gói logic thành reusable functions thay vì nhúng toàn bộ logic vào prompt tự do.
- Đảm bảo mỗi stage có input/output contract rõ ràng.

Kết quả mong đợi:

- Luồng AI dễ test từng bước.
- Giảm coupling giữa prompt, retrieval, và response formatting.
- Dễ thay đổi một stage mà không phá toàn pipeline.

### 4. Validation và Guardrails

Mục tiêu: không tin mù output AI, chỉ trả kết quả khi qua kiểm tra tối thiểu.

Việc cần làm:

- Validate output theo schema trước khi trả về client.
- Kiểm tra citations có tồn tại trong context đã retrieve.
- Kiểm tra `usedContext`, `confidence`, và các metadata liên quan có hợp lệ.
- Khi output không hợp lệ, dùng fallback an toàn hoặc refusal thay vì trả JSON lỗi.

Gợi ý guardrails tối thiểu:

- JSON/schema validation cho quiz, learning path, essay feedback.
- Citation existence check cho RAG.
- Confidence clamp trong khoảng `0..1`.
- Refuse khi không có context đủ tin cậy.

Kết quả mong đợi:

- Giảm hallucination.
- Giảm output schema drift.
- Tăng tính an toàn và khả năng debug.

### 5. Documentation và Reproducibility

Mục tiêu: 6 tháng sau vẫn có thể giải thích vì sao output đổi.

Việc cần làm:

- Lưu prompt version, model version, retrieval settings, và eval result cho mỗi run quan trọng.
- Ghi lại thay đổi về prompt kèm lý do thay đổi và expected impact.
- Bổ sung runbook ngắn cho việc chạy lại eval khi đổi prompt/model.
- Đảm bảo log đủ để truy ngược lại một câu trả lời đã được sinh ra bằng cấu hình nào.

Kết quả mong đợi:

- Có traceability từ output ngược về prompt version.
- Dễ so sánh kết quả giữa các version.
- Hỗ trợ review và rollback khi regression xảy ra.

## Lộ trình chốt cho repo hiện tại

### Phase 1: Prompt infrastructure

Mục tiêu: biến prompt thành artifact có thể review, version, và thay thế có kiểm soát.

Deliverables:

- 3 đến 5 prompt template đầu tiên được tách ra khỏi code và lưu trong repo.
- Mỗi template có version rõ ràng, ví dụ `rag-learning-assistant-v1`.
- Template có cấu trúc tách bạch `system`, `context`, `instruction`, `output format`.
- Có cơ chế override runtime tùy chọn cho môi trường thử nghiệm, nhưng repo file vẫn là baseline chuẩn.

Acceptance criteria:

- Không còn prompt dài hard-code trong service cho các luồng ưu tiên.
- Có thể review thay đổi prompt bằng git diff.
- Override runtime nếu có phải có audit log và khả năng rollback.

### Phase 2: Automated evaluation

Mục tiêu: có baseline đo được để biết prompt/model change là tốt hay xấu.

Deliverables:

- Bộ 15 đến 20 test case cho RAG chatbot E-Learning.
- Script eval tự động chấm các metric chính: groundedness, citation validity, refusal accuracy.
- Kết quả baseline được lưu trong repo hoặc artifact có version.
- Nếu dùng judge model, chỉ dùng như lớp bổ trợ cho case mơ hồ, không thay thế metric khách quan.

Acceptance criteria:

- Chạy lại cùng baseline phải cho kết quả ổn định.
- Khi đổi prompt, regression được phát hiện ngay.
- CI fail nếu các metric chính giảm vượt ngưỡng đã định.

### Phase 3: Robust pipeline & guardrails

Mục tiêu: bảo vệ output trước khi trả về người dùng và giữ latency trong ngưỡng kiểm soát.

Deliverables:

- Query rewriting nhẹ trước retrieve, chỉ áp dụng khi query quá ngắn hoặc mơ hồ.
- Validation theo tầng: schema check bắt buộc, citation existence check bắt buộc, LLM check chỉ dùng cho case xám.
- Fallback extractive rõ ràng khi output không đạt chuẩn hoặc context quá yếu.

Acceptance criteria:

- Output sai schema không lọt ra client.
- Citation giả hoặc citation không tồn tại bị chặn.
- Không dùng LLM validation mặc định cho mọi request.

### Phase 4: Observability & feedback loop

Mục tiêu: theo dõi được chi phí, latency, và thu tín hiệu thực tế để cải tiến dataset.

Deliverables:

- Log đầy đủ: prompt_version, model, latency_ms, token_usage, retrieval stats.
- Endpoint feedback để frontend gửi rating hoặc thumbs up/down.
- Cơ chế định kỳ lấy các case chất lượng thấp để đưa vào candidate set cho golden dataset.
- Runbook ngắn cho việc cập nhật baseline sau khi có dữ liệu thực tế.

Acceptance criteria:

- Mỗi lần thay đổi prompt/model đều truy ngược được kết quả và cấu hình.
- Có dữ liệu thực tế để bổ sung vào dataset thay vì chỉ dựa vào test case ban đầu.

## Quy tắc chốt phạm vi

- Ưu tiên RAG chatbot trước, sau đó mới nhân rộng sang quiz, essay grading và learning path.
- Không bật hot-reload hoặc query rewriting rộng rãi nếu chưa có baseline và audit đầy đủ.
- Không dùng judge model như nguồn quyết định duy nhất cho pass/fail của CI.
- Mọi tối ưu mới phải chứng minh được tác động lên groundedness, refusal accuracy, latency hoặc cost.

## Áp dụng ngay cho chatbot RAG E-Learning

1. Tách prompt chat RAG thành template versioned.
2. Viết 15 đến 20 test case mẫu.
3. Chạy eval baseline và lưu kết quả.
4. Thêm validation theo tầng trước khi trả response.
5. Chỉ mở rộng query rewriting hoặc runtime override khi baseline đã ổn định.

## Tiêu chí thành công

- Prompt được quản lý như artifact có version.
- Mỗi thay đổi prompt đều có eval đi kèm.
- Output AI được validate trước khi trả về.
- Có log và tài liệu đủ để tái tạo kết quả.
- RAG trả lời grounded hơn, ít hallucination hơn, và có khả năng rollback khi regression.

## Ghi chú thực thi

- Bắt đầu từ RAG chatbot vì đây là luồng có citations, golden dataset, và giá trị kiểm chứng rõ nhất.
- Sau khi RAG ổn định, áp dụng cùng pattern cho quiz, essay grading, và learning path generation.
- Không tối ưu prompt trước khi đã có baseline eval, vì như vậy sẽ khó biết thay đổi nào tạo ra cải thiện thực sự.
